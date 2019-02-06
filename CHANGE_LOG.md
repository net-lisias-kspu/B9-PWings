# B9 Procedural Wings (/L Unofficial) :: Change Log

* 2019-0206: 0.80.1.2 (Lisias) for KSP >= 1.4.1
	+ Ensured compatibility to every KSP version from 1.4.1
	+ Added KSPe as Hard Dependency
		- Added Logging facilities
		- Added File facilities
	+ Some small code optimizations
	+ Fixed NFE on the KSP's asynchronous events
	+ Implemented Mirrored Deploy for Control Surfaces. #HURRAY
		- [Atmospheric AutoPilot](https://github.com/net-lisias-kspu/AtmosphereAutopilot) is supported! :) 
		- Support for FAR is preserved.
	+ Some mistakes on the documentation where fixed
* 2019-0205: 0.80.1.1 (Lisias) for KSP >= 1.4.1
	+ **DITCHED **
* 2019-0204: 0.80.1 (Lisias) for KSP >= 1.4.1
	+ **DITCHED **
* 2018-1216: 0.71 (Rafterman82) for KSP 1.5.1
	+ Fixed and issue where textures were incorrectly scaled.
* 2018-1213: 0.70 (Rafterman82) for KSP 1.5.1
	+ Release Notes:
	+ Compiled against 1.5.1 DLL files
* 2018-0522: 0.60 (Rafterman82) for KSP 1.4.3
	+ Release Notes:
			- Compiled against 1.4.3 DLL files
			- Increase root width of main wing / all moving control surface from 16 to 20 units
			- Increase tip width of main wing / all moving control surface from 16 to 20 units
			- Increase length of main wing / all moving control surface from 16 to 20 units
			- Increase length of main wing / all moving control surface from 8 to 10 units
			- In length of control surface from 8 to 10 units
			- Added tiny increment as float for control surface offset root and tip increment
			- Reduced limits of control surface root and tip offset from -6 units/6 units to -0.5 units/0.5 units
			- Changed control surface offset root/width calls to use tiny float (0.0.5f)
	+ Known Issues:
			- When using stock aero control surfaces set as spoilers/flaps will move in opposite directions. Interim Fix: Disable symmetry in the editor, place the control surface, press ALT + Mouse 1 on the part, duplicate, then roll/flip as needed with the QWEASD keys and place as close as you can to the opposite side of your craft. Or even better use FAR and enjoy full flap support
* 2015-1111: 2.1 (Crzyrndm) for KSP 1.0.5
	+ No changelog provided
* 2015-1003: 2.0.0 (Crzyrndm) for KSP 1.3
	+ No changelog provided
* 2017-0614: 0.40.13 (Crzyrndm) for KSP 1.3
	+ Add support for Configurable Containers Fuel tanks (module support from Configurable Containers) - by Allista
* 2017-0528: 0.40.12.0 (Crzyrndm) for KSP 1.3
	+ Rebuild for KSP 1.3
* 2017-0225: 0.40.11 (Crzyrndm) for KSP 1.2
	+ Stock aero calculations by Boris-Barboris
* 2016-1031: 0.40.10 (Crzyrndm) for KSP 1.2
	+ Fix part compilation error
* 2016-1023: 0.40.9 (Crzyrndm) for KSP 1.2
	+ Fix bad mirroring behaviour of control surfaces
* 2016-1012: 0.40.8 (Crzyrndm) for KSP 1.2
	+ KSP 1.2 compatible version
* 2016-0502: 0.40.7 (Crzyrndm) for KSP 1.1 Patch 4
	+ Fix fuel capacity not updating correctly
* 2016-0501: 0.40.6 (Crzyrndm) for KSP 1.1 Patch 3
	+ No changelog provided
* 2016-0427: 0.40.5 (Crzyrndm) for KSP 1.1 Patch 2
	+ Fix MFT/RF integration issues
	+ Skip the MM patch in hope of having less reports of inverted control surfaces
* 2016-0422: 0.40.4 (Crzyrndm) for KSP 1.1 Patch 1
	+ Fix fuel switch creating NaN fuel quantities
* 2016-0421: 0.40.3 (Crzyrndm) for KSP 1.1
	+ No changelog provided
* 2018-0407: 0.50 (jrodrigv) for KSP 1.4
	+ Compatible with KSP 1.4.X
* 2015-0609: 0.40 (bac9) for KSP 1.0.2
	+ Includes all changes from the maintenance fork by Crzyrndm (lots of bugfixes and improvements to the code)
	+ Full compatibility with KSP 1.0.2
	+ Full compatibility with FAR 0.15.3+
	+ Aerodynamics calculations no longer restart on every slider change, editor performance is improved
	+ New stock-alike texture (old texture is available optionally)
	+ Textures converted to .dds
	+ Improved shader and material settings
* 2015-0208: 0.34 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0207: 0.33 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0203: 0.32 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0130: 0.31 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0127: 0.30 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0124: 0.29 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0124: 0.28 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0124: 0.27 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0124: 0.26 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0124: 0.24 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0123: 0.23 (bac9) for KSP 0.90
	+ RealFuels support added, wings switch from inbuilt fuel switching code to RealFuel tweakables automatically upon detecting it installed
	+ Fixed the alternative UI getting stuck in the timeout mode upon after you exit and reenter the editor scene
	+ Added a button to the KSP editor appbar allowing you to close the alternative UI window and reopen it without mouseover+J (the button appears and disappears automatically whenever a procedural part is present in a scene)
	+ Added a configuration menu to the KSC scene, accessible through the appbar button. It allows you to enable one of 12 logging modes (all disabled by default) and might come in handy later when I might ask some of you to reproduce an issue with a certain logging mode enabled, sending me the resulting log
	+ Fixed fuel per volume multiplier - previously you were able to use the total internal volume of a wing for fuel, which is fixed now, with only reasonably realistic 70% of it available
	+ Various minor fixes and optimizations
* 2015-0122: 0.22 (bac9) for KSP 0.90
	+ Control surface edge width limits no longer reset to 0-0 instead of correct 0.24-1 (rogue Vector2 was used instead of Vector4 at a certain point, losing those limits due to absence of .z and .w values)
* 2015-0122: 0.21 (bac9) for KSP 0.90
	+ Implemented fuel switching based on code by Andreas Aakvik Gogstad (Snjo): You can use the button in both tweakable and alternative menu to scroll through four possible internal configurations of a wing: empty structure (STR), liquid fuel (LF), liquid fuel with oxidized (LFO) and monopropellant (RCS). As you alter the shape of a wing and change it's internal volume, amount of fuel within the wing is altered accordingly.
	+ Fixed some style and formatting issues of the alternative menu
	+ Fixed incorrect trailing edge width limits on control surfaces
	+ Added opt-in update notifications supported by MiniAVC
* 2015-0122: 0.20 (bac9) for KSP 0.90
	+ The alternative UI height no longer shifts height during slider editing, tooltip label height is now locked, tooltip text adjusted to always take two lines
	+ The description of the last edited property is always correct now, jumps to improper descriptions eliminated
	+ Editing mode button changed to J to prevent conflict with Editor Extensions hotkeys
	+ It is now possible to force the part you are editing to match the shape (parent tip width becomes child root width, parent tip thickness becomes child root thickness and so on)
	+ It is now possible to force the part you are editing to match the surface material settings of a parent
	+ Control surface offset settings can now assume a wider range of values, allowing proper setup of control surfaces on high-sweep trailing edges
* 2015-0121: 0.19 (bac9) for KSP 0.90
	+ Leading and trailing edge width is no longer taken into account when type 1 (flat) edge is selected
	+ Control surface main body width lower limit is now set to 0.125 (I'll look into making it lower in the future, but at the moment I have to snap it to the increment and it can't go to 0 without breaking geometry)
* 2015-0120: 0.18 (bac9) for KSP 0.90
	+ Fixed geometry counterparts not updating properties
	+ Minor UI fixes
* 2015-0120: 0.17 (bac9) for KSP 0.90
	+ Implemented coloration - every wing surface (top/bottom/leading/trailing) can now have not just previously implemented separate material type, but also an appropriately masked paint overlay with configurable opacity, hue, saturation and brightness
	+ Reimplemented alternative UI, it now closely resembles tweakable menu with a number of improvements over stock (color coded groups, longer labels, hints, collapsable sections)
	+ Moved all sliders in the stock tweakable menu to KSP API Extensions UI_FloatEdit editor, which seems to positively affect the rate of a memory leak on tweakable window redraw (sometimes it even stops entirely).
	+ Unified all properties, control surfaces and wings no longer use isolated fields (breaking change, all previously built crafts will not retain wing part configurations)
	+ Added an option to set current configuration of a wing or control surface as default, making all subsequently created parts of that type use the same values (along with an option to reset default values back)
	+ Shifted all properties into logical groups
	+ Implemented collapsable menus to make editing more convenient
	+ Geometry and performance fixes, refactoring and minor fixes in other areas
	+ Improved some layer textures, surface type 0 in particular (rivets, more appropriate seam structure and so on)
* 2015-0116: 0.16 (bac9) for KSP 0.90
	+ Wings support thickness of up to one meter from now on
	+ Removed leading and trailing edge multiplier property from wings
	+ Added leading edge root/tip width and trailing edge root/tip width properties, all four of them can be adjusted from zero to one meter, allowing you to set up vertically elongated edge cross sections, horizontally elongated edge cross sections and cross sections proportional to arbitrary wing thickness values alike.
	+ Added collapsable categories parenting logical property groups (dimensions, materials, edges and so on) to the tweakable menu, making the selection and adjustment of numerous properties more manageable
	+ All tweakable properties are no longer initially open in the context menu, being hidden under categories
	+ Replaced all edge models with new ones, improving cross section polycounts and adding hard material edge that looks better under arbitrary thickness and edge width combinations resulting in stretching
	+ Reordered edge models: first type is circular, second type is biconvex (between sharp and circular in shape), third one is triangular, which makes a bit more sense (progression from smooth to sharp shapes instead of random order)
	+ Reordered and renamed some properties for convenience
	+ Added flat fill support (fifth material type) to edges
	+ Improved aerodynamic value calculation for wings, edges now properly contribute to the behaviour
	+ Improved aerodynamic value calculation for control surfaces, trailing edge widths are now taken into account, along with contributions of offset properties to taper ratio
	+ Internal fixes and refactoring in geometry handling, setup sequence and field updates
Top and bottom surfaces of control surfaces and wings are collapsed into one mesh with a slight benefit to performance (separate material selection still available, shader based switching makes use of one mesh a non-issue)
Slight improvements to text in alternative UI
* 2015-0114: 0.15 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0113: 0.14 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0108: 0.13 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0107: 0.12 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0106: 0.11 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0103: 0.10 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0101: 0.9 (bac9) for KSP 0.90
	+ No changelog provided
* 2015-0101: 0.8 (bac9) for KSP 0.90
	+ No changelog provided
* 2014-1231: 0.7 (bac9) for KSP 0.90
	+ No changelog provided
* 2014-1230: 0.6 (bac9) for KSP 0.90
	+ No changelog provided
* 2014-1230: 0.5 (bac9) for KSP 0.90
	+ No changelog provided
* 2014-1228: 0.4 (bac9) for KSP 0.90
	+ No changelog provided
* 2014-1227: 0.3 (bac9) for KSP 0.90
	+ No changelog provided
* 2014-1227: 0.2 (bac9) for KSP 0.90
	+ No changelog provided
* 2014-1227: 0.1 (bac9) for KSP 0.90
	+ No changelog provided
