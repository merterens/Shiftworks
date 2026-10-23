using Shiftworks.Items;

namespace Shiftworks.Conveyors
{
    /// <summary>
    /// Anything that can accept a handed-off item: another ConveyorSegment, a
    /// MachineController's input, or OutputStorage. A single, uniform hand-off contract
    /// keeps every stage of the line (and the future host-authoritative version of it)
    /// talking through the same atomic "can I take this right now" boundary.
    /// </summary>
    public interface IConveyorReceiver
    {
        bool TryAccept(CarryableItem item);
    }
}
