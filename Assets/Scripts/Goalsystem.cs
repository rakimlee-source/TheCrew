using UnityEngine;
using System.Collections;

public class GoalSystem : MonoBehaviour
{
    [Header("Goal Logic")]
    public float teleportDelay = 0.8f;
    public Transform[] goalLocations; 
    public float spinSpeed = 150f; 

    [Header("Victory Effects")]
    public ParticleSystem smokeEffect; // Drag a Smoke Particle System here!

    private int score = 0;
    private bool isRelocating = false;
    private int lastLocationIndex = -1;
    private bool isFinalStage = false;
    private bool gameEnded = false; 

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (isFinalStage && !isRelocating && !gameEnded)
        {
            transform.Rotate(Vector3.forward * spinSpeed * Time.deltaTime, Space.Self);
        }
    }

    public void RegisterScore(Collider ball)
    {
        if (isRelocating || gameEnded) return;
        
        score++;
        Debug.Log($"Goal Scored! Total: {score}");

        if (isFinalStage)
        {
            StartCoroutine(VictorySequence(ball.gameObject));
        }
        else
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.linearVelocity = new Vector3(0, -2f, 0); 
                rb.angularVelocity = Vector3.zero;
            }
            StartCoroutine(RelocateSequence());
        }
    }

    private IEnumerator RelocateSequence()
    {
        isRelocating = true;
        ToggleVisible(false); 

        yield return new WaitForSeconds(teleportDelay);

        if (score >= 3)
        {
            isFinalStage = true;
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }
        else if (goalLocations.Length > 0)
        {
            int newIndex = lastLocationIndex;
            while (newIndex == lastLocationIndex && goalLocations.Length > 1)
            {
                newIndex = Random.Range(0, goalLocations.Length);
            }
            lastLocationIndex = newIndex;

            transform.position = goalLocations[newIndex].position;
            transform.rotation = goalLocations[newIndex].rotation;
        }

        ToggleVisible(true); 
        isRelocating = false;
    }

    private IEnumerator VictorySequence(GameObject ballObject)
    {
        gameEnded = true; 
        
        // 1. Play the smoke effect at the ring's position
        if (smokeEffect != null)
        {
            smokeEffect.transform.position = transform.position;
            smokeEffect.Play();
        }

        // 2. Wait just a tiny bit for the smoke to "cover" the objects
        yield return new WaitForSeconds(0.2f);
        
        // 3. Poof! They're gone
        ToggleVisible(false); 
        if (ballObject != null) ballObject.SetActive(false);

        Debug.Log("CHALLENGE COMPLETE: Up in smoke!");
    }

    void ToggleVisible(bool visible) 
    {
        Renderer[] renders = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renders) r.enabled = visible;
        
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in allColliders) c.enabled = visible;
    }
}