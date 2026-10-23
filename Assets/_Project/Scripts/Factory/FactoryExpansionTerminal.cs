using System;
using Shiftworks.Core.Interaction;
using Shiftworks.Economy;
using Shiftworks.UI;
using UnityEngine;

namespace Shiftworks.Factory
{
    [RequireComponent(typeof(Collider))]
    public class FactoryExpansionTerminal : MonoBehaviour, IInteractable
    {
        [SerializeField] private FactoryExpansionDefinition expansionDefinition;
        [SerializeField] private FactoryExpansionManager expansionManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private FactoryExpansionPanelUI panelUI;

        public string InteractionPrompt =>
            expansionManager != null && expansionManager.IsUnlocked(expansionDefinition)
                ? "Üretim Alanı B (Satın Alındı)"
                : "E - Üretim Alanı Genişletmesini Görüntüle";

        public bool CanInteract(IInteractor interactor) => true;

        // Generic contract fulfilled for interface compliance. Real behavior goes through
        // OpenPanel, called directly by PlayerInteractor (same pattern as MachineShopTerminal
        // / OrderBoard) since opening a modal panel needs to suspend player input, which only
        // PlayerInteractor can safely coordinate.
        public void Interact(IInteractor interactor)
        {
        }

        public void OpenPanel(Action onClose)
        {
            if (panelUI != null)
            {
                panelUI.Open(expansionManager, economyManager, expansionDefinition, onClose);
            }
        }
    }
}
