using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Habilidad secundaria de salto cuántico / teletransporte orbital.
    /// </summary>
    public class TeleportAbility : Ability
    {
        public Vector2 TargetPosition { get; set; }
        public float DefaultDisplacement { get; private set; }

        public TeleportAbility(int cooldownTurns = 3, float defaultDisplacement = 4f, string abilityName = "Salto Cuántico", string description = "Se desmaterializa y reaparece en una nueva posición del planetoide.", Sprite icon = null, Color? themeColor = null) 
            : base(abilityName, cooldownTurns, description, icon, themeColor ?? new Color(0.85f, 0.4f, 1f, 1f))
        {
            DefaultDisplacement = Mathf.Max(1f, defaultDisplacement);
        }

        protected override void ExecuteEffect(Character user, Character target)
        {
            Vector2 destination = TargetPosition;
            if (destination.sqrMagnitude < 0.01f)
            {
                // Si no se definió coordenada explícita, trasladarse en la dirección de la normal de la superficie
                Vector2 upDir = user != null ? (Vector2)user.transform.up : Vector2.up;
                destination = (Vector2)user.transform.position + upDir * DefaultDisplacement;
            }

            Debug.Log($"[TeleportAbility] {user.CharacterName} ejecuta {AbilityName}. Reubicación instantánea hacia {destination}.");
            user.transform.position = (Vector3)destination;
        }
    }
}
