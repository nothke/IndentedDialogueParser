using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

using System.Linq;

namespace IndentedDialogue
{
    public class DialogueTree
    {
        public DialogueNode rootNode;
        public Dictionary<int, DialogueNode> nodes = new Dictionary<int, DialogueNode>();

        /// <summary>
        /// Gets the next dialogue node. Returns null when it's the end.
        /// </summary>
        /// <param name="current">The node you are currently on</param>
        /// <param name="choice">The index of choice</param>
        public DialogueNode GetNext(DialogueNode current, int choice)
        {
            int link = current.links[choice];

            if (link == -1) return null; // Dialogue ends
            if (!nodes.ContainsKey(link)) return null;

            Debug.Log("Fetching line " + link);
            return nodes[link];
        }

        public DialogueNode GetFirstNode()
        {
            return rootNode;
        }
    }

    public class DialogueNode
    {
        public string prompt;
        public string[] choices;
        public int[] links;
    }


    public class DialogueParser : MonoBehaviour
    {
        public Dictionary<string, DialogueTree> trees = new Dictionary<string, DialogueTree>();

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

        //List<TabLine> tabLines = new List<TabLine>();

        public string fileName;



        public void Parse()
        {
            string[] lines = File.ReadAllLines(fileName);

            int li = 0; // line index

            Dictionary<string, List<TabLine>> tabLinesDict = new Dictionary<string, List<TabLine>>();

            List<TabLine> tabLines = new List<TabLine>();

            for (int ln = 0; ln < lines.Length; ln++)
            {
                string line = lines[ln];

                int indent = 0;

                for (int ci = 0; ci < line.Length; ci++)
                {
                    if (line[ci] == '\t') indent++; // count indent
                    else
                    {
                        bool lineIsTreeInitializer = line[ci] == '#';
                        bool lineIsPrompt = line[ci] == '-';
                        bool lineIsChoice = line[ci] == '*';

                        if (lineIsTreeInitializer)
                        {
                            string name = line.Substring(ci + 1).Trim();

                            tabLines = new List<TabLine>();

                            if (tabLinesDict.ContainsKey(name))
                            {
                                Error(string.Format("Found more than one trees called '{0}', this is not allowed", name));
                                return;
                            }

                            tabLinesDict.Add(name, tabLines);
                            li = 0;

                            break;
                        }

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

            foreach (var pair in tabLinesDict)
            {
                var debugTabLines = pair.Value;

                foreach (var tabLine in debugTabLines)
                {
                    Debug.Log(pair.Key + " || " + tabLine.ToString());
                }
            }

            DialogueTree currentTree = null;

            Debug.Log("Num of trees: " + tabLinesDict.Count);

            foreach (var pair in tabLinesDict)
            {
                tabLines = pair.Value;

                currentTree = new DialogueTree();
                trees.Add(pair.Key, currentTree);

                // Create all nodes as dictionary entries linked by 'line' number
                for (int tli = 0; tli < tabLines.Count; tli++)
                {
                    if (tabLines[tli].isPrompt)
                    {
                        DialogueNode node = new DialogueNode();
                        node.prompt = tabLines[tli].text;

                        // Choices
                        int linkCount = tabLines[tli].links.Count;

                        node.links = new int[linkCount];
                        node.choices = new string[linkCount];

                        for (int i = 0; i < linkCount; i++)
                        {
                            int tabLineIndex = tabLines[tli].links[i];
                            node.links[i] = tabLineIndex + 1;
                            Debug.Assert(tabLineIndex < tabLines.Count, "tablineIndex is out of range");
                            node.choices[i] = tabLines[tabLineIndex].text;
                        }

                        currentTree.nodes.Add(tabLines[tli].lineIndex, node);

                        if (tabLines[tli].indent == 0)
                            currentTree.rootNode = node;
                    }
                }
            }
        }

        void Error(string message)
        {
            Debug.LogError("IDP: Parsing unsuccessful: " + message);
        }
    }
}
