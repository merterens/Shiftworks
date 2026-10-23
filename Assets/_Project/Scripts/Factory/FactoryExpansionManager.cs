using System;
using System.Collections.Generic;
using Shiftworks.Economy;
using UnityEngine;

namespace Shiftworks.Factory
{
    /// <summary>
    /// Single authority over which factory expansions are unlocked. TryPurchase is one
    /// atomic method: it checks the expansion isn't already unlocked, spends through
    /// EconomyManager.TrySpend (the same single balance authority every other system
    /// uses), and only then marks it unlocked and raises ExpansionUnlocked — so a failed
    /// spend never unlocks, and clicking Buy twice never spends twice (the second call
    /// is rejected by the already-unlocked check before it can touch the economy).
    /// Gates (world doorways) only react to ExpansionUnlocked; they never write unlock
    /// state themselves. This purchase boundary is the seam a future host-authoritative
    /// RPC would wrap, same pattern as MachinePurchaseManager.TryPurchase.
    /// </summary>
    public class FactoryExpansionManager : MonoBehaviour
    {
        [SerializeField] private EconomyManager economyManager;

        public event Action<FactoryExpansionDefinition> ExpansionUnlocked;

        private readonly HashSet<FactoryExpansionDefinition> _unlocked = new();

        public bool IsUnlocked(FactoryExpansionDefinition definition)
        {
            return definition != null && _unlocked.Contains(definition);
        }

        public bool TryPurchase(FactoryExpansionDefinition definition)
        {
            if (definition == null || economyManager == null || IsUnlocked(definition))
            {
                return false;
            }

            if (!economyManager.TrySpend(definition.Price))
            {
                return false;
            }

            _unlocked.Add(definition);
            Debug.Log($"[Shiftworks] Üretim alanı satın alındı: '{definition.ExpansionName}' (-{definition.Price} $).");
            ExpansionUnlocked?.Invoke(definition);
            return true;
        }
    }
}
