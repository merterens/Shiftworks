using Shiftworks.Items;
using UnityEngine;

namespace Shiftworks.Orders
{
    /// <summary>
    /// A thin tag riding alongside a CarryableItem on the same GameObject. CarryableItem
    /// already handles every bit of pickup/carry/drop physics and interaction — this
    /// component only remembers which order the package belongs to, so ShippingZone can
    /// refuse anything that isn't a genuine, matching package.
    /// </summary>
    [RequireComponent(typeof(CarryableItem))]
    public class ShipmentPackage : MonoBehaviour
    {
        public OrderDefinition AssociatedOrder { get; private set; }

        public void Initialize(OrderDefinition order)
        {
            AssociatedOrder = order;
        }
    }
}
