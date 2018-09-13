# Ark Save Analyzer
Basically a tool to analyze savegames files for ARK: Survival Evolved

## Contents

* ArkSaveAnalyzer  
a small GUI to present the data in the savegame
* ArkTools  
mostly a translation of Qowyn's [ark-savegame-toolkit](https://github.com/Qowyn/ark-savegame-toolkit) v0.8.1 and [ark-tools](https://github.com/Qowyn/ark-tools) v0.6.4 from Java into C#.

### SavegameToolkit

This is a library that actually reads the files of an ARK "Saved" folder. It also can write the content in Json format, can read the Json files and write back to binary files.

However, reading Json is not fully implemented, writing binary is not tested at all and certainly contains some errors that break the savegame files.
I also refactored some parts to better match the C# philosophy, or to be able to write C# code at all, or because I thought it is better structured in a different way.

### SavegameToolkitAdditions

A few parts that were originally part of ArkTools but are usefull for other projects too.

### ArkTools

This is a command line tool to convert the binary files to Json and back, and to create usefull json files like a list of all creatures.
The converting back part and some other not finished/tested parts are deactivated. I also can't guarantee that all of the option switches work as intended (e.g. mmap, parallel).
Because I used a different library for reading the command line the syntax might be different compared to the Java version of ArkTools.

### CompareJson

A small tool to compare the contents of two Json files. Different formatting doesn't matter, and float/double/decimal values are considered the same as long as the difference is very small.
C# serializes these types slightly different than Java.

### MonoOptions

I forgot why I included this lib as source instead of just a NuGet package reference.

### ArkSaveAnalyzer

A GUI tool to inspect the objects of a savegame file (the map file, not the player or tribe files).

A click on a map name will copy the file from ARK's Saved folder to the configured working directory, then will read it. Another click will copy and read it again. 
Enter `saveworld` at the command line in-game to write a file that can be analyzed. The tool is designed for singleplayer and might not work with the structure of the Saved folder in servers.
However the files can be read nontheless, opening manual after selecting a map name.

The tabs do:
* Content: list of all objects. Double click on a line will show its details.
* Wildlife: list of wild creatures with sorting (click the column headers) and basic filtering. You can prepend the level number with < or >. Doubleclick a line for details.
* Map: Shows all tamed creatures and also player created structures on maps or as a list. The structures list shows only one line for each unique pair of coordinates where structures are placed (doubleclick to expand). Some of them might be marked as hidden when the hiding feature of the S+ mod was used.
* Settings: self-explanatory, almost. There is no save button, it's self-saving when changed. 
For "Excluded Wildlife" use case insensitive significant parts of the creature name, or the full name, or a regular expression like `^ant` to exclude Ants (i.e. Titanomyrma) but not Manta or Gigant (i.e. Giganotosaurus).

## Thanks

Thanks to Qowyn for his great work to analyze the binary format of the ARK savegame files and his Java version of ArkTools/SavegameToolkit.
No thanks for the invention of the Java programming language. 90% of the translation was adjusting minor language differences, 9% was finding the equivalent C# expression, 1% was figuring out how it can be done in C#. The time effort seemed to be the opposite.
