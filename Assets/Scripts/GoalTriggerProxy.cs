using UnityEngine;

public class GoalTriggerProxy : MonoBehaviour 
{
    private void OnTriggerEnter(Collider other) 
    {
        // Check if the ball hit the trigger
        if (other.CompareTag("Kickable")) 
        {
            // Look for the GoalSystem on the parent
            GoalSystem system = GetComponentInParent<GoalSystem>();
            
            if (system != null) 
            {
                system.RegisterScore(other);
            }
            else 
            {
                Debug.LogWarning("GoalTriggerProxy: No GoalSystem found on parent!");
            }
        }
    }
}