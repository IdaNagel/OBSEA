using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target; 
    public float rotationSpeed = 5.0f;
    public float distance = 60.0f;
    public float height = 1.0f;

    void Update()
    {
        if (target == null) return;
        
        float angle = Time.time * rotationSpeed * Mathf.Deg2Rad;
        
        float x = target.position.x + Mathf.Cos(angle) * distance;
        float z = target.position.z + Mathf.Sin(angle) * distance;

        transform.position = new Vector3(x, target.position.y + height, z);

        transform.LookAt(target);
    }
}
