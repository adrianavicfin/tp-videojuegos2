using System;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Representa al Jefe / Boss colosal de la partida con fases de combate (H3-T8/T9).
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

        [Header("Phase Feedback")]
        [SerializeField] private Color _phaseTwoColor = new Color(1f, 0.38f, 0.08f, 1f);
        [SerializeField] private Color _phaseThreeColor = new Color(1f, 0.08f, 0.55f, 1f);

        [Header("Phase Attacks")]
        [SerializeField, Min(1f)] private float _phaseTwoPowerMultiplier = 1.25f;
        [SerializeField, Min(1f)] private float _phaseThreePowerMultiplier = 1.35f;
        [SerializeField, Min(1f)] private float _phaseThreeDamageMultiplier = 1.5f;
        [SerializeField, Min(1f)] private float _phaseThreeRadiusMultiplier = 1.25f;

        private Color _phaseOneColor;

        public int CurrentPhase => _currentPhase;
        public int TotalPhases => _totalPhases;
        public BossAIController AIController => _aiController;
        public float AttackPowerMultiplier => _currentPhase >= 3 ? _phaseThreePowerMultiplier
            : _currentPhase == 2 ? _phaseTwoPowerMultiplier : 1f;
        public float AttackDamageMultiplier => _currentPhase >= 3 ? _phaseThreeDamageMultiplier : 1f;
        public float AttackRadiusMultiplier => _currentPhase >= 3 ? _phaseThreeRadiusMultiplier : 1f;

        protected override void Awake()
        {
            base.Awake();

            if (_spriteRenderer != null)
            {
                _phaseOneColor = _spriteRenderer.color;
                UpdatePhaseVisual();
            }

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

            if (MaxHealth <= 0 || IsDead) return;

            float healthPercentage = (float)CurrentHealth / MaxHealth;
            int nextPhase = healthPercentage < 0.2f ? 3 : healthPercentage < 0.5f ? 2 : 1;
            if (nextPhase > _currentPhase)
                AdvancePhase(nextPhase);
        }

        private void AdvancePhase(int nextPhase)
        {
            _currentPhase = nextPhase;
            UpdatePhaseVisual();
            Debug.Log($"[Boss] ¡{_characterName} entró en Fase {_currentPhase}!");
            OnPhaseChanged?.Invoke(_currentPhase);
        }

        private void UpdatePhaseVisual()
        {
            if (_spriteRenderer == null) return;
            _spriteRenderer.color = _currentPhase >= 3 ? _phaseThreeColor
                : _currentPhase == 2 ? _phaseTwoColor : _phaseOneColor;
        }
    }
}
