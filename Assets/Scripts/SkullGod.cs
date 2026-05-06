using UnityEngine;

public class FloatingObjectController : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float amplitude = 0.5f;
    public float frequency = 1f;

    [Header("Transition Settings")]
    public float scaleSpeed = 5f;
    
    private Vector3 startPosition;
    private Vector3 targetScale;
    private bool isHiding = false;

    void Awake()
    {
        startPosition = transform.position;
        targetScale = transform.localScale;
        
        // Start "hidden"
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        // 1. Handle Smooth Scaling
        Vector3 currentTarget = isHiding ? Vector3.zero : targetScale;
        transform.localScale = Vector3.Lerp(transform.localScale, currentTarget, Time.deltaTime * scaleSpeed);

        // Deactivate object fully once it's small enough to save performance
        if (isHiding && transform.localScale.magnitude < 0.01f)
        {
            gameObject.SetActive(false);
        }

        // 2. Bobbing Logic (only if active)
        float newY = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPosition + new Vector3(0, newY, 0);
    }

    public void Appear(Vector3 newLocation, Quaternion newRotation)
    {
        isHiding = false;
        transform.position = newLocation;
        transform.rotation = newRotation;
        startPosition = newLocation; 
        // Reset bobbing anchor to new spot
        gameObject.SetActive(true);
    }

    public void Disappear()
    {
        isHiding = true;
        // We don't SetActive(false) immediately so the scale-down animation can play
    }
}