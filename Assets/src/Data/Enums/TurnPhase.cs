namespace CosmosCritters
{
    public enum TurnPhase
    {
        WaitingInput,       // El jugador activo se mueve y apunta (countdown activo)
        ActionExecuting,    // Proyectil en vuelo / Físicas resolviéndose (inputs bloqueados)
        Resolving,          // Chequeo de daño, knockback y condiciones de victoria/derrota
        RoundEnded          // Fin de la ronda global (recalcula cola de turnos o eventos)
    }
}
