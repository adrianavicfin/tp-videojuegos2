using System;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Implementación pura C# de un temporizador regresivo.
    /// </summary>
    public class CountdownTimer : ICountdownTimer
    {
        public float RemainingTime { get; private set; }
        public float TotalDuration { get; private set; }
        public bool IsRunning { get; private set; }
        public float Progress => TotalDuration > 0 ? Mathf.Clamp01(RemainingTime / TotalDuration) : 0f;

        public event Action<float> OnTick;
        public event Action OnFinished;

        public void Start(float duration)
        {
            TotalDuration = Mathf.Max(0f, duration);
            RemainingTime = TotalDuration;
            IsRunning = true;
            OnTick?.Invoke(RemainingTime);
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;

            RemainingTime -= deltaTime;
            OnTick?.Invoke(Mathf.Max(0f, RemainingTime));

            if (RemainingTime <= 0f)
            {
                RemainingTime = 0f;
                IsRunning = false;
                OnFinished?.Invoke();
            }
        }

        public void Pause() => IsRunning = false;
        public void Resume() => IsRunning = true;
        public void Stop() => IsRunning = false;

        public void Reset()
        {
            IsRunning = false;
            RemainingTime = TotalDuration;
            OnTick?.Invoke(RemainingTime);
        }
    }
}
