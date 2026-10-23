using System.Collections.Generic;
using Shiftworks.Items;
using Shiftworks.Power;
using UnityEngine;

namespace Shiftworks.Conveyors
{
    /// <summary>
    /// Moves exactly one item deterministically from startPoint to endPoint and hands it
    /// off to the next receiver. Ownership is explicit: the segment that currently holds
    /// an item is the only thing allowed to move it, and a hand-off only ever happens
    /// through TryAccept — no Rigidbody pushing, no two owners at once.
    ///
    /// (Stage 8) The belt's drive motor draws power continuously once it has something to
    /// do, not per-item: TryAccept requires HasPower, so a de-powered segment simply refuses
    /// every hand-off exactly like an already-occupied one — upstream backs up through the
    /// same existing back-pressure mechanism, no new code needed to avoid ever discarding an
    /// item. Whatever is already aboard just freezes (Update's progress advance is gated on
    /// HasPower too) until power returns.
    /// </summary>
    public class ConveyorSegment : MonoBehaviour, IConveyorReceiver, IPowerConsumer
    {
        [Header("Path")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private float speed = 1.5f;

        [Header("Output")]
        // Must implement IConveyorReceiver (ConveyorSegment, MachineController or
        // OutputStorage). Left empty, the belt simply holds its item at the end (back
        // pressure) forever instead of ever deleting it.
        [SerializeField] private MonoBehaviour nextReceiverBehaviour;

        [Header("Power (Stage 8)")]
        [SerializeField] private float powerDemandKw = 0.5f;
        // See MachineController's identical field for why a runtime fallback is needed.
        [SerializeField] private PowerManager powerManager;

        [Header("Visual (optional)")]
        [SerializeField] private Transform[] beltMarkers;
        [SerializeField] private float markerLoopSpeed = 0.5f;

        public bool IsOccupied => _occupant != null;
        public float PowerDemandKw => powerDemandKw;
        public bool HasPower { get; private set; }

        private IConveyorReceiver _nextReceiver;
        private CarryableItem _occupant;
        private float _progress;
        private float _segmentLength;

        private void Awake()
        {
            _nextReceiver = nextReceiverBehaviour as IConveyorReceiver;
            _segmentLength = Mathf.Max(0.01f, Vector3.Distance(startPoint.position, endPoint.position));

            if (powerManager == null)
            {
                powerManager = FindFirstObjectByType<PowerManager>();
            }

            if (powerManager != null)
            {
                powerManager.Register(this);
            }
        }

        private void OnDestroy()
        {
            if (powerManager != null)
            {
                powerManager.Unregister(this);
            }
        }

        private void Start()
        {
            DetectCycle();
        }

        public bool TryAccept(CarryableItem item)
        {
            if (IsOccupied || item == null || !RequestPower())
            {
                return false;
            }

            _occupant = item;
            _progress = 0f;
            item.BeginAutomatedTransport(startPoint.position);
            return true;
        }

        private bool RequestPower()
        {
            if (HasPower)
            {
                return true;
            }

            if (powerManager == null)
            {
                // No manager wired: unrestricted, same as before Stage 8.
                HasPower = true;
                return true;
            }

            HasPower = powerManager.TryRequestPower(this);
            return HasPower;
        }

        private void Update()
        {
            AnimateMarkers();

            if (_occupant == null)
            {
                return;
            }

            if (!RequestPower())
            {
                return;
            }

            if (_progress < 1f)
            {
                _progress = Mathf.Min(1f, _progress + (speed * Time.deltaTime) / _segmentLength);
                _occupant.SetAutomatedPosition(Vector3.Lerp(startPoint.position, endPoint.position, _progress));
            }

            if (_progress >= 1f)
            {
                TryHandOff();
            }
        }

        private void TryHandOff()
        {
            if (_nextReceiver == null || !_nextReceiver.TryAccept(_occupant))
            {
                // No downstream, or downstream is full/busy: hold position, wait our turn.
                return;
            }

            _occupant = null;
            _progress = 0f;
        }

        private void AnimateMarkers()
        {
            if (beltMarkers == null || beltMarkers.Length == 0)
            {
                return;
            }

            for (int i = 0; i < beltMarkers.Length; i++)
            {
                if (beltMarkers[i] == null)
                {
                    continue;
                }

                float phase = i / (float)beltMarkers.Length;
                float t = Mathf.Repeat(Time.time * markerLoopSpeed / _segmentLength + phase, 1f);
                beltMarkers[i].position = Vector3.Lerp(startPoint.position, endPoint.position, t);
            }
        }

        private void DetectCycle()
        {
            var visited = new HashSet<ConveyorSegment>();
            ConveyorSegment current = this;
            int guard = 0;

            while (current != null && guard++ < 200)
            {
                if (!visited.Add(current))
                {
                    Debug.LogWarning($"[Shiftworks] Konveyör döngüsü tespit edildi: '{name}' zincirinde bir halka var.", this);
                    return;
                }

                current = current._nextReceiver as ConveyorSegment;
            }
        }
    }
}
