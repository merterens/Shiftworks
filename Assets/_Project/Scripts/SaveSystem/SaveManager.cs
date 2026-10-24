using System;
using System.Collections.Generic;
using Shiftworks.Conveyors;
using Shiftworks.Economy;
using Shiftworks.Factory;
using Shiftworks.Items;
using Shiftworks.Machines;
using Shiftworks.Orders;
using Shiftworks.Player;
using Shiftworks.Power;
using UnityEngine;

namespace Shiftworks.SaveSystem
{
    /// <summary>
    /// Single orchestrator for save/load — every other manager stays the single authority
    /// over its OWN state (EconomyManager still owns Balance, OrderManager still owns order
    /// state, etc.); this class only ever calls their existing or Stage-10-added
    /// Capture*/Restore* entry points, never reaches into their private fields. That split
    /// is deliberate: the same boundary that lets a future host-authoritative server validate
    /// every live operation is exactly what lets this capture/restore the same state offline.
    ///
    /// Capture walks the scene (FindObjectsByType + a few fixed, already-established scene
    /// object names — "CuttingMachine", "Conveyor_In_1", etc., the same names every Editor
    /// setup tool in this project already hardcodes) rather than keeping its own tracked
    /// list, so it can never drift from what's actually in the world.
    ///
    /// Restore distinguishes "scene-baked original" objects (matched back by GameObject
    /// name — already present after a scene reload, only their STATE needs restoring) from
    /// "player-placed/produced" objects (a purchased machine, a dropped part, a packed
    /// shipment — re-created fresh from save data). ClearDynamicState() destroys every
    /// player-placed/produced object before restoring, so loading mid-session (not just at
    /// a fresh boot) can never end up with duplicates.
    ///
    /// Known, documented simplification: an item mid-transit on a conveyor restores to the
    /// START of its current segment rather than its exact interpolated position — exact
    /// mid-belt position isn't tracked anywhere else in the codebase (ConveyorSegment only
    /// ever computes it from a live 0..1 progress float against two Transforms), so
    /// reproducing it losslessly would mean inventing new persistent state nothing else
    /// uses. This is deterministic and never loses/duplicates the item, just visually
    /// resets its progress along that one segment.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public const int CurrentSaveVersion = 1;

        [Header("System references")]
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private PowerManager powerManager;
        [SerializeField] private FactoryExpansionManager factoryExpansionManager;
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private ProductionStatsTracker statsTracker;
        [SerializeField] private SaveCatalog catalog;
        [SerializeField] private PlayerInteractor playerInteractor;
        [SerializeField] private FirstPersonController firstPersonController;
        [SerializeField] private GameObject playerStart;
        [SerializeField] private GameObject shipmentPackagePrefab;

        [Header("Scene object names (match the Editor setup tools' fixed naming)")]
        [SerializeField] private string[] conveyorNames = { "Conveyor_In_1", "Conveyor_In_2", "Conveyor_Out_1" };
        [SerializeField] private string[] storageNames = { "OutputStorage" };
        [SerializeField] private string[] feederNames = { "RawMaterialFeeder" };

        /// <summary>(success, message) — raised after every save/load attempt so UI (or
        /// anything else) can react without this class knowing about any particular panel.</summary>
        public event Action<bool, string> SaveCompleted;
        public event Action<bool, string> LoadCompleted;

        public bool HasSaveFile => SaveFileHandler.HasSaveFile;

        public void SaveGame()
        {
            SaveGameData data = Capture();
            bool success = SaveFileHandler.TryWrite(data, out string error);
            SaveCompleted?.Invoke(success, success ? "Oyun kaydedildi." : $"Kayıt başarısız: {error}");
        }

