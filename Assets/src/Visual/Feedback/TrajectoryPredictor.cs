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
        [SerializeField] private int _maxSimulationSteps = 50;

        [Tooltip("Paso de integración temporal (delta time simulado por muestra).")]
        [SerializeField] private float _timeStep = 0.05f;

        [Tooltip("Masa simulada del proyectil.")]
        [SerializeField] private float _simulatedMass = 1.0f;

        [Tooltip("Capas de colisión que detienen el trazado de la trayectoria (Terreno, Coberturas, etc.).")]
        [SerializeField] private LayerMask _collisionMask = ~0;

        [Header("Line & Dot Visuals")]
        [SerializeField] private float _startWidth = 0.6f;
        [SerializeField] private float _endWidth = 0.2f;
        [SerializeField] private Color _trajectoryColor = new Color(0.2f, 0.9f, 1f, 1f);
        [SerializeField] private bool _useTrajectoryDots = true;
        [SerializeField] private int _dotCount = 25;
        [SerializeField] private float _dotScale = 0.35f;

        private LineRenderer _lineRenderer;
        private Vector3[] _simulationPoints;
        private GameObject[] _dotPool;
        private SpriteRenderer[] _dotRenderers;
        private bool _isSubscribedSlingshot = false;
        private bool _isSubscribedJumpAim = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

            _lineRenderer = GetComponent<LineRenderer>();
            if (_lineRenderer == null)
            {
                _lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            _simulationPoints = new Vector3[_maxSimulationSteps];

            ConfigureLineRenderer();
            CreateDotPool();
            HideTrajectory();
        }

        private void Start()
        {
            ConfigureLineRenderer();
            SubscribeToAimControllers();
        }

        private void Update()
        {
            if (!_isSubscribedSlingshot && SlingshotAimController.Instance != null)
            {
                SubscribeToSlingshot();
            }
            if (!_isSubscribedJumpAim && JumpAimController.Instance != null)
            {
                SubscribeToJumpAim();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromAimControllers();
        }

        private void CreateDotPool()
        {
            if (!_useTrajectoryDots) return;

            _dotPool = new GameObject[_dotCount];
            _dotRenderers = new SpriteRenderer[_dotCount];

            // Crear un sprite circular blanco procedimental
            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] colors = new Color[32 * 32];
            Vector2 center = new Vector2(15.5f, 15.5f);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    colors[y * 32 + x] = dist <= 14f ? Color.white : Color.clear;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            Sprite dotSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);

            Material dotMat = null;
            SpriteRenderer anySprite = UnityEngine.Object.FindObjectOfType<SpriteRenderer>();
            if (anySprite != null && anySprite.sharedMaterial != null)
            {
                dotMat = anySprite.sharedMaterial;
            }

            for (int i = 0; i < _dotCount; i++)
            {
                GameObject dot = new GameObject($"TrajectoryDot_{i}");
                dot.transform.SetParent(transform);
                SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
                sr.sprite = dotSprite;
                if (dotMat != null) sr.material = dotMat;
                sr.color = _trajectoryColor;
                sr.sortingOrder = 600; // Por encima de todo
                dot.transform.localScale = Vector3.one * _dotScale;
                dot.SetActive(false);

                _dotPool[i] = dot;
                _dotRenderers[i] = sr;
            }
        }

        private void ConfigureLineRenderer()
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
                if (_lineRenderer == null) return;
            }

            _lineRenderer.useWorldSpace = true;
            _lineRenderer.alignment = LineAlignment.View;
            _lineRenderer.startWidth = _startWidth;
            _lineRenderer.endWidth = _endWidth;
            _lineRenderer.sortingOrder = 550;
            _lineRenderer.positionCount = 0;

            SpriteRenderer anySprite = UnityEngine.Object.FindObjectOfType<SpriteRenderer>();
            if (anySprite != null && anySprite.sharedMaterial != null)
            {
                _lineRenderer.material = new Material(anySprite.sharedMaterial);
            }
            else
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") 
                             ?? Shader.Find("Sprites/Default") 
                             ?? Shader.Find("Unlit/Color");

                if (shader != null)
                {
                    _lineRenderer.material = new Material(shader);
                }
            }

            SetTrajectoryColor(_trajectoryColor);
        }

        public void SetTrajectoryColor(Color color)
        {
            if (_lineRenderer != null)
            {
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.3f, 1f) }
                );
                _lineRenderer.colorGradient = gradient;
            }

            if (_dotRenderers != null)
            {
                for (int i = 0; i < _dotRenderers.Length; i++)
                {
                    if (_dotRenderers[i] != null)
                    {
                        _dotRenderers[i].color = color;
                    }
                }
            }
        }

        private void SubscribeToAimControllers()
        {
            SubscribeToSlingshot();
            SubscribeToJumpAim();
        }

        private void SubscribeToSlingshot()
        {
            if (SlingshotAimController.Instance != null && !_isSubscribedSlingshot)
            {
                SlingshotAimController.Instance.OnAimStarted -= HandleAimStarted;
                SlingshotAimController.Instance.OnAimUpdated -= HandleAimUpdated;
                SlingshotAimController.Instance.OnAimReleased -= HandleAimReleased;
                SlingshotAimController.Instance.OnAimCanceled -= HandleAimCanceled;

                SlingshotAimController.Instance.OnAimStarted += HandleAimStarted;
                SlingshotAimController.Instance.OnAimUpdated += HandleAimUpdated;
                SlingshotAimController.Instance.OnAimReleased += HandleAimReleased;
                SlingshotAimController.Instance.OnAimCanceled += HandleAimCanceled;
                _isSubscribedSlingshot = true;
            }
        }

        private void SubscribeToJumpAim()
        {
            if (JumpAimController.Instance != null && !_isSubscribedJumpAim)
            {
                JumpAimController.Instance.OnJumpAimStarted -= HandleJumpAimStarted;
                JumpAimController.Instance.OnJumpAimUpdated -= HandleJumpAimUpdated;
                JumpAimController.Instance.OnJumpAimExecuted -= HandleJumpAimExecuted;
                JumpAimController.Instance.OnJumpAimCanceled -= HandleJumpAimCanceled;

                JumpAimController.Instance.OnJumpAimStarted += HandleJumpAimStarted;
                JumpAimController.Instance.OnJumpAimUpdated += HandleJumpAimUpdated;
                JumpAimController.Instance.OnJumpAimExecuted += HandleJumpAimExecuted;
                JumpAimController.Instance.OnJumpAimCanceled += HandleJumpAimCanceled;
                _isSubscribedJumpAim = true;
            }
        }

        private void UnsubscribeFromAimControllers()
        {
            if (SlingshotAimController.Instance != null && _isSubscribedSlingshot)
            {
                SlingshotAimController.Instance.OnAimStarted -= HandleAimStarted;
                SlingshotAimController.Instance.OnAimUpdated -= HandleAimUpdated;
                SlingshotAimController.Instance.OnAimReleased -= HandleAimReleased;
                SlingshotAimController.Instance.OnAimCanceled -= HandleAimCanceled;
                _isSubscribedSlingshot = false;
            }

            if (JumpAimController.Instance != null && _isSubscribedJumpAim)
            {
                JumpAimController.Instance.OnJumpAimStarted -= HandleJumpAimStarted;
                JumpAimController.Instance.OnJumpAimUpdated -= HandleJumpAimUpdated;
                JumpAimController.Instance.OnJumpAimExecuted -= HandleJumpAimExecuted;
                JumpAimController.Instance.OnJumpAimCanceled -= HandleJumpAimCanceled;
                _isSubscribedJumpAim = false;
            }
        }

        #region Slingshot Event Handlers
        private void HandleAimStarted(Vector2 origin)
        {
            SetTrajectoryColor(_trajectoryColor);
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = true;
                _lineRenderer.positionCount = 0;
            }
        }

        private void HandleAimUpdated(Vector2 origin, Vector2 direction, float power, float normalizedPower)
        {
            SetTrajectoryColor(_trajectoryColor);
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

        #region Jump Aim Event Handlers
        private void HandleJumpAimStarted(Vector2 origin)
        {
            SetTrajectoryColor(Color.white);
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = true;
                _lineRenderer.positionCount = 0;
            }
        }

        private void HandleJumpAimUpdated(Vector2 origin, Vector2 direction, float force, int level, float gravityMultiplier)
        {
            SetTrajectoryColor(Color.white);
            PredictTrajectory(origin, direction, force, gravityMultiplier);
        }

        private void HandleJumpAimExecuted(Vector2 direction, float force)
        {
            HideTrajectory();
        }

        private void HandleJumpAimCanceled()
        {
            HideTrajectory();
        }
        #endregion

        /// <summary>
        /// Simulación numérica física paso a paso considerando campos gravitatorios radiales (Euler Integration).
        /// </summary>
        public void PredictTrajectory(Vector2 startPos, Vector2 direction, float launchPower, float gravityMultiplier = 1.0f)
        {
            float power = Mathf.Max(launchPower, 1.0f);
            Vector2 currentPos = startPos + (direction.normalized * 1.0f);
            Vector2 velocity = direction.normalized * power;

            int validPointCount = 0;
            _simulationPoints[validPointCount++] = new Vector3(currentPos.x, currentPos.y, 0f);

            for (int i = 1; i < _maxSimulationSteps; i++)
            {
                // 1. Calcular gravedad acumulada en el punto actual escalada por el multiplicador
                Vector2 gravityForce = GravityBody.GetTotalGravitationalPull(currentPos) * gravityMultiplier;
                Vector2 acceleration = gravityForce / _simulatedMass;

                // 2. Integración de velocidad y posición
                velocity += acceleration * _timeStep;
                Vector2 nextPos = currentPos + (velocity * _timeStep);

                // 3. Chequeo de colisión en el trayecto (ignorando al propio héroe y triggers)
                Vector2 stepDir = nextPos - currentPos;
                float stepDist = stepDir.magnitude;

                if (i > 2 && stepDist > 0.001f)
                {
                    RaycastHit2D hit = Physics2D.Raycast(currentPos, stepDir.normalized, stepDist, _collisionMask);
                    if (hit.collider != null && !hit.collider.isTrigger && !hit.collider.TryGetComponent<Hero>(out _))
                    {
                        _simulationPoints[validPointCount++] = new Vector3(hit.point.x, hit.point.y, 0f);
                        break;
                    }
                }

                _simulationPoints[validPointCount++] = new Vector3(nextPos.x, nextPos.y, 0f);
                currentPos = nextPos;
            }

            // Aplicar puntos al LineRenderer
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = true;
                _lineRenderer.positionCount = validPointCount;
                for (int i = 0; i < validPointCount; i++)
                {
                    _lineRenderer.SetPosition(i, _simulationPoints[i]);
                }
            }

            // Aplicar puntos a los Dots visuales 2D
            if (_dotPool != null && _useTrajectoryDots)
            {
                int stepInterval = Mathf.Max(1, validPointCount / _dotCount);
                for (int d = 0; d < _dotCount; d++)
                {
                    int pointIndex = d * stepInterval;
                    if (pointIndex < validPointCount)
                    {
                        _dotPool[d].transform.position = _simulationPoints[pointIndex];
                        float scaleFactor = Mathf.Lerp(_dotScale, _dotScale * 0.4f, (float)d / _dotCount);
                        _dotPool[d].transform.localScale = Vector3.one * scaleFactor;
                        _dotPool[d].SetActive(true);
                    }
                    else
                    {
                        _dotPool[d].SetActive(false);
                    }
                }
            }
        }

        public void HideTrajectory()
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.positionCount = 0;
                _lineRenderer.enabled = false;
            }

            if (_dotPool != null)
            {
                for (int i = 0; i < _dotPool.Length; i++)
                {
                    if (_dotPool[i] != null)
                    {
                        _dotPool[i].SetActive(false);
                    }
                }
            }
        }
    }
}
