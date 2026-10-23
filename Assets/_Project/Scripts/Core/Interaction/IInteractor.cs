using UnityEngine;

namespace Shiftworks.Core.Interaction
{
    /// <summary>
    /// Anything that can perform interactions on an IInteractable (currently only the
    /// local player). Kept abstract so IInteractable implementations never depend on
    /// concrete player types.
    /// </summary>
    public interface IInteractor
    {
        Transform HoldPoint { get; }
        Collider BodyCollider { get; }
    }
}
