using UnityEngine;

public class ObjectTrigger : MonoBehaviour
{
    [Header("Setup")]
    public FloatingObjectController targetObject; // The fire object
    public Transform spawnLocation;               // Where it should appear
    
    [Header("Settings")]
    public bool isDisappearTrigger = false;       // Check this if this trigger should hide it
    public bool triggerOnce = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isDisappearTrigger)
            {
                targetObject.Disappear();
            }
            else
            {
                // Move it and show it
                targetObject.Appear(spawnLocation.position, spawnLocation.rotation);
            }

            if (triggerOnce) gameObject.SetActive(false);
        }
    }
}