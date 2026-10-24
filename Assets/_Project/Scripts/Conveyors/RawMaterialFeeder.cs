using Shiftworks.Core.Interaction;
using Shiftworks.Items;
using UnityEngine;

namespace Shiftworks.Conveyors
{
    /// <summary>
    /// Holds a finite stock count and hands spawned instances to the first conveyor
    /// segment one at a time. Stock is only ever decremented on a confirmed hand-off, so
    /// a full downstream belt simply means the feeder's single pending item waits —
    /// never an unbounded spawn loop.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class RawMaterialFeeder : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemDefinition feedItem;
        [SerializeField] private int stock = 5;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private MonoBehaviour outputReceiverBehaviour;

        public int Stock => stock;

        /// <summary>(Stage 10) Overwrites remaining stock for a save restore. Any in-flight
        /// _pendingItem (an item already spawned from stock but not yet handed to the first
        /// conveyor — sub-second in practice) is intentionally not separately saved: stock
        /// itself is only ever decremented on a CONFIRMED hand-off, so it's still accurate
        /// without it, and Update() simply spawns a fresh pending item next frame as usual —
        /// no double-count, no loss.</summary>
        public void RestoreStock(int restoredStock)
        {
            stock = Mathf.Max(0, restoredStock);
            if (_pendingItem != null)
            {
                Destroy(_pendingItem.gameObject);
                _pendingItem = null;
            }
        }

        public string InteractionPrompt =>
            feedItem != null ? $"Besleme İstasyonu: {feedItem.DisplayName} Stok {stock}" : "Besleme İstasyonu";

        private IConveyorReceiver _outputReceiver;
        private CarryableItem _pendingItem;

        private void Awake()
        {
            _outputReceiver = outputReceiverBehaviour as IConveyorReceiver;
        }

        private void Update()
        {
            if (_pendingItem == null)
            {
                if (stock <= 0 || feedItem == null || feedItem.WorldPrefab == null || spawnPoint == null)
                {
                    return;
                }

                GameObject spawned = Instantiate(feedItem.WorldPrefab, spawnPoint.position, spawnPoint.rotation);
                _pendingItem = spawned.GetComponent<CarryableItem>();
                _pendingItem.BeginAutomatedTransport(spawnPoint.position);
            }

            if (_outputReceiver != null && _outputReceiver.TryAccept(_pendingItem))
            {
                stock--;
                _pendingItem = null;
            }
        }

        // Always lookable so the stock count can be shown; there is nothing for the
        // player to actually do here (feeding is fully automatic), so Interact is a no-op.
        public bool CanInteract(IInteractor interactor) => true;

        public void Interact(IInteractor interactor)
        {
        }
    }
}
