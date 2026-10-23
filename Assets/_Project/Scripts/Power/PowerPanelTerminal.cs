using System;
using Shiftworks.Core.Interaction;
using Shiftworks.Economy;
using Shiftworks.Machines;
using Shiftworks.UI;
using UnityEngine;

namespace Shiftworks.Power
{
    [RequireComponent(typeof(Collider))]
    public class PowerPanelTerminal : MonoBehaviour, IInteractable
    {
        [SerializeField] private PowerManager powerManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private ProductionStatsTracker statsTracker;
        [SerializeField] private MachineController trackedMachine;
        [SerializeField] private PowerPanelUI panelUI;

        public string InteractionPrompt => "E - Elektrik Panosunu Aç";

        public bool CanInteract(IInteractor interactor) => true;

        // Generic contract fulfilled for interface compliance. Real behavior goes through
        // OpenPanel, called directly by PlayerInteractor (same pattern as MachineShopTerminal
        // / OrderBoard / FactoryExpansionTerminal) since opening a modal panel needs to
        // suspend player input, which only PlayerInteractor can safely coordinate.
        public void Interact(IInteractor interactor)
        {
        }

        public void OpenPanel(Action onClose)
        {
            if (panelUI != null)
            {
                panelUI.Open(powerManager, economyManager, statsTracker, trackedMachine, onClose);
            }
        }
    }
}
