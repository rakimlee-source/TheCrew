using UnityEngine;

public class ObjectReset : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    [SerializeField] private float resetYThreshold = -30f;

    [Header("Jar & Bridge Settings")]
    [SerializeField] private bool isJar = false;           // ← Check this box on the Jar only
    [SerializeField] private GameObject bridgePivot;       // ← Drag your BridgePivot here
    [SerializeField] private float bridgeRotationDegrees = 90f;
    [SerializeField] private float bridgeRotationTime = 3f;

    private bool bridgeIsFalling = false;
    private float bridgeTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (transform.position.y < resetYThreshold)
        {
            if (isJar)
                HandleJarFall();
            else
                ResetObject();
        }

        // Smooth bridge rotation
        if (bridgeIsFalling && bridgePivot != null)
        {
            float rotationThisFrame = (bridgeRotationDegrees / bridgeRotationTime) * Time.deltaTime;
            bridgePivot.transform.Rotate(0, 0, rotationThisFrame);

            bridgeTimer += Time.deltaTime;

            if (bridgeTimer >= bridgeRotationTime)
            {
                bridgeIsFalling = false;
                Debug.Log("Bridge rotation finished");
                Destroy(gameObject);
            }
        }
    }

    private void HandleJarFall()
    {
        if (!bridgeIsFalling)
        {
            Debug.Log("Jar fell → rotating bridge");
            bridgeIsFalling = true;
            bridgeTimer = 0f;
        }
    }

    private void ResetObject()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = startPosition;
        transform.rotation = startRotation;
    }
}