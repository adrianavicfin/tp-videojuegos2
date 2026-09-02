using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Contrato para entidades que son afectadas por campos gravitatorios radiales y fuerzas de empuje.
    /// </summary>
    public interface IGravityAffected
    {
        Rigidbody2D Rigidbody { get; }
        Transform Transform { get; }
        void ApplyGravitationalPull(Vector2 force);
        void AlignWithSurface(Vector2 upDirection);
    }
}
