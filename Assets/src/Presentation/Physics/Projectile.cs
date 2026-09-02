using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Proyectil balístico físico afectado por gravedad radial en FixedUpdate.
    /// Al detonar, inflige daño a enemigos y aplica Knockback físico a aliados (Friendly Fire Indirecto).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour, IGravityAffected
    {
        [Header("Projectile Stats")]
        [SerializeField] private int _damage = 35;
        [SerializeField] private float _explosionRadius = 2.5f;
        [SerializeField] private float _knockbackForce = 15f;
        [SerializeField] private GameObject _explosionVfxPrefab;

        private Rigidbody2D _rb;
        private Character _owner;
        private bool _hasExploded = false;

        // Buffer pre-alocado para detección de explosión Zero-Alloc
        private readonly Collider2D[] _explosionHits = new Collider2D[20];

        #region IGravityAffected
        public Rigidbody2D Rigidbody => _rb;
        public Transform Transform => transform;

        public void ApplyGravitationalPull(Vector2 force)
        {
            if (_rb != null && !_rb.isKinematic)
            {
                _rb.AddForce(force, ForceMode2D.Force);
            }
        }

        public void AlignWithSurface(Vector2 upDirection)
        {
            // Opcional: orientar la punta del proyectil hacia su vector de velocidad
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
        }

        public void Launch(Vector2 direction, float power, Character owner, int damage = 35, float explosionRadius = 2.5f, float knockbackForce = 15f)
        {
            _owner = owner;
            _damage = damage;
            _explosionRadius = explosionRadius;
            _knockbackForce = knockbackForce;

            if (_rb != null)
            {
                _rb.velocity = Vector2.zero;
                _rb.AddForce(direction.normalized * power, ForceMode2D.Impulse);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_hasExploded) return;
            Explode();
        }

        public void Explode()
        {
            if (_hasExploded) return;
            _hasExploded = true;

            Debug.Log($"[Projectile] ¡Explosión en {transform.position}! Radio: {_explosionRadius}, Daño: {_damage}");

            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, _explosionRadius, _explosionHits);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = _explosionHits[i];
                if (col == null || col.gameObject == gameObject) continue;

                Vector2 direction = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;
                if (direction == Vector2.zero) direction = Vector2.up;

                // 1. Si es un Character
                if (col.TryGetComponent<Character>(out var character))
                {
                    bool isAlly = (_owner is Hero && character is Hero);

                    if (isAlly)
                    {
                        // Friendly Fire Indirecto: Empuje físico sin restar puntos de salud
                        Debug.Log($"[FriendlyFire] Aliado {character.CharacterName} empujado por onda expansiva (Knockback: {_knockbackForce})");
                        character.ApplyGravitationalPull(direction * _knockbackForce * 2f);
                    }
                    else
                    {
                        // Daño directo a enemigos
                        character.TakeDamage(_damage);
                        character.ApplyGravitationalPull(direction * _knockbackForce);
                    }
                }
                // 2. Si es un Rigidbody2D genérico (escombros/coberturas)
                else if (col.TryGetComponent<Rigidbody2D>(out var rb) && !rb.isKinematic)
                {
                    rb.AddForce(direction * _knockbackForce, ForceMode2D.Impulse);
                }
            }

            if (_explosionVfxPrefab != null)
            {
                Instantiate(_explosionVfxPrefab, transform.position, Quaternion.identity);
            }

            // Notificar al TurnManager que la acción terminó de resolverse
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.NotifyActionResolved();
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}
