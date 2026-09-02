using System.Collections.Generic;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Singleton persistente que sobrevive entre cambios de escena (DontDestroyOnLoad).
    /// Actúa como Bootstrapper del IoCContainer y puente de datos persistentes (MatchSettings) entre el Menú y la Escena de Juego.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public IoCContainer Container { get; private set; }

        /// <summary>
        /// Datos persistidos seleccionados en el Menú Principal para la partida activa (Hito 2).
        /// </summary>
        public MatchSettings CurrentMatchSettings { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeIoC();
            InitializeDefaultSettings();
        }

        private void InitializeIoC()
        {
            Container = IoCContainer.Instance;
            RegisterDependencies(Container);
        }

        private void InitializeDefaultSettings()
        {
            // Configuración por defecto por si se inicia directo desde la escena de juego
            CurrentMatchSettings = new MatchSettings(new List<HeroDataSO>(), 0, 15f);
        }

        private void RegisterDependencies(IoCContainer container)
        {
            container.AddTransient(typeof(ICountdownTimer), typeof(CountdownTimer));
            container.AddTransient(typeof(IStopwatch), typeof(StopwatchTimer));
        }

        /// <summary>
        /// Guarda la configuración elegida en el menú antes de cargar la escena de combate.
        /// </summary>
        public void SetMatchSettings(MatchSettings settings)
        {
            if (settings != null)
            {
                CurrentMatchSettings = settings;
                Debug.Log($"[GameManager] Configuración guardada: {CurrentMatchSettings.SelectedHeroes.Count} héroes, Mapa: {CurrentMatchSettings.SelectedMapIndex}, Duración: {CurrentMatchSettings.TurnDuration}s");
            }
        }
    }
}
