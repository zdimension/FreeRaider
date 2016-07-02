FreeRaider — an open-source Tomb Raider 1-5 engine remake
-------------------------------------------------------

### Table of contents ###

1. What is this?
2. Why is it developed?
3. Features
4. System requirements
5. Supported platforms
6. Configuration and autoexec files
7. Installation and running
8. Compiling
9. Licensing
10. Credits


1. What is this?
----------------

FreeRaider is a port of OpenTomb to C#.
FreeRaider is an open-source engine reimplementation project intended to play levels from all
classic-era Tomb Raider games (TR 1—5) including TRLE levels. The project does not use any of
the original Tomb Raider source code, because all attempts to retrieve sources from Eidos / Core
were in vain.

Instead, everything is being developed completely from scratch.
However, FreeRaider uses certain legacy routines from unfinished open-source projects such as
OpenRaider and VT project (found at icculus.org), plus it incorporates some code from
Quake Tenebrae.

All in all, FreeRaider is an attempt to recreate the original Tomb Raider experience, along with
contemporary updates, features and additions — to fully benefit from running on modern
PCs with powerful CPUs and graphic cards — unlike the original engines, which are getting older.
The original engine, on which all classics were based on will turn 20 next year!

2. Why it's developed?
----------------------

Many may ask — "Why develop another TR engine clone, while we have fully working Windows
builds of TR2-5, and TR1 is perfectly working through DosBox?". The answer is simple - the
older the engine gets, the lower the chance it'll be compatible with future systems; but in case of
FreeRaider, you can port it to any platform you wish due to usage of many cross-platform libraries.

Other people may ask — "Why we're developing it". If there are already patchers for existing
engines, like TREP, TRNG, etc.? The answer is simple — no matter how advanced your patcher
is, you are limited by the original binary meaning: no new features, no graphical enhancements and
no new structures or functions. You are not that limited with open-source engine.

3. Features
-----------

* FreeRaider has a completely different collision approach. The Engine uses a special terrain
  generation algorithm to convert every room's optimized collisional mesh from so-called "floordata",
  which was a significant limiting factor in the original engine.  
* FreeRaider does not run at fixed 30 FPS, as any old engine did. Instead, variable FPS
  rate is implemented, just like in any contemporary PC game.  
* FreeRaider uses common and flexible libraries such as OpenGL, OpenAL, SDL and Bullet Physics.  
* Lua scripting is a key feature in FreeRaider, all entity functionality is not hardcoded like 
  it was in the original engines. Lua scripts are game script files which can be
  modified and extended any time providing the ability to manipulate several FreeRaider level factors.
* Many abandoned and unused features from originals were enabled in FreeRaider. New animations,
  unused items, hidden PSX-specific structures inside level files, and so on! Also, original
  functionality is being drastically extended, while preserving original gameplay pipeline.

4. System requirements
----------------------

FreeRaider should run fine on any contemporary computer, but you **absolutely** need OpenGL 2.1
compliant videocard (with support of VBOs). Also, make sure you have latest drivers installed
for your videocard, as FreeRaider may use some other advanced OpenGL features.

5. Supported platforms
----------------------

FreeRaider is a cross-platform engine — currently, you can run it on Windows, Mac or Linux.
No mobile ports have been made yet, but they are fully possible.

6. Configuration and autoexec files
-----------------------------------

Currently, all settings in FreeRaider are managed through configuration and autoexec files.
Configuration file contains persistent engine and game settings, while autoexec contains
any commands which should be executed on engine start-up.

Configuration file (**config.lua**) is divided into different sections: screen, audio, render,
controls, console and system. In each of these sections, you can change numerous parameters,
which names are usually intuitive to understand.  
Autoexec file (**autoexec.lua**) is a simple command file which is executed at engine start-up,
just like you type them in the console. Basically, you shouldn't remove any existing commands
from autoexec, as most likely engine won't start properly then, but you can modify these
commands or add new ones — like changing start-up level by modifying setgamef() command.

7. Installation and running
---------------------------

