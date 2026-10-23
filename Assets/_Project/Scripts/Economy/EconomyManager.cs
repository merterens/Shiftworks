using UnityEngine;

namespace Shiftworks.Economy
{
    /// <summary>
    /// Single authority over the player's money. All balance changes go through
    /// AddFunds/TrySpend — this is the one place a future shared/host-authoritative
    /// economy would need to touch, nothing else ever writes Balance directly.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        [SerializeField] private int startingBalance = 500;

        public int Balance { get; private set; }

        private void Awake()
        {
            Balance = startingBalance;
        }

        public void AddFunds(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Balance += amount;
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0 || amount > Balance)
            {
                return false;
            }

            Balance -= amount;
            return true;
        }
    }
}
