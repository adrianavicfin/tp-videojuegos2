using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CosmosCritters
{
    /// <summary>
    /// Componente visual pasivo del HUD del jugador en Unity (Patrón MVP - View).
    /// Recibe órdenes exclusivamente del PlayerHUDPresenter y actualiza Sliders/Textos.
    /// </summary>
    public class PlayerHUDView : MonoBehaviour, IPlayerHUDView
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _slotText;
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private Image _portraitImage;
        [SerializeField] private GameObject _activeTurnIndicator;
        [SerializeField] private TextMeshProUGUI _countdownText;

        public void SetCharacterName(string characterName)
        {
            if (_nameText != null)
                _nameText.text = characterName;
        }

        public void SetSlotIndex(int slotIndex)
        {
            if (_slotText != null)
                _slotText.text = $"P{slotIndex}";
        }

        public void UpdateHealth(int currentHealth, int maxHealth)
        {
            if (_healthSlider != null && maxHealth > 0)
            {
                _healthSlider.maxValue = maxHealth;
                _healthSlider.value = currentHealth;
            }

            if (_healthText != null)
            {
                _healthText.text = $"{currentHealth} / {maxHealth}";
            }
        }

        public void SetPortrait(Sprite portrait)
        {
            if (_portraitImage != null && portrait != null)
            {
                _portraitImage.sprite = portrait;
            }
        }

        public void SetTurnActiveState(bool isActive)
        {
            if (_activeTurnIndicator != null)
            {
                _activeTurnIndicator.SetActive(isActive);
            }
        }

        public void UpdateCountdown(float remainingSeconds)
        {
            if (_countdownText != null)
            {
                _countdownText.text = $"{Mathf.CeilToInt(remainingSeconds)}s";
            }
        }
    }
}
