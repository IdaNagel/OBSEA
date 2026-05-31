using UnityEngine;
using UnityEngine.SceneManagement;

public class WebManager : MonoBehaviour
{
    [Header("Script References")]
    public EcologyBrain ecologyBrain;
    public SunController sunController;

    private void Awake()
    {
        // Find all instances of WebManager
        WebManager[] managers = Object.FindObjectsByType<WebManager>(FindObjectsInactive.Include);

        if (managers.Length > 1)
        {
            // If I am not the first one in the list, destroy myself
            if (managers[0].gameObject != this.gameObject)
            {
                Debug.Log("Destroying duplicate Web_Manager.");
                Destroy(this.gameObject);
                return;
            }
        }
        
        // If I am the primary manager, persist across scenes
        DontDestroyOnLoad(this.gameObject);
        Debug.Log("Web_Manager initialized and marked DontDestroyOnLoad.");
        
        // Subscribe to scene loading to re-hook references in new scenes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Always unsubscribe from events when destroyed to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Using FindAnyObjectByType to comply with Unity 6 performance standards
        if (ecologyBrain == null)
        {
            ecologyBrain = Object.FindAnyObjectByType<EcologyBrain>();
        }
        if (sunController == null)
        {
            sunController = Object.FindAnyObjectByType<SunController>();
        }
    }

    public void ChangeScene(string sceneName)
    {
        Debug.Log("Web Trigger: Loading scene -> " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void UpdateSimulationTime(string dateTimeString)
    {
        Debug.Log("Web Trigger: Data received -> " + dateTimeString);

        // 1. Update EcologyBrain using the continuous single string layout
        if (ecologyBrain != null)
        {
            ecologyBrain.UpdateSimulationTime(dateTimeString);
        }
        else
        {
            // Using FindAnyObjectByType to safely fetch the reference if missing
            ecologyBrain = Object.FindAnyObjectByType<EcologyBrain>();
            if (ecologyBrain != null) ecologyBrain.UpdateSimulationTime(dateTimeString);
        }

        // 2. Process split strings for SunController which still requires individual tokens
        string[] parts = dateTimeString.Split(' ');
        if (parts.Length < 2) return;

        string datePart = parts[0]; 
        string timePart = parts[1]; 

        if (sunController != null)
        {
            sunController.UpdateSimulationTime(datePart, timePart);
        }
        else
        {
            // Using FindAnyObjectByType to safely fetch the reference if missing
            sunController = Object.FindAnyObjectByType<SunController>();
            if (sunController != null) sunController.UpdateSimulationTime(datePart, timePart);
        }
    }
}