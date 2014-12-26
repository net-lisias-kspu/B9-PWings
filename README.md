# B9 Aerospace / Procedural parts #

Proof of concept implementation of procedural wings and control surfaces.
Employs different approach than Procedural Dynamics by DYJ (direct mesh manipulation instead of skinned mesh renderers) that is more verbose and inconvenient to set up, but allows proper UV mapping of arbitrary geometry and some other neat features.


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