using System;
using Shiftworks.Multiplayer;
using Shiftworks.Orders;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Shiftworks.UI
{
    /// <summary>
    /// Modal order panel. Opening/closing is entirely driven by whoever calls Open() (the
    /// Player); this component only owns the panel's own visibility and button wiring, not
    /// cursor state — that stays PlayerInteractor's responsibility so there is exactly one
    /// place deciding when the player can look/move.
    ///
    /// (Stage 9 fix) OrderManager only ever allows one Active/ReadyToShip order at a time
    /// (HasActiveOrder), but this board previously only checked ITS OWN order's state
    /// (Available) to decide whether Accept was clickable — with a second order board now in
    /// the scene, clicking Accept while a DIFFERENT order was already active silently did
    /// nothing (TryAcceptOrder correctly refused, but nothing on screen said why). The button
    /// and a dedicated warning line now also read HasActiveOrder, so this is visible instead
    /// of a silent dead end.
    /// </summary>
    public class OrderBoardPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text orderNameText;
        [SerializeField] private Text itemText;
        [SerializeField] private Text quantityText;
        [SerializeField] private Text rewardText;
        [SerializeField] private Text stateText;
        [SerializeField] private Text warningText;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button closeButton;

        private OrderManager _orderManager;
        private OrderDefinition _order;
        private Action _onClose;

        private bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            if (acceptButton != null)
            {
                acceptButton.onClick.AddListener(OnAcceptClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        public void Open(OrderManager orderManager, OrderDefinition order, Action onClose)
        {
            _orderManager = orderManager;
            _order = order;
            _onClose = onClose;

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            if (warningText != null)
            {
                warningText.enabled = false;
            }

            Refresh();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

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

        private void OnAcceptClicked()
        {
            if (_orderManager == null || _order == null)
            {
                return;
            }

            // (Stage 11B) Route through NetworkEconomyBridge when present — see
            // MachineShopPanelUI.OnBuyClicked for the full reasoning. Single-player keeps
            // the exact original direct call.
            NetworkEconomyBridge bridge = NetworkEconomyBridge.Instance;
            if (bridge != null)
            {
                bridge.RequestOrderAccept(_order, HandleAcceptResult);
                return;
            }

            HandleAcceptResult(_orderManager.TryAcceptOrder(_order),
                "Sipariş kabul edilemedi — başka bir sipariş zaten aktif olabilir.");
        }

        private void HandleAcceptResult(bool success, string reason)
        {
            if (!success && warningText != null)
            {
                // Defensive: Refresh()'s own HasActiveOrder check already disables the button
                // for this exact case, so a click normally can't reach here — but if it ever
                // does (e.g. a stale interactable state), the player still sees why nothing
                // happened instead of a silent no-op.
                warningText.enabled = true;
                warningText.text = reason;
            }

            Refresh();
        }

        private void Refresh()
        {
            if (_order == null || _orderManager == null)
            {
                return;
            }

            OrderState state = _orderManager.GetState(_order);
            bool blockedByOtherActiveOrder = state == OrderState.Available && _orderManager.HasActiveOrder;

            if (orderNameText != null) orderNameText.text = _order.OrderName;
            if (itemText != null) itemText.text = _order.RequestedItem != null ? _order.RequestedItem.DisplayName : "-";
            if (quantityText != null) quantityText.text = $"{_orderManager.GetProgress(_order)} / {_order.RequestedQuantity}";
            if (rewardText != null) rewardText.text = $"$ {_order.Reward}";

            if (stateText != null)
            {
                stateText.text = state switch
                {
                    OrderState.Available => "Müsait",
                    OrderState.Active => "Devam Ediyor",
                    OrderState.ReadyToShip => "Sevkiyata Hazır",
                    OrderState.Completed => "Tamamlandı",
                    _ => state.ToString(),
                };
            }

            if (acceptButton != null)
            {
                // Available is necessary but not sufficient: OrderManager only ever allows
                // one Active/ReadyToShip order factory-wide (HasActiveOrder), so this board's
                // own Accept must stay disabled while a DIFFERENT order already holds that slot.
                acceptButton.interactable = state == OrderState.Available && !_orderManager.HasActiveOrder;
            }

            if (warningText != null)
            {
                if (blockedByOtherActiveOrder)
                {
                    warningText.enabled = true;
                    warningText.text = "Başka bir sipariş zaten aktif — önce onu tamamlayıp teslim edin.";
                }
                else
                {
                    warningText.enabled = false;
                }
            }
        }
    }
}
