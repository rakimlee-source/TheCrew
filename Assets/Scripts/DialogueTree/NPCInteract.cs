using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    public QuestDialogueData questDialogueData;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Input.GetKeyDown(KeyCode.E) && other.CompareTag("Player"))
        {
            if (questDialogueData != null && questDialogueData.rootNode != null)
            {
                // Skickar med NPC:ns position så kameran vet vart den ska titta
                DialogueManager.Instance.StartDialogue(questDialogueData.rootNode, transform);
            }
        }
    }
}