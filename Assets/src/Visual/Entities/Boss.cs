using System;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Representa al Jefe / Boss colosal de la partida con fases de combate.
    /// </summary>
    public class Boss : Enemy
    {
        public event Action<int> OnPhaseChanged;

        [Header("Boss Specifics")]
        [SerializeField] private int _currentPhase = 1;
        [SerializeField] private int _totalPhases = 3;

        public int CurrentPhase => _currentPhase;
        public int TotalPhases => _totalPhases;

        public override void ExecuteAITurn()
        {
            Debug.Log($"[Boss] Ejecutando ataque de IA en Fase {_currentPhase}");
        }

        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);

            float healthPercentage = (float)_currentHealth / _maxHealth;
            if (healthPercentage <= 0.5f && _currentPhase == 1)
            {
                AdvancePhase(2);
            }
            else if (healthPercentage <= 0.2f && _currentPhase == 2)
            {
                AdvancePhase(3);
            }
        }

        private void AdvancePhase(int nextPhase)
        {
            _currentPhase = nextPhase;
            Debug.Log($"[Boss] ¡El Jefe entró en Fase {_currentPhase}!");
            OnPhaseChanged?.Invoke(_currentPhase);
        }
    }
}
