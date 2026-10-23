using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace Shiftworks.Multiplayer
{
    /// <summary>
    /// Developer-facing test panel for the two-player technical prototype: Start Host,
    /// Connect Client, Disconnect, and a status readout. Pure UI glue over
    /// NetworkManager.Singleton's existing API — no gameplay state of its own, so it
    /// cannot desync anything if closed/reopened or if the connection drops unexpectedly.
    /// </summary>
    public class MultiplayerConnectionUI : MonoBehaviour
    {
        [SerializeField] private UnityTransport transport;
        [SerializeField] private InputField ipInputField;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button disconnectButton;
        [SerializeField] private Text statusText;
        [SerializeField] private ushort port = 7777;

        private void Awake()
        {
            if (hostButton != null) hostButton.onClick.AddListener(OnHostClicked);
            if (clientButton != null) clientButton.onClick.AddListener(OnClientClicked);
            if (disconnectButton != null) disconnectButton.onClick.AddListener(OnDisconnectClicked);
        }

        private void OnEnable()
        {
            if (NetworkManager.Singleton == null)
            {
                return;
            }

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnServerStopped += OnServerStopped;
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton == null)
            {
                return;
            }

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
        }

        private void Start()
        {
            RefreshStatus();
        }

        private void OnHostClicked()
        {
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsListening)
            {
                return;
            }

            ApplyConnectionAddress();
            NetworkManager.Singleton.StartHost();
            RefreshStatus();
        }

        private void OnClientClicked()
        {
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsListening)
            {
                return;
            }

            ApplyConnectionAddress();
            NetworkManager.Singleton.StartClient();
            RefreshStatus();
        }

        private void OnDisconnectClicked()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            {
                return;
            }

            NetworkManager.Singleton.Shutdown();
            RefreshStatus();
        }

        private void ApplyConnectionAddress()
        {
            if (transport == null)
            {
                return;
            }

            string address = ipInputField != null && !string.IsNullOrWhiteSpace(ipInputField.text)
                ? ipInputField.text
                : "127.0.0.1";

            transport.SetConnectionData(address, port);
        }

        private void OnClientConnected(ulong clientId) => RefreshStatus();

        private void OnClientDisconnected(ulong clientId) => RefreshStatus();

        private void OnServerStopped(bool wasHost) => RefreshStatus();

        private void RefreshStatus()
        {
            if (statusText == null)
            {
                return;
            }

            NetworkManager manager = NetworkManager.Singleton;
            bool listening = manager != null && manager.IsListening;

            if (hostButton != null) hostButton.interactable = !listening;
            if (clientButton != null) clientButton.interactable = !listening;
            if (disconnectButton != null) disconnectButton.interactable = listening;

            if (!listening)
            {
                statusText.text = "Durum: Bağlı değil";
                return;
            }

            if (manager.IsHost)
            {
                statusText.text = $"Durum: Host çalışıyor ({manager.ConnectedClientsList.Count} istemci bağlı)";
            }
            else if (manager.IsClient)
            {
                statusText.text = manager.IsConnectedClient ? "Durum: İstemci olarak bağlı" : "Durum: Bağlanıyor...";
            }
            else
            {
                statusText.text = "Durum: Sunucu çalışıyor";
            }
        }
    }
}
