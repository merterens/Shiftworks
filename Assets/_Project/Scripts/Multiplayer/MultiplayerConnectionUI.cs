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

        // (Stage 11B bugfix) NetworkManager.Singleton is assigned inside NetworkManager's OWN
        // OnEnable() (verified against the installed Netcode package source), not Awake() —
        // and Unity does not guarantee OnEnable() ordering BETWEEN different GameObjects
        // (only that every object's Awake() runs before any object's OnEnable(), and every
        // OnEnable() runs before any Start()). Subscribing here in OnEnable() meant that
        // whenever Unity happened to enable this Canvas before the NetworkManager GameObject,
        // NetworkManager.Singleton was still null, the early-return skipped the subscription,
        // and it was NEVER retried — this component would then never hear about a real,
        // successful connection. The Windows client's Player.log proved exactly this: both
        // NetworkPlayer objects spawned and the local camera/controls were enabled (so the
        // connection genuinely succeeded), yet the 8s "no response" watchdog fired anyway and
        // called Shutdown() on a perfectly good connection, because OnClientConnected was
        // simply never subscribed, let alone called. Start() is scene-wide guaranteed to run
        // after every object's OnEnable(), so NetworkManager.Singleton is always set by then.
        private bool _subscribed;

        private void OnDisable()
        {
            if (!_subscribed || NetworkManager.Singleton == null)
            {
                return;
            }

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
            _subscribed = false;
        }

        private void Start()
        {
            if (NetworkManager.Singleton != null && !_subscribed)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                NetworkManager.Singleton.OnServerStopped += OnServerStopped;
                _subscribed = true;
            }
            else if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[Shiftworks][MP] NetworkManager.Singleton Start() sırasında hâlâ null — bağlantı olayları dinlenemeyecek.");
            }

            RefreshStatus();
        }

        private void Update()
        {
            if (_clientConnectAttemptPending)
            {
                _clientConnectElapsed += Time.deltaTime;
                if (_clientConnectElapsed >= clientConnectTimeoutSeconds)
                {
                    Debug.Log($"[Shiftworks][MP] Client bağlantı zaman aşımı ({clientConnectTimeoutSeconds}s) — host hiç yanıt vermedi.");
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
            bool started = NetworkManager.Singleton.StartHost();
            Debug.Log(started
                ? "[Shiftworks][MP] Host başlatma başarılı."
                : "[Shiftworks][MP] Host başlatma BAŞARISIZ (StartHost false döndü).");
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
            Debug.Log($"[Shiftworks][MP] Client bağlantı isteği gönderiliyor: {transport?.ConnectionData.Address}:{port}");
            bool started = NetworkManager.Singleton.StartClient();
            Debug.Log(started
                ? "[Shiftworks][MP] StartClient çağrıldı, host yanıtı bekleniyor."
                : "[Shiftworks][MP] Client bağlantı isteği BAŞARISIZ (StartClient false döndü).");
            RefreshStatus();
        }

        private void OnDisconnectClicked()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
            {
                return;
            }

            Debug.Log("[Shiftworks][MP] Bağlantı kullanıcı tarafından kesildi (Bağlantıyı Kes).");
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
            bool isMe = NetworkManager.Singleton != null && clientId == NetworkManager.Singleton.LocalClientId;
            Debug.Log(isMe
                ? "[Shiftworks][MP] Client bağlandı (bu makine)."
                : $"[Shiftworks][MP] Başka bir client bağlandı: {clientId}.");
            _clientConnectAttemptPending = false;
            RefreshStatus();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            string reason = NetworkManager.Singleton != null ? NetworkManager.Singleton.DisconnectReason : "";
            Debug.Log($"[Shiftworks][MP] Client bağlantısı kesildi/reddedildi: clientId={clientId}" +
                (string.IsNullOrEmpty(reason) ? "" : $", sebep: {reason}"));

            if (_clientConnectAttemptPending)
            {
                // The handshake never finished — the host was unreachable, not running,
                // or rejected the connection.
                FailClientConnectAttempt(reason);
                return;
            }

            RefreshStatus();
        }

        private void OnServerStopped(bool wasHost)
        {
            Debug.Log("[Shiftworks][MP] Sunucu/host durduruldu.");
            RefreshStatus();
        }

        private void FailClientConnectAttempt(string reason = "")
        {
            _clientConnectAttemptPending = false;
            _clientConnectElapsed = 0f;

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                NetworkManager.Singleton.Shutdown();
            }

            string suffix = string.IsNullOrEmpty(reason) ? "Host bulunamadı" : reason;
            ShowTransientStatus($"Durum: Bağlantı hatası ({suffix})");
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
