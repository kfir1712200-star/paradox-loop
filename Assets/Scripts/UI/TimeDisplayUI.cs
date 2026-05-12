using UnityEngine;
using TMPro;

/// <summary>
/// UI display for the in-game clock and day.
/// Shows current time, day name, and loop number.
/// </summary>
public class TimeDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI loopText;
    [SerializeField] private TextMeshProUGUI speedText;

    [Header("Settings")]
    [SerializeField] private bool useHebrewDayNames = true;

    void OnEnable()
    {
        GameEvents.OnTimeChanged += UpdateTimeDisplay;
        GameEvents.OnDayChanged += UpdateDayDisplay;
        GameEvents.OnLoopStart += UpdateLoopDisplay;
        GameEvents.OnTimeSpeedChanged += UpdateSpeedDisplay;
        GameEvents.OnTimePaused += OnTimePaused;
    }

    void OnDisable()
    {
        GameEvents.OnTimeChanged -= UpdateTimeDisplay;
        GameEvents.OnDayChanged -= UpdateDayDisplay;
        GameEvents.OnLoopStart -= UpdateLoopDisplay;
        GameEvents.OnTimeSpeedChanged -= UpdateSpeedDisplay;
        GameEvents.OnTimePaused -= OnTimePaused;
    }

    void Start()
    {
        // Initial display
        if (TimeManager.Instance != null)
        {
            UpdateTimeDisplay(TimeManager.Instance.CurrentHour, TimeManager.Instance.CurrentMinute);
            UpdateDayDisplay(TimeManager.Instance.CurrentDay);
        }
        if (LoopManager.Instance != null)
        {
            UpdateLoopDisplay(LoopManager.Instance.CurrentLoop);
        }
    }

    private void UpdateTimeDisplay(int hour, int minute)
    {
        if (timeText != null)
        {
            timeText.text = $"{hour:D2}:{minute:D2}";
        }
    }

    private void UpdateDayDisplay(int dayIndex)
    {
        if (dayText != null)
        {
            if (useHebrewDayNames && TimeManager.Instance != null)
            {
                dayText.text = TimeManager.Instance.GetDayNameHebrew();
            }
            else if (TimeManager.Instance != null)
            {
                dayText.text = TimeManager.Instance.GetDayNameEnglish();
            }
        }
    }

    private void UpdateLoopDisplay(int loopNumber)
    {
        if (loopText != null)
        {
            loopText.text = $"Loop #{loopNumber}";
        }
    }

    private void UpdateSpeedDisplay(float multiplier)
    {
        if (speedText != null)
        {
            speedText.text = multiplier > 1f ? $"x{multiplier:F0}" : "";
        }
    }

    private void OnTimePaused(bool isPaused)
    {
        if (timeText != null)
        {
            // Blink effect when paused could be added here
            timeText.color = isPaused ? new Color(1f, 1f, 1f, 0.5f) : Color.white;
        }
    }
}
