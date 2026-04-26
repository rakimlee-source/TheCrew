using UnityEngine;

public class HeartControls : MonoBehaviour
{
    [SerializeField] float rotateSpeed = 0.5f; 
    [SerializeField] AudioClip collectSound;

    void Update()
    {
        // Smooth rotation
        transform.Rotate(0, rotateSpeed * 100 * Time.deltaTime, 0, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position, 1f);
            }

            Destroy(gameObject);
        }
    }
}