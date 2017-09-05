# Indented Dialogue

### Motivation

When writing dialogues for games, I would often write them first into one text file, and then assemble them in an 'off the shelf' flowgraph-style system such as [Yarn](https://github.com/InfiniteAmmoInc/Yarn). I do that because I personally find text editing much quicker and reliable, and often feel I spend more time arranging the flowcharts then actually writing the text. Just making new nodes and connecting them can cause my brain to get distracted for a moment and I forget what I wanted to write. But then I realized, hey, why don't I just write them once and have them parsed directly from the text file?!  So, here is my solution!

## Advantages
- Write as you think! Write one conversation stream quickly, then add alternatives later..

![alt text](https://i.imgur.com/lqrGSMC.gif "Write as you think")

- ..Or, write with choices in mind first, if you wish..

![alt text](https://i.imgur.com/KZd7nkl.gif "Write as you choose")

- Easily editable. Need to insert a branch? Just press enter and navigate to the right indentation.
- Clear and easily readable.
- No clickety dragity clickety clicking. Work with keyboard only, just like when you write code.
- No spagetti flowgraphs! Everything is as indented!
- Use any text editor as you wish. Since it's a plain text file, you can even edit in notepad!
- C#, Unity-ready parser. Deploy from a serialized DialogueNode tree or read from text files directly ingame

## Disadantages:
- Not so pretty as some flowgraphs .. *Looking at [yarn](https://github.com/InfiniteAmmoInc/Yarn)* (but hey, text editors can look nice too!)
- If the trees are gonna be hyuuuge, sometimes it's hard to see what choices are on the same indentation. However, making indentation lines visible or collapsing regions in text editors like sublime or VS code can greatly help with this problem.
- Currently missing are the stuff in the [TODO](#todo) list


## How to write
- You can write __comments__ similarily to C-like languages, preceeding with ```//```, they will simply be discarded by the parser. There are no block comments.

- You initiate your dialogue with a __tree identifier__, preceeded with a '#', eg. ```# BOB_KITCHEN``` this is then used in game to start that dialogue (using ```dialogueForest["BOB_KITCHEN"]```, see [Accessing nodes](#accessing-nodes). Note that you don't need to have capital letters, or underscores, you can also use spaces, this is just a personal preference, but keep in mind that all white space *around* the identifier name will be trimmed, so ```#BOB KITCHEN``` will be parsed the same as ```# BOB KITCHEN   ```.

- Now comes your tree, a __prompt__ is preceeded with a '-', while a __choice__ is preceeded with a '*'. You must start with a prompt, and then write a choice in a next indented line. Then, you need to alternate between the 2, having prompts and choices on a single line is not allowed.

```
# THIS IS AN IDENTIFIER
- Hi! This is a prompt
	* Hey, this is a choice
		- Oh, really, you call that a choice? Well, here's another prompt!
  ```
  
- To add more choices, write them in the same indentation as other choices

```
# ADDING CHOICES
- Hi! This is a prompt
	* Hey, this is a first choice
		- Ok, well, here's another prompt!
	* This is second choice!
		- This is the answer (prompt) to the second choice
```

- __Link tags__ allow you to jump / goto from choices to disconnected prompts. By writing a tag name in square brackets like ```[ASK]```, before a prompt, you can tag that prompt with a name. Then, you can just add a tag to the end of a choice and it will go to that prompt after you have selected the choice. Here is an example:

```
* Can I ask you something
	- [ASK] Sure, go ahead // This is now a line tagged as 'ASK'
		* Where can I find the closest toilet?
			- It's on the left!
				* Thanks!
				* If I could also ask.. [ASK] // This will jump to the 'ASK' line
		* Where is the closest ATM?
			- It's forward, right next to the bank
				* Thanks!
				* If I could also ask.. [ASK] // This will also jump back to the 'ASK' line
```

### Caveats, warnings and notes
- "Indentation" means "tab", spaces are not considered indentation;
- You must always alternate between prompt - choice - prompt - choice ..
	- If you want to have 2 prompts, you can add a single 'continue' choice in between, like:
```
# EMPHASIS EXAMPLE
- OMG!
	* What happened!?
		- There..
			* ... // using three dots as a 'continue' statement
				- ..is..
					* ...
						- ..no..
							* ...
								- ..COFFEEE!!!!
									* oh damn..
```
- Having prompts and choices mixed in a single indentation is also not allowed;
- The first line in a dialogue MUST have no indentation (this will be allowed in the future update, see [TODO](#todo));
- You MUST not have 2 or more indentations more between following lines. Parsing requires this;
- Multiline text editing is currently not supported, but you can use '\n' character for a new line;
- Make sure you don't have empty sections between or after tree identifiers. This will return a "Tree is defined, but no dialogue found" error and parsing will fail.
- Files without tree identifiers will not be parsed

## Parsing in Unity
Dialogues are parsed from text files into a DialogueForest object. This is done as simply as calling:
```
var dialogueForest = DialogueForest.ParseFromFile("DialogueData\myfile.idp");
```
The best is to look into examples in the project like the DialogueManager.cs and DialogueParserGUI.cs

## Accessing nodes
- As mentioned, after parsing, the dialogue structure is stored inside a DialogueForest. 
- The DialogueForest contains one or more DialogueTrees, which can be accessed with eg. ```dialogueForest["BOB KITCHEN"]```, where ```BOB KITCHEN``` is the tree identifier (see in [How to write](#how-to-write)).
- A DialogueTree contains DialogueNodes, where each node is a prompt with choices and links to next prompts.
- To get the first node of the dialogue, call ```dialogueTree.GetFirstNode()```
- To get the next node, call ```dialogueTree.GetNext(currentNode, choiceIndex)```

You can see these in use in the DialogueParserGUI.cs script

## Serialization
Classes using the system: DialogueNode, DialogueTree and DialogueForest, are all serializable, therefore there are multiple ways you can save the dialogues:
- The first obvious way is to just keep them in the **text form, and parse them when you need them** at runtime, eg. at start of the game. This will incur some cost of parsing the text, especially if there is a lot of dialogues. Parsing process is not very optimized, it uses a bunch of regex matches and linq operations, so you might have freezes when loading and possible GC spikes. Also, more importantly, this also of course means that anyone can view the dialogues on the system, and worse, anyone can modify them, potentially breaking the game if parsing fails. This mode is however very useful for production/debugging, eg. in the case where the game is built, but a writer can modify dialogues independently. But this method should NEVER be used for release.
- The second way is to parse the files and **save them into a serialized DialogueForest field** (public field or tagged with [SerializeField]) in one of the components in a scene. Since they are saved as scene data, they will be built together with the game, and therefore inaccessible to the player.
- If you wish to save dialogues into a non-human-readable external file, you can use **binary serialization built-into a DialogueForest class**. Call ```DialogueForest.SerializeIntoBinary("path/dialogue.dat")``` to save it, and ```DialogueForest.DeserializeFromBinary("path/dialogue.dat")``` to load it. Now you can access the dialogues by calling ```dialogueForest["NAME OF YOUR TREE"]``` as explained in [Accessing nodes](#accessing-nodes]. (Note that path, filename and extension can be any name you wish)

## TODO:
- Multiline text editing
- Reusing choices, by linking them in similar way to choice>prompt link tags
- Conditions, randomization, and events. They currently need to be implemented by the user
- Syntax highlighter for sublime
