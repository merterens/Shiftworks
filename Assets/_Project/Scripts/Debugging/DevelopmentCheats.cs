using Shiftworks.Economy;
using Shiftworks.Player;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

namespace Shiftworks.Debugging
{
    /// <summary>
    /// Editor-only test-money cheat: F8 in Play Mode adds a fixed amount of funds through
    /// EconomyManager.AddFunds — the exact same single-authority entry point every real
    /// reward (order completion) already uses, so this can never bypass or corrupt the
    /// real economy rules, only speed up testing. Entirely wrapped in #if UNITY_EDITOR: a
    /// Steam/release build keeps the component (no missing-script reference on the scene
    /// object) but its Update() does nothing, so F8 is physically inert there.
    /// </summary>
    public class DevelopmentCheats : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private PlayerInteractor playerInteractor;
        [SerializeField] private int testFundsAmount = 10000;

        private void Update()
        {
            if (economyManager == null)
            {
                return;
            }

            // Skip while a modal panel (shop/order/power/expansion) owns input — every
            // panel already funnels through PlayerInteractor.SuspendForUI, so this is the
            // same "a UI window is open" signal the game itself uses, not a new check.
            if (playerInteractor != null && playerInteractor.IsInputSuspended)
            {
                return;
            }

            // wasPressedThisFrame fires exactly once per physical press, never while held.
            if (Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame)
            {
                economyManager.AddFunds(testFundsAmount);
                Debug.Log($"TEST: +{testFundsAmount}$ eklendi");
            }
        }
#endif
    }
}
