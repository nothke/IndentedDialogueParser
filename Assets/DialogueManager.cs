///
/// Combines all trees from a folder
/// 
/// Add this component into the scene, at it will parse all files at start
/// 
/// You can get any dialogue tree by simply querying by tree definition,
/// eg. DialogueManager.Trees[DIALOGUE 1]
/// 
/// To start a dialogue, get DialogueManager.Trees[DIALOGUE 1].GetFirstNode();
/// To get the next node, get DialogueManager.Trees[DIALOGUE 1].GetNext(*node*, *choiceNumber*);
/// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using System.IO;
using IndentedDialogue;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    void Awake() { instance = this; }

    Dictionary<string, DialogueTree> treeDict = new Dictionary<string, DialogueTree>();

    public static Dictionary<string, DialogueTree> Trees { get { return instance.treeDict; } }

    public string folderName = "DialogueData";

    public void Start()
    {
        string[] fileNames = Directory.GetFiles(folderName);

        foreach (var fileName in fileNames)
        {
            var parsedDict = DialogueParser.ParseIntoTreeDict(fileName);
            parsedDict.ToList().ForEach(x => treeDict.Add(x.Key, x.Value));
        }
    }
}