You don't need to install FreeRaider, but you need the classic TR game resources for the specifc
games you'd like to play within FreeRaider. 
Problem is, these resources (except level files) tend to be in some cryptic formats or
are incompatible across game versions. Because of this, you need to convert some game resources
by yourself or get them from somewhere on the Net. Anyway, here is the list of all needed
assets and where to get them:

 * Data folders from each game. Get them from your retail game CDs or Steam/GOG bundles.
   Just take data folder from each game's folder, and put it into the corresponding
   /data/tr*/ folder. 
  
   An example level path is: "root/data/tr1/data/level1.phd".
   Where "root" is the folder containing FreeRaider.exe. GOG versions may have these files
   in a separate file called GAME.GOG. This can be simply renamed to GAME.ISO then mounted
   as a standard ISO file revealing the "/DATA/" folders.
   
 * CD audio tracks. FreeRaider supports OGG audiotracks (for TR1/TR2), CDAUDIO.WAD file (for TR3),
   PCM and MS-ADPCM wave files (for TRLE and TR4/5 respectively). For TR1/TR2, you can
   convert the original soundtracks by yourself or just download the whole TR1-2 music 
   package here: https://www.dropbox.com/s/fm3qpdhnbzntkml/tr1-2_soundtracks_for_opentomb.zip?dl=0
   
 * Loading screens for TR1-3 and TR5. For TR3, get them from pix directory of your officially
   installed game. Just copy or move the pix directory into /data/tr3/ within FreeRaider's folder.
   As for other games, it's a bit tricky to get loading screens. There were no loading screens for
   PC versions of TR1-2, and TR4 used level screenshots as loading screens. TR5 used an encrypted
   format to store all loading screen files. So, to ease your life, you can simply download the
   loading screen package here: https://www.dropbox.com/s/uycdw9x294ipc0r/loading_screens.zip?dl=0
   Just extract these files directly into the main FreeRaider directory, and that should do the trick.

 * If you are looking for the soundtracks and loading screens of only a single Tomb Raider game, you
   can also download them at [opentomb.earvillage.net](http://opentomb.earvillage.net/).
    
8. Compiling
------------

FreeRaider uses the following libraries:

* OpenTK 1.1 (https://github.com/opentk/opentk)
* BulletSharp 2.87.3.0 (https://github.com/AndresTraks/BulletSharp)
* NLua with KeraLua 1.3.2.0 (https://github.com/NLua/NLua)
* SharpFont 3.1.0 (https://github.com/Robmaister/SharpFont)
* Zlib.Portable 1.11.0.0 (https://github.com/advancedrei/Zlib.Portable)
* NLibsndfile 1.0.0.0 (modified) (https://github.com/ahawker/NLibsndfile)
* SDL2-CS (modified) (https://github.com/flibitijibibo/SDL2-CS)

All these libraries are included in the project file.
To compile FreeRaider, you need Visual Studio 2015 and the .NET Framework 4.0.

9. Licensing
------------

FreeRaider is an open-source engine distributed under LGPLv3 license, which means that ANY part of
the source code must be open-source as well. Hence, all used libraries and bundled resources must
be open-source with GPL-compatible licenses. Here is the list of used libraries, resources and
their licenses:

* OpenTK — The Open Toolkit library license
* BulletSharp — MIT
* NLua — MIT
* SharpFont — MIT
* Zlib.Portable — Apache 2.0
* NLibsndfile — None
* SDL2-CS — zlib
    
10. Credits
----------

### FreeRaider developers

* zdimension: main developer

### OpenTomb developers

NB: Please note that the authors and contributors list is constantly extending! There are more and
more developers getting involved in the development of FreeRaider so some recent ones may be missing
from this list!

* TeslaRus: main developer.
* ablepharus: compilation fix-ups and miscellaneous patches.
* Cochrane: renderer rewrites and optimizing, Mac OS X support.
* Gh0stBlade: renderer add-ons, shader port, gameflow implementation, state fix-ups, camera.
* Lwmte: state fix-ups, controls, GUI and audio modules, trigger and entity scripts.
* Nickotte: interface programming, ring inventory implementation, camera fix-ups.
* pmatulka: Linux port and testing.
* richard_ba: Github migration, Github repo maintenance, website design.
* Saracen: room and static mesh lighting.
* stltomb: general code maintenance, enhancements and bugfixes.
* stohrendorf: CXX-fication, general code refactoring and optimizing.
* T4Larson: general stability patches and bugfixing.
* vobject: nightly builds, maintaining general compiler compatibility.
* vvs-: extensive testing and bug reporting.

Additional contributions from: Ado Croft (extensive testing), E. Popov (TRN caustics shader port),
godmodder (general help), jack9267 (vt loader optimization), meta2tr (testing and bugtracking),
shabtronic (renderer fix-ups), Tonttu (console patch) and xythobuz (additional Mac patches).

Translations by: Joey79100 and zdimension (French), Nickotte (Italian), Lwmte (Russian), SuiKaze Raider (Spanish).