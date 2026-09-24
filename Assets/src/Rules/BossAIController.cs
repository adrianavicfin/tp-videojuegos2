using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Controlador de Inteligencia Artificial para el Boss (H3-T8).
    /// Ejecuta la toma de decisiones por turno:
    /// 1. Búsqueda y selección táctica del objetivo (héroe vivo más cercano o con menor salud según la fase).
    /// 2. Cálculo balístico / estimación de ángulo y potencia considerando la gravedad de los planetoides.
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

        [Tooltip("Margen de dispersión o error intencional en grados según la fase.")]
        [SerializeField] private float _angleInaccuracy = 4f;

        [Tooltip("Fuerza del arco parabólico adicional para salvar curvaturas de planetoides.")]
        [SerializeField] private float _loftFactor = 0.35f;

        private Boss _boss;

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

            // 4. Calcular vector de lanzamiento, ángulo y potencia
            Vector2 launchDirection;
            float launchPower;
            CalculateBallisticParameters(targetHero.transform.position, out launchDirection, out launchPower);

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
            List<Hero> aliveHeroes = new List<Hero>();

            for (int i = 0; i < heroes.Length; i++)
            {
                if (heroes[i] != null && !heroes[i].IsDead && heroes[i].gameObject.activeSelf)
                {
                    aliveHeroes.Add(heroes[i]);
                }
            }

            if (aliveHeroes.Count == 0) return null;

            Vector2 bossPos = transform.position;

            if (_boss.CurrentPhase >= 2)
            {
                // Priorizar héroe con menor salud
                Hero lowestHPHero = aliveHeroes[0];
                for (int i = 1; i < aliveHeroes.Count; i++)
                {
                    if (aliveHeroes[i].CurrentHealth < lowestHPHero.CurrentHealth)
                    {
                        lowestHPHero = aliveHeroes[i];
                    }
                }
                return lowestHPHero;
            }
            else
            {
                // Fase 1: Priorizar héroe más cercano
                Hero closestHero = aliveHeroes[0];
                float minDistanceSqr = Vector2.SqrMagnitude((Vector2)closestHero.transform.position - bossPos);

                for (int i = 1; i < aliveHeroes.Count; i++)
                {
                    float distSqr = Vector2.SqrMagnitude((Vector2)aliveHeroes[i].transform.position - bossPos);
                    if (distSqr < minDistanceSqr)
                    {
                        minDistanceSqr = distSqr;
                        closestHero = aliveHeroes[i];
                    }
                }
                return closestHero;
            }
        }

        /// <summary>
        /// Calcula la dirección y potencia del disparo hacia el objetivo considerando la normal de la superficie y la gravedad.
        /// </summary>
        public void CalculateBallisticParameters(Vector2 targetPos, out Vector2 launchDirection, out float launchPower)
        {
            Vector2 bossPos = transform.position;
            Vector2 toTarget = targetPos - bossPos;
            float distance = toTarget.magnitude;

            // Dirección base hacia el objetivo
            Vector2 directDir = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : Vector2.right;

            // Arco ascendente según la normal del Boss (transform.up) para contrarrestar la atracción radial
            Vector2 upDir = (Vector2)transform.up;
            Vector2 loftedDir = (directDir + upDir * _loftFactor).normalized;

            // Aplicar dispersión aleatoria controlada
            float phaseAccuracyModifier = Mathf.Max(0.5f, 1f - (_boss.CurrentPhase - 1) * 0.25f);
            float randomAngleOffset = Random.Range(-_angleInaccuracy, _angleInaccuracy) * phaseAccuracyModifier;
            Quaternion spreadRotation = Quaternion.Euler(0, 0, randomAngleOffset);
            launchDirection = spreadRotation * loftedDir;

            // Calcular potencia basada en distancia y límites del arma
            float maxAllowedPower = _bossWeapon != null ? _bossWeapon.MaxPower : _defaultPower;
            float estimatedPower = Mathf.Clamp(distance * 1.5f + 4f, 8f, maxAllowedPower);
            launchPower = estimatedPower;
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
