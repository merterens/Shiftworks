using System;
using Shiftworks.Economy;
using Shiftworks.Machines;
using Shiftworks.Multiplayer;
using Shiftworks.Power;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Shiftworks.UI
{
    /// <summary>
    /// Modal electricity panel. Opening/closing is entirely driven by whoever calls Open()
    /// (the Player); this component only owns the panel's own visibility and button wiring —
    /// cursor state stays PlayerInteractor's responsibility, same pattern as
    /// MachineShopPanelUI/FactoryExpansionPanelUI. Unlike those two, it refreshes every
    /// frame while open (not just after a button click): capacity/usage/waiting-count change
    /// continuously as production runs, not only in response to this panel's own buttons.
    /// </summary>
    public class PowerPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text capacityText;
        [SerializeField] private Text usedText;
        [SerializeField] private Text availableText;
        [SerializeField] private Text connectedCountText;
        [SerializeField] private Text waitingCountText;
        [SerializeField] private Text balanceText;
        [SerializeField] private Text upgradeText;
        [SerializeField] private Text statsText;
        [SerializeField] private Text warningText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button closeButton;

        private PowerManager _powerManager;
        private EconomyManager _economyManager;
        private ProductionStatsTracker _statsTracker;
        private MachineController _trackedMachine;
        private Action _onClose;

        private bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (panelRoot != null) panelRoot.SetActive(false);
            if (warningText != null) warningText.enabled = false;
        }

        public void Open(PowerManager powerManager, EconomyManager economyManager,
            ProductionStatsTracker statsTracker, MachineController trackedMachine, Action onClose)
        {
            _powerManager = powerManager;
            _economyManager = economyManager;
            _statsTracker = statsTracker;
            _trackedMachine = trackedMachine;
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
                return;
            }

            Refresh();
        }

        private void OnUpgradeClicked()
        {
            if (_powerManager == null)
            {
                return;
            }

            if (_powerManager.IsUpgradePurchased)
            {
                ShowWarning("Bu yükseltme zaten satın alındı.");
                Refresh();
                return;
            }

            // (Stage 11B) Route through NetworkEconomyBridge when present — see
            // MachineShopPanelUI.OnBuyClicked for the full reasoning. Single-player keeps
            // the exact original direct call.
            NetworkEconomyBridge bridge = NetworkEconomyBridge.Instance;
            if (bridge != null)
            {
                bridge.RequestPowerUpgrade(HandleUpgradeResult);
                return;
            }

            HandleUpgradeResult(_powerManager.TryPurchaseUpgrade(), "Yetersiz bakiye!");
        }

        private void HandleUpgradeResult(bool success, string reason)
        {
            if (!success)
            {
                ShowWarning(reason);
            }
            else if (warningText != null)
            {
                warningText.enabled = false;
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
            if (_powerManager == null)
            {
                return;
            }

            if (capacityText != null) capacityText.text = $"Toplam Kapasite: {_powerManager.TotalCapacityKw:0.#} kW";
            if (usedText != null) usedText.text = $"Kullanılan: {_powerManager.UsedCapacityKw:0.#} kW";
            if (availableText != null) availableText.text = $"Boş Kapasite: {_powerManager.AvailableCapacityKw:0.#} kW";
            if (connectedCountText != null) connectedCountText.text = $"Bağlı Ekipman: {_powerManager.ConnectedConsumerCount}";
            if (waitingCountText != null) waitingCountText.text = $"Yetersiz Güç Nedeniyle Bekleyen: {_powerManager.WaitingConsumerCount}";
            if (balanceText != null && _economyManager != null) balanceText.text = $"Bakiye: {_economyManager.Balance} $";

            bool upgraded = _powerManager.IsUpgradePurchased;
            if (upgradeText != null)
            {
                upgradeText.text = upgraded
                    ? "Yükseltme: Satın Alındı"
                    : $"Yükseltme ({_powerManager.TotalCapacityKw:0.#}→{_powerManager.UpgradeTargetCapacityKw:0.#} kW): {_powerManager.UpgradePriceDollars} $";
            }

            if (upgradeButton != null)
            {
                upgradeButton.interactable = !upgraded && _economyManager != null
                    && _economyManager.Balance >= _powerManager.UpgradePriceDollars;
            }

            if (statsText != null)
            {
                int total = _statsTracker != null ? _statsTracker.TotalUnitsProduced : 0;
                string line = $"Toplam Üretilen Parça: {total}";

                if (_trackedMachine != null)
                {
                    line += $"\nAna Hat Makinesi — Üretim: {_trackedMachine.CompletedCount}, " +
                            $"Çalışma Süresi: {_trackedMachine.TotalProcessingSeconds:0} sn, " +
                            $"Güç Bekleme Süresi: {_trackedMachine.TotalPowerWaitSeconds:0} sn";
                }

                statsText.text = line;
            }
        }
    }
}
