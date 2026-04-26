using UnityEngine;

public class ObjectReset : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    [SerializeField] float resetYThreshold = -30f;

    void Start()
    {
        // Remember the position and rotation at the very start of the game
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Check if the object has fallen below the threshold
        if (transform.position.y < resetYThreshold)
        {
            ResetObject();
        }
    }

    public void ResetObject()
    {
        // Stop all physical movement before moving the object
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Snap back to start
        transform.position = startPosition;
        transform.rotation = startRotation;
    }
}