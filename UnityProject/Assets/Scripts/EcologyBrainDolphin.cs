using UnityEngine;
using System;

/**
 * EcologyBrainDolphin handles environmental logic for Dentex within Dolphin scene
 */
public class EcologyBrainDolphin : MonoBehaviour
{
    // Static variables
    public static int DentexScore { get; private set; }

    [Header("Manual Overrides (For Testing)")]
    public bool useSystemTime = true;
    [Range(0, 23)] public float manualHour = 12;
    [Range(1, 365)] public int manualDay = 100;


    // Dentex constants
    private const float DE_MIN = 1.540490f;
    private const float DE_MAX = 7.296769f;

    public void UpdateFromWeb(string dateString, string timeString)
    {
        useSystemTime = false;
        try {
            string[] t = timeString.Split(':');
            manualHour = float.Parse(t[0]) + (float.Parse(t[1]) / 60f);

            DateTime parsedDate = DateTime.Parse(dateString);
            manualDay = parsedDate.DayOfYear;
            
            Debug.Log($"EcologyBrainDolphin (Web Update): Day {manualDay}, Hour {manualHour}");
        }
        catch (Exception e) {
            Debug.LogError("Error parsing date/time: " + e.Message);
        }
    }

    void Update()
    {
        float h = useSystemTime ? (float)DateTime.Now.Hour + (DateTime.Now.Minute / 60f) : manualHour;
        int d = useSystemTime ? DateTime.Now.DayOfYear : manualDay;
        float pi = Mathf.PI;

        // Dentex
        float deRaw = 4.418598f 
            + (-0.925028f) * Mathf.Sin(2 * pi * h / 24f) 
            + (-1.129073f) * Mathf.Cos(2 * pi * h / 24f) 
            + (-0.876612f) * Mathf.Sin(2 * pi * d / 365f) 
            + (-1.513570f) * Mathf.Cos(2 * pi * d / 365f);

        float deNorm = (deRaw - DE_MIN) / (DE_MAX - DE_MIN);
        DentexScore = Mathf.Clamp(Mathf.RoundToInt(1 + 9 * deNorm), 0, 10);
    }
}