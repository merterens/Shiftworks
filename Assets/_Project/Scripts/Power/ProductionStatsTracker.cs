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

        /// <summary>(Stage 10) Overwrites the total for a save restore — the counters from
        /// the saved session are re-applied once, not replayed event-by-event, so loading
        /// can never double-count a unit that was already counted when it was first produced.</summary>
        public void RestoreTotal(int totalUnitsProduced)
        {
            TotalUnitsProduced = Mathf.Max(0, totalUnitsProduced);
        }
    }
}
