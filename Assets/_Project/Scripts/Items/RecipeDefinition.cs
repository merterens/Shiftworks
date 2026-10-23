using UnityEngine;

namespace Shiftworks.Items
{
    [CreateAssetMenu(fileName = "RecipeDefinition", menuName = "Shiftworks/Items/Recipe Definition")]
    public class RecipeDefinition : ScriptableObject
    {
        [SerializeField] private ItemDefinition inputItem;
        [SerializeField] private int inputCount = 1;
        [SerializeField] private ItemDefinition outputItem;
        [SerializeField] private int outputCount = 1;
        [SerializeField] private float processingTimeSeconds = 5f;

        public ItemDefinition InputItem => inputItem;
        public int InputCount => inputCount;
        public ItemDefinition OutputItem => outputItem;
        public int OutputCount => outputCount;
        public float ProcessingTimeSeconds => processingTimeSeconds;
    }
}
