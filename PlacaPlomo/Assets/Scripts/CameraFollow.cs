using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3.5f, -6f);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        // Aplica el offset en el espacio local del coche
        Vector3 desiredPosition = target.TransformPoint(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Mira hacia el coche
        transform.LookAt(target);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}