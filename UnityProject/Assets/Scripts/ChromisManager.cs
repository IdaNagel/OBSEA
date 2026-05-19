using UnityEngine;
using System.Collections.Generic;

public class ChromisManager : MonoBehaviour
{
    public GameObject chromisPrefab;
    public MeshRenderer groundPlane;
    
    [Header("Abundance Multiplier")]
    public int fishPerScoreLevel = 3; // Level 1 = 3 fish, Level 2 = 6 fish, etc.

    private List<GameObject> activeFishes = new List<GameObject>();

    void Update()
    {
        // Translate Score (1-10) to actual Fish Count
        int targetFishCount = EcologyBrain.ChromisScore * fishPerScoreLevel;

        // Adjust population
        while (activeFishes.Count < targetFishCount) SpawnFish();
        while (activeFishes.Count > targetFishCount) RemoveFish();
    }

    void SpawnFish()
    {
        if (groundPlane == null) return;

        Bounds b = groundPlane.bounds;

        Vector3 pos = new Vector3(
            Random.Range(b.min.x, b.max.x),
            Random.Range(3.5f, 7.0f), // Chromis height
            Random.Range(b.min.z, b.max.z)
        );

        GameObject f = Instantiate(chromisPrefab, pos, Quaternion.identity);
        f.GetComponent<ChromisMovement>().groundPlane = groundPlane;
        
        var wander = f.GetComponent<ChromisMovement>();
        if (wander != null) wander.groundPlane = groundPlane;
        activeFishes.Add(f);
    }

    void RemoveFish()
    {
        GameObject f = activeFishes[activeFishes.Count - 1];
        activeFishes.RemoveAt(activeFishes.Count - 1);
        Destroy(f);
    }
}