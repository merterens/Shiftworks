using Shiftworks.Conveyors;
using Shiftworks.Core.Interaction;
using Shiftworks.Items;
using Shiftworks.Power;
using UnityEngine;

namespace Shiftworks.Machines
{
    /// <summary>
    /// Single authority over this machine's production state. All state changes happen
    /// through TryAcceptInput/TryCollectOutput, each a single atomic call guarded by the
    /// current state — the exact boundary a future host-authoritative RPC would wrap
    /// without changing anything else. Player input (PlayerInteractor) and visuals (Update's
    /// animation/color block) only ever read this state, never write it.
    ///
    /// Works standalone (manual-only, as in Stage 3) or wired to conveyors: outputReceiverBehaviour
    /// is optional, and TryAccept lets an input conveyor feed this machine through the exact
    /// same guarded TryAcceptInput a player's E press uses — one authority, two entry points.
    ///
    /// (Stage 8) Power is requested atomically inside TryAcceptInput, alongside input
    /// consumption and the Idle→Processing transition: if PowerManager refuses, nothing is
    /// consumed and the machine stays Idle, so a power shortage can never lose or duplicate
    /// an item. The held allocation is released the moment processing completes (the "motor"
    /// only draws power while actually cutting). Processing's own elapsed-time accumulation
    /// is additionally gated on HasPower every frame — a no-op today since nothing in Stage 8
    /// revokes an already-granted allocation, but it means a future power-interruption
    /// feature (outages, breakdowns) would correctly pause/resume this machine with zero
    /// changes here.
    ///
    /// (Stage 9) recipe.InputCount (previously unread — every Stage 1-8 recipe happened to
    /// default to 1) now genuinely gates when Processing starts: items 1..InputCount-1 are
    /// buffered for free while still Idle, and only the item that COMPLETES the batch is
    /// gated on power — exactly where TryAcceptInput's single atomic check already lived.
    /// A 1-input recipe (the original cutting line) always completes its batch on the very
    /// first item, so its behavior is pixel-identical to before Stage 9.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MachineController : MonoBehaviour, IInteractable, IConveyorReceiver, IPowerConsumer
    {
        [Header("Recipe")]
        [SerializeField] private RecipeDefinition recipe;

        [Header("Points")]
        [SerializeField] private Transform outputSpawnPoint;

        [Header("Automation (optional)")]
        // Must implement IConveyorReceiver. Leave empty to keep this machine manual-only.
        [SerializeField] private MonoBehaviour outputReceiverBehaviour;

        [Header("Power (Stage 8)")]
        [SerializeField] private float powerDemandKw = 4f;
        // Scene-baked original machine gets this wired directly by the Editor setup tool.
        // A prefab ASSET (e.g. the purchasable CuttingMachine.prefab) cannot hold a baked
        // reference to a scene-resident singleton — Unity silently nulls it on save — so any
        // instance spawned later (Stage 6 shop purchase) falls back to a one-time lookup in
        // Awake() instead. Either way there is exactly one PowerManager in the scene.
        [SerializeField] private PowerManager powerManager;
        [SerializeField] private ProductionStatsTracker statsTracker;

        [Header("Visuals")]
        [SerializeField] private Renderer statusIndicatorRenderer;
        [SerializeField] private Transform animatedPart;
        [SerializeField] private Color idleColor = Color.red;
        [SerializeField] private Color processingColor = Color.yellow;
        [SerializeField] private Color outputReadyColor = Color.green;
        [SerializeField] private Color insufficientPowerColor = new(0.6f, 0.1f, 0.8f);
        [SerializeField] private float animationAmplitude = 0.05f;
        [SerializeField] private float animationSpeed = 6f;

        public MachineState State { get; private set; } = MachineState.Idle;

        /// <summary>0..1 while Processing; meaningless in other states.</summary>
        public float Progress { get; private set; }

        public string InteractionPrompt => GetPromptFor(null);

        public float PowerDemandKw => powerDemandKw;
        public bool HasPower { get; private set; }
        public int CompletedCount { get; private set; }
        public float TotalProcessingSeconds { get; private set; }
        public float TotalPowerWaitSeconds { get; private set; }

        /// <summary>(Stage 10) True only for an instance Instantiate()'d at runtime via
        /// MachinePlacementController (a shop purchase) — false for the scene-baked original.
        /// SaveManager uses this to tell "re-instantiate from SourceDefinition on load" apart
        /// from "already exists in the freshly-loaded scene, just restore its state".</summary>
        public bool IsPlayerPlaced { get; private set; }
        public string InstanceId { get; private set; }
        public MachineDefinition SourceDefinition { get; private set; }

        /// <summary>(Stage 11C bugfix) Read-only peeks for NetworkMachineController —
        /// same "outside code only ever reads, never mutates" rule as every other save/sync
        /// accessor on this class.</summary>
        public int BufferedInputCount => _bufferedInputCount;
        public float ProcessingElapsedSeconds => _processingElapsed;
        public CarryableItem PendingOutput => _pendingOutput;

        private float _processingElapsed;
        private CarryableItem _pendingOutput;
        private Vector3 _animatedPartRestPosition;
        private IConveyorReceiver _outputReceiver;
        private int _bufferedInputCount;

        private void Awake()
        {
            if (animatedPart != null)
            {
                _animatedPartRestPosition = animatedPart.localPosition;
            }

            _outputReceiver = outputReceiverBehaviour as IConveyorReceiver;

            if (powerManager == null)
            {
                powerManager = FindFirstObjectByType<PowerManager>();
            }

            if (statsTracker == null)
            {
                statsTracker = FindFirstObjectByType<ProductionStatsTracker>();
            }

            if (powerManager != null)
            {
                powerManager.Register(this);
            }

            UpdateStatusColor();
        }

        private void OnDestroy()
        {
            if (powerManager != null)
            {
                powerManager.Unregister(this);
            }
        }

        /// <summary>Called once by MachinePlacementController right after Instantiate(),
        /// before this machine has done anything else — purely identity bookkeeping for
        /// SaveManager, no gameplay effect.</summary>
        public void MarkAsPlayerPlaced(string instanceId, MachineDefinition sourceDefinition)
        {
            IsPlayerPlaced = true;
            InstanceId = instanceId;
            SourceDefinition = sourceDefinition;
        }

        private void Update()
        {
            AnimateWhileProcessing();

            if (State == MachineState.Processing)
            {
                if (HasPower)
                {
                    _processingElapsed += Time.deltaTime;
                    TotalProcessingSeconds += Time.deltaTime;
                    Progress = Mathf.Clamp01(_processingElapsed / recipe.ProcessingTimeSeconds);

                    if (_processingElapsed >= recipe.ProcessingTimeSeconds)
                    {
                        CompleteProcessing();
                    }
                }
                return;
            }

            if (State == MachineState.Idle)
            {
                // Capacity can change (another machine finishing, a capacity upgrade) without
                // this machine's own state ever transitioning, so the "insufficient power"
                // tint needs a live per-frame check rather than only updating on transitions.
                UpdateStatusColor();
            }

            if (State == MachineState.OutputReady && _outputReceiver != null)
            {
                TryEjectOutputTo(_outputReceiver);
            }
        }

        // IConveyorReceiver: an input conveyor's end-of-belt hand-off arrives here and
        // goes through the exact same guard as a manual player E-press.
        public bool TryAccept(CarryableItem item) => TryAcceptInput(item);

        public bool CanInteract(IInteractor interactor) => true;

        // Generic contract fulfilled for interface compliance. The real, context-dependent
        // actions (place input / collect output) go through TryAcceptInput/TryCollectOutput,
        // called directly by PlayerInteractor since it already knows what the player carries.
        public void Interact(IInteractor interactor)
        {
        }

        public bool TryAcceptInput(CarryableItem carried)
        {
            if (State != MachineState.Idle || carried == null || recipe == null)
            {
                return false;
            }

            if (carried.Definition != recipe.InputItem)
            {
                return false;
            }

            // Only the item that completes the batch (the only one for a 1-input recipe,
            // same as every machine before Stage 9) needs power — materials can be buffered
            // into a multi-input recipe freely, the "motor" only has to run once there is
            // enough to actually start a cycle.
            bool completesBatch = _bufferedInputCount + 1 >= recipe.InputCount;

            if (completesBatch && powerManager != null && !powerManager.TryRequestPower(this))
            {
                // Refused atomically, before anything is consumed — the carried item stays
                // with whoever offered it (player or upstream conveyor), so a power shortage
                // can never lose an item. Attributing this frame's delta as "waiting" means a
                // continuously-retrying automated feed accumulates real wait time, while a
                // single manual E-press the player doesn't repeat contributes ~0 — an honest
                // approximation without needing a separate persistent "is waiting" flag.
                TotalPowerWaitSeconds += Time.deltaTime;
                return false;
            }

            Destroy(carried.gameObject);
            _bufferedInputCount++;

            if (completesBatch)
            {
                _bufferedInputCount = 0;
                State = MachineState.Processing;
                _processingElapsed = 0f;
                Progress = 0f;
                HasPower = true;
            }

            UpdateStatusColor();
            return true;
        }

        public bool TryCollectOutput(IInteractor interactor, out CarryableItem output)
        {
            output = null;

            if (State != MachineState.OutputReady || _pendingOutput == null)
            {
                return false;
            }

            output = _pendingOutput;
            _pendingOutput = null;

            State = MachineState.Idle;
            Progress = 0f;
            UpdateStatusColor();

            output.Interact(interactor);
            return true;
        }

        /// <summary>Tries to push the pending output onto the connected output conveyor.
        /// Mirrors TryCollectOutput's guard exactly, so whichever of the two (manual
        /// collect vs. automatic eject) runs first on a given frame is the only one that
        /// can succeed — the output can never be taken twice.</summary>
        private bool TryEjectOutputTo(IConveyorReceiver receiver)
        {
            if (State != MachineState.OutputReady || _pendingOutput == null)
            {
                return false;
            }

            if (!receiver.TryAccept(_pendingOutput))
            {
                return false;
            }

            _pendingOutput = null;
            State = MachineState.Idle;
            Progress = 0f;
            UpdateStatusColor();
            return true;
        }

        public string GetPromptFor(CarryableItem carriedItem)
        {
            if (recipe == null)
            {
                return string.Empty;
            }

            switch (State)
            {
                case MachineState.Processing:
                    return $"Üretim yapılıyor... %{Mathf.RoundToInt(Progress * 100f)}";

                case MachineState.OutputReady:
                    // PlayerInteractor now auto-drops whatever is carried before collecting
                    // when E is pressed here (see HandleMachineInteraction), so a single E
                    // always works regardless of what's in hand — the prompt just says so.
                    if (carriedItem != null)
                    {
                        return $"E - Elinizdekini Bırak ve {recipe.OutputItem.DisplayName} Al";
                    }
                    return $"E - {recipe.OutputItem.DisplayName} Al (Çıktı Hazır)";

                case MachineState.Idle:
                    // The "(N/M)" progress suffix only appears for a multi-input recipe —
                    // a 1-input recipe's text stays exactly what it was before Stage 9.
                    string progressSuffix = recipe.InputCount > 1 ? $" ({_bufferedInputCount}/{recipe.InputCount})" : "";

                    if (carriedItem != null && carriedItem.Definition == recipe.InputItem)
                    {
                        bool wouldCompleteBatch = _bufferedInputCount + 1 >= recipe.InputCount;
                        if (wouldCompleteBatch && powerManager != null && !powerManager.HasCapacityFor(powerDemandKw))
                        {
                            return "Yetersiz Güç";
                        }
                        return $"E - {recipe.InputItem.DisplayName} Yerleştir{progressSuffix}";
                    }
                    return $"{recipe.InputItem.DisplayName} Gerekiyor{progressSuffix}";

                default:
                    return string.Empty;
            }
        }

        private void CompleteProcessing()
        {
            // The "motor" only draws power while actively cutting — once the part is ready
            // and just sitting there for pickup/ejection, the allocation is freed for
            // whoever else is waiting (another machine, or this same one next cycle).
            if (powerManager != null)
            {
                powerManager.ReleasePower(this);
            }
            HasPower = false;

            SpawnPendingOutput();

            State = MachineState.OutputReady;
            Progress = 1f;
            CompletedCount++;

            if (statsTracker != null)
            {
                statsTracker.RecordUnitCompleted();
            }

            UpdateStatusColor();
        }

        /// <summary>Shared by CompleteProcessing (a cycle just finished) and
        /// RestoreProductionState (a save said a cycle had already finished) — the only
        /// place _pendingOutput is ever created, so both paths spawn it identically.</summary>
        private void SpawnPendingOutput()
        {
            if (recipe.OutputItem == null || recipe.OutputItem.WorldPrefab == null || outputSpawnPoint == null)
            {
                return;
            }

            GameObject spawned = Instantiate(recipe.OutputItem.WorldPrefab, outputSpawnPoint.position, outputSpawnPoint.rotation);
            _pendingOutput = spawned.GetComponent<CarryableItem>();

            // Only mark it automated (kinematic, not manually grabbable) if an output
            // conveyor is actually wired. A standalone Stage 3-style machine keeps
            // producing a perfectly normal, manually-collectible item.
            if (_outputReceiver != null)
            {
                _pendingOutput.BeginAutomatedTransport(outputSpawnPoint.position);
            }
        }

        /// <summary>(Stage 10) Read-only snapshot of everything SaveManager needs to
        /// reconstruct this machine's production cycle — never mutates anything, same
        /// "visual/save code reads, never writes" rule as the status-color logic.</summary>
        public MachineProductionSnapshot CaptureProductionState()
        {
            return new MachineProductionSnapshot
            {
                State = State,
                BufferedInputCount = _bufferedInputCount,
                ProcessingElapsedSeconds = _processingElapsed,
                HasPendingOutput = _pendingOutput != null,
                CompletedCount = CompletedCount,
                TotalProcessingSeconds = TotalProcessingSeconds,
                TotalPowerWaitSeconds = TotalPowerWaitSeconds,
            };
        }

        /// <summary>(Stage 10) Controlled restore entry point — sets state directly instead
        /// of going through TryAcceptInput/CompleteProcessing, since nothing is actually
        /// being consumed or produced right now, only re-applied. Only ever called by
        /// SaveManager, immediately after Awake() (so Register() has already run) and
        /// before the first Update().</summary>
        public void RestoreProductionState(MachineProductionSnapshot snapshot)
        {
            _bufferedInputCount = snapshot.BufferedInputCount;
            State = snapshot.State;
            _processingElapsed = snapshot.ProcessingElapsedSeconds;
            Progress = recipe != null && recipe.ProcessingTimeSeconds > 0f
                ? Mathf.Clamp01(_processingElapsed / recipe.ProcessingTimeSeconds)
                : 0f;
            CompletedCount = snapshot.CompletedCount;
            TotalProcessingSeconds = snapshot.TotalProcessingSeconds;
            TotalPowerWaitSeconds = snapshot.TotalPowerWaitSeconds;
            HasPower = false;

            if (State == MachineState.Processing)
            {
                // Re-request the exact allocation this machine held when saved. If capacity
                // is tight now (e.g. another restored machine claimed it first), this
                // correctly refuses and HasPower stays false — Update()'s existing HasPower
                // gate then simply waits, exactly like a live power shortage mid-cycle
                // (Stage 8), rather than fabricating power or losing progress.
                HasPower = powerManager != null && powerManager.TryRequestPower(this);
            }

            if (State == MachineState.OutputReady && snapshot.HasPendingOutput)
            {
                SpawnPendingOutput();
            }

            UpdateStatusColor();
        }

        /// <summary>(Stage 11C bugfix) Narrower sibling of RestoreProductionState for
        /// multiplayer display sync on a non-host client — deliberately does NOT touch
        /// _pendingOutput/HasPower/stats. A save/load restore correctly re-creates the
        /// pending output locally (a solo session has exactly one simulation to begin
        /// with); in multiplayer the output item must be created and NetworkObject.Spawn'd
        /// by the host exactly once (NetworkMachineController's job), never independently
        /// re-instantiated by every client that happens to receive this state update.</summary>
        public void RestoreDisplayState(MachineState state, int bufferedInputCount, float processingElapsedSeconds)
        {
            State = state;
            _bufferedInputCount = bufferedInputCount;
            _processingElapsed = processingElapsedSeconds;
            Progress = state == MachineState.OutputReady
                ? 1f
                : (recipe != null && recipe.ProcessingTimeSeconds > 0f
                    ? Mathf.Clamp01(_processingElapsed / recipe.ProcessingTimeSeconds)
                    : 0f);

            UpdateStatusColor();
        }

        private void UpdateStatusColor()
        {
            if (statusIndicatorRenderer == null)
            {
                return;
            }

            Color color;
            if (State == MachineState.Processing)
            {
                color = processingColor;
            }
            else if (State == MachineState.OutputReady)
            {
                color = outputReadyColor;
            }
            else if (State == MachineState.Idle && powerManager != null && !powerManager.HasCapacityFor(powerDemandKw))
            {
                color = insufficientPowerColor;
            }
            else
            {
                color = idleColor;
            }

            statusIndicatorRenderer.material.color = color;
        }

        private void AnimateWhileProcessing()
        {
            if (animatedPart == null)
            {
                return;
            }

            if (State != MachineState.Processing)
            {
                animatedPart.localPosition = _animatedPartRestPosition;
                return;
            }

            float offset = Mathf.Sin(Time.time * animationSpeed) * animationAmplitude;
            animatedPart.localPosition = _animatedPartRestPosition + new Vector3(0f, offset, 0f);
        }
    }

    /// <summary>(Stage 10) Plain carrier for MachineController's production state — not a
    /// MonoBehaviour/ScriptableObject, so SaveManager can freely copy its fields into/out of
    /// a JSON-serializable save record without touching Unity object references.</summary>
    public struct MachineProductionSnapshot
    {
        public MachineState State;
        public int BufferedInputCount;
        public float ProcessingElapsedSeconds;
        public bool HasPendingOutput;
        public int CompletedCount;
        public float TotalProcessingSeconds;
        public float TotalPowerWaitSeconds;
    }
}
