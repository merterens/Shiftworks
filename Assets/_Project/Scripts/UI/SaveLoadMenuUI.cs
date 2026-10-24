using System;
using Shiftworks.SaveSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Shiftworks.UI
{
    /// <summary>
    /// Modal save/load panel, opened by PlayerInteractor on Escape (see its HandleEscapeKey)
    /// whenever no other panel already owns input — same open/close contract as every other
    /// modal here (Open takes an onClose callback, PlayerInteractor owns the cursor). Only
    /// owns its own visibility/buttons and the status line; SaveManager is the single
    /// authority that actually knows how to save/load.
    /// </summary>
    public class SaveLoadMenuUI : MonoBehaviour
    {
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text statusText;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button resumeButton;

        private Action _onClose;

        private bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        private void Awake()
        {
            if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
            if (loadButton != null) loadButton.onClick.AddListener(OnLoadClicked);
            if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameClicked);
            if (resumeButton != null) resumeButton.onClick.AddListener(Close);
            if (panelRoot != null) panelRoot.SetActive(false);
            if (statusText != null) statusText.enabled = false;
        }

        private void OnEnable()
        {
            if (saveManager != null)
            {
                saveManager.SaveCompleted += HandleSaveCompleted;
                saveManager.LoadCompleted += HandleLoadCompleted;
            }
        }

        private void OnDisable()
        {
            if (saveManager != null)
            {
                saveManager.SaveCompleted -= HandleSaveCompleted;
                saveManager.LoadCompleted -= HandleLoadCompleted;
            }
        }

        public void Open(Action onClose)
        {
            _onClose = onClose;

            if (panelRoot != null) panelRoot.SetActive(true);
            if (statusText != null) statusText.enabled = false;

            RefreshLoadButton();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            if (panelRoot != null) panelRoot.SetActive(false);

            Action callback = _onClose;
            _onClose = null;
            callback?.Invoke();
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
            }
        }

        private void OnSaveClicked() => saveManager?.SaveGame();

        private void OnLoadClicked() => saveManager?.LoadGame();

        private void OnNewGameClicked()
        {
            saveManager?.NewGame();
            Close();
        }

        private void HandleSaveCompleted(bool success, string message) => ShowStatus(message, success);

        private void HandleLoadCompleted(bool success, string message)
        {
            ShowStatus(message, success);
            RefreshLoadButton();
        }

        private void ShowStatus(string message, bool success)
        {
            if (statusText == null)
            {
                return;
            }

            statusText.enabled = true;
            statusText.text = message;
            statusText.color = success ? new Color(0.55f, 1f, 0.55f) : new Color(1f, 0.45f, 0.45f);
        }

        private void RefreshLoadButton()
        {
            if (loadButton != null)
            {
                loadButton.interactable = saveManager != null && saveManager.HasSaveFile;
            }
        }
    }
}
