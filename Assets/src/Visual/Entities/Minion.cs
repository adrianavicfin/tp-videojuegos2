using System;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Representa a un esbirro enemigo secundario en la cola de turnos.
    /// </summary>
    public class Minion : Enemy
    {
        [Header("Minion AI")]
        [SerializeField] private float _attackDamage = 15f;
        [SerializeField] private float _aggroRadius = 8f;

        public float AttackDamage => _attackDamage;

        public override void ExecuteAITurn()
        {
            Debug.Log($"[Minion] Esbirro {_characterName} avanzando y disparando al héroe más cercano.");
        }
    }
}
