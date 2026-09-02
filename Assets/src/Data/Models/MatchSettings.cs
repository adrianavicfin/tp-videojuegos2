using System.Collections.Generic;

namespace CosmosCritters
{
    /// <summary>
    /// Contenedor de datos de configuración de la partida seleccionados desde el Menú Principal.
    /// Cumple el requisito de persistencia de al menos 3 datos para Hito 2.
    /// </summary>
    public class MatchSettings
    {
        // Dato 1: Lista de héroes/alienígenas seleccionados para la escuadra
        public List<HeroDataSO> SelectedHeroes { get; private set; } = new List<HeroDataSO>();

        // Dato 2: Índice o identificador del mapa/escenario seleccionado
        public int SelectedMapIndex { get; private set; } = 0;

        // Dato 3: Duración del temporizador de turno (en segundos)
        public float TurnDuration { get; private set; } = 15f;

        public MatchSettings(List<HeroDataSO> selectedHeroes, int mapIndex, float turnDuration)
        {
            if (selectedHeroes != null)
            {
                SelectedHeroes = new List<HeroDataSO>(selectedHeroes);
            }
            SelectedMapIndex = mapIndex;
            TurnDuration = turnDuration > 0f ? turnDuration : 15f;
        }

        public void SetSelectedHeroes(List<HeroDataSO> heroes)
        {
            if (heroes != null)
            {
                SelectedHeroes = new List<HeroDataSO>(heroes);
            }
        }

        public void SetMapIndex(int mapIndex)
        {
            SelectedMapIndex = mapIndex;
        }

        public void SetTurnDuration(float duration)
        {
            TurnDuration = duration > 0f ? duration : 15f;
        }
    }
}
