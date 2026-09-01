using System;
using UnityEngine;

namespace CosmosCritters
{
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // 1. Registro de dependencias en el contenedor de IoC
                RegisterDependencies(IoCContainer.Instance);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void RegisterDependencies(IoCContainer container)
        {
            // -------------------------------------------------------------
            // TRANSIENT: Nuevas instancias de temporizadores y cronómetros
            // -------------------------------------------------------------
            container.AddTransient(typeof(ICountdownTimer), typeof(CountdownTimer));
            container.AddTransient(typeof(IStopwatch), typeof(StopwatchTimer));

            // -------------------------------------------------------------
            // SCOPED: Viven solo durante la escena / partida activa
            // -------------------------------------------------------------
            // container.AddScoped(typeof(IGravitySimulator), typeof(GravitySimulator));

            // -------------------------------------------------------------
            // SINGLETONS: Viven para siempre en toda la aplicación
            // -------------------------------------------------------------
            // container.AddSingleton(typeof(IAudioService), typeof(AudioService));
        }
    }
}
