using UnityEngine;
using IndentedDialogue;

public class DialogueParserTestGUI : MonoBehaviour
{
    public string fileName;
    public string treeName = "TEST 1";

    DialogueForest forest;

    DialogueNode currentNode;

    Rect rect = new Rect(100, 100, 400, 400);

    void Start()
    {
        forest = new DialogueForest();
        forest.ParseFromFile(fileName);

        ResetRoot();
    }

    private void OnGUI()
    {
        GUI.Window(0, rect, Window, "Dialogue");
    }

    void ResetRoot()
    {
        currentNode = forest[treeName].GetFirstNode();
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
                currentNode = forest[treeName].GetNext(currentNode, i);
                break;
            }
        }
    }
}