        public void LoadGame()
        {
            if (!SaveFileHandler.TryRead(out SaveGameData data, out string error))
            {
                LoadCompleted?.Invoke(false, $"Kayıt yüklenemedi: {error}");
                return;
            }

            if (data.saveVersion != CurrentSaveVersion)
            {
                LoadCompleted?.Invoke(false,
                    $"Desteklenmeyen kayıt sürümü ({data.saveVersion}). Bu oyun sürümü {CurrentSaveVersion} bekliyor.");
                return;
            }

            try
            {
                ClearDynamicState();
                Apply(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Shiftworks] Yükleme sırasında hata, oyun yarım durumda kalmış olabilir: {ex}");
                LoadCompleted?.Invoke(false, "Yükleme sırasında bir hata oluştu. Güvenli olması için Editor'de " +
                                              "sahneyi yeniden başlatıp tekrar deneyin.");
                return;
            }

            LoadCompleted?.Invoke(true, "Oyun yüklendi.");
        }

        /// <summary>Starting a new game needs no action beyond closing the menu: the
        /// currently running scene already IS a fresh game until something is loaded on top
        /// of it. Deliberately never touches/deletes the save file — only an explicit Save
        /// overwrites it.</summary>
        public void NewGame()
        {
        }

        // ------------------------------------------------------------------ Capture

        private SaveGameData Capture()
        {
            var data = new SaveGameData
            {
                saveVersion = CurrentSaveVersion,
                savedAtUtc = DateTime.UtcNow.ToString("o"),
            };

            if (economyManager != null)
            {
                data.economy.balance = economyManager.Balance;
            }

            if (powerManager != null)
            {
                data.power.upgradePurchased = powerManager.IsUpgradePurchased;
            }

            CaptureFactoryExpansions(data);
            CaptureOrders(data);
            CaptureMachines(data);
            CaptureConveyors(data);
            CaptureStorages(data);
            CaptureFeeders(data);
            CaptureWorldItems(data);
            CaptureShipmentPackages(data);
            CapturePlayer(data);

            if (statsTracker != null)
            {
                data.stats.totalUnitsProduced = statsTracker.TotalUnitsProduced;
            }

            return data;
        }

        private void CaptureFactoryExpansions(SaveGameData data)
        {
            if (factoryExpansionManager == null || catalog == null)
            {
                return;
            }

            foreach (FactoryExpansionDefinition expansion in catalog.AllExpansions)
            {
                if (expansion == null)
                {
                    continue;
                }

                data.factoryExpansions.Add(new FactoryExpansionSaveData
                {
                    expansionId = catalog.GetExpansionId(expansion),
                    unlocked = factoryExpansionManager.IsUnlocked(expansion),
                });
            }
        }

        private void CaptureOrders(SaveGameData data)
        {
            if (orderManager == null || catalog == null)
            {
                return;
            }

            foreach (OrderDefinition order in orderManager.AvailableOrders)
            {
                if (order == null)
                {
                    continue;
                }

                data.orders.Add(new OrderSaveData
                {
                    orderId = catalog.GetOrderId(order),
                    state = orderManager.GetState(order).ToString(),
                    progressCount = orderManager.GetProgress(order),
                    rewardClaimed = orderManager.IsRewardClaimed(order),
                });
            }
        }

        private void CaptureMachines(SaveGameData data)
        {
            foreach (MachineController machine in FindObjectsByType<MachineController>(FindObjectsSortMode.None))
            {
                MachineProductionSnapshot snapshot = machine.CaptureProductionState();
                Vector3 pos = machine.transform.position;

                var entry = new MachineSaveData
                {
                    posX = pos.x,
                    posY = pos.y,
                    posZ = pos.z,
                    rotY = machine.transform.eulerAngles.y,
                    state = snapshot.State.ToString(),
                    bufferedInputCount = snapshot.BufferedInputCount,
                    processingElapsedSeconds = snapshot.ProcessingElapsedSeconds,
                    hasPendingOutput = snapshot.HasPendingOutput,
                    completedCount = snapshot.CompletedCount,
                    totalProcessingSeconds = snapshot.TotalProcessingSeconds,
                    totalPowerWaitSeconds = snapshot.TotalPowerWaitSeconds,
                };

                if (machine.IsPlayerPlaced)
                {
                    entry.instanceId = machine.InstanceId;
                    entry.machineDefinitionId = catalog != null ? catalog.GetMachineId(machine.SourceDefinition) : "";
                }
                else
                {
                    entry.sceneObjectName = machine.gameObject.name;
                }

                data.machines.Add(entry);
            }
        }

