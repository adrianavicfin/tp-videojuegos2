using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Acción de desplazamiento orbital/superficial de un Character.
    /// </summary>
    public class ActionMove : ICharacterAction
    {
        public string ActionName => "Move";
        public Vector2 Direction { get; private set; }
        public float Distance { get; private set; }

        public ActionMove(Vector2 direction, float distance)
        {
            Direction = direction.normalized;
            Distance = Mathf.Max(0f, distance);
        }

        public bool CanExecute(Character user)
        {
            return user != null && !user.IsDead;
        }

        public void Execute(Character user, Character target)
        {
            if (!CanExecute(user)) return;

            Debug.Log($"[ActionMove] {user.CharacterName} se desplaza en dirección {Direction} una distancia de {Distance} unidades.");
            user.transform.position += (Vector3)(Direction * Distance);
        }
    }
}
