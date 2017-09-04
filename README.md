# Indented Dialogue

## THIS LIBRARY IS STILL WIP! I RECOMMEND NOT USING IT YET!

### Motivation

When writing dialogues for games, I would often write them first into one text file, and then assemble them in an 'off the shelf' flowgraph-style system such as [Yarn](https://github.com/InfiniteAmmoInc/Yarn). I do that because I personally find text editing much quicker and reliable, and often feel I spend more time arranging the flowcharts then actually writing the text. Just making new nodes and connecting them can cause my brain to get distracted for a moment and I forget what I wanted to write. But then I realized, hey, why don't I just write them once and have them parsed directly from the text file?!  So, here is my solution!

## Advantages
- Write as you think! Write one conversation stream quickly, then add alternatives later..

![alt text](https://i.imgur.com/EoHmodH.gif "Write as you think")

- ..Or, write with choices in mind first, if you wish..

![alt text](https://i.imgur.com/Dlja0EQ.gif "Write as you choose")

- Easily editable. Need to insert a branch? Just press enter and navigate to the right indentation.
- Clear and easily readable.
- No clickety dragity clickety clicking. Work with keyboard only, just like when you write code.
- No spagetti flowgraphs! Everything is as indented!
- Use any text editor as you wish. Since it's a plain text file, you can even edit in notepad!
- C#, Unity-ready parser. Deploy from a serialized DialogueNode tree or read from text files directly ingame

## Disadantages:
- Not so pretty as some flowgraphs .. *Looking at [yarn](https://github.com/InfiniteAmmoInc/Yarn)* (but hey, text editors can look nice too!)
- If the trees are gonna be hyuuuge, sometimes it's hard to see what choices are on the same indentation. However, making indentation lines visible or collapsing regions in text editors like sublime or VS code can greatly help with this problem.
- Currently missing is the stuff in the TODO list

## TODO:
- Currenlty only answer - choice - answer - choice form is supported, you can't have 2 answers in a row.
- Multiline text editing is currently not supported, use '\n' character for a new line
- Reusing parts, links to trees
- Tags for adding features such as requirement checking, randomization, or events. They currently need to be implemented by the user
- Syntax highlighter for sublime
