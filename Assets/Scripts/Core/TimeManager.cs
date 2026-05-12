using UnityEngine;
using System;

/// <summary>
/// Manages in-game time for Project Paradox Loop.
/// Each loop is a school week: Sunday (0) to Friday (5).
/// Time runs from 07:00 to 14:00 each day.
/// </summary>
public class TimeManager : Singleton<TimeManager>
{
    [Header("Time Settings")]
    [Tooltip("How many real seconds = 1 in-game minute")]
    [SerializeField] private float secondsPerGameMinute = 2f;
    
    [Tooltip("Starting hour of each school day")]
    [SerializeField] private int dayStartHour = 7;
    
    [Tooltip("Ending hour of each school day")]
    [SerializeField] private int dayEndHour = 14;
    
    [Header("Time Speed")]
    [SerializeField] private float timeMultiplier = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // Current time state
    private int currentDay;      // 0=Sunday, 1=Monday, ... 5=Friday
    private int currentHour;
    private int currentMinute;
    private float minuteTimer;
    private bool isTimePaused;
    private bool isTimeRunning;

    // Day names in Hebrew
    private static readonly string[] DayNamesHebrew = { "יום א׳", "יום ב׳", "יום ג׳", "יום ד׳", "יום ה׳", "יום ו׳" };
    private static readonly string[] DayNamesEnglish = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

    // Properties
    public int CurrentDay => currentDay;
    public int CurrentHour => currentHour;
    public int CurrentMinute => currentMinute;
    public bool IsTimePaused => isTimePaused;
    public bool IsTimeRunning => isTimeRunning;
    public float TimeMultiplier => timeMultiplier;
    public float SecondsPerGameMinute => secondsPerGameMinute;
    
    /// <summary>Returns time as a normalized value 0-1 across the entire week</summary>
    public float WeekProgress
    {
        get
        {
            float totalMinutesInWeek = 6f * (dayEndHour - dayStartHour) * 60f;
            float currentTotalMinutes = currentDay * (dayEndHour - dayStartHour) * 60f 
                                       + (currentHour - dayStartHour) * 60f 
                                       + currentMinute;
            return Mathf.Clamp01(currentTotalMinutes / totalMinutesInWeek);
        }
    }
    
    /// <summary>Returns time as a normalized value 0-1 across the current day</summary>
    public float DayProgress
    {
        get
        {
            float totalMinutesInDay = (dayEndHour - dayStartHour) * 60f;
            float currentDayMinutes = (currentHour - dayStartHour) * 60f + currentMinute;
            return Mathf.Clamp01(currentDayMinutes / totalMinutesInDay);
        }
    }

    protected override void OnSingletonAwake()
    {
        ResetToWeekStart();
    }

    void Update()
    {
        if (!isTimeRunning || isTimePaused) return;

        minuteTimer += Time.deltaTime * timeMultiplier;

        if (minuteTimer >= secondsPerGameMinute)
        {
            minuteTimer -= secondsPerGameMinute;
            AdvanceMinute();
        }
    }

    private void AdvanceMinute()
    {
        int oldHour = currentHour;
        currentMinute++;

        if (currentMinute >= 60)
        {
            currentMinute = 0;
            currentHour++;

            if (currentHour != oldHour)
            {
                GameEvents.InvokeHourChanged(currentHour);
            }

            // End of school day
            if (currentHour >= dayEndHour)
            {
                EndDay();
                return;
            }
        }

        GameEvents.InvokeTimeChanged(currentHour, currentMinute);
    }

    private void EndDay()
    {
        // Friday end = week end = loop trigger
        if (currentDay >= 5)
        {
            isTimeRunning = false;
            GameEvents.InvokeWeekEnd();
            return;
        }

        // Advance to next day
        currentDay++;
        currentHour = dayStartHour;
        currentMinute = 0;
        minuteTimer = 0f;

        GameEvents.InvokeDayChanged(currentDay);
        GameEvents.InvokeTimeChanged(currentHour, currentMinute);
    }

