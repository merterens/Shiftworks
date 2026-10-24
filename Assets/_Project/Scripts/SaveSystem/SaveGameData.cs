using System;
using System.Collections.Generic;

namespace Shiftworks.SaveSystem
{
    /// <summary>
    /// The entire save file, as plain serializable data — never a MonoBehaviour,
    /// ScriptableObject or UnityEngine.Object reference anywhere in this tree (JsonUtility
    /// can't serialize those meaningfully, and they wouldn't survive a session boundary
    /// anyway). Every reference to a game asset (an item, a machine type, an order, an
    /// expansion) is a stable string ID resolved back to the real asset through SaveCatalog
    /// on load — see SaveCatalog's own doc comment for exactly which ID each type uses.
    /// </summary>
    [Serializable]
    public class SaveGameData
    {
        public int saveVersion;
        public string savedAtUtc;

        public EconomySaveData economy = new();
        public PowerSaveData power = new();
        public List<FactoryExpansionSaveData> factoryExpansions = new();
        public List<OrderSaveData> orders = new();
        public List<MachineSaveData> machines = new();
        public List<ConveyorSaveData> conveyors = new();
        public List<StorageSaveData> storages = new();
        public List<FeederSaveData> feeders = new();
        public List<WorldItemSaveData> worldItems = new();
        public List<ShipmentPackageSaveData> shipmentPackages = new();
        public PlayerSaveData player = new();
        public StatsSaveData stats = new();
    }

    [Serializable]
    public class EconomySaveData
    {
        public int balance;
    }

    [Serializable]
    public class PowerSaveData
    {
        public bool upgradePurchased;
    }

    [Serializable]
    public class FactoryExpansionSaveData
    {
        public string expansionId;
        public bool unlocked;
    }

    [Serializable]
    public class OrderSaveData
    {
        public string orderId;
        public string state; // OrderState as a string — readable/debuggable in the JSON file
        public int progressCount;
        public bool rewardClaimed;
    }

    [Serializable]
    public class MachineSaveData
    {
        /// <summary>Non-empty only for the scene-baked original (matched back to it by
        /// GameObject name on load, e.g. "CuttingMachine") — mutually exclusive with
        /// instanceId/machineDefinitionId below.</summary>
        public string sceneObjectName;

        /// <summary>Non-empty only for a player-placed machine — a stable GUID assigned once
        /// at placement time (MachinePlacementController), not reused by anything else.</summary>
        public string instanceId;
        public string machineDefinitionId;

        public float posX, posY, posZ;
        public float rotY;

        public string state; // MachineState as a string
        public int bufferedInputCount;
        public float processingElapsedSeconds;
        public bool hasPendingOutput;
        public int completedCount;
        public float totalProcessingSeconds;
        public float totalPowerWaitSeconds;
    }

    [Serializable]
    public class ConveyorSaveData
    {
        public string sceneObjectName;
        /// <summary>Empty string = unoccupied. See SaveManager's class doc for why an
        /// occupied segment restores to the START of its belt rather than its exact
        /// mid-transit position.</summary>
        public string occupantItemId;
    }

    [Serializable]
    public class StorageSaveData
    {
        public string sceneObjectName;
        public int storedCount;
    }

    [Serializable]
    public class FeederSaveData
    {
        public string sceneObjectName;
        public int stock;
    }

    /// <summary>A loose, trackable item (MetalSheet / CutMetalPart / AssembledComponent)
    /// lying in the world — not held, not mid-transit on a conveyor/feeder, not stored.
    /// Rotation is a full quaternion (not just yaw) since a dropped/thrown item can land at
    /// any orientation, unlike a placed machine.</summary>
    [Serializable]
    public class WorldItemSaveData
    {
        public string itemId;
        public float posX, posY, posZ;
        public float rotX, rotY, rotZ, rotW;
    }

    [Serializable]
    public class ShipmentPackageSaveData
    {
        public string orderId;
        public float posX, posY, posZ;
        public float rotX, rotY, rotZ, rotW;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public float posX, posY, posZ;
        public float yaw;
        public float pitch;

        /// <summary>At most one of these two is non-empty — a plain item vs. a shipment
        /// package need different reconstruction (the package also needs its order link
        /// restored), so they're kept as separate, mutually-exclusive fields rather than
        /// one ambiguous "carriedItemId".</summary>
        public string carriedItemId;
        public string carriedPackageOrderId;
    }

    [Serializable]
    public class StatsSaveData
    {
        public int totalUnitsProduced;
    }
}
