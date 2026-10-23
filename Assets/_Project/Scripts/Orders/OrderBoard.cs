using System;
using Shiftworks.Core.Interaction;
using Shiftworks.UI;
using UnityEngine;

namespace Shiftworks.Orders
{
    [RequireComponent(typeof(Collider))]
    public class OrderBoard : MonoBehaviour, IInteractable
    {
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private OrderDefinition order;
        [SerializeField] private OrderBoardPanelUI panelUI;

        public string InteractionPrompt => "E - Sipariş Panosunu Aç";

        public bool CanInteract(IInteractor interactor) => true;

        // Real behavior goes through OpenPanel, called directly by PlayerInteractor (same
        // pattern as MachineController/OutputStorage): opening a modal panel needs to
        // suspend player input, which only PlayerInteractor can safely coordinate.
        public void Interact(IInteractor interactor)
        {
        }

        public void OpenPanel(Action onClose)
        {
            if (panelUI != null)
            {
                panelUI.Open(orderManager, order, onClose);
            }
        }
    }
}
