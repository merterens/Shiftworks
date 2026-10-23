using UnityEngine;

namespace Shiftworks.Factory
{
    /// <summary>Purchasable factory hall. Price and display size are data, never hard-coded
    /// in code — a future "Üretim Alanı C/D" is just a new asset, not a new class.</summary>
    [CreateAssetMenu(fileName = "FactoryExpansionDefinition", menuName = "Shiftworks/Factory/Factory Expansion Definition")]
    public class FactoryExpansionDefinition : ScriptableObject
    {
        [SerializeField] private string expansionName;
        [SerializeField] private int price;
        [SerializeField] private Vector2 sizeMeters;

        public string ExpansionName => expansionName;
        public int Price => price;

        /// <summary>Display-only (x = width, y = depth); the scene geometry is hand-built
        /// per hall, not generated from this value.</summary>
        public Vector2 SizeMeters => sizeMeters;
    }
}
