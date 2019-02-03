# B9 Procedural Wings /L Unofficial

The procedural dynamics procedural wing (pWing for short) is a wing piece that the user can procedurally manipulate, the wing automatically generates colliders and set its .cfg parameters accordingly.

Unofficial fork by Lisias.


## Installation Instructions

To install, place the GameData folder inside your Kerbal Space Program folder. Optionally, you can also do the same for the PluginData (be careful to do not overwrite your custom settings):

* **REMOVE ANY OLD VERSIONS OF THE PRODUCT BEFORE INSTALLING**, including any other fork:
	+ Delete `<KSP_ROOT>/GameData/B9_Aerospace_ProceduralWings`
* Extract the package's `GameData` folder into your KSP's root:
	+ `<PACKAGE>/GameData` --> `<KSP_ROOT>/GameData`
* Extract the package's `PluginData` folder (if available) into your KSP's root, taking precautions to do not overwrite your custom settings if this is not what you want to.
	+ `<PACKAGE>/PluginData` --> `<KSP_ROOT>/PluginData`
	+ You can safely ignore this step if you already had installed it previously and didn't deleted your custom configurable files.
* **Optionally**, extract `GameData_AlternativeTexture/` contents into `<KSP_ROOT>/GameData` overwriting any content for a alternative set of textures	
	+ Beware, don't copy `GameData_AlternativeTexture` itself into your game's `GameData`, but its contents!

The following file layout must be present after installation:

```
<KSP_ROOT>
	[GameData]
		[B9_Aerospace_ProceduralWings]
			[AssetBundles]
				...
			[Parts]
				...
			[Patches]
				...
			[PluginData]
				icon_stock.png
			CHANGE_LOG.md
			LICENSE
			NOTICE
			B9_Aerospace_ProceduralWings.dll
			...
		000_KSPe.dll
		...
	[PluginData]
		[B9_Aerospace_ProceduralWings] <not present until you run it for the fist time>
			user.cfg 
	KSP.log
	PartDatabase.cfg
	...
```

The following file layout **IS WRONG** and **SHOOUD NOT** be found on your's `GameData`

```
<KSP_ROOT>
	[GameData]
		[B9_Aerospace_ProceduralWings]
			[AssetBundles]
				...
		[GameData_AlternativeTexture] <-- NO!!! 
			...
```

### Dependencies

* [KSP API Extensions/L](https://github.com/net-lisias-ksp/KSPAPIExtensions) 2.1 or later
	+ Hard Dependency - Plugin will not work without it.
	+ Not Included

