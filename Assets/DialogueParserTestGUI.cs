using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IndentedDialogue;

public class DialogueParserTestGUI : MonoBehaviour
{
    DialogueParser _dialogueParser;
    DialogueParser dialogueParser { get { if (!_dialogueParser) _dialogueParser = GetComponent<DialogueParser>(); return _dialogueParser; } }

    void Start()
    {
        dialogueParser.Parse();
        ResetRoot();
    }

    Rect rect = new Rect(100, 100, 400, 400);

    public const string TREE_NAME = "TEST 1";

    private void OnGUI()
    {
        GUI.Window(0, rect, Window, "Dialogue");
    }

    DialogueNode currentNode;

    void ResetRoot()
    {
        currentNode = dialogueParser.trees[TREE_NAME].GetFirstNode();
    }

    void Window(int id)
    {
        if (currentNode == null)
        {
            GUILayout.Label("Dialogue ended");
            if (GUILayout.Button("Restart"))
                ResetRoot();

            return;
        }


        GUILayout.Label(currentNode.prompt);

        if (currentNode.choices == null || currentNode.choices.Length == 0)
        {
            if (GUILayout.Button("Dialogue Ended. Restart"))
                ResetRoot();

            return;
        }

        Debug.Assert(currentNode != null, "Node is null");
        Debug.Assert(currentNode.choices != null, "Choices are null");
        Debug.Assert(currentNode.choices.Length != 0, "Choices are empty");

        for (int i = 0; i < currentNode.choices.Length; i++)
        {
            if (GUILayout.Button(currentNode.choices[i]))
            {
                currentNode = dialogueParser.trees[TREE_NAME].GetNext(currentNode, i);
                //currentNode = dialogueParser.GetNext(currentNode, i);
                break;
            }
        }
    }
}
