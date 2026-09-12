using System;

namespace CosmosCritters
{
    /// <summary>
    /// Implementación pura C# de un cronómetro progresivo.
    /// </summary>
    public class StopwatchTimer : IStopwatch
    {
        public float ElapsedTime { get; private set; }
        public bool IsRunning { get; private set; }

        public event Action<float> OnTick;

        public void Start()
        {
            ElapsedTime = 0f;
            IsRunning = true;
            OnTick?.Invoke(ElapsedTime);
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;

            ElapsedTime += deltaTime;
            OnTick?.Invoke(ElapsedTime);
        }

        public void Pause() => IsRunning = false;
        public void Resume() => IsRunning = true;
        public void Stop() => IsRunning = false;

        public void Reset()
        {
            IsRunning = false;
            ElapsedTime = 0f;
            OnTick?.Invoke(ElapsedTime);
        }
    }
}
