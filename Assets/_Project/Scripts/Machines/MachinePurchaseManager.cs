using System.Collections.Generic;
using Shiftworks.Economy;
using UnityEngine;

namespace Shiftworks.Machines
{
    /// <summary>
    /// Single authority over purchased-but-not-yet-placed machines. Money is spent exactly
    /// once, inside TryPurchase (which itself delegates to EconomyManager.TrySpend — the
    /// same single balance authority every other system already uses). TryConsumePending is
    /// a separate atomic step that only runs when a placement actually succeeds, so a
    /// cancelled placement never loses the purchase and a successful one never spends twice.
    /// This purchase/pending/consume boundary is the seam a future host-authoritative RPC
    /// would wrap.
    /// </summary>
    public class MachinePurchaseManager : MonoBehaviour
    {
        [SerializeField] private EconomyManager economyManager;

        private readonly Dictionary<MachineDefinition, int> _pendingCounts = new();

        public int GetPendingCount(MachineDefinition definition)
        {
            if (definition == null)
            {
                return 0;
            }

            return _pendingCounts.TryGetValue(definition, out int count) ? count : 0;
        }

        public bool TryPurchase(MachineDefinition definition)
        {
            if (definition == null || economyManager == null)
            {
                return false;
            }

            if (!economyManager.TrySpend(definition.Price))
            {
                return false;
            }

            int newCount = GetPendingCount(definition) + 1;
            _pendingCounts[definition] = newCount;
            Debug.Log($"[Shiftworks] Makine satın alındı: '{definition.MachineName}' (-{definition.Price} $). Bekleyen: {newCount}.");
            return true;
        }

        /// <summary>(Stage 11B) Directly overwrites a pending count for network sync on
        /// non-host clients — the host's MachinePurchaseManager already ran the real
        /// TryPurchase/TryConsumePending; this just mirrors the resulting shared count
        /// without spending money or consuming a pending slot a second time. Same
        /// "RestoreX" exception pattern as EconomyManager.RestoreBalance.</summary>
        public void RestorePendingCount(MachineDefinition definition, int count)
        {
            if (definition == null)
            {
                return;
            }

            _pendingCounts[definition] = Mathf.Max(0, count);
        }

        public bool TryConsumePending(MachineDefinition definition)
        {
            int count = GetPendingCount(definition);
            if (count <= 0)
            {
                return false;
            }

            _pendingCounts[definition] = count - 1;
            Debug.Log($"[Shiftworks] Bekleyen makine yerleştirildi: '{definition.MachineName}'. Kalan bekleyen: {count - 1}.");
            return true;
        }
    }
}