    // ═══════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════

    /// <summary>Reset time to Sunday 07:00</summary>
    public void ResetToWeekStart()
    {
        currentDay = 0;
        currentHour = dayStartHour;
        currentMinute = 0;
        minuteTimer = 0f;
        isTimePaused = false;
        isTimeRunning = true;

        GameEvents.InvokeDayChanged(currentDay);
        GameEvents.InvokeTimeChanged(currentHour, currentMinute);
    }

    /// <summary>Pause/Resume time flow</summary>
    public void SetTimePaused(bool paused)
    {
        isTimePaused = paused;
        GameEvents.InvokeTimePaused(paused);
    }

    /// <summary>Set time speed multiplier (1x, 2x, 4x)</summary>
    public void SetTimeMultiplier(float multiplier)
    {
        timeMultiplier = Mathf.Clamp(multiplier, 0.25f, 8f);
        GameEvents.InvokeTimeSpeedChanged(timeMultiplier);
    }

    /// <summary>Skip to a specific day and hour</summary>
    public void SkipTo(int day, int hour)
    {
        if (day < currentDay || (day == currentDay && hour <= currentHour))
        {
            Debug.LogWarning("TimeManager: Cannot skip backwards in time!");
            return;
        }

        currentDay = Mathf.Clamp(day, 0, 5);
        currentHour = Mathf.Clamp(hour, dayStartHour, dayEndHour - 1);
        currentMinute = 0;
        minuteTimer = 0f;

        GameEvents.InvokeDayChanged(currentDay);
        GameEvents.InvokeTimeChanged(currentHour, currentMinute);
    }

    /// <summary>Skip to next day morning</summary>
    public void SkipToNextDay()
    {
        if (currentDay >= 5)
        {
            EndDay();
            return;
        }
        SkipTo(currentDay + 1, dayStartHour);
    }

    /// <summary>Stop time completely (for cutscenes, etc.)</summary>
    public void StopTime()
    {
        isTimeRunning = false;
    }

    /// <summary>Resume time after stopping</summary>
    public void StartTime()
    {
        isTimeRunning = true;
    }

    // ═══════════════════════════════════════════
    // DISPLAY HELPERS
    // ═══════════════════════════════════════════

    /// <summary>Get formatted time string like "08:30"</summary>
    public string GetTimeString()
    {
        return $"{currentHour:D2}:{currentMinute:D2}";
    }

    /// <summary>Get day name in Hebrew</summary>
    public string GetDayNameHebrew()
    {
        return DayNamesHebrew[Mathf.Clamp(currentDay, 0, 5)];
    }

    /// <summary>Get day name in English</summary>
    public string GetDayNameEnglish()
    {
        return DayNamesEnglish[Mathf.Clamp(currentDay, 0, 5)];
    }

    // ═══════════════════════════════════════════
    // SAVE/LOAD
    // ═══════════════════════════════════════════

    [Serializable]
    public class TimeData
    {
        public int day;
        public int hour;
        public int minute;
        public float multiplier;
    }

    public TimeData GetSaveData()
    {
        return new TimeData
        {
            day = currentDay,
            hour = currentHour,
            minute = currentMinute,
            multiplier = timeMultiplier
        };
    }

    public void LoadSaveData(TimeData data)
    {
        currentDay = data.day;
        currentHour = data.hour;
        currentMinute = data.minute;
        timeMultiplier = data.multiplier;
        minuteTimer = 0f;
        isTimeRunning = true;
        isTimePaused = false;

        GameEvents.InvokeDayChanged(currentDay);
        GameEvents.InvokeTimeChanged(currentHour, currentMinute);
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.yellow;

        string info = $"Day: {GetDayNameEnglish()} | Time: {GetTimeString()} | Speed: x{timeMultiplier} | {(isTimePaused ? "PAUSED" : "RUNNING")}";
        GUI.Label(new Rect(10, 10, 600, 30), info, style);
    }
#endif
}
