using KSP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using KSPAPIExtensions;

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


        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Semispan", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float wingSpan = 4f;
        public float wingSpanCached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Width (root)", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float wingWidthRoot = 4f;
        public float wingWidthRootCached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Width (tip)", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float wingWidthTip = 4f;
        public float wingWidthTipCached = 4f;

        // [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Sync width"),
        // UI_Toggle (disabledText = "Off", enabledText = "On", scene = UI_Scene.Editor)]
        public bool syncWidth = false;
        public bool syncWidthCached = false;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Offset", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = -8f, maxValue = 8f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float wingOffset = 0f;
        public float wingOffsetCached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Height (root)"),
        UI_FloatRange (minValue = 0.08f, maxValue = 0.48f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float wingThicknessRoot = 0.24f;
        public float wingThicknessRootCached = 0.24f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Height (tip)"),
        UI_FloatRange (minValue = 0.08f, maxValue = 0.48f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float wingThicknessTip = 0.24f;
        public float wingThicknessTipCached = 0.24f;

        // [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Sync height"),
        // UI_Toggle (disabledText = "Off", enabledText = "On", scene = UI_Scene.Editor)]
        public bool syncThickness = false;
        public bool syncThicknessCached = false;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Side A (type)"),
        UI_FloatRange (minValue = 1f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float wingSurfaceTextureTop = 3f;
        public float wingSurfaceTextureTopCached = 3f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Side B (type)"),
        UI_FloatRange (minValue = 1f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float wingSurfaceTextureBottom = 4f;
        public float wingSurfaceTextureBottomCached = 4f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Edge L (type)"),
        UI_FloatRange (minValue = 1f, maxValue = 3f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float wingEdgeTypeLeading = 2f;
        public float wingEdgeTypeLeadingCached = 2f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Edge T (type)"),
        UI_FloatRange (minValue = 1f, maxValue = 3f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float wingEdgeTypeTrailing = 3f;
        public float wingEdgeTypeTrailingCached = 3f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Edge L (material)"),
        UI_FloatRange (minValue = 1f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float wingEdgeTextureLeading = 4f;
        public float wingEdgeTextureLeadingCached = 4f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Edge T (material)"),
        UI_FloatRange (minValue = 1f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float wingEdgeTextureTrailing = 4f;
        public float wingEdgeTextureTrailingCached = 4f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Edge L (size)"),
        UI_FloatRange (minValue = 0f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.125f)]
        public float wingEdgeScaleLeading = 1f;
        public float wingEdgeScaleLeadingCached = 1f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Edge T (size)"),
        UI_FloatRange (minValue = 0f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.125f)]
        public float wingEdgeScaleTrailing = 1f;
        public float wingEdgeScaleTrailingCached = 1f;

        // [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Sync edges"),
        // UI_Toggle (disabledText = "Off", enabledText = "On", scene = UI_Scene.Editor)]
        public bool syncEdge = false;
        public bool syncEdgeCached = false;

        public MeshFilter meshFilterWingSection;
        public MeshFilter meshFilterWingSurfaceTop;
        public MeshFilter meshFilterWingSurfaceBottom;
        public MeshFilter meshFilterWingEdgeTrailing;
        public MeshFilter meshFilterWingEdgeLeading;
        public MeshFilter meshFilterWingEdgeA;
        public MeshFilter meshFilterWingEdgeB;
        public MeshFilter meshFilterWingEdgeC;

        public static MeshReference meshReferenceWingSection;
        public static MeshReference meshReferenceWingSurfaceTop;
        public static MeshReference meshReferenceWingSurfaceBottom;
        public static MeshReference meshReferenceWingEdgeA;
        public static MeshReference meshReferenceWingEdgeB;
        public static MeshReference meshReferenceWingEdgeC;

        public Vector2 wingSpanLimits = new Vector2 (0.25f, 16f);
        public Vector2 wingWidthLimits = new Vector2 (0.25f, 16f);
        public Vector2 wingThicknessLimits = new Vector2 (0.08f, 0.48f);
        public Vector2 wingOffsetLimits = new Vector2 (-8f, 8f);
        public Vector2 wingEdgeScaleLimits = new Vector2 (0f, 1f);




        // Prerequisites / Control surfaces

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Length", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 8f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float ctrlSpan = 1f;
        public float ctrlSpanCached = 1f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width R"),
        UI_FloatRange (minValue = 0.25f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.25f)]
        public float ctrlWidthRoot = 0.25f;
        public float ctrlWidthRootCached = 0.25f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width T"),
        UI_FloatRange (minValue = 0.25f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.25f)]
        public float ctrlWidthTip = 0.25f;
        public float ctrlWidthTipCached = 0.25f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Height R"),
        UI_FloatRange (minValue = 0.08f, maxValue = 0.48f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float ctrlThicknessRoot = 0.24f;
        public float ctrlThicknessRootCached = 0.24f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Height T"),
        UI_FloatRange (minValue = 0.08f, maxValue = 0.48f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float ctrlThicknessTip = 0.24f;
        public float ctrlThicknessTipCached = 0.24f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Offset R"),
        UI_FloatRange (minValue = -1f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.125f)]
        public float ctrlOffsetRoot = 0.0f; 
        public float ctrlOffsetRootCached = 0.0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Offset T"),
        UI_FloatRange (minValue = -1f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.125f)]
        public float ctrlOffsetTip = 0.0f;
        public float ctrlOffsetTipCached = 0.0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Material A"),
        UI_FloatRange (minValue = 1f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float ctrlSurfaceTextureTop = 1f;
        public float ctrlSurfaceTextureTopCached;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Material B"),
        UI_FloatRange (minValue = 1f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float ctrlSurfaceTextureBottom = 4f;
        public float ctrlSurfaceTextureBottomCached;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Material C"),
        UI_FloatRange (minValue = 1f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float ctrlEdgeTexture = 4f;
        public float ctrlEdgeTextureCached = 4f;

        public MeshFilter meshFilterCtrlEdge;
        public MeshFilter meshFilterCtrlEdgeReference;
        public MeshFilter meshFilterCtrlSurfaceTop;
        public MeshFilter meshFilterCtrlSurfaceBottom;

        public static MeshReference meshReferenceCtrlEdge;
        public static MeshReference meshReferenceCtrlSurfaceTop;
        public static MeshReference meshReferenceCtrlSurfaceBottom;

        public Vector2 ctrlSpanLimits = new Vector2 (0.25f, 8f);
        public Vector2 ctrlWidthLimits = new Vector2 (0.25f, 1f);
        public Vector2 ctrlThicknessLimits = new Vector2 (0.08f, 0.48f);
        public Vector2 ctrlOffsetLimits = new Vector2 (-1f, 1f);

        public Transform temporaryCollider;




        // Some handy bools

        [KSPField (isPersistant = true)]
        public bool isAttached = false;

        public bool isStarted = false;
        public bool isStartingNow = false;
        public bool justDetached = false;
        public bool logUpdate = true;
        public bool logUpdateGeometry = false;




        // Debug

        private float debugTimer = 0f;

        private void DebugTimerUpdate ()
        {
            debugTimer += Time.deltaTime;
            if (debugTimer > 1000f) debugTimer = 0f;
        }

        private void DebugLogWithID (string method, string message)
        {
            Debug.Log ("WP | ID: " + part.gameObject.GetInstanceID () + " | T: " + debugTimer.ToString ("F1") + " | " + method + " | " + message);
        }



        // Update
        // As nothing like GUI.changed or per-KSPFields change delegates is available, we have to resort to dirty comparisons
        // Performance hit shouldn't be too bad as all KSPFields have snapped sliders that can't spam dozens of values per second

        public  bool updateCounterpartsAllowed = true;
        private bool updateRequiredOnMeshes = false;
        private bool updateRequiredOnGeometry = false;
        private bool updateRequiredOnWindow = false;
        private bool updateCounterparts = false;

        public void Update ()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (CachedOnEditorAttach == null) CachedOnEditorAttach = new Callback (UpdateOnEditorAttach);
                if (!this.part.OnEditorAttach.GetInvocationList ().Contains (CachedOnEditorAttach)) this.part.OnEditorAttach += CachedOnEditorAttach;

                if (CachedOnEditorDetach == null) CachedOnEditorDetach = new Callback (UpdateOnEditorDetach);
                if (!this.part.OnEditorDetach.GetInvocationList ().Contains (CachedOnEditorDetach)) this.part.OnEditorDetach += CachedOnEditorDetach;

                DebugTimerUpdate ();

                if (isStarted)
                {

                    // Next, compare the properties to cached values
                    // If there is a mismatch, then update is required

                    if (!isCtrlSrf)
                    {
                        if (wingSpan != wingSpanCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingSpan");
                            updateRequiredOnGeometry = true;
                            wingSpanCached = wingSpan;
                        }
                        if (wingWidthRoot != wingWidthRootCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingWidthRoot");
                            updateRequiredOnGeometry = true;
                            wingWidthRootCached = wingWidthRoot;
                            if (syncWidth) wingWidthTip = wingWidthTipCached = wingWidthRoot;
                        }
                        if (wingWidthTip != wingWidthTipCached && !syncWidth)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingWidthTip");
                            updateRequiredOnGeometry = true;
                            wingWidthTipCached = wingWidthTip;
                        }
                        if (wingThicknessRoot != wingThicknessRootCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingThicknessRoot");
                            updateRequiredOnGeometry = true;
                            wingThicknessRootCached = wingThicknessRoot;
                            if (syncThickness) wingThicknessTip = wingThicknessTipCached = wingThicknessRoot;
                        }
                        if (wingThicknessTip != wingThicknessTipCached && !syncThickness)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingThicknessTip");
                            updateRequiredOnGeometry = true;
                            wingThicknessTipCached = wingThicknessTip;
                        }
                        if (wingOffset != wingOffsetCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingOffset");
                            updateRequiredOnGeometry = true;
                            wingOffsetCached = wingOffset;
                        }
                        if (wingSurfaceTextureTop != wingSurfaceTextureTopCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal scawingSurfaceTextureTop");
                            updateRequiredOnGeometry = true;
                            wingSurfaceTextureTopCached = wingSurfaceTextureTop;
                        }
                        if (wingSurfaceTextureBottom != wingSurfaceTextureBottomCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingSurfaceTextureBottom");
                            updateRequiredOnGeometry = true;
                            wingSurfaceTextureBottomCached = wingSurfaceTextureBottom;
                        }
                        if (wingEdgeTypeTrailing != wingEdgeTypeTrailingCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingEdgeTypeTrailing");
                            updateRequiredOnMeshes = updateRequiredOnGeometry = true;
                            wingEdgeTypeTrailingCached = wingEdgeTypeTrailing;
                            if (syncEdge) wingEdgeTypeLeading = wingEdgeTypeLeadingCached = wingEdgeTypeTrailing;
                        }
                        if (wingEdgeTypeLeading != wingEdgeTypeLeadingCached && !syncEdge)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingEdgeTypeLeading");
                            updateRequiredOnMeshes = updateRequiredOnGeometry = true;
                            wingEdgeTypeLeadingCached = wingEdgeTypeLeading;
                        }
                        if (wingEdgeScaleTrailing != wingEdgeScaleTrailingCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingEdgeScaleTrailing");
                            updateRequiredOnGeometry = true;
                            wingEdgeScaleTrailingCached = wingEdgeScaleTrailing;
                            if (syncEdge) wingEdgeScaleLeading = wingEdgeScaleLeadingCached = wingEdgeScaleTrailing;
                        }
                        if (wingEdgeScaleLeading != wingEdgeScaleLeadingCached && !syncEdge)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingEdgeScaleLeading");
                            updateRequiredOnGeometry = true;
                            wingEdgeScaleLeadingCached = wingEdgeScaleLeading;
                        }
                        if (wingEdgeTextureTrailing != wingEdgeTextureTrailingCached && !syncEdge)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingEdgeTextureTrailing");
                            updateRequiredOnGeometry = true;
                            wingEdgeTextureTrailingCached = wingEdgeTextureTrailing;
                        }
                        if (wingEdgeTextureLeading != wingEdgeTextureLeadingCached && !syncEdge)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal wingEdgeLeading");
                            updateRequiredOnGeometry = true;
                            wingEdgeTextureLeadingCached = wingEdgeTextureLeading;
                        }
                        if (syncEdge != syncEdgeCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal syncEdge");
                            updateRequiredOnGeometry = updateRequiredOnMeshes = updateRequiredOnWindow = true;
                            SetFieldVisibility ("wingEdgeTypeLeading", !syncEdge);
                            SetFieldVisibility ("wingEdgeScaleLeading", !syncEdge);
                            if (syncEdge)
                            {
                                wingEdgeScaleLeading = wingEdgeScaleLeadingCached = wingEdgeScaleTrailing;
                                wingEdgeTypeLeading = wingEdgeTypeLeadingCached = wingEdgeTypeTrailing;
                            }
                            syncEdgeCached = syncEdge;
                        }
                        if (syncThickness != syncThicknessCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal syncThickness");
                            updateRequiredOnGeometry = updateRequiredOnWindow = true;
                            SetFieldVisibility ("wingThicknessTip", !syncThickness);
                            if (syncThickness) wingThicknessTip = wingThicknessTip = wingThicknessRoot;
                            syncThicknessCached = syncThickness;
                        }
                        if (syncWidth != syncWidthCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal syncWidth");
                            updateRequiredOnGeometry = updateRequiredOnWindow = true;
                            SetFieldVisibility ("wingWidthTip", !syncWidth);
                            if (syncWidth) wingWidthTip = wingWidthTipCached = wingWidthRoot;
                            syncWidthCached = syncWidth;
                        }
                    }
                    else
                    {
                        if (ctrlSpan != ctrlSpanCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal ctrlSpan");
                            updateRequiredOnGeometry = true;
                            ctrlSpanCached = ctrlSpan;
                        }
                        if (ctrlWidthRoot != ctrlWidthRootCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal ctrlWidthRoot");
                            updateRequiredOnGeometry = true;
                            ctrlWidthRootCached = ctrlWidthRoot;
                        }
                        if (ctrlWidthTip != ctrlWidthTipCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal ctrlWidthTip");
                            updateRequiredOnGeometry = true;
                            ctrlWidthTipCached = ctrlWidthTip;
                        }
                        if (ctrlThicknessRoot != ctrlThicknessRootCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal ctrlThicknessRoot");
                            updateRequiredOnGeometry = true;
                            ctrlThicknessRootCached = ctrlThicknessRoot;
                        }
                        if (ctrlThicknessTip != ctrlThicknessTipCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal ctrlThicknessTip");
                            updateRequiredOnGeometry = true;
                            ctrlThicknessTipCached = ctrlThicknessTip;
                        }
                        if (ctrlOffsetRoot != ctrlOffsetRootCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal ctrlOffsetRoot");
                            updateRequiredOnGeometry = true;
                            ctrlOffsetRootCached = ctrlOffsetRoot;
                        }
                        if (ctrlOffsetTip != ctrlOffsetTipCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal ctrlOffsetRoot");
                            updateRequiredOnGeometry = true;
                            ctrlOffsetTipCached = ctrlOffsetTip;
                        }
                        if (ctrlSurfaceTextureTop != ctrlSurfaceTextureTopCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal ctrlSurfaceTop");
                            updateRequiredOnGeometry = true;
                            ctrlSurfaceTextureTopCached = ctrlSurfaceTextureTop;
                        }
                        if (ctrlSurfaceTextureBottom != ctrlSurfaceTextureBottomCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal ctrlSurfaceBottom");
                            updateRequiredOnGeometry = true;
                            ctrlSurfaceTextureBottomCached = ctrlSurfaceTextureBottom;
                        }
                        if (ctrlEdgeTexture != ctrlEdgeTextureCached)
                        {
                            if (logUpdate) DebugLogWithID ("Update", "Non-equal ctrlEdgeTexture");
                            updateRequiredOnGeometry = true;
                            ctrlEdgeTextureCached = ctrlEdgeTexture;
                        }
                    }

                    // Trigger update of the counterparts
                    // Has to be done through a special method that overrides their cached values, preventing feedback loop
                    // Also, a somewhat strange check for attachment after detachment, seems to help in a certain case

                    if (updateRequiredOnMeshes || updateRequiredOnGeometry) updateCounterparts = true;
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
                        if (logUpdate) DebugLogWithID ("Update", "Update required on meshes");
                        updateRequiredOnMeshes = false;
                        UpdateMeshes ();
                    }
                    if (updateRequiredOnGeometry)
                    {
                        if (logUpdate) DebugLogWithID ("Update", "Update required on geometry");
                        updateRequiredOnGeometry = false;
                        UpdateGeometry ();
                    }
                    if (updateCounterparts && updateCounterpartsAllowed)
                    {
                        if (logUpdate) DebugLogWithID ("Update", "Update required on counterparts");
                        updateCounterparts = false;
                        UpdateCounterparts ();
                    }
                    if (updateRequiredOnWindow)
                    {
                        updateRequiredOnWindow = false;
                        UpdateWindow ();
                    }
                }
                else if (isAttached && !isStartingNow)
                {
                    DebugLogWithID ("Update", "Setup started in absense of attachment event");
                    Setup ();
                    isStarted = true;
                }
            }
            else
            {
                if (isAttached && !isStarted)
                {
                    DebugLogWithID ("Update", "Setup started on flight");
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
            DebugLogWithID ("UpdateOnEditorAttach", "Setup started");
            Setup ();
            isStarted = true;
            DebugLogWithID ("UpdateOnEditorAttach", "Setup ended");
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

        public void UpdateGeometry ()
        {
            DebugLogWithID ("UpdateGeometry", "Started | isCtrlSrf: " + isCtrlSrf);
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
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing section | Passed array setup");

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
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing section | Finished");
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
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing surface top | Passed array setup");

                    vp[0] = new Vector3 (-wingSpan, vp[0].y * wingThicknessDeviationTip, wingWidthTip / 2f + wingOffset);
                    uv[0] = new Vector2 (wingSpan / 4f, 0f + 0.5f - wingWidthTip / 8f - wingOffset / 4f);

                    vp[1] = new Vector3 (0f, vp[1].y * wingThicknessDeviationRoot, wingWidthRoot / 2f);
                    uv[1] = new Vector2 (0f, 0f + 0.5f - wingWidthRoot / 8f);

                    vp[2] = new Vector3 (0f, vp[2].y * wingThicknessDeviationRoot, -wingWidthRoot / 2f);
                    uv[2] = new Vector2 (0.0f, 1f - 0.5f + wingWidthRoot / 8f);

                    vp[3] = new Vector3 (-wingSpan, vp[3].y * wingThicknessDeviationTip, -wingWidthTip / 2f + wingOffset);
                    uv[3] = new Vector2 (wingSpan / 4f, 1f - 0.5f + wingWidthTip / 8f - wingOffset / 4f);

                    Color[] cl = new Color[length];
                    for (int i = 0; i < length; ++i) cl[i] = new Color (0f, 0f, 0f, GetMaterialVertexAlpha (wingSurfaceTextureTop));

                    meshFilterWingSurfaceTop.mesh.vertices = vp;
                    meshFilterWingSurfaceTop.mesh.uv = uv;
                    meshFilterWingSurfaceTop.mesh.colors = cl;
                    meshFilterWingSurfaceTop.mesh.RecalculateBounds ();
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing surface top | Finished");
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
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing surface bottom | Passed array setup");

                    vp[0] = new Vector3 (-wingSpan, vp[0].y * wingThicknessDeviationTip, wingWidthTip / 2f - wingOffset);
                    uv[0] = new Vector2 (wingSpan / 4f, 0f + 0.5f - wingWidthTip / 8f + wingOffset / 4f);

                    vp[1] = new Vector3 (0f, vp[1].y * wingThicknessDeviationRoot, wingWidthRoot / 2f);
                    uv[1] = new Vector2 (0f, 0f + 0.5f - wingWidthRoot / 8f);

                    vp[2] = new Vector3 (0f, vp[2].y * wingThicknessDeviationRoot, -wingWidthRoot / 2f);
                    uv[2] = new Vector2 (0f, 1f - 0.5f + wingWidthRoot / 8f);

                    vp[3] = new Vector3 (-wingSpan, vp[3].y * wingThicknessDeviationTip, -wingWidthTip / 2f - wingOffset);
                    uv[3] = new Vector2 (wingSpan / 4f, 1f - 0.5f + wingWidthTip / 8f + wingOffset / 4f);

                    Color[] cl = new Color[length];
                    for (int i = 0; i < length; ++i) cl[i] = new Color (0f, 0f, 0f, GetMaterialVertexAlpha (wingSurfaceTextureBottom));

                    meshFilterWingSurfaceBottom.mesh.vertices = vp;
                    meshFilterWingSurfaceBottom.mesh.uv = uv;
                    meshFilterWingSurfaceBottom.mesh.colors = cl;
                    meshFilterWingSurfaceBottom.mesh.RecalculateBounds ();
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing surface bottom | Finished");
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
                        Color[] cl = new Color[length];
                        if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge trailing | Passed array setup");

                        for (int i = 0; i < vp.Length; ++i)
                        {
                            if (nm[i].x == -1.0f) vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, Mathf.Lerp (2f, vp[i].z, wingEdgeScaleTrailing) - 2f + wingWidthTip / 2f + wingOffset); // Tip section
                            else if (nm[i].x == 1.0f) vp[i] = new Vector3 (0f, vp[i].y * wingThicknessDeviationRoot, Mathf.Lerp (2f, vp[i].z, wingEdgeScaleTrailing) - 2f + wingWidthRoot / 2f); // Root section
                            else
                            {
                                if (vp[i].x < -0.1f)
                                {
                                    vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, Mathf.Lerp (2f, vp[i].z, wingEdgeScaleTrailing) - 2f + wingWidthTip / 2f + wingOffset); // Tip edge
                                    uv[i] = new Vector2 (wingSpan, uv[i].y);
                                }
                                else vp[i] = new Vector3 (0f, vp[i].y * wingThicknessDeviationRoot, Mathf.Lerp (2f, vp[i].z, wingEdgeScaleTrailing) - 2f + wingWidthRoot / 2f); // Root edge
                            }

                            // Colors
                            cl[i] = new Color (0f, 0f, 0f, GetMaterialVertexAlpha (wingEdgeTextureTrailing));
                        }

                        meshFilterWingEdgeTrailing.mesh.vertices = vp;
                        meshFilterWingEdgeTrailing.mesh.uv = uv;
                        meshFilterWingEdgeTrailing.mesh.colors = cl;
                        meshFilterWingEdgeTrailing.mesh.RecalculateBounds ();
                        if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge trailing | Finished");
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
                        Color[] cl = new Color[length];
                        if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge leading | Passed array setup");

                        for (int i = 0; i < vp.Length; ++i)
                        {
                            if (nm[i].x == -1.0f) vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, Mathf.Lerp (2f, vp[i].z, wingEdgeScaleLeading) - 2f + wingWidthTip / 2f - wingOffset); // Tip section
                            else if (nm[i].x == 1.0f) vp[i] = new Vector3 (0f, vp[i].y * wingThicknessDeviationRoot, Mathf.Lerp (2f, vp[i].z, wingEdgeScaleLeading) - 2f + wingWidthRoot / 2f); // Root section
                            else
                            {
                                if (vp[i].x < -0.1f)
                                {
                                    vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, Mathf.Lerp (2f, vp[i].z, wingEdgeScaleLeading) - 2f + wingWidthTip / 2f - wingOffset); // Tip edge
                                    uv[i] = new Vector2 (wingSpan, uv[i].y);
                                }
                                else vp[i] = new Vector3 (0f, vp[i].y * wingThicknessDeviationRoot, Mathf.Lerp (2f, vp[i].z, wingEdgeScaleLeading) - 2f + wingWidthRoot / 2f); // Root edge
                            }

                            // Colors
                            cl[i] = new Color (0f, 0f, 0f, GetMaterialVertexAlpha (wingEdgeTextureLeading));
                        }

                        meshFilterWingEdgeLeading.mesh.vertices = vp;
                        meshFilterWingEdgeLeading.mesh.uv = uv;
                        meshFilterWingEdgeLeading.mesh.colors = cl;
                        meshFilterWingEdgeLeading.mesh.RecalculateBounds ();
                        if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge leading | Finished");
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
                    Color[] cl = new Color[length];
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface edge | Passed array setup");
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

                        // Colors
                        cl[i] = new Color (0f, 0f, 0f, GetMaterialVertexAlpha (ctrlEdgeTexture));
                    }

                    meshFilterCtrlEdge.mesh.vertices = vp;
                    meshFilterCtrlEdge.mesh.uv = uv;
                    meshFilterCtrlEdge.mesh.colors = cl;
                    meshFilterCtrlEdge.mesh.RecalculateBounds ();

                    MeshCollider meshCollider = meshFilterCtrlEdge.gameObject.GetComponent<MeshCollider> ();
                    if (meshCollider == null) meshCollider = meshFilterCtrlEdge.gameObject.AddComponent<MeshCollider> ();
                    meshCollider.sharedMesh = null;
                    meshCollider.sharedMesh = meshFilterCtrlEdge.mesh;
                    meshCollider.convex = true;
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface edge | Finished");
                }
                if (meshFilterCtrlSurfaceTop != null)
                {
                    int length = meshReferenceCtrlSurfaceTop.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReferenceCtrlSurfaceTop.vp, vp, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReferenceCtrlSurfaceTop.uv, uv, length);
                    Color[] cl = new Color[length];
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface top | Passed array setup");
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

                        // Colors
                        cl[i] = new Color (0f, 0f, 0f, GetMaterialVertexAlpha (ctrlSurfaceTextureTop));
                    }
                    meshFilterCtrlSurfaceTop.mesh.vertices = vp;
                    meshFilterCtrlSurfaceTop.mesh.uv = uv;
                    meshFilterCtrlSurfaceTop.mesh.colors = cl;
                    meshFilterCtrlSurfaceTop.mesh.RecalculateBounds ();
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface top | Finished");
                }
                if (meshFilterCtrlSurfaceBottom != null)
                {
                    int length = meshReferenceCtrlSurfaceBottom.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReferenceCtrlSurfaceBottom.vp, vp, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReferenceCtrlSurfaceBottom.uv, uv, length);
                    Color[] cl = new Color[length];
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface bottom | Passed array setup");
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

                        // Colors
                        cl[i] = new Color (0f, 0f, 0f, GetMaterialVertexAlpha (ctrlSurfaceTextureBottom));
                    }
                    meshFilterCtrlSurfaceBottom.mesh.vertices = vp;
                    meshFilterCtrlSurfaceBottom.mesh.uv = uv;
                    meshFilterCtrlSurfaceBottom.mesh.colors = cl;
                    meshFilterCtrlSurfaceBottom.mesh.RecalculateBounds ();
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface bottom | Finished");
                }
            }
            DebugLogWithID ("UpdateGeometry", "Finished");
            CalculateAerodynamicValues ();
        }




        // Edge geometry

        public MeshReference GetWingEdgeReference (float selection)
        {
            if (selection == 1f) return meshReferenceWingEdgeA;
            else if (selection == 2f) return meshReferenceWingEdgeB;
            else return meshReferenceWingEdgeC;
        }

        public MeshFilter GetWingEdgeFilter (float selection)
        {
            if (selection == 1f) return meshFilterWingEdgeA;
            else if (selection == 2f) return meshFilterWingEdgeB;
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
                    DebugLogWithID ("UpdateMeshes", "Wing trailing edge | Mesh present | Length: " + meshFilterWingEdgeTrailing.mesh.vertices.Length + " | Removing");
                    DestroyImmediate (meshFilterWingEdgeTrailing.mesh);
                }
                meshFilterWingEdgeTrailing.mesh = Instantiate (GetWingEdgeFilter (wingEdgeTypeTrailing).mesh) as Mesh;
                DebugLogWithID ("UpdateMeshes", "Wing trailing edge | Mesh created | Length: " + meshFilterWingEdgeTrailing.mesh.vertices.Length);

                if (meshFilterWingEdgeLeading.mesh != null)
                {
                    DebugLogWithID ("UpdateMeshes", "Wing leading edge | Mesh present | Length: " + meshFilterWingEdgeLeading.mesh.vertices.Length + " | Removing");
                    DestroyImmediate (meshFilterWingEdgeLeading.mesh);
                }
                meshFilterWingEdgeLeading.mesh = Instantiate (GetWingEdgeFilter (wingEdgeTypeLeading).mesh) as Mesh;
                DebugLogWithID ("UpdateMeshes", "Wing leading edge | Mesh created | Length: " + meshFilterWingEdgeLeading.mesh.vertices.Length);
            }
        }




        // Materials


        public static Material materialLayeredSurface;
        public static Texture materialLayeredSurfaceTexture;

        public static Material materialLayeredEdge;
        public static Texture materialLayeredEdgeTexture;

        private float materialPropertyShininess = 0.078125f;
        private Color materialPropertySpecular = new Color (0.5f, 0.5f, 0.5f, 1.0f);

        public void UpdateMaterials ()
        {
            if (materialLayeredSurface == null || materialLayeredEdge == null) SetMaterialReferences ();
            if (materialLayeredSurface != null)
            {
                if (!isCtrlSrf)
                {
                    SetMaterial (meshFilterWingSurfaceTop, materialLayeredSurface);
                    SetMaterial (meshFilterWingSurfaceBottom, materialLayeredSurface);
                    SetMaterial (meshFilterWingEdgeTrailing, materialLayeredEdge);
                    SetMaterial (meshFilterWingEdgeLeading, materialLayeredEdge);
                }
                else
                {
                    SetMaterial (meshFilterCtrlSurfaceTop, materialLayeredSurface);
                    SetMaterial (meshFilterCtrlSurfaceBottom, materialLayeredSurface);
                    SetMaterial (meshFilterCtrlEdge, materialLayeredEdge);
                }
            }
            else DebugLogWithID ("UpdateMaterials", "Material creation failed");
        }

        private void SetMaterialReferences ()
        {
            if (materialLayeredSurface == null) materialLayeredSurface = ResourceExtractor.GetEmbeddedMaterial ("B9_Aerospace_WingStuff.SpecularLayered.txt");
            if (materialLayeredEdge == null) materialLayeredEdge = ResourceExtractor.GetEmbeddedMaterial ("B9_Aerospace_WingStuff.SpecularLayered.txt");

            if (!isCtrlSrf) SetTextures (meshFilterWingSurfaceTop, meshFilterWingEdgeA);
            else SetTextures (meshFilterCtrlSurfaceTop, meshFilterCtrlEdge);

            if (materialLayeredSurfaceTexture != null)
            {
                materialLayeredSurface.SetTexture ("_MainTex", materialLayeredSurfaceTexture);
                materialLayeredSurface.SetFloat ("_Shininess", materialPropertyShininess);
                materialLayeredSurface.SetColor ("_SpecColor", materialPropertySpecular);
            }
            else DebugLogWithID ("SetMaterialReferences", "Surface texture not found");

            if (materialLayeredEdgeTexture != null)
            {
                materialLayeredEdge.SetTexture ("_MainTex", materialLayeredEdgeTexture);
                materialLayeredEdge.SetFloat ("_Shininess", materialPropertyShininess);
                materialLayeredEdge.SetColor ("_SpecColor", materialPropertySpecular);
            }
            else DebugLogWithID ("SetMaterialReferences", "Edge texture not found");
        }

        private void SetMaterial (MeshFilter target, Material material)
        {
            if (target != null)
            {
                Renderer r = target.gameObject.GetComponent<Renderer> ();
                if (r != null) r.sharedMaterial = material;
            }
        }

        private void SetTextures (MeshFilter sourceSurface, MeshFilter sourceEdge)
        {
            if (sourceSurface != null)
            {
                Renderer r = sourceSurface.gameObject.GetComponent<Renderer> ();
                if (r != null) materialLayeredSurfaceTexture = r.sharedMaterial.GetTexture ("_MainTex");
            }
            if (sourceEdge != null)
            {
                Renderer r = sourceEdge.gameObject.GetComponent<Renderer> ();
                if (r != null) materialLayeredEdgeTexture = r.sharedMaterial.GetTexture ("_MainTex");
            }
        }

        private float GetMaterialVertexAlpha (float selection)
        {
            float a = (selection - 1f) / 3f;
            return a;
        }




        // Setup

        public void Setup ()
        {
            DebugLogWithID ("Setup", "Started");
            isStartingNow = true;
            SetupMeshFilters ();
            SetupClamping ();
            SetupFields ();
            SetupSyncTemporary ();
            SetupMeshReferences ();
            SetupTemporaryCollider ();
            ReportOnMeshReferences ();
            SetupRecurring ();
            isStartingNow = false;
        }

        public void SetupRecurring ()
        {
            UpdateMeshes ();
            UpdateMaterials ();
            UpdateGeometry ();
            UpdateWindow ();
        }

        public void SetupSyncTemporary ()
        {
            syncEdge = syncEdgeCached = false;
            syncWidth = syncWidthCached = false;
            syncThickness = syncThicknessCached = false;
        }

        public void UpdateCounterparts ()
        {
            DebugLogWithID ("UpdateCounterparts", "Started");
            for (int i = 0; i < this.part.symmetryCounterparts.Count; ++i)
            {
                var clone = this.part.symmetryCounterparts[i].Modules.OfType<WingProcedural> ().FirstOrDefault ();
                clone.updateCounterpartsAllowed = false;
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
                    clone.wingEdgeScaleTrailing =       clone.wingEdgeScaleTrailingCached =     wingEdgeScaleTrailing;
                    clone.wingEdgeScaleLeading =        clone.wingEdgeScaleLeadingCached =      wingEdgeScaleLeading;
                    clone.wingEdgeTypeTrailing =        clone.wingEdgeTypeTrailingCached =      wingEdgeTypeTrailing;
                    clone.wingEdgeTypeLeading =         clone.wingEdgeTypeLeadingCached =       wingEdgeTypeLeading;
                    clone.wingSurfaceTextureTop =       clone.wingSurfaceTextureTopCached =     wingSurfaceTextureTop;
                    clone.wingSurfaceTextureBottom =    clone.wingSurfaceTextureBottomCached =  wingSurfaceTextureBottom;
                    clone.wingEdgeTextureLeading =      clone.wingEdgeTextureLeadingCached =    wingEdgeTextureLeading;
                    clone.wingEdgeTextureTrailing =     clone.wingEdgeTextureTrailingCached =   wingEdgeTextureTrailing;
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
                    clone.ctrlSurfaceTextureTop =       clone.ctrlSurfaceTextureTopCached =     ctrlSurfaceTextureTop;
                    clone.ctrlSurfaceTextureBottom =    clone.ctrlSurfaceTextureBottomCached =  ctrlSurfaceTextureBottom;
                    clone.ctrlEdgeTexture =             clone.ctrlEdgeTextureCached =           ctrlEdgeTexture;
                }
                clone.SetupRecurring ();
                clone.updateCounterpartsAllowed = true;
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
                wingEdgeScaleTrailing = Mathf.Clamp (wingEdgeScaleTrailing, wingEdgeScaleLimits.x, wingEdgeScaleLimits.y);
                wingEdgeScaleLeading = Mathf.Clamp (wingEdgeScaleLeading, wingEdgeScaleLimits.x, wingEdgeScaleLimits.y);

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
                SetFieldVisibility ("ctrlSpan", false);
                SetFieldVisibility ("ctrlWidthRoot", false);
                SetFieldVisibility ("ctrlWidthTip", false);
                SetFieldVisibility ("ctrlThicknessRoot", false);
                SetFieldVisibility ("ctrlThicknessTip", false);
                SetFieldVisibility ("ctrlOffsetRoot", false);
                SetFieldVisibility ("ctrlOffsetTip", false);
                SetFieldVisibility ("ctrlSurfaceTextureTop", false);
                SetFieldVisibility ("ctrlSurfaceTextureBottom", false);
                SetFieldVisibility ("ctrlEdgeTexture", false);

                SetFieldType ("wingSpan", 1, wingSpanLimits, 0.125f);
                SetFieldType ("wingWidthRoot", 1, wingWidthLimits, 0.125f);
                SetFieldType ("wingWidthTip", 1, wingWidthLimits, 0.125f);
                SetFieldType ("wingOffset", 1, wingOffsetLimits, 0.125f);
            }
            else
            {
                SetFieldVisibility ("wingSpan", false);
                SetFieldVisibility ("wingWidthRoot", false);
                SetFieldVisibility ("wingWidthTip", false);
                SetFieldVisibility ("wingThicknessRoot", false);
                SetFieldVisibility ("wingThicknessTip", false);
                SetFieldVisibility ("wingOffset", false);
                SetFieldVisibility ("wingSurfaceTextureTop", false);
                SetFieldVisibility ("wingSurfaceTextureBottom", false);
                SetFieldVisibility ("wingEdgeScaleTrailing", false);
                SetFieldVisibility ("wingEdgeScaleLeading", false);
                SetFieldVisibility ("wingEdgeTypeTrailing", false);
                SetFieldVisibility ("wingEdgeTypeLeading", false);
                SetFieldVisibility ("wingEdgeTextureTrailing", false);
                SetFieldVisibility ("wingEdgeTextureLeading", false);

                SetFieldType ("ctrlSpan", 1, ctrlSpanLimits, 0.125f);

                // SetFieldVisibility ("syncWidth", false);
                // SetFieldVisibility ("syncThickness", false);
                // SetFieldVisibility ("syncEdge", false);
            }
        }

        private void SetFieldVisibility (string name, bool visible)
        {
            BaseField field = Fields[name];
            field.uiControlEditor.controlEnabled = visible;
            field.uiControlFlight.controlEnabled = visible;
            field.guiActiveEditor = visible;
            field.guiActive = visible;
        }

        private void SetFieldType (string name, int type, Vector2 limits, float increment)
        {
            BaseField field = Fields[name];
            field.uiControlEditor.controlEnabled = false;
            field.uiControlEditor = null;
            if (type == 0)
            {
                UI_FloatRange ui = new UI_FloatRange ();
                field.uiControlEditor = ui;
                ui.minValue = limits.x;
                ui.maxValue = limits.y;
                ui.stepIncrement = increment;
                ui.scene = UI_Scene.Editor;
                ui.controlEnabled = true;
                ui.Setup (field);
            }
            if (type == 1)
            {
                UI_FloatEdit ui = new UI_FloatEdit ();
                field.uiControlEditor = ui;
                ui.minValue = limits.x;
                ui.maxValue = limits.y;
                ui.incrementSlide = increment;
                ui.incrementLarge = 1f;
                ui.controlEnabled = true;
                ui.Setup (field);
            }
        }

        public void SetupMeshReferences ()
        {
            bool required = true;
            if (!isCtrlSrf)
            {
                if 
                (
                    meshReferenceWingSection != null &&
                    meshReferenceWingSurfaceTop != null &&
                    meshReferenceWingSurfaceBottom != null &&
                    meshReferenceWingEdgeA != null &&
                    meshReferenceWingEdgeB != null &&
                    meshReferenceWingEdgeC != null
                )
                {
                    if 
                    (
                        meshReferenceWingSection.vp.Length > 0 &&
                        meshReferenceWingSurfaceTop.vp.Length > 0 &&
                        meshReferenceWingSurfaceBottom.vp.Length > 0 &&
                        meshReferenceWingEdgeA.vp.Length > 0 &&
                        meshReferenceWingEdgeB.vp.Length > 0 &&
                        meshReferenceWingEdgeC.vp.Length > 0
                    )
                    {
                        required = false;
                    }
                }
            }
            else
            {
                if 
                (
                    meshReferenceCtrlEdge != null &&
                    meshReferenceCtrlSurfaceTop != null &&
                    meshReferenceCtrlSurfaceBottom != null
                )
                {
                    if 
                    (
                        meshReferenceCtrlEdge.vp.Length > 0 &&
                        meshReferenceCtrlSurfaceTop.vp.Length > 0 &&
                        meshReferenceCtrlSurfaceBottom.vp.Length > 0
                    )
                    {
                        required = false;
                    }
                }
            }
            if (required)
            {
                DebugLogWithID ("SetupMeshReferences", "References missing | isCtrlSrf: " + isCtrlSrf);
                SetupMeshReferencesFromScratch ();
            }
            else
            {
                DebugLogWithID ("SetupMeshReferences", "Skipped, all references seem to be in order");
            }
        }

        public void ReportOnMeshReferences ()
        {
            if (isCtrlSrf)
            {
                DebugLogWithID
                (
                    "ReportOnMeshReferences",
                    "Control surface reference length check"
                    + " | Edge: " + meshReferenceCtrlEdge.vp.Length
                    + " | Top: " + meshReferenceCtrlSurfaceTop.vp.Length
                    + " | Bottom: " + meshReferenceCtrlSurfaceBottom.vp.Length
                );
            }
            else
            {
                DebugLogWithID
                (
                    "ReportOnMeshReferences",
                    "Wing reference length check"
                    + " | Section: " + meshReferenceWingSection.vp.Length
                    + " | Top: " + meshReferenceWingSurfaceTop.vp.Length
                    + " | Bottom: " + meshReferenceWingSurfaceBottom.vp.Length
                    + " | Edge A: " + meshReferenceWingEdgeA.vp.Length
                    + " | Edge B: " + meshReferenceWingEdgeB.vp.Length
                    + " | Edge C: " + meshReferenceWingEdgeC.vp.Length
                );
            }
        }

        private void SetupMeshReferencesFromScratch ()
        {
            DebugLogWithID ("SetupMeshReferencesFromScratch", "No sources found, creating new references");
            if (isCtrlSrf)
            {
                WingProcedural.meshReferenceCtrlEdge = FillMeshRefererence (meshFilterCtrlEdgeReference);
                WingProcedural.meshReferenceCtrlSurfaceTop = FillMeshRefererence (meshFilterCtrlSurfaceTop);
                WingProcedural.meshReferenceCtrlSurfaceBottom = FillMeshRefererence (meshFilterCtrlSurfaceBottom);
            }
            else
            {
                WingProcedural.meshReferenceWingSection = FillMeshRefererence (meshFilterWingSection);
                WingProcedural.meshReferenceWingSurfaceTop = FillMeshRefererence (meshFilterWingSurfaceTop);
                WingProcedural.meshReferenceWingSurfaceBottom = FillMeshRefererence (meshFilterWingSurfaceBottom);
                WingProcedural.meshReferenceWingEdgeA = FillMeshRefererence (meshFilterWingEdgeA);
                WingProcedural.meshReferenceWingEdgeB = FillMeshRefererence (meshFilterWingEdgeB);
                WingProcedural.meshReferenceWingEdgeC = FillMeshRefererence (meshFilterWingEdgeC);
            }
        }

        private void SetupTemporaryCollider ()
        {
            temporaryCollider = CheckTransform ("proxy_collision_temporary");
            if (temporaryCollider != null) temporaryCollider.gameObject.SetActive (false);
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

        private bool logCheckMeshFilter = false;

        private MeshFilter CheckMeshFilter (MeshFilter reference, string name) { return CheckMeshFilter (reference, name, false); }
        private MeshFilter CheckMeshFilter (MeshFilter reference, string name, bool disable)
        {
            if (reference == null)
            {
                if (logCheckMeshFilter) DebugLogWithID ("CheckMeshFilter", "Looking for object: " + name);
                Transform parent = part.transform.GetChild (0).GetChild (0).GetChild (0).Find (name);
                if (parent != null)
                {
                    if (logCheckMeshFilter) DebugLogWithID ("CheckMeshFilter", "Object " + name + " was found");
                    reference = parent.gameObject.GetComponent<MeshFilter> ();
                    if (disable) parent.gameObject.SetActive (false);
                }
                else
                {
                    if (logCheckMeshFilter) DebugLogWithID ("CheckMeshFilter", "Object " + name + " was not found!");
                }
            }
            return reference;
        }

        private Transform CheckTransform (string name)
        {
            Transform t = part.transform.GetChild (0).GetChild (0).GetChild (0).Find (name);
            return t;
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




        // Aerodynamics value calculation
        // More or less lifted from pWings, so credit goes to DYJ and Taverius

        private bool FARactive = false;
        private bool NEARactive = false;
        private bool FARmass = false;

        [KSPField]
        public bool isCtrlSrf = true;

        [KSPField]
        public float liftFudgeNumber = 0.0775f;
        [KSPField]
        public float massFudgeNumber = 0.015f;
        [KSPField]
        public float dragBaseValue = 0.6f;
        [KSPField]
        public float dragMultiplier = 3.3939f;
        [KSPField]
        public float connectionFactor = 150f;
        [KSPField]
        public float connectionMinimum = 50f;
        [KSPField]
        public float costDensity = 5300f;
        [KSPField]
        public float costDensityControl = 6500f;
        [KSPField]
        public float modelControlSurfaceFraction = 1f;

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
                DebugLogWithID ("CalculateAerodynamicValues", "Started");

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

                DebugLogWithID ("CalculateAerodynamicValues", "Passed B2/TR/MAC/MCS");

                surfaceArea = meanAerodynamicChord * b_2;
                aspectRatio = 2.0f * b_2 / meanAerodynamicChord;

                aspectRatioSweepScale = MathD.Pow (aspectRatio / MathD.Cos (MathD.Deg2Rad * midChordSweep), 2.0f) + 4.0f;
                aspectRatioSweepScale = 2.0f + MathD.Sqrt (aspectRatioSweepScale);
                aspectRatioSweepScale = (2.0f * MathD.PI) / aspectRatioSweepScale * aspectRatio;

                wingMass = MathD.Clamp (massFudgeNumber * surfaceArea * ((aspectRatioSweepScale * 2.0) / (3.0 + aspectRatioSweepScale)) * ((1.0 + taperRatio) / 2), 0.01, double.MaxValue);
                Cd = dragBaseValue / aspectRatioSweepScale * dragMultiplier;
                Cl = liftFudgeNumber * surfaceArea * aspectRatioSweepScale;

                connectionForce = MathD.Round (MathD.Clamp (MathD.Sqrt (Cl + ChildrenCl) * (double) connectionFactor, (double) connectionMinimum, double.MaxValue));

                DebugLogWithID ("CalculateAerodynamicValues", "Passed SR/AR/ARSS/mass/Cl/Cd/connection");

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

                DebugLogWithID ("CalculateAerodynamicValues", "Passed cost/force/torque");

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

                DebugLogWithID ("CalculateAerodynamicValues", "Passed stock drag/deflection/area");

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

                DebugLogWithID ("CalculateAerodynamicValues", "Passed FAR/NEAR parameter setting");

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

                DebugLogWithID ("CalculateAerodynamicValues", "Passed GUI setup");

                if (HighLogic.LoadedSceneIsEditor) GameEvents.onEditorShipModified.Fire (EditorLogic.fetch.ship);

                DebugLogWithID ("CalculateAerodynamicValues", "Finished");
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
    }
}
