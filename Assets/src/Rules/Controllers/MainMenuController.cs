using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CosmosCritters
{
    /// <summary>
    /// Controlador del Menú Principal que gestiona la selección de héroes, mapa y dificultad,
    /// persistiendo los datos a través del GameManager al cargar la escena de combate (Hito 2).
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Available Data")]
        [SerializeField] private List<HeroDataSO> _availableHeroes = new List<HeroDataSO>();

        [Header("Scene Configuration")]
        [SerializeField] private string _gameSceneName = "GameScene";

        [Header("Current Selection State")]
        [SerializeField] private int _selectedMapIndex = 0;
        [SerializeField] private float _turnDuration = 15f;
        private readonly List<HeroDataSO> _selectedHeroes = new List<HeroDataSO>();

        public IReadOnlyList<HeroDataSO> SelectedHeroes => _selectedHeroes;
        public int SelectedMapIndex => _selectedMapIndex;
        public float TurnDuration => _turnDuration;

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
        }

        public void SelectHeroByIndex(int availableIndex)
        {
            if (availableIndex >= 0 && availableIndex < _availableHeroes.Count)
            {
                ToggleSelectHero(_availableHeroes[availableIndex]);
            }
        }

        public void SelectMap(int mapIndex)
        {
            _selectedMapIndex = Mathf.Max(0, mapIndex);
            Debug.Log($"[MainMenu] Mapa seleccionado: {_selectedMapIndex}");
        }

        public void SetTurnDuration(float duration)
        {
            _turnDuration = Mathf.Max(5f, duration);
            Debug.Log($"[MainMenu] Duración de turno configurada: {_turnDuration}s");
        }
        #endregion

        #region Match Launch & Persistence
        public void StartMatch()
        {
            if (_selectedHeroes.Count == 0 && _availableHeroes.Count > 0)
            {
                // Fallback automático por si no se seleccionó ninguno manualmente
                _selectedHeroes.Add(_availableHeroes[0]);
            }

            MatchSettings settings = new MatchSettings(_selectedHeroes, _selectedMapIndex, _turnDuration);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetMatchSettings(settings);
            }

            Debug.Log($"[MainMenu] Iniciando partida... Cargando escena '{_gameSceneName}'");
            SceneManager.LoadScene(_gameSceneName);
        }
        #endregion
    }
}
