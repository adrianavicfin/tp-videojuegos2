using System;

namespace CosmosCritters
{
    /// <summary>
    /// Presentador para el HUD de combate del jugador (Patrón MVP - Presenter).
    /// Clase en C# puro (sin MonoBehaviour) que media entre el Modelo de estadísticas y la Vista visual.
    /// </summary>
    public class PlayerHUDPresenter : IDisposable
    {
        private readonly IPlayerHUDView _view;
        private readonly CharacterStats _model;
        private readonly ICountdownTimer _turnTimer;

        public PlayerHUDPresenter(IPlayerHUDView view, CharacterStats model, ICountdownTimer turnTimer = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _turnTimer = turnTimer;

            SubscribeEvents();
            InitializeView();
        }

        private void InitializeView()
        {
            _view.SetCharacterName(_model.CharacterName);
            _view.UpdateHealth(_model.CurrentHealth, _model.MaxHealth);
            _view.SetTurnActiveState(false);
        }

        private void SubscribeEvents()
        {
            _model.OnHealthChanged += HandleHealthChanged;
            _model.OnDied += HandleDied;

            if (_turnTimer != null)
            {
                _turnTimer.OnTick += HandleTimerTick;
            }
        }

        private void UnsubscribeEvents()
        {
            _model.OnHealthChanged -= HandleHealthChanged;
            _model.OnDied -= HandleDied;

            if (_turnTimer != null)
            {
                _turnTimer.OnTick -= HandleTimerTick;
            }
        }

        #region Event Handlers
        private void HandleHealthChanged(int currentHealth, int maxHealth)
        {
            _view.UpdateHealth(currentHealth, maxHealth);
        }

        private void HandleDied()
        {
            _view.UpdateHealth(0, _model.MaxHealth);
            _view.SetTurnActiveState(false);
        }

        private void HandleTimerTick(float remainingTime)
        {
            _view.UpdateCountdown(remainingTime);
        }
        #endregion

        public void SetTurnActive(bool isActive)
        {
            _view.SetTurnActiveState(isActive);
        }

        public void Dispose()
        {
            UnsubscribeEvents();
        }
    }
}
