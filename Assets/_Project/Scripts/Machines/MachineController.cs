using Shiftworks.Conveyors;
using Shiftworks.Core.Interaction;
using Shiftworks.Items;
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
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MachineController : MonoBehaviour, IInteractable, IConveyorReceiver
    {
        [Header("Recipe")]
        [SerializeField] private RecipeDefinition recipe;

        [Header("Points")]
        [SerializeField] private Transform outputSpawnPoint;

        [Header("Automation (optional)")]
        // Must implement IConveyorReceiver. Leave empty to keep this machine manual-only.
        [SerializeField] private MonoBehaviour outputReceiverBehaviour;

        [Header("Visuals")]
        [SerializeField] private Renderer statusIndicatorRenderer;
        [SerializeField] private Transform animatedPart;
        [SerializeField] private Color idleColor = Color.red;
        [SerializeField] private Color processingColor = Color.yellow;
        [SerializeField] private Color outputReadyColor = Color.green;
        [SerializeField] private float animationAmplitude = 0.05f;
        [SerializeField] private float animationSpeed = 6f;

        public MachineState State { get; private set; } = MachineState.Idle;

        /// <summary>0..1 while Processing; meaningless in other states.</summary>
        public float Progress { get; private set; }

        public string InteractionPrompt => GetPromptFor(null);

        private float _processingElapsed;
        private CarryableItem _pendingOutput;
        private Vector3 _animatedPartRestPosition;
        private IConveyorReceiver _outputReceiver;

        private void Awake()
        {
            if (animatedPart != null)
            {
                _animatedPartRestPosition = animatedPart.localPosition;
            }

            _outputReceiver = outputReceiverBehaviour as IConveyorReceiver;
            UpdateStatusColor();
        }

        private void Update()
        {
            AnimateWhileProcessing();

            if (State == MachineState.Processing)
            {
                _processingElapsed += Time.deltaTime;
                Progress = Mathf.Clamp01(_processingElapsed / recipe.ProcessingTimeSeconds);

                if (_processingElapsed >= recipe.ProcessingTimeSeconds)
                {
                    CompleteProcessing();
                }
                return;
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

            Destroy(carried.gameObject);

            State = MachineState.Processing;
            _processingElapsed = 0f;
            Progress = 0f;
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
                    return $"Kesim yapılıyor... %{Mathf.RoundToInt(Progress * 100f)}";

                case MachineState.OutputReady:
                    return $"E - {recipe.OutputItem.DisplayName} Al";

                case MachineState.Idle:
                    if (carriedItem != null && carriedItem.Definition == recipe.InputItem)
                    {
                        return $"E - {recipe.InputItem.DisplayName} Yerleştir";
                    }
                    return $"{recipe.InputItem.DisplayName} Gerekiyor";

                default:
                    return string.Empty;
            }
        }

        private void CompleteProcessing()
        {
            if (recipe.OutputItem != null && recipe.OutputItem.WorldPrefab != null && outputSpawnPoint != null)
            {
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

            State = MachineState.OutputReady;
            Progress = 1f;
            UpdateStatusColor();
        }

        private void UpdateStatusColor()
        {
            if (statusIndicatorRenderer == null)
            {
                return;
            }

            Color color = State switch
            {
                MachineState.Processing => processingColor,
                MachineState.OutputReady => outputReadyColor,
                _ => idleColor,
            };

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
}
