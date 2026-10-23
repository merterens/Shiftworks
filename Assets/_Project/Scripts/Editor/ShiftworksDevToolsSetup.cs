using Shiftworks.Debugging;
using Shiftworks.Economy;
using Shiftworks.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// Idempotent editor tooling: adds the editor-only DevelopmentCheats (F8 test money)
    /// component to the existing WorkshopPrototype scene, wired to the existing
    /// EconomyManager and Player. Never touches gameplay systems or MultiplayerPrototype.unity.
    /// </summary>
    public static class ShiftworksDevToolsSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/WorkshopPrototype.unity";

        [MenuItem("Shiftworks/Setup/27. Add Development Cheats To Scene")]
        public static void AddDevelopmentCheatsMenuItem() => AddDevelopmentCheatsToScene();

        private static void AddDevelopmentCheatsToScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            if (GameObject.Find("DevelopmentCheats") != null)
            {
                Debug.Log("[Shiftworks] DevelopmentCheats zaten sahnede var, dokunulmadı.");
                return;
            }

            GameObject economyGo = GameObject.Find("EconomyManager");
            EconomyManager economy = economyGo != null ? economyGo.GetComponent<EconomyManager>() : null;

            GameObject playerGo = GameObject.Find("Player");
            PlayerInteractor playerInteractor = playerGo != null ? playerGo.GetComponent<PlayerInteractor>() : null;

            if (economy == null)
            {
                Debug.LogError("[Shiftworks] EconomyManager bulunamadı. Önce Aşama 5 kurulumu çalıştırılmalı.");
                return;
            }

            GameObject cheatsGo = new GameObject("DevelopmentCheats");
            DevelopmentCheats cheats = cheatsGo.AddComponent<DevelopmentCheats>();
            SerializedObject so = new SerializedObject(cheats);
            SerializedProperty economyProp = so.FindProperty("economyManager");
            if (economyProp != null)
            {
                economyProp.objectReferenceValue = economy;
            }
            SerializedProperty playerProp = so.FindProperty("playerInteractor");
            if (playerProp != null)
            {
                playerProp.objectReferenceValue = playerInteractor;
            }
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[Shiftworks] DevelopmentCheats (F8 → +10.000 $, yalnızca Editor) sahneye eklendi.");
        }
    }
}
