using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Clase abstracta base para entidades hostiles en la cola de turnos.
    /// </summary>
    public abstract class Enemy : Character
    {
        #region Turn Queue Lifecycle
        public override void StartTurn()
        {
            Debug.Log($"[TurnQueue] Turno de IA activado para el Enemigo: {_characterName}");
            ExecuteAITurn();
        }

        public override void EndTurn()
        {
            Debug.Log($"[TurnQueue] Turno de IA finalizado para: {_characterName}");
        }
        #endregion

        /// <summary>
        /// Lógica de decisión y ataque de la IA durante su turno.
        /// </summary>
        public abstract void ExecuteAITurn();
    }
}
