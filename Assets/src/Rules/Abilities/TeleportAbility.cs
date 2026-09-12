using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Habilidad secundaria de salto cuántico / teletransporte orbital.
    /// </summary>
    public class TeleportAbility : Ability
    {
        public Vector2 TargetPosition { get; set; }

        public TeleportAbility(int cooldownTurns = 3) 
            : base("Salto Cuántico", cooldownTurns)
        {
        }

        protected override void ExecuteEffect(Character user, Character target)
        {
            Debug.Log($"[TeleportAbility] {user.CharacterName} ejecuta {AbilityName}. Reubicación instantánea hacia {TargetPosition}.");
            user.transform.position = (Vector3)TargetPosition;
        }
    }
}
