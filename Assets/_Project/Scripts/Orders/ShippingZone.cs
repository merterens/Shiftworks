using Shiftworks.Core.Interaction;
using Shiftworks.Items;
using UnityEngine;

namespace Shiftworks.Orders
{
    /// <summary>
    /// Only ever completes an order for a genuine ShipmentPackage whose AssociatedOrder is
    /// currently ReadyToShip. A plain CarryableItem (no ShipmentPackage component) or a
    /// package for the wrong/finished order is rejected outright — nothing is consumed.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ShippingZone : MonoBehaviour, IInteractable
    {
        [SerializeField] private OrderManager orderManager;

        public string InteractionPrompt => GetPromptFor(null);

        public bool CanInteract(IInteractor interactor) => true;

        // Real delivery goes through TryDeliver, called directly by PlayerInteractor (same
        // pattern as the other stations) since it needs to know exactly what is carried.
        public void Interact(IInteractor interactor)
        {
        }

        public bool TryDeliver(CarryableItem carried)
        {
            if (carried == null)
            {
                return false;
            }

            ShipmentPackage package = carried.GetComponent<ShipmentPackage>();
            if (package == null || package.AssociatedOrder == null)
            {
                Debug.Log($"[Shiftworks] Teslimat reddedildi: '{carried.name}' bir sevkiyat paketi değil.");
                return false;
            }

            OrderState state = orderManager.GetState(package.AssociatedOrder);
            if (state != OrderState.ReadyToShip)
            {
                Debug.Log($"[Shiftworks] Teslimat reddedildi: '{package.AssociatedOrder.OrderName}' durumu {state}, ReadyToShip değil.");
                return false;
            }

            if (!orderManager.TryCompleteOrder(package.AssociatedOrder))
            {
                Debug.LogWarning($"[Shiftworks] Teslimat reddedildi: '{package.AssociatedOrder.OrderName}' ödülü zaten alınmış.");
                return false;
            }

            Debug.Log($"[Shiftworks] Sipariş teslim edildi: '{package.AssociatedOrder.OrderName}' (+{package.AssociatedOrder.Reward} $).");
            Destroy(carried.gameObject);
            return true;
        }

        public string GetPromptFor(CarryableItem carriedItem)
        {
            if (carriedItem == null)
            {
                return "Sevkiyat Alanı";
            }

            ShipmentPackage package = carriedItem.GetComponent<ShipmentPackage>();
            if (package == null)
            {
                return "Bu bir sevkiyat paketi değil";
            }

            if (package.AssociatedOrder == null || orderManager == null)
            {
                return "Geçersiz paket";
            }

            OrderState state = orderManager.GetState(package.AssociatedOrder);
            switch (state)
            {
                case OrderState.ReadyToShip:
                    return "E - Siparişi Teslim Et";
                case OrderState.Completed:
                    return "Bu sipariş zaten teslim edildi";
                default:
                    return "Bu paket henüz sevkiyata hazır değil";
            }
        }
    }
}
