using System;

namespace CosmosCritters
{
    /// <summary>
    /// Modelo de datos (C# puro / Engine-Agnostic) para el estado mutable de estadísticas y salud de una entidad.
    /// Parte del Patrón MVP (Model).
    /// </summary>
    public class CharacterStats
    {
        public string CharacterName { get; private set; }
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public float MoveSpeed { get; private set; }
        public float JumpForce { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public event Action<int, int> OnHealthChanged; // (current, max)
        public event Action OnDied;

        public CharacterStats(string name, int maxHealth, float moveSpeed, float jumpForce)
        {
            CharacterName = name;
            MaxHealth = Math.Max(1, maxHealth);
            CurrentHealth = MaxHealth;
            MoveSpeed = Math.Max(0f, moveSpeed);
            JumpForce = Math.Max(0f, jumpForce);
        }

        public void ApplyDamage(int amount)
        {
            if (IsDead || amount <= 0) return;

            CurrentHealth = Math.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth == 0)
            {
                OnDied?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0) return;

            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }
}
