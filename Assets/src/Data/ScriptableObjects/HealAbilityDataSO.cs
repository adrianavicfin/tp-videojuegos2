using UnityEngine;

namespace CosmosCritters
{
    [CreateAssetMenu(fileName = "Ability_Heal", menuName = "CosmosCritters/Abilities/Heal Ability")]
    public class HealAbilityDataSO : AbilityDataSO
    {
        [Header("Heal Settings")]
        [SerializeField] private int _healAmount = 30;

        public int HealAmount => _healAmount;

        public override Ability CreateRuntimeAbility()
        {
            return new HealAbility(_healAmount, CooldownTurns, AbilityName, Description, Icon, ThemeColor);
        }
    }
}
