namespace CosmosCritters
{
    /// <summary>
    /// Contrato para cualquier entidad o elemento del entorno capaz de recibir daño y destruirse.
    /// </summary>
    public interface IDamageable
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsDead { get; }
        void TakeDamage(int amount);
    }
}
