using System.Collections;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Controlador de Inteligencia Artificial para el Boss (H3-T8).
    /// Ejecuta la toma de decisiones por turno:
    /// 1. Búsqueda y selección táctica del objetivo (héroe vivo más cercano o con menor salud según la fase).
    /// 2. Búsqueda de ángulo y potencia simulando la gravedad y las colisiones del proyectil.
    /// 3. Visualización previa o animación de apuntado y ejecución del disparo (ActionShoot).
    /// 4. Notificación al TurnManager del ciclo de resolución de la acción.
    /// </summary>
    [RequireComponent(typeof(Boss))]
    public class BossAIController : MonoBehaviour
    {
        [Header("Weapon & Projectile")]
        [SerializeField] private WeaponDataSO _bossWeapon;
        [SerializeField] private float _defaultPower = 18f;
        [SerializeField] private int _defaultDamage = 45;
        [SerializeField] private float _explosionRadius = 3.5f;
        [SerializeField] private float _knockbackForce = 25f;

        [Header("AI Behavior Settings")]
        [Tooltip("Tiempo de anticipación / apuntado antes de disparar para dar feedback al jugador.")]
        [SerializeField] private float _aimDelaySeconds = 1.2f;

        [Header("Ballistic Search")]
        [SerializeField, Range(1f, 15f)] private float _angleStepDegrees = 5f;
        [SerializeField, Range(2, 10)] private int _powerSamples = 6;
        [SerializeField, Range(1, 50)] private int _candidatesPerFrame = 12;

        [Header("Tactical Movement")]
        [SerializeField, Range(0f, 3f)] private float _walkDuration = 0.8f;
        [SerializeField, Range(1, 6)] private int _jumpEveryTurns = 3;
        [SerializeField, Range(1f, 8f)] private float _landingTimeout = 4f;

        private Boss _boss;
        private CharacterMovementController _movement;
        private int _turnCount;
        private readonly RaycastHit2D[] _collisionHits = new RaycastHit2D[16];

        public WeaponDataSO BossWeapon => _bossWeapon;

        private void Awake()
        {
            _boss = GetComponent<Boss>();
            _movement = GetComponent<CharacterMovementController>();

            // Si no se asignó arma en el Inspector, cargar HeavyMissile por defecto
            if (_bossWeapon == null)
            {
                _bossWeapon = Resources.Load<WeaponDataSO>("ScriptableObjects/Weapon_HeavyMissile");
            }
        }

        /// <summary>
        /// Inicia la rutina de ataque de IA del Boss.
        /// </summary>
        public void PerformTurnAction()
        {
            StartCoroutine(ExecuteAITurnRoutine());
        }

        private IEnumerator ExecuteAITurnRoutine()
        {
            _turnCount++;
            Debug.Log($"[BossAIController] {_boss.CharacterName} evaluando campo de batalla en Fase {_boss.CurrentPhase}...");

            // 1. Notificar al TurnManager que una acción está en preparación/ejecución
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.NotifyActionExecuting();
            }

            // 2. Buscar héroe objetivo vivo
            Hero targetHero = SelectBestTarget();

            if (targetHero == null)
            {
                Debug.LogWarning("[BossAIController] No se encontraron héroes vivos en el campo de batalla.");
                yield return new WaitForSeconds(0.5f);
                TurnManager.Instance?.NotifyActionResolved();
                yield break;
            }

            Debug.Log($"[BossAIController] Objetivo fijado: {targetHero.CharacterName} (Salud: {targetHero.CurrentHealth}/{targetHero.MaxHealth})");

            // 3. Alternar entre acercamiento, búsqueda de ocultamiento y salto viable.
            if (_movement == null) _movement = GetComponent<CharacterMovementController>();
            if (_movement != null && _movement.IsGrounded && _turnCount % _jumpEveryTurns == 0 &&
                TryPlanInterplanetaryJump(targetHero, out Vector2 jumpDirection, out float jumpSpeed,
                    out GravityBody destination))
            {
                Debug.Log($"[BossAIController] Salto táctico hacia {destination.name}: dirección {jumpDirection}, velocidad {jumpSpeed:F1}.");
                new ActionJump(jumpDirection, jumpSpeed).Execute(_boss);

                float deadline = Time.time + _landingTimeout;
                yield return new WaitForSeconds(0.4f);
                while (Time.time < deadline && !_boss.IsDead &&
                    (!IsNearPlanet(_boss.transform.position, destination, 2.5f) || !_movement.IsGrounded))
                {
                    yield return null;
                }

                if (_boss.IsDead) yield break;
                if (!_movement.IsGrounded || !IsNearPlanet(_boss.transform.position, destination, 2.5f))
                {
                    Debug.LogWarning("[BossAIController] El salto no terminó en el planeta previsto; se cede el turno sin disparar.");
                    TurnManager.Instance?.NotifyActionResolved();
                    yield break;
                }
            }
            else if (_movement != null && _movement.IsGrounded && _walkDuration > 0f && _turnCount % 3 != 0)
            {
                Vector2 toHero = targetHero.transform.position - transform.position;
                float towardHero = Mathf.Sign(Vector2.Dot(_movement.SurfaceTangent, toHero));
                bool seekOcclusion = _boss.CurrentPhase >= 2 && HasClearLineOfSight(targetHero);
                float walkInput = seekOcclusion ? -towardHero : towardHero;
                _movement.SetAutonomousHorizontalInput(walkInput);
                Debug.Log(seekOcclusion
                    ? "[BossAIController] Buscando ocultamiento detrás del planeta."
                    : "[BossAIController] Reposicionándose sobre la superficie.");
                yield return new WaitForSeconds(_walkDuration);
                _movement.SetAutonomousHorizontalInput(0f);
                if (_boss.IsDead) yield break;
            }

            // 4. Pausa de anticipación visual (simula tiempo de cálculo y mira del Boss)
            yield return new WaitForSeconds(_aimDelaySeconds);

            // El objetivo puede haber muerto durante la pausa de anticipación.
            if (targetHero == null || targetHero.IsDead)
            {
                targetHero = SelectBestTarget();
                if (targetHero == null)
                {
                    TurnManager.Instance?.NotifyActionResolved();
                    yield break;
                }
            }

            // 4. Buscar un tiro que realmente alcance al objetivo (o explote a su lado).
            Vector2 launchDirection;
            float launchPower;
            float bestScore = float.PositiveInfinity;
            launchDirection = ((Vector2)(targetHero.transform.position - transform.position)).normalized;
            float phasePowerMultiplier = _boss.AttackPowerMultiplier;
            launchPower = Mathf.Clamp((_bossWeapon != null ? _bossWeapon.MaxPower : _defaultPower)
                * phasePowerMultiplier, 1f, 100f);

            GameObject projectilePrefab = _bossWeapon != null ? _bossWeapon.ProjectilePrefab : null;
            Rigidbody2D projectileBody = projectilePrefab != null ? projectilePrefab.GetComponent<Rigidbody2D>() : null;
            Projectile projectile = projectilePrefab != null ? projectilePrefab.GetComponent<Projectile>() : null;
            CircleCollider2D projectileCollider = projectilePrefab != null ? projectilePrefab.GetComponent<CircleCollider2D>() : null;
            Collider2D targetCollider = targetHero.GetComponent<Collider2D>();

            if (projectileBody != null && projectile != null && projectileCollider != null && targetCollider != null)
            {
                float mass = Mathf.Max(projectileBody.mass, 0.01f);
                float radius = projectileCollider.radius * Mathf.Max(
                    Mathf.Abs(projectilePrefab.transform.localScale.x), Mathf.Abs(projectilePrefab.transform.localScale.y));
                float explosionRadius = _bossWeapon.ExplosionRadius * _boss.AttackRadiusMultiplier;
                float maxPower = launchPower;
                float minPower = _boss.CurrentPhase >= 2
                    ? Mathf.Min(maxPower, _bossWeapon.MaxPower * 1.05f)
                    : maxPower * 0.5f;
                float directAngle = Mathf.Atan2(launchDirection.y, launchDirection.x) * Mathf.Rad2Deg;
                int angleSamples = Mathf.CeilToInt(180f / _angleStepDegrees);
                int evaluated = 0;

                for (int angleIndex = 0; angleIndex <= angleSamples; angleIndex++)
                {
                    float angle = directAngle - 90f + angleIndex * (180f / angleSamples);
                    Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                    for (int powerIndex = 0; powerIndex < _powerSamples; powerIndex++)
                    {
                        float power = Mathf.Lerp(minPower, maxPower, (float)powerIndex / (_powerSamples - 1));
                        float score = ScoreShot(direction, power, mass, radius, projectile.GravityResponse,
                            projectile.MaxLifetime, explosionRadius, targetCollider);

                        if (score < bestScore)
                        {
                            bestScore = score;
                            launchDirection = direction;
                            launchPower = power;
                        }

                        if (++evaluated % _candidatesPerFrame == 0)
                        {
                            yield return null;
                        }
                    }
                }

                // Afinar alrededor del mejor tiro grueso para no depender de saltos de 5 grados.
                if (bestScore > -100f)
                {
                    float centerAngle = Mathf.Atan2(launchDirection.y, launchDirection.x) * Mathf.Rad2Deg;
                    float centerPower = launchPower;
                    float fineAngleStep = _angleStepDegrees / 5f;
                    float finePowerStep = (maxPower - minPower) / (_powerSamples - 1) / 4f;

                    for (int angleOffset = -5; angleOffset <= 5; angleOffset++)
                    {
                        float angle = (centerAngle + angleOffset * fineAngleStep) * Mathf.Deg2Rad;
                        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                        for (int powerOffset = -4; powerOffset <= 4; powerOffset++)
                        {
                            float power = Mathf.Clamp(centerPower + powerOffset * finePowerStep, minPower, maxPower);
                            float score = ScoreShot(direction, power, mass, radius, projectile.GravityResponse,
                                projectile.MaxLifetime, explosionRadius, targetCollider);

                            if (score < bestScore)
                            {
                                bestScore = score;
                                launchDirection = direction;
                                launchPower = power;
                            }

                            if (++evaluated % _candidatesPerFrame == 0)
                            {
                                yield return null;
                            }
                        }
                    }
                }

                Debug.Log($"[BossAIController] Tiro elegido tras {evaluated} simulaciones. " +
                    $"Puntaje: {bestScore:F2}, potencia: {launchPower:F1}.");
            }
            else
            {
                Debug.LogWarning("[BossAIController] Prefab o colliders incompletos; usando tiro directo como respaldo.");
            }

            if (targetHero == null || targetHero.IsDead)
            {
                TurnManager.Instance?.NotifyActionResolved();
                yield break;
            }

            // 5. Instanciar y disparar proyectil mediante Command ActionShoot
            ExecuteBossShoot(launchDirection, launchPower, targetHero);
        }

        /// <summary>
        /// Selecciona el objetivo óptimo según la fase actual del Boss.
        /// - Fase 1: Héroe más cercano.
        /// - Fase 2 o superior: Héroe con menor salud (enfoque de aniquilación/foco).
        /// </summary>
        public Hero SelectBestTarget()
        {
            Hero[] heroes = FindObjectsOfType<Hero>();
            Hero bestHero = null;
            Vector2 bossPos = transform.position;
            float bestDistance = float.PositiveInfinity;
            for (int i = 0; i < heroes.Length; i++)
            {
                Hero hero = heroes[i];
                if (hero == null || hero.IsDead || !hero.gameObject.activeSelf) continue;
                float distance = ((Vector2)hero.transform.position - bossPos).sqrMagnitude;
                if (bestHero == null || (_boss.CurrentPhase >= 2
                    ? hero.CurrentHealth < bestHero.CurrentHealth ||
                      (hero.CurrentHealth == bestHero.CurrentHealth && distance < bestDistance)
                    : distance < bestDistance))
                {
                    bestHero = hero;
                    bestDistance = distance;
                }
            }

            return bestHero;
        }

        private bool HasClearLineOfSight(Hero target)
        {
            Vector2 origin = target.transform.position;
            Vector2 towardBoss = (Vector2)transform.position - origin;
            float distance = towardBoss.magnitude;
            if (distance < 0.01f) return true;

            int hitCount = Physics2D.RaycastNonAlloc(origin, towardBoss / distance, _collisionHits,
                distance, 1 << LayerMask.NameToLayer("Ground"));
            for (int i = 0; i < hitCount; i++)
            {
                if (_collisionHits[i].collider != null && _collisionHits[i].distance > 1.5f &&
                    _collisionHits[i].distance < distance - 1.5f)
                    return false;
            }

            return true;
        }

        private bool TryPlanInterplanetaryJump(Hero target, out Vector2 direction,
            out float speed, out GravityBody destination)
        {
            direction = Vector2.zero;
            speed = 0f;
            destination = FindClosestPlanet(target.transform.position);
            GravityBody source = FindClosestPlanet(transform.position);
            if (source == null || destination == null || source == destination || _boss.Rigidbody == null)
                return false;

            float bossRadius = _boss.GetComponent<Collider2D>().bounds.extents.magnitude;
            if (!IsNearPlanet(transform.position, source, 3f)) return false;

            Vector2 surfaceNormal = ((Vector2)transform.position - source.Position).normalized;
            KillZoneView killZone = FindObjectOfType<KillZoneView>();
            Collider2D boundary = killZone != null ? killZone.GetComponent<Collider2D>() : null;
            Bounds mapBounds = boundary != null ? boundary.bounds : new Bounds(Vector3.zero, new Vector3(200f, 200f, 1f));
            float bestScore = float.PositiveInfinity;
            var activeBodies = GravityBody.AllBodies;
            GravityBody[] planets = new GravityBody[activeBodies.Count];
            float[] planetRadii = new float[activeBodies.Count];
            int planetCount = 0;
            int groundLayer = LayerMask.NameToLayer("Ground");
            for (int i = 0; i < activeBodies.Count; i++)
            {
                GravityBody planet = activeBodies[i];
                if (planet == null || planet.gameObject.layer != groundLayer) continue;
                planets[planetCount] = planet;
                planetRadii[planetCount] = GetPlanetRadius(planet);
                planetCount++;
            }

            for (int angle = -80; angle <= 80; angle += 10)
            {
                Vector2 candidateDirection = Quaternion.Euler(0f, 0f, angle) * surfaceNormal;
                for (float candidateSpeed = 32f; candidateSpeed <= 64f; candidateSpeed += 4f)
                {
                    if (!SimulateLanding(candidateDirection, candidateSpeed, source, destination,
                        bossRadius, mapBounds, planets, planetRadii, planetCount,
                        out Vector2 landingPoint)) continue;

                    float score = ((Vector2)target.transform.position - landingPoint).sqrMagnitude;
                    if (score < bestScore)
                    {
                        bestScore = score;
                        direction = candidateDirection;
                        speed = candidateSpeed;
                    }
                }
            }

            return bestScore < float.PositiveInfinity;
        }

        private bool SimulateLanding(Vector2 direction, float speed, GravityBody source,
            GravityBody destination, float bossRadius, Bounds mapBounds, GravityBody[] planets,
            float[] planetRadii, int planetCount, out Vector2 landingPoint)
        {
            landingPoint = Vector2.zero;
            Vector2 position = transform.position;
            Vector2 velocity = direction * speed;
            float step = Time.fixedDeltaTime;
            float mass = Mathf.Max(_boss.Rigidbody.mass, 0.01f);
            bool clearedSource = false;
            float initialSourceSurface = Vector2.Distance(position, source.Position) - GetPlanetRadius(source);
            for (float elapsed = 0f; elapsed < _landingTimeout; elapsed += step)
            {
                velocity += GravityBody.GetTotalGravitationalPull(position) *
                    (_boss.GravityMultiplier / mass) * step;
                position += velocity * step;

                if (position.x < mapBounds.min.x + bossRadius || position.x > mapBounds.max.x - bossRadius ||
                    position.y < mapBounds.min.y + bossRadius || position.y > mapBounds.max.y - bossRadius)
                    return false;

                for (int i = 0; i < planetCount; i++)
                {
                    GravityBody planet = planets[i];
                    if (planet == null) continue;
                    float surfaceDistance = Vector2.Distance(position, planet.Position) - planetRadii[i];

                    if (planet == source)
                    {
                        if (!clearedSource && surfaceDistance < initialSourceSurface - 0.3f) return false;
                        if (surfaceDistance > bossRadius + 0.5f) clearedSource = true;
                        else if (clearedSource && surfaceDistance <= bossRadius) return false;
                    }
                    else if (surfaceDistance <= bossRadius)
                    {
                        if (planet != destination || elapsed < 0.4f) return false;
                        landingPoint = position;
                        return true;
                    }
                }
            }

            return false;
        }

        private static GravityBody FindClosestPlanet(Vector2 point)
        {
            GravityBody closest = null;
            float nearestSurface = float.PositiveInfinity;
            var planets = GravityBody.AllBodies;
            for (int i = 0; i < planets.Count; i++)
            {
                GravityBody planet = planets[i];
                if (planet == null || planet.gameObject.layer != LayerMask.NameToLayer("Ground")) continue;
                float surfaceDistance = Mathf.Abs(Vector2.Distance(point, planet.Position) - GetPlanetRadius(planet));
                if (surfaceDistance < nearestSurface)
                {
                    nearestSurface = surfaceDistance;
                    closest = planet;
                }
            }
            return closest;
        }

        private static bool IsNearPlanet(Vector2 point, GravityBody planet, float tolerance)
        {
            return planet != null &&
                Mathf.Abs(Vector2.Distance(point, planet.Position) - GetPlanetRadius(planet)) <= tolerance;
        }

        private static float GetPlanetRadius(GravityBody planet)
        {
            CircleCollider2D collider = planet.GetComponent<CircleCollider2D>();
            return collider != null
                ? collider.radius * Mathf.Max(Mathf.Abs(planet.transform.lossyScale.x), Mathf.Abs(planet.transform.lossyScale.y))
                : 0f;
        }

        private float ScoreShot(Vector2 direction, float power, float mass, float radius,
            float gravityResponse, float maxLifetime, float explosionRadius, Collider2D target)
        {
            Vector2 position = (Vector2)transform.position + direction * 1.2f;
            Vector2 velocity = direction * (power / mass);
            float step = Time.fixedDeltaTime;
            int maxSteps = Mathf.CeilToInt(maxLifetime / step);

            for (int i = 0; i < maxSteps; i++)
            {
                velocity += GravityBody.GetTotalGravitationalPull(position) * (gravityResponse / mass) * step;
                Vector2 nextPosition = position + velocity * step;
                Vector2 movement = nextPosition - position;
                float distance = movement.magnitude;
                if (distance < 0.0001f) continue;
                int hitCount = Physics2D.CircleCastNonAlloc(position, radius, movement / distance,
                    _collisionHits, distance);
                RaycastHit2D nearest = default;
                float nearestDistance = float.PositiveInfinity;

                for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
                {
                    Collider2D collider = _collisionHits[hitIndex].collider;
                    if (collider == null || collider.isTrigger || collider.GetComponentInParent<Boss>() == _boss)
                        continue;
                    if (_collisionHits[hitIndex].distance < nearestDistance)
                    {
                        nearest = _collisionHits[hitIndex];
                        nearestDistance = nearest.distance;
                    }
                }

                if (nearest.collider != null)
                {
                    if (nearest.collider == target || nearest.collider.transform.IsChildOf(target.transform))
                        return -100f;

                    Vector2 impactPosition = position + movement.normalized * nearestDistance;
                    return Vector2.Distance(impactPosition, target.ClosestPoint(impactPosition)) - explosionRadius;
                }

                position = nextPosition;
            }

            // Sin impacto, el proyectil se extingue y no hace daño.
            return 1000f + Vector2.Distance(position, target.ClosestPoint(position));
        }

        private void ExecuteBossShoot(Vector2 direction, float power, Character target)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            int damage = Mathf.RoundToInt((_bossWeapon != null ? _bossWeapon.BaseDamage : _defaultDamage)
                * _boss.AttackDamageMultiplier);
            float radius = (_bossWeapon != null ? _bossWeapon.ExplosionRadius : _explosionRadius)
                * _boss.AttackRadiusMultiplier;
            float knockback = _bossWeapon != null ? _bossWeapon.KnockbackForce : _knockbackForce;
            GameObject prefab = _bossWeapon != null ? _bossWeapon.ProjectilePrefab : null;

            Debug.Log($"[BossAIController] {_boss.CharacterName} (Fase {_boss.CurrentPhase}) dispara contra {target?.CharacterName}! Ángulo: {angle:F1}°, Potencia: {power:F1}, Daño: {damage}, Radio: {radius:F1}, Prefab: {(prefab != null ? prefab.name : "null")}");

            ICharacterAction shootAction = new ActionShoot(angle, power, damage, prefab, radius, knockback);

            if (shootAction.CanExecute(_boss))
            {
                shootAction.Execute(_boss, target);
            }
            else
            {
                Debug.LogWarning("[BossAIController] No se pudo ejecutar ActionShoot para el Boss.");
                TurnManager.Instance?.NotifyActionResolved();
            }
        }
    }
}
