namespace CosmosCritters
{
    /// <summary>
    /// Contrato para el modelado de acciones ejecutables por un Character durante su turno (Patrón Command).
    /// </summary>
    public interface ICharacterAction
    {
        string ActionName { get; }
        bool CanExecute(Character user);
        void Execute(Character user, Character target);
    }
}
