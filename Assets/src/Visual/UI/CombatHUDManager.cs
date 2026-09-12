using System.Collections.Generic;
using UnityEngine;

namespace CosmosCritters
{
    /// <summary>
    /// Coordinador en escena del HUD de combate (Patrón MVP).
    /// Instancia y conecta los Presenters (PlayerHUDPresenter) entre las entidades vivas (CharacterStats) y las Vistas de UI (PlayerHUDView).
    /// </summary>
    public class CombatHUDManager : MonoBehaviour
    {
        [Header("HUD View Reference")]
        [SerializeField] private PlayerHUDView _activePlayerHUD;

        [Header("Boss HUD View (Opcional)")]
        [SerializeField] private PlayerHUDView _bossHUD;

        private PlayerHUDPresenter _activePresenter;
        private PlayerHUDPresenter _bossPresenter;

        private void Start()
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnTurnStarted += HandleTurnStarted;
                TurnManager.Instance.OnTurnEnded += HandleTurnEnded;
            }

            SetupBossHUD();
        }

        private void OnDestroy()
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnTurnStarted -= HandleTurnStarted;
                TurnManager.Instance.OnTurnEnded -= HandleTurnEnded;
            }

            _activePresenter?.Dispose();
            _bossPresenter?.Dispose();
        }

        private void SetupBossHUD()
        {
            if (_bossHUD == null) return;

            Boss boss = FindObjectOfType<Boss>();
            if (boss != null && boss.Stats != null)
            {
                _bossPresenter = new PlayerHUDPresenter(_bossHUD, boss.Stats);
            }
        }

        private void HandleTurnStarted(Character activeChar)
        {
            if (_activePlayerHUD == null || activeChar == null || activeChar.Stats == null) return;

            // Desconectar el presenter anterior
            _activePresenter?.Dispose();

            // Crear el nuevo Presenter conectando la Vista con el Modelo del personaje de turno
            ICountdownTimer timer = TurnManager.Instance != null ? TurnManager.Instance.TurnTimer : null;
            _activePresenter = new PlayerHUDPresenter(_activePlayerHUD, activeChar.Stats, timer);
            _activePresenter.SetTurnActive(true);

            if (activeChar is Hero hero)
            {
                _activePlayerHUD.SetSlotIndex(hero.SlotIndex);
                if (hero.HeroData != null && hero.HeroData.Portrait != null)
                {
                    _activePlayerHUD.SetPortrait(hero.HeroData.Portrait);
                }
                else if (hero.SpriteRenderer != null && hero.SpriteRenderer.sprite != null)
                {
                    _activePlayerHUD.SetPortrait(hero.SpriteRenderer.sprite, hero.SpriteRenderer.color);
                }
            }
            else if (activeChar.SpriteRenderer != null && activeChar.SpriteRenderer.sprite != null)
            {
                _activePlayerHUD.SetPortrait(activeChar.SpriteRenderer.sprite, activeChar.SpriteRenderer.color);
            }
        }

        private void HandleTurnEnded(Character activeChar)
        {
            if (_activePresenter != null)
            {
                _activePresenter.SetTurnActive(false);
            }
        }
    }
}
