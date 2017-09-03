using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueParserTestGUI : MonoBehaviour
{
    DialogueParser _dialogueParser;
    DialogueParser dialogueParser { get { if (!_dialogueParser) _dialogueParser = GetComponent<DialogueParser>(); return _dialogueParser; } }

    void Start()
    {
        dialogueParser.Parse();
        currentNode = dialogueParser.GetFirstNode();
    }

    Rect rect = new Rect(100, 100, 400, 400);

    private void OnGUI()
    {
        GUI.Window(0, rect, Window, "Dialogue");
    }

    DialogueNode currentNode;

    void Window(int id)
    {
        GUILayout.Label(currentNode.answer);
    }
}
