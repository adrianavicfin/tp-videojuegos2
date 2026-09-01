namespace CosmosCritters
{
    /// <summary>
    /// Contrato base para cualquier controlador o medidor de tiempo en el juego.
    /// </summary>
    public interface ITimer
    {
        bool IsRunning { get; }
        void Tick(float deltaTime);
        void Pause();
        void Resume();
        void Stop();
        void Reset();
    }
}
