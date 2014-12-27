using KSP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace WingProcedural
{
    public class WingProcedural : PartModule
    {
        // Neater way to cache mesh properties

        [System.Serializable]
        public class MeshReference
        {
            public Vector3[] vp;
            public Vector3[] nm;
            public Vector2[] uv;
        }




        // Prerequisites / Wing


        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Semispan"),
        UI_FloatRange (minValue = 0.5f, maxValue = 16f, scene = UI_Scene.Editor, stepIncrement = 0.5f)]
        public float wingSpan = 4f;
        public float wingSpanCached = 4f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Width / root"),
        UI_FloatRange (minValue = 0.5f, maxValue = 16f, scene = UI_Scene.Editor, stepIncrement = 0.5f)]
        public float wingWidthRoot = 4f;
        public float wingWidthRootCached = 4f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Width / tip"),
        UI_FloatRange (minValue = 0.5f, maxValue = 16f, scene = UI_Scene.Editor, stepIncrement = 0.5f)]
        public float wingWidthTip = 4f;
        public float wingWidthTipCached = 4f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height / root"),
        UI_FloatRange (minValue = 0.08f, maxValue = 0.48f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float wingThicknessRoot = 0.24f;
        public float wingThicknessRootCached = 0.24f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height / tip"),
        UI_FloatRange (minValue = 0.08f, maxValue = 0.48f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float wingThicknessTip = 0.24f;
        public float wingThicknessTipCached = 0.24f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Offset"),
        UI_FloatRange (minValue = -8f, maxValue = 8f, scene = UI_Scene.Editor, stepIncrement = 0.5f)]
        public float wingOffset = 0f;
        public float wingOffsetCached = 0f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Sync width"),
        UI_Toggle (disabledText = "Off", enabledText = "On", scene = UI_Scene.Editor)]
        public bool syncWidth = false;
        public bool syncWidthCached = false;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Sync height"),
        UI_Toggle (disabledText = "Off", enabledText = "On", scene = UI_Scene.Editor)]
        public bool syncThickness = false;
        public bool syncThicknessCached = false;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Sync edges"),
        UI_Toggle (disabledText = "Off", enabledText = "On", scene = UI_Scene.Editor)]
        public bool syncEdge = false;
        public bool syncEdgeCached = false;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Edge T (scale)"),
        UI_FloatRange (minValue = 0f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.25f)]
        public float wingEdgeTrailing = 1f;
        public float wingEdgeTrailingCached = 1f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Edge L (scale)"),
        UI_FloatRange (minValue = 0f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.25f)]
        public float wingEdgeLeading = 1f;
        public float wingEdgeLeadingCached = 1f;

        public MeshFilter meshFilterWingSection;
        public MeshFilter meshFilterWingSurfaceTop;
        public MeshFilter meshFilterWingSurfaceBottom;
        public MeshFilter meshFilterWingEdgeTrailing;
        public MeshFilter meshFilterWingEdgeLeading;
        public MeshFilter meshFilterWingEdgeA;
        public MeshFilter meshFilterWingEdgeB;
        public MeshFilter meshFilterWingEdgeC;

        public MeshReference meshReferenceWingSection;
        public MeshReference meshReferenceWingSurfaceTop;
        public MeshReference meshReferenceWingSurfaceBottom;
        public MeshReference meshReferenceWingEdgeA;
        public MeshReference meshReferenceWingEdgeB;
        public MeshReference meshReferenceWingEdgeC;

        public Vector2 wingSpanLimits = new Vector2 (0.5f, 16f);
        public Vector2 wingWidthLimits = new Vector2 (0.5f, 16f);
        public Vector2 wingThicknessLimits = new Vector2 (0.08f, 0.48f);
        public Vector2 wingOffsetLimits = new Vector2 (-8f, 8f);
        public Vector2 wingEdgeLimits = new Vector2 (0f, 1f);

        [KSPField (isPersistant = true)] public int wingEdgeTypeTrailing = 2;
        [KSPField (isPersistant = true)] public int wingEdgeTypeTrailingCached = 2;
        [KSPField (isPersistant = true)] public int wingEdgeTypeLeading = 1;
        [KSPField (isPersistant = true)] public int wingEdgeTypeLeadingCached = 1;

        public MeshFilter meshFilterWingSurfaceA;
        public MeshFilter meshFilterWingSurfaceB;
        public MeshFilter meshFilterWingSurfaceC;

        [KSPField (isPersistant = true)] public int wingSurfaceTextureTop = 1;
        [KSPField (isPersistant = true)] public int wingSurfaceTextureTopCached = 1;
        [KSPField (isPersistant = true)] public int wingSurfaceTextureBottom = 2; 
        [KSPField (isPersistant = true)] public int wingSurfaceTextureBottomCached = 2;

        public Texture wingSurfaceTextureA;
        public Texture wingSurfaceTextureB;
        public Texture wingSurfaceTextureC;




        // Prerequisites / Control surfaces

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Span"),
        UI_FloatRange (minValue = 0.25f, maxValue = 8f, scene = UI_Scene.Editor, stepIncrement = 0.25f)]
        public float ctrlSpan = 1f;
        public float ctrlSpanCached = 1f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Width R"),
        UI_FloatRange (minValue = 0.25f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.25f)]
        public float ctrlWidthRoot = 0.25f;
        public float ctrlWidthRootCached = 0.25f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Width T"),
        UI_FloatRange (minValue = 0.25f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.25f)]
        public float ctrlWidthTip = 0.25f;
        public float ctrlWidthTipCached = 0.25f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height R"),
        UI_FloatRange (minValue = 0.08f, maxValue = 0.48f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float ctrlThicknessRoot = 0.24f;
        public float ctrlThicknessRootCached = 0.24f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Height T"),
        UI_FloatRange (minValue = 0.08f, maxValue = 0.48f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float ctrlThicknessTip = 0.24f;
        public float ctrlThicknessTipCached = 0.24f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Offset R"),
        UI_FloatRange (minValue = -1f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.125f)]
        public float ctrlOffsetRoot = 0.0f; 
        public float ctrlOffsetRootCached = 0.0f;

        [KSPField (isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Offset T"),
        UI_FloatRange (minValue = -1f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.125f)]
        public float ctrlOffsetTip = 0.0f;
        public float ctrlOffsetTipCached = 0.0f; 

        public MeshFilter meshFilterCtrlEdge;
        public MeshFilter meshFilterCtrlEdgeReference;
        public MeshFilter meshFilterCtrlSurfaceTop;
        public MeshFilter meshFilterCtrlSurfaceBottom;

        public MeshReference meshReferenceCtrlEdge;
        public MeshReference meshReferenceCtrlSurfaceTop;
        public MeshReference meshReferenceCtrlSurfaceBottom;

        public Vector2 ctrlSpanLimits = new Vector2 (0.5f, 4f);
        public Vector2 ctrlWidthLimits = new Vector2 (0.25f, 1f);
        public Vector2 ctrlThicknessLimits = new Vector2 (0.08f, 0.48f);
        public Vector2 ctrlOffsetLimits = new Vector2 (-1f, 1f);

        public Transform temporaryCollider;




        // Some handy bools

        [KSPField (isPersistant = true)]
        public bool isAttached = false;

        public bool isStarted = false;
        public bool justDetached = false;
        public bool logUpdate = true;
        public bool logUpdateGeometry = false;




        // Events

        public override void OnStart (PartModule.StartState state)
        {
            base.OnStart (state);
            FARactive = AssemblyLoader.loadedAssemblies.Any (a => a.assembly.GetName ().Name.Equals ("FerramAerospaceResearch", StringComparison.InvariantCultureIgnoreCase));
            NEARactive = AssemblyLoader.loadedAssemblies.Any (a => a.assembly.GetName ().Name.Equals ("NEAR", StringComparison.InvariantCultureIgnoreCase));
            if (FARactive || NEARactive)
            {
                // If FAR|NEAR have the "massPerWingAreaSupported" value, disable mass calculations, and the mass editor info.
                foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes ("FARAeroData"))
                {
                    if (node == null)
                        continue;

                    if (node.HasValue ("massPerWingAreaSupported"))
                        FARmass = true;
                }
            }
        }




        // Update
        // As nothing like GUI.changed or per-KSPFields change delegates is available, we have to resort to dirty comparisons
        // Performance hit shouldn't be too bad as all KSPFields have snapped sliders that can't spam dozens of values per second

        private bool updateRequiredOnMeshes = false;
        private bool updateRequiredOnGeometry = false;
        private bool updateRequiredOnTextures = false;
        private bool updateRequiredOnWindow = false;
        private bool updateCounterparts = false;
        private float updateTimer;

        public void Update ()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (CachedOnEditorAttach == null) CachedOnEditorAttach = new Callback (UpdateOnEditorAttach);
                if (!this.part.OnEditorAttach.GetInvocationList ().Contains (CachedOnEditorAttach)) this.part.OnEditorAttach += CachedOnEditorAttach;

                if (CachedOnEditorDetach == null) CachedOnEditorDetach = new Callback (UpdateOnEditorDetach);
                if (!this.part.OnEditorDetach.GetInvocationList ().Contains (CachedOnEditorDetach)) this.part.OnEditorDetach += CachedOnEditorDetach;

                // Used to determine whether updates are spammed
                // Switch off in release

                updateTimer += Time.deltaTime;
                if (updateTimer > 1000f) updateTimer = 0f;

                if (isStarted)
                {

                    // Next, compare the properties to cached values
                    // If there is a mismatch, then update is required

                    if (!isCtrlSrf)
                    {
                        if (wingSpan != wingSpanCached)
                        {
                            if (logUpdate) Debug.Log ("WP |  Update at " + updateTimer.ToString ("F1") + " | Non-equal wingSpan");
                            updateRequiredOnGeometry = true;
                            wingSpanCached = wingSpan;
                        }
                        if (wingWidthRoot != wingWidthRootCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal wingWidthRoot");
                            updateRequiredOnGeometry = true;
                            wingWidthRootCached = wingWidthRoot;
                            if (syncWidth) wingWidthTip = wingWidthTipCached = wingWidthRoot;
                        }
                        if (wingWidthTip != wingWidthTipCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal wingWidthTip");
                            updateRequiredOnGeometry = true;
                            wingWidthTipCached = wingWidthTip;
                        }
                        if (wingThicknessRoot != wingThicknessRootCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal wingThicknessRoot");
                            updateRequiredOnGeometry = true;
                            wingThicknessRootCached = wingThicknessRoot;
                            if (syncThickness) wingThicknessTip = wingThicknessTipCached = wingThicknessRoot;
                        }
                        if (wingThicknessTip != wingThicknessTipCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal wingThicknessTip");
                            updateRequiredOnGeometry = true;
                            wingThicknessTipCached = wingThicknessTip;
                        }
                        if (wingEdgeTrailing != wingEdgeTrailingCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal wingEdgeTrailing");
                            updateRequiredOnGeometry = true;
                            wingEdgeTrailingCached = wingEdgeTrailing;
                            if (syncEdge) wingEdgeLeading = wingEdgeLeadingCached = wingEdgeTrailing;
                        }
                        if (wingEdgeLeading != wingEdgeLeadingCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal wingEdgeLeading");
                            updateRequiredOnGeometry = true;
                            wingEdgeLeadingCached = wingEdgeLeading;
                        }
                        if (wingOffset != wingOffsetCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal wingOffset");
                            updateRequiredOnGeometry = true;
                            wingOffsetCached = wingOffset;
                        }
                        if (wingEdgeTypeTrailing != wingEdgeTypeTrailingCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal edgeGeometryTrailing");
                            updateRequiredOnMeshes = updateRequiredOnGeometry = true;
                            wingEdgeTypeTrailingCached = wingEdgeTypeTrailing;
                            if (syncEdge) wingEdgeTypeLeading = wingEdgeTypeLeadingCached = wingEdgeTypeTrailing;
                        }
                        if (wingEdgeTypeLeading != wingEdgeTypeLeadingCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal edgeGeometryLeading");
                            updateRequiredOnMeshes = updateRequiredOnGeometry = true;
                            wingEdgeTypeLeadingCached = wingEdgeTypeLeading;
                        }
                        if (wingSurfaceTextureTop != wingSurfaceTextureTopCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal shieldingSurfaceTop");
                            updateRequiredOnTextures = true;
                            wingSurfaceTextureTopCached = wingSurfaceTextureTop;
                        }
                        if (wingSurfaceTextureBottom != wingSurfaceTextureBottomCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal shieldingSurfaceBottom");
                            updateRequiredOnTextures = true;
                            wingSurfaceTextureBottomCached = wingSurfaceTextureBottom;
                        }
                        if (syncEdge != syncEdgeCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal syncEdge");
                            updateRequiredOnGeometry = updateRequiredOnWindow = updateRequiredOnGeometry = true;
                            Fields["wingEdgeLeading"].uiControlEditor.controlEnabled = Fields["wingEdgeLeading"].guiActive = Fields["wingEdgeLeading"].guiActiveEditor = Events["SelectNextEdgeLeading"].guiActiveEditor = !syncEdge;
                            if (syncEdge) wingEdgeLeading = wingEdgeTrailing;
                            syncEdgeCached = syncEdge;
                        }
                        if (syncThickness != syncThicknessCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal syncThickness");
                            updateRequiredOnGeometry = updateRequiredOnWindow = updateRequiredOnGeometry = true;
                            Fields["wingThicknessTip"].uiControlEditor.controlEnabled = Fields["wingThicknessTip"].guiActive = Fields["wingThicknessTip"].guiActiveEditor = !syncThickness;
                            if (syncThickness) wingThicknessTip = wingThicknessRoot;
                            syncThicknessCached = syncThickness;
                        }
                        if (syncWidth != syncWidthCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal syncWidth");
                            updateRequiredOnGeometry = updateRequiredOnWindow = updateRequiredOnGeometry = true;
                            Fields["wingWidthTip"].uiControlEditor.controlEnabled = Fields["wingWidthTip"].guiActive = Fields["wingWidthTip"].guiActiveEditor = !syncWidth;
                            if (syncWidth) wingWidthTip = wingWidthRoot;
                            syncWidthCached = syncWidth;
                        }
                    }
                    else
                    {
                        if (ctrlSpan != ctrlSpanCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal ctrlSpan");
                            updateRequiredOnGeometry = true;
                            ctrlSpanCached = ctrlSpan;
                        }
                        if (ctrlWidthRoot != ctrlWidthRootCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal ctrlWidthRoot");
                            updateRequiredOnGeometry = true;
                            ctrlWidthRootCached = ctrlWidthRoot;
                        }
                        if (ctrlWidthTip != ctrlWidthTipCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal ctrlWidthTip");
                            updateRequiredOnGeometry = true;
                            ctrlWidthTipCached = ctrlWidthTip;
                        }
                        if (ctrlThicknessRoot != ctrlThicknessRootCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal ctrlThicknessRoot");
                            updateRequiredOnGeometry = true;
                            ctrlThicknessRootCached = ctrlThicknessRoot;
                        }
                        if (ctrlThicknessTip != ctrlThicknessTipCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal ctrlThicknessTip");
                            updateRequiredOnGeometry = true;
                            ctrlThicknessTipCached = ctrlThicknessTip;
                        }
                        if (ctrlOffsetRoot != ctrlOffsetRootCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal ctrlOffsetRoot");
                            updateRequiredOnGeometry = true;
                            ctrlOffsetRootCached = ctrlOffsetRoot;
                        }
                        if (ctrlOffsetTip != ctrlOffsetTipCached)
                        {
                            if (logUpdate) Debug.Log ("WP | Update at " + updateTimer.ToString ("F1") + " | Non-equal ctrlOffsetRoot");
                            updateRequiredOnGeometry = true;
                            ctrlOffsetTipCached = ctrlOffsetTip;
                        }
                    }

                    // Trigger update of the counterparts
                    // Has to be done through a special method that overrides their cached values, preventing feedback loop
                    // Also, a somewhat strange check for attachment after detachment, seems to help in a certain case

                    if (updateRequiredOnMeshes || updateRequiredOnGeometry || updateRequiredOnTextures) updateCounterparts = true;
                    else if (justDetached)
                    {
                        justDetached = false;
                        if (isAttached)
                        {
                            CalculateAerodynamicValues ();
                        }
                    }

                    // If some updates were marked, execute them
                    // Updates are split into groups to prevent unnecessary use

                    if (updateRequiredOnMeshes)
                    {
                        if (logUpdate) Debug.Log ("WP | Update required on meshes");
                        updateRequiredOnMeshes = false;
                        UpdateMeshes ();
                    }
                    if (updateRequiredOnGeometry)
                    {
                        if (logUpdate) Debug.Log ("WP | Update required on geometry");
                        updateRequiredOnGeometry = false;
                        UpdateGeometry ();
                    }
                    if (updateRequiredOnTextures)
                    {
                        if (logUpdate) Debug.Log ("WP | Update required on textures");
                        updateRequiredOnTextures = false;
                        UpdateTextures ();
                    }
                    if (updateCounterparts)
                    {
                        if (logUpdate) Debug.Log ("WP | Update required on counterparts");
                        updateCounterparts = false;
                        UpdateCounterparts ();
                    }
                    if (updateRequiredOnWindow)
                    {
                        updateRequiredOnWindow = false;
                        UpdateWindow ();
                    }
                }
                else if (isAttached)
                {
                    Setup ();
                    isStarted = true;
                }
            }
            else
            {
                if (isAttached && !isStarted)
                {
                    Setup ();
                    isStarted = true;
                }
            }
        }




        // Attachment handling

        private Callback CachedOnEditorAttach;
        private Callback CachedOnEditorDetach;

        public void UpdateOnEditorAttach ()
        {
            isAttached = true;
            Debug.Log ("WP | UpdateOnEditorAttach | Fired");
            Setup ();
        }

        public void UpdateOnEditorDetach ()
        {
            // If the root is not null and is a pWing, set its justDetached so it knows to check itself next Update
            if (this.part.parent != null && this.part.parent.Modules.Contains ("WingProcedural"))
                this.part.parent.Modules.OfType<WingProcedural> ().FirstOrDefault ().justDetached = true;

            isAttached = false;
            justDetached = true;
        }




        // Geometry

        private int geometryUpdateCounterDebug = 0;

        public void UpdateGeometry ()
        {
            geometryUpdateCounterDebug += 1;
            Debug.Log ("WP | UpdateGeometry | Started for " + geometryUpdateCounterDebug.ToString ("000") + " time | isCtrlSrf: " + isCtrlSrf);
            if (!isCtrlSrf)
            {
                float wingThicknessDeviationRoot = wingThicknessRoot / 0.24f;
                float wingThicknessDeviationTip = wingThicknessTip / 0.24f;

                if (meshFilterWingSection != null)
                {
                    int length = meshReferenceWingSection.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReferenceWingSection.vp, vp, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReferenceWingSection.uv, uv, length);
                    if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Wing section | Passed array setup");

                    float wingWidthTipBasedOffsetTrailing = wingWidthTip / 2f + wingOffset;
                    float wingWidthTipBasedOffsetLeading = -wingWidthTip / 2f + wingOffset;
                    float wingWidthRootBasedOffset = wingWidthRoot / 2f;

                    vp[0] = new Vector3 (-wingSpan, vp[0].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetLeading);
                    uv[0] = new Vector2 (wingWidthTip, uv[0].y);

                    vp[1] = new Vector3 (-wingSpan, vp[1].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetTrailing);
                    uv[1] = new Vector2 (0f, uv[1].y);

                    vp[2] = new Vector3 (-wingSpan, vp[2].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetTrailing);
                    uv[2] = new Vector2 (0f, uv[2].y);

                    vp[3] = new Vector3 (-wingSpan, vp[3].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetLeading);
                    uv[3] = new Vector2 (wingWidthTip, uv[3].y);

                    vp[4] = new Vector3 (0f, vp[4].y * wingThicknessDeviationRoot, wingWidthRootBasedOffset);
                    uv[4] = new Vector2 (wingWidthRoot, uv[4].y);

                    vp[5] = new Vector3 (0f, vp[5].y * wingThicknessDeviationRoot, wingWidthRootBasedOffset);
                    uv[5] = new Vector2 (wingWidthRoot, uv[5].y);

                    vp[6] = new Vector3 (0f, vp[6].y * wingThicknessDeviationRoot, -wingWidthRootBasedOffset);
                    uv[6] = new Vector2 (0f, uv[6].y);

                    vp[7] = new Vector3 (0f, vp[7].y * wingThicknessDeviationRoot, -wingWidthRootBasedOffset);
                    uv[7] = new Vector2 (0f, uv[7].y);

                    meshFilterWingSection.mesh.vertices = vp;
                    meshFilterWingSection.mesh.uv = uv;
                    meshFilterWingSection.mesh.RecalculateBounds ();

                    MeshCollider meshCollider = meshFilterWingSection.gameObject.GetComponent<MeshCollider> ();
                    if (meshCollider == null) meshCollider = meshFilterWingSection.gameObject.AddComponent<MeshCollider> ();
                    meshCollider.sharedMesh = null;
                    meshCollider.sharedMesh = meshFilterWingSection.mesh;
                    meshCollider.convex = true;
                    if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Wing section | Finished");
                }
                if (meshFilterWingSurfaceTop != null)
                {
                    meshFilterWingSurfaceTop.transform.localPosition = Vector3.zero;
                    meshFilterWingSurfaceTop.transform.localRotation = Quaternion.Euler (0f, 0f, 0f);

                    int length = meshReferenceWingSurfaceTop.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReferenceWingSurfaceTop.vp, vp, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReferenceWingSurfaceTop.uv, uv, length);
                    if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Wing surface top | Passed array setup");

                    vp[0] = new Vector3 (-wingSpan, vp[0].y * wingThicknessDeviationTip, wingWidthTip / 2f + wingOffset);
                    uv[0] = new Vector2 (wingSpan / 4f, 0f + 0.5f - wingWidthTip / 8f - wingOffset / 4f);

                    vp[1] = new Vector3 (0f, vp[1].y * wingThicknessDeviationRoot, wingWidthRoot / 2f);
                    uv[1] = new Vector2 (0f, 0f + 0.5f - wingWidthRoot / 8f);

                    vp[2] = new Vector3 (0f, vp[2].y * wingThicknessDeviationRoot, -wingWidthRoot / 2f);
                    uv[2] = new Vector2 (0.0f, 1f - 0.5f + wingWidthRoot / 8f);

                    vp[3] = new Vector3 (-wingSpan, vp[3].y * wingThicknessDeviationTip, -wingWidthTip / 2f + wingOffset);
                    uv[3] = new Vector2 (wingSpan / 4f, 1f - 0.5f + wingWidthTip / 8f - wingOffset / 4f);

                    meshFilterWingSurfaceTop.mesh.vertices = vp;
                    meshFilterWingSurfaceTop.mesh.uv = uv;
                    meshFilterWingSurfaceTop.mesh.RecalculateBounds ();
                    if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Wing surface top | Finished");
                }
                if (meshFilterWingSurfaceBottom != null)
                {
                    meshFilterWingSurfaceBottom.transform.localPosition = Vector3.zero;
                    meshFilterWingSurfaceBottom.transform.localRotation = Quaternion.Euler (180f, 0f, 0f);

                    int length = meshReferenceWingSurfaceBottom.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReferenceWingSurfaceBottom.vp, vp, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReferenceWingSurfaceBottom.uv, uv, length);
                    if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Wing surface bottom | Passed array setup");

                    vp[0] = new Vector3 (-wingSpan, vp[0].y * wingThicknessDeviationTip, wingWidthTip / 2f - wingOffset);
                    uv[0] = new Vector2 (wingSpan / 4f, 0f + 0.5f - wingWidthTip / 8f + wingOffset / 4f);

                    vp[1] = new Vector3 (0f, vp[1].y * wingThicknessDeviationRoot, wingWidthRoot / 2f);
                    uv[1] = new Vector2 (0f, 0f + 0.5f - wingWidthRoot / 8f);

                    vp[2] = new Vector3 (0f, vp[2].y * wingThicknessDeviationRoot, -wingWidthRoot / 2f);
                    uv[2] = new Vector2 (0f, 1f - 0.5f + wingWidthRoot / 8f);

                    vp[3] = new Vector3 (-wingSpan, vp[3].y * wingThicknessDeviationTip, -wingWidthTip / 2f - wingOffset);
                    uv[3] = new Vector2 (wingSpan / 4f, 1f - 0.5f + wingWidthTip / 8f + wingOffset / 4f);

                    meshFilterWingSurfaceBottom.mesh.vertices = vp;
                    meshFilterWingSurfaceBottom.mesh.uv = uv;
                    meshFilterWingSurfaceBottom.mesh.RecalculateBounds ();
                    if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Wing surface bottom | Finished");
                }
                if (meshFilterWingEdgeA != null && meshFilterWingEdgeB != null && meshFilterWingEdgeC != null)
                {
                    if (meshFilterWingEdgeTrailing != null)
                    {
                        meshFilterWingEdgeTrailing.transform.localPosition = Vector3.zero;
                        meshFilterWingEdgeTrailing.transform.localRotation = Quaternion.Euler (0f, 0f, 0f);

                        MeshReference meshReference = GetWingEdgeReference (wingEdgeTypeTrailing);
                        int length = meshReference.vp.Length;
                        Vector3[] vp = new Vector3[length];
                        Array.Copy (meshReference.vp, vp, length);
                        Vector3[] nm = new Vector3[length];
                        Array.Copy (meshReference.nm, nm, length);
                        Vector2[] uv = new Vector2[length];
                        Array.Copy (meshReference.uv, uv, length);
                        if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Wing edge trailing | Passed array setup | Edge type: " + wingEdgeTypeTrailing + " | Reference length: " + length + " | Mesh length: " + meshFilterWingEdgeTrailing.mesh.vertices.Length);

                        for (int i = 0; i < vp.Length; ++i)
                        {
                            if (nm[i].x == -1.0f) vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, Mathf.Lerp (2f, vp[i].z, wingEdgeTrailing) - 2f + wingWidthTip / 2f + wingOffset); // Tip section
                            else if (nm[i].x == 1.0f) vp[i] = new Vector3 (0f, vp[i].y * wingThicknessDeviationRoot, Mathf.Lerp (2f, vp[i].z, wingEdgeTrailing) - 2f + wingWidthRoot / 2f); // Root section
                            else
                            {
                                if (vp[i].x < -0.1f)
                                {
                                    vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, Mathf.Lerp (2f, vp[i].z, wingEdgeTrailing) - 2f + wingWidthTip / 2f + wingOffset); // Tip edge
                                    uv[i] = new Vector2 (wingSpan, uv[i].y);
                                }
                                else vp[i] = new Vector3 (0f, vp[i].y * wingThicknessDeviationRoot, Mathf.Lerp (2f, vp[i].z, wingEdgeTrailing) - 2f + wingWidthRoot / 2f); // Root edge
                            }
                        }

                        meshFilterWingEdgeTrailing.mesh.vertices = vp;
                        meshFilterWingEdgeTrailing.mesh.uv = uv;
                        meshFilterWingEdgeTrailing.mesh.RecalculateBounds ();
                        if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Wing edge trailing | Finished");
                    }
                    if (meshFilterWingEdgeLeading != null)
                    {
                        meshFilterWingEdgeLeading.transform.localPosition = Vector3.zero;
                        meshFilterWingEdgeLeading.transform.localRotation = Quaternion.Euler (180f, 0f, 0f);

                        MeshReference meshReference = GetWingEdgeReference (wingEdgeTypeLeading);
                        int length = meshReference.vp.Length;
                        Vector3[] vp = new Vector3[length];
                        Array.Copy (meshReference.vp, vp, length);
                        Vector3[] nm = new Vector3[length];
                        Array.Copy (meshReference.nm, nm, length);
                        Vector2[] uv = new Vector2[length];
                        Array.Copy (meshReference.uv, uv, length);
                        if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Wing edge leading | Passed array setup | Edge type: " + wingEdgeTypeLeading + " | Reference length: " + length + " | Mesh length: " + meshFilterWingEdgeLeading.mesh.vertices.Length);

                        for (int i = 0; i < vp.Length; ++i)
                        {
                            if (nm[i].x == -1.0f) vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, Mathf.Lerp (2f, vp[i].z, wingEdgeLeading) - 2f + wingWidthTip / 2f - wingOffset); // Tip section
                            else if (nm[i].x == 1.0f) vp[i] = new Vector3 (0f, vp[i].y * wingThicknessDeviationRoot, Mathf.Lerp (2f, vp[i].z, wingEdgeLeading) - 2f + wingWidthRoot / 2f); // Root section
                            else
                            {
                                if (vp[i].x < -0.1f)
                                {
                                    vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, Mathf.Lerp (2f, vp[i].z, wingEdgeLeading) - 2f + wingWidthTip / 2f - wingOffset); // Tip edge
                                    uv[i] = new Vector2 (wingSpan, uv[i].y);
                                }
                                else vp[i] = new Vector3 (0f, vp[i].y * wingThicknessDeviationRoot, Mathf.Lerp (2f, vp[i].z, wingEdgeLeading) - 2f + wingWidthRoot / 2f); // Root edge
                            }
                        }

                        meshFilterWingEdgeLeading.mesh.vertices = vp;
                        meshFilterWingEdgeLeading.mesh.uv = uv;
                        meshFilterWingEdgeLeading.mesh.RecalculateBounds ();
                        if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Wing edge leading | Finished");
                    }
                }
            }
            else
            {
                // To prevent intersections on low span to width configurations

                float ctrlOffsetRootLimit = (ctrlSpan / 2f) / (ctrlWidthRoot + 1f);
                float ctrlOffsetTipLimit = (ctrlSpan / 2f) / (ctrlWidthTip + 1f);

                float ctrlOffsetRootClamped = Mathf.Clamp (ctrlOffsetRoot, -ctrlOffsetRootLimit, ctrlOffsetRootLimit);
                float ctrlOffsetTipClamped = Mathf.Clamp (ctrlOffsetTip, -ctrlOffsetTipLimit, ctrlOffsetTipLimit);

                float ctrlThicknessDeviationRoot = ctrlThicknessRoot / 0.24f;
                float ctrlThicknessDeviationTip = ctrlThicknessTip / 0.24f;

                if (meshFilterCtrlEdge != null)
                {
                    int length = meshReferenceCtrlEdge.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReferenceCtrlEdge.vp, vp, length);
                    Vector3[] nm = new Vector3[length];
                    Array.Copy (meshReferenceCtrlEdge.nm, nm, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReferenceCtrlEdge.uv, uv, length);
                    if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Control surface edge | Passed array setup");

                    for (int i = 0; i < vp.Length; ++i)
                    {
                        if (nm[i] != new Vector3 (1f, 0f, 0f) && nm[i] != new Vector3 (-1f, 0f, 0f))
                        {
                            if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationTip, vp[i].y, -ctrlSpan / 2f);
                            else vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationRoot, vp[i].y, ctrlSpan / 2f);

                            // Sides
                            if (nm[i] == new Vector3 (0f, 0f, 1f) || nm[i] == new Vector3 (0f, 0f, -1f))
                            {
                                if (uv[i].y > 0.185f)
                                {
                                    if (vp[i].y < -0.01f)
                                    {
                                        if (vp[i].z < 0f)
                                        {
                                            vp[i] = new Vector3 (vp[i].x, -ctrlWidthTip, vp[i].z);
                                            uv[i] = new Vector2 (ctrlWidthTip, uv[i].y);
                                        }
                                        else
                                        {
                                            vp[i] = new Vector3 (vp[i].x, -ctrlWidthRoot, vp[i].z);
                                            uv[i] = new Vector2 (ctrlWidthRoot, uv[i].y);
                                        }
                                    }
                                }
                                else
                                {
                                    if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthTip, vp[i].z);
                                    else vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthRoot, vp[i].z);
                                }
                            }

                            // Root
                            else if (nm[i] == new Vector3 (0f, 1f, 0f))
                            {
                                if (vp[i].z < 0f) uv[i] = new Vector2 (ctrlSpan, uv[i].y);
                            }

                            // Trailing edge
                            else
                            {
                                if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthTip, vp[i].z);
                                else vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthRoot, vp[i].z);
                            }

                            // Offset
                            if (vp[i].z < 0f)
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetTipClamped);
                                if (nm[i] != new Vector3 (0f, 0f, 1f) && nm[i] != new Vector3 (0f, 0f, -1f)) uv[i] = new Vector2 (uv[i].x - (vp[i].y * ctrlOffsetTipClamped) / 4f, uv[i].y);
                            }
                            else
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetRootClamped);
                                if (nm[i] != new Vector3 (0f, 0f, 1f) && nm[i] != new Vector3 (0f, 0f, -1f)) uv[i] = new Vector2 (uv[i].x - (vp[i].y * ctrlOffsetRootClamped) / 4f, uv[i].y);
                            }

                            // Trailing edge UVs

                            if (nm[i] != new Vector3 (0f, 1f, 0f) && nm[i] != new Vector3 (0f, 0f, 1f) && nm[i] != new Vector3 (0f, 0f, -1f))
                            {
                                if (vp[i].z < 0f) uv[i] = new Vector2 (vp[i].z, uv[i].y);
                                else uv[i] = new Vector2 (vp[i].z, uv[i].y);
                            }
                        }

                        // Border
                        else
                        {
                            // Span-based shift
                            if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x, vp[i].y, vp[i].z + 0.5f - ctrlSpan / 2f);
                            else vp[i] = new Vector3 (vp[i].x, vp[i].y, vp[i].z - 0.5f + ctrlSpan / 2f);

                            // Width-based shift
                            if (vp[i].y < -0.1f)
                            {
                                if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthTip, vp[i].z);
                                else vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthRoot, vp[i].z);
                            }

                            // Offsets && thickness
                            if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationTip + 0.00f * nm[i].x, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetTipClamped);
                            else vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationRoot + 0.00f * nm[i].x, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetRootClamped);
                        }
                    }

                    meshFilterCtrlEdge.mesh.vertices = vp;
                    meshFilterCtrlEdge.mesh.uv = uv;
                    meshFilterCtrlEdge.mesh.RecalculateBounds ();

                    MeshCollider meshCollider = meshFilterCtrlEdge.gameObject.GetComponent<MeshCollider> ();
                    if (meshCollider == null) meshCollider = meshFilterCtrlEdge.gameObject.AddComponent<MeshCollider> ();
                    meshCollider.sharedMesh = null;
                    meshCollider.sharedMesh = meshFilterCtrlEdge.mesh;
                    meshCollider.convex = true;
                    if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Control surface edge | Finished");
                }
                if (meshFilterCtrlSurfaceTop != null)
                {
                    int length = meshReferenceCtrlSurfaceTop.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReferenceCtrlSurfaceTop.vp, vp, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReferenceCtrlSurfaceTop.uv, uv, length);
                    if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Control surface top | Passed array setup");

                    for (int i = 0; i < vp.Length; ++i)
                    {
                        // Span-based shift
                        if (vp[i].z < 0f)
                        {
                            vp[i] = new Vector3 (vp[i].x, vp[i].y, vp[i].z + 0.5f - ctrlSpan / 2f);
                            uv[i] = new Vector2 (0f, uv[i].y);
                        }
                        else
                        {
                            vp[i] = new Vector3 (vp[i].x, vp[i].y, vp[i].z - 0.5f + ctrlSpan / 2f);
                            uv[i] = new Vector2 (ctrlSpan / 4f, uv[i].y);
                        }

                        // Width-based shift
                        if (vp[i].y < -0.1f)
                        {
                            if (vp[i].z < 0f)
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthTip, vp[i].z);
                                uv[i] = new Vector2 (uv[i].x, ctrlWidthTip / 4f);
                            }
                            else
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthRoot, vp[i].z);
                                uv[i] = new Vector2 (uv[i].x, ctrlWidthRoot / 4f);
                            }
                        }
                        else uv[i] = new Vector2 (uv[i].x, 0f);

                        // Offsets & thickness
                        if (vp[i].z < 0f)
                        {
                            vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationTip, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetTipClamped);
                            uv[i] = new Vector2 (uv[i].x + (vp[i].y * ctrlOffsetTipClamped) / 4f, uv[i].y);
                        }
                        else
                        {
                            vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationRoot, vp[i].y, vp[i].z + vp[i].y * ctrlOffsetRootClamped);
                            uv[i] = new Vector2 (uv[i].x + (vp[i].y * ctrlOffsetRootClamped) / 4f, uv[i].y);
                        }
                    }
                    meshFilterCtrlSurfaceTop.mesh.vertices = vp;
                    meshFilterCtrlSurfaceTop.mesh.uv = uv;
                    meshFilterCtrlSurfaceTop.mesh.RecalculateBounds ();
                    if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Control surface top | Finished");
                }
                if (meshFilterCtrlSurfaceBottom != null)
                {
                    int length = meshReferenceCtrlSurfaceBottom.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReferenceCtrlSurfaceBottom.vp, vp, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReferenceCtrlSurfaceBottom.uv, uv, length);
                    if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Control surface bottom | Passed array setup");

                    for (int i = 0; i < vp.Length; ++i)
                    {
                        // Span-based shift
                        if (vp[i].z < 0f)
                        {
                            vp[i] = new Vector3 (vp[i].x, vp[i].y, vp[i].z + 0.5f - ctrlSpan / 2f);
                            uv[i] = new Vector2 (0f, uv[i].y);
                        }
                        else
                        {
                            vp[i] = new Vector3 (vp[i].x, vp[i].y, vp[i].z - 0.5f + ctrlSpan / 2f);
                            uv[i] = new Vector2 (ctrlSpan / 4f, uv[i].y);
                        }

                        // Width-based shift
                        if (vp[i].y < -0.1f)
                        {
                            if (vp[i].z < 0f)
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthRoot, vp[i].z);
                                uv[i] = new Vector2 (uv[i].x, ctrlWidthRoot / 4f);
                            }
                            else
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthTip, vp[i].z);
                                uv[i] = new Vector2 (uv[i].x, ctrlWidthTip / 4f);
                            }
                        }
                        else uv[i] = new Vector2 (uv[i].x, 0f);

                        // Offsets & thickness
                        if (vp[i].z < 0f)
                        {
                            vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationRoot, vp[i].y, vp[i].z - vp[i].y * ctrlOffsetRootClamped);
                            uv[i] = new Vector2 (uv[i].x - (vp[i].y * ctrlOffsetRootClamped) / 4f, uv[i].y);
                        }
                        else
                        {
                            vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationTip, vp[i].y, vp[i].z - vp[i].y * ctrlOffsetTipClamped);
                            uv[i] = new Vector2 (uv[i].x - (vp[i].y * ctrlOffsetTipClamped) / 4f, uv[i].y);
                        }
                    }
                    meshFilterCtrlSurfaceBottom.mesh.vertices = vp;
                    meshFilterCtrlSurfaceBottom.mesh.uv = uv;
                    meshFilterCtrlSurfaceBottom.mesh.RecalculateBounds ();
                    if (logUpdateGeometry) Debug.Log ("WP | UG-" + geometryUpdateCounterDebug.ToString ("000") + " | Control surface bottom | Finished");
                }
            }
            Debug.Log ("WP | UpdateGeometry | Finished");
            CalculateAerodynamicValues ();
        }




        // Edge geometry

        public MeshReference GetWingEdgeReference (int selection)
        {
            if (selection == 0) return meshReferenceWingEdgeA;
            else if (selection == 1) return meshReferenceWingEdgeB;
            else return meshReferenceWingEdgeC;
        }

        public MeshFilter GetWingEdgeFilter (int selection)
        {
            if (selection == 0) return meshFilterWingEdgeA;
            else if (selection == 1) return meshFilterWingEdgeB;
            else return meshFilterWingEdgeC;
        }

        public Vector3[] GetReferenceVertices (MeshFilter source)
        {
            Vector3[] positions = new Vector3[0];
            if (source != null)
            {
                if (source.mesh != null)
                {
                    positions = source.mesh.vertices;
                    return positions;
                }
            }
            return positions;
        }

        public void UpdateMeshes ()
        {
            if (!isCtrlSrf)
            {
                if (meshFilterWingEdgeTrailing.mesh != null)
                {
                    Debug.Log ("WP | UM | Wing trailing edge | Mesh present | Length: " + meshFilterWingEdgeTrailing.mesh.vertices.Length + " | Removing");
                    DestroyImmediate (meshFilterWingEdgeTrailing.mesh);
                }
                meshFilterWingEdgeTrailing.mesh = Instantiate (GetWingEdgeFilter (wingEdgeTypeTrailing).mesh) as Mesh;
                Debug.Log ("WP | UM | Wing trailing edge | Mesh created | Length: " + meshFilterWingEdgeTrailing.mesh.vertices.Length);

                if (meshFilterWingEdgeLeading.mesh != null)
                {
                    Debug.Log ("WP | UM | Wing trailing edge | Mesh present | Length: " + meshFilterWingEdgeLeading.mesh.vertices.Length + " | Removing");
                    DestroyImmediate (meshFilterWingEdgeLeading.mesh);
                }
                meshFilterWingEdgeLeading.mesh = Instantiate (GetWingEdgeFilter (wingEdgeTypeLeading).mesh) as Mesh;
                Debug.Log ("WP | UM | Wing trailing edge | Mesh created | Length: " + meshFilterWingEdgeLeading.mesh.vertices.Length);
            }
        }




        // Materials

        public void UpdateTextures ()
        {
            if (!isCtrlSrf)
            {
                if (meshFilterWingSurfaceTop != null)
                {
                    Renderer r = meshFilterWingSurfaceTop.gameObject.GetComponent<Renderer> ();
                    Texture t = GetShieldingTexture (wingSurfaceTextureTop);
                    SetTexture (r, t);
                }
                if (meshFilterWingSurfaceBottom != null)
                {
                    Renderer r = meshFilterWingSurfaceBottom.gameObject.GetComponent<Renderer> ();
                    Texture t = GetShieldingTexture (wingSurfaceTextureBottom);
                    SetTexture (r, t);
                }
            }
        }

        private Texture GetShieldingTexture (int selection)
        {
            if (selection == 0) return wingSurfaceTextureA;
            else if (selection == 1) return wingSurfaceTextureB;
            else return wingSurfaceTextureC;
        }

        private void SetTexture (Renderer r, Texture t)
        {
            if (r != null)
            {
                if (t != r.sharedMaterial.GetTexture ("_MainTex"))
                    r.sharedMaterial.SetTexture ("_MainTex", t);
            }
        }




        // Aerodynamics value calculation
        // More or less lifted from pWings, so credit goes to DYJ and Taverius

        private bool FARactive = false;
        private bool NEARactive = false;
        private bool FARmass = false;

        [KSPField] public bool isCtrlSrf = true;

        [KSPField] public float liftFudgeNumber = 0.0775f;
        [KSPField] public float massFudgeNumber = 0.015f;
        [KSPField] public float dragBaseValue = 0.6f;
        [KSPField] public float dragMultiplier = 3.3939f;
        [KSPField] public float connectionFactor = 150f;
        [KSPField] public float connectionMinimum = 50f;
        [KSPField] public float costDensity = 5300f;
        [KSPField] public float costDensityControl = 6500f;
        [KSPField] public float modelControlSurfaceFraction = 1f;

        [KSPField (guiActiveEditor = false, guiName = "Coefficient of Drag", guiFormat = "F3")]
        public float guiCd;

        [KSPField (guiActiveEditor = false, guiName = "Coefficient of Lift", guiFormat = "F3")]
        public float guiCl;

        [KSPField (guiActiveEditor = false, guiName = "Mass", guiFormat = "F3", guiUnits = "t")]
        public float guiWingMass;

        [KSPField (guiActiveEditor = false, guiName = "Cost")]
        public float guiWingCost;

        [KSPField (guiActiveEditor = false, guiName = "Mean Aerodynamic Chord", guiFormat = "F3", guiUnits = "m")]
        public float guiMAC;

        [KSPField (guiActiveEditor = false, guiName = "Semi-Span", guiFormat = "F3", guiUnits = "m")]
        public float guiB_2;

        [KSPField (guiActiveEditor = false, guiName = "Mid-Chord Sweep", guiFormat = "F3", guiUnits = "deg.")]
        public float guiMidChordSweep;

        [KSPField (guiActiveEditor = false, guiName = "Taper Ratio", guiFormat = "F3")]
        public float guiTaperRatio;

        [KSPField (guiActiveEditor = false, guiName = "Surface Area", guiFormat = "F3", guiUnits = "m²")]
        public float guiSurfaceArea;

        [KSPField (guiActiveEditor = false, guiName = "Aspect Ratio", guiFormat = "F3")]
        public float guiAspectRatio;

        public double Cd;
        public double Cl;
        public double ChildrenCl;
        public double wingMass;
        public double connectionForce;

        public double meanAerodynamicChord; // MAC
        public double b_2;
        public double midChordSweep;
        public double taperRatio;
        public double surfaceArea;
        public double aspectRatio;
        public double aspectRatioSweepScale;

        public void CalculateAerodynamicValues ()
        {
            if (isAttached || HighLogic.LoadedSceneIsFlight)
            {
                // float offsetLeading = Mathf.Abs (wingWidthRoot / 2f - (wingWidthTip / 2f - wingOffset));
                // float offsetTrailing = Mathf.Abs (wingWidthRoot / 2f - (wingWidthTip / 2f + wingOffset));

                // float edgeLengthLeading = Mathf.Sqrt (Mathf.Pow (offsetLeading, 2) + Mathf.Pow (wingSpan, 2));
                // float edgeLengthTrailing = Mathf.Sqrt (Mathf.Pow (offsetTrailing, 2) + Mathf.Pow (wingSpan, 2));

                // float sweepLeading = Mathf.Atan (offsetLeading / wingSpan) * Mathf.Rad2Deg;
                // float sweepTrailing =  Mathf.Atan (offsetTrailing / wingSpan) * Mathf.Rad2Deg;

                if (!isCtrlSrf)
                {
                    b_2 = wingSpan;
                    taperRatio = (double) wingWidthTip / (double) wingWidthRoot;
                    meanAerodynamicChord = (double) (wingWidthTip + wingWidthRoot) / 2.0;
                    midChordSweep = MathD.Atan ((double) wingOffset / (double) wingSpan) * MathD.Rad2Deg; // (double)(sweepLeading + sweepTrailing) / 2.0;
                }
                else
                {
                    b_2 = ctrlSpan;
                    taperRatio = (double) ctrlWidthTip / (double) ctrlWidthRoot;
                    meanAerodynamicChord = (double) (ctrlWidthTip + ctrlWidthRoot) / 2.0;
                    midChordSweep = MathD.Atan ((double) Mathf.Abs (ctrlWidthRoot - ctrlWidthTip) / (double) ctrlSpan) * MathD.Rad2Deg;
                }

                surfaceArea = meanAerodynamicChord * b_2;
                aspectRatio = 2.0f * b_2 / meanAerodynamicChord;

                aspectRatioSweepScale = MathD.Pow (aspectRatio / MathD.Cos (MathD.Deg2Rad * midChordSweep), 2.0f) + 4.0f;
                aspectRatioSweepScale = 2.0f + MathD.Sqrt (aspectRatioSweepScale);
                aspectRatioSweepScale = (2.0f * MathD.PI) / aspectRatioSweepScale * aspectRatio;

                wingMass = MathD.Clamp (massFudgeNumber * surfaceArea * ((aspectRatioSweepScale * 2.0) / (3.0 + aspectRatioSweepScale)) * ((1.0 + taperRatio) / 2), 0.01, double.MaxValue);
                Cd = dragBaseValue / aspectRatioSweepScale * dragMultiplier;
                Cl = liftFudgeNumber * surfaceArea * aspectRatioSweepScale;

                connectionForce = MathD.Round (MathD.Clamp (MathD.Sqrt (Cl + ChildrenCl) * (double) connectionFactor, (double) connectionMinimum, double.MaxValue));

                // Values always set

                if (!isCtrlSrf)
                {
                    guiWingCost = (float) wingMass * (1f + (float) aspectRatioSweepScale / 4f) * costDensity;
                    guiWingCost = Mathf.Round (guiWingCost / 5f) * 5f;
                }
                else
                {
                    guiWingCost = (float) wingMass * (1f + (float) aspectRatioSweepScale / 4f) * costDensity * (1f - modelControlSurfaceFraction);
                    guiWingCost += (float) wingMass * (1f + (float) aspectRatioSweepScale / 4f) * costDensityControl * modelControlSurfaceFraction;
                    guiWingCost = Mathf.Round (guiWingCost / 5f) * 5f;
                }

                part.breakingForce = Mathf.Round ((float) connectionForce);
                part.breakingTorque = Mathf.Round ((float) connectionForce);

                // Stock-only values

                if ((!FARactive && !NEARactive) || !FARmass)
                {
                    part.mass = Mathf.Round ((float) wingMass * 100f) / 100f;
                }
                if (!FARactive && !NEARactive)
                {
                    if (!isCtrlSrf)
                    {
                        ((Winglet) this.part).deflectionLiftCoeff = Mathf.Round ((float) Cl * 100f) / 100f;
                        ((Winglet) this.part).dragCoeff = Mathf.Round ((float) Cd * 100f) / 100f;
                    }
                    else
                    {
                        var mCtrlSrf = part.Modules.OfType<ModuleControlSurface> ().FirstOrDefault ();
                        mCtrlSrf.deflectionLiftCoeff = Mathf.Round ((float) Cl * 100f) / 100f;
                        mCtrlSrf.dragCoeff = Mathf.Round ((float) Cd * 100f) / 100f;
                        mCtrlSrf.ctrlSurfaceArea = modelControlSurfaceFraction;
                    }
                }

                // FAR values
                // With reflection stuff from r4m0n

                if (FARactive || NEARactive)
                {
                    if (part.Modules.Contains ("FARControllableSurface"))
                    {
                        PartModule FARmodule = part.Modules["FARControllableSurface"];
                        Type FARtype = FARmodule.GetType ();
                        FARtype.GetField ("b_2").SetValue (FARmodule, b_2);
                        FARtype.GetField ("MAC").SetValue (FARmodule, meanAerodynamicChord);
                        FARtype.GetField ("S").SetValue (FARmodule, surfaceArea);
                        FARtype.GetField ("MidChordSweep").SetValue (FARmodule, midChordSweep);
                        FARtype.GetField ("TaperRatio").SetValue (FARmodule, taperRatio);
                        FARtype.GetField ("ctrlSurfFrac").SetValue (FARmodule, modelControlSurfaceFraction);
                        FARtype.GetMethod ("StartInitialization").Invoke (FARmodule, null); // if (doInteraction)
                    }
                    else if (part.Modules.Contains ("FARWingAerodynamicModel"))
                    {
                        PartModule FARmodule = part.Modules["FARWingAerodynamicModel"];
                        Type FARtype = FARmodule.GetType ();
                        FARtype.GetField ("b_2").SetValue (FARmodule, b_2);
                        FARtype.GetField ("MAC").SetValue (FARmodule, meanAerodynamicChord);
                        FARtype.GetField ("S").SetValue (FARmodule, surfaceArea);
                        FARtype.GetField ("MidChordSweep").SetValue (FARmodule, midChordSweep);
                        FARtype.GetField ("TaperRatio").SetValue (FARmodule, taperRatio);
                        FARtype.GetMethod ("StartInitialization").Invoke (FARmodule, null); // if (doInteraction)
                    }
                }
                // Update GUI values

                if (!FARactive && !NEARactive)
                {
                    guiCd = Mathf.Round ((float) Cd * 100f) / 100f;
                    guiCl = Mathf.Round ((float) Cl * 100f) / 100f;
                }
                if ((!FARactive && !NEARactive) || !FARmass) guiWingMass = part.mass;
                guiMAC = (float) meanAerodynamicChord;
                guiB_2 = (float) b_2;
                guiMidChordSweep = (float) midChordSweep;
                guiTaperRatio = (float) taperRatio;
                guiSurfaceArea = (float) surfaceArea;
                guiAspectRatio = (float) aspectRatio;
                if (HighLogic.LoadedSceneIsEditor) GameEvents.onEditorShipModified.Fire (EditorLogic.fetch.ship);
            }
        }

        public void OnCenterOfLiftQuery (CenterOfLiftQuery qry)
        {
            if (isAttached && !FARactive)
            {
                qry.lift = (float) Cl;
            }
        }

        public void GatherChildrenCl ()
        {
            ChildrenCl = 0;

            // Add up the Cl and ChildrenCl of all our children to our ChildrenCl
            foreach (Part p in this.part.children)
            {
                if (p.Modules.Contains ("WingProcedural"))
                {
                    var child = p.Modules.OfType<WingProcedural> ().FirstOrDefault ();
                    ChildrenCl += child.Cl;
                    ChildrenCl += child.ChildrenCl;
                }
            }

            // If parent is a pWing, trickle the call to gather ChildrenCl down to them.
            if (this.part.parent != null && this.part.parent.Modules.Contains ("WingProcedural"))
            {
                var Parent = this.part.parent.Modules.OfType<WingProcedural> ().FirstOrDefault ();
                Parent.GatherChildrenCl ();
            }
        }




        public bool showWingData = false;
        [KSPEvent (guiActiveEditor = true, guiName = "Show wing data")]
        public void InfoToggleEvent ()
        {
            if (isAttached &&
                this.part.parent != null)
            {
                showWingData = !showWingData;
                if (showWingData) Events["InfoToggleEvent"].guiName = "Hide wing data";
                else Events["InfoToggleEvent"].guiName = "Show wing data";

                // If FAR|NEAR arent present, toggle Cl/Cd
                if (!FARactive && !NEARactive)
                {
                    Fields["guiCd"].guiActiveEditor = showWingData;
                    Fields["guiCl"].guiActiveEditor = showWingData;
                }

                // If FAR|NEAR are not present, or its a version without wing mass calculations, toggle wing mass
                if ((!FARactive && !NEARactive) || !FARmass)
                    Fields["guiWingMass"].guiActive = showWingData;

                // Toggle the rest of the info values
                Fields["guiWingCost"].guiActiveEditor = showWingData;
                Fields["guiMAC"].guiActiveEditor = showWingData;
                Fields["guiB_2"].guiActiveEditor = showWingData;
                Fields["guiMidChordSweep"].guiActiveEditor = showWingData;
                Fields["guiTaperRatio"].guiActiveEditor = showWingData;
                Fields["guiSurfaceArea"].guiActiveEditor = showWingData;
                Fields["guiAspectRatio"].guiActiveEditor = showWingData;

                // Force tweakable window to refresh
                if (myWindow != null)
                    myWindow.displayDirty = true;
            }
        }




        // Debug info

        public void DumpInfo ()
        {
            Debug.Log
            (
                "WP | Dumping data..." +
                " | Span: " + wingSpan +
                " | Width / R: " + wingWidthRoot +
                " | Width / T: " + wingWidthTip +
                " | Thickness / R: " + wingThicknessRoot +
                " | Thickness / T: " + wingThicknessTip +
                " | Edge T / Length: " + wingEdgeTrailing +
                " | Edge T / Type: " + wingEdgeTypeTrailing +
                " | Edge L / Length: " + wingEdgeLeading +
                " | Edge L / Type: " + wingEdgeTypeLeading +
                " | Sync / Width: " + syncWidth +
                " | Sync / Whickness: " + syncThickness +
                " | Sync / Edge length: " + syncEdge +
                " | Shielding / T: " + wingSurfaceTextureTop +
                " | Shielding / B: " + wingSurfaceTextureBottom
            );
            if (myWindow != null) myWindow.displayDirty = true;
        }




        // Setup

        public void Setup ()
        {
            Debug.Log ("WP | Setup | Started on object ID: " + part.GetInstanceID ());
            SetupMeshFilters ();
            SetupClamping ();
            SetupFields ();
            SetupMeshReferences ();
            SetupTemporaryCollider ();
            ReportOnMeshReferences ();
            SetupRecurring ();
        }

        public void SetupRecurring ()
        {
            UpdateMeshes ();
            UpdateGeometry ();
            UpdateTextures ();
            UpdateWindow ();
        }

        public void UpdateCounterparts ()
        {
            Debug.Log ("WP | UpdateCounterparts | Started on object ID: " + part.GetInstanceID ());
            for (int i = 0; i < this.part.symmetryCounterparts.Count; ++i)
            {
                var clone = this.part.symmetryCounterparts[i].Modules.OfType<WingProcedural> ().FirstOrDefault ();
                if (!isCtrlSrf)
                {
                    clone.syncWidth =                   clone.syncWidthCached =                 syncWidth;
                    clone.syncThickness =               clone.syncThicknessCached =             syncThickness;
                    clone.syncEdge =                    clone.syncEdgeCached =                  syncEdge;
                    clone.wingSpan =                    clone.wingSpanCached =                  wingSpan;
                    clone.wingWidthRoot =               clone.wingWidthRootCached =             wingWidthRoot;
                    clone.wingWidthTip =                clone.wingWidthTipCached =              wingWidthTip;
                    clone.wingThicknessRoot =           clone.wingThicknessRootCached =         wingThicknessRoot;
                    clone.wingThicknessTip =            clone.wingThicknessTipCached =          wingThicknessTip;
                    clone.wingOffset =                  clone.wingOffsetCached =                wingOffset;
                    clone.wingEdgeTrailing =            clone.wingEdgeTrailingCached =          wingEdgeTrailing;
                    clone.wingEdgeLeading =             clone.wingEdgeLeadingCached =           wingEdgeLeading;
                    clone.wingEdgeTypeTrailing =        clone.wingEdgeTypeTrailingCached =      wingEdgeTypeTrailing;
                    clone.wingEdgeTypeLeading =         clone.wingEdgeTypeLeadingCached =       wingEdgeTypeLeading;
                    clone.wingSurfaceTextureTop =       clone.wingSurfaceTextureTopCached =     wingSurfaceTextureTop;
                    clone.wingSurfaceTextureBottom =    clone.wingSurfaceTextureBottomCached =  wingSurfaceTextureBottom;
                }
                else
                {
                    clone.ctrlSpan =                    clone.ctrlSpanCached =                  ctrlSpan;
                    clone.ctrlWidthRoot =               clone.ctrlWidthRootCached =             ctrlWidthRoot;
                    clone.ctrlWidthTip =                clone.ctrlWidthTipCached =              ctrlWidthTip;
                    clone.ctrlThicknessRoot =           clone.ctrlThicknessRootCached =         ctrlThicknessRoot;
                    clone.ctrlThicknessTip =            clone.ctrlThicknessTipCached =          ctrlThicknessTip;
                    clone.ctrlOffsetRoot =              clone.ctrlOffsetRootCached =            ctrlOffsetRoot;
                    clone.ctrlOffsetTip =               clone.ctrlOffsetTipCached =             ctrlOffsetTip;
                }
                clone.SetupRecurring ();
            }
        }

        private void SetupMeshFilters ()
        {
            if (!isCtrlSrf)
            {
                meshFilterWingSurfaceTop = CheckMeshFilter (meshFilterWingSurfaceTop, "surface_top");
                meshFilterWingSurfaceBottom = CheckMeshFilter (meshFilterWingSurfaceBottom, "surface_bottom");
                meshFilterWingEdgeTrailing = CheckMeshFilter (meshFilterWingEdgeTrailing, "edge_trailing");
                meshFilterWingEdgeLeading = CheckMeshFilter (meshFilterWingEdgeLeading, "edge_leading");
                meshFilterWingSection = CheckMeshFilter (meshFilterWingSection, "proxy_collision");
                meshFilterWingEdgeA = CheckMeshFilter (meshFilterWingEdgeA, "proxy_edge_a", true);
                meshFilterWingEdgeB = CheckMeshFilter (meshFilterWingEdgeB, "proxy_edge_b", true);
                meshFilterWingEdgeC = CheckMeshFilter (meshFilterWingEdgeC, "proxy_edge_c", true);
                meshFilterWingSurfaceA = CheckMeshFilter (meshFilterWingSurfaceA, "proxy_material_a", true);
                meshFilterWingSurfaceB = CheckMeshFilter (meshFilterWingSurfaceB, "proxy_material_b", true);
                meshFilterWingSurfaceC = CheckMeshFilter (meshFilterWingSurfaceC, "proxy_material_c", true);

                wingSurfaceTextureA = CheckTexture (wingSurfaceTextureA, meshFilterWingSurfaceA);
                wingSurfaceTextureB = CheckTexture (wingSurfaceTextureB, meshFilterWingSurfaceB);
                wingSurfaceTextureC = CheckTexture (wingSurfaceTextureC, meshFilterWingSurfaceC);
            }
            else
            {
                meshFilterCtrlEdge = CheckMeshFilter (meshFilterCtrlEdge, "ctrl_surface_edge");
                meshFilterCtrlEdgeReference = CheckMeshFilter (meshFilterCtrlEdgeReference, "ctrl_surface_edge_reference", true);
                meshFilterCtrlSurfaceTop = CheckMeshFilter (meshFilterCtrlSurfaceTop, "ctrl_surface_top");
                meshFilterCtrlSurfaceBottom = CheckMeshFilter (meshFilterCtrlSurfaceBottom, "ctrl_surface_bottom");
            }
        }

        private void SetupClamping ()
        {
            if (!isCtrlSrf)
            {
                wingSpan = Mathf.Clamp (wingSpan, wingSpanLimits.x, wingSpanLimits.y);
                wingWidthRoot = Mathf.Clamp (wingWidthRoot, wingWidthLimits.x, wingWidthLimits.y);
                wingWidthTip = Mathf.Clamp (wingWidthTip, wingWidthLimits.x, wingWidthLimits.y);
                wingThicknessRoot = Mathf.Clamp (wingThicknessRoot, wingThicknessLimits.x, wingThicknessLimits.y);
                wingThicknessTip = Mathf.Clamp (wingThicknessTip, wingThicknessLimits.x, wingThicknessLimits.y);
                wingOffset = Mathf.Clamp (wingOffset, wingOffsetLimits.x, wingOffsetLimits.y);
                wingEdgeTrailing = Mathf.Clamp (wingEdgeTrailing, wingEdgeLimits.x, wingEdgeLimits.y);
                wingEdgeLeading = Mathf.Clamp (wingEdgeLeading, wingEdgeLimits.x, wingEdgeLimits.y);

            }
            else
            {
                ctrlSpan = Mathf.Clamp (ctrlSpan, ctrlSpanLimits.x, ctrlSpanLimits.y);
                ctrlWidthRoot = Mathf.Clamp (ctrlWidthRoot, ctrlWidthLimits.x, ctrlWidthLimits.y);
                ctrlWidthTip = Mathf.Clamp (ctrlWidthTip, ctrlWidthLimits.x, ctrlWidthLimits.y);
                ctrlThicknessRoot = Mathf.Clamp (ctrlThicknessRoot, ctrlThicknessLimits.x, ctrlThicknessLimits.y);
                ctrlThicknessTip = Mathf.Clamp (ctrlThicknessTip, ctrlThicknessLimits.x, ctrlThicknessLimits.y);
                ctrlOffsetRoot = Mathf.Clamp (ctrlOffsetRoot, ctrlOffsetLimits.x, ctrlOffsetLimits.y);
                ctrlOffsetTip = Mathf.Clamp (ctrlOffsetTip, ctrlOffsetLimits.x, ctrlOffsetLimits.y);
            }
        }

        private void SetupFields ()
        {
            if (!isCtrlSrf)
            {
                SelectNextEdgeGeneric (wingEdgeTypeTrailing, "Trailing", "T", false);
                SelectNextEdgeGeneric (wingEdgeTypeLeading, "Leading", "L", false);

                SelectNextSurfaceGeneric (wingSurfaceTextureTop, "Top", "T", false);
                SelectNextSurfaceGeneric (wingSurfaceTextureBottom, "Bottom", "B", false);

                SetFieldVisibility ("ctrlSpan", false);
                SetFieldVisibility ("ctrlWidthRoot", false);
                SetFieldVisibility ("ctrlWidthTip", false);
                SetFieldVisibility ("ctrlThicknessRoot", false);
                SetFieldVisibility ("ctrlThicknessTip", false);
                SetFieldVisibility ("ctrlOffsetRoot", false);
                SetFieldVisibility ("ctrlOffsetTip", false);
            }
            else
            {
                SetFieldVisibility ("wingSpan", false);
                SetFieldVisibility ("wingWidthRoot", false);
                SetFieldVisibility ("wingWidthTip", false);
                SetFieldVisibility ("wingThicknessRoot", false);
                SetFieldVisibility ("wingThicknessTip", false);
                SetFieldVisibility ("wingOffset", false);
                SetFieldVisibility ("wingEdgeTrailing", false);
                SetFieldVisibility ("wingEdgeLeading", false);
                SetFieldVisibility ("syncWidth", false);
                SetFieldVisibility ("syncThickness", false);
                SetFieldVisibility ("syncEdge", false);

                SetEventVisibility ("SelectNextEdgeTrailing", false);
                SetEventVisibility ("SelectNextEdgeLeading", false);
                SetEventVisibility ("SelectNextSurfaceBottom", false);
                SetEventVisibility ("SelectNextSurfaceTop", false);
            }
        }

        private void SetFieldVisibility (string name, bool visible)
        {
            BaseField field = Fields[name];
            field.uiControlEditor.controlEnabled = false;
            field.uiControlFlight.controlEnabled = false;
            field.guiActiveEditor = false;
            field.guiActive = false;
        }

        private void SetEventVisibility (string name, bool visible)
        {
            BaseEvent basefield = Events[name];
            basefield.guiActiveEditor = false;
            basefield.guiActive = false;
        }

        public void SetupMeshReferences ()
        {
            Debug.Log ("WP | SetupMeshReferences");
            if (this.part.parent != null && this.part.parent.Modules.Contains ("WingProcedural") && !isCtrlSrf)
            {
                Debug.Log ("WP | CheckAndRepairMeshReferences | Parent module found, using the references");
                var source = this.part.parent.Modules.OfType<WingProcedural> ().FirstOrDefault ();
                SetupMeshReferencesFromSource (source);
            }
            else if (this.part.symmetryCounterparts.Count > 0)
            {
                Debug.Log ("WP | CheckAndRepairMeshReferences | Symmetry counterparts found, using the references");
                var source = this.part.symmetryCounterparts[0].Modules.OfType<WingProcedural> ().FirstOrDefault ();
                if (source != null)
                {
                    if (CheckMeshReferenceAvailability (source)) SetupMeshReferencesFromSource (source);
                    else SetupMeshReferencesFromScratch ();
                }
                else SetupMeshReferencesFromScratch ();
            }
            else SetupMeshReferencesFromScratch ();
        }

        public void ReportOnMeshReferences ()
        {
            if (isCtrlSrf)
            {
                Debug.Log
                (
                    "WP | UpdateOnEditorAttach | Control surface reference length check"
                    + " | Edge: " + meshReferenceCtrlEdge.vp.Length
                    + " | Top: " + meshReferenceCtrlSurfaceTop.vp.Length
                    + " | Bottom: " + meshReferenceCtrlSurfaceBottom.vp.Length
                );
            }
            else
            {
                Debug.Log
                (
                    "WP | UpdateOnEditorAttach | Wing reference length check"
                    + " | Section: " + meshReferenceWingSection.vp.Length
                    + " | Top: " + meshReferenceWingSurfaceTop.vp.Length
                    + " | Bottom: " + meshReferenceWingSurfaceBottom.vp.Length
                    + " | Edge A: " + meshReferenceWingEdgeA.vp.Length
                    + " | Edge B: " + meshReferenceWingEdgeB.vp.Length
                    + " | Edge C: " + meshReferenceWingEdgeC.vp.Length
                );
            }
        }

        private bool CheckMeshReferenceAvailability (WingProcedural source)
        {
            bool valid = true;
            if (isCtrlSrf)
            {
                if (source.isCtrlSrf)
                {
                    if (source.meshReferenceCtrlEdge == null) valid = false;
                    if (source.meshReferenceCtrlSurfaceTop == null) valid = false;
                    if (source.meshReferenceCtrlSurfaceBottom == null) valid = false;
                }
                else valid = false;
            }
            else
            {
                if (!source.isCtrlSrf)
                {
                    if (source.meshReferenceWingSection == null) valid = false;
                    if (source.meshReferenceWingSurfaceTop == null) valid = false;
                    if (source.meshReferenceWingSurfaceBottom == null) valid = false;
                    if (source.meshReferenceWingEdgeA == null) valid = false;
                    if (source.meshReferenceWingEdgeB == null) valid = false;
                    if (source.meshReferenceWingEdgeC == null) valid = false;
                }
                else valid = false;
            }
            return valid;
        }

        private void SetupMeshReferencesFromSource (WingProcedural source)
        {
            if (source != null)
            {
                if (isCtrlSrf)
                {
                    if (meshReferenceCtrlEdge == null && source.meshReferenceCtrlEdge != null) meshReferenceCtrlEdge = source.meshReferenceCtrlEdge; 
                    if (meshReferenceCtrlSurfaceTop == null && source.meshReferenceCtrlSurfaceTop != null) meshReferenceCtrlSurfaceTop = source.meshReferenceCtrlSurfaceTop;
                    if (meshReferenceCtrlSurfaceBottom == null && source.meshReferenceCtrlSurfaceBottom != null) meshReferenceCtrlSurfaceBottom = source.meshReferenceCtrlSurfaceBottom;
                }
                else
                {
                    if (meshReferenceWingSection == null && source.meshReferenceWingSection != null) meshReferenceWingSection = source.meshReferenceWingSection;
                    if (meshReferenceWingSurfaceTop == null && source.meshReferenceWingSurfaceTop != null) meshReferenceWingSurfaceTop = source.meshReferenceWingSurfaceTop;
                    if (meshReferenceWingSurfaceBottom == null && source.meshReferenceWingSurfaceBottom != null) meshReferenceWingSurfaceBottom = source.meshReferenceWingSurfaceBottom;
                    if (meshReferenceWingEdgeA == null && source.meshReferenceWingEdgeA != null) meshReferenceWingEdgeA = source.meshReferenceWingEdgeA;
                    if (meshReferenceWingEdgeB == null && source.meshReferenceWingEdgeB != null) meshReferenceWingEdgeB = source.meshReferenceWingEdgeB;
                    if (meshReferenceWingEdgeC == null && source.meshReferenceWingEdgeC != null) meshReferenceWingEdgeC = source.meshReferenceWingEdgeC;
                }
            }
        }

        private void SetupMeshReferencesFromScratch ()
        {
            Debug.Log ("WP | SetupMeshReferencesFromScratch | No sources found, creating new references");
            if (isCtrlSrf)
            {
                meshReferenceCtrlEdge = FillMeshRefererence (meshFilterCtrlEdgeReference);
                meshReferenceCtrlSurfaceTop = FillMeshRefererence (meshFilterCtrlSurfaceTop);
                meshReferenceCtrlSurfaceBottom = FillMeshRefererence (meshFilterCtrlSurfaceBottom);
            }
            else
            {
                meshReferenceWingSection = FillMeshRefererence (meshFilterWingSection);
                meshReferenceWingSurfaceTop = FillMeshRefererence (meshFilterWingSurfaceTop);
                meshReferenceWingSurfaceBottom = FillMeshRefererence (meshFilterWingSurfaceBottom);
                meshReferenceWingEdgeA = FillMeshRefererence (meshFilterWingEdgeA);
                meshReferenceWingEdgeB = FillMeshRefererence (meshFilterWingEdgeB);
                meshReferenceWingEdgeC = FillMeshRefererence (meshFilterWingEdgeC);
            }
        }

        private void SetupTemporaryCollider ()
        {
            temporaryCollider = CheckTransform ("proxy_collision_temporary");
            if (temporaryCollider != null) temporaryCollider.gameObject.SetActive (false);
        }



        // KSPEvents / Wing surfaces

        [KSPEvent (guiActiveEditor = true, guiName = "Side T: Medium | Next")]
        public void SelectNextSurfaceTop ()
        {
            if (wingSurfaceTextureTop == 0) wingSurfaceTextureTop = 1;
            else if (wingSurfaceTextureTop == 1) wingSurfaceTextureTop = 2;
            else wingSurfaceTextureTop = 0;
            SelectNextSurfaceGeneric (wingSurfaceTextureTop, "Top", "T", true);
        }

        [KSPEvent (guiActiveEditor = true, guiName = "Side B: Medium | Next")]
        public void SelectNextSurfaceBottom ()
        {
            if (wingSurfaceTextureBottom == 0) wingSurfaceTextureBottom = 1;
            else if (wingSurfaceTextureBottom == 1) wingSurfaceTextureBottom = 2;
            else wingSurfaceTextureBottom = 0;
            SelectNextSurfaceGeneric (wingSurfaceTextureBottom, "Bottom", "B", true);
        }

        private void SelectNextSurfaceGeneric (int target, string side, string letter, bool forceUpdates)
        {
            if (target == 0) Events["SelectNextSurface" + side].guiName = "Side " + letter + ": Light  | Next";
            else if (target == 1) Events["SelectNextSurface" + side].guiName = "Side " + letter + ": Medium | Next";
            else Events["SelectNextSurface" + side].guiName = "Side " + letter + ": Heavy  | Next";

            if (forceUpdates)
            {
                Debug.Log ("WP | Changing " + side + " surface to " + target);
                if (myWindow != null) myWindow.displayDirty = true;
            }
        }




        // KSPEvents / Wing edges


        [KSPEvent (guiActiveEditor = true, guiName = "Edge L: Round  | Next")]
        public void SelectNextEdgeLeading ()
        {
            if (wingEdgeTypeLeading == 0) wingEdgeTypeLeading = 1;
            else if (wingEdgeTypeLeading == 1) wingEdgeTypeLeading = 2;
            else wingEdgeTypeLeading = 0;
            SelectNextEdgeGeneric (wingEdgeTypeLeading, "Leading", "L", true);
        }

        [KSPEvent (guiActiveEditor = true, guiName = "Edge T: Round  | Next")]
        public void SelectNextEdgeTrailing ()
        {
            if (wingEdgeTypeTrailing == 0) wingEdgeTypeTrailing = 1;
            else if (wingEdgeTypeTrailing == 1) wingEdgeTypeTrailing = 2;
            else wingEdgeTypeTrailing = 0;
            SelectNextEdgeGeneric (wingEdgeTypeTrailing, "Trailing", "T", true);
        }

        private void SelectNextEdgeGeneric (int target, string side, string letter, bool forceUpdates)
        {
            if (target == 0) Events["SelectNextEdge" + side].guiName = "Edge " + letter + ": Flat   | Next";
            else if (target == 1) Events["SelectNextEdge" + side].guiName = "Edge " + letter + ": Round  | Next";
            else Events["SelectNextEdge" + side].guiName = "Edge " + letter + ": Sharp  | Next";

            if (forceUpdates)
            {
                Debug.Log ("WP | Changing " + side + " edge to " + target);
                if (myWindow != null) myWindow.displayDirty = true;
            }
        }




        // Supposed to fix context menu updates
        // Proposed by NathanKell, if I'm not mistaken

        UIPartActionWindow _myWindow = null;
        UIPartActionWindow myWindow
        {
            get
            {
                if (_myWindow == null)
                {
                    foreach (UIPartActionWindow window in FindObjectsOfType (typeof (UIPartActionWindow)))
                    {
                        if (window.part == part) _myWindow = window;
                    }
                }
                return _myWindow;
            }
        }

        private void UpdateWindow ()
        {
            if (myWindow != null) myWindow.displayDirty = true;
        }




        // Reference fetching

        private MeshFilter CheckMeshFilter (MeshFilter reference, string name) { return CheckMeshFilter (reference, name, false); }
        private MeshFilter CheckMeshFilter (MeshFilter reference, string name, bool disable)
        {
            if (reference == null)
            {
                Transform parent = part.transform.GetChild (0).GetChild (0).GetChild (0).Find (name);
                if (parent != null)
                {
                    reference = parent.gameObject.GetComponent<MeshFilter> ();
                    if (disable) parent.gameObject.SetActive (false);
                }
            }
            return reference;
        }

        private Transform CheckTransform (string name)
        {
            Transform t = part.transform.GetChild (0).GetChild (0).GetChild (0).Find (name);
            return t;
        }

        private Texture CheckTexture (Texture reference, MeshFilter source)
        {
            if (source != null && reference == null)
            {
                Renderer r = source.gameObject.GetComponent<Renderer> ();
                if (r != null) reference = r.material.GetTexture ("_MainTex");
            }
            return reference;
        }

        private MeshReference FillMeshRefererence (MeshFilter source)
        {
            MeshReference reference = new MeshReference ();
            int length = source.mesh.vertices.Length;
            reference.vp = new Vector3[length];
            Array.Copy (source.mesh.vertices, reference.vp, length);
            reference.nm = new Vector3[length];
            Array.Copy (source.mesh.normals, reference.nm, length);
            reference.uv = new Vector2[length];
            Array.Copy (source.mesh.uv, reference.uv, length);
            return reference;
        }
    }
}
