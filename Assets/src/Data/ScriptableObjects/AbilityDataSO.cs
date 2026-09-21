using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// ScriptableObject base abstracto para la configuración desacoplada de Habilidades Secundarias.
    /// Permite crear nuevas habilidades directamente como Assets en el Inspector y asignarlas a los Critters.
    /// </summary>
    public abstract class AbilityDataSO : ScriptableObject
    {
        [Header("Ability Identity")]
        [SerializeField] private string _abilityName = "Nueva Habilidad";
        [TextArea(2, 4)]
        [SerializeField] private string _description = "Descripción de la habilidad";
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _themeColor = Color.cyan;

        [Header("Cooldown")]
        [Min(0)]
        [SerializeField] private int _cooldownTurns = 2;

        public string AbilityName => _abilityName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public Color ThemeColor => _themeColor;
        public int CooldownTurns => _cooldownTurns;

        /// <summary>
        /// Instancia polimórfica en tiempo de ejecución de la clase base Ability.
        /// </summary>
        public abstract Ability CreateRuntimeAbility();
    }
}
