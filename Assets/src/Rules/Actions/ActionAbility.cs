using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Acción de habilidad especial (curación o soporte) sobre un objetivo aliado o sí mismo.
    /// </summary>
    public class ActionAbility : ICharacterAction
    {
        public string ActionName => "Use Ability";
        public string AbilityName { get; private set; }
        public int HealAmount { get; private set; }

        public ActionAbility(string abilityName, int healAmount)
        {
            AbilityName = abilityName;
            HealAmount = Mathf.Max(0, healAmount);
        }

        public bool CanExecute(Character user)
        {
            return user != null && !user.IsDead;
        }

        public void Execute(Character user, Character target)
        {
            if (!CanExecute(user)) return;

            Character recipient = target != null ? target : user;
            Debug.Log($"[ActionAbility] {user.CharacterName} activa habilidad '{AbilityName}' sobre {recipient.CharacterName}. Curación: +{HealAmount}");

            if (HealAmount > 0)
            {
                recipient.Heal(HealAmount);
            }
        }
    }
}
