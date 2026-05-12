using UnityEngine;

/// <summary>
/// Controls the day/night cycle based on TimeManager.
/// Rotates the directional light and adjusts ambient lighting.
/// </summary>
public class DayNightCycle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light directionalLight;
    
    [Header("Sun Settings")]
    [Tooltip("Sun rotation at day start (07:00) — morning angle")]
    [SerializeField] private float sunriseAngle = 10f;
    
    [Tooltip("Sun rotation at midday (10:30)")]
    [SerializeField] private float noonAngle = 70f;
    
    [Tooltip("Sun rotation at day end (14:00) — afternoon angle")]
    [SerializeField] private float sunsetAngle = 50f;
    
    [Header("Light Settings")]
    [SerializeField] private float morningIntensity = 0.8f;
    [SerializeField] private float noonIntensity = 1.2f;
    [SerializeField] private float afternoonIntensity = 1.0f;
    
    [Header("Colors")]
    [SerializeField] private Color morningColor = new Color(1f, 0.9f, 0.7f);
    [SerializeField] private Color noonColor = new Color(1f, 1f, 0.95f);
    [SerializeField] private Color afternoonColor = new Color(1f, 0.85f, 0.6f);
    
    [Header("Ambient")]
    [SerializeField] private Color morningAmbient = new Color(0.4f, 0.45f, 0.5f);
    [SerializeField] private Color noonAmbient = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color afternoonAmbient = new Color(0.45f, 0.4f, 0.35f);

    [Header("Smoothing")]
    [SerializeField] private float transitionSpeed = 2f;

    private float targetAngle;
    private float targetIntensity;
    private Color targetColor;
    private Color targetAmbient;

    void Start()
    {
        if (directionalLight == null)
        {
            directionalLight = GetComponent<Light>();
        }
        if (directionalLight == null)
        {
            // Try to find the directional light in the scene
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
        }
    }

    void OnEnable()
    {
        GameEvents.OnTimeChanged += OnTimeChanged;
    }

    void OnDisable()
    {
        GameEvents.OnTimeChanged -= OnTimeChanged;
    }

    void Update()
    {
        if (directionalLight == null) return;

        // Smooth transitions
        Vector3 currentEuler = directionalLight.transform.eulerAngles;
        float smoothAngle = Mathf.LerpAngle(currentEuler.x, targetAngle, Time.deltaTime * transitionSpeed);
        directionalLight.transform.rotation = Quaternion.Euler(smoothAngle, -30f, 0f);

        directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
        directionalLight.color = Color.Lerp(directionalLight.color, targetColor, Time.deltaTime * transitionSpeed);
        RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, targetAmbient, Time.deltaTime * transitionSpeed);
    }

    private void OnTimeChanged(int hour, int minute)
    {
        if (TimeManager.Instance == null) return;

        float dayProgress = TimeManager.Instance.DayProgress; // 0 to 1

        if (dayProgress < 0.5f)
        {
            // Morning to noon (07:00 - 10:30)
            float t = dayProgress / 0.5f;
            targetAngle = Mathf.Lerp(sunriseAngle, noonAngle, t);
            targetIntensity = Mathf.Lerp(morningIntensity, noonIntensity, t);
            targetColor = Color.Lerp(morningColor, noonColor, t);
            targetAmbient = Color.Lerp(morningAmbient, noonAmbient, t);
        }
        else
        {
            // Noon to afternoon (10:30 - 14:00)
            float t = (dayProgress - 0.5f) / 0.5f;
            targetAngle = Mathf.Lerp(noonAngle, sunsetAngle, t);
            targetIntensity = Mathf.Lerp(noonIntensity, afternoonIntensity, t);
            targetColor = Color.Lerp(noonColor, afternoonColor, t);
            targetAmbient = Color.Lerp(noonAmbient, afternoonAmbient, t);
        }
    }
}
