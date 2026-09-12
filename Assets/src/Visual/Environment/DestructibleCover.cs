using System;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Cobertura destructible en el escenario planetario con resistencia pasiva de material.
    /// Implementa IDamageable para recibir daño de proyectiles balísticos.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DestructibleCover : MonoBehaviour, IDamageable
    {
        public event Action<int, int> OnIntegrityChanged; // (current, max)
        public event Action OnDestroyed;

        [Header("Material Configuration")]
        [SerializeField] private TerrainMaterialSO _materialData;

        [Header("Integrity Stats")]
        [SerializeField] private int _maxIntegrity = 100;
        [SerializeField] private int _currentIntegrity = 100;
        [SerializeField] private GameObject _debrisPrefab;

        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;

        #region IDamageable
        public int CurrentHealth => _currentIntegrity;
        public int MaxHealth => _maxIntegrity;
        public bool IsDead => _currentIntegrity <= 0;
        #endregion

        public TerrainMaterialSO MaterialData => _materialData;

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _currentIntegrity = _maxIntegrity;

            if (_materialData != null && _spriteRenderer != null && _materialData.MaterialSprite != null)
            {
                _spriteRenderer.sprite = _materialData.MaterialSprite;
            }
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0) return;

            // Calcular absorción pasiva según la resistencia del material
            int resistance = _materialData != null ? _materialData.DamageResistance : 0;
            int netDamage = Mathf.Max(1, amount - resistance);

            _currentIntegrity = Mathf.Max(0, _currentIntegrity - netDamage);
            Debug.Log($"[DestructibleCover] {gameObject.name} (Material: {_materialData?.MaterialName}) recibió {amount} de daño (absorbido: {resistance}). Daño neto: {netDamage}. Integridad: {_currentIntegrity}/{_maxIntegrity}");

            OnIntegrityChanged?.Invoke(_currentIntegrity, _maxIntegrity);

            if (_currentIntegrity == 0)
            {
                BreakCover();
            }
        }

        private void BreakCover()
        {
            Debug.Log($"[DestructibleCover] ¡Cobertura {gameObject.name} destruida!");
            OnDestroyed?.Invoke();

            if (_debrisPrefab != null)
            {
                Instantiate(_debrisPrefab, transform.position, transform.rotation);
            }

            Destroy(gameObject);
        }
    }
}
