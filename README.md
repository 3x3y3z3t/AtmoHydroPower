This is the repository for "the script part" for Roshag & DEeM0NX's mod [Atmo Thrusters with Hydrogen (Standalone)](https://steamcommunity.com/sharedfiles/filedetails/?id=2807922557).
This branch (`advanced usage`) is for advanced usage where you need to set up your own blocks' SubtypeIds, and failsafe has been removed.

# AtmoHydroPower - Advanced Usage branch
![Thumbnail](thumb.png)

# Installation
## If using as-is
1. Clone the whole repository into your local Mods folder.
2. Navigate to `Data\Scripts\AtmoHydroPower` and open file `AtmoHydroPower_GameLogic_Setup.cs`
3. Put your thruster blocks' SubtypeIDs in MyEntityComponentDescriptor attribute above GameLogic class. Any block with SubtypeID present here will be attached.
4. You are done now. You should load your thruster mod before this mod.  

## If using embeded
Embeding is a bit more complicated. You first need to know which files do what before deciding which to keeps and which to remove.
- `AtmoHydroPower_GameLogic.cs`: This is the main logic file. **You need this file.**
- `AtmoHydroPower_GameLogic_Setup.cs`: This is the setup file (for GameLogic). **You need this file.**
- `AtmoHydroPower_Session.cs`: This file is for Session Comp, responsible for initializing Logger and User Config.
- `Config.cs`: This file is for User Config, Constant values and ultilities. You need this file, **or** you can merge things in this file into your Configs/Constants/Utils files.
- `Logger.cs`: This file is for Logging. You need this file, **or** you can use your own logging system. *Note* that if you use your own logging system, you need to point this Logger class to yours, or replace every Logger calls in GameLogic class with yours.
- `\\ExShared\\ConfigCore.cs`: This file is for Config core. If you use your own Config, you don't need this.
- `\\ExShared\\LoggerCore.cs`: This file is for Logger core. If you use your own Logger, you don't need this.

Keep the GameLogic files, merge what you need and remove the rest to avoid conflict (classes with duplicated name).

# Notes
- What is balancing?

# Version
Script version v1.4.1
