using UnityEngine;

namespace Shiftworks.Power
{
    /// <summary>
    /// Single authority for the factory-wide completed-unit counter. Only ever incremented
    /// by MachineController.CompleteProcessing() — exactly once per finished production
    /// cycle, never on player pickup/packing — so the count can't be inflated by collecting
    /// or re-handling the same output. Per-machine counts/timings live on each
    /// MachineController instance itself (it is the single authority for its own history);
    /// this tracker only owns the factory-wide total.
    /// </summary>
    public class ProductionStatsTracker : MonoBehaviour
    {
        public int TotalUnitsProduced { get; private set; }

        public void RecordUnitCompleted()
        {
            TotalUnitsProduced++;
        }
    }
}
