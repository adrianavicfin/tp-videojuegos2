using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Héroe alienígena controlado por jugador en la cola de turnos.
    /// </summary>
    public class Hero : Character
    {
        [Header("Hero Configuration")]
        [SerializeField] private HeroDataSO _heroData;

        public HeroDataSO HeroData => _heroData;
        public int SlotIndex { get; private set; } = 1;

        protected override void Awake()
        {
            base.Awake();

            if (_heroData != null)
            {
                Initialize(_heroData, SlotIndex);
            }
        }

        public void Initialize(HeroDataSO data, int slotIndex)
        {
            _heroData = data;
            SlotIndex = slotIndex;

            if (_heroData != null)
            {
                _characterName = _heroData.HeroName;
                _maxHealth = _heroData.MaxHealth;
                _currentHealth = _maxHealth;
                _moveSpeed = _heroData.MoveSpeed;
                _jumpForce = _heroData.JumpForce;

                if (_spriteRenderer != null && _heroData.CharacterSprite != null)
                {
                    _spriteRenderer.sprite = _heroData.CharacterSprite;
                }
            }
        }

        #region Turn Queue Lifecycle
        public override void StartTurn()
        {
            Debug.Log($"[TurnQueue] Turno activado para el Héroe: {_characterName} (Slot {SlotIndex})");
        }

        public override void EndTurn()
        {
            Debug.Log($"[TurnQueue] Turno finalizado para el Héroe: {_characterName}");
        }
        #endregion

        #region Actions
        public void ExecuteMove(Vector2 direction) { }
        public void ExecuteShoot(float angle, float force) { }
        public void ExecuteAbility() { }
        #endregion
    }
}
