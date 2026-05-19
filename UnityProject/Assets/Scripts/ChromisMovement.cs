using UnityEngine;

public class ChromisMovement : MonoBehaviour
{
    [Header("Area Settings")]
    public MeshRenderer groundPlane; 
    public float minHeight = 4.0f;   
    public float maxHeight = 8.0f;

    [Header("Movement")]
    public float speed = 3.0f;       
    public float turnSpeed = 2.0f;
    public float sensorDistance = 1.5f; // Hvor langt foran fisken ser

    private Vector3 targetPoint;
    private float changeTimer;
    private float nextChangeTime;

    void Start()
    {
        if (groundPlane == null)
        {
            GameObject found = GameObject.Find("SeaBottom");
            if (found != null) groundPlane = found.GetComponent<MeshRenderer>();
        }

        nextChangeTime = Random.Range(2f, 5f);
        PickNewRandomPoint();
    }

    void Update()
    {
        if (groundPlane == null) return;

        // --- 1. KOLLISJONSSENSOR (RAYCAST) ---
        // Sjekker om vi er i ferd med å krasje i noe med en collider
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, sensorDistance))
        {
            // Vi ser noe! Finn et nytt mål med en gang for å svinge unna
            PickNewRandomPoint();
            changeTimer = 0;
        }

        // --- 2. BEVEGELSE ---
        // Bruker transform.forward direkte for mer stabil styring
        transform.position += transform.forward * speed * Time.deltaTime;

        // --- 3. TIMER OG GRENSER ---
        changeTimer += Time.deltaTime;

        if (changeTimer > nextChangeTime || IsOutsidePlane())
        {
            PickNewRandomPoint();
            changeTimer = 0;
            nextChangeTime = Random.Range(3f, 7f);
        }

        // --- 4. ROTASJON ---
        Vector3 direction = (targetPoint - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    void PickNewRandomPoint()
    {
        if (groundPlane == null) return;
        Bounds b = groundPlane.bounds;
        float margin = 4.0f; 

        targetPoint = new Vector3(
            Random.Range(b.min.x + margin, b.max.x - margin),
            Random.Range(minHeight, maxHeight),
            Random.Range(b.min.z + margin, b.max.z - margin)
        );
    }

    bool IsOutsidePlane()
    {
        if (groundPlane == null) return false;
        Bounds b = groundPlane.bounds;
        bool horizontalCheck = transform.position.x > b.min.x && transform.position.x < b.max.x &&
                               transform.position.z > b.min.z && transform.position.z < b.max.z;
        return !horizontalCheck;
    }

    // Tegner en rød hjelpestrek i Scene-view
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * sensorDistance);
    }
}