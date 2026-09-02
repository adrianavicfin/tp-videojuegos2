using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Delimita el perímetro del mapa orbital y detecta la expulsión de entidades hacia el espacio exterior (KillZone).
    /// Provoca la muerte instantánea de combatientes al caer al vacío (OnTriggerExit2D).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KillZoneView : MonoBehaviour
    {
        [Header("KillZone Configuration")]
        [SerializeField] private bool _destroyDebris = true;
        [SerializeField] private GameObject _deathVfxPrefab;

        private Collider2D _boundaryCollider;

        private void Awake()
        {
            _boundaryCollider = GetComponent<Collider2D>();
            _boundaryCollider.isTrigger = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other == null) return;

            // 1. Caso A: Un Character (Héroe o Enemigo) cayó al vacío del espacio exterior
            if (other.TryGetComponent<Character>(out var character))
            {
                if (!character.IsDead)
                {
                    Debug.LogWarning($"[KillZone] ¡{character.CharacterName} fue expulsado al vacío espacial! Muerte instantánea.");

                    if (_deathVfxPrefab != null)
                    {
                        Instantiate(_deathVfxPrefab, character.transform.position, Quaternion.identity);
                    }

                    character.TakeDamage(99999);
                }
            }
            // 2. Caso B: Un Proyectil salió de los límites orbitales del mapa
            else if (other.TryGetComponent<Projectile>(out var projectile))
            {
                Debug.Log("[KillZone] Proyectil perdido en el espacio profundo.");
                projectile.Explode();
            }
            // 3. Caso C: Escombros o esferas físicas sueltas
            else if (_destroyDebris && other.GetComponent<Rigidbody2D>() != null)
            {
                Destroy(other.gameObject);
            }
        }

        #region Debug Gizmos
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.4f);

            if (TryGetComponent<BoxCollider2D>(out var box))
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
            }
            else if (TryGetComponent<CircleCollider2D>(out var circle))
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
        }
        #endregion
    }
}
