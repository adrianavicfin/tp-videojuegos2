using System;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Representa al Jefe / Boss colosal de la partida con fases de combate (H3-T8).
    /// Conecta con BossAIController para la ejecución de su turno y gestiona las transiciones de fase.
    /// </summary>
    [RequireComponent(typeof(BossAIController))]
    public class Boss : Enemy
    {
        public event Action<int> OnPhaseChanged;

        [Header("Boss Specifics")]
        [SerializeField] private int _currentPhase = 1;
        [SerializeField] private int _totalPhases = 3;
        [SerializeField] private BossAIController _aiController;

        public int CurrentPhase => _currentPhase;
        public int TotalPhases => _totalPhases;
        public BossAIController AIController => _aiController;

        protected override void Awake()
        {
            base.Awake();

            if (_aiController == null)
            {
                _aiController = GetComponent<BossAIController>();
            }

            if (GetComponent<CharacterMovementController>() == null)
            {
                gameObject.AddComponent<CharacterMovementController>();
            }
        }

        public override void ExecuteAITurn()
        {
            Debug.Log($"[Boss] {_characterName} iniciando turno en Fase {_currentPhase}.");

            if (_aiController != null)
            {
                _aiController.PerformTurnAction();
            }
            else
            {
                Debug.LogWarning("[Boss] No se encontró BossAIController adjunto.");
                TurnManager.Instance?.NotifyActionResolved();
            }
        }

        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);

            if (MaxHealth > 0 && !IsDead)
            {
                float healthPercentage = (float)CurrentHealth / MaxHealth;
                if (healthPercentage <= 0.6f && _currentPhase == 1)
                {
                    AdvancePhase(2);
                }
                else if (healthPercentage <= 0.3f && _currentPhase == 2)
                {
                    AdvancePhase(3);
                }
            }
        }

        private void AdvancePhase(int nextPhase)
        {
            _currentPhase = nextPhase;
            Debug.Log($"[Boss] ¡{_characterName} entró en Fase {_currentPhase}!");
            OnPhaseChanged?.Invoke(_currentPhase);
        }
    }
}
