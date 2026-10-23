using Shiftworks.Core.Interaction;
using Shiftworks.Items;
using UnityEngine;

namespace Shiftworks.Conveyors
{
    /// <summary>
    /// Terminal receiver for the line. Accepted items are destroyed immediately and
    /// tracked only as a count — the single source of truth for "how many are stored"
    /// is this int, never a pile of lingering GameObjects. Withdrawing spawns a fresh
    /// instance on demand and hands it to the player via CarryableItem's own pickup
    /// path, the same reuse pattern already used by MachineController's output collection.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class OutputStorage : MonoBehaviour, IConveyorReceiver, IInteractable
    {
        [SerializeField] private ItemDefinition acceptedItem;
        [SerializeField] private int capacity = 10;
        [SerializeField] private Transform pickupSpawnPoint;

        public int StoredCount { get; private set; }
        public bool IsFull => StoredCount >= capacity;

        public string InteractionPrompt =>
            StoredCount > 0
                ? $"E - {acceptedItem.DisplayName} Al ({StoredCount}/{capacity})"
                : $"Depo Boş (0/{capacity})";

        public bool TryAccept(CarryableItem item)
        {
            if (item == null || acceptedItem == null || item.Definition != acceptedItem || IsFull)
            {
                return false;
            }

            Destroy(item.gameObject);
            StoredCount++;
            return true;
        }

        public bool TryWithdraw(IInteractor interactor, out CarryableItem output)
        {
            output = null;

            if (StoredCount <= 0 || acceptedItem == null || acceptedItem.WorldPrefab == null || pickupSpawnPoint == null)
            {
                return false;
            }

            GameObject spawned = Instantiate(acceptedItem.WorldPrefab, pickupSpawnPoint.position, pickupSpawnPoint.rotation);
            output = spawned.GetComponent<CarryableItem>();
            if (output == null)
            {
                Destroy(spawned);
                return false;
            }

            output.Interact(interactor);
            StoredCount--;
            return true;
        }

        public bool CanInteract(IInteractor interactor) => true;

        // Real withdrawal goes through TryWithdraw (called directly by PlayerInteractor,
        // same pattern as MachineController) so the caller gets the resulting
        // CarryableItem back and can track it as the carried item.
        public void Interact(IInteractor interactor)
        {
        }
    }
}
