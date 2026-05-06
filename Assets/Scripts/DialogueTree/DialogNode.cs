using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogChoice
{
    public string choiceText;      
    
    [SerializeReference]           // ← DETTA ÄR DEN VIKTIGA RADEN
    public DialogNode nextNode;    // Nu hanterar Unity referensen korrekt
}

[System.Serializable]
public class DialogNode
{
    public string speakerName;
    public string text;
    public Sprite portrait;
    
    public List<DialogChoice> choices = new List<DialogChoice>(); // bättre default
}