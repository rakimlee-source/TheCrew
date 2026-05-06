using UnityEngine;

public class HeartControls : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float rotateSpeed = 0.5f; 

    [Header("Audio")]
    [SerializeField] AudioClip collectSound;

    [Header("Checkpoint Settings")]
    [Tooltip("Drag the specific Respawn Tile for this heart here.")]
    [SerializeField] Transform respawnTile; 

    void Update()
    {
        // Smooth rotation using World space to prevent gimbal lock
        transform.Rotate(0, rotateSpeed * 100 * Time.deltaTime, 0, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the Player
        if (other.CompareTag("Player"))
        {
            // Try to find the PlayerControls script on the object
            PlayerControls player = other.GetComponent<PlayerControls>();

            if (player != null && respawnTile != null)
            {
                // Extract the Vector3 position and Quaternion rotation from the tile
                Vector3 newPos = respawnTile.position;
                Quaternion newRot = respawnTile.rotation;

                // Send the coordinates to the player's script
                player.UpdateSpawnPoint(newPos, newRot);
            }

            // Play the collection sound at the heart's position
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position, 1f);
            }

            // Remove the heart from the scene
            Destroy(gameObject);
        }
    }
}