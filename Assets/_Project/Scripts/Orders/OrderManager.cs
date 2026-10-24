using System.Collections.Generic;
using Shiftworks.Economy;
using UnityEngine;

namespace Shiftworks.Orders
{
    /// <summary>
    /// Single authority over every order's state. Acceptance, packing progress and
    /// completion each go through one guarded method keyed off the current state, so
    /// repeated calls (double E-presses, a race between manual and automatic flows)
    /// can only ever succeed once per real transition — the same boundary a future
    /// host-authoritative RPC would wrap. Designed to hold several orders at once; this
    /// stage only ever configures one.
    /// </summary>
    public class OrderManager : MonoBehaviour
    {
        [SerializeField] private List<OrderDefinition> availableOrders = new();
        [SerializeField] private EconomyManager economyManager;

        private readonly List<OrderRuntime> _orders = new();

        private void Awake()
        {
            foreach (OrderDefinition definition in availableOrders)
            {
                if (definition != null)
                {
                    _orders.Add(new OrderRuntime(definition));
                }
            }
        }

        public OrderState GetState(OrderDefinition order) => Find(order)?.State ?? OrderState.Available;

        public int GetProgress(OrderDefinition order) => Find(order)?.ProgressCount ?? 0;

        public bool IsRewardClaimed(OrderDefinition order) => Find(order)?.RewardClaimed ?? false;

        public bool HasActiveOrder => FindActive() != null;

        public OrderDefinition ActiveOrder => FindActive()?.Definition;

        /// <summary>(Stage 10) Read-only list of every order this manager tracks, in the
        /// same order as the Inspector-configured availableOrders — SaveManager needs this
        /// to enumerate and save every order's state, not just the currently active one.</summary>
        public IReadOnlyList<OrderDefinition> AvailableOrders => availableOrders;

        public bool TryAcceptOrder(OrderDefinition order)
        {
            OrderRuntime runtime = Find(order);
            if (runtime == null || runtime.State != OrderState.Available || HasActiveOrder)
            {
                return false;
            }

            runtime.State = OrderState.Active;
            runtime.ProgressCount = 0;
            Debug.Log($"[Shiftworks] Sipariş kabul edildi: '{order.OrderName}' (Available -> Active).");
            return true;
        }

        /// <summary>Called once per successfully packed item. Returns false (no change)
        /// once the order is already full or not Active, so a station can never count
        /// more items than requested or count against a finished order.</summary>
        public bool TryRegisterPackedItem(OrderDefinition order)
        {
            OrderRuntime runtime = Find(order);
            if (runtime == null || runtime.State != OrderState.Active)
            {
                return false;
            }

            if (runtime.ProgressCount >= runtime.Definition.RequestedQuantity)
            {
                return false;
            }

            runtime.ProgressCount++;
            Debug.Log($"[Shiftworks] '{order.OrderName}' ilerlemesi: {runtime.ProgressCount}/{runtime.Definition.RequestedQuantity}.");

            if (runtime.ProgressCount >= runtime.Definition.RequestedQuantity)
            {
                runtime.State = OrderState.ReadyToShip;
                Debug.Log($"[Shiftworks] Sipariş sevkiyata hazır: '{order.OrderName}' (Active -> ReadyToShip).");
            }

            return true;
        }

        /// <summary>Called once by a successful shipment delivery. The RewardClaimed flag
        /// plus the State no longer being ReadyToShip afterwards double-guard against
        /// ever granting the reward twice for the same order.</summary>
        public bool TryCompleteOrder(OrderDefinition order)
        {
            OrderRuntime runtime = Find(order);
            if (runtime == null || runtime.State != OrderState.ReadyToShip || runtime.RewardClaimed)
            {
                return false;
            }

            runtime.RewardClaimed = true;
            runtime.State = OrderState.Completed;

            if (economyManager != null)
            {
                economyManager.AddFunds(runtime.Definition.Reward);
            }

            Debug.Log($"[Shiftworks] Sipariş tamamlandı: '{order.OrderName}' (ReadyToShip -> Completed), +{runtime.Definition.Reward} $.");
            return true;
        }

        /// <summary>(Stage 10) Controlled restore entry point — sets an order's state
        /// directly instead of replaying TryAcceptOrder/TryRegisterPackedItem/TryCompleteOrder,
        /// since nothing is actually being accepted/packed/delivered right now, only
        /// re-applied from a save. Deliberately bypasses HasActiveOrder/RewardClaimed guards
        /// (restoring the one order that WAS active, or a Completed order's already-claimed
        /// flag, is exactly what those guards exist to protect) — never reachable from normal
        /// gameplay, only from SaveManager.</summary>
        public void RestoreOrderState(OrderDefinition order, OrderState state, int progressCount, bool rewardClaimed)
        {
            OrderRuntime runtime = Find(order);
            if (runtime == null)
            {
                return;
            }

            runtime.State = state;
            runtime.ProgressCount = progressCount;
            runtime.RewardClaimed = rewardClaimed;
        }

        private OrderRuntime Find(OrderDefinition order)
        {
            if (order == null)
            {
                return null;
            }

            foreach (OrderRuntime runtime in _orders)
            {
                if (runtime.Definition == order)
                {
                    return runtime;
                }
            }

            return null;
        }

        private OrderRuntime FindActive()
        {
            foreach (OrderRuntime runtime in _orders)
            {
                if (runtime.State == OrderState.Active || runtime.State == OrderState.ReadyToShip)
                {
                    return runtime;
                }
            }

            return null;
        }

        private class OrderRuntime
        {
            public readonly OrderDefinition Definition;
            public OrderState State = OrderState.Available;
            public int ProgressCount;
            public bool RewardClaimed;

            public OrderRuntime(OrderDefinition definition)
            {
                Definition = definition;
            }
        }
    }
}
