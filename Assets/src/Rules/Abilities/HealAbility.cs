using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Habilidad secundaria de curación para soporte o auto-recuperación.
    /// </summary>
    public class HealAbility : Ability
    {
        public int HealAmount { get; private set; }

        public HealAbility(int healAmount = 30, int cooldownTurns = 2, string abilityName = "Curación de Plasma", string description = "Restaura salud al usuario o aliado objetivo.", Sprite icon = null, Color? themeColor = null) 
            : base(abilityName, cooldownTurns, description, icon, themeColor ?? new Color(0.2f, 0.95f, 0.4f, 1f))
        {
            HealAmount = Mathf.Max(1, healAmount);
        }

        protected override void ExecuteEffect(Character user, Character target)
        {
            Character recipient = target != null ? target : user;
            Debug.Log($"[HealAbility] {user.CharacterName} activa {AbilityName} sobre {recipient.CharacterName}. Salud restaurada: +{HealAmount}");
            recipient.Heal(HealAmount);
        }
    }
}
