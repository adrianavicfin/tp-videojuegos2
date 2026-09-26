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

        private Boss _boss;
        private readonly RaycastHit2D[] _collisionHits = new RaycastHit2D[16];

        public WeaponDataSO BossWeapon => _bossWeapon;

        private void Awake()
        {
            _boss = GetComponent<Boss>();

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

            // 3. Pausa de anticipación visual (simula tiempo de cálculo y mira del Boss)
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
            launchPower = _bossWeapon != null ? _bossWeapon.MaxPower : _defaultPower;

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
                float explosionRadius = _bossWeapon.ExplosionRadius;
                float maxPower = Mathf.Max(_bossWeapon.MaxPower, 1f);
                float directAngle = Mathf.Atan2(launchDirection.y, launchDirection.x) * Mathf.Rad2Deg;
                int angleSamples = Mathf.CeilToInt(180f / _angleStepDegrees);
                int evaluated = 0;

                for (int angleIndex = 0; angleIndex <= angleSamples; angleIndex++)
                {
                    float angle = directAngle - 90f + angleIndex * (180f / angleSamples);
                    Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                    for (int powerIndex = 0; powerIndex < _powerSamples; powerIndex++)
                    {
                        float power = maxPower * Mathf.Lerp(0.5f, 1f, (float)powerIndex / (_powerSamples - 1));
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
                    float finePowerStep = maxPower * 0.5f / (_powerSamples - 1) / 4f;

                    for (int angleOffset = -5; angleOffset <= 5; angleOffset++)
                    {
                        float angle = (centerAngle + angleOffset * fineAngleStep) * Mathf.Deg2Rad;
                        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                        for (int powerOffset = -4; powerOffset <= 4; powerOffset++)
                        {
                            float power = Mathf.Clamp(centerPower + powerOffset * finePowerStep, 1f, maxPower);
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
            int damage = _bossWeapon != null ? _bossWeapon.BaseDamage : _defaultDamage;
            float radius = _bossWeapon != null ? _bossWeapon.ExplosionRadius : _explosionRadius;
            float knockback = _bossWeapon != null ? _bossWeapon.KnockbackForce : _knockbackForce;
            GameObject prefab = _bossWeapon != null ? _bossWeapon.ProjectilePrefab : null;

            Debug.Log($"[BossAIController] {_boss.CharacterName} dispara contra {target?.CharacterName}! Ángulo: {angle:F1}°, Potencia: {power:F1}, Daño: {damage}, Prefab: {(prefab != null ? prefab.name : "null")}");

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
