using UnityEngine;

[CreateAssetMenu(fileName = "QuestDialogue", menuName = "Dialogue/Quest Dialogue")]
public class QuestDialogueData : ScriptableObject
{
    public DialogNode rootNode; // Rotnoden för dialogträdet
}