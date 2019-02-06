# B9 Procedural Wings /L Unofficial

The procedural dynamics procedural wing (pWing for short) is a wing piece that the user can procedurally manipulate, the wing automatically generates colliders and set its .cfg parameters accordingly.

Unofficial fork by Lisias.


## In a Hurry

* [Latest Release](https://github.com/net-lisias-kspu/B9-PWings/releases)
	+ [Binaries](https://github.com/net-lisias-kspu/B9-PWings/tree/Archive)
* [Source](https://github.com/net-lisias-kspu/B9-PWings)
* Documentation
	+ [Project's README](https://github.com/net-lisias-kspu/B9-PWings/blob/master/README.md)
	+ [Install Instructions](https://github.com/net-lisias-kspu/B9-PWings/blob/master/INSTALL.md)
	+ [Change Log](./CHANGE_LOG.md)
	+ [TODO](./TODO.md) list


## Description

There are things about existing procedural wings, like texture stretching and lack of some options, that have long bothered me a bit. Few days ago I had an idea about solving them, and here is the proof of concept implementation of procedural wings and control surfaces. It employs different approach than Procedural Dynamics by DYJ (direct mesh manipulation instead of skinned mesh renderers) that is more verbose and inconvenient to set up, but allows proper UV mapping of arbitrary geometry and some other neat features.

### Features

You have control over the following options:

* Wing semispan selection
* Wing width selection (at root and tip cross section)
* Forward axis offset selection (the distance between root and tip cross section midpoints)
* Wing thickness selection (at root and tip cross section)
* Wing surface type selection (from light material to heavy STS-like shielding)
* Edge geometry type and scaling selection (for leading and trailing edges)
* Convenience option for syncing root/tip cross section parameters
* Convenience option for syncing leading/trailing edge parameters

Control surfaces have limited subset of those options available at the moment as some aren't applicable (like offset), some aren't yet implemented (like surface type selection). Additional control surface-exclusive options might be added later too.

### Notes

Some important information on the current state of the mod:

* So far it has only been tested with FAR, whether config values for the stock flight model are calculated correctly, I have no idea
* Wings with inbuilt control surfaces won't be supported, same thing can be built out of a wing segment with a slightly smaller root/tip width compared to neighbours, Flat trailing edge type on that wing, and a control surface attached to it
* Update FAR to the latest development version, otherwise parts can cause NREs on attachment and break their parent parts, making them permanently unavailable until you reload a craft


## Installation

Detailed installation instructions are now on its own file (see the [In a Hurry](#in-a-hurry) section) and on the distribution fi

### License:

Released under MIT license. See [here](./LICENSE).

Please note the copyrights and trademarks in [NOTICE](./NOTICE).


## Acknowledgements

### from [bac9](https://forum.kerbalspaceprogram.com/index.php?/profile/57757-bac9/)

I referenced how attachment/detachment is handled in [pWings](https://forum.kerbalspaceprogram.com/index.php?/topic/27608-090-procedural-dynamics-procedural-wing-093-dec-24/&) by [DYJ](https://forum.kerbalspaceprogram.com/index.php?/profile/8636-dyj/) and I have used the same aerodynamic stats calculation methods (if I'm not mistaken, those were set up by [Taverius](https://forum.kerbalspaceprogram.com/index.php?/profile/11815-taverius/) and [ferram4](https://forum.kerbalspaceprogram.com/index.php?/profile/21328-ferram4/)).

Wing geometry itself is not handled in the same way (I'm not using skinned mesh renderers or same parameters) and no part content is shared with Procedural Dynamics, so this is not exactly a proper derivative work, I guess.

Credit where it's due, I'm sure I would've stepped into ten times more mistakes if I had not studied the source. Thanks to DYJ, [NathanKell](https://forum.kerbalspaceprogram.com/index.php?/profile/75006-nathankell/) and Taverius for their work.

Also, thanks to ferram4 for advice on FAR support and to helpful folks from #kspmodders IRC channel who answered some of my horrifyingly incompetent questions about C#.

Special thanks to [xEvilReeperx](https://forum.kerbalspaceprogram.com/index.php?/profile/75857-xevilreeperx/) for helping me out with figuring custom shader loading.

Fuel switching is based on code from [Snjo](https://forum.kerbalspaceprogram.com/index.php?/profile/57198-snjo/).

Maintenance in the few last months *(note from L: the text is from about Dec/2014)*, changes required for KSP 1.0.x and FAR 0.15.x compatibility and enormous number of fixes and improvements to the code were performed by [Crzyrndm](https://forum.kerbalspaceprogram.com/index.php?/profile/92871-crzyrndm/).

### from LisiasT

I'm happy that B9 Parts Switch was kept up to date by the current Maintainer, however I'm also disappointed that something easy to fix but yet important was dismissed so easily, as in the current [OP on forum](https://forum.kerbalspaceprogram.com/index.php?/topic/175197-13114x151-b9-procedural-wings-fork-go-big-or-go-home-update-20-larger-wings/):

> When using stock aero control surfaces set as spoilers/flaps will move in opposite directions. 
> 
> Interim Fix: Disable symmetry in the editor, place the control surface, press ALT + Mouse 1 on the part, duplicate, then roll/flip as needed with the QWEASD keys and place as close as you can to the opposite side of your craft.
> 
> Or even better use FAR and enjoy full flap support.

I fixed my plane (by editing the craft!) in 15 minutes, and spent less than an hour researching the matter. I fail to understand why this was lingering for so much time without a proper fix. No one is helping this guy? 

More fixes are to come, and pull requests will be issued to the upstream. In the mean time, rescuing this Add'On history was a pleasure by itself, and I intent to pull it up to upstream too.

Thanks, [bac9](https://forum.kerbalspaceprogram.com/index.php?/profile/57757-bac9/). You work is appreciated even after so many years.


## UPSTREAM

* [Jebman82](https://forum.kerbalspaceprogram.com/index.php?/profile/76431-jebman82/) CURRENT MAINTAINER
	+ [Forum](https://forum.kerbalspaceprogram.com/index.php?/topic/175197-13114x151-b9-procedural-wings-fork-go-big-or-go-home-update-20-larger-wings/&)
	+ [GitHub](https://github.com/Rafterman82/B9-PWings-Fork)
	+ [GitHub](https://github.com/Rafterman82/B9-Pwings-Fork-Backport) (backport to 1.3.1)
* [Crzyrndm﻿](https://forum.kerbalspaceprogram.com/index.php?/profile/92871-crzyrndm/) 
	+ [Forum](https://forum.kerbalspaceprogram.com/index.php?/topic/117236-13-procedural-wings/)
	+ [GitHub](https://github.com/Crzyrndm/B9-PWings-Fork)
	+ [BitBucket](https://bitbucket.org/Crzyrndm/b9_aerospace_plugins/overview) (Historical)
* [jrodrigv](https://github.com/jrodrigv)
	+ [GitHub](https://github.com/jrodrigv/B9-PWings-Fork)
* [bac9](https://forum.kerbalspaceprogram.com/index.php?/profile/57757-bac9/)
	+ [Forum](https://forum.kerbalspaceprogram.com/index.php?/topic/94621-102-b9-aerospace-procedural-parts-040-updated-090615/)
	+ [BitBucket](https://bitbucket.org/bac9/b9_aerospace_plugins/overview)
* Honorable Mentions:
	+ [DYJ](https://forum.kerbalspaceprogram.com/index.php?/profile/8636-dyj/)'s [Procedural Dynamics](https://forum.kerbalspaceprogram.com/index.php?/topic/27608-090-procedural-dynamics-procedural-wing-093-dec-24/&)	 
