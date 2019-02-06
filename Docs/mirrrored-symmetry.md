# Symmetric Mirrored Control Surfaces

by LisiasT, 2019-0206
- - - - 

Any part willing to use [ModuleControlSurface](https://kerbalspaceprogram.com/api/class_module_control_surface.html) needs to be aware of the following features:

* `PartModule`
	+ `OnEditorAttach` event handler
	+ `symMethod` field
	+ `symmetryCounterparts` field
* `ModuleControlSurface` fields:
	+  `usesMirrorDeploy`
	+  `mirrorDeploy`
	+  `partDeployInvert`

Your part module must hook a EventHandler to the OnEditorAttach:

```
	part.OnEditorAttach += new Callback(UpdateOnEditorAttach);

```

So, everytime the user attach something with your module on another part on the Editor, you will be notified.

On the event handler, you need to check the Symmetry Method - on this case, we need to check for the `Mirror` one. And then set some ModuleControlSurface flags, as follows:

1. Check if the part has any Counter Parts on the Symmetry - no need to waste time when the part has no mirrored part! (technically, a but but whatever - better safe than sorry).
	+ To be really pick, one should check if the Count is **exactly** one, but if you get a situation where a mirrored par has more than one, the user have a lot of worse problems to cope wth. :)
2. Get a pointer to the `ModuleControlSurface` instance of the part.
3. Get a pointer to the mirror counterpart of this part.
4. set `usesMirrorDeploy` to TRUE
5. set `mirrorDeploy` differently for each part using some criteria (any one fits, the important part is that one gets TRUE and the other gets FALSE)
6. For a reason beyond my grasp, on B9 Procedural Control Surfaces you **need** to set `partDeployInvert` to the negation of the `mirrorDeploy`. Stock Control Surfaces doesn't use that.

And that's it.

Here is the code I used on B9's Procedural Wings (released under the [MIT](https://opensource.org/licenses/MIT), by the way):

```c#
	public void Start()
	{
		part.OnEditorAttach += SetupMirroredCntrlSrf;
	}
	
	private void SetupMirroredCntrlSrf()
	{
		if (part.symMethod == SymmetryMethod.Mirror && part.symmetryCounterparts.Count > 0)
			if (this.part.Modules.Contains<ModuleControlSurface>())
			{ 
				ModuleControlSurface m = this.part.Modules.GetModule<ModuleControlSurface>();
				m.usesMirrorDeploy = true;
				{
				    Part other = part.symmetryCounterparts[0];
				    m.mirrorDeploy = this.part.transform.position.x > other.transform.position.x;
				    m.partDeployInvert = !m.mirrorDeploy;
				}
	}

```

And that's it! **Any part** willing to implement mirrored behaviours (as elevons and flaps) and uses `ModuleControlSurface` only need to do this, and that's it!
