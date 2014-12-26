# B9 Aerospace / Procedural parts #

There are things about [URL="http://forum.kerbalspaceprogram.com/threads/29862-0-90-Procedural-Dynamics-Procedural-Wing-0-9-2-Dec-21"]existing procedural wings[/URL], like texture stretching stretching and lack of some options, that have long bothered me a bit. Few days ago I had an idea about solving them, and here is the proof of concept implementation of procedural wings and control surfaces. It employs different approach than Procedural Dynamics by DYJ (direct mesh manipulation instead of skinned mesh renderers) that is more verbose and inconvenient to set up, but allows proper UV mapping of arbitrary geometry and some other neat features.

Mind that this is a very early and potentially unstable version - I'm putting it out there mostly for testing and feedback and I accept no responsibility for broken craft files and PCs on fire and other stuff like that (not that it occurred to me, but saying just in case).


### Features ###

You have control over the following options:

* Wing semispan selection
* Wing width selection (at root and tip cross section)
* Forward axis offset selection (the distance between root and tip cross section midpoints)
* Wing thickness selection (at root and tip cross section)
* Wing surface type selection (from light material to heavy STS-like shielding)
* Edge geometry type and scaling selection (for leading and trailing edges)
* Convenience option for syncing root/tip cross section parameters
* Convenience option for syncing leading/trailing edge parameters

Control surfaces have limited subset of those options available at the moment as some aren't applicable (like offset), some aren't yet implemented (like surface type selection).
Additional control surface-exclusive options might be added later too.


### Notes ###

Some important imformation on the current state of the mod:

* So far it has only been tested with FAR, whether config values for the stock flight model are calculated correctly, I have no idea
* Well, actually, I have no idea whether FAR config values are right either - I think they are, but beyound "huh, it flies" I'm not an authority on that
* Mirroring is not working on procedural control surfaces for some weird reason (investigating that), so while asymmetric control surfaces are supported, they are not usable for anything but centered tails yet
* Wings with inbuilt control surfaces won't be supported, same thing can be built out of a wing segment with a slightly smaller root/tip width compared to neighbours, Flat trailing edge type on that wing, and a control surface attached to it
* Control surfaces cause FAR to drop NREs in some particular situations, I'm still investigating the reason
* Symmetry for control surfaces is not entirely stable, except attachment not to work sometimes


### Plans ###

In no particular order:

* Fixing symmetry and attachment issues with control surfaces
* Fixing possibly bugged aerodynamic values calculation
* Better tweakable UI and/or alternative input options
* Adding more options for control surfaces
* Adding better control over wing edges

I'm not sure whether it's best to integrate this into B9 or leave it as a separate mod. For the meantime it will be the latter as there are no shared dependencies.


### Credit ###

I referenced how attachment/detachment is handled in pWings by DYJ and I have used the same aerodynamic stats calculation methods (if I'm not mistaken, those were set up by Taverius).
Wing geometry itself is not handled in the same way (I'm not using skinned mesh renderers or same parameters) and no part content is shared with Procedural Dynamics, so this is not exactly a proper derivative work, I guess.

Still, credit where it's due, I'm sure I would've stepped into far, far more horrifying bugs and design mistakes if I had not studied the source. Thanks to [B]DYJ[/B], [B]NathanKell[/B] and [B]Taverius[/B] for working on it.
Also, thanks to [B]ferram4[/B] for advice on FAR support and to helpful folks from #kspmodders IRC channel who answered some of my horrifyingly incompetent questions about C#.