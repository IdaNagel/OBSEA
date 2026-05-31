using UnityEngine;
using System;

public class SunController : MonoBehaviour
{
    [Header("Place")]
    public double latitude = 41.21;  // Oppdatert til Vilanova (OBSEA)
    public double longitude = 1.75;

    [Header("References")]
    public Light sunLight;

    [Header("Settings")]
    public bool useSystemTime = true;
    
    private DateTime internalDateTime;

    void Start()
    {
        internalDateTime = DateTime.UtcNow;
    }

    // Denne metoden kalles fra WebManager.cs
    public void UpdateSimulationTime(string dateString, string timeString)
    {
        Debug.Log("SUN CONTROLLER IS RECEIVING DATA");
        
        useSystemTime = false;
        try {
            string fullString = $"{dateString} {timeString}";
            internalDateTime = DateTime.Parse(fullString);
            Debug.Log("SunController updated via Web: " + internalDateTime);
        }
        catch (Exception e) {
            Debug.LogError("SunController Parse Error: " + e.Message);
        }
    }

    void Update()
    {
        // Hvis vi ikke har fått input fra web, bruk nåtid
        DateTime timeToUse = useSystemTime ? DateTime.UtcNow : internalDateTime;

        // Bruker din statiske DayLight-klasse for kalkulasjon
        Vector3 sunRotation = DayLight.GetLightRotation(timeToUse, latitude, longitude);
        
        if (sunLight != null)
        {
            sunLight.transform.rotation = Quaternion.Euler(sunRotation);
        }
    }
}