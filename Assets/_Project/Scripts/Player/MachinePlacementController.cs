using Shiftworks.Machines;
using Shiftworks.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Shiftworks.Player
{
    /// <summary>
    /// Drives the semi-transparent "ghost" preview while placing a purchased machine.
    /// Movement/look stay fully active during placement (the player can walk around to
    /// find a spot); only PlayerInteractor's own E/G/left-click handling is suspended for
    /// the duration via SetPlacementModeActive, so left-click/R/Escape can be repurposed
    /// here without colliding with carry/throw/interact.
    /// </summary>
    [RequireComponent(typeof(PlayerInteractor))]
    public class MachinePlacementController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private InteractionPromptUI promptUI;
        [SerializeField] private float maxPlacementDistance = 6f;
        [SerializeField] private Material validPreviewMaterial;
        [SerializeField] private Material invalidPreviewMaterial;

        [Header("Debug")]
        [SerializeField] private bool debugLogging = true;

        // Excludes the built-in "Ignore Raycast" layer. The ghost preview is put on this
        // layer so the placement raycast looks straight through it to the real floor/walls
        // behind it instead of always hitting itself — the same class of bug fixed for the
        // player's own carried items (CarryableItem / PlayerInteractor.InteractionLayerMask).
        private static readonly int PlacementLayerMask = ~(1 << 2);

        // The ghost's bottom face is placed with ZERO gap against whatever surface it is
        // resting on (by design, so it "sits" on the floor). PhysX's contact/skin tolerance
        // treats an exactly-touching box as overlapping, so the overlap PROBE (not the visual
        // ghost, not the final placed object) is shrunk inward and lifted by this margin —
        // otherwise the floor itself registers as a blocking collider everywhere in the room.
        private const float OverlapSkin = 0.03f;

        public bool IsActive { get; private set; }

        private PlayerInteractor _playerInteractor;
        private MachineDefinition _pendingDefinition;
        private MachinePurchaseManager _purchaseManager;
        private GameObject _previewInstance;
        private Renderer[] _previewRenderers;
        private Vector3 _halfExtents;
        private float _yaw;
        private bool _currentPlacementValid;
        private Vector3 _currentPlacementPosition;
        private Quaternion _currentPlacementRotation;
        private string _lastLoggedReason = "";
        private readonly Collider[] _overlapBuffer = new Collider[8];

        private void Awake()
        {
            _playerInteractor = GetComponent<PlayerInteractor>();
        }

        public void BeginPlacement(MachineDefinition definition, MachinePurchaseManager purchaseManager)
        {
            if (IsActive || definition == null || definition.MachinePrefab == null)
            {
                return;
            }

            _pendingDefinition = definition;
            _purchaseManager = purchaseManager;
            _yaw = 0f;
            _lastLoggedReason = "";

            CreatePreview();

            IsActive = true;
            _playerInteractor.SetPlacementModeActive(true);
        }

        private void CreatePreview()
        {
            _previewInstance = Instantiate(_pendingDefinition.MachinePrefab);
            _previewInstance.name = "MachinePlacementPreview";

            MachineController controller = _previewInstance.GetComponent<MachineController>();
            if (controller != null)
            {
                Destroy(controller);
            }

            BoxCollider boxCollider = _previewInstance.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.isTrigger = true;
                _halfExtents = Vector3.Scale(boxCollider.size, _previewInstance.transform.localScale) * 0.5f;
            }
            else
            {
                _halfExtents = _previewInstance.transform.localScale * 0.5f;
            }

            SetLayerRecursively(_previewInstance.transform, 2);
            _previewRenderers = _previewInstance.GetComponentsInChildren<Renderer>();
        }

        private static void SetLayerRecursively(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            foreach (Transform child in root)
            {
                SetLayerRecursively(child, layer);
            }
        }

        private void Update()
        {
            if (!IsActive)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                EndPlacement();
                return;
            }

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                _yaw = (_yaw + 90f) % 360f;
            }

            UpdateGhostPose();
            ShowHint();

            if (_currentPlacementValid && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                ConfirmPlacement();
            }
        }

        private void UpdateGhostPose()
        {
            _currentPlacementRotation = Quaternion.Euler(0f, _yaw, 0f);

            if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit,
                    maxPlacementDistance, PlacementLayerMask))
            {
                SetValidity(false, "Zemin bulunamadı (ışın menzil içinde hiçbir yüzeye çarpmadı)");
                _previewInstance.SetActive(false);
                return;
            }

            _previewInstance.SetActive(true);

            Vector3 candidatePosition = hit.point + Vector3.up * _halfExtents.y;
            _currentPlacementPosition = candidatePosition;
            _previewInstance.transform.SetPositionAndRotation(candidatePosition, _currentPlacementRotation);

            bool onFloor = Vector3.Dot(hit.normal, Vector3.up) > 0.7f;
            if (!onFloor)
            {
                SetValidity(false, $"Zemin değil: '{hit.collider.name}' yüzeyi çok eğik/dik");
                ApplyPreviewMaterial(false);
                return;
            }

            // Shrink + lift the overlap probe slightly so the ghost's own resting surface
            // (the floor it is exactly touching) never counts as a blocker, while genuine
            // wall/machine/player penetration — far larger than this 3cm margin — still does.
            Vector3 overlapHalfExtents = new Vector3(
                Mathf.Max(0.01f, _halfExtents.x - OverlapSkin),
                Mathf.Max(0.01f, _halfExtents.y - OverlapSkin),
                Mathf.Max(0.01f, _halfExtents.z - OverlapSkin));
            Vector3 overlapCenter = candidatePosition + Vector3.up * OverlapSkin;

            int overlapCount = Physics.OverlapBoxNonAlloc(overlapCenter, overlapHalfExtents, _overlapBuffer,
                _currentPlacementRotation, PlacementLayerMask, QueryTriggerInteraction.Ignore);

            if (overlapCount > 0)
            {
                Collider blocker = _overlapBuffer[0];
                string layerName = LayerMask.LayerToName(blocker.gameObject.layer);
                SetValidity(false, $"Çakışma: '{blocker.name}' (layer: {layerName})");
                ApplyPreviewMaterial(false);
                return;
            }

            SetValidity(true, "Geçerli konum");
            ApplyPreviewMaterial(true);
        }

        private void SetValidity(bool valid, string reason)
        {
            _currentPlacementValid = valid;

            if (!debugLogging || reason == _lastLoggedReason)
            {
                return;
            }

            _lastLoggedReason = reason;
            Debug.Log($"[Shiftworks] Yerleştirme: {reason}");
        }

        private void ApplyPreviewMaterial(bool valid)
        {
            Material material = valid ? validPreviewMaterial : invalidPreviewMaterial;
            if (material == null || _previewRenderers == null)
            {
                return;
            }

            foreach (Renderer renderer in _previewRenderers)
            {
                renderer.sharedMaterial = material;
            }
        }

        private void ShowHint()
        {
            if (promptUI == null)
            {
                return;
            }

            string status = _currentPlacementValid ? "Geçerli konum" : "Geçersiz konum";
            promptUI.Show($"{status}  |  R: Döndür  |  Sol Tık: Yerleştir  |  Esc: İptal");
        }

        private void ConfirmPlacement()
        {
            if (_purchaseManager == null || !_purchaseManager.TryConsumePending(_pendingDefinition))
            {
                return;
            }

            Instantiate(_pendingDefinition.MachinePrefab, _currentPlacementPosition, _currentPlacementRotation);
            EndPlacement();
        }

        private void EndPlacement()
        {
            if (_previewInstance != null)
            {
                Destroy(_previewInstance);
            }

            if (promptUI != null)
            {
                promptUI.Hide();
            }

            IsActive = false;
            _playerInteractor.SetPlacementModeActive(false);
        }
    }
}
