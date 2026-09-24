using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CosmosCritters
{
    /// <summary>
    /// Componente de UI para el botón táctico de Habilidad Secundaria del Héroe activo (H3-T7).
    /// Completamente desacoplado: obtiene nombre, ícono, descripción y colores de forma data-driven polimórfica.
    /// </summary>
    public class AbilityButtonView : MonoBehaviour
    {
        [Header("UI Component References")]
        [SerializeField] private Button _abilityButton;
        [SerializeField] private TextMeshProUGUI _abilityNameText;
        [SerializeField] private TextMeshProUGUI _cooldownText;
        [SerializeField] private TextMeshProUGUI _hotkeyText;
        [SerializeField] private Image _abilityIcon;
        [SerializeField] private Image _cooldownOverlay;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Hotkey Configuration")]
        [SerializeField] private KeyCode _triggerHotkey = KeyCode.E;

        private Hero _currentHero;
        private Ability _currentAbility;

        public Hero CurrentHero => _currentHero;
        public Ability CurrentAbility => _currentAbility;

        private void Awake()
        {
            if (_abilityButton != null)
            {
                _abilityButton.onClick.AddListener(HandleButtonClicked);
            }

            if (_hotkeyText != null)
            {
                _hotkeyText.text = $"[{_triggerHotkey}]";
            }
        }

        private void OnDestroy()
        {
            if (_abilityButton != null)
            {
                _abilityButton.onClick.RemoveListener(HandleButtonClicked);
            }

            UnbindHero();
        }

        private void Update()
        {
            // Atajo de teclado (tecla E) para ejecutar la habilidad en el turno activo
            if (Input.GetKeyDown(_triggerHotkey) && CanExecuteCurrentAbility())
            {
                TriggerAbility();
            }
        }

        /// <summary>
        /// Vincula el botón a la habilidad secundaria del héroe de turno.
        /// </summary>
        public void BindHero(Hero hero)
        {
            UnbindHero();

            if (hero == null)
            {
                SetVisible(false);
                return;
            }

            _currentHero = hero;
            _currentAbility = hero.SecondaryAbility;

            if (_currentAbility != null)
            {
                _currentAbility.OnCooldownChanged += HandleCooldownChanged;
                SetVisible(true);
                UpdateUI();
            }
            else
            {
                SetVisible(false);
            }
        }

        /// <summary>
        /// Desvincula al héroe anterior y limpia eventos.
        /// </summary>
        public void UnbindHero()
        {
            if (_currentAbility != null)
            {
                _currentAbility.OnCooldownChanged -= HandleCooldownChanged;
            }

            _currentHero = null;
            _currentAbility = null;
        }

        private void HandleCooldownChanged(int remainingTurns)
        {
            UpdateUI();
        }

        private void HandleButtonClicked()
        {
            if (CanExecuteCurrentAbility())
            {
                TriggerAbility();
            }
        }

        /// <summary>
        /// Comprueba si se cumplen todas las condiciones de turno y cooldown para activar la habilidad.
        /// </summary>
        public bool CanExecuteCurrentAbility()
        {
            if (_currentHero == null || _currentHero.IsDead || _currentAbility == null)
            {
                return false;
            }

            // Validar turno si hay TurnManager
            if (TurnManager.Instance != null)
            {
                if (TurnManager.Instance.CurrentPhase != TurnPhase.WaitingInput) return false;
                if (TurnManager.Instance.ActiveCharacter != _currentHero) return false;
            }

            return _currentAbility.IsReady;
        }

        private void TriggerAbility()
        {
            if (_currentHero == null || _currentAbility == null) return;

            Debug.Log($"[AbilityButtonView] Gatillando habilidad '{_currentAbility.AbilityName}' para {_currentHero.CharacterName}.");
            
            _currentHero.ExecuteSecondaryAbility();
            UpdateUI();
        }

        /// <summary>
        /// Refresca los textos, colores y estado interactivo del botón según los metadatos de la habilidad.
        /// Cero switches o type-checks duros; puro polimorfismo y data-driven.
        /// </summary>
        public void UpdateUI()
        {
            if (_currentAbility == null)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            // 1. Nombre de la habilidad
            if (_abilityNameText != null)
            {
                _abilityNameText.text = _currentAbility.AbilityName;
            }

            // 2. Estado de Cooldown e interactividad
            bool isReady = _currentAbility.IsReady;

            if (_abilityButton != null)
            {
                _abilityButton.interactable = isReady;
            }

            if (_cooldownOverlay != null)
            {
                _cooldownOverlay.gameObject.SetActive(!isReady);
            }

            if (_cooldownText != null)
            {
                if (isReady)
                {
                    _cooldownText.text = "LISTO";
                    _cooldownText.color = new Color(0.25f, 0.95f, 0.45f, 1f); // Verde brillante
                }
                else
                {
                    _cooldownText.text = $"CD: {_currentAbility.CurrentCooldown}T";
                    _cooldownText.color = new Color(1f, 0.4f, 0.3f, 1f); // Naranja/Rojo recarga
                }
            }

            // 3. Ícono y Color Temático (desacoplado directo de Ability)
            if (_abilityIcon != null)
            {
                if (_currentAbility.Icon != null)
                {
                    _abilityIcon.sprite = _currentAbility.Icon;
                    _abilityIcon.color = Color.white;
                }
                else
                {
                    _abilityIcon.color = _currentAbility.ThemeColor;
                }
            }
        }

        public void SetVisible(bool isVisible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isVisible ? 1f : 0f;
                _canvasGroup.interactable = isVisible;
                _canvasGroup.blocksRaycasts = isVisible;
            }
            else
            {
                gameObject.SetActive(isVisible);
            }
        }
    }
}
