using System;
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
    /// </summary>
    public class OrderBoardPanelUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text orderNameText;
        [SerializeField] private Text itemText;
        [SerializeField] private Text quantityText;
        [SerializeField] private Text rewardText;
        [SerializeField] private Text stateText;
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
            if (_orderManager != null && _order != null)
            {
                _orderManager.TryAcceptOrder(_order);
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
                acceptButton.interactable = state == OrderState.Available;
            }
        }
    }
}
