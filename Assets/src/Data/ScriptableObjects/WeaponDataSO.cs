using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Configuración inmutable de un arma y su tipo de proyectil (Data-Driven).
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "CosmosCritters/Weapons/Weapon Data")]
    public class WeaponDataSO : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _weaponName = "Lanzador de Plasma";
        [SerializeField] private Sprite _weaponIcon;

        [Header("Combat Stats")]
        [SerializeField] private int _baseDamage = 35;
        [SerializeField] private float _explosionRadius = 2.5f;
        [SerializeField] private float _knockbackForce = 15f;

        [Header("Ballistics")]
        [SerializeField] private float _maxPower = 25f;
        [SerializeField] private GameObject _projectilePrefab;

        public string WeaponName => _weaponName;
        public Sprite WeaponIcon => _weaponIcon;
        public int BaseDamage => _baseDamage;
        public float ExplosionRadius => _explosionRadius;
        public float KnockbackForce => _knockbackForce;
        public float MaxPower => _maxPower;
        public GameObject ProjectilePrefab => _projectilePrefab;
    }
}
