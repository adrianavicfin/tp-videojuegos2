using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Héroe alienígena controlado por jugador en la cola de turnos.
    /// Configura dinámicamente su modelo de estadísticas y conecta con el presentador del HUD.
    /// </summary>
    public class Hero : Character
    {
        [Header("Hero Configuration")]
        [SerializeField] private HeroDataSO _heroData;

        public HeroDataSO HeroData => _heroData;
        public int SlotIndex { get; private set; } = 1;

        protected override void Awake()
        {
            if (_heroData != null)
            {
                Initialize(_heroData, SlotIndex);
            }
            else
            {
                base.Awake();
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
                _moveSpeed = _heroData.MoveSpeed;
                _jumpForce = _heroData.JumpForce;

                UnbindStatsEvents();
                Stats = new CharacterStats(_heroData.HeroName, _heroData.MaxHealth, _heroData.MoveSpeed, _heroData.JumpForce);
                BindStatsEvents();

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

        #region Actions Execution (Patrón Command)
        public void ExecuteAction(ICharacterAction action, Character target = null)
        {
            if (action == null) return;

            if (action.CanExecute(this))
            {
                action.Execute(this, target);
            }
            else
            {
                Debug.LogWarning($"[Hero] No se puede ejecutar la acción {action.ActionName}.");
            }
        }

        public void ExecuteMove(Vector2 direction, float distance)
        {
            ExecuteAction(new ActionMove(direction, distance));
        }

        public void ExecuteShoot(float angle, float power, int damage, Character target)
        {
            ExecuteAction(new ActionShoot(angle, power, damage), target);
        }

        public void ExecuteAbility(string abilityName, int healAmount, Character target = null)
        {
            ExecuteAction(new ActionAbility(abilityName, healAmount), target);
        }
        #endregion
    }
}
