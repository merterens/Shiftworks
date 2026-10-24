using Shiftworks.Core.Interaction;
using Shiftworks.Items;
using UnityEngine;

namespace Shiftworks.Orders
{
    /// <summary>
    /// Packs the currently active order's requested item. TryRegisterPackedItem on
    /// OrderManager is the single guarded operation — the item is only destroyed after
    /// that call succeeds, so consumption and progress can never drift apart or double-count.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PackingStation : MonoBehaviour, IInteractable
    {
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private GameObject packagePrefab;
        [SerializeField] private Transform packageSpawnPoint;

        public string InteractionPrompt => GetPromptFor(null);

        /// <summary>(Stage 11C extension) The package SpawnReadyPackage most recently
        /// created — read-only peek for NetworkItemInteractionBridge, so it can
        /// NetworkObject.Spawn it the moment a host-authoritative TryPack call completes a
        /// batch, the same "outside code only ever reads" rule as every other sync accessor
        /// (MachineController.PendingOutput, etc.). Null/stale otherwise; the bridge only
        /// ever reads it immediately after a TryPack call that just returned true.</summary>
        public ShipmentPackage LastSpawnedPackage { get; private set; }

        public bool CanInteract(IInteractor interactor) => true;

        // Real packing goes through TryPack, called directly by PlayerInteractor (same
        // pattern as MachineController/OutputStorage) since it needs to know exactly
        // what the player is carrying.
        public void Interact(IInteractor interactor)
        {
        }

        public bool TryPack(CarryableItem carried)
        {
            OrderDefinition activeOrder = orderManager != null ? orderManager.ActiveOrder : null;
            if (activeOrder == null || carried == null)
            {
                return false;
            }

            if (orderManager.GetState(activeOrder) != OrderState.Active)
            {
                return false;
            }

            if (carried.Definition == null || carried.Definition != activeOrder.RequestedItem)
            {
                return false;
            }

            if (!orderManager.TryRegisterPackedItem(activeOrder))
            {
                return false;
            }

            Destroy(carried.gameObject);

            // TryRegisterPackedItem flips the order to ReadyToShip at most once (guarded
            // by its own requested-quantity check), so this spawns the package exactly once.
            if (orderManager.GetState(activeOrder) == OrderState.ReadyToShip)
            {
                SpawnReadyPackage(activeOrder);
            }

            return true;
        }

        public string GetPromptFor(CarryableItem carriedItem)
        {
            OrderDefinition activeOrder = orderManager != null ? orderManager.ActiveOrder : null;
            if (activeOrder == null)
            {
                return "Aktif sipariş yok";
            }

            OrderState state = orderManager.GetState(activeOrder);
            int progress = orderManager.GetProgress(activeOrder);

            if (state == OrderState.ReadyToShip)
            {
                return "Paket Hazır - Sevkiyat Alanına Taşı";
            }

            string itemName = activeOrder.RequestedItem != null ? activeOrder.RequestedItem.DisplayName : "Ürün";

            if (carriedItem != null && carriedItem.Definition == activeOrder.RequestedItem)
            {
                return $"E - {itemName} Yerleştir ({progress}/{activeOrder.RequestedQuantity})";
            }

            return $"{itemName} Gerekiyor ({progress}/{activeOrder.RequestedQuantity})";
        }

        private void SpawnReadyPackage(OrderDefinition order)
        {
            if (packagePrefab == null || packageSpawnPoint == null)
            {
                Debug.LogWarning("[Shiftworks] Sevkiyat paketi oluşturulamadı: packagePrefab/packageSpawnPoint bağlı değil.");
                return;
            }

            GameObject instance = Instantiate(packagePrefab, packageSpawnPoint.position, packageSpawnPoint.rotation);
            ShipmentPackage package = instance.GetComponent<ShipmentPackage>();
            if (package == null)
            {
                Debug.LogWarning("[Shiftworks] Oluşturulan paket prefab'ında ShipmentPackage bileşeni yok.");
                return;
            }

            package.Initialize(order);
            LastSpawnedPackage = package;
            Debug.Log($"[Shiftworks] Sevkiyat paketi oluşturuldu ve '{order.OrderName}' siparişine bağlandı.");
        }
    }
}
