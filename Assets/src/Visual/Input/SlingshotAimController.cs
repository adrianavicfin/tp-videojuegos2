using System;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Controlador de puntería mediante arrastre inverso (Mecánica Slingshot / Resortera tipo Angry Birds).
    /// El jugador hace clic sobre el héroe activo, arrastra en dirección opuesta al tiro deseado para tensar potencia y ángulo,
    /// y emite eventos balísticos para el cálculo de trayectoria (H3-T3) y disparo (H3-T4).
    /// </summary>
    public class SlingshotAimController : MonoBehaviour
    {
        public static SlingshotAimController Instance { get; private set; }

        #region Events (Contratos para H3-T3 y H3-T4)
        /// <summary>
        /// Se dispara al iniciar el arrastre/tensado. Pasa la posición de origen del tiro.
        /// </summary>
        public event Action<Vector2> OnAimStarted;

        /// <summary>
        /// Se dispara continuamente en cada frame de arrastre.
        /// Parámetros: (Vector2 origin, Vector2 direction, float power, float normalizedPower)
        /// </summary>
        public event Action<Vector2, Vector2, float, float> OnAimUpdated;

        /// <summary>
        /// Se dispara al soltar el clic tras un arrastre válido.
        /// Parámetros: (Vector2 direction, float power)
        /// </summary>
        public event Action<Vector2, float> OnAimReleased;

        /// <summary>
        /// Se dispara si el jugador cancela el disparo (clic derecho, escape o arrastre inferior al umbral mínimo).
        /// </summary>
        public event Action OnAimCanceled;
        #endregion

        [Header("Aim Settings")]
        [Tooltip("Sensibilidad o multiplicador de potencia por distancia de arrastre en unidades del mundo.")]
        [SerializeField] private float _pullPowerMultiplier = 6f;

        [Tooltip("Distancia mínima de arrastre para considerar un tiro válido (evita micro-clics accidentales).")]
        [SerializeField] private float _minDragThreshold = 0.35f;

        [Tooltip("Si es true, permite iniciar el arrastre haciendo clic en cualquier parte de la pantalla mientras sea el turno del héroe.")]
        [SerializeField] private bool _allowClickAnywhere = true;

        [Tooltip("Distancia máxima de selección de clic alrededor del héroe para iniciar el tensado si _allowClickAnywhere es false.")]
        [SerializeField] private float _heroClickRadius = 3.5f;

        [Tooltip("Potencia máxima por defecto si el héroe no tiene arma equipada.")]
        [SerializeField] private float _defaultMaxPower = 25f;

        [Header("Camera & Layers")]
        [SerializeField] private Camera _mainCamera;

        // Estado interno
        private bool _isAiming = false;
        private Vector2 _aimOrigin;
        private Vector2 _currentLaunchDirection = Vector2.up;
        private float _currentPower = 0f;
        private float _maxAllowedPower = 25f;
        private Hero _currentActiveHero;

        #region Properties
        public bool IsAiming => _isAiming;
        public Vector2 AimOrigin => _aimOrigin;
        public Vector2 CurrentLaunchDirection => _currentLaunchDirection;
        public float CurrentPower => _currentPower;
        public float NormalizedPower => _maxAllowedPower > 0f ? Mathf.Clamp01(_currentPower / _maxAllowedPower) : 0f;
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

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstanceInScene()
        {
            if (Instance == null && UnityEngine.Object.FindObjectOfType<SlingshotAimController>() == null)
            {
                if (UnityEngine.Object.FindObjectOfType<TurnManager>() != null || UnityEngine.Object.FindObjectOfType<Hero>() != null)
                {
                    GameObject go = new GameObject("SlingshotAimSystem");
                    go.AddComponent<SlingshotAimController>();
                    go.AddComponent<JumpAimController>();
                    go.AddComponent<TrajectoryPredictor>();
                    Debug.Log("[Slingshot] AimSystem auto-creado y configurado en la escena de Gameplay.");
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
            if (!CanAim())
            {
                if (_isAiming)
                {
                    CancelAim();
                }
                return;
            }

            HandleAimInput();
        }

        /// <summary>
        /// Valida si el turno actual permite iniciar o mantener el modo de apuntado.
        /// </summary>
        private bool CanAim()
        {
            // Si se está apuntando el salto, no interferir con la resortera
            if (JumpAimController.Instance != null && JumpAimController.Instance.IsAimingJump)
            {
                return false;
            }

            if (TurnManager.Instance == null)
            {
                // Fallback directo por si se prueba sin TurnManager
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
                _maxAllowedPower = hero.EquippedWeapon != null ? hero.EquippedWeapon.MaxPower : _defaultMaxPower;
                return true;
            }

            _currentActiveHero = null;
            return false;
        }

        private void HandleAimInput()
        {
            Vector2 mouseWorldPos = GetMouseWorldPosition();

            // 1. Iniciar Apuntado (Clic Izquierdo presionado)
            if (Input.GetMouseButtonDown(0))
            {
                if (_currentActiveHero != null)
                {
                    Vector2 heroPos = _currentActiveHero.transform.position;
                    float distToHero = Vector2.Distance(mouseWorldPos, heroPos);

                    if (_allowClickAnywhere || distToHero <= _heroClickRadius)
                    {
                        Debug.Log($"[Slingshot] ¡Apuntado iniciado para {_currentActiveHero.CharacterName}! HeroPos: {heroPos}, MousePos: {mouseWorldPos}");
                        StartAim(heroPos);
                    }
                    else
                    {
                        Debug.Log($"[Slingshot] Clic fuera de rango. Distancia al héroe: {distToHero:F2} > Radio ({_heroClickRadius:F2})");
                    }
                }
            }

            // 2. Mantener y Actualizar Apuntado (Arrastre continuo)
            if (_isAiming && Input.GetMouseButton(0))
            {
                UpdateAim(mouseWorldPos);

                // Cancelación voluntaria mediante Clic Derecho o Tecla Escape
                if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
                {
                    CancelAim();
                    return;
                }
            }

            // 3. Soltar Disparo (Liberar Clic Izquierdo)
            if (_isAiming && Input.GetMouseButtonUp(0))
            {
                ReleaseAim();
            }
        }

        private void StartAim(Vector2 origin)
        {
            _isAiming = true;
            _aimOrigin = origin;
            _currentLaunchDirection = _currentActiveHero != null ? (Vector2)_currentActiveHero.transform.up : Vector2.up;
            _currentPower = 0f;

            Debug.Log($"[Slingshot] Tensado iniciado para {_currentActiveHero?.CharacterName} en origen {_aimOrigin}.");
            OnAimStarted?.Invoke(_aimOrigin);
        }

        private void UpdateAim(Vector2 mouseWorldPos)
        {
            if (_currentActiveHero != null)
            {
                _aimOrigin = _currentActiveHero.transform.position;
            }

            // Vector de arrastre: De la posición del mouse hacia el origen (dirección opuesta = slingshot)
            Vector2 dragVector = _aimOrigin - mouseWorldPos;
            float dragDistance = dragVector.magnitude;

            if (dragDistance > 0.05f)
            {
                _currentLaunchDirection = dragVector.normalized;
            }

            // Potencia escalada y clampada a la capacidad máxima del arma
            float rawPower = dragDistance * _pullPowerMultiplier;
            _currentPower = Mathf.Clamp(rawPower, 0f, _maxAllowedPower);

            OnAimUpdated?.Invoke(_aimOrigin, _currentLaunchDirection, _currentPower, NormalizedPower);
        }

        private void ReleaseAim()
        {
            _isAiming = false;

            // Si el arrastre fue insignificante, se considera cancelación
            if (_currentPower < (_minDragThreshold * _pullPowerMultiplier))
            {
                Debug.Log("[Slingshot] Arrastre por debajo del umbral mínimo. Disparo cancelado.");
                OnAimCanceled?.Invoke();
                return;
            }

            Debug.Log($"[Slingshot] ¡Disparo ejecutado! Dirección: {_currentLaunchDirection}, Potencia: {_currentPower:F1}/{_maxAllowedPower:F1}");
            OnAimReleased?.Invoke(_currentLaunchDirection, _currentPower);

            // Ejecutar el disparo mediante el Héroe activo (Patrón Command ActionShoot)
            if (_currentActiveHero != null && !_currentActiveHero.IsDead)
            {
                _currentActiveHero.ExecuteShoot(_currentLaunchDirection, _currentPower);
            }
        }

        public void CancelAim()
        {
            if (!_isAiming) return;

            _isAiming = false;
            _currentPower = 0f;
            Debug.Log("[Slingshot] Modo de apuntado cancelado.");
            OnAimCanceled?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_isAiming) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_aimOrigin, 0.3f);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(_aimOrigin, (Vector3)(_currentLaunchDirection * (_currentPower * 0.2f)));
        }
    }
}
