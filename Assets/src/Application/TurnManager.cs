using System;
using System.Collections.Generic;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Orquestador del flujo de combate por turnos, FSM de fases y resolución de victoria/derrota.
    /// Consume las dependencias temporales vía IoC y configura los slots de héroes con los datos persistidos de GameManager (Hito 2).
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        public event Action<TurnPhase> OnPhaseChanged;
        public event Action<Character> OnTurnStarted;
        public event Action<Character> OnTurnEnded;
        public event Action<int> OnRoundStarted;
        public event Action<bool> OnMatchFinished; // true = Win, false = Defeat

        [Header("Match Settings")]
        [SerializeField] private float _turnDuration = 15f;
        [SerializeField] private List<Hero> _heroSlots = new List<Hero>();
        [SerializeField] private Boss _boss;

        private ICountdownTimer _turnTimer;
        private readonly Queue<Character> _turnQueue = new Queue<Character>();
        private Character _activeCharacter;
        private TurnPhase _currentPhase = TurnPhase.WaitingInput;
        private int _currentRound = 0;
        private bool _isMatchOver = false;

        #region Properties
        public Character ActiveCharacter => _activeCharacter;
        public TurnPhase CurrentPhase => _currentPhase;
        public int CurrentRound => _currentRound;
        public ICountdownTimer TurnTimer => _turnTimer;
        #endregion

        #region Unity Lifecycle & Inyección IoC
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (GameManager.Instance != null && GameManager.Instance.Container != null)
            {
                GameManager.Instance.Container.Inject(this);
            }
        }

        public void Construct(ICountdownTimer turnTimer)
        {
            _turnTimer = turnTimer;

            if (_turnTimer != null)
            {
                _turnTimer.OnFinished += HandleTurnTimerFinished;
            }
        }

        private void Start()
        {
            ApplyPersistentMatchSettings();
            SubscribeCombatantEvents();
            StartNewRound();
        }

        private void OnDestroy()
        {
            if (_turnTimer != null)
            {
                _turnTimer.OnFinished -= HandleTurnTimerFinished;
            }
            UnsubscribeCombatantEvents();
        }

        private void Update()
        {
            if (_isMatchOver || _turnTimer == null) return;

            if (_currentPhase == TurnPhase.WaitingInput && _turnTimer.IsRunning)
            {
                _turnTimer.Tick(Time.deltaTime);
            }
        }
        #endregion

        #region Settings Persistence Consumer (Hito 2)
        private void ApplyPersistentMatchSettings()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentMatchSettings != null)
            {
                MatchSettings settings = GameManager.Instance.CurrentMatchSettings;
                _turnDuration = settings.TurnDuration;

                // Configurar los slots de héroes con los ScriptableObjects seleccionados en el Menú
                if (settings.SelectedHeroes != null && settings.SelectedHeroes.Count > 0)
                {
                    for (int i = 0; i < _heroSlots.Count; i++)
                    {
                        if (i < settings.SelectedHeroes.Count && _heroSlots[i] != null)
                        {
                            _heroSlots[i].gameObject.SetActive(true);
                            _heroSlots[i].Initialize(settings.SelectedHeroes[i], i + 1);
                        }
                    }
                }
            }
        }
        #endregion

        #region Queue & Combat Flow
        private void StartNewRound()
        {
            if (_isMatchOver) return;

            _currentRound++;
            _currentPhase = TurnPhase.WaitingInput;
            Debug.Log($"[TurnManager] === Iniciando Ronda {_currentRound} ===");
            OnRoundStarted?.Invoke(_currentRound);

            RebuildTurnQueue();
            AdvanceToNextTurn();
        }

        private void RebuildTurnQueue()
        {
            _turnQueue.Clear();

            for (int i = 0; i < _heroSlots.Count; i++)
            {
                if (_heroSlots[i] != null && !_heroSlots[i].IsDead && _heroSlots[i].gameObject.activeSelf)
                {
                    _turnQueue.Enqueue(_heroSlots[i]);
                }
            }

            if (_boss != null && !_boss.IsDead)
            {
                _turnQueue.Enqueue(_boss);
            }
        }

        public void AdvanceToNextTurn()
        {
            if (_isMatchOver) return;

            if (_turnQueue.Count == 0)
            {
                StartNewRound();
                return;
            }

            _activeCharacter = _turnQueue.Dequeue();

            if (_activeCharacter.IsDead)
            {
                AdvanceToNextTurn();
                return;
            }

            SetPhase(TurnPhase.WaitingInput);

            if (_turnTimer != null)
            {
                _turnTimer.Start(_turnDuration);
            }

            _activeCharacter.StartTurn();
            OnTurnStarted?.Invoke(_activeCharacter);
        }

        public void EndCurrentTurn()
        {
            if (_activeCharacter != null)
            {
                _activeCharacter.EndTurn();
                OnTurnEnded?.Invoke(_activeCharacter);
            }

            if (_turnTimer != null)
            {
                _turnTimer.Stop();
            }

            AdvanceToNextTurn();
        }

        private void HandleTurnTimerFinished()
        {
            Debug.Log("[TurnManager] ¡Tiempo agotado para el turno activo!");
            EndCurrentTurn();
        }
        #endregion

        #region Phase Transitions
        public void SetPhase(TurnPhase newPhase)
        {
            _currentPhase = newPhase;
            OnPhaseChanged?.Invoke(_currentPhase);
        }

        public void NotifyActionExecuting()
        {
            SetPhase(TurnPhase.ActionExecuting);
            if (_turnTimer != null)
            {
                _turnTimer.Pause();
            }
        }

        public void NotifyActionResolved()
        {
            SetPhase(TurnPhase.Resolving);
            CheckEndGameConditions();

            if (!_isMatchOver)
            {
                EndCurrentTurn();
            }
        }
        #endregion

        #region Combat Conditions & Event Subscriptions
        private void SubscribeCombatantEvents()
        {
            if (_boss != null)
            {
                _boss.OnDied += HandleBossDied;
            }

            for (int i = 0; i < _heroSlots.Count; i++)
            {
                if (_heroSlots[i] != null)
                {
                    _heroSlots[i].OnDied += HandleHeroDied;
                }
            }
        }

        private void UnsubscribeCombatantEvents()
        {
            if (_boss != null)
            {
                _boss.OnDied -= HandleBossDied;
            }

            for (int i = 0; i < _heroSlots.Count; i++)
            {
                if (_heroSlots[i] != null)
                {
                    _heroSlots[i].OnDied -= HandleHeroDied;
                }
            }
        }

        private void HandleBossDied()
        {
            TriggerMatchEnd(true);
        }

        private void HandleHeroDied()
        {
            bool anyHeroAlive = false;
            for (int i = 0; i < _heroSlots.Count; i++)
            {
                if (_heroSlots[i] != null && !_heroSlots[i].IsDead && _heroSlots[i].gameObject.activeSelf)
                {
                    anyHeroAlive = true;
                    break;
                }
            }

            if (!anyHeroAlive)
            {
                TriggerMatchEnd(false);
            }
        }

        private void CheckEndGameConditions()
        {
            if (_boss != null && _boss.IsDead)
            {
                TriggerMatchEnd(true);
            }
            else
            {
                HandleHeroDied();
            }
        }

        private void TriggerMatchEnd(bool isVictory)
        {
            if (_isMatchOver) return;

            _isMatchOver = true;
            if (_turnTimer != null)
            {
                _turnTimer.Stop();
            }

            if (isVictory)
            {
                Debug.Log("[TurnManager] ¡VICTORIA! El Boss fue derrotado.");
            }
            else
            {
                Debug.Log("[TurnManager] ¡DERROTA! Todos los héroes han caído.");
            }

            OnMatchFinished?.Invoke(isVictory);
        }
        #endregion
    }
}
