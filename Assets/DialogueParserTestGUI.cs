using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IndentedDialogue;

public class DialogueParserTestGUI : MonoBehaviour
{
    public string fileName;
    public string treeName = "TEST 1";

    Dictionary<string, DialogueTree> trees;

    DialogueNode currentNode;

    Rect rect = new Rect(100, 100, 400, 400);

    void Start()
    {
        trees = DialogueParser.ParseIntoTreeDict(fileName);

        DebugTrees();

        ResetRoot();
    }

    void DebugTrees()
    {
        Debug.Log("GUI DEBUG! There are " + trees.Count + " trees");

        foreach (var tree in trees)
        {
            Debug.Log(tree.Key + "; " + tree.Value.rootNode.prompt);
            //Debug.Log(tree.Key + "; " + tree.Value.nodes[0].prompt + "; " + tree.Value.rootNode.prompt);
        }
    }

    private void OnGUI()
    {
        GUI.Window(0, rect, Window, "Dialogue");
    }

    void ResetRoot()
    {
        currentNode = trees[treeName].GetFirstNode();
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
                currentNode = trees[treeName].GetNext(currentNode, i);
                break;
            }
        }
    }
}
