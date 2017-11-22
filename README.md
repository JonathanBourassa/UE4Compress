# UE4Compress
Small Console Utility to Compress Cooked Mod Prior to Steam Workshop Upload, to be used in Continious integration tools.

Cooking Ark Dev Kit Mods in Continious Integration mode using a solution Like Teamcity or Jenkins requires that you do stuff through command line or scripts. The Ark Dev kit does support that with the use of UE4Editor-Cmd.exe

Here's an example of the cook command assuming you have the devkit installed in C:\ArkDevKit :

```
C:\ArkDevKit\Engine\Binaries\Win64\UE4Editor-Cmd.exe "ShooterGame/ShooterGame.uproject" -run=Cook -Map=ModName -CookCultures=en -TargetPlatform=WindowsNoEditor -ModDir=ModDirectoryName -GameMod -ModGUID=000000AA0000AAA00AA000A0A0AA00AA -pkgnfo="C:/ArkDevKitPackageInfo.bin" -Output="C:/ArkDevKit/ModTools/Output/ModName/WindowsNoEditor_STAGING"
```

This work very well except for one thing. If you ever got to your mod output folder before uploading it to steam. You probably noticed all the File are compressed in a propriatery format using zlib compression each of your asset ends up there in two files.
```
AssetName.uasset.z
AssetName.uasset.z.uncompressed_size
```

the problem you will 1st see if you cook from command line is that you asset get's cooked but uncompressed, like they should be once installed in Ark after download only the UI wizard does it for you as it's not part of the cook but actual Custom preparation for Ark to avoid big download on mod installation.

That's where this project comes in, if you want to go forward with continious integration you can use this tool to do that extra needed step before Using Steamcmd.exe to upload.
