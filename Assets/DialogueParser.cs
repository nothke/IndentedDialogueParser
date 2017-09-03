using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DialogueNode
{
    public string answer;
    public string[] choices;
    public int[] links;
}

public class DialogueParser : MonoBehaviour
{
    List<DialogueNode> nodes = new List<DialogueNode>();

    public string fileName;

    public void Parse()
    {
        string[] lines = File.ReadAllLines(fileName);

        List<string> dialines = new List<string>();

        Dictionary<int, List<string>> tabbedDict = new Dictionary<int, List<string>>();

        foreach (var line in lines)
        {
            // find number of tabs preceeding line
            int tabNum = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '\t') tabNum++;
                else
                {
                    if (line[i] == '-') // if line starts with - it's a dialogue line
                    {
                        // manage dict
                        List<string> tabLines;

                        if (tabbedDict.ContainsKey(tabNum))
                            tabLines = tabbedDict[tabNum];
                        else
                        {
                            tabLines = new List<string>();
                            tabbedDict.Add(tabNum, tabLines);
                        }

                        // add
                        tabLines.Add(line.Substring(i + 1).Trim());
                        break;
                    }
                }
            }
        }

        foreach (var pair in tabbedDict)
        {
            DialogueNode node = new DialogueNode();
            node.answer = pair.Value[0];

            if (pair.Value.Count > 1)
            {
                node.choices = pair.Value.ToArray();
                //node.choices = new string[pair.Value.Count - 2];
            }

            nodes.Add(node);
        }

        foreach (var node in nodes)
        {
            Debug.Log(node.answer);
        }
    }
}
