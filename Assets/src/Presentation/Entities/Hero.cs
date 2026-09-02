using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Héroe alienígena controlado por jugador en la cola de turnos.
    /// Configura su modelo de estadísticas y su Habilidad Secundaria Polimórfica (Hito 2).
    /// </summary>
    public class Hero : Character
    {
        [Header("Hero Configuration")]
        [SerializeField] private HeroDataSO _heroData;

        public HeroDataSO HeroData => _heroData;
        public int SlotIndex { get; private set; } = 1;

        /// <summary>
        /// Habilidad secundaria polimórfica asignada al héroe según su rol.
        /// </summary>
        public Ability SecondaryAbility { get; private set; }

        protected override void Awake()
        {
            if (_heroData != null)
            {
                Initialize(_heroData, SlotIndex);
            }
            else
            {
                base.Awake();
                AssignDefaultSecondaryAbility();
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

                AssignSecondaryAbilityByRole(_heroData.Role);
            }
        }

        private void AssignDefaultSecondaryAbility()
        {
            SecondaryAbility = new HealAbility(25, 2);
        }

        private void AssignSecondaryAbilityByRole(HeroRole role)
        {
            switch (role)
            {
                case HeroRole.Support:
                    SecondaryAbility = new HealAbility(35, 2);
                    break;
                case HeroRole.HeavyDamage:
                    SecondaryAbility = new ShieldAbility(30, 3);
                    break;
                case HeroRole.GravitationalControl:
                case HeroRole.Scout:
                    SecondaryAbility = new TeleportAbility(3);
                    break;
                default:
                    SecondaryAbility = new HealAbility(20, 2);
                    break;
            }
        }

        #region Turn Queue Lifecycle
        public override void StartTurn()
        {
            Debug.Log($"[TurnQueue] Turno activado para el Héroe: {_characterName} (Slot {SlotIndex})");
            SecondaryAbility?.TickCooldown();
        }

        public override void EndTurn()
        {
            Debug.Log($"[TurnQueue] Turno finalizado para el Héroe: {_characterName}");
        }
        #endregion

        #region Actions Execution (Patrón Command & Habilidades Polimórficas)
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

        public void ExecuteSecondaryAbility(Character target = null)
        {
            if (SecondaryAbility != null)
            {
                SecondaryAbility.Trigger(this, target);
            }
            else
            {
                Debug.LogWarning("[Hero] No tiene ninguna habilidad secundaria asignada.");
            }
        }
        #endregion
    }
}
