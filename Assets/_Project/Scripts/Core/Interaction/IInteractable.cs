namespace Shiftworks.Core.Interaction
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }

        bool CanInteract(IInteractor interactor);

        void Interact(IInteractor interactor);
    }
}
