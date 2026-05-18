using UnityEngine;
using System.Collections.Generic;

public class DentexManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject dentexPrefab;
    public MeshRenderer groundPlane; 
    
    private List<GameObject> activeDentex = new List<GameObject>();

    void Update()
    {
    
        int targetCount = EcologyBrain.DentexScore;

        // make more
        if (activeDentex.Count < targetCount)
        {
            SpawnDentex();
        }
        // Removes
        else if (activeDentex.Count > targetCount)
        {
            RemoveDentex();
        }
    }

    void SpawnDentex()
    {
        if (dentexPrefab == null || groundPlane == null) return;


        Bounds b = groundPlane.bounds;
        Vector3 spawnPos = new Vector3(
            Random.Range(b.min.x, b.max.x),
            Random.Range(1f, 3f), 
            Random.Range(b.min.z, b.max.z)
        );

        GameObject newFish = Instantiate(dentexPrefab, spawnPos, Quaternion.identity);
        activeDentex.Add(newFish);
    }

    void RemoveDentex()
    {
        if (activeDentex.Count > 0)
        {
            GameObject fishToRemove = activeDentex[0];
            activeDentex.RemoveAt(0);
            Destroy(fishToRemove);
        }
    }
}