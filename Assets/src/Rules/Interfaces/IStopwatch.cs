using System;

namespace CosmosCritters
{
    /// <summary>
    /// Contrato para cronómetros progresivos (Stopwatch) que miden tiempo transcurrido hacia adelante.
    /// </summary>
    public interface IStopwatch : ITimer
    {
        float ElapsedTime { get; }
        event Action<float> OnTick;

        void Start();
    }
}
