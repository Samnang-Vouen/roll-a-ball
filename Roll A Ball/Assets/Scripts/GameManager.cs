using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Auto-bootstrap so you don't need to place this in every scene
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }
    }

    [Header("Gameplay Settings")]
    [Tooltip("Lives at the start of a New Game")] public int startingLives = 3;
    [Tooltip("Level time limit in seconds")] public float levelTimeSeconds = 60f;

    [Header("Runtime State (read-only)")]
    [SerializeField] private int lives;
    [SerializeField] private float timeRemaining;
    [SerializeField] private bool hasWon;
    [SerializeField] private bool timeExpired;

    [Header("Optional UI (auto-created if empty)")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private Text timerText;
    [SerializeField] private Text livesText;
    [SerializeField] private GameObject timeoutPanel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (lives <= 0) lives = startingLives;
        if (timeRemaining <= 0) timeRemaining = levelTimeSeconds;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureEventSystem();
        EnsureUI();
        hasWon = false;
        timeExpired = false;
        Time.timeScale = 1f; // resume time on scene load
        RefreshUI();
    }

    void Update()
    {
        if (hasWon || timeExpired) return;

        // Countdown only while playing
        if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                OnTimeExpired();
            }
            UpdateTimerText();
        }
    }

    private void OnTimeExpired()
    {
        timeExpired = true;
        Time.timeScale = 0f; // pause gameplay
        ShowTimeoutPanel();
    }

    public void OnPlayerWin()
    {
        hasWon = true;
        HideTimeoutPanel();
        Time.timeScale = 1f;
    }

    public void OnContinueButton()
    {
        if (lives <= 0)
        {
            // No lives left; force New Game
            OnNewGameButton();
            return;
        }

        lives = Mathf.Max(0, lives - 1);
        ResetTimer();
        ReloadCurrentScene();
    }

    public void OnNewGameButton()
    {
        lives = startingLives;
        ResetTimer();
        ReloadCurrentScene();
    }

    private void ResetTimer()
    {
        timeRemaining = levelTimeSeconds;
        hasWon = false;
        timeExpired = false;
        Time.timeScale = 1f;
        RefreshUI();
    }

    private void ReloadCurrentScene()
    {
        HideTimeoutPanel();
        Time.timeScale = 1f;
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    private void RefreshUI()
    {
        UpdateLivesText();
        UpdateTimerText();

        if (continueButton != null)
        {
            continueButton.interactable = lives > 0;
        }
    }

    // ---------- UI helpers ----------

    private void EnsureUI()
    {
        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
            if (uiCanvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                uiCanvas = canvasGO.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }
        }

        if (timerText == null)
        {
            // Place at top-right to avoid overlapping with typical Count text at top-left
            timerText = CreateCornerText("TimerText", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -10));
        }
        if (livesText == null)
        {
            // Place just below the timer at top-right
            livesText = CreateCornerText("LivesText", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-10, -40));
        }
        if (timeoutPanel == null)
        {
            CreateTimeoutPanel();
        }

        RefreshUI();
        HideTimeoutPanel();
    }

    private Text CreateCornerText(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(uiCanvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        // Set pivot based on target corner (left/right, top/bottom)
        float pivotX = (anchorMin.x >= 0.5f) ? 1f : 0f; // right -> 1, left -> 0
        float pivotY = (anchorMin.y >= 0.5f) ? 1f : 0f; // top -> 1, bottom -> 0
        rt.pivot = new Vector2(pivotX, pivotY);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(240, 28);

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.3f);

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        var txt = textGO.AddComponent<Text>();
        txt.font = GetDefaultFont();
        txt.color = Color.white;
        // Align to the corner horizontally (right/left), keep middle vertically
        bool right = anchorMin.x >= 0.5f;
        txt.alignment = right ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
        txt.raycastTarget = false;
        txt.text = "";
        return txt;
    }

    private void CreateTimeoutPanel()
    {
        timeoutPanel = new GameObject("TimeoutPanel");
        timeoutPanel.transform.SetParent(uiCanvas.transform, false);
        var rt = timeoutPanel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var bg = timeoutPanel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.6f);

        // Center container
        var container = new GameObject("Container");
        container.transform.SetParent(timeoutPanel.transform, false);
        var crt = container.AddComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.5f, 0.5f);
        crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(360, 200);
        crt.anchoredPosition = Vector2.zero;

        var panelBg = container.AddComponent<Image>();
        panelBg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        // Message
        var msgGO = new GameObject("Message");
        msgGO.transform.SetParent(container.transform, false);
        var mrt = msgGO.AddComponent<RectTransform>();
        mrt.anchorMin = new Vector2(0.1f, 0.6f);
        mrt.anchorMax = new Vector2(0.9f, 0.9f);
        mrt.offsetMin = Vector2.zero;
        mrt.offsetMax = Vector2.zero;
        var msg = msgGO.AddComponent<Text>();
        msg.font = GetDefaultFont();
        msg.alignment = TextAnchor.MiddleCenter;
        msg.color = Color.white;
        msg.text = "Time's up!";

        // Continue Button
        continueButton = CreateButton(container.transform, "ContinueButton", new Vector2(0.25f, 0.2f), new Vector2(0.45f, 0.45f), "Continue", OnContinueButton);

        // New Game Button
        newGameButton = CreateButton(container.transform, "NewGameButton", new Vector2(0.55f, 0.2f), new Vector2(0.75f, 0.45f), "New Game", OnNewGameButton);

        timeoutPanel.SetActive(false);
    }

    private Button CreateButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, string label, Action onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f, 0.9f);

        var btn = go.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick());

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(go.transform, false);
        var trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        var txt = textGO.AddComponent<Text>();
        txt.font = GetDefaultFont();
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = label;
        txt.raycastTarget = false;

        return btn;
    }

    private void ShowTimeoutPanel()
    {
        if (timeoutPanel != null)
        {
            timeoutPanel.SetActive(true);
            if (continueButton != null) continueButton.interactable = lives > 0;
        }
    }

    private void HideTimeoutPanel()
    {
        if (timeoutPanel != null) timeoutPanel.SetActive(false);
    }

    private void UpdateTimerText()
    {
        if (timerText == null) return;
        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(timeRemaining));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void UpdateLivesText()
    {
        if (livesText == null) return;
        livesText.text = $"Lives: {Mathf.Max(0, lives)}";
    }

    private static Font GetDefaultFont()
    {
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}
        if (f != null) return f;
        try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch {}
        return f;
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
}
