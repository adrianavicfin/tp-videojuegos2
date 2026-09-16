using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Renderizador y predictor numérico de trayectorias balísticas curvas bajo campos de gravedad radial multicuerpo.
    /// Se conecta a los eventos de SlingshotAimController para proyectar en tiempo real la trayectoria antes del disparo (H3-T3).
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryPredictor : MonoBehaviour
    {
        public static TrajectoryPredictor Instance { get; private set; }

        [Header("Simulation Parameters")]
        [Tooltip("Cantidad máxima de pasos o muestras calculadas en la parábola.")]
        [SerializeField] private int _maxSimulationSteps = 45;

        [Tooltip("Paso de integración temporal (delta time simulado por muestra).")]
        [SerializeField] private float _timeStep = 0.05f;

        [Tooltip("Masa simulada del proyectil.")]
        [SerializeField] private float _simulatedMass = 1.0f;

        [Tooltip("Capas de colisión que detienen el trazado de la trayectoria (Terreno, Coberturas, etc.).")]
        [SerializeField] private LayerMask _collisionMask = ~0;

        [Header("Line Visuals")]
        [SerializeField] private float _startWidth = 0.15f;
        [SerializeField] private float _endWidth = 0.02f;
        [SerializeField] private Color _trajectoryColor = new Color(0.2f, 0.85f, 1f, 0.85f);

        private LineRenderer _lineRenderer;
        private Vector3[] _simulationPoints;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _lineRenderer = GetComponent<LineRenderer>();
            _simulationPoints = new Vector3[_maxSimulationSteps];

            ConfigureLineRenderer();
            HideTrajectory();
        }

        private void Start()
        {
            SubscribeToAimController();
        }

        private void OnDestroy()
        {
            UnsubscribeFromAimController();
        }

        private void ConfigureLineRenderer()
        {
            if (_lineRenderer == null) return;

            _lineRenderer.useWorldSpace = true;
            _lineRenderer.startWidth = _startWidth;
            _lineRenderer.endWidth = _endWidth;
            _lineRenderer.positionCount = 0;

            // Configurar gradiente estético suave
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(_trajectoryColor, 0f), new GradientColorKey(_trajectoryColor, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0.1f, 1f) }
            );
            _lineRenderer.colorGradient = gradient;
        }

        private void SubscribeToAimController()
        {
            if (SlingshotAimController.Instance != null)
            {
                SlingshotAimController.Instance.OnAimStarted += HandleAimStarted;
                SlingshotAimController.Instance.OnAimUpdated += HandleAimUpdated;
                SlingshotAimController.Instance.OnAimReleased += HandleAimReleased;
                SlingshotAimController.Instance.OnAimCanceled += HandleAimCanceled;
            }
        }

        private void UnsubscribeFromAimController()
        {
            if (SlingshotAimController.Instance != null)
            {
                SlingshotAimController.Instance.OnAimStarted -= HandleAimStarted;
                SlingshotAimController.Instance.OnAimUpdated -= HandleAimUpdated;
                SlingshotAimController.Instance.OnAimReleased -= HandleAimReleased;
                SlingshotAimController.Instance.OnAimCanceled -= HandleAimCanceled;
            }
        }

        #region Event Handlers
        private void HandleAimStarted(Vector2 origin)
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = true;
                _lineRenderer.positionCount = 0;
            }
        }

        private void HandleAimUpdated(Vector2 origin, Vector2 direction, float power, float normalizedPower)
        {
            if (_lineRenderer == null) return;

            _lineRenderer.enabled = true;
            PredictTrajectory(origin, direction, power);
        }

        private void HandleAimReleased(Vector2 direction, float power)
        {
            HideTrajectory();
        }

        private void HandleAimCanceled()
        {
            HideTrajectory();
        }
        #endregion

        /// <summary>
        /// Simulación numérica física paso a paso considerando campos gravitatorios radiales (Euler Integration).
        /// </summary>
        public void PredictTrajectory(Vector2 startPos, Vector2 direction, float launchPower)
        {
            if (_lineRenderer == null) return;

            Vector2 currentPos = startPos + (direction.normalized * 1.2f);
            Vector2 velocity = direction.normalized * launchPower;

            int validPointCount = 0;
            _simulationPoints[validPointCount++] = currentPos;

            for (int i = 1; i < _maxSimulationSteps; i++)
            {
                // 1. Calcular gravedad acumulada en el punto actual
                Vector2 gravityForce = GravityBody.GetTotalGravitationalPull(currentPos);
                Vector2 acceleration = gravityForce / _simulatedMass;

                // 2. Integración de velocidad y posición
                velocity += acceleration * _timeStep;
                Vector2 nextPos = currentPos + (velocity * _timeStep);

                // 3. Chequeo de colisión en el trayecto (Raycast entre muestras)
                Vector2 stepDir = nextPos - currentPos;
                float stepDist = stepDir.magnitude;

                if (stepDist > 0.001f)
                {
                    RaycastHit2D hit = Physics2D.Raycast(currentPos, stepDir.normalized, stepDist, _collisionMask);
                    if (hit.collider != null)
                    {
                        // Si choca con un obstáculo o planeta, terminar trayectoria en el punto de impacto
                        _simulationPoints[validPointCount++] = hit.point;
                        break;
                    }
                }

                _simulationPoints[validPointCount++] = nextPos;
                currentPos = nextPos;
            }

            // Aplicar puntos al LineRenderer sin re-alocaciones
            _lineRenderer.positionCount = validPointCount;
            for (int i = 0; i < validPointCount; i++)
            {
                _lineRenderer.SetPosition(i, _simulationPoints[i]);
            }
        }

        public void HideTrajectory()
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = 0;
                _lineRenderer.enabled = false;
            }
        }
    }
}
