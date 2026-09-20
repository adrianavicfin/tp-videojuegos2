using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Controlador de desplazamiento físico orbital sobre superficies planetarias curvas (H3-T5).
    /// Proyecta el input horizontal (A/D o flechas) perpendicular a la gravedad radial (tangente a la superficie)
    /// y alinea automáticamente al personaje con la normal del terreno en FixedUpdate.
    /// </summary>
    [RequireComponent(typeof(Character), typeof(Rigidbody2D), typeof(Collider2D))]
    public class CharacterMovementController : MonoBehaviour
    {
        [Header("Movement Configuration")]
        [Tooltip("Velocidad de aceleración y respuesta sobre la tangente.")]
        [SerializeField] private float _moveSpeed = 6f;

        [Tooltip("Factor de control aéreo mientras está en el aire (0 = sin control, 1 = control total).")]
        [SerializeField] private float _airControlMultiplier = 0.2f;

        [Tooltip("Capas consideradas suelo/terreno planetario.")]
        [SerializeField] private LayerMask _groundLayers = ~0;

        [Tooltip("Distancia del Raycast hacia los pies para detectar suelo.")]
        [SerializeField] private float _groundCheckDistance = 1.3f;

        [Header("Jump Configuration")]
        [Tooltip("Multiplicador de fuerza de salto.")]
        [SerializeField] private float _jumpForceMultiplier = 2.0f;

        private Character _character;
        private Rigidbody2D _rb;
        private Collider2D _ownCollider;
        private SpriteRenderer _spriteRenderer;

        private float _horizontalInput = 0f;
        private bool _isGrounded = true;
        private float _jumpCooldownTimer = 0f;
        private Vector2 _surfaceNormal = Vector2.up;
        private Vector2 _surfaceTangent = Vector2.right;

        // Estado interno de salto táctico en 2 fases
        private bool _isAimingJump = false;
        private int _jumpPowerLevel = 10; // 1 = 10%, 10 = 100%
        private Vector2 _currentJumpDirection = Vector2.up;

        private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[8];

        public bool IsGrounded => _isGrounded;
        public bool IsAimingJump => _isAimingJump;
        public int JumpPowerLevel => _jumpPowerLevel;
        public Vector2 SurfaceNormal => _surfaceNormal;
        public Vector2 SurfaceTangent => _surfaceTangent;

        /// <summary>
        /// Aplica un impulso de salto deshabilitando temporalmente el chequeo de suelo para permitir el despegue físico.
        /// </summary>
        public void LaunchJump(Vector2 direction, float force)
        {
            _isGrounded = false;
            _jumpCooldownTimer = 0.35f;
            if (_rb != null)
            {
                _rb.velocity = direction.normalized * force;
            }
        }

        private void Awake()
        {
            _character = GetComponent<Character>();
            _rb = GetComponent<Rigidbody2D>();
            _ownCollider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (_rb != null)
            {
                _rb.freezeRotation = true;
            }

            if (_character != null && _character.MoveSpeed > 0f)
            {
                _moveSpeed = _character.MoveSpeed;
            }
        }

        private void Update()
        {
            if (!CanMove())
            {
                if (_isAimingJump)
                {
                    CancelJumpAim();
                }
                _horizontalInput = 0f;
                return;
            }

            // 1. Manejo de Salto Direccional en 2 Fases (Barra Espaciadora)
            HandleJumpInput();

            // 2. Si está en modo apuntado de salto, no caminar
            if (_isAimingJump)
            {
                _horizontalInput = 0f;
                return;
            }

            // 3. Capturar entrada horizontal del jugador (A/D o Flechas)
            _horizontalInput = Input.GetAxisRaw("Horizontal");

            // Voltear el sprite según la dirección de marcha relativa
            if (_spriteRenderer != null && Mathf.Abs(_horizontalInput) > 0.05f)
            {
                _spriteRenderer.flipX = _horizontalInput < 0f;
            }
        }

        private void HandleJumpInput()
        {
            // Fase 1 / Fase 2 con Barra Espaciadora
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

            // Cancelar con Clic Derecho o Escape
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelJumpAim();
                return;
            }

            // Regular potencia con W/S o Flechas Arriba/Abajo (de 1 a 10 = 10% a 100%)
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                _jumpPowerLevel = Mathf.Min(10, _jumpPowerLevel + 1);
                Debug.Log($"[CharacterMovementController] Potencia de salto: {_jumpPowerLevel * 10}%");
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                _jumpPowerLevel = Mathf.Max(1, _jumpPowerLevel - 1);
                Debug.Log($"[CharacterMovementController] Potencia de salto: {_jumpPowerLevel * 10}%");
            }

            // Actualizar vector de puntería hacia el cursor del mouse
            Vector2 mouseWorld = GetMouseWorldPosition();
            Vector2 heroPos = (Vector2)transform.position;
            Vector2 aimDir = mouseWorld - heroPos;
            if (aimDir.sqrMagnitude > 0.01f)
            {
                _currentJumpDirection = aimDir.normalized;
            }

            float baseJump = _character != null ? _character.JumpForce : 7f;
            float gravMult = _character != null ? _character.GravityMultiplier : 1.0f;
            float finalForce = baseJump * _jumpForceMultiplier * (_jumpPowerLevel * 0.10f);

            // Renderizar la parábola blanca en tiempo real
            if (TrajectoryPredictor.Instance != null)
            {
                TrajectoryPredictor.Instance.SetTrajectoryColor(Color.white);
                TrajectoryPredictor.Instance.PredictTrajectory(heroPos, _currentJumpDirection, finalForce, gravMult);
            }
        }

        private void StartJumpAim()
        {
            _isAimingJump = true;
            Vector2 mouseWorld = GetMouseWorldPosition();
            Vector2 heroPos = (Vector2)transform.position;
            Vector2 aimDir = mouseWorld - heroPos;
            _currentJumpDirection = aimDir.sqrMagnitude > 0.01f ? aimDir.normalized : (Vector2)transform.up;

            float baseJump = _character != null ? _character.JumpForce : 7f;
            float gravMult = _character != null ? _character.GravityMultiplier : 1.0f;
            float finalForce = baseJump * _jumpForceMultiplier * (_jumpPowerLevel * 0.10f);

            if (TrajectoryPredictor.Instance != null)
            {
                TrajectoryPredictor.Instance.SetTrajectoryColor(Color.white);
                TrajectoryPredictor.Instance.PredictTrajectory(heroPos, _currentJumpDirection, finalForce, gravMult);
            }

            Debug.Log($"[CharacterMovementController] Modo de salto iniciado para {_character?.CharacterName}. Potencia: {_jumpPowerLevel * 10}%");
        }

        private void ExecuteJump()
        {
            _isAimingJump = false;
            TrajectoryPredictor.Instance?.HideTrajectory();

            float baseJump = _character != null ? _character.JumpForce : 7f;
            float finalForce = baseJump * _jumpForceMultiplier * (_jumpPowerLevel * 0.10f);

            Debug.Log($"[CharacterMovementController] ¡Salto confirmado! Dirección: {_currentJumpDirection}, Fuerza: {finalForce:F2}");

            if (_character is Hero hero)
            {
                hero.ExecuteJump(_currentJumpDirection, finalForce);
            }
            else
            {
                LaunchJump(_currentJumpDirection, finalForce);
            }
        }

        public void CancelJumpAim()
        {
            if (!_isAimingJump) return;
            _isAimingJump = false;
            TrajectoryPredictor.Instance?.HideTrajectory();
            Debug.Log("[CharacterMovementController] Apuntado de salto cancelado.");
        }

        private Vector2 GetMouseWorldPosition()
        {
            Camera cam = Camera.main;
            if (cam == null) return Vector2.zero;

            Vector3 mouseScreen = Input.mousePosition;
            mouseScreen.z = Mathf.Abs(cam.transform.position.z);
            Vector3 worldPos3D = cam.ScreenToWorldPoint(mouseScreen);
            return new Vector2(worldPos3D.x, worldPos3D.y);
        }

        private void FixedUpdate()
        {
            UpdateSurfaceVectors();
            CheckGrounded();
            ApplyTangentLocomotion();
            AlignRotationWithSurface();
        }

        /// <summary>
        /// Valida si el personaje tiene permiso de moverse en este momento.
        /// </summary>
        private bool CanMove()
        {
            if (_character == null || _character.IsDead) return false;

            // Si se está apuntando con la resortera, congelar la caminata para apuntar con precisión
            if (SlingshotAimController.Instance != null && SlingshotAimController.Instance.IsAiming)
            {
                return false;
            }

            // Si hay TurnManager, solo se mueve el personaje que tiene el turno activo en WaitingInput
            if (TurnManager.Instance != null)
            {
                if (TurnManager.Instance.CurrentPhase != TurnPhase.WaitingInput) return false;
                if (TurnManager.Instance.ActiveCharacter != _character) return false;
            }

            return true;
        }

        /// <summary>
        /// Calcula la normal de la superficie (vector hacia afuera del planeta) y la tangente perpendicular.
        /// </summary>
        private void UpdateSurfaceVectors()
        {
            Vector2 gravity = GravityBody.GetTotalGravitationalPull(transform.position);

            if (gravity.sqrMagnitude > 0.01f)
            {
                // La gravedad tira hacia el centro del planeta, la normal de la superficie apunta en sentido contrario
                _surfaceNormal = -gravity.normalized;
            }
            else
            {
                _surfaceNormal = Vector2.up;
            }

            // Tangente a la superficie (perpendicular 2D: rotada 90° horario)
            _surfaceTangent = new Vector2(_surfaceNormal.y, -_surfaceNormal.x);
        }

        /// <summary>
        /// Detección física de contacto con la superficie planetaria (Zero-Alloc, ignora colisionadores propios).
        /// </summary>
        private void CheckGrounded()
        {
            if (_jumpCooldownTimer > 0f)
            {
                _jumpCooldownTimer -= Time.fixedDeltaTime;
                _isGrounded = false;
                return;
            }

            Vector2 feetDirection = -_surfaceNormal;
            int count = Physics2D.RaycastNonAlloc(transform.position, feetDirection, _groundHits, _groundCheckDistance, _groundLayers);

            _isGrounded = false;
            for (int i = 0; i < count; i++)
            {
                Collider2D col = _groundHits[i].collider;
                if (col == null || col == _ownCollider || col.isTrigger || col.gameObject == gameObject)
                {
                    continue;
                }

                _isGrounded = true;
                break;
            }
        }

        /// <summary>
        /// Aplica la fuerza/velocidad tangencial en la superficie curva del planeta.
        /// </summary>
        private void ApplyTangentLocomotion()
        {
            if (_rb == null || _rb.isKinematic) return;

            float speed = _character != null ? _character.MoveSpeed : _moveSpeed;
            float targetTangentSpeed = _horizontalInput * speed;

            // Proyectar la velocidad actual sobre los ejes Normal y Tangente
            float currentTangentSpeed = Vector2.Dot(_rb.velocity, _surfaceTangent);
            float currentNormalSpeed = Vector2.Dot(_rb.velocity, _surfaceNormal);

            if (_isGrounded)
            {
                if (Mathf.Abs(_horizontalInput) > 0.05f)
                {
                    // Desplazamiento tangencial directo sobre la superficie
                    _rb.velocity = (_surfaceTangent * targetTangentSpeed) + (_surfaceNormal * currentNormalSpeed);
                }
                else
                {
                    // Fricción y frenado suave en reposo
                    float dampedTangentSpeed = Mathf.MoveTowards(currentTangentSpeed, 0f, 25f * Time.fixedDeltaTime);
                    _rb.velocity = (_surfaceTangent * dampedTangentSpeed) + (_surfaceNormal * currentNormalSpeed);
                }
            }
            else
            {
                if (Mathf.Abs(_horizontalInput) > 0.05f)
                {
                    // Control aéreo leve
                    _rb.AddForce(_surfaceTangent * (_horizontalInput * speed * 10f * _airControlMultiplier), ForceMode2D.Force);
                }
            }
        }

        /// <summary>
        /// Rota al personaje perpendicularmente a la superficie planetaria.
        /// </summary>
        private void AlignRotationWithSurface()
        {
            if (_character != null)
            {
                _character.AlignWithSurface(_surfaceNormal);
            }
            else if (_rb != null)
            {
                float targetAngle = Mathf.Atan2(_surfaceNormal.y, _surfaceNormal.x) * Mathf.Rad2Deg - 90f;
                _rb.rotation = targetAngle;
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Vector Normal (Hacia arriba de la superficie)
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, (Vector3)_surfaceNormal * 2f);

            // Vector Tangente (Hacia la derecha de la superficie)
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, (Vector3)_surfaceTangent * 2f);

            // Raycast de suelo
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawRay(transform.position, (Vector3)(-_surfaceNormal * _groundCheckDistance));
        }
    }
}
