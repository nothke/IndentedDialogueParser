using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using System.Linq;

public class DialogueNode
{
    public string prompt;
    public string[] choices;
    public int[] links;
}


public class DialogueParser : MonoBehaviour
{
    struct TabLine
    {
        public bool isPrompt;
        public string text;
        public int lineIndex;
        public int parent;
        public int indent;

        public List<int> links;

        public TabLine(bool isPrompt, string text, int lineIndex, int indent, int parent)
        {
            this.isPrompt = isPrompt;
            this.text = text;
            this.lineIndex = lineIndex;
            this.indent = indent;
            this.parent = parent;

            links = new List<int>();
        }

        public List<TabLine> GetLinkedTabLines(List<TabLine> tabLines)
        {
            if (links.Count == 0) return null;

            List<TabLine> outTabLines = new List<TabLine>();

            for (int i = 0; i < links.Count; i++)
                outTabLines.Add(tabLines[links[i]]);

            return outTabLines;
        }

        public override string ToString()
        {
            return string.Format("{0} || Line Index: {1}, Indent: {2}, Parent: {3}, Links num: {4}", text, lineIndex, indent, parent, links.Count);
        }
    }

    List<TabLine> tabLines = new List<TabLine>();

    Dictionary<int, DialogueNode> nodes = new Dictionary<int, DialogueNode>();
    DialogueNode rootNode;

    public string fileName;

    public DialogueNode GetFirstNode()
    {
        return rootNode;
    }

    public void Parse()
    {
        string[] lines = File.ReadAllLines(fileName);

        int li = 0; // line index

        for (int ln = 0; ln < lines.Length; ln++)
        {
            string line = lines[ln];

            int indent = 0;

            for (int ci = 0; ci < line.Length; ci++)
            {
                if (line[ci] == '\t') indent++; // count indent
                else
                {
                    bool lineIsPrompt = line[ci] == '-';
                    bool lineIsChoice = line[ci] == '*';

                    if (lineIsPrompt || lineIsChoice)
                    {
                        string text = line.Substring(ci + 1).Trim();
                        TabLine tabLine = new TabLine(lineIsPrompt, text, li, indent, 0);

                        if (indent != 0) // if no indent, it's root
                        {
                            // otherwise look up for the parent
                            for (int i = tabLines.Count - 1; i >= 0; i--) // replace with li?
                            {
                                if (tabLines[i].indent == indent - 1)
                                {
                                    // add a parent
                                    tabLine.parent = tabLines[i].lineIndex;

                                    // add this as a child to a parent
                                    tabLines[i].links.Add(li);

                                    break;
                                }
                            }
                        }

                        tabLines.Add(tabLine);
                        li++; // increment index by one

                        break;
                    }
                }
            }
        }

        for (int i = 0; i < tabLines.Count; i++)
        {
            Debug.Log(tabLines[i].ToString());
        }

        // Create all nodes as dictionary entries linked by 'line' number
        for (int tli = 0; tli < tabLines.Count; tli++)
        {
            if (tabLines[tli].isPrompt)
            {
                DialogueNode node = new DialogueNode();
                node.prompt = tabLines[tli].text;
                node.links = tabLines[tli].links.ToArray();

                int linkCount = tabLines[tli].links.Count;

                node.links = new int[linkCount];
                node.choices = new string[linkCount];

                for (int i = 0; i < linkCount; i++)
                {
                    int tabLineIndex = tabLines[tli].links[i];
                    node.links[i] = tabLineIndex + 1;
                    node.choices[i] = tabLines[tabLineIndex].text;
                }

                nodes.Add(tabLines[tli].lineIndex, node);

                if (tabLines[tli].indent == 0)
                    rootNode = node;
            }
        }

        tabLines.Clear();
        tabLines = null;
    }

    public DialogueNode GetNext(DialogueNode current, int choice)
    {
        Debug.Log("Fetching line " + current.links[choice]);
        return nodes[current.links[choice]];
    }
}
