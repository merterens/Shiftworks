using Unity.Netcode;
using UnityEngine;

namespace Shiftworks.Multiplayer
{
    /// <summary>
    /// (Stage 11A) Host-side only: hands each connecting player a distinct spawn point
    /// instead of letting every auto-spawned NetworkPlayer land at the prefab's single
    /// default position, where two players would start stacked inside each other ("aynı
    /// yerde doğup sıkışmasın"). Registers NetworkManager's own ConnectionApprovalCallback
    /// and sets the approval response's Position/Rotation — the NGO-native mechanism for
    /// per-client spawn placement, so no custom spawn RPC or NetworkObject management is
    /// needed here. Round-robins through spawnPoints, so this already generalizes past
    /// exactly 2 players without changes, even though Stage 11A only targets 2.
    /// </summary>
    public class MultiplayerSpawnPoints : MonoBehaviour
    {
        [SerializeField] private Transform[] spawnPoints;

        private int _nextSpawnIndex;

        private void Awake()
        {
            if (NetworkManager.Singleton == null)
            {
                return;
            }

            NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
            NetworkManager.Singleton.ConnectionApprovalCallback += HandleConnectionApproval;
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.ConnectionApprovalCallback -= HandleConnectionApproval;
            }
        }

        private void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            Transform point = NextSpawnPoint();

            response.CreatePlayerObject = true;
            response.Position = point != null ? point.position : Vector3.zero;
            response.Rotation = point != null ? point.rotation : Quaternion.identity;
            response.Approved = true;
            response.Pending = false;

            Debug.Log($"[Shiftworks][MP] Bağlantı onaylandı: clientId={request.ClientNetworkId}, " +
                $"spawnPoint={(point != null ? point.name : "yok (0,0,0)")}.");
        }

        private Transform NextSpawnPoint()
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                return null;
            }

            Transform point = spawnPoints[_nextSpawnIndex % spawnPoints.Length];
            _nextSpawnIndex++;
            return point;
        }
    }
}
