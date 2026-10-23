using System;
using Shiftworks.Core.Interaction;
using Shiftworks.Economy;
using Shiftworks.UI;
using UnityEngine;

namespace Shiftworks.Machines
{
    [RequireComponent(typeof(Collider))]
    public class MachineShopTerminal : MonoBehaviour, IInteractable
    {
        [SerializeField] private MachineDefinition machineDefinition;
        [SerializeField] private MachinePurchaseManager purchaseManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private MachineShopPanelUI panelUI;

        public MachineDefinition Definition => machineDefinition;
        public MachinePurchaseManager PurchaseManager => purchaseManager;

        public string InteractionPrompt => "E - Makine Mağazasını Aç";

        public bool CanInteract(IInteractor interactor) => true;

        // Generic contract fulfilled for interface compliance. Real behavior goes through
        // OpenPanel, called directly by PlayerInteractor (same pattern as OrderBoard) since
        // opening a modal panel needs to suspend player input, which only PlayerInteractor
        // can safely coordinate.
        public void Interact(IInteractor interactor)
        {
        }

        public void OpenPanel(Action onClose, Action onBeginPlacement)
        {
            if (panelUI != null)
            {
                panelUI.Open(purchaseManager, economyManager, machineDefinition, onClose, onBeginPlacement);
            }
        }
    }
}
