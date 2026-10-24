using System.Collections.Generic;
using Shiftworks.Factory;
using Shiftworks.Items;
using Shiftworks.Machines;
using Shiftworks.Orders;
using UnityEngine;

namespace Shiftworks.SaveSystem
{
    /// <summary>
    /// Runtime ID → asset lookup for everything the save file references. A save file
    /// never embeds a UnityEngine.Object reference (JsonUtility can't round-trip one
    /// meaningfully across a session boundary) — only a stable string ID — so SaveManager
    /// needs this to resolve IDs back to the real ScriptableObject assets on load.
    ///
    /// ID source per type (chosen to need zero new fields on the definition classes
    /// themselves, wherever one already existed):
    /// - ItemDefinition: its own existing ItemId field (already designed for exactly this).
    /// - MachineDefinition / OrderDefinition / FactoryExpansionDefinition: the asset's own
    ///   Unity "name" (the .asset filename) — already unique and stable in this project,
    ///   no dedicated ID field existed, so no class needed to change.
    /// </summary>
    public class SaveCatalog : MonoBehaviour
    {
        [SerializeField] private ItemDefinition[] items;
        [SerializeField] private MachineDefinition[] machines;
        [SerializeField] private OrderDefinition[] orders;
        [SerializeField] private FactoryExpansionDefinition[] expansions;

        /// <summary>(Stage 10) FactoryExpansionManager has no list of "every expansion that
        /// exists" (it only tracks which ones got unlocked) — this is the one place that
        /// enumeration lives, needed to capture every expansion's unlocked/not-unlocked
        /// state, not just the unlocked ones.</summary>
        public IReadOnlyList<FactoryExpansionDefinition> AllExpansions => expansions;

        /// <summary>(Stage 11B) Same need as AllExpansions, now for NetworkEconomyBridge: it
        /// has to sync a pending-purchase count per MachineDefinition and needs a stable,
        /// data-driven index to do that over the network instead of inventing a second
        /// machine list.</summary>
        public IReadOnlyList<MachineDefinition> AllMachines => machines;

        public string GetItemId(ItemDefinition item) => item != null ? item.ItemId : "";
        public string GetMachineId(MachineDefinition machine) => machine != null ? machine.name : "";
        public string GetOrderId(OrderDefinition order) => order != null ? order.name : "";
        public string GetExpansionId(FactoryExpansionDefinition expansion) => expansion != null ? expansion.name : "";

        public ItemDefinition FindItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || items == null)
            {
                return null;
            }

            foreach (ItemDefinition item in items)
            {
                if (item != null && item.ItemId == itemId)
                {
                    return item;
                }
            }

            return null;
        }

        public MachineDefinition FindMachine(string machineId)
        {
            if (string.IsNullOrEmpty(machineId) || machines == null)
            {
                return null;
            }

            foreach (MachineDefinition machine in machines)
            {
                if (machine != null && machine.name == machineId)
                {
                    return machine;
                }
            }

            return null;
        }

        public OrderDefinition FindOrder(string orderId)
        {
            if (string.IsNullOrEmpty(orderId) || orders == null)
            {
                return null;
            }

            foreach (OrderDefinition order in orders)
            {
                if (order != null && order.name == orderId)
                {
                    return order;
                }
            }

            return null;
        }

        public FactoryExpansionDefinition FindExpansion(string expansionId)
        {
            if (string.IsNullOrEmpty(expansionId) || expansions == null)
            {
                return null;
            }

            foreach (FactoryExpansionDefinition expansion in expansions)
            {
                if (expansion != null && expansion.name == expansionId)
                {
                    return expansion;
                }
            }

            return null;
        }
    }
}
