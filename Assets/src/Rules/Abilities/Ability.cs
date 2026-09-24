using System;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Clase abstracta base para habilidades secundarias de los personajes en tiempo de ejecución.
    /// Cumple el requisito de Herencia y Polimorfismo para Hito 2 y Hito 3.
    /// </summary>
    public abstract class Ability
    {
        public string AbilityName { get; protected set; }
        public string Description { get; protected set; }
        public Sprite Icon { get; protected set; }
        public Color ThemeColor { get; protected set; } = Color.cyan;
        public int CooldownTurns { get; protected set; }
        public int CurrentCooldown { get; protected set; }
        public bool IsReady => CurrentCooldown <= 0;

        public event Action<int> OnCooldownChanged;

        public Ability(string abilityName, int cooldownTurns, string description = "", Sprite icon = null, Color? themeColor = null)
        {
            AbilityName = abilityName;
            CooldownTurns = Math.Max(0, cooldownTurns);
            CurrentCooldown = 0;
            Description = description;
            Icon = icon;
            if (themeColor.HasValue)
            {
                ThemeColor = themeColor.Value;
            }
        }

        /// <summary>
        /// Comprueba si la habilidad puede ejecutarse en el turno actual.
        /// </summary>
        public virtual bool CanExecute(Character user)
        {
            return user != null && !user.IsDead && IsReady;
        }

        /// <summary>
        /// Ejecuta la acción polimórfica de la habilidad y activa el cooldown.
        /// </summary>
        public void Trigger(Character user, Character target = null)
        {
            if (!CanExecute(user))
            {
                Debug.LogWarning($"[Ability] No se puede usar {AbilityName}. Cooldown restante: {CurrentCooldown} turnos.");
                return;
            }

            ExecuteEffect(user, target);
            CurrentCooldown = CooldownTurns;
            OnCooldownChanged?.Invoke(CurrentCooldown);
        }

        /// <summary>
        /// Efecto específico que implementa cada subclase de habilidad polimórfica.
        /// </summary>
        protected abstract void ExecuteEffect(Character user, Character target);

        /// <summary>
        /// Se invoca al inicio de cada turno para reducir el tiempo de espera.
        /// </summary>
        public virtual void TickCooldown()
        {
            if (CurrentCooldown > 0)
            {
                CurrentCooldown--;
                OnCooldownChanged?.Invoke(CurrentCooldown);
            }
        }

        public void ResetCooldown()
        {
            CurrentCooldown = 0;
            OnCooldownChanged?.Invoke(CurrentCooldown);
        }
    }
}
