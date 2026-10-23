using System;
using Shiftworks.Economy;
using Shiftworks.Factory;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Shiftworks.UI
{
    /// <summary>
    /// Modal expansion-purchase panel. Opening/closing is entirely driven by whoever calls
    /// Open() (the Player); this component only owns the panel's own visibility and button
    /// wiring — cursor state stays PlayerInteractor's responsibility, same pattern as
    /// MachineShopPanelUI/OrderBoardPanelUI. Balance/status are re-read fresh on every
    /// Refresh() call, so the panel can never show a stale balance after a purchase.
    /// </summary>
    public class FactoryExpansionPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text nameText;
        [SerializeField] private Text sizeText;
        [SerializeField] private Text priceText;
        [SerializeField] private Text balanceText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text warningText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameHUD gameHUD;

        private FactoryExpansionManager _expansionManager;
        private EconomyManager _economyManager;
        private FactoryExpansionDefinition _definition;
        private Action _onClose;

        private bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            if (buyButton != null) buyButton.onClick.AddListener(OnBuyClicked);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (panelRoot != null) panelRoot.SetActive(false);
            if (warningText != null) warningText.enabled = false;
        }

        public void Open(FactoryExpansionManager expansionManager, EconomyManager economyManager,
            FactoryExpansionDefinition definition, Action onClose)
        {
            _expansionManager = expansionManager;
            _economyManager = economyManager;
            _definition = definition;
            _onClose = onClose;

            if (panelRoot != null) panelRoot.SetActive(true);
            if (warningText != null) warningText.enabled = false;
            Refresh();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            if (panelRoot != null) panelRoot.SetActive(false);

            Action callback = _onClose;
            _onClose = null;
            callback?.Invoke();
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
            }
        }

        private void OnBuyClicked()
        {
            if (_expansionManager == null || _definition == null)
            {
                return;
            }

            if (_expansionManager.IsUnlocked(_definition))
            {
                ShowWarning("Bu alan zaten satın alındı.");
                Refresh();
                return;
            }

            bool success = _expansionManager.TryPurchase(_definition);
            if (!success)
            {
                ShowWarning("Yetersiz bakiye!");
            }
            else
            {
                if (warningText != null) warningText.enabled = false;
                if (gameHUD != null) gameHUD.ShowNotification("Yeni Üretim Alanı Açıldı!");
            }

            Refresh();
        }

        private void ShowWarning(string message)
        {
            if (warningText == null)
            {
                return;
            }

            warningText.enabled = true;
            warningText.text = message;
        }

        private void Refresh()
        {
            if (_definition == null)
            {
                return;
            }

            bool unlocked = _expansionManager != null && _expansionManager.IsUnlocked(_definition);

            if (nameText != null) nameText.text = _definition.ExpansionName;
            if (sizeText != null) sizeText.text = $"Alan Büyüklüğü: {_definition.SizeMeters.x:0}x{_definition.SizeMeters.y:0} m";
            if (priceText != null) priceText.text = $"Fiyat: {_definition.Price} $";
            if (balanceText != null && _economyManager != null) balanceText.text = $"Bakiye: {_economyManager.Balance} $";
            if (statusText != null) statusText.text = unlocked ? "Durum: Satın Alındı" : "Durum: Kilitli";

            if (buyButton != null)
            {
                buyButton.interactable = !unlocked && _economyManager != null && _economyManager.Balance >= _definition.Price;
            }
        }
    }
}
