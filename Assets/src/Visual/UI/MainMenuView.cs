using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CosmosCritters
{
    [Serializable]
    public class HeroSlotUI
    {
        public GameObject Container;
        public Image PortraitImage;
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI RoleText;
        public GameObject EmptyStateIndicator; // Texto o ícono de "+"
    }

    /// <summary>
    /// Vista de UI para el Menú Principal (MVP - View).
    /// Muestra los slots de héroes seleccionados (1 a 4), héroes disponibles y mapa/tiempo.
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [Header("Controller Reference")]
        [SerializeField] private MainMenuController _controller;

        [Header("Squad Selection Slots (Max 4)")]
        [SerializeField] private List<HeroSlotUI> _heroSlots = new List<HeroSlotUI>();

        [Header("Settings UI Elements")]
        [SerializeField] private TextMeshProUGUI _mapText;
        [SerializeField] private TextMeshProUGUI _turnDurationText;
        [SerializeField] private Button _startMatchButton;

        private void Awake()
        {
            if (_controller == null)
                _controller = GetComponentInParent<MainMenuController>();
        }

        private void OnEnable()
        {
            if (_controller != null)
            {
                _controller.OnSelectedHeroesChanged += UpdateHeroSlots;
                _controller.OnMapChanged += UpdateMapDisplay;
                _controller.OnTurnDurationChanged += UpdateTurnDurationDisplay;
            }

            if (_startMatchButton != null && _controller != null)
            {
                _startMatchButton.onClick.AddListener(_controller.StartMatch);
            }
        }

        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.OnSelectedHeroesChanged -= UpdateHeroSlots;
                _controller.OnMapChanged -= UpdateMapDisplay;
                _controller.OnTurnDurationChanged -= UpdateTurnDurationDisplay;
            }

            if (_startMatchButton != null && _controller != null)
            {
                _startMatchButton.onClick.RemoveListener(_controller.StartMatch);
            }
        }

        private void UpdateHeroSlots(IReadOnlyList<HeroDataSO> selectedHeroes)
        {
            for (int i = 0; i < _heroSlots.Count; i++)
            {
                var slot = _heroSlots[i];
                if (slot == null) continue;

                if (i < selectedHeroes.Count)
                {
                    // Slot ocupado
                    HeroDataSO hero = selectedHeroes[i];

                    if (slot.PortraitImage != null)
                    {
                        slot.PortraitImage.gameObject.SetActive(hero.Portrait != null);
                        if (hero.Portrait != null) slot.PortraitImage.sprite = hero.Portrait;
                    }

                    if (slot.NameText != null) slot.NameText.text = hero.HeroName;
                    if (slot.RoleText != null) slot.RoleText.text = hero.Role.ToString();
                    if (slot.EmptyStateIndicator != null) slot.EmptyStateIndicator.SetActive(false);
                }
                else
                {
                    // Slot vacío
                    if (slot.PortraitImage != null) slot.PortraitImage.gameObject.SetActive(false);
                    if (slot.NameText != null) slot.NameText.text = "VACÍO";
                    if (slot.RoleText != null) slot.RoleText.text = "Haz clic para añadir";
                    if (slot.EmptyStateIndicator != null) slot.EmptyStateIndicator.SetActive(true);
                }
            }
        }

        private void UpdateMapDisplay(int mapIndex)
        {
            if (_mapText != null)
            {
                _mapText.text = $"Mapa: Orbital #{mapIndex + 1}";
            }
        }

        private void UpdateTurnDurationDisplay(float duration)
        {
            if (_turnDurationText != null)
            {
                _turnDurationText.text = $"Turno: {duration:F0}s";
            }
        }
    }
}
