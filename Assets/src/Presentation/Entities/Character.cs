using System;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Clase abstracta base para todas las entidades participantes en la cola de turnos.
    /// Implementa IDamageable e IGravityAffected.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public abstract class Character : MonoBehaviour, IDamageable, IGravityAffected
    {
        public event Action<int, int> OnHealthChanged; // (current, max)
        public event Action OnDied;

        [Header("Identity & Visuals")]
        [SerializeField] protected string _characterName = "Entity";
        [SerializeField] protected SpriteRenderer _spriteRenderer;

        [Header("Stats (Initial Config)")]
        [SerializeField] protected int _maxHealth = 100;
        [SerializeField] protected float _moveSpeed = 5f;
        [SerializeField] protected float _jumpForce = 7f;

        protected Rigidbody2D _rb;

        /// <summary>
        /// Modelo de datos de estado vivo en memoria (C# puro).
        /// </summary>
        public CharacterStats Stats { get; protected set; }

        #region IGravityAffected Properties
        public Rigidbody2D Rigidbody => _rb;
        public Transform Transform => transform;
        #endregion

        #region IDamageable & Properties
        public string CharacterName => Stats != null ? Stats.CharacterName : _characterName;
        public int CurrentHealth => Stats != null ? Stats.CurrentHealth : _maxHealth;
        public int MaxHealth => Stats != null ? Stats.MaxHealth : _maxHealth;
        public float MoveSpeed => Stats != null ? Stats.MoveSpeed : _moveSpeed;
        public float JumpForce => Stats != null ? Stats.JumpForce : _jumpForce;
        public bool IsDead => Stats != null && Stats.IsDead;
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (Stats == null)
            {
                Stats = new CharacterStats(_characterName, _maxHealth, _moveSpeed, _jumpForce);
                BindStatsEvents();
            }
        }

        protected virtual void OnDestroy()
        {
            UnbindStatsEvents();
        }
        #endregion

        #region Stats Binding
        protected void BindStatsEvents()
        {
            if (Stats == null) return;
            Stats.OnHealthChanged += HandleStatsHealthChanged;
            Stats.OnDied += HandleStatsDied;
        }

        protected void UnbindStatsEvents()
        {
            if (Stats == null) return;
            Stats.OnHealthChanged -= HandleStatsHealthChanged;
            Stats.OnDied -= HandleStatsDied;
        }

        private void HandleStatsHealthChanged(int current, int max) => OnHealthChanged?.Invoke(current, max);
        private void HandleStatsDied() => OnDied?.Invoke();
        #endregion

        #region Physics & Radial Gravity (FixedUpdate)
        public virtual void ApplyGravitationalPull(Vector2 force)
        {
            if (_rb != null && !_rb.isKinematic)
            {
                _rb.AddForce(force, ForceMode2D.Force);
            }
        }

        public virtual void AlignWithSurface(Vector2 upDirection)
        {
            if (_rb == null || upDirection.sqrMagnitude < 0.001f) return;

            float targetAngle = Mathf.Atan2(upDirection.y, upDirection.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = Mathf.LerpAngle(_rb.rotation, targetAngle, 10f * Time.fixedDeltaTime);
            _rb.MoveRotation(currentAngle);
        }
        #endregion

        #region Health Operations (IDamageable)
        public virtual void TakeDamage(int amount)
        {
            if (Stats == null) return;
            Stats.ApplyDamage(amount);
        }

        public virtual void Heal(int amount)
        {
            if (Stats == null) return;
            Stats.Heal(amount);
        }
        #endregion

        #region Turn Lifecycle
        public abstract void StartTurn();
        public abstract void EndTurn();
        #endregion
    }
}
