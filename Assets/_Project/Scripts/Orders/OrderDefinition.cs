using Shiftworks.Items;
using UnityEngine;

namespace Shiftworks.Orders
{
    [CreateAssetMenu(fileName = "OrderDefinition", menuName = "Shiftworks/Orders/Order Definition")]
    public class OrderDefinition : ScriptableObject
    {
        [SerializeField] private string orderName;
        [SerializeField] private ItemDefinition requestedItem;
        [SerializeField] private int requestedQuantity = 1;
        [SerializeField] private int reward = 100;

        // Reserved for a future stage (reputation/expiry). Not read or enforced yet.
        [SerializeField] private float timeLimitSeconds;

        public string OrderName => orderName;
        public ItemDefinition RequestedItem => requestedItem;
        public int RequestedQuantity => requestedQuantity;
        public int Reward => reward;
        public float TimeLimitSeconds => timeLimitSeconds;
    }
}
