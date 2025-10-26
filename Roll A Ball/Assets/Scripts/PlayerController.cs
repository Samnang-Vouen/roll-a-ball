using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float speed = 20f;
    public Text countText;
    public Text winText;

    [Header("UI")]
    [Tooltip("Optional: Assign a UI Button from the Canvas. If left empty, a Restart button will be created at runtime when you win.")]
    [SerializeField] private Button restartButton;
    [Tooltip("Optional: Hotkey to restart after winning.")]
    [SerializeField] private KeyCode restartHotkey = KeyCode.R;

    [Header("Collision/Bounce")]
    [Tooltip("Impulse applied to the player when colliding with an enemy.")]
    public float bounceImpulse = 8f;

    [Tooltip("Extra upward boost added on enemy collision to make the bounce feel snappier.")]
    public float bounceUpwardBoost = 2f;

    Rigidbody m_Rigidbody;
    Vector3 m_Movement;
    int m_Count;
    int m_TotalPickups;
    bool m_HasWon;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Count = 0;
        m_HasWon = false;

        // Count all pickups present at start; fallback to 12 if none found (keeps old behavior working)
        m_TotalPickups = GameObject.FindGameObjectsWithTag("PickUp").Length;
        if (m_TotalPickups == 0) m_TotalPickups = 12;

        // Prepare UI
        if (winText != null) winText.text = "";
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(Restart);
        }

        // Ensure there is an EventSystem so the button can be clicked
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        setCountText();
    } 

    void FixedUpdate() {
        if (m_HasWon) return; // Stop movement after winning
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        m_Movement.Set(horizontal, 0f, vertical);

        m_Rigidbody.AddForce(m_Movement * speed);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            m_Count++;
            setCountText();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Bounce only when colliding with an Enemy (detected by Enemy component on the other object or its parent)
        var enemy = collision.collider.GetComponentInParent<Enemy>();
        if (enemy == null || m_HasWon) return;

        // Use the first contact normal to compute a bounce direction
        if (collision.contactCount > 0)
        {
            ContactPoint contact = collision.GetContact(0);
            Vector3 normal = contact.normal; // Points from the other collider towards this rigidbody

            // Reflect current velocity and add an impulse in the collision normal direction
            Vector3 reflected = Vector3.Reflect(m_Rigidbody.velocity, normal);
            m_Rigidbody.velocity = reflected;

            Vector3 impulse = normal * bounceImpulse + Vector3.up * bounceUpwardBoost;
            m_Rigidbody.AddForce(impulse, ForceMode.Impulse);
        }
    }

    void setCountText()
    {
        if (countText != null)
            countText.text = "Count: " + m_Count.ToString() + " / " + m_TotalPickups.ToString();

        if (!m_HasWon && m_Count >= m_TotalPickups)
        {
            m_HasWon = true;
            if (winText != null) winText.text = "You Win!";
            if (restartButton == null)
            {
                restartButton = CreateRestartButton();
                restartButton.onClick.AddListener(Restart);
            }
            if (restartButton != null) restartButton.gameObject.SetActive(true);
        }
    }

    // Creates a simple Restart button under the first Canvas found
    private Button CreateRestartButton()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        var btnGO = new GameObject("RestartButton");
        btnGO.transform.SetParent(canvas.transform, false);

        var rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, -40);
        rt.sizeDelta = new Vector2(160, 40);

        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f, 0.9f);

        var button = btnGO.AddComponent<Button>();

        var textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        var trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        var txt = textGO.AddComponent<Text>();
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = "Restart";
        txt.font = GetDefaultFont();
        txt.color = Color.white;
        txt.raycastTarget = false;

        button.gameObject.SetActive(false);
        return button;
    }

    // Handles Unity version differences for built-in fonts
    private static Font GetDefaultFont()
    {
        // Newer Unity versions deprecate Arial.ttf as a built-in; try LegacyRuntime.ttf first
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch {}
        if (f != null) return f;
        try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch {}
        return f; // may be null; Unity will still render but it's better to assign via Inspector if available
    }

    private void Update()
    {
        if (m_HasWon && Input.GetKeyDown(restartHotkey))
        {
            Restart();
        }
    }

    public void Restart()
    {
        // Reload the current active scene
        var scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
