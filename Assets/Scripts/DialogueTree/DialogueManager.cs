using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Image portraitImage;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;

    [Header("Cutscene Settings")]
    [Tooltip("Hur snabbt kameran vänder sig mot Skull God")]
    public float cameraTurnSpeed = 5f;

    [Tooltip("Justera höjden på fokus-punkten (t.ex. 1.5 för huvudet)")]
    public float lookAtHeight = 1.8f;

    private DialogNode currentNode;
    private Transform currentNPC;
    private Quaternion originalRotation;
    private bool isInDialogue = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        dialoguePanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinuePressed);
    }

    public void StartDialogue(DialogNode rootNode, Transform npcTransform)
    {
        currentNode = rootNode;
        currentNPC = npcTransform;
        isInDialogue = true;

        originalRotation = Camera.main.transform.rotation;

        dialoguePanel.SetActive(true);
        DisplayCurrentNode();

        Debug.Log("🎥 Cutscene startar – fokuserar på " + npcTransform.name);
        StartCoroutine(CameraFocusOnNPC());
    }

    private void DisplayCurrentNode()
    {
        speakerNameText.text = currentNode.speakerName;
        dialogueText.text = currentNode.text;
        portraitImage.sprite = currentNode.portrait;

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);
    }

    private IEnumerator CameraFocusOnNPC()
    {
        while (isInDialogue && currentNPC != null)
        {
            Vector3 targetPoint = currentNPC.position + Vector3.up * lookAtHeight;
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint - Camera.main.transform.position);

            Camera.main.transform.rotation = Quaternion.Slerp(
                Camera.main.transform.rotation,
                targetRotation,
                Time.deltaTime * cameraTurnSpeed
            );

            yield return null;
        }
    }

    private void OnContinuePressed()
    {
        EndDialogue();
    }

    private void EndDialogue()
    {
        isInDialogue = false;
        dialoguePanel.SetActive(false);

        // Återställ kameran
        if (Camera.main != null)
            Camera.main.transform.rotation = originalRotation;

        Debug.Log("🎥 Cutscene avslutad – kamera återställd");
    }
}