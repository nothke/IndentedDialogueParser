using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using System.Linq;

public class DialogueNode
{
    public string answer;
    public string[] choices;
    public DialogueNode[] links;
}


public class DialogueParser : MonoBehaviour
{
    struct TabLine
    {
        public string text;
        public int line;
        public int parent;
        public int indent;

        public List<int> links;

        public TabLine(string text, int line, int indent, int parent)
        {
            this.text = text;
            this.line = line;
            this.indent = indent;
            this.parent = parent;

            links = new List<int>();
        }
    }

    List<TabLine> tabLines = new List<TabLine>();

    List<DialogueNode> nodes = new List<DialogueNode>();

    public string fileName;

    public DialogueNode GetFirstNode()
    {
        return nodes[0];
    }

    public void Parse()
    {
        string[] lines = File.ReadAllLines(fileName);

        List<string> diaLines = new List<string>();

        //Dictionary<int, List<string>> tabbedDict = new Dictionary<int, List<string>>();

        // isolate only dialogue lines

        for (int ln = 0; ln < lines.Length; ln++)
        {
            string line = lines[ln];

            int indent = 0;
            int li = 0; // line index

            for (int ci = 0; ci < line.Length; ci++)
            {
                if (line[ci] == '\t') indent++;
                else
                {
                    if (line[ci] == '-') // if line starts with - it's a dialogue line
                    {
                        li++; // increment index by one

                        string text = line.Substring(ci + 1).Trim();
                        TabLine tabLine = new TabLine(text, li, indent, 0);

                        if (indent != 0) // if no indent, it's root
                        {
                            // otherwise look up for the parent
                            for (int i = tabLines.Count - 1; i >= 0; i--) // replace with li?
                            {
                                if (tabLines[i].indent == indent - 1)
                                {
                                    // add a parent
                                    tabLine.parent = tabLines[i].line;

                                    // add this as a child to a parent
                                    tabLines[i].links.Add(li);

                                    break;
                                }
                            }
                        }

                        tabLines.Add(tabLine);

                        break;
                    }
                }
            }
        }

        // go through each tabline and make a node
        for (int tli = 0; tli < tabLines.Count; tli++)
        {
            int ln = tabLines[tli].line;

            // find all that link to it
            List<int> linkedLines = tabLines[tli].links;

            List<TabLine> linkedTabLines = new List<TabLine>();

            // find all links by line
            for (int ltli = 0; ltli < linkedLines.Count; ltli++)
                linkedTabLines.Add(tabLines[linkedLines[ltli]]);

            string[] linkedTexts = linkedTabLines.Select(x => x.text).ToArray();

            DialogueNode node = new DialogueNode();
            node.choices = linkedTexts;
            //node.links = linkedLinks;
        }
    }
}
