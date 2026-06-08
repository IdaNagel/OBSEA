using UnityEngine;

public class DentexMovement : MonoBehaviour
{
    [Header("Area Settings")]
    public MeshRenderer groundPlane; 
    public float minHeight = 1.0f; 
    public float maxHeight = 3.0f;   

    [Header("Movement")]
    public float speed = 2.0f;
    public float turnSpeed = 0.8f;

    private Vector3 targetPoint;
    private Quaternion targetRotation;

    void Start()
    {
                if (groundPlane == null)
    {
        groundPlane = GameObject.Find("SeaBottom").GetComponent<MeshRenderer>();
    }
    
        PickNewRandomPoint();
    }

    void Update()
    {
        // 1. Bevegelse fremover
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // 2. Sjekk om vi har nådd målet eller er utafor planet
        if (Vector3.Distance(transform.position, targetPoint) < 2.0f || IsOutsidePlane())
        {
            PickNewRandomPoint();
        }

        // 3. Myk rotasjon mot det tilfeldige punktet
        Vector3 direction = targetPoint - transform.position;
        if (direction != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    void PickNewRandomPoint()
    {
        if (groundPlane == null) return;

        Bounds b = groundPlane.bounds;
        float randomX = Random.Range(b.min.x, b.max.x);
        float randomZ = Random.Range(b.min.z, b.max.z);
        float randomY = Random.Range(minHeight, maxHeight);

        targetPoint = new Vector3(randomX, randomY, randomZ);
    }

    bool IsOutsidePlane()
    {
        Bounds b = groundPlane.bounds;
        return !b.Contains(new Vector3(transform.position.x, b.center.y, transform.position.z));
    }
}