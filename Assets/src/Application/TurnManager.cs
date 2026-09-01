using System;
using System.Collections.Generic;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Orquestador del flujo de combate por turnos y árbitro de la partida.
    /// Gestiona la cola polimórfica de Characters (Héroes, Boss, Esbirros), el countdown y las fases de turno.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        #region Events
        public event Action<Character> OnTurnStarted;
        public event Action<Character> OnTurnEnded;
        public event Action<TurnPhase> OnPhaseChanged;
        public event Action<float> OnCountdownTick; // Tiempo restante en segundos
        public event Action<int> OnRoundChanged;     // Número de ronda global
        public event Action<bool> OnMatchFinished;  // true = Victoria de Héroes, false = Derrota
        #endregion

        [Header("Turn Settings")]
        [SerializeField] private float _turnDuration = 30f;
        [SerializeField] private List<Hero> _heroSlots = new List<Hero>();
        [SerializeField] private Boss _boss;
        [SerializeField] private List<Minion> _minions = new List<Minion>();

        // Inyectado vía IoC
        private ICountdownTimer _turnTimer;

        #region Internal State
        private readonly Queue<Character> _turnQueue = new Queue<Character>();
        private readonly List<Character> _allCombatants = new List<Character>();

        private Character _activeCharacter;
        private TurnPhase _currentPhase = TurnPhase.WaitingInput;
        private int _currentRound = 1;
        private bool _isMatchOver;
        #endregion

        #region Properties
        public Character ActiveCharacter => _activeCharacter;
        public TurnPhase CurrentPhase => _currentPhase;
        public int CurrentRound => _currentRound;
        public float RemainingTime => _turnTimer?.RemainingTime ?? 0f;
        public bool IsMatchOver => _isMatchOver;
        #endregion

        #region Unity Lifecycle & Inversion of Control
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Inyección automática de dependencias desde el IoC
                IoCContainer.Instance.Inject(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Inyección de dependencias pasiva mediante IoC (Construct).
        /// </summary>
        public void Construct(ICountdownTimer turnTimer)
        {
            _turnTimer = turnTimer;
            _turnTimer.OnTick += (remaining) => OnCountdownTick?.Invoke(remaining);
            _turnTimer.OnFinished += EndCurrentTurn;
        }

        private void Start()
        {
            InitializeCombatants();
            StartNewRound();
        }

        private void Update()
        {
            if (_isMatchOver || _turnTimer == null) return;

            if (_currentPhase == TurnPhase.WaitingInput)
            {
                _turnTimer.Tick(Time.deltaTime);
            }
        }
        #endregion

        #region Match & Queue Setup
        private void InitializeCombatants()
        {
            _allCombatants.Clear();

            // 1. Configurar Héroes según los slots activos
            foreach (var hero in _heroSlots)
            {
                if (hero != null && hero.gameObject.activeSelf && !hero.IsDead)
                {
                    _allCombatants.Add(hero);
                    hero.OnDied += () => HandleCharacterDied(hero);
                }
            }

            // 2. Configurar Boss
            if (_boss != null && !_boss.IsDead)
            {
                _allCombatants.Add(_boss);
                _boss.OnDied += () => HandleCharacterDied(_boss);
            }

            // 3. Configurar Esbirros
            foreach (var minion in _minions)
            {
                if (minion != null && minion.gameObject.activeSelf && !minion.IsDead)
                {
                    _allCombatants.Add(minion);
                    minion.OnDied += () => HandleCharacterDied(minion);
                }
            }
        }

        private void StartNewRound()
        {
            if (_isMatchOver) return;

            Debug.Log($"[TurnManager] === Iniciando Ronda {_currentRound} ===");
            _turnQueue.Clear();

            foreach (var combatant in _allCombatants)
            {
                if (combatant != null && !combatant.IsDead)
                {
                    _turnQueue.Enqueue(combatant);
                }
            }

            OnRoundChanged?.Invoke(_currentRound);
            AdvanceToNextTurn();
        }
        #endregion

        #region Turn Flow Execution
        public void AdvanceToNextTurn()
        {
            if (_isMatchOver) return;

            if (_turnQueue.Count == 0)
            {
                _currentRound++;
                StartNewRound();
                return;
            }

            _activeCharacter = _turnQueue.Dequeue();

            if (_activeCharacter == null || _activeCharacter.IsDead)
            {
                AdvanceToNextTurn();
                return;
            }

            SetPhase(TurnPhase.WaitingInput);
            _turnTimer.Start(_turnDuration);

            _activeCharacter.StartTurn();
            OnTurnStarted?.Invoke(_activeCharacter);
        }

        public void NotifyActionExecuting()
        {
            _turnTimer.Pause();
            SetPhase(TurnPhase.ActionExecuting);
        }

        public void NotifyActionResolved()
        {
            SetPhase(TurnPhase.Resolving);
            CheckMatchEndConditions();

            if (!_isMatchOver)
            {
                EndCurrentTurn();
            }
        }

        public void EndCurrentTurn()
        {
            _turnTimer.Stop();

            if (_activeCharacter != null)
            {
                _activeCharacter.EndTurn();
                OnTurnEnded?.Invoke(_activeCharacter);
            }

            AdvanceToNextTurn();
        }

        private void SetPhase(TurnPhase phase)
        {
            _currentPhase = phase;
            OnPhaseChanged?.Invoke(_currentPhase);
        }
        #endregion

        #region Victory / Defeat Conditions
        private void HandleCharacterDied(Character character)
        {
            Debug.Log($"[TurnManager] El combatiente {character.CharacterName} ha caído.");
            CheckMatchEndConditions();
        }

        private void CheckMatchEndConditions()
        {
            if (_isMatchOver) return;

            // 1. Chequear si el Boss murió (Victoria)
            if (_boss == null || _boss.IsDead)
            {
                FinishMatch(victory: true);
                return;
            }

            // 2. Chequear si todos los Héroes murieron (Derrota)
            bool anyHeroAlive = false;
            foreach (var hero in _heroSlots)
            {
                if (hero != null && !hero.IsDead && hero.gameObject.activeSelf)
                {
                    anyHeroAlive = true;
                    break;
                }
            }

            if (!anyHeroAlive)
            {
                FinishMatch(victory: false);
            }
        }

        private void FinishMatch(bool victory)
        {
            _isMatchOver = true;
            _turnTimer?.Stop();
            Debug.Log(victory ? "[TurnManager] ¡VICTORIA! El Boss fue derrotado." : "[TurnManager] ¡DERROTA! Todos los héroes han caído.");
            OnMatchFinished?.Invoke(victory);
        }
        #endregion
    }
}
