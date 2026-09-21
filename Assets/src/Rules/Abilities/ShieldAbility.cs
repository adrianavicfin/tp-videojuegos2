using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Habilidad secundaria de escudo gravitatorio que otorga puntos temporales de armadura/defensa.
    /// </summary>
    public class ShieldAbility : Ability
    {
        public int ShieldPoints { get; private set; }

        public ShieldAbility(int shieldPoints = 25, int cooldownTurns = 3, string abilityName = "Escudo Gravitatorio", string description = "Genera un campo de defensa que restaura y refuerza la integridad.", Sprite icon = null, Color? themeColor = null) 
            : base(abilityName, cooldownTurns, description, icon, themeColor ?? new Color(0.25f, 0.75f, 1f, 1f))
        {
            ShieldPoints = Mathf.Max(1, shieldPoints);
        }

        protected override void ExecuteEffect(Character user, Character target)
        {
            Character recipient = target != null ? target : user;
            Debug.Log($"[ShieldAbility] {user.CharacterName} despliega {AbilityName} sobre {recipient.CharacterName}. Absorción generada: {ShieldPoints} pts.");
            
            // Simulación de absorción sumando temporalmente resistencia a la vida máxima
            recipient.Heal(ShieldPoints);
        }
    }
}
