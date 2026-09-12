using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Configuración inmutable de las propiedades de un material de terreno o cobertura (Data-Driven).
    /// Define la resistencia o absorción de daño pasiva.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTerrainMaterial", menuName = "CosmosCritters/Environment/Terrain Material")]
    public class TerrainMaterialSO : ScriptableObject
    {
        [Header("Material Identity")]
        [SerializeField] private string _materialName = "Roca Lunar";
        [SerializeField] private Sprite _materialSprite;

        [Header("Resistance & Hardness")]
        [Tooltip("Cantidad de daño fijo que absorbe el material antes de restar integridad")]
        [SerializeField] private int _damageResistance = 10;

        public string MaterialName => _materialName;
        public Sprite MaterialSprite => _materialSprite;
        public int DamageResistance => _damageResistance;
    }
}
