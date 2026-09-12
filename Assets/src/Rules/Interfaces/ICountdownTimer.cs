using System;

namespace CosmosCritters
{
    /// <summary>
    /// Contrato para temporizadores regresivos (Countdown) con eventos de tick y finalización.
    /// </summary>
    public interface ICountdownTimer : ITimer
    {
        float RemainingTime { get; }
        float TotalDuration { get; }
        float Progress { get; }

        event Action<float> OnTick;
        event Action OnFinished;

        void Start(float duration);
    }
}
