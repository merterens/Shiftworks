namespace Shiftworks.Power
{
    /// <summary>Anything that draws a fixed amount of power while registered with
    /// PowerManager: MachineController (while Processing) or ConveyorSegment (continuously
    /// while enabled). A single, uniform contract keeps every power-aware system — and a
    /// future host-authoritative version of it — talking through the same
    /// Register/TryRequestPower/ReleasePower/Unregister boundary.</summary>
    public interface IPowerConsumer
    {
        float PowerDemandKw { get; }
    }
}
