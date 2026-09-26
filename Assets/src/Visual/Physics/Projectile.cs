using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Proyectil balístico físico afectado por gravedad radial en FixedUpdate.
    /// Al detonar, inflige daño a entidades y coberturas (IDamageable) y aplica Knockback físico a aliados.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour, IGravityAffected
    {
        [Header("Projectile Stats")]
        [SerializeField] private int _damage = 35;
        [SerializeField] private float _explosionRadius = 2.5f;
        [SerializeField] private float _knockbackForce = 15f;
        [SerializeField] private GameObject _explosionVfxPrefab;
        [SerializeField] private float _maxLifetime = 10f;
        [SerializeField] private bool _useGeneratedPlaceholder = true;
        [Header("Ballistics")]
        [Tooltip("Porcion de la fuerza gravitatoria de los planetas aplicada a este proyectil. Los personajes conservan su propia gravedad.")]
        [SerializeField, Range(0f, 1f)] private float _gravityResponse = 0.1f;

        private Rigidbody2D _rb;
        private SpriteRenderer _spriteRenderer;
        private Character _owner;
        private bool _hasExploded = false;
        private float _aliveTimer = 0f;

        private static Sprite _generatedPlaceholderSprite;

        // Buffer pre-alocado para detección de explosión Zero-Alloc
        private readonly Collider2D[] _explosionHits = new Collider2D[20];

        #region IGravityAffected
        public Rigidbody2D Rigidbody => _rb;
        public Transform Transform => transform;
        public float GravityResponse => _gravityResponse;

        public void ApplyGravitationalPull(Vector2 force)
        {
            if (_rb != null && !_rb.isKinematic)
            {
                _rb.AddForce(force * _gravityResponse, ForceMode2D.Force);
            }
        }

        public void AlignWithSurface(Vector2 upDirection)
        {
            if (_rb != null && _rb.velocity.sqrMagnitude > 0.1f)
            {
                float angle = Mathf.Atan2(_rb.velocity.y, _rb.velocity.x) * Mathf.Rad2Deg;
                _rb.MoveRotation(angle);
            }
        }
        #endregion

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_useGeneratedPlaceholder && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = GetOrCreatePlaceholderSprite();
                _spriteRenderer.sortingOrder = Mathf.Max(_spriteRenderer.sortingOrder, 100);
            }
        }

        private static Sprite GetOrCreatePlaceholderSprite()
        {
            if (_generatedPlaceholderSprite != null)
            {
                return _generatedPlaceholderSprite;
            }

            const int textureSize = 32;
            const float pixelsPerUnit = 16f;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            texture.name = "ProjectilePlaceholderTexture";
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.hideFlags = HideFlags.HideAndDontSave;

            Color32[] pixels = new Color32[textureSize * textureSize];
            Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
            float radius = textureSize * 0.45f;
            float radiusSquared = radius * radius;

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float deltaX = x - center.x;
                    float deltaY = y - center.y;
                    bool insideCircle = deltaX * deltaX + deltaY * deltaY <= radiusSquared;
                    pixels[y * textureSize + x] = insideCircle
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(255, 255, 255, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            _generatedPlaceholderSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, textureSize, textureSize),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
            _generatedPlaceholderSprite.name = "ProjectilePlaceholderSprite";
            _generatedPlaceholderSprite.hideFlags = HideFlags.HideAndDontSave;

            return _generatedPlaceholderSprite;
        }

        private void Update()
        {
            if (_hasExploded) return;

            _aliveTimer += Time.deltaTime;
            if (_aliveTimer >= _maxLifetime)
            {
                _hasExploded = true;
                Debug.LogWarning($"[Projectile] {name} agotó su tiempo de vida ({_maxLifetime}s) sin impactar.");
                FinishFlight();
            }
        }

        public void Launch(Vector2 direction, float power, Character owner, int damage = 35, float explosionRadius = 2.5f, float knockbackForce = 15f)
        {
            _owner = owner;
            _damage = damage;
            _explosionRadius = explosionRadius;
            _knockbackForce = knockbackForce;

            // Ignorar colisiones inmediatas con el atacante para evitar auto-detonación en frame 0
            if (_owner != null)
            {
                Collider2D myCol = GetComponent<Collider2D>();
                Collider2D[] ownerCols = _owner.GetComponentsInChildren<Collider2D>();
                if (myCol != null && ownerCols != null)
                {
                    for (int i = 0; i < ownerCols.Length; i++)
                    {
                        if (ownerCols[i] != null)
                        {
                            Physics2D.IgnoreCollision(myCol, ownerCols[i], true);
                        }
                    }
                }
            }

            if (_rb != null)
            {
                _rb.velocity = Vector2.zero;
                _rb.AddForce(direction.normalized * power, ForceMode2D.Impulse);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_hasExploded) return;

            Collider2D hit = collision.collider;
            if (hit != null && hit.GetComponentInParent<Boss>() != null)
            {
                Debug.Log($"[Projectile] {name} impactó directamente contra el Boss en {transform.position}.");
            }
            else if (hit != null && (hit.GetComponentInParent<GravityBody>() != null || hit.gameObject.layer == LayerMask.NameToLayer("Ground")))
            {
                Debug.Log($"[Projectile] {name} impactó contra el suelo en {transform.position}.");
            }
            else
            {
                Debug.Log($"[Projectile] {name} impactó contra {collision.gameObject.name} en {transform.position}.");
            }

            Explode();
        }

        public void ExitMap()
        {
            if (_hasExploded) return;
            _hasExploded = true;
            Debug.Log($"[Projectile] {name} salió del mapa por la KillZone en {transform.position}. Disparo perdido.");
            FinishFlight();
        }

        public void Explode()
        {
            if (_hasExploded) return;
            _hasExploded = true;

            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, _explosionRadius, _explosionHits);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = _explosionHits[i];
                // Excluir el proyectil mismo y al dueño que disparó para evitar autodestrucción/auto-knockback
                if (col == null || col.gameObject == gameObject || (_owner != null && col.gameObject == _owner.gameObject)) continue;

                Vector2 direction = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;
                if (direction == Vector2.zero) direction = Vector2.up;

                // 1. Caso A: Es un Character (Héroe o Enemigo)
                if (col.TryGetComponent<Character>(out var character))
                {
                    bool isAlly = (_owner is Hero && character is Hero);

                    if (isAlly)
                    {
                        // Friendly Fire Indirecto: Empuje físico sin daño
                        Debug.Log($"[FriendlyFire] Aliado {character.CharacterName} empujado por la explosión.");
                        character.ApplyGravitationalPull(direction * _knockbackForce * 2f);
                    }
                    else
                    {
                        character.TakeDamage(_damage);
                        character.ApplyGravitationalPull(direction * _knockbackForce);
                    }
                }
                // 2. Caso B: Es una Cobertura Destructible (DestructibleCover)
                else if (col.TryGetComponent<DestructibleCover>(out var cover))
                {
                    cover.TakeDamage(_damage);
                }
                // 3. Caso C: Escombros o Rigidbody2D genérico
                else if (col.TryGetComponent<Rigidbody2D>(out var rb) && !rb.isKinematic)
                {
                    rb.AddForce(direction * _knockbackForce, ForceMode2D.Impulse);
                }
            }

            if (_explosionVfxPrefab != null)
            {
                Instantiate(_explosionVfxPrefab, transform.position, Quaternion.identity);
            }

            FinishFlight();
        }

        private void FinishFlight()
        {
            TurnManager.Instance?.NotifyActionResolved();
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}