        private void CaptureConveyors(SaveGameData data)
        {
            foreach (string sceneName in conveyorNames)
            {
                ConveyorSegment segment = FindSceneComponent<ConveyorSegment>(sceneName);
                if (segment == null)
                {
                    continue;
                }

                string occupantId = "";
                if (segment.OccupantDefinition != null && catalog != null)
                {
                    occupantId = catalog.GetItemId(segment.OccupantDefinition);
                }

                data.conveyors.Add(new ConveyorSaveData { sceneObjectName = sceneName, occupantItemId = occupantId });
            }
        }

        private void CaptureStorages(SaveGameData data)
        {
            foreach (string sceneName in storageNames)
            {
                OutputStorage storage = FindSceneComponent<OutputStorage>(sceneName);
                if (storage == null)
                {
                    continue;
                }

                data.storages.Add(new StorageSaveData { sceneObjectName = sceneName, storedCount = storage.StoredCount });
            }
        }

        private void CaptureFeeders(SaveGameData data)
        {
            foreach (string sceneName in feederNames)
            {
                RawMaterialFeeder feeder = FindSceneComponent<RawMaterialFeeder>(sceneName);
                if (feeder == null)
                {
                    continue;
                }

                data.feeders.Add(new FeederSaveData { sceneObjectName = sceneName, stock = feeder.Stock });
            }
        }

        private void CaptureWorldItems(SaveGameData data)
        {
            if (catalog == null)
            {
                return;
            }

            foreach (CarryableItem item in FindObjectsByType<CarryableItem>(FindObjectsSortMode.None))
            {
                if (!IsLooseTrackableItem(item))
                {
                    continue;
                }

                string itemId = catalog.GetItemId(item.Definition);
                if (string.IsNullOrEmpty(itemId))
                {
                    continue;
                }

                Vector3 pos = item.transform.position;
                Quaternion rot = item.transform.rotation;
                data.worldItems.Add(new WorldItemSaveData
                {
                    itemId = itemId,
                    posX = pos.x,
                    posY = pos.y,
                    posZ = pos.z,
                    rotX = rot.x,
                    rotY = rot.y,
                    rotZ = rot.z,
                    rotW = rot.w,
                });
            }
        }

        private void CaptureShipmentPackages(SaveGameData data)
        {
            if (catalog == null)
            {
                return;
            }

            foreach (ShipmentPackage package in FindObjectsByType<ShipmentPackage>(FindObjectsSortMode.None))
            {
                if (package.AssociatedOrder == null)
                {
                    continue;
                }

                CarryableItem carryable = package.GetComponent<CarryableItem>();
                if (carryable == null || carryable.IsHeld)
                {
                    // Held package is captured as part of player data instead.
                    continue;
                }

                Vector3 pos = package.transform.position;
                Quaternion rot = package.transform.rotation;
                data.shipmentPackages.Add(new ShipmentPackageSaveData
                {
                    orderId = catalog.GetOrderId(package.AssociatedOrder),
                    posX = pos.x,
                    posY = pos.y,
                    posZ = pos.z,
                    rotX = rot.x,
                    rotY = rot.y,
                    rotZ = rot.z,
                    rotW = rot.w,
                });
            }
        }

        private void CapturePlayer(SaveGameData data)
        {
            if (playerInteractor == null)
            {
                return;
            }

            Transform playerTransform = playerInteractor.transform;
            Vector3 pos = playerTransform.position;
            data.player.posX = pos.x;
            data.player.posY = pos.y;
            data.player.posZ = pos.z;
            data.player.yaw = playerTransform.eulerAngles.y;
            data.player.pitch = firstPersonController != null ? firstPersonController.CurrentPitch : 0f;

            CarryableItem carried = playerInteractor.CarriedItem;
            if (carried == null || catalog == null)
            {
                return;
            }

            ShipmentPackage heldPackage = carried.GetComponent<ShipmentPackage>();
            if (heldPackage != null && heldPackage.AssociatedOrder != null)
            {
                data.player.carriedPackageOrderId = catalog.GetOrderId(heldPackage.AssociatedOrder);
            }
            else if (carried.Definition != null)
            {
                data.player.carriedItemId = catalog.GetItemId(carried.Definition);
            }
        }

