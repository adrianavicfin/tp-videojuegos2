using UnityEngine;

namespace CosmosCritters
{
    [CreateAssetMenu(fileName = "Ability_Shield", menuName = "CosmosCritters/Abilities/Shield Ability")]
    public class ShieldAbilityDataSO : AbilityDataSO
    {
        [Header("Shield Settings")]
        [SerializeField] private int _shieldPoints = 25;

        public int ShieldPoints => _shieldPoints;

        public override Ability CreateRuntimeAbility()
        {
            return new ShieldAbility(_shieldPoints, CooldownTurns, AbilityName, Description, Icon, ThemeColor);
        }
    }
}
