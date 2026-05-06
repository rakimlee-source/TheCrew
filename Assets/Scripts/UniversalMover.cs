using UnityEngine;

public class UniversalMover : MonoBehaviour
{
    public enum MovementType { PingPong, Circular }

    [Header("Movement Path")]
    public MovementType movementType = MovementType.PingPong;
    [Tooltip("X=Local Right, Y=Local Up, Z=Local Forward")]
    public Vector3 distance = new Vector3(2f, 0f, 0f); 
    public float speed = 2f;

    [Header("Rotation")]
    public bool rotate = false;
    public Vector3 rotationSpeed = new Vector3(0, 100, 0); 

    [Header("Offset")]
    [Tooltip("Use this to stagger movement for multiple objects")]
    public float timeOffset = 0f;

    private Vector3 startPos;

    void Start()
    {
        // Store the world position where the object begins
        startPos = transform.position;
    }

    void Update()
    {
        float time = (Time.time + timeOffset) * speed;
        Vector3 localOffset = Vector3.zero;

        if (movementType == MovementType.PingPong)
        {
            // All axes use Sine, resulting in straight lines or diagonals
            localOffset.x = Mathf.Sin(time) * distance.x;
            localOffset.y = Mathf.Sin(time) * distance.y;
            localOffset.z = Mathf.Sin(time) * distance.z;
        }
        else if (movementType == MovementType.Circular)
        {
            // To get a circle, one axis must be Sin and the other Cos.
            // By setting it up this way:
            // X & Z creates a horizontal circle.
            // Y & Z creates a vertical circle.
            localOffset.x = Mathf.Cos(time) * distance.x;
            localOffset.y = Mathf.Cos(time) * distance.y;
            localOffset.z = Mathf.Sin(time) * distance.z;
        }

        // Convert the local movement math into world space relative to the object's rotation
        Vector3 worldOffset = transform.TransformDirection(localOffset);
        
        // Apply the offset to our starting point
        transform.position = startPos + worldOffset;

        // Apply local rotation if enabled
        if (rotate)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}