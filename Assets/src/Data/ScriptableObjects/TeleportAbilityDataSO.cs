using UnityEngine;

namespace CosmosCritters
{
    [CreateAssetMenu(fileName = "Ability_Teleport", menuName = "CosmosCritters/Abilities/Teleport Ability")]
    public class TeleportAbilityDataSO : AbilityDataSO
    {
        [Header("Teleport Settings")]
        [SerializeField] private float _defaultDisplacement = 4f;

        public float DefaultDisplacement => _defaultDisplacement;

        public override Ability CreateRuntimeAbility()
        {
            return new TeleportAbility(CooldownTurns, _defaultDisplacement, AbilityName, Description, Icon, ThemeColor);
        }
    }
}
