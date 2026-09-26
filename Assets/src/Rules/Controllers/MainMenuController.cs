using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CosmosCritters
{
    /// <summary>
    /// Controlador del Menú Principal que gestiona la selección de héroes en slots, mapa y duración del turno,
    /// notificando a la vista/UI mediante eventos y persistiendo los datos a través del GameManager (Hito 2).
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        public event Action<IReadOnlyList<HeroDataSO>> OnSelectedHeroesChanged;
        public event Action<int> OnMapChanged;
        public event Action<float> OnTurnDurationChanged;

        [Header("Available Data")]
        [SerializeField] private List<HeroDataSO> _availableHeroes = new List<HeroDataSO>();

        [Header("Scene Configuration")]
        [SerializeField] private string _gameSceneName = "GameplayScene";

        [Header("Current Selection State")]
        [SerializeField] private int _selectedMapIndex = 0;
        [SerializeField] private float _turnDuration = 15f;
        private readonly List<HeroDataSO> _selectedHeroes = new List<HeroDataSO>();

        public IReadOnlyList<HeroDataSO> AvailableHeroes => _availableHeroes;
        public IReadOnlyList<HeroDataSO> SelectedHeroes => _selectedHeroes;
        public int SelectedMapIndex => _selectedMapIndex;
        public float TurnDuration => _turnDuration;

        private void Start()
        {
            // Auto-seleccionar por defecto los primeros disponibles si la lista arranca vacía
            if (_selectedHeroes.Count == 0 && _availableHeroes.Count > 0)
            {
                int initialCount = Mathf.Min(2, _availableHeroes.Count);
                for (int i = 0; i < initialCount; i++)
                {
                    _selectedHeroes.Add(_availableHeroes[i]);
                }
            }

            NotifyStateChanged();
        }

        #region Selection API (Para conectar a los botones del Canvas)
        public void ToggleSelectHero(HeroDataSO hero)
        {
            if (hero == null) return;

            if (_selectedHeroes.Contains(hero))
            {
                _selectedHeroes.Remove(hero);
                Debug.Log($"[MainMenu] Héroe deseleccionado: {hero.HeroName}. Total seleccionados: {_selectedHeroes.Count}");
            }
            else
            {
                if (_selectedHeroes.Count < 4)
                {
                    _selectedHeroes.Add(hero);
                    Debug.Log($"[MainMenu] Héroe seleccionado: {hero.HeroName}. Total seleccionados: {_selectedHeroes.Count}");
                }
                else
                {
                    Debug.LogWarning("[MainMenu] No se pueden seleccionar más de 4 héroes para la escuadra.");
                }
            }

            OnSelectedHeroesChanged?.Invoke(_selectedHeroes);
        }

        public void SelectHeroByIndex(int availableIndex)
        {
            if (availableIndex >= 0 && availableIndex < _availableHeroes.Count)
            {
                ToggleSelectHero(_availableHeroes[availableIndex]);
            }
        }

        public void RemoveHeroFromSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < _selectedHeroes.Count)
            {
                HeroDataSO removed = _selectedHeroes[slotIndex];
                _selectedHeroes.RemoveAt(slotIndex);
                Debug.Log($"[MainMenu] Héroe {removed.HeroName} removido del slot {slotIndex + 1}.");
                OnSelectedHeroesChanged?.Invoke(_selectedHeroes);
            }
        }

        public void SelectMap(int mapIndex)
        {
            _selectedMapIndex = Mathf.Max(0, mapIndex);
            Debug.Log($"[MainMenu] Mapa seleccionado: {_selectedMapIndex}");
            OnMapChanged?.Invoke(_selectedMapIndex);
        }

        public void SetTurnDuration(float duration)
        {
            _turnDuration = Mathf.Max(5f, duration);
            Debug.Log($"[MainMenu] Duración de turno configurada: {_turnDuration}s");
            OnTurnDurationChanged?.Invoke(_turnDuration);
        }

        private void NotifyStateChanged()
        {
            OnSelectedHeroesChanged?.Invoke(_selectedHeroes);
            OnMapChanged?.Invoke(_selectedMapIndex);
            OnTurnDurationChanged?.Invoke(_turnDuration);
        }
        #endregion

        #region Match Launch & Persistence
        public void StartMatch()
        {
            if (_selectedHeroes.Count == 0)
            {
                Debug.LogWarning("[MainMenu] Selecciona al menos un heroe antes de iniciar la partida.");
                return;
            }

            if (_selectedHeroes.Count > 4)
            {
                _selectedHeroes.RemoveRange(4, _selectedHeroes.Count - 4);
            }

            MatchSettings settings = new MatchSettings(_selectedHeroes, _selectedMapIndex, _turnDuration);

            if (GameManager.Instance == null)
            {
                Debug.LogError("[MainMenu] No existe GameManager. Se cancela la carga para no perder la seleccion de heroes.");
                return;
            }


            GameManager.Instance.SetMatchSettings(settings);

            Debug.Log($"[MainMenu] Iniciando partida con {_selectedHeroes.Count} heroes. Cargando escena '{_gameSceneName}'");
            SceneManager.LoadScene(_gameSceneName);
        }
        #endregion
    }
}
