using System.Collections.Generic;
using Shiftworks.Items;
using UnityEngine;

namespace Shiftworks.Conveyors
{
    /// <summary>
    /// Moves exactly one item deterministically from startPoint to endPoint and hands it
    /// off to the next receiver. Ownership is explicit: the segment that currently holds
    /// an item is the only thing allowed to move it, and a hand-off only ever happens
    /// through TryAccept — no Rigidbody pushing, no two owners at once.
    /// </summary>
    public class ConveyorSegment : MonoBehaviour, IConveyorReceiver
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

        [Header("Visual (optional)")]
        [SerializeField] private Transform[] beltMarkers;
        [SerializeField] private float markerLoopSpeed = 0.5f;

        public bool IsOccupied => _occupant != null;

        private IConveyorReceiver _nextReceiver;
        private CarryableItem _occupant;
        private float _progress;
        private float _segmentLength;

        private void Awake()
        {
            _nextReceiver = nextReceiverBehaviour as IConveyorReceiver;
            _segmentLength = Mathf.Max(0.01f, Vector3.Distance(startPoint.position, endPoint.position));
        }

        private void Start()
        {
            DetectCycle();
        }

        public bool TryAccept(CarryableItem item)
        {
            if (IsOccupied || item == null)
            {
                return false;
            }

            _occupant = item;
            _progress = 0f;
            item.BeginAutomatedTransport(startPoint.position);
            return true;
        }

        private void Update()
        {
            AnimateMarkers();

            if (_occupant == null)
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
