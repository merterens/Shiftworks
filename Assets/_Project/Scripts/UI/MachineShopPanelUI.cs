using System;
using Shiftworks.Economy;
using Shiftworks.Machines;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Shiftworks.UI
{
    /// <summary>
    /// Modal shop panel. Opening/closing is entirely driven by whoever calls Open() (the
    /// Player); this component only owns the panel's own visibility and button wiring —
    /// cursor state stays PlayerInteractor's responsibility, same pattern as OrderBoardPanelUI.
    /// </summary>
    public class MachineShopPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text machineNameText;
        [SerializeField] private Text priceText;
        [SerializeField] private Text producesText;
        [SerializeField] private Text balanceText;
        [SerializeField] private Text pendingText;
        [SerializeField] private Text warningText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button placeButton;
        [SerializeField] private Button closeButton;

        private MachinePurchaseManager _purchaseManager;
        private EconomyManager _economyManager;
        private MachineDefinition _definition;
        private Action _onClose;
        private Action _onBeginPlacement;

        private bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            if (buyButton != null) buyButton.onClick.AddListener(OnBuyClicked);
            if (placeButton != null) placeButton.onClick.AddListener(OnPlaceClicked);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (panelRoot != null) panelRoot.SetActive(false);
            if (warningText != null) warningText.enabled = false;
        }

        public void Open(MachinePurchaseManager purchaseManager, EconomyManager economyManager,
            MachineDefinition definition, Action onClose, Action onBeginPlacement)
        {
            _purchaseManager = purchaseManager;
            _economyManager = economyManager;
            _definition = definition;
            _onClose = onClose;
            _onBeginPlacement = onBeginPlacement;

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
            if (_purchaseManager == null || _definition == null)
            {
                return;
            }

            bool success = _purchaseManager.TryPurchase(_definition);
            if (warningText != null)
            {
                warningText.enabled = !success;
                warningText.text = "Yetersiz bakiye!";
            }

            Refresh();
        }

        private void OnPlaceClicked()
        {
            // Close first so the cursor re-locks and FirstPersonController resumes, then
            // start placement mode — which only re-suspends PlayerInteractor (not movement).
            Action beginPlacement = _onBeginPlacement;
            Close();
            beginPlacement?.Invoke();
        }

        private void Refresh()
        {
            if (_definition == null)
            {
                return;
            }

            if (machineNameText != null) machineNameText.text = _definition.MachineName;
            if (priceText != null) priceText.text = $"Fiyat: {_definition.Price} $";
            if (balanceText != null && _economyManager != null) balanceText.text = $"Bakiye: {_economyManager.Balance} $";

            if (producesText != null)
            {
                string producedItem = _definition.Recipe != null && _definition.Recipe.OutputItem != null
                    ? _definition.Recipe.OutputItem.DisplayName
                    : "-";
                producesText.text = $"Üretir: {producedItem}";
            }

            int pending = _purchaseManager != null ? _purchaseManager.GetPendingCount(_definition) : 0;
            if (pendingText != null) pendingText.text = $"Bekleyen (yerleştirilmemiş): {pending}";

            if (buyButton != null)
            {
                buyButton.interactable = _economyManager != null && _economyManager.Balance >= _definition.Price;
            }

            if (placeButton != null)
            {
                placeButton.interactable = pending > 0;
            }
        }
    }
}
