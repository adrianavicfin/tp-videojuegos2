using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Acción de disparo balístico de proyectil (Patrón Command).
    /// Instancia o lanza un proyectil físico en la escena que orbita bajo la gravedad radial.
    /// </summary>
    public class ActionShoot : ICharacterAction
    {
        public string ActionName => "Shoot";
        public float Angle { get; private set; }
        public float Power { get; private set; }
        public int Damage { get; private set; }
        public GameObject ProjectilePrefab { get; private set; }

        public ActionShoot(float angle, float power, int damage, GameObject projectilePrefab = null)
        {
            Angle = angle;
            Power = Mathf.Clamp(power, 1f, 100f);
            Damage = Mathf.Max(1, damage);
            ProjectilePrefab = projectilePrefab;
        }

        public bool CanExecute(Character user)
        {
            return user != null && !user.IsDead;
        }

        public void Execute(Character user, Character target)
        {
            if (!CanExecute(user)) return;

            // Calcular vector de dirección a partir del ángulo
            float rad = Angle * Mathf.Deg2Rad;
            Vector2 launchDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            Debug.Log($"[ActionShoot] {user.CharacterName} dispara hacia {launchDirection} con Potencia: {Power}, Daño: {Damage}");

            // Notificar al TurnManager que hay una acción ejecutándose en la escena (inputs bloqueados)
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.NotifyActionExecuting();
            }

            // Si hay Prefab de Proyectil físico, instanciarlo y lanzarlo
            if (ProjectilePrefab != null)
            {
                Vector3 spawnPos = user.transform.position + (Vector3)(launchDirection * 1.2f);
                GameObject projObj = Object.Instantiate(ProjectilePrefab, spawnPos, Quaternion.identity);

                if (projObj.TryGetComponent<Projectile>(out var projectile))
                {
                    projectile.Launch(launchDirection, Power, user, Damage);
                }
            }
            else
            {
                // Fallback directo por si se prueba sin Prefab físico
                if (target != null && !target.IsDead)
                {
                    target.TakeDamage(Damage);
                }

                if (TurnManager.Instance != null)
                {
                    TurnManager.Instance.NotifyActionResolved();
                }
            }
        }
    }
}
