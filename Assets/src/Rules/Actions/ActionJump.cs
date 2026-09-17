using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Acción de salto direccional con impulso vectorial (Patrón Command - H3-T6).
    /// Aplica un vector de fuerza/velocidad instantánea sobre el Rigidbody2D del Character.
    /// </summary>
    public class ActionJump : ICharacterAction
    {
        public string ActionName => "Jump";
        public Vector2 Direction { get; private set; }
        public float Force { get; private set; }

        public ActionJump(Vector2 direction, float force)
        {
            Direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.up;
            Force = Mathf.Max(0f, force);
        }

        public bool CanExecute(Character user)
        {
            return user != null && !user.IsDead && user.Rigidbody != null;
        }

        public void Execute(Character user, Character target = null)
        {
            if (!CanExecute(user)) return;

            Debug.Log($"[ActionJump] {user.CharacterName} salta en dirección {Direction} con Fuerza: {Force:F2}");
            
            // Aplicar impulso directo sobre la velocidad del Rigidbody2D
            user.Rigidbody.velocity = Direction * Force;
        }
    }
}
