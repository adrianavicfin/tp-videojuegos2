using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Habilidad secundaria de curación para soporte o auto-recuperación.
    /// </summary>
    public class HealAbility : Ability
    {
        public int HealAmount { get; private set; }

        public HealAbility(int healAmount = 30, int cooldownTurns = 2) 
            : base("Curación de Plasma", cooldownTurns)
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
