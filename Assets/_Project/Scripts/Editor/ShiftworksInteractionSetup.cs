using Shiftworks.Items;
using Shiftworks.Player;
using Shiftworks.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shiftworks.EditorTools
{
    /// <summary>
    /// Idempotent editor tooling that wires PlayerInteractor + prompt UI into the existing
    /// Player prefab and makes the existing WorkshopPrototype test boxes carryable.
    /// Safe to run repeatedly: every step checks for an existing child/component first and
    /// only adds what is missing, never duplicating anything.
    /// </summary>
    public static class ShiftworksInteractionSetup
    {
        private const string PrefabPath = "Assets/_Project/Prefabs/Player/Player.prefab";
        private const string ScenePath = "Assets/_Project/Scenes/WorkshopPrototype.unity";
        private static readonly string[] CarryableBoxNames = { "TestBox_1", "TestBox_2", "TestBox_3", "TestBox_4" };

        [MenuItem("Shiftworks/Setup/3. Update Player Prefab (Interaction)")]
        public static void UpdatePlayerPrefabMenuItem()
        {
            UpdatePlayerPrefab();
        }

        [MenuItem("Shiftworks/Setup/4. Make Workshop Boxes Carryable")]
        public static void MakeTestBoxesCarryableMenuItem()
        {
            MakeTestBoxesCarryable();
        }

        [MenuItem("Shiftworks/Setup/Apply Interaction System (Prefab + Scene)")]
        public static void ApplyInteractionSystem()
        {
            UpdatePlayerPrefab();
            MakeTestBoxesCarryable();
        }

        private static void UpdatePlayerPrefab()
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefabAsset == null)
            {
                Debug.LogError($"[Shiftworks] Player prefab bulunamadı: {PrefabPath}. Önce Stage 1 kurulum menüsünü çalıştırın.");
                return;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                Transform cameraPivot = root.transform.Find("CameraPivot");
                Transform cameraTransform = cameraPivot != null ? cameraPivot.Find("PlayerCamera") : null;
                if (cameraTransform == null)
                {
                    Debug.LogError("[Shiftworks] Player prefabında CameraPivot/PlayerCamera bulunamadı, güncelleme atlandı.");
                    return;
                }

                Transform holdPoint = cameraTransform.Find("HoldPoint");
                if (holdPoint == null)
                {
                    GameObject holdPointGo = new GameObject("HoldPoint");
                    holdPointGo.transform.SetParent(cameraTransform);
                    holdPointGo.transform.localPosition = new Vector3(0f, 0f, 1.5f);
                    holdPointGo.transform.localRotation = Quaternion.identity;
                    holdPoint = holdPointGo.transform;
                }

                InteractionPromptUI promptUI = GetOrCreatePromptUI(root.transform);

                PlayerInteractor interactor = root.GetComponent<PlayerInteractor>();
                if (interactor == null)
                {
                    interactor = root.AddComponent<PlayerInteractor>();
                }

                SerializedObject serializedInteractor = new SerializedObject(interactor);
                serializedInteractor.FindProperty("cameraTransform").objectReferenceValue = cameraTransform;
                serializedInteractor.FindProperty("holdPoint").objectReferenceValue = holdPoint;
                serializedInteractor.FindProperty("promptUI").objectReferenceValue = promptUI;
                serializedInteractor.ApplyModifiedProperties();

                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log($"[Shiftworks] Player prefab etkileşim sistemiyle güncellendi: {PrefabPath}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static InteractionPromptUI GetOrCreatePromptUI(Transform playerRoot)
        {
            Transform canvasTransform = playerRoot.Find("InteractionPromptCanvas");
            if (canvasTransform != null)
            {
                return canvasTransform.GetComponent<InteractionPromptUI>();
            }

            GameObject canvasGo = new GameObject("InteractionPromptCanvas");
            canvasGo.transform.SetParent(playerRoot);

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();

            GameObject crosshairGo = new GameObject("Crosshair");
            crosshairGo.transform.SetParent(canvasGo.transform);
            Image crosshairImage = crosshairGo.AddComponent<Image>();
            crosshairImage.color = Color.white;
            RectTransform crosshairRect = crosshairGo.GetComponent<RectTransform>();
            crosshairRect.sizeDelta = new Vector2(6f, 6f);
            crosshairRect.anchorMin = crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
            crosshairRect.anchoredPosition = Vector2.zero;

            GameObject textGo = new GameObject("PromptText");
            textGo.transform.SetParent(canvasGo.transform);
            Text text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 20;
            text.color = Color.white;
            text.text = string.Empty;
            text.enabled = false;
            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(320f, 40f);
            textRect.anchorMin = textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0f, -40f);

            InteractionPromptUI promptUI = canvasGo.AddComponent<InteractionPromptUI>();
            SerializedObject serializedPromptUi = new SerializedObject(promptUI);
            serializedPromptUi.FindProperty("promptText").objectReferenceValue = text;
            serializedPromptUi.ApplyModifiedProperties();

            return promptUI;
        }

        private static void MakeTestBoxesCarryable()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.LogWarning("[Shiftworks] İşlem iptal edildi: açık sahnede kaydedilmemiş değişiklikler vardı.");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            int updatedCount = 0;
            foreach (string boxName in CarryableBoxNames)
            {
                GameObject box = GameObject.Find(boxName);
                if (box == null)
                {
                    Debug.LogWarning($"[Shiftworks] Sahnede '{boxName}' bulunamadı, atlandı.");
                    continue;
                }

                if (!box.TryGetComponent(out Rigidbody rb))
                {
                    rb = box.AddComponent<Rigidbody>();
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                }

                if (!box.TryGetComponent<CarryableItem>(out _))
                {
                    box.AddComponent<CarryableItem>();
                }

                updatedCount++;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Shiftworks] {updatedCount} test kutusu taşınabilir hale getirildi: {ScenePath}");
        }
    }
}
