using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Componente físico para planetas y cuerpos celestes que ejerce atracción gravitatoria radial multicuerpo en FixedUpdate.
    /// Cumple con las reglas de rendimiento Zero-Alloc (Physics2D.OverlapCircleNonAlloc).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GravityBody : MonoBehaviour
    {
        [Header("Gravitational Parameters")]
        [SerializeField] private float _gravityRadius = 10f;
        [SerializeField] private float _gravityForce = 20f;
        [SerializeField] private bool _alignObjectsToSurface = true;
        [SerializeField] private float _alignmentSpeed = 10f;

        [Header("Layer Mask Filtering")]
        [SerializeField] private LayerMask _affectedLayers = ~0; // Por defecto afecta a todas las capas

        // Buffer pre-alocado para evitar Garbage Collection en FixedUpdate (Zero-Alloc)
        private readonly Collider2D[] _overlapResults = new Collider2D[32];

        public float GravityRadius => _gravityRadius;
        public float GravityForce => _gravityForce;
        public Vector2 Position => transform.position;

        private void FixedUpdate()
        {
            ApplyRadialGravity();
        }

        private void ApplyRadialGravity()
        {
            int hitCount = Physics2D.OverlapCircleNonAlloc(Position, _gravityRadius, _overlapResults, _affectedLayers);

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = _overlapResults[i];
                if (col == null || col.gameObject == gameObject) continue;

                // 1. Caso A: Implementa IGravityAffected (Héroes, Enemigos, Proyectiles avanzados)
                if (col.TryGetComponent<IGravityAffected>(out var gravityAffected))
                {
                    Vector2 directionToPlanet = (Position - (Vector2)gravityAffected.Transform.position);
                    float distance = directionToPlanet.magnitude;

                    if (distance > 0.01f)
                    {
                        Vector2 force = directionToPlanet.normalized * _gravityForce;
                        gravityAffected.ApplyGravitationalPull(force);

                        if (_alignObjectsToSurface)
                        {
                            Vector2 surfaceUp = -directionToPlanet.normalized;
                            gravityAffected.AlignWithSurface(surfaceUp);
                        }
                    }
                }
                // 2. Caso B: Rigidbody2D genérico (escombros, objetos sueltos)
                else if (col.TryGetComponent<Rigidbody2D>(out var rb) && !rb.isKinematic)
                {
                    Vector2 directionToPlanet = (Position - rb.position);
                    float distance = directionToPlanet.magnitude;

                    if (distance > 0.01f)
                    {
                        Vector2 force = directionToPlanet.normalized * _gravityForce;
                        rb.AddForce(force, ForceMode2D.Force);

                        if (_alignObjectsToSurface)
                        {
                            Vector2 surfaceUp = -directionToPlanet.normalized;
                            float targetAngle = Mathf.Atan2(surfaceUp.y, surfaceUp.x) * Mathf.Rad2Deg - 90f;
                            float currentAngle = Mathf.LerpAngle(rb.rotation, targetAngle, _alignmentSpeed * Time.fixedDeltaTime);
                            rb.MoveRotation(currentAngle);
                        }
                    }
                }
            }
        }

        #region Debug Gizmos
        private void OnDrawGizmosSelected()
        {
            // Radio de gravedad
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, _gravityRadius);

            // Centro del cuerpo celeste
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        #endregion
    }
}
