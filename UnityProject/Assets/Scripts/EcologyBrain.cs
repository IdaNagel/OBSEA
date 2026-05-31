using UnityEngine;
using System;

/**
 * EcologyBrain handles environmental logic for Chromis and Dentex.
 * Calculates population scores using mathematical sine/cosine models 
 * and syncs data in real-time with the web browser interface.
 */
public class EcologyBrain : MonoBehaviour
{
    // Static properties allowing global access to calculated species scores
    public static int ChromisScore { get; private set; }
    public static int DentexScore { get; private set; }

    [Header("Manual Overrides (For Testing)")]
    public bool useSystemTime = true;
    [Range(0, 23)] public float manualHour = 12;
    [Range(1, 365)] public int manualDay = 100;

    // --- CHROMIS ECO-MODEL CONSTANTS ---
    private const float CH_MIN = 1.756202f;
    private const float CH_MAX = 14.674733f;

    // --- DENTEX ECO-MODEL CONSTANTS ---
    private const float DE_MIN = 1.540490f;
    private const float DE_MAX = 7.296769f;

    private float webUpdateTimer = 0f;

    /// <summary>
    /// Called by Web_Manager when receiving a synchronized date/time string from JavaScript.
    /// Expects formatting like: "2026-05-31 16:45"
    /// </summary>
    public void UpdateSimulationTime(string combinedDateTime)
    {
        useSystemTime = false;
        try 
        {
            // Split the incoming string into date and time components
            string[] parts = combinedDateTime.Split(' ');
            string dateString = parts[0];
            string timeString = parts[1];

            // Convert time (HH:MM) into a continuous float value (0.0 - 23.99)
            string[] t = timeString.Split(':');
            manualHour = float.Parse(t[0]) + (float.Parse(t[1]) / 60f);

            // Parse date to extract the specific day of the year (1 - 365)
            DateTime parsedDate = DateTime.Parse(dateString);
            manualDay = parsedDate.DayOfYear;
            
            Debug.Log($"EcologyBrain (Web Update Success): Day {manualDay}, Hour {manualHour}");
        }
        catch (Exception e) 
        {
            Debug.LogError("EcologyBrain failed to parse combined date/time: " + e.Message);
        }
    }

    void Update()
    {
        // Establish current evaluation parameters based on system time or web overrides
        float h = useSystemTime ? (float)DateTime.Now.Hour + (DateTime.Now.Minute / 60f) : manualHour;
        int d = useSystemTime ? DateTime.Now.DayOfYear : manualDay;
        float pi = Mathf.PI;

        // --- CHROMIS MATHEMATICAL MODEL ---
        float chRaw = 8.215518f 
            + (2.720429f) * Mathf.Sin(2 * pi * h / 24f) 
            + (1.281165f) * Mathf.Cos(2 * pi * h / 24f) 
            + (2.542171f) * Mathf.Sin(2 * pi * d / 365f) 
            + (2.741654f) * Mathf.Cos(2 * pi * d / 365f);

        // Normalize value between 0.0 and 1.0, then scale to an integer score between 0 and 10
        float chNorm = (chRaw - CH_MIN) / (CH_MAX - CH_MIN);
        ChromisScore = Mathf.Clamp(Mathf.RoundToInt(1 + 9 * chNorm), 0, 10);

        // --- DENTEX MATHEMATICAL MODEL ---
        float deRaw = 4.418598f 
            + (-0.925028f) * Mathf.Sin(2 * pi * h / 24f) 
            + (-1.129073f) * Mathf.Cos(2 * pi * h / 24f) 
            + (-0.876612f) * Mathf.Sin(2 * pi * d / 365f) 
            + (-1.513570f) * Mathf.Cos(2 * pi * d / 365f);

        // Normalize value between 0.0 and 1.0, then scale to an integer score between 0 and 10
        float deNorm = (deRaw - DE_MIN) / (DE_MAX - DE_MIN);
        DentexScore = Mathf.Clamp(Mathf.RoundToInt(1 + 9 * deNorm), 0, 10);
    }

    void LateUpdate()
    {
        // Send calculated ecosystem data back to the webpage every 0.5 seconds to optimize performance
        webUpdateTimer += Time.deltaTime;
        if (webUpdateTimer >= 0.5f)
        {
            webUpdateTimer = 0f;
            
            // Format state data into a structured JSON string payload
            string jsonPayload = "{" +
                $"\"hour\":{manualHour}," +
                $"\"day\":{manualDay}," +
                $"\"chromis\":{ChromisScore}," +
                $"\"dentex\":{DentexScore}" +
            "}";

            // Safely execute JavaScript callback function 'ReceiveEcologyData' if running as a WebGL client
            #if !UNITY_EDITOR && UNITY_WEBGL
            Application.ExternalCall("ReceiveEcologyData", jsonPayload);
            #endif
        }
    }
}