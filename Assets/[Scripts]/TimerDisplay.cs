using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class TimerDisplay : MonoBehaviour
{
    [Header("Bootstrap Settings")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool createWorldSpaceCanvas = true;
    
    [Tooltip("If WorldSpace: Height offset from this object. If ScreenSpace: Percentage offset from the screen edge (0 to 1).")]
    [SerializeField] private Vector3 smartOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Timer Settings")]
    [SerializeField] private float countdownSeconds = 60f;
    [SerializeField] private bool countdownMode = true;

    [Header("End Sound")]
    [SerializeField] private AudioClip endSound;

    [Header("Debug")]
    [SerializeField] private bool showTimerOnStart = true;

    private TextMeshProUGUI timerText;
    private AudioSource audioSource;
    private float timeRemaining;
    private bool isTimerRunning;
    private bool endSoundPlayed;

    private void Awake()
    {
        timeRemaining = countdownSeconds;
        isTimerRunning = autoStart;
        endSoundPlayed = false;

        EnsureAudioSource();
        EnsureTimerText();
        UpdateDisplay();
    }

    private void Start()
    {
        if (showTimerOnStart)
        {
            UpdateDisplay();
        }
    }

    private void Update()
    {
        if (!isTimerRunning)
        {
            return;
        }

        if (countdownMode)
        {
            timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
        }
        else
        {
            timeRemaining += Time.deltaTime;
        }

        UpdateDisplay();

        if (countdownMode && timeRemaining <= 0f && !endSoundPlayed)
        {
            EndTimer();
        }

        // Dynamically rotate the 3D text to always face the primary viewport camera
        if (createWorldSpaceCanvas && Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
        }
    }

    public void StartTimer()
    {
        isTimerRunning = true;
        endSoundPlayed = false;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        timeRemaining = countdownSeconds;
        endSoundPlayed = false;
        isTimerRunning = autoStart;
        UpdateDisplay();
    }

    private void EnsureAudioSource()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.clip = endSound;
    }

    private void EnsureTimerText()
    {
        if (timerText != null)
        {
            return;
        }

        Camera mainCam = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
        
        var canvasObject = new GameObject("TimerCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        var rect = canvasObject.GetComponent<RectTransform>();

        if (createWorldSpaceCanvas)
        {
            // --- WORLD SPACE MODE (Intelligent Floating Label) ---
            canvasObject.transform.SetParent(transform, false);
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCam;

            // Compute smart dimensions based on font asset tracking guidelines
            rect.sizeDelta = new Vector2(300f, 100f);
            
            // Scaled down uniformly so 1 UI pixel equals 1 millimeter in 3D game coordinates
            rect.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            rect.localPosition = smartOffset;
        }
        else
        {
            // --- SCREEN SPACE MODE (Responsive Viewport Overlay) ---
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Base size matches standard clean UI layout ratios
            rect.sizeDelta = new Vector2(250f, 80f);

            // Use smartOffset as anchor coordinates (e.g. X: 0.5 = Center, Y: 0.9 = Top of screen)
            rect.anchorMin = new Vector2(smartOffset.x, smartOffset.y);
            rect.anchorMax = new Vector2(smartOffset.x, smartOffset.y);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero; 
        }

        // --- CORE TEXT SETUP ---
        var textObject = new GameObject("TimerText", typeof(RectTransform));
        textObject.transform.SetParent(canvasObject.transform, false);

        var textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        timerText = textObject.AddComponent<TextMeshProUGUI>();
        timerText.font = TMP_Settings.defaultFontAsset;
        timerText.fontSize = createWorldSpaceCanvas ? 55 : 40;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.color = Color.white;
        timerText.enableWordWrapping = false;
        timerText.overflowMode = TextOverflowModes.Overflow;
        
        // Apply text auto-sizing properties to stay strictly within dynamic bounds
        timerText.enableAutoSizing = true;
        timerText.fontSizeMin = 12;
        timerText.fontSizeMax = createWorldSpaceCanvas ? 72 : 50;
    }

    private void UpdateDisplay()
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(timeRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void EndTimer()
    {
        isTimerRunning = false;
        timeRemaining = 0f;
        endSoundPlayed = true;
        UpdateDisplay();

        if (audioSource != null && endSound != null)
        {
            audioSource.PlayOneShot(endSound);
        }
    }
}
