using System.Collections.Generic;
using Shiftworks.Economy;
using UnityEngine;

namespace Shiftworks.Power
{
    /// <summary>
    /// Single authority over the factory's power capacity. TryRequestPower/ReleasePower are
    /// the only places UsedCapacityKw changes, and TryRequestPower is idempotent (a consumer
    /// already holding power just gets "true" back, never double-counted) — the same
    /// single-authority + atomic-method pattern as EconomyManager/MachinePurchaseManager.
    ///
    /// Grant order is plain first-come-first-served against live AvailableCapacityKw — no
    /// stored waiting queue, no preemption of an already-granted consumer. This is
    /// deterministic (never random) and, because every consumer type in this codebase already
    /// re-asks on its own natural cadence (ConveyorSegment every Update, a manually-fed
    /// MachineController every player E-press, an automated one every conveyor hand-off
    /// retry), "bekleyen ekipmanlar tekrar değerlendirilmeli" falls out for free without a
    /// replay/callback system. The single seam a future explicit priority scheme would touch
    /// is HasCapacityFor/TryRequestPower's capacity check below.
    /// </summary>
    public class PowerManager : MonoBehaviour
    {
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private float initialCapacityKw = 10f;
        [SerializeField] private float upgradedCapacityKw = 20f;
        [SerializeField] private int upgradePriceDollars = 300;

        public float TotalCapacityKw { get; private set; }
        public float UsedCapacityKw { get; private set; }
        public float AvailableCapacityKw => TotalCapacityKw - UsedCapacityKw;
        public int ConnectedConsumerCount => _consumers.Count;
        public int WaitingConsumerCount => _waiting.Count;
        public bool IsUpgradePurchased { get; private set; }
        public float UpgradeTargetCapacityKw => upgradedCapacityKw;
        public int UpgradePriceDollars => upgradePriceDollars;

        private readonly List<IPowerConsumer> _consumers = new();
        private readonly HashSet<IPowerConsumer> _granted = new();
        private readonly HashSet<IPowerConsumer> _waiting = new();

        private void Awake()
        {
            TotalCapacityKw = initialCapacityKw;
        }

        public void Register(IPowerConsumer consumer)
        {
            if (consumer != null && !_consumers.Contains(consumer))
            {
                _consumers.Add(consumer);
            }
        }

        /// <summary>Safe to call even if the consumer was never granted power or never
        /// registered — releases any held allocation and drops all bookkeeping, so a
        /// destroyed/disabled object never leaves capacity permanently locked.</summary>
        public void Unregister(IPowerConsumer consumer)
        {
            if (consumer == null)
            {
                return;
            }

            _consumers.Remove(consumer);
            _waiting.Remove(consumer);

            if (_granted.Remove(consumer))
            {
                UsedCapacityKw = Mathf.Max(0f, UsedCapacityKw - consumer.PowerDemandKw);
            }
        }

        public bool HasCapacityFor(float demandKw) => demandKw <= AvailableCapacityKw + 0.0001f;

        public bool TryRequestPower(IPowerConsumer consumer)
        {
            if (consumer == null)
            {
                return false;
            }

            if (_granted.Contains(consumer))
            {
                return true;
            }

            if (HasCapacityFor(consumer.PowerDemandKw))
            {
                _granted.Add(consumer);
                UsedCapacityKw += consumer.PowerDemandKw;
                _waiting.Remove(consumer);
                return true;
            }

            _waiting.Add(consumer);
            return false;
        }

        public void ReleasePower(IPowerConsumer consumer)
        {
            if (consumer == null)
            {
                return;
            }

            if (_granted.Remove(consumer))
            {
                UsedCapacityKw = Mathf.Max(0f, UsedCapacityKw - consumer.PowerDemandKw);
            }

            _waiting.Remove(consumer);
        }

        public bool TryPurchaseUpgrade()
        {
            if (IsUpgradePurchased || economyManager == null)
            {
                return false;
            }

            if (!economyManager.TrySpend(upgradePriceDollars))
            {
                return false;
            }

            TotalCapacityKw = upgradedCapacityKw;
            IsUpgradePurchased = true;
            return true;
        }

        /// <summary>(Stage 10) Re-applies a previously-purchased upgrade on load — deliberately
        /// bypasses TryPurchaseUpgrade entirely (no EconomyManager.TrySpend call), since the
        /// player already paid for this in the saved session. UsedCapacityKw is intentionally
        /// left untouched here: it is never read from the save file, only ever rebuilt by each
        /// restored consumer's own TryRequestPower call, so it can never drift from what's
        /// actually granted or get double-counted against an old saved number.</summary>
        public void RestoreUpgradeState(bool upgradePurchased)
        {
            IsUpgradePurchased = upgradePurchased;
            TotalCapacityKw = upgradePurchased ? upgradedCapacityKw : initialCapacityKw;
        }
    }
}
