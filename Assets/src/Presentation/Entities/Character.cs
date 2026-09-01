using System;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Clase abstracta base para todas las entidades participantes en la cola de turnos (Héroes, Enemigos, Boss).
    /// Contiene los atributos compartidos de combate, movimiento e identidad de turno.
    /// </summary>
    public abstract class Character : MonoBehaviour
    {
        public event Action<int, int> OnHealthChanged; // (current, max)
        public event Action OnDied;

        [Header("Identity & Turn Queue")]
        [SerializeField] protected string _characterName = "Entity";
        [SerializeField] protected SpriteRenderer _spriteRenderer;

        [Header("Stats")]
        [SerializeField] protected int _currentHealth = 100;
        [SerializeField] protected int _maxHealth = 100;
        [SerializeField] protected float _moveSpeed = 5f;
        [SerializeField] protected float _jumpForce = 7f;

        #region Properties
        public string CharacterName => _characterName;
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public float JumpForce => _jumpForce;
        public bool IsDead => _currentHealth <= 0;
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        #endregion

        #region Health Operations
        public virtual void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0) return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (_currentHealth == 0)
            {
                Die();
            }
        }

        public virtual void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        protected virtual void Die()
        {
            OnDied?.Invoke();
        }
        #endregion

        #region Turn Lifecycle (Polimorfismo para la Cola de Turnos)
        /// <summary>
        /// Se invoca cuando este Character toma el turno activo en la cola del TurnManager.
        /// </summary>
        public abstract void StartTurn();

        /// <summary>
        /// Se invoca cuando este Character termina o cede su turno.
        /// </summary>
        public abstract void EndTurn();
        #endregion
    }
}
