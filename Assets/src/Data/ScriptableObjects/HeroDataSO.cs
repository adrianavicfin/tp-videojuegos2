using UnityEngine;

namespace CosmosCritters
{
    [CreateAssetMenu(fileName = "NewHeroData", menuName = "CosmosCritters/Heroes/Hero Data")]
    public class HeroDataSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _heroName = "New Critter";
        [SerializeField] private Sprite _portrait;
        [SerializeField] private Sprite _characterSprite;

        [Header("Stats")]
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpForce = 7f;
        [Tooltip("Multiplicador porcentual de gravedad (1.0 = normal, 0.5 = baja gravedad, 2.0 = alta gravedad).")]
        [SerializeField] private float _gravityMultiplier = 1.0f;

        [Header("Role & Loadout")]
        [SerializeField] private HeroRole _role = HeroRole.HeavyDamage;
        [SerializeField] private WeaponDataSO _defaultWeapon;
        [SerializeField] private AbilityDataSO _secondaryAbility;

        public string HeroName => _heroName;
        public Sprite Portrait => _portrait;
        public Sprite CharacterSprite => _characterSprite;
        public int MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float JumpForce => _jumpForce;
        public float GravityMultiplier => _gravityMultiplier;
        public HeroRole Role => _role;
        public WeaponDataSO DefaultWeapon => _defaultWeapon;
        public AbilityDataSO SecondaryAbility => _secondaryAbility;
    }
}
