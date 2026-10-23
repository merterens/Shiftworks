using Shiftworks.Items;
using UnityEngine;

namespace Shiftworks.Machines
{
    /// <summary>Purchasable machine type. Price and prefab are data, never hard-coded in code.</summary>
    [CreateAssetMenu(fileName = "MachineDefinition", menuName = "Shiftworks/Machines/Machine Definition")]
    public class MachineDefinition : ScriptableObject
    {
        [SerializeField] private string machineName;
        [SerializeField] private int price;
        [SerializeField] private RecipeDefinition recipe;
        [SerializeField] private GameObject machinePrefab;

        public string MachineName => machineName;
        public int Price => price;
        public RecipeDefinition Recipe => recipe;

        /// <summary>Prefab instantiated on a successful placement. Already carries its own
        /// configured MachineController wired to Recipe — placing one is just Instantiate.</summary>
        public GameObject MachinePrefab => machinePrefab;
    }
}
