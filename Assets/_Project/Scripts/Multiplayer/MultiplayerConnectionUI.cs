using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace Shiftworks.Multiplayer
{
    /// <summary>
    /// Developer-facing test panel for the two-player technical prototype: Start Host,
    /// Connect Client, Disconnect, and a status readout covering all requested states
    /// (not connected / host active / client connecting / client connected / disconnected /
    /// connection error). Pure UI glue over NetworkManager.Singleton's existing API — the
    /// only state this script owns is the transient status message's own timer, never
    /// gameplay state, so it cannot desync anything if closed/reopened or if the
    /// connection drops unexpectedly.
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
        [SerializeField] private float transientMessageDuration = 3f;

        // UTP's own internal retry/timeout can take up to ~60s before it gives up and
        // fires a disconnect callback, which made a "no host running" test look like a
        // frozen "Client bağlanıyor..." screen. This is a UI-only watchdog — it forces a
        // much faster, visible "connection failed" result for testing, without touching
        // NetworkManager/UnityTransport's own retry configuration.
        [SerializeField] private float clientConnectTimeoutSeconds = 8f;

        // True from the moment StartClient() is called until either a successful connect
        // (OnClientConnected) or a disconnect callback arrives. A disconnect that arrives
        // while this is still true means the handshake never completed — i.e. the host
        // was unreachable/not found — as opposed to a client that connected fine and was
        // later dropped.
        private bool _clientConnectAttemptPending;
        private float _clientConnectElapsed;
        private float _transientTimer;

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

        private void Update()
        {
            if (_clientConnectAttemptPending)
            {
                _clientConnectElapsed += Time.deltaTime;
                if (_clientConnectElapsed >= clientConnectTimeoutSeconds)
                {
                    FailClientConnectAttempt();
                }
            }

            // Lets a one-shot message (disconnect / connection error) stay on screen for a
            // few seconds instead of being instantly overwritten by the next NetworkManager
            // callback (Shutdown() alone can fire OnServerStopped right after the click).
            if (_transientTimer <= 0f)
            {
                return;
            }

            _transientTimer -= Time.deltaTime;
            if (_transientTimer <= 0f)
            {
                RefreshStatus();
            }
        }

        private void OnHostClicked()
        {
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsListening)
            {
                return;
            }

            _clientConnectAttemptPending = false;
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

            _clientConnectAttemptPending = true;
            _clientConnectElapsed = 0f;
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
            ShowTransientStatus("Durum: Bağlantı kesildi");
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

        private void OnClientConnected(ulong clientId)
        {
            _clientConnectAttemptPending = false;
            RefreshStatus();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (_clientConnectAttemptPending)
            {
                // The handshake never finished — the host was unreachable, not running,
                // or rejected the connection.
                FailClientConnectAttempt();
                return;
            }

            RefreshStatus();
        }

        private void OnServerStopped(bool wasHost) => RefreshStatus();

        private void FailClientConnectAttempt()
        {
            _clientConnectAttemptPending = false;
            _clientConnectElapsed = 0f;

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            ShowTransientStatus("Durum: Bağlantı hatası (Host bulunamadı)");
        }

        private void ShowTransientStatus(string message)
        {
            _transientTimer = transientMessageDuration;
            if (statusText != null) statusText.text = message;
            UpdateButtonInteractivity();
        }

        private void RefreshStatus()
        {
            if (_transientTimer > 0f)
            {
                // A transient message is still being shown; let it finish on its own
                // timer instead of snapping back to the live state early.
                return;
            }

            UpdateButtonInteractivity();

            if (statusText == null)
            {
                return;
            }

            NetworkManager manager = NetworkManager.Singleton;
            bool listening = manager != null && manager.IsListening;

            if (!listening)
            {
                statusText.text = "Durum: Bağlı değil";
                return;
            }

            if (manager.IsHost)
            {
                statusText.text = $"Durum: Host aktif ({manager.ConnectedClientsList.Count} istemci bağlı)";
            }
            else if (manager.IsClient)
            {
                statusText.text = manager.IsConnectedClient ? "Durum: Client bağlı" : "Durum: Client bağlanıyor...";
            }
            else
            {
                statusText.text = "Durum: Sunucu çalışıyor";
            }
        }

        private void UpdateButtonInteractivity()
        {
            NetworkManager manager = NetworkManager.Singleton;
            bool listening = manager != null && manager.IsListening;

            if (hostButton != null) hostButton.interactable = !listening;
            if (clientButton != null) clientButton.interactable = !listening;
            if (disconnectButton != null) disconnectButton.interactable = listening;
        }
    }
}
