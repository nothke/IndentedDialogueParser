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
    DialogueNode[] nodes;

    public string fileName;

    void Start()
    {
        Parse();
    }

    void Parse()
    {
        string[] lines = File.ReadAllLines(fileName);

        List<string> dialines = new List<string>();

        foreach (var line in lines)
        {
            // find number of tabs preceeding line
            int tabNum = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '\t') tabNum++;
                else if (line[i] == '-')
                {
                    dialines.Add(line.Substring(i + 1).Trim());
                    break;
                }
                else break;
            }

            Debug.Log(tabNum);
        }

        foreach (var line in dialines)
        {
            Debug.Log(line);
        }

    }
}
