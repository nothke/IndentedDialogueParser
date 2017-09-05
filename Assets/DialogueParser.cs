using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace IndentedDialogue
{
    [System.Serializable]
    public class DialogueForest
    {
        public string name;
        [SerializeField]
        DialogueTree[] treesArray;
        [System.NonSerialized]
        Dictionary<string, DialogueTree> treeDict = new Dictionary<string, DialogueTree>();

        public void SerializeIntoBinary(string fileName)
        {
            treesArray = treeDict.Values.ToArray();

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(fileName);
            bf.Serialize(file, this);
            file.Close();
        }

        public static DialogueForest DeserializeFromBinary(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Debug.LogError("Deserialization attempt failed. File " + fileName + " doesn't exist");
                return null;
            }

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(fileName, FileMode.Open);
            DialogueForest forest = (DialogueForest)bf.Deserialize(file);
            file.Close();

            forest.PopulateDict();

            return forest;
        }

        public void ParseFromFile(string fileName)
        {
            treeDict = DialogueParser.ParseIntoTreeDict(fileName);

            treesArray = treeDict.Values.ToArray();

            foreach (var tree in treesArray)
                tree.SerializeFromDict();

            //Debug.Log("TreeDict size: " + treeDict.Count + ", array size: " + treesArray.Length);

        }

        public void CombineWith(DialogueForest forest)
        {
            var parsedDict = forest.treeDict;
            parsedDict.ToList().ForEach(x => treeDict.Add(x.Key, x.Value));
        }

        void PopulateDict()
        {
            treeDict = new Dictionary<string, DialogueTree>();

            foreach (var tree in treesArray)
            {
                treeDict.Add(tree.name, tree);
            }
        }
    }

    [System.Serializable]
    public class DialogueTree
    {
        public string name;

        public DialogueNode rootNode;

        [SerializeField]
        DialogueNode[] nodesArray;

        Dictionary<int, DialogueNode> nodes = new Dictionary<int, DialogueNode>();

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

            //Debug.Log("Fetching line " + link);
            return nodes[link];
        }

        public DialogueNode GetFirstNode()
        {
            return rootNode;
        }

        public void AddToDict(int nodeIndex, DialogueNode node)
        {
            nodes.Add(nodeIndex, node);
        }

        public void SerializeFromDict()
        {
            nodesArray = nodes.Values.ToArray();
        }

        public void DeserializeToDict()
        {
            if (nodes == null)
                nodes = new Dictionary<int, DialogueNode>();
            else nodes.Clear();

            for (int i = 0; i < nodesArray.Length; i++)
            {
                nodes.Add(nodesArray[i].index, nodesArray[i]);
            }
        }
    }

    [System.Serializable]
    public class DialogueNode
    {
        public string prompt;
        public int index;
        public string[] choices;
        public int[] links;
    }

    public static class DialogueParser
    {
        struct TabLine
        {
            public bool isPrompt; // if false, it's a choice
            public string text;
            public int lineIndex;
            public int parent;
            public int indent;
            public string tag;

            // on a prompt this links to choices
            // on a choice, this links to a prompt
            public List<int> links;

            public TabLine(bool isPrompt, string text, int lineIndex, int indent, int parent)
            {
                this.isPrompt = isPrompt;
                this.text = text;
                this.lineIndex = lineIndex;
                this.indent = indent;
                this.parent = parent;

                tag = string.Empty;

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

        static string tagRegexPattern = Regex.Escape("[") + "(.*?)]";

        public static Dictionary<string, DialogueTree> ParseIntoTreeDict(string fileName)
        {
            Dictionary<string, DialogueTree> treeDict = new Dictionary<string, DialogueTree>();

            string[] lines = File.ReadAllLines(fileName);

            int li = 0; // line index

            Dictionary<string, List<TabLine>> tabLinesDict = new Dictionary<string, List<TabLine>>();
            Dictionary<string, int> tags = new Dictionary<string, int>();

            List<TabLine> tabLines = new List<TabLine>();

            string prevTreeName = string.Empty;

            for (int ln = 0; ln < lines.Length; ln++)
            {
                string line = lines[ln];
                line = RemoveComments(line);

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

                            if (tabLinesDict.Count != 0)
                            {
                                // RUN THE 'SECOND PASS', link everything
                                Link(ref tabLines, ref tags);

                                tabLinesDict[prevTreeName] = new List<TabLine>(tabLines);

                                tabLines.Clear();
                                tags.Clear();

                            }

                            prevTreeName = name;

                            if (tabLinesDict.ContainsKey(name))
                            {
                                Error(string.Format("Found more than one trees called '{0}', this is not allowed", name));
                                return null;
                            }

                            tabLinesDict.Add(name, null); // We add a null
                            li = 0;

                            break;
                        }

                        if (lineIsPrompt || lineIsChoice)
                        {
                            string text = line;

                            string tag = Regex.Match(text, tagRegexPattern).Value;
                            text = Regex.Replace(text, tagRegexPattern, string.Empty);

                            text = text.Substring(ci + 1).Trim();

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
                                        //tabLines[i].links.Add(li);

                                        break;
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(tag))
                            {
                                if (lineIsPrompt)
                                    tags.Add(tag, li);
                                else if (lineIsChoice)
                                {
                                    tabLine.tag = tag;

                                    //if (tags.ContainsKey(tag))
                                    //tabLine.lineIndex = tags[tag] - 1; // Hack, this will fool the parser that the line is where it should be
                                    //else Error("Tag " + tag + " doesn't exist");
                                }
                            }

                            tabLines.Add(tabLine);
                            li++; // increment index by one

                            break;
                        }
                    }
                }
            }

            Link(ref tabLines, ref tags);

            tabLinesDict[prevTreeName] = new List<TabLine>(tabLines);

            tabLines.Clear();
            tags.Clear();

            /*// Debug tabLinesDict
            Debug.Log("TABLINING COMPLETE: " + tabLinesDict.Count);

            
            foreach (var pair in tabLinesDict)
            {
                var debugTabLines = pair.Value;

                Debug.Log("TABLINER, size of tablines for " + pair.Key + " is " + debugTabLines.Count);

                foreach (var tabLine in debugTabLines)
                {
                    Debug.Log("TABLINER: " + pair.Key + " || " + tabLine.ToString());
                }
            }
            */

            DialogueTree currentTree = null;

            //Debug.Log("Num of trees: " + tabLinesDict.Count);

            foreach (var pair in tabLinesDict)
            {
                List<TabLine> tabs = pair.Value;

                currentTree = new DialogueTree();

                // Create all nodes as dictionary entries linked by 'line' number
                for (int tli = 0; tli < tabs.Count; tli++)
                {
                    TabLine tab = tabs[tli];

                    if (tab.isPrompt)
                    {
                        DialogueNode node = new DialogueNode();
                        node.prompt = tab.text;

                        // Choices
                        int linkCount = tab.links.Count;

                        node.links = new int[linkCount];
                        node.choices = new string[linkCount];

                        for (int i = 0; i < linkCount; i++)
                        {
                            int choiceIndex = tab.links[i];

                            TabLine choiceTabline = tabs[choiceIndex];

                            node.links[i] = choiceTabline.links[0];

                            Debug.Assert(choiceIndex < tabs.Count, "tablineIndex is out of range");
                            node.choices[i] = choiceTabline.text;
                        }

                        node.index = tabs[tli].lineIndex;
                        currentTree.AddToDict(tabs[tli].lineIndex, node);

                        if (tabs[tli].indent == 0)
                            currentTree.rootNode = node;
                    }
                }

                currentTree.name = pair.Key;
                treeDict.Add(pair.Key, currentTree);
            }

            Debug.Log("Finished parsing. Found " + treeDict.Count + " trees in " + fileName);

            return treeDict;
        }

        static void Link(ref List<TabLine> tablines, ref Dictionary<string, int> tags)
        {
            if (tablines.Count == 0)
            {
                Error("A tree is defined, but it is empty. Perhaps you have two tree identifiers (#) following each other, or a tree identifier (#) is at the end of the page. This is not allowed");
                return;
            }

            //Debug.Log("Found tags: " + tags.Count);

            foreach (var tab in tablines)
            {
                //Debug.Log("LINKER: " + tab.text);

                if (tab.isPrompt) continue;

                // Add a 'reference to the choice' to parent
                tablines[tab.parent].links.Add(tab.lineIndex);

                // Add a link to the next prompt
                if (tab.tag != string.Empty)
                {
                    if (tags.ContainsKey(tab.tag))
                        tab.links.Add(tags[tab.tag]); // using a tag
                    else Error("Tag " + tab.tag + " is refered to, but it doesn't exist");
                }
                else
                    tab.links.Add(tab.lineIndex + 1); // otherwise, next prompt is in the next line
            }
        }

        static string RemoveComments(string str, string delimiter = "//")
        {
            return Regex.Replace(str, delimiter + ".+", string.Empty);
        }

        static void Error(string message)
        {
            Debug.LogError("IDP: Parsing unsuccessful: " + message);
        }
    }
}
