using UnityEngine;

namespace Shiftworks.Factory
{
    /// <summary>
    /// Purely reactive world barrier: physically blocks a hall's doorway opening until
    /// FactoryExpansionManager reports the matching definition unlocked, then deactivates
    /// itself — collider, visual and locked label all live on this same GameObject, so one
    /// SetActive(false) opens the passage in a single step. Never writes unlock state
    /// itself, the same read-only relationship GameHUD has with OrderManager/EconomyManager.
    /// </summary>
    public class FactoryExpansionGate : MonoBehaviour
    {
        [SerializeField] private FactoryExpansionDefinition definition;
        [SerializeField] private FactoryExpansionManager expansionManager;

        private void OnEnable()
        {
            if (expansionManager == null)
            {
                return;
            }

            // Covers the case where the manager already unlocked this definition before
            // the gate enabled (e.g. after an editor domain reload) — without this a
            // stale barrier would be left standing after a successful purchase.
            if (expansionManager.IsUnlocked(definition))
            {
                gameObject.SetActive(false);
                return;
            }

            expansionManager.ExpansionUnlocked += HandleExpansionUnlocked;
        }

        private void OnDisable()
        {
            if (expansionManager != null)
            {
                expansionManager.ExpansionUnlocked -= HandleExpansionUnlocked;
            }
        }

        private void HandleExpansionUnlocked(FactoryExpansionDefinition unlocked)
        {
            if (unlocked == definition)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
