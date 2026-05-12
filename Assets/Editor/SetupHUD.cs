using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public static class SetupHUD
{
    [MenuItem("Mimi/Setup HUD Canvas")]
    public static void Run() => Execute();

    public static void Execute()
    {
        // Remove existing HUD if present
        var existing = GameObject.Find("HUD_Canvas");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
            Debug.Log("[SetupHUD] Removed existing HUD_Canvas");
        }

        // ── CANVAS ─────────────────────────────────────────
        var canvas = new GameObject("HUD_Canvas");
        var canvasComp = canvas.AddComponent<Canvas>();
        canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasComp.sortingOrder = 10;

        var scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvas.AddComponent<GraphicRaycaster>();

        // ── TIME PANEL (top-right) ──────────────────────────
        var timePanel = CreatePanel("TimePanel", canvas.transform,
            new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-20, -20), new Vector2(300, 110),
            new Color(0, 0, 0, 0.55f));

        var dayTMP   = CreateTMP("DayText",  timePanel.transform, 12, 60, -12, -8,  18, new Color(0.85f,0.85f,1f), TextAlignmentOptions.TopRight, "Sunday");
        var timeTMP  = CreateTMP("TimeText", timePanel.transform, 12, 30, -12, -32, 28, Color.white, TextAlignmentOptions.TopRight, "07:00");
        timeTMP.fontStyle = FontStyles.Bold;
        var loopTMP  = CreateTMP("LoopText", timePanel.transform, 12,  4, -12, -72, 14, new Color(0.6f,0.8f,1f), TextAlignmentOptions.BottomRight, "Loop #1");
        var speedTMP = CreateTMP("SpeedText", timePanel.transform, 0, -8, -60, -24, 12, new Color(1f,0.9f,0.3f), TextAlignmentOptions.TopLeft, "");
        // Speed anchored top-left of panel
        var speedRT = speedTMP.GetComponent<RectTransform>();
        speedRT.anchorMin = new Vector2(0,1); speedRT.anchorMax = new Vector2(0,1);
        speedRT.pivot = new Vector2(0,1);
        speedRT.anchoredPosition = new Vector2(8,-8);
        speedRT.sizeDelta = new Vector2(60,24);

        var timeDisplayUI = timePanel.AddComponent<TimeDisplayUI>();
        var tso = new SerializedObject(timeDisplayUI);
        tso.FindProperty("timeText").objectReferenceValue = timeTMP;
        tso.FindProperty("dayText").objectReferenceValue  = dayTMP;
        tso.FindProperty("loopText").objectReferenceValue = loopTMP;
        tso.FindProperty("speedText").objectReferenceValue = speedTMP;
        tso.FindProperty("useHebrewDayNames").boolValue   = false;
        tso.ApplyModifiedProperties();

        // ── INTERACTION PROMPT (bottom-center) ──────────────
        var promptPanel = CreatePanel("InteractionPrompt", canvas.transform,
            new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 80), new Vector2(400, 50),
            new Color(0, 0, 0, 0.6f));
        promptPanel.SetActive(false);

        var promptTMP = CreateTMP("PromptText", promptPanel.transform, 12, 4, -12, -4, 20, Color.white, TextAlignmentOptions.Center, "[E] Interact");

        // ── CROSSHAIR (center) ──────────────────────────────
        var crosshairGO = new GameObject("Crosshair");
        crosshairGO.transform.SetParent(canvas.transform, false);
        var chRT = crosshairGO.AddComponent<RectTransform>();
        chRT.anchorMin = new Vector2(0.5f, 0.5f);
        chRT.anchorMax = new Vector2(0.5f, 0.5f);
        chRT.sizeDelta = new Vector2(8, 8);
        chRT.anchoredPosition = Vector2.zero;
        var chImg = crosshairGO.AddComponent<Image>();
        chImg.color = new Color(1,1,1,0.7f);

        // ── WIRE InteractionSystem on Mimi#1 ────────────────
        var mimi = GameObject.Find("Mimi#1");
        if (mimi != null)
        {
            var sys = mimi.GetComponent<InteractionSystem>();
            if (sys != null)
            {
                var iso = new SerializedObject(sys);
                iso.FindProperty("promptPanel").objectReferenceValue = promptPanel;
                iso.FindProperty("promptText").objectReferenceValue  = promptTMP;
                if (Camera.main != null)
                    iso.FindProperty("raycastOrigin").objectReferenceValue = Camera.main.transform;
                iso.ApplyModifiedProperties();
                Debug.Log("[SetupHUD] InteractionSystem wired on Mimi#1");
            }
        }

        EditorUtility.SetDirty(canvas);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[SetupHUD] HUD_Canvas created with TimePanel + InteractionPrompt + Crosshair");
        Selection.activeGameObject = canvas;
    }

    // ── helpers ────────────────────────────────────────────

    static GameObject CreatePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 pos, Vector2 size, Color bgColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        return go;
    }

    static TextMeshProUGUI CreateTMP(string name, Transform parent,
        float left, float bottom, float right, float top,
        float fontSize, Color color, TextAlignmentOptions align, string defaultText)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(right, top);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = align;
        return tmp;
    }
}
