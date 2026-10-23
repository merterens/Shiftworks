using Shiftworks.Economy;
using Shiftworks.Orders;
using UnityEngine;
using UnityEngine.UI;

namespace Shiftworks.UI
{
    /// <summary>
    /// Persistent, always-on-screen readout. Purely reactive: polls EconomyManager and
    /// OrderManager's existing public state every frame and writes a few Text fields —
    /// it never mutates either manager, so it cannot affect correctness of the economy
    /// or order flow.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private OrderDefinition trackedOrder;

        [SerializeField] private Text balanceText;
        [SerializeField] private Text orderText;
        [SerializeField] private Text notificationText;
        [SerializeField] private float notificationDuration = 4f;

        private OrderState _lastSeenState = OrderState.Available;
        private float _notificationTimer;
        private string _notificationMessage = "";

        private void Update()
        {
            if (economyManager != null && balanceText != null)
            {
                balanceText.text = $"Bakiye: {economyManager.Balance} $";
            }

            UpdateOrderText();
            UpdateNotification();
        }

        private void UpdateOrderText()
        {
            if (orderManager == null || trackedOrder == null || orderText == null)
            {
                return;
            }

            OrderState state = orderManager.GetState(trackedOrder);

            orderText.text = state switch
            {
                OrderState.Available => "Aktif sipariş yok",
                OrderState.Completed => $"{trackedOrder.OrderName}: Tamamlandı",
                _ => $"{trackedOrder.OrderName}: {orderManager.GetProgress(trackedOrder)}/{trackedOrder.RequestedQuantity}",
            };

            if (state == OrderState.Completed && _lastSeenState != OrderState.Completed)
            {
                int reward = trackedOrder != null ? trackedOrder.Reward : 0;
                ShowNotification($"Sipariş Tamamlandı! +{reward} $");
            }

            _lastSeenState = state;
        }

        /// <summary>Generic notification slot other systems (e.g. factory expansion
        /// purchases) can reuse instead of duplicating their own toast/timer logic.</summary>
        public void ShowNotification(string message)
        {
            _notificationMessage = message;
            _notificationTimer = notificationDuration;
        }

        private void UpdateNotification()
        {
            if (notificationText == null)
            {
                return;
            }

            if (_notificationTimer > 0f)
            {
                _notificationTimer -= Time.deltaTime;
                notificationText.enabled = true;
                notificationText.text = _notificationMessage;
            }
            else
            {
                notificationText.enabled = false;
            }
        }
    }
}
