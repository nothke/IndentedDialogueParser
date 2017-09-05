///
/// Combines all trees from a folder
/// 
/// Add this component into the scene, it will parse all files at start
/// 
/// You can get any dialogue tree by simply querying by tree definition,
/// eg. DialogueManager.Trees[DIALOGUE 1]
/// 
/// To start a dialogue, get DialogueManager.Trees[DIALOGUE 1].GetFirstNode();
/// To get the next node, get DialogueManager.Trees[DIALOGUE 1].GetNext(*node*, *choiceNumber*);
/// 

using UnityEngine;

using System.IO;
using IndentedDialogue;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    void Awake() { instance = this; }

    DialogueForest forest;

    public static DialogueForest Trees { get { return instance.forest; } }

    public string folderName = "DialogueData";

    public void Start()
    {
        string[] fileNames = Directory.GetFiles(folderName);

        foreach (var fileName in fileNames)
        {
            DialogueForest f = new DialogueForest();
            f.ParseFromFile(fileName);
            forest.CombineWith(f);
        }
    }
}