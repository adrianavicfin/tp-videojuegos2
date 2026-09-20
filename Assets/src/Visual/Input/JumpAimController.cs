using System;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Controlador de salto táctico direccional en 2 fases con Barra Espaciadora (H3-T6).
    /// - Fase 1: Presionar Espacio activa el modo de apuntado directo al cursor (parábola blanca).
    /// - Regulación: Con W/S o Flechas Arriba/Abajo se gradúa la potencia en 10 niveles (del 10% al 100%).
    /// - Fase 2: Presionar Espacio nuevamente ejecuta el impulso de salto físico mediante ActionJump.
    /// </summary>
    public class JumpAimController : MonoBehaviour
    {
        public static JumpAimController Instance { get; private set; }

        #region Events
        /// <summary>
        /// Se dispara al activar el modo de apuntado de salto. Pasa la posición del héroe.
        /// </summary>
        public event Action<Vector2> OnJumpAimStarted;

        /// <summary>
        /// Se dispara continuamente mientras se apunta el salto.
        /// Parámetros: (Vector2 origin, Vector2 direction, float force, int level, float gravityMultiplier)
        /// </summary>
        public event Action<Vector2, Vector2, float, int, float> OnJumpAimUpdated;

        /// <summary>
        /// Se dispara al confirmar y ejecutar el salto.
        /// Parámetros: (Vector2 direction, float force)
        /// </summary>
        public event Action<Vector2, float> OnJumpAimExecuted;

        /// <summary>
        /// Se dispara si el jugador cancela el salto (clic derecho o Escape).
        /// </summary>
        public event Action OnJumpAimCanceled;
        #endregion

        [Header("Jump Configuration")]
        [Tooltip("Multiplicador de impulso de salto base sobre el JumpForce del personaje.")]
        [SerializeField] private float _jumpForceMultiplier = 2.0f;

        [Tooltip("Nivel inicial de potencia (1 a 10).")]
        [Range(1, 10)]
        [SerializeField] private int _defaultPowerLevel = 10;

        [Header("Camera")]
        [SerializeField] private Camera _mainCamera;

        // Estado interno
        private bool _isAimingJump = false;
        private int _currentPowerLevel = 10; // 1 = 10%, 10 = 100%
        private Vector2 _currentDirection = Vector2.up;
        private Hero _currentActiveHero;

        #region Properties
        public bool IsAimingJump => _isAimingJump;
        public int CurrentPowerLevel => _currentPowerLevel;
        public float PowerPercentage => _currentPowerLevel * 0.10f; // 0.1 a 1.0
        public Vector2 CurrentDirection => _currentDirection;
        public Hero CurrentHero => _currentActiveHero;
        #endregion

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            _currentPowerLevel = Mathf.Clamp(_defaultPowerLevel, 1, 10);

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstanceInScene()
        {
            if (Instance == null && UnityEngine.Object.FindObjectOfType<JumpAimController>() == null)
            {
                if (UnityEngine.Object.FindObjectOfType<TurnManager>() != null || UnityEngine.Object.FindObjectOfType<Hero>() != null)
                {
                    GameObject go = GameObject.Find("SlingshotAimSystem");
                    if (go == null)
                    {
                        go = new GameObject("SlingshotAimSystem");
                    }
                    if (go.GetComponent<JumpAimController>() == null)
                    {
                        go.AddComponent<JumpAimController>();
                    }
                    Debug.Log("[JumpAim] JumpAimController auto-configurado en la escena.");
                }
            }
        }

        private Vector2 GetMouseWorldPosition()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null) return Vector2.zero;
            }

            Vector3 mouseScreen = Input.mousePosition;
            mouseScreen.z = Mathf.Abs(_mainCamera.transform.position.z);
            Vector3 worldPos3D = _mainCamera.ScreenToWorldPoint(mouseScreen);
            return new Vector2(worldPos3D.x, worldPos3D.y);
        }

        private void Update()
        {
            if (!CanControlJump())
            {
                if (_isAimingJump)
                {
                    CancelJumpAim();
                }
                return;
            }

            HandleJumpInput();
        }

        /// <summary>
        /// Valida si el turno actual y el estado del héroe permiten apuntar o ejecutar salto.
        /// </summary>
        private bool CanControlJump()
        {
            // Si la resortera está activa, no interferir
            if (SlingshotAimController.Instance != null && SlingshotAimController.Instance.IsAiming)
            {
                return false;
            }

            if (TurnManager.Instance == null)
            {
                if (_currentActiveHero == null)
                {
                    _currentActiveHero = UnityEngine.Object.FindObjectOfType<Hero>();
                }
                return _currentActiveHero != null && !_currentActiveHero.IsDead;
            }

            if (TurnManager.Instance.CurrentPhase != TurnPhase.WaitingInput) return false;

            Character activeChar = TurnManager.Instance.ActiveCharacter;
            if (activeChar is Hero hero && !hero.IsDead)
            {
                _currentActiveHero = hero;
                return true;
            }

            _currentActiveHero = null;
            return false;
        }

        private void HandleJumpInput()
        {
            // 1. Alternar entre Fase 1 (Iniciar apuntado) y Fase 2 (Ejecutar salto) con Barra Espaciadora
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!_isAimingJump)
                {
                    StartJumpAim();
                }
                else
                {
                    ExecuteJump();
                    return;
                }
            }

            if (!_isAimingJump) return;

            // 2. Cancelación (Clic Derecho o Tecla Escape)
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelJumpAim();
                return;
            }

            // 3. Regulación de fuerza en 10 niveles (W / S o Flechas Arriba / Abajo)
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                _currentPowerLevel = Mathf.Min(10, _currentPowerLevel + 1);
                Debug.Log($"[JumpAim] Potencia de salto aumentada a {_currentPowerLevel * 10}% (Nivel {_currentPowerLevel}/10)");
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                _currentPowerLevel = Mathf.Max(1, _currentPowerLevel - 1);
                Debug.Log($"[JumpAim] Potencia de salto reducida a {_currentPowerLevel * 10}% (Nivel {_currentPowerLevel}/10)");
            }

            // 4. Actualizar dirección orientada directamente hacia el cursor del mouse
            Vector2 mouseWorldPos = GetMouseWorldPosition();
            Vector2 heroPos = _currentActiveHero != null ? (Vector2)_currentActiveHero.transform.position : Vector2.zero;
            Vector2 aimVector = mouseWorldPos - heroPos;

            if (aimVector.sqrMagnitude > 0.01f)
            {
                _currentDirection = aimVector.normalized;
            }

            float baseJumpForce = _currentActiveHero != null ? _currentActiveHero.JumpForce : 7f;
            float gravityMultiplier = _currentActiveHero != null ? _currentActiveHero.GravityMultiplier : 1.0f;
            float finalForce = baseJumpForce * _jumpForceMultiplier * PowerPercentage;

            if (TrajectoryPredictor.Instance != null)
            {
                TrajectoryPredictor.Instance.SetTrajectoryColor(Color.white);
                TrajectoryPredictor.Instance.PredictTrajectory(heroPos, _currentDirection, finalForce, gravityMultiplier);
            }

            OnJumpAimUpdated?.Invoke(heroPos, _currentDirection, finalForce, _currentPowerLevel, gravityMultiplier);
        }

        private void StartJumpAim()
        {
            if (_currentActiveHero == null) return;

            _isAimingJump = true;
            Vector2 heroPos = _currentActiveHero.transform.position;
            Vector2 mouseWorldPos = GetMouseWorldPosition();
            Vector2 aimVector = mouseWorldPos - heroPos;

            _currentDirection = aimVector.sqrMagnitude > 0.01f ? aimVector.normalized : (Vector2)_currentActiveHero.transform.up;

            float baseJumpForce = _currentActiveHero.JumpForce;
            float gravityMultiplier = _currentActiveHero.GravityMultiplier;
            float finalForce = baseJumpForce * _jumpForceMultiplier * PowerPercentage;

            if (TrajectoryPredictor.Instance != null)
            {
                TrajectoryPredictor.Instance.SetTrajectoryColor(Color.white);
                TrajectoryPredictor.Instance.PredictTrajectory(heroPos, _currentDirection, finalForce, gravityMultiplier);
            }

            Debug.Log($"[JumpAim] Modo de salto iniciado para {_currentActiveHero.CharacterName}. Nivel: {_currentPowerLevel * 10}%");
            OnJumpAimStarted?.Invoke(heroPos);
        }

        private void ExecuteJump()
        {
            _isAimingJump = false;

            TrajectoryPredictor.Instance?.HideTrajectory();

            if (_currentActiveHero == null || _currentActiveHero.IsDead)
            {
                CancelJumpAim();
                return;
            }

            float baseJumpForce = _currentActiveHero.JumpForce;
            float finalForce = baseJumpForce * _jumpForceMultiplier * PowerPercentage;

            Debug.Log($"[JumpAim] ¡Salto confirmado! Dirección: {_currentDirection}, Fuerza: {finalForce:F2} ({_currentPowerLevel * 10}%)");

            OnJumpAimExecuted?.Invoke(_currentDirection, finalForce);

            // Ejecutar el comando ActionJump sobre el héroe activo
            _currentActiveHero.ExecuteJump(_currentDirection, finalForce);
        }

        public void CancelJumpAim()
        {
            if (!_isAimingJump) return;

            _isAimingJump = false;
            TrajectoryPredictor.Instance?.HideTrajectory();
            Debug.Log("[JumpAim] Apuntado de salto cancelado.");
            OnJumpAimCanceled?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_isAimingJump || _currentActiveHero == null) return;

            Gizmos.color = Color.white;
            Vector2 heroPos = _currentActiveHero.transform.position;
            Gizmos.DrawRay(heroPos, (Vector3)(_currentDirection * 3f));
        }
    }
}
