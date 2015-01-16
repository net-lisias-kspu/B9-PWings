using KSP;
using KSP.IO;
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



        // Common values

        private float incrementMain = 0.125f;
        private float incrementSmall = 0.04f;
        private float incrementInt = 1f;




        // Wing properties / core

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Base"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool wingFieldGroupBase = false;
        public bool wingFieldGroupBaseCached = false;
        public static bool wingFieldGroupBaseStatic = false;
        private static string[] wingFieldGroupBaseArray = new string[] { "wingSpan", "wingWidthRoot", "wingWidthTip", "wingOffset", "wingThicknessRoot", "wingThicknessTip" };

        // [KSPField (guiActiveEditor = true, guiActive = false, guiName = "Test"),
        // UI_Label (scene = UI_Scene.Editor)]
        // public string testB = "Label";

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Semispan", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float wingSpan = 4f;
        public float wingSpanCached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Width R", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float wingWidthRoot = 4f;
        public float wingWidthRootCached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Width T", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float wingWidthTip = 4f;
        public float wingWidthTipCached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Offset", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = -8f, maxValue = 8f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float wingOffset = 0f;
        public float wingOffsetCached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Height R"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float wingThicknessRoot = 0.24f;
        public float wingThicknessRootCached = 0.24f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Height T"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float wingThicknessTip = 0.24f;
        public float wingThicknessTipCached = 0.24f;

        // Wing properties / Leading edge

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Lead. edge"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool wingFieldGroupEdgeLeading = false;
        public bool wingFieldGroupEdgeLeadingCached = false;
        public static bool wingFieldGroupEdgeLeadingStatic = false;
        private static string[] wingFieldGroupEdgeLeadingArray = new string[] { "wingEdgeTypeLeading", "wingEdgeWidthLeadingRoot", "wingEdgeWidthLeadingTip" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Shape"),
        UI_FloatRange (minValue = 1f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float wingEdgeTypeLeading = 2f;
        public float wingEdgeTypeLeadingCached = 2f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (root)"),
        UI_FloatRange (minValue = 0f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float wingEdgeWidthLeadingRoot = 0.24f;
        public float wingEdgeWidthLeadingRootCached = 0.24f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (tip)"),
        UI_FloatRange (minValue = 0f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float wingEdgeWidthLeadingTip = 0.24f;
        public float wingEdgeWidthLeadingTipCached = 0.24f;

        // Wind properties / Trailing edge

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Trail. edge"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool wingFieldGroupEdgeTrailing = false;
        public bool wingFieldGroupEdgeTrailingCached = false;
        public static bool wingFieldGroupEdgeTrailingStatic = false;
        private static string[] wingFieldGroupEdgeTrailingArray = new string[] { "wingEdgeTypeTrailing", "wingEdgeWidthTrailingRoot", "wingEdgeWidthTrailingTip" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Shape"),
        UI_FloatRange (minValue = 1f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float wingEdgeTypeTrailing = 3f;
        public float wingEdgeTypeTrailingCached = 3f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (root)"),
        UI_FloatRange (minValue = 0f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float wingEdgeWidthTrailingRoot = 0.48f;
        public float wingEdgeWidthTrailingRootCached = 0.48f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (tip)"),
        UI_FloatRange (minValue = 0f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float wingEdgeWidthTrailingTip = 0.48f;
        public float wingEdgeWidthTrailingTipCached = 0.48f;

        // Wing properties / Surface materials

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Materials"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool wingFieldGroupMaterials = false;
        public bool wingFieldGroupMaterialsCached = false;
        public static bool wingFieldGroupMaterialsStatic = false;
        private static string[] wingFieldGroupMaterialsArray = new string[] { "wingSurfaceTextureTop", "wingSurfaceTextureBottom", "wingEdgeTextureLeading", "wingEdgeTextureTrailing" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Side A"),
        UI_FloatRange (minValue = 0f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float wingSurfaceTextureTop = 3f;
        public float wingSurfaceTextureTopCached = 3f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Side B"),
        UI_FloatRange (minValue = 0f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float wingSurfaceTextureBottom = 4f;
        public float wingSurfaceTextureBottomCached = 4f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Edge L"),
        UI_FloatRange (minValue = 0f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float wingEdgeTextureLeading = 4f;
        public float wingEdgeTextureLeadingCached = 4f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Edge T"),
        UI_FloatRange (minValue = 0f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float wingEdgeTextureTrailing = 4f;
        public float wingEdgeTextureTrailingCached = 4f;

        // Other

        public MeshFilter meshFilterWingSection;
        public MeshFilter meshFilterWingSurface;
        public List<MeshFilter> meshFiltersWingEdgeTrailing = new List<MeshFilter> ();
        public List<MeshFilter> meshFiltersWingEdgeLeading = new List<MeshFilter> ();

        public static MeshReference meshReferenceWingSection;
        public static MeshReference meshReferenceWingSurface;
        public static List<MeshReference> meshReferencesWingEdge = new List<MeshReference> ();

        private Vector2 wingSpanLimits = new Vector2 (0.25f, 16f);
        private Vector2 wingWidthLimits = new Vector2 (0.25f, 16f);
        private Vector2 wingThicknessLimits = new Vector2 (0.08f, 1f);
        private Vector2 wingOffsetLimits = new Vector2 (-8f, 8f);
        private Vector2 wingEdgeWidthLimits = new Vector2 (0f, 1f);
        private Vector2 wingEdgeTypeLimits = new Vector2 (1f, 4f);
        private Vector2 wingTextureLimits = new Vector2 (0f, 4f);
        private int     wingEdgeTypeCount = 4;




        // Control surface properties / Core

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Base"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool ctrlFieldGroupBase = false;
        public bool ctrlFieldGroupBaseCached = false;
        public static bool ctrlFieldGroupBaseStatic = false;
        private static string[] ctrlFieldGroupBaseArray = new string[] { "ctrlSpan", "ctrlWidthRoot", "ctrlWidthTip", "ctrlThicknessRoot", "ctrlThicknessTip"};

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Length", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 8f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float ctrlSpan = 1f;
        public float ctrlSpanCached = 1f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width R"),
        UI_FloatRange (minValue = 0.25f, maxValue = 1.5f, scene = UI_Scene.Editor, stepIncrement = 0.125f)]
        public float ctrlWidthRoot = 0.25f;
        public float ctrlWidthRootCached = 0.25f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width T"),
        UI_FloatRange (minValue = 0.25f, maxValue = 1.5f, scene = UI_Scene.Editor, stepIncrement = 0.125f)]
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

        // Edge

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Edge"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool ctrlFieldGroupEdge = false;
        public bool ctrlFieldGroupEdgeCached = false;
        public static bool ctrlFieldGroupEdgeStatic = false;
        private static string[] ctrlFieldGroupEdgeArray = new string[] { "ctrlEdgeType", "ctrlEdgeWidthRoot", "ctrlEdgeWidthTip", "ctrlOffsetRoot", "ctrlOffsetTip" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Shape"),
        UI_FloatRange (minValue = 1f, maxValue = 3f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float ctrlEdgeType = 2f;
        public float ctrlEdgeTypeCached = 2f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (root)"),
        UI_FloatRange (minValue = 0.24f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float ctrlEdgeWidthRoot = 0.48f;
        public float ctrlEdgeWidthRootCached = 0.48f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (tip)"),
        UI_FloatRange (minValue = 0.24f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float ctrlEdgeWidthTip = 0.48f;
        public float ctrlEdgeWidthTipCached = 0.48f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Sweep (root)"),
        UI_FloatRange (minValue = -1f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.125f)]
        public float ctrlOffsetRoot = 0.0f;
        public float ctrlOffsetRootCached = 0.0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Sweep (tip)"),
        UI_FloatRange (minValue = -1f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.125f)]
        public float ctrlOffsetTip = 0.0f;
        public float ctrlOffsetTipCached = 0.0f;

        // Materials

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Materials"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool ctrlFieldGroupMaterials = false;
        public bool ctrlFieldGroupMaterialsCached = false;
        public static bool ctrlFieldGroupMaterialsStatic = false;
        private static string[] ctrlFieldGroupMaterialsArray = new string[] { "ctrlSurfaceTextureTop", "ctrlSurfaceTextureBottom", "ctrlEdgeTexture" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Side A"),
        UI_FloatRange (minValue = 0f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float ctrlSurfaceTextureTop = 1f;
        public float ctrlSurfaceTextureTopCached;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Side B"),
        UI_FloatRange (minValue = 0f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float ctrlSurfaceTextureBottom = 4f;
        public float ctrlSurfaceTextureBottomCached;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Edge"),
        UI_FloatRange (minValue = 0f, maxValue = 4f, scene = UI_Scene.Editor, stepIncrement = 1f)]
        public float ctrlEdgeTexture = 4f;
        public float ctrlEdgeTextureCached = 4f;

        // Other

        public MeshFilter meshFilterCtrlFrame;
        public MeshFilter meshFilterCtrlSurface;
        public List<MeshFilter> meshFiltersCtrlEdge = new List<MeshFilter> ();

        public static MeshReference meshReferenceCtrlFrame;
        public static MeshReference meshReferenceCtrlSurface;
        public static List<MeshReference> meshReferencesCtrlEdge = new List<MeshReference> ();

        private Vector2 ctrlSpanLimits = new Vector2 (0.25f, 8f);
        private Vector2 ctrlWidthLimits = new Vector2 (0.25f, 1.5f);
        private Vector2 ctrlThicknessLimits = new Vector2 (0.08f, 0.48f);
        private Vector2 ctrlOffsetLimits = new Vector2 (-1f, 1f);
        private Vector2 ctrlTextureLimits = new Vector2 (0f, 4f);
        private Vector2 ctrlEdgeWidthLimits = new Vector2 (0f, 1f);
        private Vector2 ctrlEdgeTypeLimits = new Vector2 (1f, 3f);
        private int     ctrlEdgeTypeCount = 3;




        // Some handy bools

        [KSPField]
        public bool isCtrlSrf = false;

        [KSPField]
        public bool isWingAsCtrlSrf = false;

        [KSPField (isPersistant = true)]
        public bool isAttached = false;
        public bool isStarted = false;
        public bool isStartingNow = false;
        public bool justDetached = false;

        private bool logCAV = false;
        private bool logUpdate = false;
        private bool logUpdateGeometry = false;
        private bool logUpdateMaterials = false;
        private bool logMeshReferences = false;
        private bool logCheckMeshFilter = false;
        private bool logPropertyWindow = false;
        private bool logFlightSetup = false;




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
                UpdateUI ();

                if (isStarted)
                {

                    // Next, compare the properties to cached values
                    // If there is a mismatch, then update is required

                    if (!isCtrlSrf)
                    {
                        CheckFieldValue (wingSpan, ref wingSpanCached);
                        CheckFieldValue (wingWidthRoot, ref wingWidthRootCached);
                        CheckFieldValue (wingWidthTip, ref wingWidthTipCached);

                        CheckFieldValue (wingThicknessRoot, ref wingThicknessRootCached);
                        CheckFieldValue (wingThicknessTip, ref wingThicknessTipCached);
                        CheckFieldValue (wingOffset, ref wingOffsetCached);

                        CheckFieldValue (wingSurfaceTextureTop, ref wingSurfaceTextureTopCached);
                        CheckFieldValue (wingSurfaceTextureBottom, ref wingSurfaceTextureBottomCached);

                        CheckFieldValue (wingEdgeTypeTrailing, ref wingEdgeTypeTrailingCached);
                        CheckFieldValue (wingEdgeTypeLeading, ref wingEdgeTypeLeadingCached);
                        CheckFieldValue (wingEdgeTextureTrailing, ref wingEdgeTextureTrailingCached);
                        CheckFieldValue (wingEdgeTextureLeading, ref wingEdgeTextureLeadingCached);

                        CheckFieldValue (wingEdgeWidthLeadingRoot, ref wingEdgeWidthLeadingRootCached);
                        CheckFieldValue (wingEdgeWidthLeadingTip, ref wingEdgeWidthLeadingTipCached);
                        CheckFieldValue (wingEdgeWidthTrailingRoot, ref wingEdgeWidthTrailingRootCached);
                        CheckFieldValue (wingEdgeWidthTrailingTip, ref wingEdgeWidthTrailingTipCached);

                        CheckFieldGroup (wingFieldGroupBase, ref wingFieldGroupBaseCached, ref wingFieldGroupBaseStatic, wingFieldGroupBaseArray, false);
                        CheckFieldGroup (wingFieldGroupEdgeLeading, ref wingFieldGroupEdgeLeadingCached, ref wingFieldGroupEdgeLeadingStatic, wingFieldGroupEdgeLeadingArray, false);
                        CheckFieldGroup (wingFieldGroupEdgeTrailing, ref wingFieldGroupEdgeTrailingCached, ref wingFieldGroupEdgeTrailingStatic, wingFieldGroupEdgeTrailingArray, false);
                        CheckFieldGroup (wingFieldGroupMaterials, ref wingFieldGroupMaterialsCached, ref wingFieldGroupMaterialsStatic, wingFieldGroupMaterialsArray, false);
                    }
                    else
                    {
                        CheckFieldValue (ctrlSpan, ref ctrlSpanCached);
                        CheckFieldValue (ctrlWidthRoot, ref ctrlWidthRootCached);
                        CheckFieldValue (ctrlWidthTip, ref ctrlWidthTipCached);
                        CheckFieldValue (ctrlEdgeWidthRoot, ref ctrlEdgeWidthRootCached);
                        CheckFieldValue (ctrlEdgeWidthTip, ref ctrlEdgeWidthTipCached);

                        CheckFieldValue (ctrlThicknessRoot, ref ctrlThicknessRootCached);
                        CheckFieldValue (ctrlThicknessTip, ref ctrlThicknessTipCached);
                        CheckFieldValue (ctrlOffsetRoot, ref ctrlOffsetRootCached);
                        CheckFieldValue (ctrlOffsetTip, ref ctrlOffsetTipCached);

                        CheckFieldValue (ctrlSurfaceTextureTop, ref ctrlSurfaceTextureTopCached);
                        CheckFieldValue (ctrlSurfaceTextureBottom, ref ctrlSurfaceTextureBottomCached);
                        CheckFieldValue (ctrlEdgeTexture, ref ctrlEdgeTextureCached);
                        CheckFieldValue (ctrlEdgeType, ref ctrlEdgeTypeCached);

                        CheckFieldGroup (ctrlFieldGroupBase, ref ctrlFieldGroupBaseCached, ref ctrlFieldGroupBaseStatic, ctrlFieldGroupBaseArray, false);
                        CheckFieldGroup (ctrlFieldGroupEdge, ref ctrlFieldGroupEdgeCached, ref ctrlFieldGroupEdgeStatic, ctrlFieldGroupEdgeArray, false);
                        CheckFieldGroup (ctrlFieldGroupMaterials, ref ctrlFieldGroupMaterialsCached, ref ctrlFieldGroupMaterialsStatic, ctrlFieldGroupMaterialsArray, false);
                    }

                    // Trigger update of the counterparts
                    // Has to be done through a special method that overrides their cached values, preventing feedback loop
                    // Also, a somewhat strange check for attachment after detachment, seems to help in a certain case

                    if (updateRequiredOnGeometry) updateCounterparts = true;
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

                    if (updateRequiredOnGeometry)
                    {
                        updateRequiredOnGeometry = false;
                        UpdateGeometry ();
                    }
                    if (updateCounterparts && updateCounterpartsAllowed)
                    {
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

        private void CheckFieldValue (float fieldValue, ref float fieldCache)
        {
            if (fieldValue != fieldCache)
            {
                if (logUpdate) DebugLogWithID ("Update", "Detected value change");
                updateRequiredOnGeometry = true;
                fieldCache = fieldValue;
            }
        }

        private void CheckFieldGroup (bool groupStatus, ref bool groupCache, ref bool groupStatic, string[] groupEntries, bool skipCheck)
        {
            if (!skipCheck)
            {
                if (groupStatus != groupCache)
                {
                    if (logUpdate) DebugLogWithID ("Update", "Detected field group state change");
                    for (int i = 0; i < groupEntries.Length; ++i) SetFieldVisibility (groupEntries[i], groupStatus);
                    updateRequiredOnWindow = true;
                    groupCache = groupStatus;
                    groupStatic = groupStatus;
                }
            }
            else
            {
                groupCache = groupStatic;
                groupStatus = groupStatic;
                for (int i = 0; i < groupEntries.Length; ++i) SetFieldVisibility (groupEntries[i], groupStatus);
                updateRequiredOnWindow = true;
            }
        }

        private void SetFieldVisibility (string name, bool visible)
        {
            BaseField field = Fields[name];
            field.uiControlEditor.controlEnabled = visible;
            field.guiActiveEditor = visible;
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
            if (this.part.parent != null && this.part.parent.Modules.Contains ("WingProcedural"))
                this.part.parent.Modules.OfType<WingProcedural> ().FirstOrDefault ().justDetached = true;

            isAttached = false;
            justDetached = true;
            if (uiEditMode) uiEditMode = false;
        }




        // Geometry

        public void UpdateGeometry ()
        {
            if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Started | isCtrlSrf: " + isCtrlSrf);
            if (!isCtrlSrf)
            {
                float wingThicknessDeviationRoot = wingThicknessRoot / 0.24f;
                float wingThicknessDeviationTip = wingThicknessTip / 0.24f;
                float wingWidthTipBasedOffsetTrailing = wingWidthTip / 2f + wingOffset;
                float wingWidthTipBasedOffsetLeading = -wingWidthTip / 2f + wingOffset;
                float wingWidthRootBasedOffset = wingWidthRoot / 2f;

                // First, wing cross section
                // No need to filter vertices by normals

                if (meshFilterWingSection != null)
                {
                    int length = meshReferenceWingSection.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReferenceWingSection.vp, vp, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReferenceWingSection.uv, uv, length);
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing section | Passed array setup");

                    for (int i = 0; i < length; ++i)
                    {
                        // Root/tip filtering followed by leading/trailing filtering
                        if (vp[i].x < -0.05f)
                        {
                            if (vp[i].z < 0f)
                            {
                                vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetLeading);
                                uv[i] = new Vector2 (wingWidthTip, uv[i].y);
                            }
                            else
                            {
                                vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetTrailing);
                                uv[i] = new Vector2 (0f, uv[i].y);
                            }
                        }
                        else
                        {
                            if (vp[i].z < 0f)
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y * wingThicknessDeviationRoot, -wingWidthRootBasedOffset);
                                uv[i] = new Vector2 (wingWidthRoot, uv[i].y);
                            }
                            else
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y * wingThicknessDeviationRoot, wingWidthRootBasedOffset);
                                uv[i] = new Vector2 (0f, uv[i].y);
                            }
                        }
                    }

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

                // Second, wing surfaces
                // Again, no need for filtering by normals

                if (meshFilterWingSurface != null)
                {
                    meshFilterWingSurface.transform.localPosition = Vector3.zero;
                    meshFilterWingSurface.transform.localRotation = Quaternion.Euler (0f, 0f, 0f);

                    int length = meshReferenceWingSurface.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReferenceWingSurface.vp, vp, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReferenceWingSurface.uv, uv, length);
                    Color[] cl = new Color[length];
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing surface top | Passed array setup");

                    for (int i = 0; i < length; ++i)
                    {
                        // Root/tip filtering followed by leading/trailing filtering
                        if (vp[i].x < -0.05f)
                        {
                            if (vp[i].z < 0f)
                            {
                                vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetLeading);
                                uv[i] = new Vector2 (wingSpan / 4f, 1f - 0.5f + wingWidthTip / 8f - wingOffset / 4f);
                            }
                            else
                            {
                                vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetTrailing);
                                uv[i] = new Vector2 (wingSpan / 4f, 0f + 0.5f - wingWidthTip / 8f - wingOffset / 4f);
                            }
                        }
                        else
                        {
                            if (vp[i].z < 0f)
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y * wingThicknessDeviationRoot, -wingWidthRootBasedOffset);
                                uv[i] = new Vector2 (0.0f, 1f - 0.5f + wingWidthRoot / 8f);
                            }
                            else
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y * wingThicknessDeviationRoot, wingWidthRootBasedOffset);
                                uv[i] = new Vector2 (0f, 0f + 0.5f - wingWidthRoot / 8f);
                            }
                        }

                        // Top/bottom filtering
                        if (vp[i].y > 0f) cl[i] = GetVertexColorFromSelection (wingSurfaceTextureTop, 0.6f);
                        else cl[i] = GetVertexColorFromSelection (wingSurfaceTextureBottom, 0.6f);
                    }

                    meshFilterWingSurface.mesh.vertices = vp;
                    meshFilterWingSurface.mesh.uv = uv;
                    meshFilterWingSurface.mesh.colors = cl;
                    meshFilterWingSurface.mesh.RecalculateBounds ();
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing surface | Finished");
                }

                // Next, time for leading and trailing edges
                // Before modifying geometry, we have to show the correct objects for the current selection
                // As UI only works with floats, we have to cast selections into ints too

                int wingEdgeTypeTrailingInt = Mathf.RoundToInt (wingEdgeTypeTrailing - 1);
                int wingEdgeTypeLeadingInt = Mathf.RoundToInt (wingEdgeTypeLeading - 1);

                for (int i = 0; i < wingEdgeTypeCount; ++i)
                {
                    if (i != wingEdgeTypeTrailingInt) meshFiltersWingEdgeTrailing[i].gameObject.SetActive (false);
                    else meshFiltersWingEdgeTrailing[i].gameObject.SetActive (true);

                    if (i != wingEdgeTypeLeadingInt) meshFiltersWingEdgeLeading[i].gameObject.SetActive (false);
                    else meshFiltersWingEdgeLeading[i].gameObject.SetActive (true);
                }

                // Next we calculate some values reused for all edge geometry

                float wingEdgeWidthLeadingRootDeviation = wingEdgeWidthLeadingRoot / 0.24f;
                float wingEdgeWidthLeadingTipDeviation = wingEdgeWidthLeadingTip / 0.24f;

                float wingEdgeWidthTrailingRootDeviation = wingEdgeWidthTrailingRoot / 0.24f;
                float wingEdgeWidthTrailingTipDeviation = wingEdgeWidthTrailingTip / 0.24f;

                // Next, we fetch appropriate mesh reference and mesh filter for the edges and modify the meshes
                // Geometry is split into groups through simple vertex normal filtering 

                if (meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt] != null)
                {
                    MeshReference meshReference = meshReferencesWingEdge[wingEdgeTypeTrailingInt];
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
                        if (vp[i].x < -0.1f)
                        {
                            vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, vp[i].z * wingEdgeWidthTrailingTipDeviation + wingWidthTip / 2f + wingOffset); // Tip edge
                            if (nm[i].x == 0f) uv[i] = new Vector2 (wingSpan, uv[i].y);
                        }
                        else vp[i] = new Vector3 (0f, vp[i].y * wingThicknessDeviationRoot, vp[i].z * wingEdgeWidthTrailingRootDeviation + wingWidthRoot / 2f); // Root edge
                        if (nm[i].x == 0f) cl[i] = GetVertexColorFromSelection (wingEdgeTextureTrailing, 0.5f);
                    }

                    meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt].mesh.vertices = vp;
                    meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt].mesh.uv = uv;
                    meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt].mesh.colors = cl;
                    meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt].mesh.RecalculateBounds ();
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge trailing | Finished");
                }
                if (meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt] != null)
                {
                    MeshReference meshReference = meshReferencesWingEdge [wingEdgeTypeLeadingInt];
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
                        if (vp[i].x < -0.1f)
                        {
                            vp[i] = new Vector3 (-wingSpan, vp[i].y * wingThicknessDeviationTip, vp[i].z * wingEdgeWidthLeadingTipDeviation + wingWidthTip / 2f - wingOffset); // Tip edge
                            if (nm[i].x == 0f) uv[i] = new Vector2 (wingSpan, uv[i].y);
                        }
                        else vp[i] = new Vector3 (0f, vp[i].y * wingThicknessDeviationRoot, vp[i].z * wingEdgeWidthLeadingRootDeviation + wingWidthRoot / 2f); // Root edge
                        if (nm[i].x == 0f) cl[i] = GetVertexColorFromSelection (wingEdgeTextureLeading, 0.5f);
                    }

                    meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.vertices = vp;
                    meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.uv = uv;
                    meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.colors = cl;
                    meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.RecalculateBounds ();
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge leading | Finished");
                }
            }
            else
            {
                // Some reusable values

                float ctrlOffsetRootLimit = (ctrlSpan / 2f) / (ctrlWidthRoot + 1f);
                float ctrlOffsetTipLimit = (ctrlSpan / 2f) / (ctrlWidthTip + 1f);

                float ctrlOffsetRootClamped = Mathf.Clamp (ctrlOffsetRoot, -ctrlOffsetRootLimit, ctrlOffsetRootLimit);
                float ctrlOffsetTipClamped = Mathf.Clamp (ctrlOffsetTip, -ctrlOffsetTipLimit, ctrlOffsetTipLimit);

                float ctrlThicknessDeviationRoot = ctrlThicknessRoot / 0.24f;
                float ctrlThicknessDeviationTip = ctrlThicknessTip / 0.24f;

                float widthDifference = ctrlWidthRoot - ctrlWidthTip;
                float edgeLengthTrailing = Mathf.Sqrt (Mathf.Pow (ctrlSpan, 2) + Mathf.Pow (widthDifference, 2));
                float sweepTrailing = 90f - Mathf.Atan (ctrlSpan / widthDifference) * Mathf.Rad2Deg;

                if (meshFilterCtrlFrame != null)
                {
                    int length = meshReferenceCtrlFrame.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReferenceCtrlFrame.vp, vp, length);
                    Vector3[] nm = new Vector3[length];
                    Array.Copy (meshReferenceCtrlFrame.nm, nm, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReferenceCtrlFrame.uv, uv, length);
                    Color[] cl = new Color[length];
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface frame | Passed array setup");

                    for (int i = 0; i < vp.Length; ++i)
                    {
                        // Span-based shift & thickness correction
                        if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationTip, vp[i].y, vp[i].z + 0.5f - ctrlSpan / 2f);
                        else vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationRoot, vp[i].y, vp[i].z - 0.5f + ctrlSpan / 2f);

                        // Left/right sides
                        if (nm[i] == new Vector3 (0f, 0f, 1f) || nm[i] == new Vector3 (0f, 0f, -1f))
                        {
                            // Filtering out trailing edge cross sections
                            if (uv[i].y > 0.185f)
                            {
                                // Filtering out root neighbours
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

                            /*
                            // Trailing edge cross section
                            else
                            {
                                if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthTip, vp[i].z);
                                else vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthRoot, vp[i].z);
                            }
                            */
                        }

                        // Root (only needs UV adjustment)
                        else if (nm[i] == new Vector3 (0f, 1f, 0f))
                        {
                            if (vp[i].z < 0f) uv[i] = new Vector2 (ctrlSpan, uv[i].y);
                        }

                        // Trailing edge
                        else
                        {
                            // Filtering out root neighbours
                            if (vp[i].y < -0.1f)
                            {
                                if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthTip, vp[i].z);
                                else vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthRoot, vp[i].z);
                            }
                        }

                        // Offset-based distortion
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

                        /*
                        // Trailing edge (UV adjustment, has to be the last as it's based on cumulative vertex positions)
                        if (nm[i] != new Vector3 (0f, 1f, 0f) && nm[i] != new Vector3 (0f, 0f, 1f) && nm[i] != new Vector3 (0f, 0f, -1f) && uv[i].y > 0.3f)
                        {
                            if (vp[i].z < 0f) uv[i] = new Vector2 (vp[i].z, uv[i].y);
                            else uv[i] = new Vector2 (vp[i].z, uv[i].y);

                            // Color has to be applied there to avoid blanking out cross sections
                            cl[i] = GetVertexColorFromSelection (ctrlEdgeTexture, 0.75f);
                        }
                        */
                    }

                    meshFilterCtrlFrame.mesh.vertices = vp;
                    meshFilterCtrlFrame.mesh.uv = uv;
                    meshFilterCtrlFrame.mesh.colors = cl;
                    meshFilterCtrlFrame.mesh.RecalculateBounds ();

                    MeshCollider meshCollider = meshFilterCtrlFrame.gameObject.GetComponent<MeshCollider> ();
                    if (meshCollider == null) meshCollider = meshFilterCtrlFrame.gameObject.AddComponent<MeshCollider> ();
                    meshCollider.sharedMesh = null;
                    meshCollider.sharedMesh = meshFilterCtrlFrame.mesh;
                    meshCollider.convex = true;
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface frame | Finished");
                }

                // Next, time for edge types
                // Before modifying geometry, we have to show the correct objects for the current selection
                // As UI only works with floats, we have to cast selections into ints too

                int ctrlEdgeTypeInt = Mathf.RoundToInt (ctrlEdgeType - 1);
                for (int i = 0; i < ctrlEdgeTypeCount; ++i)
                {
                    if (i != ctrlEdgeTypeInt) meshFiltersCtrlEdge[i].gameObject.SetActive (false);
                    else meshFiltersCtrlEdge[i].gameObject.SetActive (true);
                }

                // Now we can modify geometry
                // Copy-pasted frame deformation sequence at the moment, to be pruned later

                if (meshFiltersCtrlEdge[ctrlEdgeTypeInt] != null)
                {
                    MeshReference meshReference = meshReferencesCtrlEdge[ctrlEdgeTypeInt];
                    int length = meshReference.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReference.vp, vp, length);
                    Vector3[] nm = new Vector3[length];
                    Array.Copy (meshReference.nm, nm, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReference.uv, uv, length);
                    Color[] cl = new Color[length];
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface edge | Passed array setup");

                    float ctrlEdgeWidthRootDeviation = ctrlEdgeWidthRoot / 0.24f;
                    float ctrlEdgeWidthTipDeviation = ctrlEdgeWidthTip / 0.24f;

                    for (int i = 0; i < vp.Length; ++i)
                    {
                        // Thickness correction (X), edge width correction (Y) and span-based offset (Z)
                        if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationTip, ((vp[i].y + 0.5f) * ctrlEdgeWidthTipDeviation) - 0.5f, vp[i].z + 0.5f - ctrlSpan / 2f);
                        else vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationRoot, ((vp[i].y + 0.5f) * ctrlEdgeWidthRootDeviation) - 0.5f, vp[i].z - 0.5f + ctrlSpan / 2f);

                        // Left/right sides
                        if (nm[i] == new Vector3 (0f, 0f, 1f) || nm[i] == new Vector3 (0f, 0f, -1f))
                        {
                            if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthTip, vp[i].z);
                            else vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthRoot, vp[i].z);
                        }

                        // Trailing edge
                        else
                        {
                            // Filtering out root neighbours
                            if (vp[i].y < -0.1f)
                            {
                                if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthTip, vp[i].z);
                                else vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - ctrlWidthRoot, vp[i].z);
                            }
                        }

                        // Offset-based distortion
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

                        // Trailing edge (UV adjustment, has to be the last as it's based on cumulative vertex positions)
                        if (nm[i] != new Vector3 (0f, 1f, 0f) && nm[i] != new Vector3 (0f, 0f, 1f) && nm[i] != new Vector3 (0f, 0f, -1f) && uv[i].y < 0.3f)
                        {
                            if (vp[i].z < 0f) uv[i] = new Vector2 (vp[i].z, uv[i].y);
                            else uv[i] = new Vector2 (vp[i].z, uv[i].y);

                            // Color has to be applied there to avoid blanking out cross sections
                            cl[i] = GetVertexColorFromSelection (ctrlEdgeTexture, 0.5f);
                        }
                    }

                    meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.vertices = vp;
                    meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.uv = uv;
                    meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.colors = cl;
                    meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.RecalculateBounds ();
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface edge | Finished");
                }

                // Finally, simple top/bottom surface changes

                if (meshFilterCtrlSurface != null)
                {
                    int length = meshReferenceCtrlSurface.vp.Length;
                    Vector3[] vp = new Vector3[length];
                    Array.Copy (meshReferenceCtrlSurface.vp, vp, length);
                    Vector2[] uv = new Vector2[length];
                    Array.Copy (meshReferenceCtrlSurface.uv, uv, length);
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
                        if (vp[i].x > 0f) cl[i] = GetVertexColorFromSelection (ctrlSurfaceTextureTop, 0.6f);
                        else cl[i] = GetVertexColorFromSelection (ctrlSurfaceTextureBottom, 0.6f);
                    }
                    meshFilterCtrlSurface.mesh.vertices = vp;
                    meshFilterCtrlSurface.mesh.uv = uv;
                    meshFilterCtrlSurface.mesh.colors = cl;
                    meshFilterCtrlSurface.mesh.RecalculateBounds ();
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface top | Finished");
                }
            }
            if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Finished");
            if (HighLogic.LoadedSceneIsEditor) CalculateAerodynamicValues ();
        }




        // Edge geometry

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
                    SetMaterial (meshFilterWingSurface, materialLayeredSurface);
                    for (int i = 0; i < wingEdgeTypeCount; ++i)
                    {
                        SetMaterial (meshFiltersWingEdgeTrailing[i], materialLayeredEdge);
                        SetMaterial (meshFiltersWingEdgeLeading[i], materialLayeredEdge);
                    }
                }
                else
                {
                    SetMaterial (meshFilterCtrlSurface, materialLayeredSurface);
                    SetMaterial (meshFilterCtrlFrame, materialLayeredEdge);
                    for (int i = 0; i < ctrlEdgeTypeCount; ++i)
                    {
                        SetMaterial (meshFiltersCtrlEdge[i], materialLayeredEdge);
                    }
                }
            }
            else if (logUpdateMaterials) DebugLogWithID ("UpdateMaterials", "Material creation failed");
        }

        private void SetMaterialReferences ()
        {
            if (materialLayeredSurface == null) materialLayeredSurface = ResourceExtractor.GetEmbeddedMaterial ("B9_Aerospace_WingStuff.SpecularLayered.txt");
            if (materialLayeredEdge == null) materialLayeredEdge = ResourceExtractor.GetEmbeddedMaterial ("B9_Aerospace_WingStuff.SpecularLayered.txt");

            if (!isCtrlSrf) SetTextures (meshFilterWingSurface, meshFiltersWingEdgeTrailing[0]);
            else SetTextures (meshFilterCtrlSurface, meshFilterCtrlFrame);

            if (materialLayeredSurfaceTexture != null)
            {
                materialLayeredSurface.SetTexture ("_MainTex", materialLayeredSurfaceTexture);
                materialLayeredSurface.SetFloat ("_Shininess", materialPropertyShininess);
                materialLayeredSurface.SetColor ("_SpecColor", materialPropertySpecular);
            }
            else if (logUpdateMaterials) DebugLogWithID ("SetMaterialReferences", "Surface texture not found");

            if (materialLayeredEdgeTexture != null)
            {
                materialLayeredEdge.SetTexture ("_MainTex", materialLayeredEdgeTexture);
                materialLayeredEdge.SetFloat ("_Shininess", materialPropertyShininess);
                materialLayeredEdge.SetColor ("_SpecColor", materialPropertySpecular);
            }
            else if (logUpdateMaterials) DebugLogWithID ("SetMaterialReferences", "Edge texture not found");
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

        private Color GetVertexColorFromSelection (float selection, float preferredFill)
        {
            if (selection == 0) return new Color (Mathf.Clamp (preferredFill, 0f, 1f), 1.0f, 0.0f, 0.0f);
            else return new Color (0.0f, 0.0f, 0.0f, (selection - 1f) / 3f);
        }




        // Setup

        public void Setup ()
        {
            isStartingNow = true;
            SetupMeshFilters ();
            SetupClamping ();
            SetupFields ();
            SetupMeshReferences ();
            ReportOnMeshReferences ();
            SetupRecurring ();
            isStartingNow = false;
        }

        public void SetupRecurring ()
        {
            UpdateMaterials ();
            UpdateGeometry ();
            UpdateCollidersForFAR ();
            UpdateWindow ();
        }

        public void UpdateCounterparts ()
        {
            for (int i = 0; i < this.part.symmetryCounterparts.Count; ++i)
            {
                var clone = this.part.symmetryCounterparts[i].Modules.OfType<WingProcedural> ().FirstOrDefault ();
                clone.updateCounterpartsAllowed = false;
                if (!isCtrlSrf)
                {
                    clone.wingSpan =                    clone.wingSpanCached =                  wingSpan;
                    clone.wingWidthRoot =               clone.wingWidthRootCached =             wingWidthRoot;
                    clone.wingWidthTip =                clone.wingWidthTipCached =              wingWidthTip;

                    clone.wingThicknessRoot =           clone.wingThicknessRootCached =         wingThicknessRoot;
                    clone.wingThicknessTip =            clone.wingThicknessTipCached =          wingThicknessTip;
                    clone.wingOffset =                  clone.wingOffsetCached =                wingOffset;
                    clone.wingSurfaceTextureTop =       clone.wingSurfaceTextureTopCached =     wingSurfaceTextureTop;
                    clone.wingSurfaceTextureBottom =    clone.wingSurfaceTextureBottomCached =  wingSurfaceTextureBottom;

                    clone.wingEdgeTypeTrailing =        clone.wingEdgeTypeTrailingCached =      wingEdgeTypeTrailing;
                    clone.wingEdgeTypeLeading =         clone.wingEdgeTypeLeadingCached =       wingEdgeTypeLeading;
                    clone.wingEdgeTextureLeading =      clone.wingEdgeTextureLeadingCached =    wingEdgeTextureLeading;
                    clone.wingEdgeTextureTrailing =     clone.wingEdgeTextureTrailingCached =   wingEdgeTextureTrailing;

                    clone.wingEdgeWidthLeadingRoot =    clone.wingEdgeWidthLeadingRootCached =  wingEdgeWidthLeadingRoot;
                    clone.wingEdgeWidthLeadingTip =     clone.wingEdgeWidthLeadingTipCached =   wingEdgeWidthLeadingTip;
                    clone.wingEdgeWidthTrailingRoot =   clone.wingEdgeWidthTrailingRootCached = wingEdgeWidthTrailingRoot;
                    clone.wingEdgeWidthTrailingTip =    clone.wingEdgeWidthTrailingTipCached =  wingEdgeWidthTrailingTip;
                }
                else
                {
                    clone.ctrlSpan =                    clone.ctrlSpanCached =                  ctrlSpan;
                    clone.ctrlWidthRoot =               clone.ctrlWidthRootCached =             ctrlWidthRoot;
                    clone.ctrlWidthTip =                clone.ctrlWidthTipCached =              ctrlWidthTip;
                    clone.ctrlEdgeWidthRoot =           clone.ctrlEdgeWidthRootCached =         ctrlEdgeWidthRoot;
                    clone.ctrlEdgeWidthTip =            clone.ctrlEdgeWidthTipCached =          ctrlEdgeWidthTip;

                    clone.ctrlThicknessRoot =           clone.ctrlThicknessRootCached =         ctrlThicknessRoot;
                    clone.ctrlThicknessTip =            clone.ctrlThicknessTipCached =          ctrlThicknessTip;
                    clone.ctrlOffsetRoot =              clone.ctrlOffsetRootCached =            ctrlOffsetRoot;
                    clone.ctrlOffsetTip =               clone.ctrlOffsetTipCached =             ctrlOffsetTip;

                    clone.ctrlSurfaceTextureTop =       clone.ctrlSurfaceTextureTopCached =     ctrlSurfaceTextureTop;
                    clone.ctrlSurfaceTextureBottom =    clone.ctrlSurfaceTextureBottomCached =  ctrlSurfaceTextureBottom;
                    clone.ctrlEdgeTexture =             clone.ctrlEdgeTextureCached =           ctrlEdgeTexture;
                    clone.ctrlEdgeType =                clone.ctrlEdgeTypeCached =              ctrlEdgeType;
                }
                clone.SetupRecurring ();
                clone.updateCounterpartsAllowed = true;
            }
        }

        private void SetupMeshFilters ()
        {
            if (!isCtrlSrf)
            {
                meshFilterWingSurface = CheckMeshFilter (meshFilterWingSurface, "surface");
                meshFilterWingSection = CheckMeshFilter (meshFilterWingSection, "section"); 
                for (int i = 0; i < wingEdgeTypeCount; ++i)
                {
                    MeshFilter meshFilterWingEdgeTrailing = CheckMeshFilter ("edge_trailing_type" + i);
                    MeshFilter meshFilterWingEdgeLeading = CheckMeshFilter ("edge_leading_type" + i);
                    meshFiltersWingEdgeTrailing.Add (meshFilterWingEdgeTrailing);
                    meshFiltersWingEdgeLeading.Add (meshFilterWingEdgeLeading);
                }
            }
            else
            {
                meshFilterCtrlFrame = CheckMeshFilter (meshFilterCtrlFrame, "frame");
                meshFilterCtrlSurface = CheckMeshFilter (meshFilterCtrlSurface, "surface");
                for (int i = 0; i < ctrlEdgeTypeCount; ++i)
                {
                    MeshFilter meshFilterCtrlEdge = CheckMeshFilter ("edge_type" + i);
                    meshFiltersCtrlEdge.Add (meshFilterCtrlEdge);
                }
            }
        }

        private void SetupClamping ()
        {
            if (!isCtrlSrf)
            {
                ClampFieldValue (ref wingSpan, wingSpanLimits);
                ClampFieldValue (ref wingWidthRoot, wingWidthLimits);
                ClampFieldValue (ref wingWidthTip, wingWidthLimits);

                ClampFieldValue (ref wingThicknessRoot, wingThicknessLimits);
                ClampFieldValue (ref wingThicknessTip, wingThicknessLimits);
                ClampFieldValue (ref wingOffset, wingOffsetLimits);
                ClampFieldValue (ref wingSurfaceTextureTop, wingTextureLimits);
                ClampFieldValue (ref wingSurfaceTextureBottom, wingTextureLimits);

                ClampFieldValue (ref wingEdgeTypeTrailing, wingEdgeTypeLimits);
                ClampFieldValue (ref wingEdgeTypeLeading, wingEdgeTypeLimits);
                ClampFieldValue (ref wingEdgeTextureLeading, wingTextureLimits);
                ClampFieldValue (ref wingEdgeTextureTrailing, wingTextureLimits);

                ClampFieldValue (ref wingEdgeWidthLeadingRoot, wingEdgeWidthLimits);
                ClampFieldValue (ref wingEdgeWidthLeadingTip, wingEdgeWidthLimits);
                ClampFieldValue (ref wingEdgeWidthTrailingRoot, wingEdgeWidthLimits);
                ClampFieldValue (ref wingEdgeWidthTrailingTip, wingEdgeWidthLimits);
            }
            else
            {
                ClampFieldValue (ref ctrlSpan, ctrlSpanLimits);
                ClampFieldValue (ref ctrlWidthRoot, ctrlWidthLimits);
                ClampFieldValue (ref ctrlWidthTip, ctrlWidthLimits);
                ClampFieldValue (ref ctrlEdgeWidthRoot, ctrlEdgeWidthLimits);
                ClampFieldValue (ref ctrlEdgeWidthTip, ctrlEdgeWidthLimits);
                ClampFieldValue (ref ctrlThicknessRoot, ctrlThicknessLimits);
                ClampFieldValue (ref ctrlThicknessTip, ctrlThicknessLimits);
                ClampFieldValue (ref ctrlOffsetRoot, ctrlOffsetLimits);
                ClampFieldValue (ref ctrlOffsetTip, ctrlOffsetLimits);
                ClampFieldValue (ref ctrlSurfaceTextureTop, ctrlTextureLimits);
                ClampFieldValue (ref ctrlSurfaceTextureBottom, ctrlTextureLimits);
                ClampFieldValue (ref ctrlEdgeTexture, ctrlTextureLimits);
                ClampFieldValue (ref ctrlEdgeType, ctrlEdgeTypeLimits);
            }
        }

        private void ClampFieldValue (ref float field, Vector2 limits)
        {
            field = Mathf.Clamp (field, limits.x, limits.y);
        }

        private void SetupFields ()
        {
            SetFieldVisibility ("wingFieldGroupBase", !isCtrlSrf);
            SetFieldVisibility ("wingFieldGroupEdgeLeading", !isCtrlSrf);
            SetFieldVisibility ("wingFieldGroupEdgeTrailing", !isCtrlSrf);
            SetFieldVisibility ("wingFieldGroupMaterials", !isCtrlSrf);

            SetFieldVisibility ("ctrlFieldGroupBase", isCtrlSrf);
            SetFieldVisibility ("ctrlFieldGroupEdge", isCtrlSrf);
            SetFieldVisibility ("ctrlFieldGroupMaterials", isCtrlSrf);

            SetFieldType ("wingSpan", 1, wingSpanLimits, incrementMain, false);
            SetFieldType ("wingWidthRoot", 1, wingWidthLimits, incrementMain, false);
            SetFieldType ("wingWidthTip", 1, wingWidthLimits, incrementMain, false);

            SetFieldType ("wingOffset", 1, wingOffsetLimits, incrementMain, false);
            SetFieldType ("wingThicknessRoot", 0, wingThicknessLimits, incrementSmall, false);
            SetFieldType ("wingThicknessTip", 0, wingThicknessLimits, incrementSmall, false);
            SetFieldType ("wingSurfaceTextureTop", 0, wingTextureLimits, incrementInt, false);
            SetFieldType ("wingSurfaceTextureBottom", 0, wingTextureLimits, incrementInt, false);

            SetFieldType ("wingEdgeTypeLeading", 0, wingEdgeTypeLimits, incrementInt, false);
            SetFieldType ("wingEdgeTypeTrailing", 0, wingEdgeTypeLimits, incrementInt, false);
            SetFieldType ("wingEdgeTextureLeading", 0, wingTextureLimits, incrementInt, false);
            SetFieldType ("wingEdgeTextureTrailing", 0, wingTextureLimits, incrementInt, false);

            SetFieldType ("wingEdgeWidthLeadingRoot", 0, wingEdgeWidthLimits, incrementSmall, false);
            SetFieldType ("wingEdgeWidthLeadingTip", 0, wingEdgeWidthLimits, incrementSmall, false);
            SetFieldType ("wingEdgeWidthTrailingRoot", 0, wingEdgeWidthLimits, incrementSmall, false);
            SetFieldType ("wingEdgeWidthTrailingTip", 0, wingEdgeWidthLimits, incrementSmall, false);

            SetFieldType ("ctrlSpan", 1, ctrlSpanLimits, incrementMain, false);
            SetFieldType ("ctrlWidthRoot", 0, ctrlWidthLimits, incrementMain, false);
            SetFieldType ("ctrlWidthTip", 0, ctrlWidthLimits, incrementMain, false);
            SetFieldType ("ctrlEdgeWidthRoot", 0, ctrlEdgeWidthLimits, incrementSmall, false);
            SetFieldType ("ctrlEdgeWidthTip", 0, ctrlEdgeWidthLimits, incrementSmall, false);

            SetFieldType ("ctrlThicknessRoot", 0, ctrlThicknessLimits, incrementSmall, false);
            SetFieldType ("ctrlThicknessTip", 0, ctrlThicknessLimits, incrementSmall, false);
            SetFieldType ("ctrlOffsetRoot", 0, ctrlOffsetLimits, incrementMain, false);
            SetFieldType ("ctrlOffsetTip", 0, ctrlOffsetLimits, incrementMain, false);
            SetFieldType ("ctrlSurfaceTextureTop", 0, ctrlTextureLimits, incrementInt, false);
            SetFieldType ("ctrlSurfaceTextureBottom", 0, ctrlTextureLimits, incrementInt, false);
            SetFieldType ("ctrlEdgeTexture", 0, ctrlTextureLimits, incrementInt, false);
            SetFieldType ("ctrlEdgeType", 0, ctrlEdgeTypeLimits, incrementInt, false);
        }

        private void SetFieldType (string name, int type, Vector2 limits, float increment, bool visible)
        {
            BaseField field = Fields[name];
            if (visible)
            {
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
            field.uiControlEditor.controlEnabled = visible;
            // field.uiControlFlight.controlEnabled = visible;
            field.guiActiveEditor = visible;
            // field.guiActive = visible;
        }

        public void SetupMeshReferences ()
        {
            bool required = true;
            if (!isCtrlSrf)
            {
                if (meshReferenceWingSection != null && meshReferenceWingSurface != null && meshReferencesWingEdge[wingEdgeTypeCount - 1] != null)
                {
                    if (meshReferenceWingSection.vp.Length > 0 && meshReferenceWingSurface.vp.Length > 0 && meshReferencesWingEdge[wingEdgeTypeCount - 1].vp.Length > 0)
                    {
                        required = false;
                    }
                }
            }
            else
            {
                if (meshReferenceCtrlFrame != null && meshReferenceCtrlSurface != null && meshReferencesCtrlEdge[ctrlEdgeTypeCount - 1] != null)
                {
                    if (meshReferenceCtrlFrame.vp.Length > 0 && meshReferenceCtrlSurface.vp.Length > 0 && meshReferencesCtrlEdge[ctrlEdgeTypeCount - 1].vp.Length > 0)
                    {
                        required = false;
                    }
                }
            }
            if (required)
            {
                if (logMeshReferences) DebugLogWithID ("SetupMeshReferences", "References missing | isCtrlSrf: " + isCtrlSrf);
                SetupMeshReferencesFromScratch ();
            }
            else
            {
                if (logMeshReferences) DebugLogWithID ("SetupMeshReferences", "Skipped, all references seem to be in order");
            }
        }

        public void ReportOnMeshReferences ()
        {
            if (isCtrlSrf)
            {
                if (logMeshReferences) DebugLogWithID
                (
                    "ReportOnMeshReferences",
                    "Control surface reference length check"
                    + " | Edge: " + meshReferenceCtrlFrame.vp.Length
                    + " | Surface: " + meshReferenceCtrlSurface.vp.Length
                );
            }
            else
            {
                if (logMeshReferences) DebugLogWithID
                (
                    "ReportOnMeshReferences",
                    "Wing reference length check"
                    + " | Section: " + meshReferenceWingSection.vp.Length
                    + " | Surface: " + meshReferenceWingSurface.vp.Length
                );
            }
        }

        private void SetupMeshReferencesFromScratch ()
        {
            if (logMeshReferences) DebugLogWithID ("SetupMeshReferencesFromScratch", "No sources found, creating new references");
            if (!isCtrlSrf)
            {
                WingProcedural.meshReferenceWingSection = FillMeshRefererence (meshFilterWingSection);
                WingProcedural.meshReferenceWingSurface = FillMeshRefererence (meshFilterWingSurface);
                for (int i = 0; i < wingEdgeTypeCount; ++i)
                {
                    MeshReference meshReferenceWingEdge = FillMeshRefererence (meshFiltersWingEdgeTrailing[i]);
                    meshReferencesWingEdge.Add (meshReferenceWingEdge);
                }
            }
            else
            {
                WingProcedural.meshReferenceCtrlFrame = FillMeshRefererence (meshFilterCtrlFrame);
                WingProcedural.meshReferenceCtrlSurface = FillMeshRefererence (meshFilterCtrlSurface);
                for (int i = 0; i < ctrlEdgeTypeCount; ++i)
                {
                    MeshReference meshReferenceCtrlEdge = FillMeshRefererence (meshFiltersCtrlEdge[i]);
                    meshReferencesCtrlEdge.Add (meshReferenceCtrlEdge);
                }
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
                    UIPartActionWindow[] windows = (UIPartActionWindow[]) FindObjectsOfType (typeof (UIPartActionWindow));
                    for (int i = 0; i < windows.Length; ++i)
                    {
                        if (windows[i].part == part) _myWindow = windows[i];
                    }
                }
                return _myWindow;
            }
        }

        private void UpdateWindow ()
        {
            //if (!isCtrlSrf)
            //{
            //    CheckFieldGroup (wingFieldGroupBase, ref wingFieldGroupBaseCached, ref wingFieldGroupBaseStatic, wingFieldGroupBaseArray, true);
            //    CheckFieldGroup (wingFieldGroupEdgeLeading, ref wingFieldGroupEdgeLeadingCached, ref wingFieldGroupEdgeLeadingStatic, wingFieldGroupEdgeLeadingArray, true);
            //    CheckFieldGroup (wingFieldGroupEdgeTrailing, ref wingFieldGroupEdgeTrailingCached, ref wingFieldGroupEdgeTrailingStatic, wingFieldGroupEdgeTrailingArray, true);
            //    CheckFieldGroup (wingFieldGroupMaterials, ref wingFieldGroupMaterialsCached, ref wingFieldGroupMaterialsStatic, wingFieldGroupMaterialsArray, true);
            //}
            //else
            //{
            //    CheckFieldGroup (ctrlFieldGroupBase, ref ctrlFieldGroupBaseCached, ref ctrlFieldGroupBaseStatic, ctrlFieldGroupBaseArray, true);
            //    CheckFieldGroup (ctrlFieldGroupEdge, ref ctrlFieldGroupEdgeCached, ref ctrlFieldGroupEdgeStatic, ctrlFieldGroupEdgeArray, true);
            //    CheckFieldGroup (ctrlFieldGroupMaterials, ref ctrlFieldGroupMaterialsCached, ref ctrlFieldGroupMaterialsStatic, ctrlFieldGroupMaterialsArray, true);
            //}
            if (myWindow != null) myWindow.displayDirty = true;
        }




        // Reference fetching

        private MeshFilter CheckMeshFilter (string name) { return CheckMeshFilter (null, name, false); }
        private MeshFilter CheckMeshFilter (MeshFilter reference, string name) { return CheckMeshFilter (reference, name, false); }
        private MeshFilter CheckMeshFilter (MeshFilter reference, string name, bool disable)
        {
            if (reference == null)
            {
                if (logCheckMeshFilter) DebugLogWithID ("CheckMeshFilter", "Looking for object: " + name);
                Transform parent = part.transform.GetChild (0).GetChild (0).GetChild (0).Find (name);
                if (parent != null)
                {
                    parent.localPosition = Vector3.zero;
                    if (logCheckMeshFilter) DebugLogWithID ("CheckMeshFilter", "Object " + name + " was found");
                    reference = parent.gameObject.GetComponent<MeshFilter> ();
                    if (disable) parent.gameObject.SetActive (false);
                }
                else { if (logCheckMeshFilter) DebugLogWithID ("CheckMeshFilter", "Object " + name + " was not found!"); }
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
            if (source != null)
            {
                int length = source.mesh.vertices.Length;
                reference.vp = new Vector3[length];
                Array.Copy (source.mesh.vertices, reference.vp, length);
                reference.nm = new Vector3[length];
                Array.Copy (source.mesh.normals, reference.nm, length);
                reference.uv = new Vector2[length];
                Array.Copy (source.mesh.uv, reference.uv, length);
            }
            else if (logMeshReferences) DebugLogWithID ("FillMeshReference", "Mesh filter reference is null, unable to set up reference arrays");
            return reference;
        }




        // Events

        public bool assembliesChecked = false;

        public override void OnStart (PartModule.StartState state)
        {
            base.OnStart (state);
            CheckAssemblies (true);
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (!uiStyleConfigured) InitStyle ();
                RenderingManager.AddToPostDrawQueue (0, OnDraw);
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (!isStarted && isAttached && !isStartingNow)
                {
                    DebugLogWithID ("OnStart", "Setup started");
                    StartCoroutine (SetupReorderedForFlight ());
                    isStarted = true;
                }
            }
        }

        public void CheckAssemblies (bool forced)
        {
            if (!assembliesChecked || forced)
            {
                aeroModelFAR = AssemblyLoader.loadedAssemblies.Any (a => a.assembly.GetName ().Name.Equals ("FerramAerospaceResearch", StringComparison.InvariantCultureIgnoreCase));
                aeroModelNEAR = AssemblyLoader.loadedAssemblies.Any (a => a.assembly.GetName ().Name.Equals ("NEAR", StringComparison.InvariantCultureIgnoreCase));
                if (aeroModelFAR || aeroModelNEAR)
                {
                    ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes ("FARAeroData");
                    for (int i = 0; i < nodes.Length; ++i)
                    {
                        if (nodes[i] == null) continue;
                        if (nodes[i].HasValue ("massPerWingAreaSupported")) aeroModelFARMass = true;
                    }
                }
                if (logCAV) DebugLogWithID ("OnStart", "Search results | FAR: " + aeroModelFAR + " | NEAR: " + aeroModelNEAR + " | FAR mass: " + aeroModelFARMass);
                if (isCtrlSrf && isWingAsCtrlSrf) Debug.LogError ("WARNING | PART IS CONFIGURED INCORRECTLY, BOTH BOOL PROPERTIES SHOULD NEVER BE SET TO TRUE");
                assembliesChecked = true;
            }
        }





        // Delayed aero value setup
        // Must be run after all geometry setups, otherwise FAR checks will be done before surrounding parts take shape, producing incorrect results

        public class VesselStatus
        {
            public Vessel vessel = null;
            public bool isUpdated = false;

            public VesselStatus (Vessel v, bool state)
            {
                vessel = v;
                isUpdated = state;
            }
        }

        public static List<VesselStatus> vesselList = new List<VesselStatus> ();

        public IEnumerator SetupReorderedForFlight ()
        {
            // First we need to determine whether the vessel this part is attached to is included into the status list
            // If it's included, we need to fetch it's index in that list

            bool vesselListInclusive = false;
            int vesselID = vessel.GetInstanceID ();
            int vesselStatusIndex = 0;
            int vesselListCount = vesselList.Count;
            for (int i = 0; i < vesselListCount; ++i)
            {
                if (vesselList[i].vessel.GetInstanceID () == vesselID)
                {
                    if (logFlightSetup) DebugLogWithID ("SetupReorderedForFlight", "Vessel " + vesselID + " found in the status list");
                    vesselListInclusive = true;
                    vesselStatusIndex = i;
                }
            }

            // If it was not included, we add it to the list
            // Correct index is then fairly obvious

            if (!vesselListInclusive)
            {
                if (logFlightSetup) DebugLogWithID ("SetupReorderedForFlight", "Vessel " + vesselID + " was not found in the status list, adding it");
                vesselList.Add (new VesselStatus (vessel, false));
                vesselStatusIndex = vesselList.Count - 1;
            }

            // Using the index for the status list we obtained, we check whether it was updated yet
            // So that only one part can run the following part

            if (!vesselList[vesselStatusIndex].isUpdated)
            {
                if (logFlightSetup) DebugLogWithID ("SetupReorderedForFlight", "Vessel " + vesselID + " was not updated yet (this message should only appear once)");
                vesselList[vesselStatusIndex].isUpdated = true;
                List<WingProcedural> moduleList = new List<WingProcedural> ();

                // First we get a list of all relevant parts in the vessel
                // Found modules are added to a list

                int vesselPartsCount = vessel.parts.Count;
                for (int i = 0; i < vesselPartsCount; ++i)
                {
                    if (vessel.parts[i].Modules.Contains ("WingProcedural"))
                        moduleList.Add ((WingProcedural) vessel.parts[i].Modules["WingProcedural"]);
                }

                // After that we make two separate runs through that list
                // First one setting up all geometry and second one setting up aerodynamic values

                if (logFlightSetup) DebugLogWithID ("SetupReorderedForFlight", "Vessel " + vesselID + " contained " + vesselPartsCount + " parts, of which " + moduleList.Count + " should be set up");
                int moduleListCount = moduleList.Count;
                for (int i = 0; i < moduleListCount; ++i)
                {
                    moduleList[i].Setup ();
                }

                yield return new WaitForFixedUpdate ();
                yield return new WaitForFixedUpdate ();

                if (logFlightSetup) DebugLogWithID ("SetupReorderedForFlight", "Vessel " + vesselID + " waited for updates, starting aero value calculation");
                for (int i = 0; i < moduleListCount; ++i)
                {
                    moduleList[i].CalculateAerodynamicValues ();
                }
                yield return null;
            }
        }




        // Aerodynamics value calculation
        // More or less lifted from pWings, so credit goes to DYJ and Taverius

        private bool aeroModelFAR = false;
        private bool aeroModelNEAR = false;
        private bool aeroModelFARMass = false;

        [KSPField] public float aeroConstLiftFudgeNumber = 0.0775f;
        [KSPField] public float aeroConstMassFudgeNumber = 0.015f;
        [KSPField] public float aeroConstDragBaseValue = 0.6f;
        [KSPField] public float aeroConstDragMultiplier = 3.3939f;
        [KSPField] public float aeroConstConnectionFactor = 150f;
        [KSPField] public float aeroConstConnectionMinimum = 50f;
        [KSPField] public float aeroConstCostDensity = 5300f;
        [KSPField] public float aeroConstCostDensityControl = 6500f;
        [KSPField] public float aeroConstControlSurfaceFraction = 1f;

        [KSPField (guiActiveEditor = false, guiName = "Coefficient of drag", guiFormat = "F3")]
        public float aeroUICd;

        [KSPField (guiActiveEditor = false, guiName = "Coefficient of lift", guiFormat = "F3")]
        public float aeroUICl;

        [KSPField (guiActiveEditor = false, guiName = "Mass", guiFormat = "F3", guiUnits = "t")]
        public float aeroUIMass;

        [KSPField (guiActiveEditor = false, guiName = "Cost")]
        public float aeroUICost;

        [KSPField (guiActiveEditor = false, guiName = "Mean aerodynamic chord", guiFormat = "F3", guiUnits = "m")]
        public float aeroUIMeanAerodynamicChord;

        [KSPField (guiActiveEditor = false, guiName = "Semispan", guiFormat = "F3", guiUnits = "m")]
        public float aeroUISemispan;

        [KSPField (guiActiveEditor = false, guiName = "Mid-chord wweep", guiFormat = "F3", guiUnits = "deg.")]
        public float aeroUIMidChordSweep;

        [KSPField (guiActiveEditor = false, guiName = "Taper ratio", guiFormat = "F3")]
        public float aeroUITaperRatio;

        [KSPField (guiActiveEditor = false, guiName = "Surface area", guiFormat = "F3", guiUnits = "m²")]
        public float aeroUISurfaceArea;

        [KSPField (guiActiveEditor = false, guiName = "Aspect ratio", guiFormat = "F3")]
        public float aeroUIAspectRatio;

        public double aeroStatCd;
        public double aeroStatCl;
        public double aeroStatClChildren;
        public double aeroStatMass;
        public double aeroStatConnectionForce;

        public double aeroStatMeanAerodynamicChord;
        public double aeroStatSemispan;
        public double aeroStatMidChordSweep;
        public double aeroStatTaperRatio;
        public double aeroStatSurfaceArea;
        public double aeroStatAspectRatio;
        public double aeroStatAspectRatioSweepScale;

        // TODO:
        // Take wing edge widths into account

        public void CalculateAerodynamicValues ()
        {
            if (isAttached || HighLogic.LoadedSceneIsFlight)
            {
                if (logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Started");
                CheckAssemblies (false);

                float wingWidthTipSum = wingWidthTip + wingEdgeWidthLeadingTip + wingEdgeWidthTrailingTip;
                float wingWidthRootSum = wingWidthRoot + wingEdgeWidthLeadingRoot + wingEdgeWidthTrailingRoot;

                float ctrlWidthTipSum = ctrlWidthTip + ctrlEdgeWidthTip;
                float ctrlWidthRootSum = ctrlWidthRoot + ctrlEdgeWidthRoot;

                float ctrlOffsetRootLimit = (ctrlSpan / 2f) / (ctrlWidthRoot + 1f);
                float ctrlOffsetTipLimit = (ctrlSpan / 2f) / (ctrlWidthTip + 1f);

                float ctrlOffsetRootClamped = Mathf.Clamp (ctrlOffsetRoot, -ctrlOffsetRootLimit, ctrlOffsetRootLimit);
                float ctrlOffsetTipClamped = Mathf.Clamp (ctrlOffsetTip, -ctrlOffsetTipLimit, ctrlOffsetTipLimit);

                // Base four values

                if (!isCtrlSrf)
                {
                    aeroStatSemispan = wingSpan;
                    aeroStatTaperRatio = (double) wingWidthTipSum / (double) wingWidthRootSum;
                    aeroStatMeanAerodynamicChord = (double) (wingWidthTipSum + wingWidthRootSum) / 2.0;
                    aeroStatMidChordSweep = MathD.Atan ((double) wingOffset / (double) wingSpan) * MathD.Rad2Deg;
                }
                else
                {
                    aeroStatSemispan = (double) ctrlSpan; // b_2 = 3.96 for SH-4
                    aeroStatTaperRatio = (double) (ctrlSpan + ctrlWidthTipSum * ctrlOffsetTipClamped - ctrlWidthRootSum * ctrlOffsetRootClamped) / (double) ctrlSpan; // (double) ctrlWidthTip / (double) ctrlWidthRoot;
                    aeroStatMeanAerodynamicChord = (double) (ctrlWidthTipSum + ctrlWidthRootSum) / 2.0; // (double) (ctrlWidthTip + ctrlWidthRoot) / 2.0;
                    aeroStatMidChordSweep = MathD.Atan ((double) Mathf.Abs (ctrlWidthRoot - ctrlWidthTip) / (double) ctrlSpan) * MathD.Rad2Deg;
                }
                if (logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Passed B2/TR/MAC/MCS");

                // Derived values

                aeroStatSurfaceArea = aeroStatMeanAerodynamicChord * aeroStatSemispan;
                aeroStatAspectRatio = 2.0f * aeroStatSemispan / aeroStatMeanAerodynamicChord;

                aeroStatAspectRatioSweepScale = MathD.Pow (aeroStatAspectRatio / MathD.Cos (MathD.Deg2Rad * aeroStatMidChordSweep), 2.0f) + 4.0f;
                aeroStatAspectRatioSweepScale = 2.0f + MathD.Sqrt (aeroStatAspectRatioSweepScale);
                aeroStatAspectRatioSweepScale = (2.0f * MathD.PI) / aeroStatAspectRatioSweepScale * aeroStatAspectRatio;

                aeroStatMass = MathD.Clamp (aeroConstMassFudgeNumber * aeroStatSurfaceArea * ((aeroStatAspectRatioSweepScale * 2.0) / (3.0 + aeroStatAspectRatioSweepScale)) * ((1.0 + aeroStatTaperRatio) / 2), 0.01, double.MaxValue);
                aeroStatCd = aeroConstDragBaseValue / aeroStatAspectRatioSweepScale * aeroConstDragMultiplier;
                aeroStatCl = aeroConstLiftFudgeNumber * aeroStatSurfaceArea * aeroStatAspectRatioSweepScale;
                aeroStatConnectionForce = MathD.Round (MathD.Clamp (MathD.Sqrt (aeroStatCl + aeroStatClChildren) * (double) aeroConstConnectionFactor, (double) aeroConstConnectionMinimum, double.MaxValue));
                if (logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Passed SR/AR/ARSS/mass/Cl/Cd/connection");

                // Shared parameters

                if (!isCtrlSrf)
                {
                    aeroUICost = (float) aeroStatMass * (1f + (float) aeroStatAspectRatioSweepScale / 4f) * aeroConstCostDensity;
                    aeroUICost = Mathf.Round (aeroUICost / 5f) * 5f;
                    part.CoMOffset = new Vector3 (wingSpan / 2f, -wingOffset / 2f, 0f);
                }
                else
                {
                    aeroUICost = (float) aeroStatMass * (1f + (float) aeroStatAspectRatioSweepScale / 4f) * aeroConstCostDensity * (1f - aeroConstControlSurfaceFraction);
                    aeroUICost += (float) aeroStatMass * (1f + (float) aeroStatAspectRatioSweepScale / 4f) * aeroConstCostDensityControl * aeroConstControlSurfaceFraction;
                    aeroUICost = Mathf.Round (aeroUICost / 5f) * 5f;
                    part.CoMOffset = new Vector3 (0f, -(ctrlWidthRoot + ctrlWidthTip) / 4f, 0f);
                }
                part.breakingForce = Mathf.Round ((float) aeroStatConnectionForce);
                part.breakingTorque = Mathf.Round ((float) aeroStatConnectionForce);
                if (logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Passed cost/force/torque");

                // Stock-only values

                if ((!aeroModelFAR && !aeroModelNEAR) || !aeroModelFARMass)
                {
                    if (logCAV) DebugLogWithID ("CalculateAerodynamicValues", "FAR/NEAR is inactive or FAR mass is not enabled, calculating stock part mass");
                    part.mass = Mathf.Round ((float) aeroStatMass * 100f) / 100f;
                }
                if (!aeroModelFAR && !aeroModelNEAR)
                {
                    if (!isCtrlSrf && !isWingAsCtrlSrf)
                    {
                        if (logCAV) DebugLogWithID ("CalculateAerodynamicValues", "FAR/NEAR is inactive, calculating values for winglet part type");
                        ((Winglet) this.part).deflectionLiftCoeff = Mathf.Round ((float) aeroStatCl * 100f) / 100f;
                        ((Winglet) this.part).dragCoeff = Mathf.Round ((float) aeroStatCd * 100f) / 100f;
                    }
                    else
                    {
                        if (logCAV) DebugLogWithID ("CalculateAerodynamicValues", "FAR/NEAR is inactive, calculating stock control surface module values");
                        var mCtrlSrf = part.Modules.OfType<ModuleControlSurface> ().FirstOrDefault ();
                        mCtrlSrf.deflectionLiftCoeff = Mathf.Round ((float) aeroStatCl * 100f) / 100f;
                        mCtrlSrf.dragCoeff = Mathf.Round ((float) aeroStatCd * 100f) / 100f;
                        mCtrlSrf.ctrlSurfaceArea = aeroConstControlSurfaceFraction;
                    }
                }
                if (logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Passed stock drag/deflection/area");

                // FAR values
                // With reflection stuff from r4m0n

                if (aeroModelFAR || aeroModelNEAR)
                {
                    if (logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Got into FAR/NEAR condition");
                    if (part.Modules.Contains ("FARControllableSurface"))
                    {
                        PartModule moduleFAR = part.Modules["FARControllableSurface"];
                        Type typeFAR = moduleFAR.GetType ();

                        typeFAR.GetField ("b_2").SetValue (moduleFAR, aeroStatSemispan); 
                        typeFAR.GetField ("MAC").SetValue (moduleFAR, aeroStatMeanAerodynamicChord);
                        typeFAR.GetField ("S").SetValue (moduleFAR, aeroStatSurfaceArea);
                        typeFAR.GetField ("MidChordSweep").SetValue (moduleFAR, aeroStatMidChordSweep);
                        typeFAR.GetField ("TaperRatio").SetValue (moduleFAR, aeroStatTaperRatio);
                        typeFAR.GetField ("ctrlSurfFrac").SetValue (moduleFAR, aeroConstControlSurfaceFraction);
                        if (aeroModelNEAR) typeFAR.GetMethod ("MathAndFunctionInitialization").Invoke (moduleFAR, null);
                        else typeFAR.GetMethod ("StartInitialization").Invoke (moduleFAR, null);
                    }
                    else if (part.Modules.Contains ("FARWingAerodynamicModel"))
                    {
                        PartModule moduleFAR = part.Modules["FARWingAerodynamicModel"];
                        Type typeFAR = moduleFAR.GetType ();

                        typeFAR.GetField ("b_2").SetValue (moduleFAR, aeroStatSemispan);
                        typeFAR.GetField ("MAC").SetValue (moduleFAR, aeroStatMeanAerodynamicChord);
                        typeFAR.GetField ("S").SetValue (moduleFAR, aeroStatSurfaceArea);
                        typeFAR.GetField ("MidChordSweep").SetValue (moduleFAR, aeroStatMidChordSweep);
                        typeFAR.GetField ("TaperRatio").SetValue (moduleFAR, aeroStatTaperRatio);
                        if (aeroModelNEAR) typeFAR.GetMethod ("MathAndFunctionInitialization").Invoke (moduleFAR, null);
                        else typeFAR.GetMethod ("StartInitialization").Invoke (moduleFAR, null);
                    }
                }
                if (logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Passed FAR/NEAR parameter setting");

                // Update GUI values and finish

                if (!aeroModelFAR && !aeroModelNEAR)
                {
                    aeroUICd = Mathf.Round ((float) aeroStatCd * 100f) / 100f;
                    aeroUICl = Mathf.Round ((float) aeroStatCl * 100f) / 100f;
                }
                if ((!aeroModelFAR && !aeroModelNEAR) || !aeroModelFARMass) aeroUIMass = part.mass;

                aeroUIMeanAerodynamicChord = (float) aeroStatMeanAerodynamicChord;
                aeroUISemispan = (float) aeroStatSemispan;
                aeroUIMidChordSweep = (float) aeroStatMidChordSweep;
                aeroUITaperRatio = (float) aeroStatTaperRatio;
                aeroUISurfaceArea = (float) aeroStatSurfaceArea;
                aeroUIAspectRatio = (float) aeroStatAspectRatio;
                if (HighLogic.LoadedSceneIsEditor) GameEvents.onEditorShipModified.Fire (EditorLogic.fetch.ship);
                if (logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Finished");
            }
        }

        private void UpdateCollidersForFAR ()
        {
            if (aeroModelFAR && aeroModelNEAR)
            {
                if (part.Modules.Contains ("FARWingAerodynamicModel"))
                {
                    PartModule moduleFAR = part.Modules["FARWingAerodynamicModel"];
                    Type typeFAR = moduleFAR.GetType ();
                    typeFAR.GetMethod ("TriggerPartColliderUpdate").Invoke (moduleFAR, null);
                }
            }
        }

        public void OnCenterOfLiftQuery (CenterOfLiftQuery qry)
        {
            if (isAttached && !aeroModelFAR)
            {
                qry.lift = (float) aeroStatCl;
            }
        }

        public void GatherChildrenCl ()
        {
            aeroStatClChildren = 0;

            // Add up the Cl and ChildrenCl of all our children to our ChildrenCl
            for (int i = 0; i < part.children.Count; ++i)
            {
                if (part.children[i].Modules.Contains ("WingProcedural"))
                {
                    var child = part.children[i].Modules.OfType<WingProcedural> ().FirstOrDefault ();
                    aeroStatClChildren += child.aeroStatCl;
                    aeroStatClChildren += child.aeroStatClChildren;
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
            if (isAttached && this.part.parent != null)
            {
                showWingData = !showWingData;
                if (showWingData) Events["InfoToggleEvent"].guiName = "Hide wing data";
                else Events["InfoToggleEvent"].guiName = "Show wing data";

                // If FAR/NEAR aren't present, toggle Cl/Cd
                if (!aeroModelFAR && !aeroModelNEAR)
                {
                    Fields["aeroUICd"].guiActiveEditor = showWingData;
                    Fields["aeroUICl"].guiActiveEditor = showWingData;
                }

                // If FAR|NEAR are not present, or its a version without wing mass calculations, toggle wing mass
                if ((!aeroModelFAR && !aeroModelNEAR) || !aeroModelFARMass)
                    Fields["aeroUIMass"].guiActive = showWingData;

                // Toggle the rest of the info values
                Fields["aeroUICost"].guiActiveEditor = showWingData;
                Fields["aeroUIMeanAerodynamicChord"].guiActiveEditor = showWingData;
                Fields["aeroUISemispan"].guiActiveEditor = showWingData;
                Fields["aeroUIMidChordSweep"].guiActiveEditor = showWingData;
                Fields["aeroUITaperRatio"].guiActiveEditor = showWingData;
                Fields["aeroUISurfaceArea"].guiActiveEditor = showWingData;
                Fields["aeroUIAspectRatio"].guiActiveEditor = showWingData;

                // Force tweakable window to refresh
                if (myWindow != null)
                    myWindow.displayDirty = true;
            }
        }




        [KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Dump interaction data")]
        public void DumpInteractionData ()
        {
            if (part.Modules.Contains ("FARWingAerodynamicModel"))
            {
                PartModule moduleFAR = part.Modules["FARWingAerodynamicModel"];
                Type typeFAR = moduleFAR.GetType ();

                var referenceInteraction = typeFAR.GetField ("wingInteraction", BindingFlags.Instance | BindingFlags.NonPublic).GetValue (moduleFAR);
                if (referenceInteraction != null)
                {
                    string report = "";
                    Type typeInteraction = referenceInteraction.GetType ();
                    Type runtimeListType = typeof (List<>).MakeGenericType (typeFAR);

                    FieldInfo forwardExposureInfo = typeInteraction.GetField ("forwardExposure", BindingFlags.NonPublic | BindingFlags.Instance);
                    double forwardExposure = (double) forwardExposureInfo.GetValue (referenceInteraction);
                    FieldInfo backwardExposureInfo = typeInteraction.GetField ("backwardExposure", BindingFlags.NonPublic | BindingFlags.Instance);
                    double backwardExposure = (double) backwardExposureInfo.GetValue (referenceInteraction);
                    FieldInfo leftwardExposureInfo = typeInteraction.GetField ("leftwardExposure", BindingFlags.NonPublic | BindingFlags.Instance);
                    double leftwardExposure = (double) leftwardExposureInfo.GetValue (referenceInteraction);
                    FieldInfo rightwardExposureInfo = typeInteraction.GetField ("rightwardExposure", BindingFlags.NonPublic | BindingFlags.Instance);
                    double rightwardExposure = (double) rightwardExposureInfo.GetValue (referenceInteraction);
                    report += "Exposure (fwd/back/left/right): " + forwardExposure.ToString ("F2") + ", " + backwardExposure.ToString ("F2") + ", " + leftwardExposure.ToString ("F2") + ", " + rightwardExposure.ToString ("F2");
                    DebugLogWithID ("DumpInteractionData", report);
                }
                else DebugLogWithID ("DumpInteractionData", "Interaction reference is null, report failed");
            }
            else DebugLogWithID ("DumpInteractionData", "FAR module not found, report failed");
        }




        // Alternative UI/input

        public KeyCode uiKeyCodeEdit = KeyCode.G;
        public KeyCode uiKeyCodeNext = KeyCode.J;
        public KeyCode uiKeyCodePrev = KeyCode.H;
        public KeyCode uiKeyCodeAdd = KeyCode.N;
        public KeyCode uiKeyCodeSubtract = KeyCode.B;

        public static Rect uiRect = new Rect ();
        public static bool uiStyleConfigured = false;
        public static bool uiWindowActive = true;
        public static float uiMouseDeltaCache = 0f;

        public static int uiInstanceIDTarget = 0;
        private int uiInstanceIDLocal = 0;

        public static int uiPropertySelectionWing = 0;
        public static int uiPropertySelectionSurface = 0;

        public static bool uiEditMode = false;
        public static bool uiEditModeTimeout = false;
        private float uiEditModeTimeoutDuration = 0.25f;
        private float uiEditModeTimer = 0f;

        public static GUIStyle uiStyleWindow = new GUIStyle ();
        public static GUIStyle uiStyleLabelMedium = new GUIStyle ();
        public static GUIStyle uiStyleLabelHint = new GUIStyle ();
        public static GUIStyle uiStyleButton = new GUIStyle ();

        private void OnDraw ()
        {
            if (uiInstanceIDLocal == 0) uiInstanceIDLocal = part.GetInstanceID ();
            if (uiInstanceIDTarget == uiInstanceIDLocal || uiInstanceIDTarget == 0)
            {
                if (uiWindowActive)
                {
                    uiRect = GUILayout.Window (273, uiRect, OnWindow, GetWindowTitle (), uiStyleWindow);
                    if (uiRect.x == 0f && uiRect.y == 0f) uiRect = uiRect.SetToScreenCenter ();
                }
            }
        }

        private void OnWindow (int window)
        {
            GUILayout.BeginHorizontal ();
            GUILayout.FlexibleSpace ();
            GUILayout.EndHorizontal ();
            if (uiEditMode)
            {
                if (!isCtrlSrf)
                {
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ("Editing: " + GetPropertyState (uiPropertySelectionWing), uiStyleLabelMedium);
                    if (GUILayout.Button ("Close", uiStyleButton, GUILayout.MaxWidth (50f))) uiWindowActive = false;
                    GUILayout.EndHorizontal ();
                    GUILayout.Label (GetPropertyDescription (uiPropertySelectionWing) + "\n", uiStyleLabelHint);
                }
                else
                {
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ("Editing: " + GetPropertyState (uiPropertySelectionSurface), uiStyleLabelMedium);
                    if (GUILayout.Button ("Close", uiStyleButton, GUILayout.MaxWidth (50f))) uiWindowActive = false;
                    GUILayout.EndHorizontal ();
                    GUILayout.Label (GetPropertyDescription (uiPropertySelectionSurface) + "\n", uiStyleLabelHint);
                }
                if (uiEditModeTimeout) GUILayout.Label ("Starting edit mode...", uiStyleLabelMedium);
                else GUILayout.Label ("G - exit edit mode\nH/J - switch between properties\nB/N or mouse - change the value", uiStyleLabelHint);
            }
            else
            {
                if (uiEditModeTimeout) GUILayout.Label ("Exiting edit mode...\n", uiStyleLabelMedium);
                else
                {
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ("Press G while pointing at a\nprocedural part to edit it", uiStyleLabelMedium);
                    if (GUILayout.Button ("Close", uiStyleButton, GUILayout.MaxWidth (50f))) uiWindowActive = false;
                    GUILayout.EndHorizontal ();
                }
            }
            GUI.DragWindow ();
        }

        private void InitStyle ()
        {
            uiStyleWindow = new GUIStyle (HighLogic.Skin.window);
            uiStyleWindow.fixedWidth = 250f;
            uiStyleWindow.wordWrap = true;

            uiStyleLabelMedium = new GUIStyle (HighLogic.Skin.label);
            uiStyleLabelMedium.stretchWidth = true;
            uiStyleLabelMedium.fontSize = 13;

            uiStyleLabelHint = new GUIStyle (HighLogic.Skin.label);
            uiStyleLabelHint.stretchWidth = true;
            uiStyleLabelHint.fontSize = 11;

            uiStyleButton = new GUIStyle (HighLogic.Skin.button);
            uiStyleButton.stretchWidth = true;
            uiStyleButton.fontSize = 11;
            uiStyleConfigured = true;
        }

        private void OnMouseOver ()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (this.part.parent != null && isAttached && !uiEditModeTimeout)
                {
                    if (uiEditMode)
                    {
                        if (Input.GetKeyDown (KeyCode.Mouse1))
                        {
                            uiEditMode = false;
                            uiEditModeTimeout = true;
                        }
                    }
                    else
                    {
                        if (Input.GetKeyDown (uiKeyCodeEdit))
                        {
                            uiInstanceIDTarget = part.GetInstanceID ();
                            uiEditMode = true;
                            uiEditModeTimeout = true;
                            uiWindowActive = true;
                        }
                    }
                }
            }
        }

        private void UpdateUI ()
        {
            // if (stockButton == null) OnStockButtonSetup ();
            if (uiInstanceIDLocal != uiInstanceIDTarget) return;
            if (uiEditModeTimeout)
            {
                uiEditModeTimer += Time.deltaTime;
                if (uiEditModeTimer > uiEditModeTimeoutDuration)
                {
                    uiEditModeTimeout = false;
                    uiEditModeTimer = 0.0f;
                }
            }
            else
            {
                if (uiEditMode)
                {
                    if (Input.GetKeyDown (uiKeyCodeEdit) || Input.GetKeyDown (KeyCode.Mouse0))
                    {
                        uiEditMode = false;
                        uiEditModeTimeout = true;
                        return;
                    }
                    if (Input.GetKeyDown (uiKeyCodeNext))
                    {
                        SwitchProperty (true);
                    }
                    else if (Input.GetKeyDown (uiKeyCodePrev))
                    {
                        SwitchProperty (false);
                    }
                    if (Input.GetKeyDown (uiKeyCodeAdd))
                    {
                        uiMouseDeltaCache = 1f;
                        AdjustSelectedProperty ();
                    }
                    else if (Input.GetKeyDown (uiKeyCodeSubtract))
                    {
                        uiMouseDeltaCache = -1f;
                        AdjustSelectedProperty ();
                    }
                    else
                    {
                        float uiMouseDelta = Input.GetAxis ("Mouse X");
                        if (uiMouseDelta != 0f)
                        {
                            uiMouseDeltaCache += uiMouseDelta;
                            AdjustSelectedProperty ();
                        }
                    }
                }
            }
        }

        private void AdjustSelectedProperty ()
        {
            int m = Mathf.RoundToInt (uiMouseDeltaCache);
            if (m != 0)
            {
                uiMouseDeltaCache = 0f;
                if (!isCtrlSrf)
                {
                    if (uiPropertySelectionWing == 0)       AdjustProperty (ref wingSpan, m, incrementMain, wingSpanLimits);
                    else if (uiPropertySelectionWing == 1)  AdjustProperty (ref wingWidthRoot, m, incrementMain, wingWidthLimits);
                    else if (uiPropertySelectionWing == 2)  AdjustProperty (ref wingWidthTip, m, incrementMain,  wingWidthLimits);
                    else if (uiPropertySelectionWing == 3)  AdjustProperty (ref wingOffset, m, incrementMain, wingOffsetLimits);
                    else if (uiPropertySelectionWing == 4)  AdjustProperty (ref wingThicknessRoot, m, incrementSmall, wingThicknessLimits);
                    else if (uiPropertySelectionWing == 5)  AdjustProperty (ref wingThicknessTip, m, incrementSmall, wingThicknessLimits);
                    else if (uiPropertySelectionWing == 6)  AdjustProperty (ref wingSurfaceTextureTop, m, incrementInt, wingTextureLimits);
                    else if (uiPropertySelectionWing == 7)  AdjustProperty (ref wingSurfaceTextureBottom, m, incrementInt, wingTextureLimits);
                    else if (uiPropertySelectionWing == 8)  AdjustProperty (ref wingEdgeTypeLeading, m, incrementInt, wingEdgeTypeLimits);
                    else if (uiPropertySelectionWing == 9)  AdjustProperty (ref wingEdgeTypeTrailing, m, incrementInt, wingEdgeTypeLimits);
                    else if (uiPropertySelectionWing == 10) AdjustProperty (ref wingEdgeTextureLeading, m, incrementInt, wingTextureLimits);
                    else if (uiPropertySelectionWing == 11) AdjustProperty (ref wingEdgeTextureTrailing, m, incrementInt, wingTextureLimits);
                    else if (uiPropertySelectionWing == 12) AdjustProperty (ref wingEdgeWidthLeadingRoot, m, incrementSmall, wingEdgeWidthLimits);
                    else if (uiPropertySelectionWing == 13) AdjustProperty (ref wingEdgeWidthLeadingTip, m, incrementSmall, wingEdgeWidthLimits);
                    else if (uiPropertySelectionWing == 14) AdjustProperty (ref wingEdgeWidthTrailingRoot, m, incrementSmall, wingEdgeWidthLimits);
                    else if (uiPropertySelectionWing == 15) AdjustProperty (ref wingEdgeWidthTrailingTip, m, incrementSmall, wingEdgeWidthLimits);
                }
                else
                {
                    if (uiPropertySelectionSurface == 0)       AdjustProperty (ref ctrlSpan, m, incrementMain, ctrlSpanLimits);
                    else if (uiPropertySelectionSurface == 1)  AdjustProperty (ref ctrlWidthRoot, m, incrementMain, ctrlWidthLimits);
                    else if (uiPropertySelectionSurface == 2)  AdjustProperty (ref ctrlWidthTip, m, incrementMain, ctrlWidthLimits);
                    else if (uiPropertySelectionSurface == 3)  AdjustProperty (ref ctrlEdgeWidthRoot, m, incrementSmall, ctrlEdgeWidthLimits);
                    else if (uiPropertySelectionSurface == 4)  AdjustProperty (ref ctrlEdgeWidthTip, m, incrementSmall, ctrlEdgeWidthLimits);
                    else if (uiPropertySelectionSurface == 5)  AdjustProperty (ref ctrlThicknessRoot, m, incrementSmall, ctrlThicknessLimits);
                    else if (uiPropertySelectionSurface == 6)  AdjustProperty (ref ctrlThicknessTip, m, incrementSmall, ctrlThicknessLimits);
                    else if (uiPropertySelectionSurface == 7)  AdjustProperty (ref ctrlOffsetRoot, m, incrementMain, ctrlOffsetLimits);
                    else if (uiPropertySelectionSurface == 8)  AdjustProperty (ref ctrlOffsetTip, m, incrementMain, ctrlOffsetLimits);
                    else if (uiPropertySelectionSurface == 9)  AdjustProperty (ref ctrlSurfaceTextureTop, m, incrementInt, ctrlTextureLimits);
                    else if (uiPropertySelectionSurface == 10) AdjustProperty (ref ctrlSurfaceTextureBottom, m, incrementInt, ctrlTextureLimits);
                    else if (uiPropertySelectionSurface == 11) AdjustProperty (ref ctrlEdgeTexture, m, incrementInt, ctrlTextureLimits);
                    else if (uiPropertySelectionSurface == 12) AdjustProperty (ref ctrlEdgeType, m, incrementInt, ctrlEdgeTypeLimits);
                }
            }
        }

        private void AdjustProperty (ref float field, float multiplier, float increment, Vector2 limits)
        {
            field = Mathf.Clamp (field + increment * multiplier, limits.x, limits.y);
        }

        private void SwitchProperty (bool forward)
        {
            int propertyShift = 1;
            if (!forward) propertyShift = -1;
            if (!isCtrlSrf)
            {
                if (forward && uiPropertySelectionWing == 15) uiPropertySelectionWing = 0;
                else if (!forward && uiPropertySelectionWing == 0) uiPropertySelectionWing = 15;
                else uiPropertySelectionWing = Mathf.Clamp (uiPropertySelectionWing + propertyShift, 0, 15);
            }
            else
            {
                if (forward && uiPropertySelectionSurface == 12) uiPropertySelectionSurface = 0;
                else if (!forward && uiPropertySelectionSurface == 0) uiPropertySelectionSurface = 12;
                else uiPropertySelectionSurface = Mathf.Clamp (uiPropertySelectionSurface + propertyShift, 0, 12);
            }
            if (logPropertyWindow) DebugLogWithID ("SwitchProperty", "Finished with following values | Wing: " + uiPropertySelectionWing + " | Surface: " + uiPropertySelectionSurface);
        }

        private string GetPropertyState (int id)
        {
            if (!isCtrlSrf && id <= 15 && id >= 0)
            {
                if (id == 0)       return "Semispan\n"            + wingSpan.ToString ("F3");
                else if (id == 1)  return "Width (root)\n"        + wingWidthRoot.ToString ("F3");
                else if (id == 2)  return "Width (tip)\n"         + wingWidthTip.ToString ("F3");
                else if (id == 3)  return "Offset\n"              + wingOffset.ToString ("F3");
                else if (id == 4)  return "Thickness (root)\n"    + wingThicknessRoot.ToString ("F2");
                else if (id == 5)  return "Thickness (tip)\n"     + wingThicknessTip.ToString ("F2");
                else if (id == 6)  return "Side A (material)\n"   + GetValueTranslationForMaterials (wingSurfaceTextureTop);
                else if (id == 7)  return "Side B (material)\n"   + GetValueTranslationForMaterials (wingSurfaceTextureBottom);
                else if (id == 8)  return "Edge L (shape)\n"      + GetValueTranslationForEdges (wingEdgeTypeLeading);
                else if (id == 9)  return "Edge T (shape)\n"      + GetValueTranslationForEdges (wingEdgeTypeTrailing);
                else if (id == 10) return "Edge L (material)\n"   + GetValueTranslationForMaterials (wingEdgeTextureLeading);
                else if (id == 11) return "Edge T (material)\n"   + GetValueTranslationForMaterials (wingEdgeTextureTrailing);
                else if (id == 12) return "Edge L (root width)\n" + wingEdgeWidthLeadingRoot.ToString ();
                else if (id == 13) return "Edge L (tip width)\n"  + wingEdgeWidthLeadingTip.ToString ();
                else if (id == 14) return "Edge T (root width)\n" + wingEdgeWidthTrailingRoot.ToString ();
                else               return "Edge T (tip width)\n"  + wingEdgeWidthTrailingTip.ToString ();
            }
            else if (isCtrlSrf && id <= 12 && id >= 0)
            {
                if (id == 0)       return "Length\n"             + ctrlSpan.ToString ("F3");
                else if (id == 1)  return "Width (main, root)\n" + ctrlWidthRoot.ToString ("F3");
                else if (id == 2)  return "Width (main, tip)\n"  + ctrlWidthTip.ToString ("F3");
                else if (id == 3)  return "Width (edge, root)\n" + ctrlEdgeWidthRoot.ToString ("F3");
                else if (id == 4)  return "Width (edge, tip)\n"  + ctrlEdgeWidthTip.ToString ("F3"); 
                else if (id == 5)  return "Thickness (root)\n"   + ctrlThicknessRoot.ToString ("F2");
                else if (id == 6)  return "Thickness (tip)\n"    + ctrlThicknessTip.ToString ("F2");
                else if (id == 7)  return "Offset R\n"           + ctrlOffsetRoot.ToString ("F3");
                else if (id == 8)  return "Offset T\n"           + ctrlOffsetTip.ToString ("F3");
                else if (id == 9)  return "Side A (material)\n"  + GetValueTranslationForMaterials (ctrlSurfaceTextureTop);
                else if (id == 10) return "Side B (material)\n"  + GetValueTranslationForMaterials (ctrlSurfaceTextureBottom);
                else if (id == 11) return "Edge (material)\n"    + GetValueTranslationForMaterials (ctrlEdgeTexture);
                else               return "Edge (shape)\n"       + GetValueTranslationForEdges (ctrlEdgeType);
            }
            else return "Invalid property ID";
        }

        private string GetPropertyDescription (int id)
        {
            if (!isCtrlSrf && id <= 15 && id >= 0)
            {
                if (id == 0)       return "Lateral measurement of the wing";
                else if (id == 1)  return "Longitudinal measurement of the wing \nat the root cross section";
                else if (id == 2)  return "Longitudinal measurement of the wing \nat the tip cross section";
                else if (id == 3)  return "Distance between midpoints of the cross \nsections on the longitudinal axis";
                else if (id == 4)  return "Thickness at the root cross section";
                else if (id == 5)  return "Thickness at the tip cross section";
                else if (id == 6)  return "Material of the wing surface A \n(usually it's the one on top)";
                else if (id == 7)  return "Material of the wing surface B \n(usually it's the bottom one)";
                else if (id == 8)  return "Leading edge cross section shape";
                else if (id == 9)  return "Trailing edge cross section shape";
                else if (id == 10) return "Leading edge material";
                else if (id == 11) return "Trailing edge material";
                else if (id == 12) return "Leading edge width at the root cross \nsection on the longitudinal axis";
                else if (id == 13) return "Leading edge width at the tip cross \nsection on the longitudinal axis";
                else if (id == 14) return "Trailing edge width at the root cross \nsection on the longitudinal axis";
                else               return "Trailing edge width at the tip cross \nsection on the longitudinal axis";
            }
            else if (isCtrlSrf && id <= 12 && id >= 0)
            {
                if (id == 0)       return "Lateral measurement of the root \ncross section of the control surface";
                else if (id == 1)  return "Longitudinal measurement of the surface \nat the left (root) cross section";
                else if (id == 2)  return "Longitudinal measurement of the surface \nat the right (tip) cross section";
                else if (id == 3)  return "Longitudinal measurement of the edge \nat the left (root) cross section";
                else if (id == 4)  return "Longitudinal measurement of the edge \nat the right (tip) cross section";
                else if (id == 5)  return "Thickness at the left cross section";
                else if (id == 6)  return "Thickness at the right cross section";
                else if (id == 7)  return "Offset of the trailing edge left corner \non the lateral axis";
                else if (id == 8)  return "Offset of the trailing edge right corner \non the lateral axis";
                else if (id == 9)  return "Material of the flat surface A \n(typically top of the control surface)";
                else if (id == 10) return "Material of the flat surface B \n(typically bottom of the control surface)";
                else if (id == 11) return "Material of the trailing edge";
                else               return "Trailing edge cross section shape";
            }
            else return "Invalid property ID";
        }

        private string GetValueTranslationForMaterials (float value)
        {
            if (value == 0f)      return "Uniform coating";
            else if (value == 1f) return "Standard alloys";
            else if (value == 2f) return "Reinforced composites";
            else if (value == 3f) return "LRSI thermal protection";
            else if (value == 4f) return "HRSI thermal protection";
            else return "Unknown material";
        }
        private string GetValueTranslationForEdges (float value)
        {
            if (value == 1f) return "No edge";
            else if (value == 2f) return "Rounded cross section";
            else if (value == 3f) return "Biconvex cross section";
            else if (value == 4f) return "Triangular cross section";
            else return "Unknown shape";
        }

        private string GetWindowTitle ()
        {
            if (uiEditMode)
            {
                if (!isCtrlSrf)
                {
                    if (isWingAsCtrlSrf) return "All-moving control surface";
                    else return "Wing";
                }
                else return "Control surface";
            }
            else return "Inactive";
        }




        // Saving/loading

        public override void OnSave (ConfigNode node)
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<WingProcedural> ();
            config.SetValue ("uiRect", uiRect);
            config.save ();
        }

        public override void OnLoad (ConfigNode node)
        {
            PluginConfiguration config = PluginConfiguration.CreateForType<WingProcedural> ();
            config.load ();
            uiRect = config.GetValue<Rect> ("uiRect");
        }




        // Stock toolbar integration

        //public static ApplicationLauncherButton stockButton = null;

        //private void OnStockButtonSetup ()
        //{
        //    stockButton = ApplicationLauncher.Instance.AddModApplication (OnStockButtonClick, OnStockButtonClick, OnStockButtonVoid, OnStockButtonVoid, OnStockButtonVoid, OnStockButtonVoid, ApplicationLauncher.AppScenes.SPH, (Texture) GameDatabase.Instance.GetTexture ("B9_Aerospace/Plugins/icon_stock", false));
        //}

        //public void OnStockButtonClick ()
        //{
        //    uiWindowActive = !uiWindowActive;
        //}

        //private void OnStockButtonVoid ()
        //{

        //}

        //public void OnDestroy ()
        //{
        //    bool stockButtonRemoved = true;
        //    ConfigNode[] nodes = GameDatabase.Instance ("");
        //    for (int i = 0; i < nodes.Length; ++i)
        //    {
        //        if (nodes[i] == null) continue;
        //        if (nodes[i].HasValue ("massPerWingAreaSupported")) aeroModelFARMass = true;
        //    }
        //    if (stockButtonRemoved) ApplicationLauncher.Instance.RemoveModApplication (stockButton);
        //}
    }
}