        // ------------------------------------------------------------------ Clear (pre-restore)

        /// <summary>Destroys every player-placed/produced dynamic object before a restore —
        /// a no-op for most of this on a fresh scene reload (nothing dynamic exists yet),
        /// but makes a load-while-already-playing equally duplicate-free.</summary>
        private void ClearDynamicState()
        {
            foreach (MachineController machine in FindObjectsByType<MachineController>(FindObjectsSortMode.None))
            {
                if (machine.IsPlayerPlaced)
                {
                    Destroy(machine.gameObject);
                }
            }

            foreach (ShipmentPackage package in FindObjectsByType<ShipmentPackage>(FindObjectsSortMode.None))
            {
                CarryableItem carryable = package.GetComponent<CarryableItem>();
                if (carryable == null || !carryable.IsHeld)
                {
                    Destroy(package.gameObject);
                }
            }

            if (catalog != null)
            {
                foreach (CarryableItem item in FindObjectsByType<CarryableItem>(FindObjectsSortMode.None))
                {
                    if (!IsLooseTrackableItem(item))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(catalog.GetItemId(item.Definition)))
                    {
                        continue;
                    }

                    Destroy(item.gameObject);
                }
            }

            playerInteractor?.ClearCarriedItemForRestore();

            foreach (string sceneName in conveyorNames)
            {
                FindSceneComponent<ConveyorSegment>(sceneName)?.ClearOccupant();
            }
        }

        private static bool IsLooseTrackableItem(CarryableItem item)
        {
            if (item.IsHeld || item.IsAutomated)
            {
                return false;
            }

            if (item.GetComponent<ShipmentPackage>() != null)
            {
                return false;
            }

            // No ItemDefinition = a plain decorative/test prop (e.g. Stage 2's test boxes),
            // deliberately left alone — see class doc.
            return item.Definition != null;
        }

        // ------------------------------------------------------------------ Apply (restore)

        private void Apply(SaveGameData data)
        {
            ApplyFactoryExpansions(data);
            ApplyEconomyAndPower(data);
            ApplyMachines(data);
            ApplyConveyors(data);
            ApplyStorages(data);
            ApplyFeeders(data);
            ApplyWorldItems(data);
            ApplyShipmentPackages(data);
            ApplyOrders(data);
            ApplyPlayer(data);
            ApplyStats(data);
        }

        private void ApplyFactoryExpansions(SaveGameData data)
        {
            if (factoryExpansionManager == null || catalog == null)
            {
                return;
            }

            foreach (FactoryExpansionSaveData entry in data.factoryExpansions)
            {
                if (!entry.unlocked)
                {
                    continue;
                }

                FactoryExpansionDefinition definition = catalog.FindExpansion(entry.expansionId);
                if (definition != null)
                {
                    factoryExpansionManager.RestoreUnlocked(definition);
                }
            }
        }

        private void ApplyEconomyAndPower(SaveGameData data)
        {
            economyManager?.RestoreBalance(data.economy.balance);
            powerManager?.RestoreUpgradeState(data.power.upgradePurchased);
        }

        private void ApplyMachines(SaveGameData data)
        {
            var sceneMachinesByName = new Dictionary<string, MachineController>();
            foreach (MachineController machine in FindObjectsByType<MachineController>(FindObjectsSortMode.None))
            {
                if (!machine.IsPlayerPlaced)
                {
                    sceneMachinesByName[machine.gameObject.name] = machine;
                }
            }

            foreach (MachineSaveData entry in data.machines)
            {
                MachineController target;

                if (!string.IsNullOrEmpty(entry.sceneObjectName))
                {
                    sceneMachinesByName.TryGetValue(entry.sceneObjectName, out target);
                    if (target == null)
                    {
                        Debug.LogWarning($"[Shiftworks] Kayıttaki sahne makinesi '{entry.sceneObjectName}' bulunamadı, atlandı.");
                        continue;
                    }
                }
                else
                {
                    MachineDefinition definition = catalog != null ? catalog.FindMachine(entry.machineDefinitionId) : null;
                    if (definition == null || definition.MachinePrefab == null)
                    {
                        Debug.LogWarning($"[Shiftworks] Kayıttaki makine tanımı '{entry.machineDefinitionId}' bulunamadı, bir makine atlandı.");
                        continue;
                    }

                    Vector3 position = new Vector3(entry.posX, entry.posY, entry.posZ);
                    Quaternion rotation = Quaternion.Euler(0f, entry.rotY, 0f);
                    GameObject spawned = Instantiate(definition.MachinePrefab, position, rotation);
                    target = spawned.GetComponent<MachineController>();
                    if (target == null)
                    {
                        Destroy(spawned);
                        continue;
                    }

                    string instanceId = string.IsNullOrEmpty(entry.instanceId) ? Guid.NewGuid().ToString("N") : entry.instanceId;
                    target.MarkAsPlayerPlaced(instanceId, definition);
                }

                var snapshot = new MachineProductionSnapshot
                {
                    State = ParseEnum(entry.state, MachineState.Idle),
                    BufferedInputCount = entry.bufferedInputCount,
                    ProcessingElapsedSeconds = entry.processingElapsedSeconds,
                    HasPendingOutput = entry.hasPendingOutput,
                    CompletedCount = entry.completedCount,
                    TotalProcessingSeconds = entry.totalProcessingSeconds,
                    TotalPowerWaitSeconds = entry.totalPowerWaitSeconds,
                };

                target.RestoreProductionState(snapshot);
            }
        }

        private void ApplyConveyors(SaveGameData data)
        {
            foreach (ConveyorSaveData entry in data.conveyors)
            {
                ConveyorSegment segment = FindSceneComponent<ConveyorSegment>(entry.sceneObjectName);
                if (segment == null || string.IsNullOrEmpty(entry.occupantItemId))
                {
                    continue;
                }

                ItemDefinition itemDef = catalog != null ? catalog.FindItem(entry.occupantItemId) : null;
                if (itemDef == null || itemDef.WorldPrefab == null)
                {
                    continue;
                }

                GameObject spawned = Instantiate(itemDef.WorldPrefab);
                CarryableItem item = spawned.GetComponent<CarryableItem>();
                if (item == null)
                {
                    Destroy(spawned);
                    continue;
                }

                segment.RestoreOccupant(item);
            }
        }

        private void ApplyStorages(SaveGameData data)
        {
            foreach (StorageSaveData entry in data.storages)
            {
                FindSceneComponent<OutputStorage>(entry.sceneObjectName)?.RestoreStoredCount(entry.storedCount);
            }
        }

        private void ApplyFeeders(SaveGameData data)
        {
            foreach (FeederSaveData entry in data.feeders)
            {
                FindSceneComponent<RawMaterialFeeder>(entry.sceneObjectName)?.RestoreStock(entry.stock);
            }
        }

        private void ApplyWorldItems(SaveGameData data)
        {
            if (catalog == null)
            {
                return;
            }

            foreach (WorldItemSaveData entry in data.worldItems)
            {
                ItemDefinition itemDef = catalog.FindItem(entry.itemId);
                if (itemDef == null || itemDef.WorldPrefab == null)
                {
                    continue;
                }

                Vector3 pos = new Vector3(entry.posX, entry.posY, entry.posZ);
                Quaternion rot = new Quaternion(entry.rotX, entry.rotY, entry.rotZ, entry.rotW);
                Instantiate(itemDef.WorldPrefab, pos, rot);
            }
        }

        private void ApplyShipmentPackages(SaveGameData data)
        {
            if (catalog == null || shipmentPackagePrefab == null)
            {
                return;
            }

            foreach (ShipmentPackageSaveData entry in data.shipmentPackages)
            {
                OrderDefinition order = catalog.FindOrder(entry.orderId);
                if (order == null)
                {
                    continue;
                }

                Vector3 pos = new Vector3(entry.posX, entry.posY, entry.posZ);
                Quaternion rot = new Quaternion(entry.rotX, entry.rotY, entry.rotZ, entry.rotW);
                GameObject spawned = Instantiate(shipmentPackagePrefab, pos, rot);
                spawned.GetComponent<ShipmentPackage>()?.Initialize(order);
            }
        }

        private void ApplyOrders(SaveGameData data)
        {
            if (orderManager == null || catalog == null)
            {
                return;
            }

            foreach (OrderSaveData entry in data.orders)
            {
                OrderDefinition order = catalog.FindOrder(entry.orderId);
                if (order == null)
                {
                    continue;
                }

                OrderState state = ParseEnum(entry.state, OrderState.Available);
                orderManager.RestoreOrderState(order, state, entry.progressCount, entry.rewardClaimed);
            }
        }

        private void ApplyPlayer(SaveGameData data)
        {
            if (playerInteractor == null)
            {
                return;
            }

            CharacterController controller = playerInteractor.Controller;
            if (controller != null)
            {
                // Disabled before the safety check too, so the player's own collider can
                // never make its own current spot register as "occupied" when checking the
                // saved target position.
                controller.enabled = false;
            }

            Vector3 savedPosition = new Vector3(data.player.posX, data.player.posY, data.player.posZ);
            Vector3 targetPosition = IsPositionSafe(savedPosition) ? savedPosition : FallbackPlayerPosition();
            playerInteractor.transform.position = targetPosition;

            if (controller != null)
            {
                controller.enabled = true;
            }

            firstPersonController?.SetLookRotation(data.player.yaw, data.player.pitch);

            if (!string.IsNullOrEmpty(data.player.carriedPackageOrderId) && shipmentPackagePrefab != null)
            {
                OrderDefinition order = catalog != null ? catalog.FindOrder(data.player.carriedPackageOrderId) : null;
                if (order != null)
                {
                    GameObject spawned = Instantiate(shipmentPackagePrefab);
                    spawned.GetComponent<ShipmentPackage>()?.Initialize(order);
                    CarryableItem carryable = spawned.GetComponent<CarryableItem>();
                    if (carryable != null)
                    {
                        playerInteractor.RestoreCarriedItem(carryable);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(data.player.carriedItemId) && catalog != null)
            {
                ItemDefinition itemDef = catalog.FindItem(data.player.carriedItemId);
                if (itemDef != null && itemDef.WorldPrefab != null)
                {
                    GameObject spawned = Instantiate(itemDef.WorldPrefab);
                    CarryableItem carryable = spawned.GetComponent<CarryableItem>();
                    if (carryable != null)
                    {
                        playerInteractor.RestoreCarriedItem(carryable);
                    }
                }
            }
        }

        private void ApplyStats(SaveGameData data)
        {
            statsTracker?.RestoreTotal(data.stats.totalUnitsProduced);
        }

        /// <summary>A small overlap check at roughly chest height: if something solid
        /// already occupies the saved spot (e.g. scene geometry changed since that save),
        /// it is not safe to teleport into — PlayerStart is used instead.</summary>
        private static bool IsPositionSafe(Vector3 position)
        {
            return !Physics.CheckSphere(position + Vector3.up * 0.9f, 0.25f, ~0, QueryTriggerInteraction.Ignore);
        }

        private Vector3 FallbackPlayerPosition()
        {
            return playerStart != null ? playerStart.transform.position : Vector3.zero;
        }

        private static T FindSceneComponent<T>(string gameObjectName) where T : Component
        {
            if (string.IsNullOrEmpty(gameObjectName))
            {
                return null;
            }

            GameObject go = GameObject.Find(gameObjectName);
            return go != null ? go.GetComponent<T>() : null;
        }

        private static T ParseEnum<T>(string value, T fallback) where T : struct
        {
            return !string.IsNullOrEmpty(value) && Enum.TryParse(value, out T result) ? result : fallback;
        }
    }
}
