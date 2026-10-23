using UnityEngine;

namespace Shiftworks.Items
{
    public enum ItemCategory
    {
        RawMaterial,
        ProcessedPart
    }

    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "Shiftworks/Items/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] private ItemCategory category;
        [SerializeField] private Color visualColor = Color.white;
        [SerializeField] private GameObject worldPrefab;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public ItemCategory Category => category;
        public Color VisualColor => visualColor;

        /// <summary>Prefab used to spawn a physical, carryable instance of this item in the world.</summary>
        public GameObject WorldPrefab => worldPrefab;
    }
}
