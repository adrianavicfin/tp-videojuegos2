namespace CosmosCritters
{
    /// <summary>
    /// Contrato para la Vista de la UI de combate / HUD del jugador (Patrón MVP - View Interface).
    /// Desacopla la lógica de presentación de los componentes gráficos de Unity.
    /// </summary>
    public interface IPlayerHUDView
    {
        void SetCharacterName(string characterName);
        void UpdateHealth(int currentHealth, int maxHealth);
        void SetSlotIndex(int slotIndex);
        void SetTurnActiveState(bool isActive);
        void UpdateCountdown(float remainingSeconds);
    }
}
