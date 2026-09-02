using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Acción de disparo balístico de proyectil que inflige daño al objetivo.
    /// </summary>
    public class ActionShoot : ICharacterAction
    {
        public string ActionName => "Shoot";
        public float Angle { get; private set; }
        public float Power { get; private set; }
        public int Damage { get; private set; }

        public ActionShoot(float angle, float power, int damage)
        {
            Angle = angle;
            Power = Mathf.Clamp(power, 0f, 100f);
            Damage = Mathf.Max(1, damage);
        }

        public bool CanExecute(Character user)
        {
            return user != null && !user.IsDead;
        }

        public void Execute(Character user, Character target)
        {
            if (!CanExecute(user)) return;

            Debug.Log($"[ActionShoot] {user.CharacterName} dispara con Ángulo: {Angle}°, Potencia: {Power}%, Daño: {Damage}");

            if (target != null && !target.IsDead)
            {
                target.TakeDamage(Damage);
                Debug.Log($"[ActionShoot] Impacto directo en {target.CharacterName}. Salud restante: {target.CurrentHealth}/{target.MaxHealth}");
            }
        }
    }
}
