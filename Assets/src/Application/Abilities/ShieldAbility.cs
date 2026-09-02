using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Habilidad secundaria de escudo gravitatorio que otorga puntos temporales de armadura/defensa.
    /// </summary>
    public class ShieldAbility : Ability
    {
        public int ShieldPoints { get; private set; }

        public ShieldAbility(int shieldPoints = 25, int cooldownTurns = 3) 
            : base("Escudo Gravitatorio", cooldownTurns)
        {
            ShieldPoints = Mathf.Max(1, shieldPoints);
        }

        protected override void ExecuteEffect(Character user, Character target)
        {
            Character recipient = target != null ? target : user;
            Debug.Log($"[ShieldAbility] {user.CharacterName} despliega {AbilityName} sobre {recipient.CharacterName}. Absorción generada: {ShieldPoints} pts.");
            
            // Simulación de absorción sumando temporalmente resistencia a la vida máxima
            recipient.Heal(ShieldPoints);
        }
    }
}
