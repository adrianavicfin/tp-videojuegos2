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
        [SerializeField] private WeaponDataSO _equippedWeapon;

        public HeroDataSO HeroData => _heroData;
        public WeaponDataSO EquippedWeapon => _equippedWeapon;
        public int SlotIndex { get; private set; } = 1;

        /// <summary>
        /// Habilidad secundaria polimórfica asignada al héroe según su rol.
        /// </summary>
        public Ability SecondaryAbility { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (GetComponent<CharacterMovementController>() == null)
            {
                gameObject.AddComponent<CharacterMovementController>();
            }

            if (_heroData != null)
            {
                Initialize(_heroData, SlotIndex);
            }
            else
            {
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
                _gravityMultiplier = _heroData.GravityMultiplier;

                if (_heroData.DefaultWeapon != null)
                {
                    _equippedWeapon = _heroData.DefaultWeapon;
                }

                UnbindStatsEvents();
                Stats = new CharacterStats(_heroData.HeroName, _heroData.MaxHealth, _heroData.MoveSpeed, _heroData.JumpForce, _heroData.GravityMultiplier);
                BindStatsEvents();

                if (_spriteRenderer != null && _heroData.CharacterSprite != null)
                {
                    _spriteRenderer.sprite = _heroData.CharacterSprite;
                }

                AssignSecondaryAbilityByRole(_heroData.Role);
            }
        }

        public void EquipWeapon(WeaponDataSO weapon)
        {
            if (weapon == null) return;
            _equippedWeapon = weapon;
            Debug.Log($"[Hero] {_characterName} equipó el arma: {weapon.WeaponName}");
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

        public void ExecuteJump(Vector2 direction, float force)
        {
            ExecuteAction(new ActionJump(direction, force));
        }

        public void ExecuteShoot(float angle, float power, int damage, Character target = null)
        {
            GameObject prefab = _equippedWeapon != null ? _equippedWeapon.ProjectilePrefab : null;
            float radius = _equippedWeapon != null ? _equippedWeapon.ExplosionRadius : 2.5f;
            float knockback = _equippedWeapon != null ? _equippedWeapon.KnockbackForce : 15f;
            int finalDamage = _equippedWeapon != null ? _equippedWeapon.BaseDamage : damage;

            ExecuteAction(new ActionShoot(angle, power, finalDamage, prefab, radius, knockback), target);
        }

        public void ExecuteShoot(Vector2 direction, float power)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float maxPower = _equippedWeapon != null ? _equippedWeapon.MaxPower : 25f;
            float clampedPower = Mathf.Clamp(power, 1f, maxPower);

            GameObject prefab = _equippedWeapon != null ? _equippedWeapon.ProjectilePrefab : null;
            float radius = _equippedWeapon != null ? _equippedWeapon.ExplosionRadius : 2.5f;
            float knockback = _equippedWeapon != null ? _equippedWeapon.KnockbackForce : 15f;
            int damage = _equippedWeapon != null ? _equippedWeapon.BaseDamage : 35;

            ExecuteAction(new ActionShoot(angle, clampedPower, damage, prefab, radius, knockback), null);
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
