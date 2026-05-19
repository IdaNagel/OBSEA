using UnityEngine;
using UnityEngine.SceneManagement;

public class WebManager : MonoBehaviour
{
    [Header("Script References")]
    public EcologyBrain ecologyBrain;
    public SunController sunController;

    public void ChangeScene(string sceneName)
    {
        Debug.Log("Web Trigger: Loading scene " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void UpdateSimulationTime(string dateTimeString)
    {
        Debug.Log("Web Trigger: Data received - " + dateTimeString);

        string[] parts = dateTimeString.Split(' ');
        if (parts.Length < 2) return;

        string datePart = parts[0]; 
        string timePart = parts[1]; 

        if (ecologyBrain != null)
        {
            ecologyBrain.UpdateFromWeb(datePart, timePart);
        }

        if (sunController != null)
        {
            sunController.UpdateFromWeb(datePart, timePart);
        }
    }

private void Awake()
{
    // Finn alle instanser av WebManager
    WebManager[] managers = Object.FindObjectsByType<WebManager>(FindObjectsInactive.Include);

    if (managers.Length > 1)
    {
        // Hvis jeg ikke er den første i listen, slett meg
        if (managers[0].gameObject != this.gameObject)
        {
            Debug.Log("Sletter duplikat av Web_Manager.");
            Destroy(this.gameObject);
            return;
        }
    }
    
    // Hvis jeg er den eneste (eller den første), bli med til neste scene
    DontDestroyOnLoad(this.gameObject);
    Debug.Log("Web_Manager har overlevd Awake og er klar!");
}
} 