using UnityEngine;
using System;

/**
 * EcologyBrain handles environmental logic for Chromis and Dentex.
 * Updated with specific formulas for both species and web override support.
 */
public class EcologyBrain : MonoBehaviour
{
    // Statiske variabler så managerne dine enkelt kan hente scoren
    public static int ChromisScore { get; private set; }
    public static int DentexScore { get; private set; }

    [Header("Manual Overrides (For Testing)")]
    public bool useSystemTime = true;
    [Range(0, 23)] public float manualHour = 12;
    [Range(1, 365)] public int manualDay = 100;

    // --- CHROMIS KONSTANTER ---
    private const float CH_MIN = 1.756202f;
    private const float CH_MAX = 14.674733f;

    // --- DENTEX KONSTANTER ---
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
            
            Debug.Log($"EcologyBrain (Web Update): Day {manualDay}, Hour {manualHour}");
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

        // ---CHROMIS ---
        float chRaw = 8.215518f 
            + (2.720429f) * Mathf.Sin(2 * pi * h / 24f) 
            + (1.281165f) * Mathf.Cos(2 * pi * h / 24f) 
            + (2.542171f) * Mathf.Sin(2 * pi * d / 365f) 
            + (2.741654f) * Mathf.Cos(2 * pi * d / 365f);

        float chNorm = (chRaw - CH_MIN) / (CH_MAX - CH_MIN);
        ChromisScore = Mathf.Clamp(Mathf.RoundToInt(1 + 9 * chNorm), 0, 10);

        // ---DENTEX ---
        float deRaw = 4.418598f 
            + (-0.925028f) * Mathf.Sin(2 * pi * h / 24f) 
            + (-1.129073f) * Mathf.Cos(2 * pi * h / 24f) 
            + (-0.876612f) * Mathf.Sin(2 * pi * d / 365f) 
            + (-1.513570f) * Mathf.Cos(2 * pi * d / 365f);

        float deNorm = (deRaw - DE_MIN) / (DE_MAX - DE_MIN);
        DentexScore = Mathf.Clamp(Mathf.RoundToInt(1 + 9 * deNorm), 0, 10);
    }
}