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
    // TODO
    // Default edge ID for surface is incorrect due to shifted limits
    // Wing/edge limit difference assignment isn't working
    // Alternative UI
    
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

        public MeshFilter meshFilterWingSection;
        public MeshFilter meshFilterWingSurface;
        public List<MeshFilter> meshFiltersWingEdgeTrailing = new List<MeshFilter> ();
        public List<MeshFilter> meshFiltersWingEdgeLeading = new List<MeshFilter> ();

        public MeshFilter meshFilterCtrlFrame;
        public MeshFilter meshFilterCtrlSurface;
        public List<MeshFilter> meshFiltersCtrlEdge = new List<MeshFilter> ();

        public static MeshReference meshReferenceWingSection;
        public static MeshReference meshReferenceWingSurface;
        public static List<MeshReference> meshReferencesWingEdge = new List<MeshReference> ();

        public static MeshReference meshReferenceCtrlFrame;
        public static MeshReference meshReferenceCtrlSurface;
        public static List<MeshReference> meshReferencesCtrlEdge = new List<MeshReference> ();

        private int meshTypeCountEdgeWing = 4;
        private int meshTypeCountEdgeCtrl = 3;





        // Shared properties

        private Vector2 sharedLengthLimits = new Vector2 (0.25f, 16f);
        private Vector2 sharedThicknessLimits = new Vector2 (0.08f, 1f);
        private Vector2 sharedWidthLimitsWing = new Vector2 (0.25f, 16f);
        private Vector2 sharedWidthLimitsCtrl = new Vector2 (0.25f, 1.5f);
        private Vector2 sharedOffsetLimitsWing = new Vector2 (-8f, 8f);
        private Vector2 sharedOffsetLimitsCtrl = new Vector2 (-2f, 2f);
        private Vector2 sharedEdgeTypeLimitsWing = new Vector2 (1f, 4f);
        private Vector2 sharedEdgeTypeLimitsCtrl = new Vector2 (1f, 3f);
        private Vector2 sharedEdgeWidthLimits = new Vector2 (0f, 1f);
        private Vector2 sharedMaterialLimits = new Vector2 (0f, 4f);
        private Vector2 sharedColorLimits = new Vector2 (0f, 1f);

        private float sharedIncrementColor = 0.01f;
        private float sharedIncrementMain = 0.125f;
        private float sharedIncrementSmall = 0.04f;
        private float sharedIncrementInt = 1f;




        // Shared properties / Base

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Base"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupBase = false;
        public bool sharedFieldGroupBaseCached = false;
        public static bool sharedFieldGroupBaseStatic = false;
        private static string[] sharedFieldGroupBaseArray = new string[] { "sharedBaseLength", "sharedBaseWidthRoot", "sharedBaseWidthTip", "sharedBaseThicknessRoot", "sharedBaseThicknessTip", "sharedBaseOffsetTip" };
        private static string[] sharedFieldGroupBaseArrayCtrl = new string[] { "sharedBaseOffsetRoot" };

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Length", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float sharedBaseLength = 4f;
        public float sharedBaseLengthCached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Width (root)", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float sharedBaseWidthRoot = 4f;
        public float sharedBaseWidthRootCached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Width (tip)", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float sharedBaseWidthTip = 4f;
        public float sharedBaseWidthTipCached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Offset (root)", guiFormat = "S4"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = -2.5f, maxValue = 2.5f, incrementSlide = 0.125f)]
        public float sharedBaseOffsetRoot = 0f;
        public float sharedBaseOffsetRootCached = 0f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Offset (tip)", guiFormat = "S4"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = -2.5f, maxValue = 2.5f, incrementSlide = 0.125f)]
        public float sharedBaseOffsetTip = 0f;
        public float sharedBaseOffsetTipCached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Thickness (root)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.08f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedBaseThicknessRoot = 0.24f;
        public float sharedBaseThicknessRootCached = 0.24f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Thickness (tip)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.08f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedBaseThicknessTip = 0.24f;
        public float sharedBaseThicknessTipCached = 0.24f;




        // Wing properties / Leading edge

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Lead. edge"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupEdgeLeading = false;
        public bool sharedFieldGroupEdgeLeadingCached = false;
        public static bool sharedFieldGroupEdgeLeadingStatic = false;
        private static string[] sharedFieldGroupEdgeLeadingArray = new string[] { "sharedEdgeTypeLeading", "sharedEdgeWidthLeadingRoot", "sharedEdgeWidthLeadingTip" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Shape", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 1f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedEdgeTypeLeading = 2f;
        public float sharedEdgeTypeLeadingCached = 2f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (root)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedEdgeWidthLeadingRoot = 0.24f;
        public float sharedEdgeWidthLeadingRootCached = 0.24f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (tip)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedEdgeWidthLeadingTip = 0.24f;
        public float sharedEdgeWidthLeadingTipCached = 0.24f;




        // Wind properties / Trailing edge

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Trail. edge"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupEdgeTrailing = false;
        public bool sharedFieldGroupEdgeTrailingCached = false;
        public static bool sharedFieldGroupEdgeTrailingStatic = false;
        private static string[] sharedFieldGroupEdgeTrailingArray = new string[] { "sharedEdgeTypeTrailing", "sharedEdgeWidthTrailingRoot", "sharedEdgeWidthTrailingTip" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Shape", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 1f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedEdgeTypeTrailing = 3f;
        public float sharedEdgeTypeTrailingCached = 3f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (root)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedEdgeWidthTrailingRoot = 0.48f;
        public float sharedEdgeWidthTrailingRootCached = 0.48f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (tip)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedEdgeWidthTrailingTip = 0.48f;
        public float sharedEdgeWidthTrailingTipCached = 0.48f;




        // Shared properties / Surface / Top

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Material A"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupColorST = false;
        public bool sharedFieldGroupColorSTCached = false;
        public bool sharedFieldGroupColorSTStatic = false;
        private static string[] sharedFieldGroupColorSTArray = new string[] { "sharedMaterialST", "sharedColorSTOpacity", "sharedColorSTHue", "sharedColorSTSaturation", "sharedColorSTBrightness" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Material", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedMaterialST = 1f;
        public float sharedMaterialSTCached = 1f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Opacity", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSTOpacity = 0f;
        public float sharedColorSTOpacityCached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (H)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSTHue = 0.10f;
        public float sharedColorSTHueCached = 0.10f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (S)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSTSaturation = 0.75f;
        public float sharedColorSTSaturationCached = 0.75f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (B)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSTBrightness = 0.6f;
        public float sharedColorSTBrightnessCached = 0.6f;




        // Shared properties / Surface / bottom

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Material B"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupColorSB = false;
        public bool sharedFieldGroupColorSBCached = false;
        public bool sharedFieldGroupColorSBStatic = false;
        private static string[] sharedFieldGroupColorSBArray = new string[] { "sharedMaterialSB", "sharedColorSBOpacity", "sharedColorSBHue", "sharedColorSBSaturation", "sharedColorSBBrightness" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Material", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedMaterialSB = 4f;
        public float sharedMaterialSBCached = 4f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Opacity", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSBOpacity = 0f;
        public float sharedColorSBOpacityCached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (H)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSBHue = 0.10f;
        public float sharedColorSBHueCached = 0.10f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (S)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSBSaturation = 0.75f;
        public float sharedColorSBSaturationCached = 0.75f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (B)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSBBrightness = 0.6f;
        public float sharedColorSBBrightnessCached = 0.6f;




        // Shared properties / Surface / trailing edge

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Material T"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupColorET = false;
        public bool sharedFieldGroupColorETCached = false;
        public bool sharedFieldGroupColorETStatic = false;
        private static string[] sharedFieldGroupColorETArray = new string[] { "sharedMaterialET", "sharedColorETOpacity", "sharedColorETHue", "sharedColorETSaturation", "sharedColorETBrightness" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Material", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedMaterialET = 4f;
        public float sharedMaterialETCached = 4f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Opacity", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorETOpacity = 0f;
        public float sharedColorETOpacityCached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (H)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorETHue = 0.10f;
        public float sharedColorETHueCached = 0.10f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (S)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorETSaturation = 0.75f;
        public float sharedColorETSaturationCached = 0.75f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (B)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorETBrightness = 0.6f;
        public float sharedColorETBrightnessCached = 0.6f;




        // Shared properties / Surface / leading edge

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Material L"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupColorEL = false;
        public bool sharedFieldGroupColorELCached = false;
        public bool sharedFieldGroupColorELStatic = false;
        private static string[] sharedFieldGroupColorELArray = new string[] { "sharedMaterialEL", "sharedColorELOpacity", "sharedColorELHue", "sharedColorELSaturation", "sharedColorELBrightness" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Material", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedMaterialEL = 4f;
        public float sharedMaterialELCached = 4f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Opacity", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorELOpacity = 0f;
        public float sharedColorELOpacityCached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (H)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorELHue = 0.10f;
        public float sharedColorELHueCached = 0.10f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (S)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorELSaturation = 0.75f;
        public float sharedColorELSaturationCached = 0.75f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (B)", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorELBrightness = 0.6f;
        public float sharedColorELBrightnessCached = 0.6f;




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

        private bool logCAV = true;
        private bool logUpdate = false;
        private bool logUpdateGeometry = false;
        private bool logUpdateMaterials = false;
        private bool logMeshReferences = false;
        private bool logCheckMeshFilter = false;
        private bool logPropertyWindow = false;
        private bool logFlightSetup = true;




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
        private bool updateRequiredOnAerodynamics = false;
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

                    CheckFieldGroup (sharedFieldGroupBase, ref sharedFieldGroupBaseCached, ref sharedFieldGroupBaseStatic, sharedFieldGroupBaseArray, false, groupEntriesCtrl: sharedFieldGroupBaseArrayCtrl);
                    CheckFieldValue (sharedBaseLength, ref sharedBaseLengthCached, true);
                    CheckFieldValue (sharedBaseWidthRoot, ref sharedBaseWidthRootCached, true);
                    CheckFieldValue (sharedBaseWidthTip, ref sharedBaseWidthTipCached, true);
                    CheckFieldValue (sharedBaseThicknessRoot, ref sharedBaseThicknessRootCached, true);
                    CheckFieldValue (sharedBaseThicknessTip, ref sharedBaseThicknessTipCached, true);
                    CheckFieldValue (sharedBaseOffsetRoot, ref sharedBaseOffsetRootCached, true);
                    CheckFieldValue (sharedBaseOffsetTip, ref sharedBaseOffsetTipCached, true);

                    CheckFieldGroup (sharedFieldGroupEdgeTrailing, ref sharedFieldGroupEdgeTrailingCached, ref sharedFieldGroupEdgeTrailingStatic, sharedFieldGroupEdgeTrailingArray, false);
                    CheckFieldValue (sharedEdgeTypeTrailing, ref sharedEdgeTypeTrailingCached, false);
                    CheckFieldValue (sharedEdgeWidthTrailingRoot, ref sharedEdgeWidthTrailingRootCached, true);
                    CheckFieldValue (sharedEdgeWidthTrailingTip, ref sharedEdgeWidthTrailingTipCached, true);

                    CheckFieldGroup (sharedFieldGroupEdgeLeading, ref sharedFieldGroupEdgeLeadingCached, ref sharedFieldGroupEdgeLeadingStatic, sharedFieldGroupEdgeLeadingArray, false);
                    CheckFieldValue (sharedEdgeTypeLeading, ref sharedEdgeTypeLeadingCached, false);
                    CheckFieldValue (sharedEdgeWidthLeadingRoot, ref sharedEdgeWidthLeadingRootCached, true);
                    CheckFieldValue (sharedEdgeWidthLeadingTip, ref sharedEdgeWidthLeadingTipCached, true);

                    CheckFieldGroup (sharedFieldGroupColorST, ref sharedFieldGroupColorSTCached, ref sharedFieldGroupColorSTStatic, sharedFieldGroupColorSTArray, false);
                    CheckFieldValue (sharedMaterialST, ref sharedMaterialSTCached, false);
                    CheckFieldValue (sharedColorSTOpacity, ref sharedColorSTOpacityCached, false);
                    CheckFieldValue (sharedColorSTHue, ref sharedColorSTHueCached, false);
                    CheckFieldValue (sharedColorSTSaturation, ref sharedColorSTSaturationCached, false);
                    CheckFieldValue (sharedColorSTBrightness, ref sharedColorSTBrightnessCached, false);

                    CheckFieldGroup (sharedFieldGroupColorSB, ref sharedFieldGroupColorSBCached, ref sharedFieldGroupColorSBStatic, sharedFieldGroupColorSBArray, false);
                    CheckFieldValue (sharedMaterialSB, ref sharedMaterialSBCached, false);
                    CheckFieldValue (sharedColorSBOpacity, ref sharedColorSBOpacityCached, false);
                    CheckFieldValue (sharedColorSBHue, ref sharedColorSBHueCached, false);
                    CheckFieldValue (sharedColorSBSaturation, ref sharedColorSBSaturationCached, false);
                    CheckFieldValue (sharedColorSBBrightness, ref sharedColorSBBrightnessCached, false);

                    CheckFieldGroup (sharedFieldGroupColorET, ref sharedFieldGroupColorETCached, ref sharedFieldGroupColorETStatic, sharedFieldGroupColorETArray, false);
                    CheckFieldValue (sharedMaterialET, ref sharedMaterialETCached, false);
                    CheckFieldValue (sharedColorETOpacity, ref sharedColorETOpacityCached, false);
                    CheckFieldValue (sharedColorETHue, ref sharedColorETHueCached, false);
                    CheckFieldValue (sharedColorETSaturation, ref sharedColorETSaturationCached, false);
                    CheckFieldValue (sharedColorETBrightness, ref sharedColorETBrightnessCached, false);

                    CheckFieldGroup (sharedFieldGroupColorEL, ref sharedFieldGroupColorELCached, ref sharedFieldGroupColorELStatic, sharedFieldGroupColorELArray, false);
                    CheckFieldValue (sharedMaterialEL, ref sharedMaterialELCached, false);
                    CheckFieldValue (sharedColorELOpacity, ref sharedColorELOpacityCached, false);
                    CheckFieldValue (sharedColorELHue, ref sharedColorELHueCached, false);
                    CheckFieldValue (sharedColorELSaturation, ref sharedColorELSaturationCached, false);
                    CheckFieldValue (sharedColorELBrightness, ref sharedColorELBrightnessCached, false);

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
                        UpdateGeometry (updateRequiredOnAerodynamics);
                        updateRequiredOnAerodynamics = false;
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

        private void CheckFieldValue (float fieldValue, ref float fieldCache, bool affectsAerodynamics)
        {
            if (fieldValue != fieldCache)
            {
                if (logUpdate) DebugLogWithID ("Update", "Detected value change");
                updateRequiredOnGeometry = true;
                if (affectsAerodynamics) updateRequiredOnAerodynamics = true;
                fieldCache = fieldValue;
            }
        }

        private void CheckFieldGroup (bool groupStatus, ref bool groupCache, ref bool groupStatic, string[] groupEntries, bool skipCheck, string[] groupEntriesWing = null, string[] groupEntriesCtrl = null) 
        {
            if (!skipCheck)
            {
                if (groupStatus != groupCache)
                {
                    if (logUpdate) DebugLogWithID ("Update", "Detected field group state change");
                    for (int i = 0; i < groupEntries.Length; ++i) SetFieldVisibility (groupEntries[i], groupStatus);
                    if (!isCtrlSrf && groupEntriesWing != null) { for (int i = 0; i < groupEntriesWing.Length; ++i) SetFieldVisibility (groupEntriesWing[i], groupStatus); }
                    if (isCtrlSrf && groupEntriesCtrl != null) { for (int i = 0; i < groupEntriesCtrl.Length; ++i) SetFieldVisibility (groupEntriesCtrl[i], groupStatus); }
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

        public void UpdateGeometry (bool updateAerodynamics)
        {
            if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Started | isCtrlSrf: " + isCtrlSrf);
            if (!isCtrlSrf)
            {
                float wingThicknessDeviationRoot = sharedBaseThicknessRoot / 0.24f;
                float wingThicknessDeviationTip = sharedBaseThicknessTip / 0.24f;
                float wingWidthTipBasedOffsetTrailing = sharedBaseWidthTip / 2f + sharedBaseOffsetTip;
                float wingWidthTipBasedOffsetLeading = -sharedBaseWidthTip / 2f + sharedBaseOffsetTip;
                float wingWidthRootBasedOffset = sharedBaseWidthRoot / 2f;

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
                                vp[i] = new Vector3 (-sharedBaseLength, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetLeading);
                                uv[i] = new Vector2 (sharedBaseWidthTip, uv[i].y);
                            }
                            else
                            {
                                vp[i] = new Vector3 (-sharedBaseLength, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetTrailing);
                                uv[i] = new Vector2 (0f, uv[i].y);
                            }
                        }
                        else
                        {
                            if (vp[i].z < 0f)
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y * wingThicknessDeviationRoot, -wingWidthRootBasedOffset);
                                uv[i] = new Vector2 (sharedBaseWidthRoot, uv[i].y);
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
                    Vector2[] uv2 = new Vector2[length];

                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing surface top | Passed array setup");
                    for (int i = 0; i < length; ++i)
                    {
                        // Root/tip filtering followed by leading/trailing filtering
                        if (vp[i].x < -0.05f)
                        {
                            if (vp[i].z < 0f)
                            {
                                vp[i] = new Vector3 (-sharedBaseLength, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetLeading);
                                uv[i] = new Vector2 (sharedBaseLength / 4f, 1f - 0.5f + sharedBaseWidthTip / 8f - sharedBaseOffsetTip / 4f);
                            }
                            else
                            {
                                vp[i] = new Vector3 (-sharedBaseLength, vp[i].y * wingThicknessDeviationTip, wingWidthTipBasedOffsetTrailing);
                                uv[i] = new Vector2 (sharedBaseLength / 4f, 0f + 0.5f - sharedBaseWidthTip / 8f - sharedBaseOffsetTip / 4f);
                            }
                        }
                        else
                        {
                            if (vp[i].z < 0f)
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y * wingThicknessDeviationRoot, -wingWidthRootBasedOffset);
                                uv[i] = new Vector2 (0.0f, 1f - 0.5f + sharedBaseWidthRoot / 8f);
                            }
                            else
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y * wingThicknessDeviationRoot, wingWidthRootBasedOffset);
                                uv[i] = new Vector2 (0f, 0f + 0.5f - sharedBaseWidthRoot / 8f);
                            }
                        }

                        // Top/bottom filtering
                        if (vp[i].y > 0f)
                        {
                            cl[i] = GetVertexColor (0);
                            uv2[i] = GetVertexUV2 (sharedMaterialST);
                        }
                        else
                        {
                            cl[i] = GetVertexColor (1);
                            uv2[i] = GetVertexUV2 (sharedMaterialSB);
                        }
                    }

                    meshFilterWingSurface.mesh.vertices = vp;
                    meshFilterWingSurface.mesh.uv = uv;
                    meshFilterWingSurface.mesh.uv2 = uv2;
                    meshFilterWingSurface.mesh.colors = cl;
                    meshFilterWingSurface.mesh.RecalculateBounds ();
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing surface | Finished");
                }

                // Next, time for leading and trailing edges
                // Before modifying geometry, we have to show the correct objects for the current selection
                // As UI only works with floats, we have to cast selections into ints too

                int wingEdgeTypeTrailingInt = Mathf.RoundToInt (sharedEdgeTypeTrailing - 1);
                int wingEdgeTypeLeadingInt = Mathf.RoundToInt (sharedEdgeTypeLeading - 1);

                for (int i = 0; i < meshTypeCountEdgeWing; ++i)
                {
                    if (i != wingEdgeTypeTrailingInt) meshFiltersWingEdgeTrailing[i].gameObject.SetActive (false);
                    else meshFiltersWingEdgeTrailing[i].gameObject.SetActive (true);

                    if (i != wingEdgeTypeLeadingInt) meshFiltersWingEdgeLeading[i].gameObject.SetActive (false);
                    else meshFiltersWingEdgeLeading[i].gameObject.SetActive (true);
                }

                // Next we calculate some values reused for all edge geometry

                float wingEdgeWidthLeadingRootDeviation = sharedEdgeWidthLeadingRoot / 0.24f;
                float wingEdgeWidthLeadingTipDeviation = sharedEdgeWidthLeadingTip / 0.24f;

                float wingEdgeWidthTrailingRootDeviation = sharedEdgeWidthTrailingRoot / 0.24f;
                float wingEdgeWidthTrailingTipDeviation = sharedEdgeWidthTrailingTip / 0.24f;

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
                    Vector2[] uv2 = new Vector2[length];

                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge trailing | Passed array setup");
                    for (int i = 0; i < vp.Length; ++i)
                    {
                        if (vp[i].x < -0.1f)
                        {
                            vp[i] = new Vector3 (-sharedBaseLength, vp[i].y * wingThicknessDeviationTip, vp[i].z * wingEdgeWidthTrailingTipDeviation + sharedBaseWidthTip / 2f + sharedBaseOffsetTip); // Tip edge
                            if (nm[i].x == 0f) uv[i] = new Vector2 (sharedBaseLength, uv[i].y);
                        }
                        else vp[i] = new Vector3 (0f, vp[i].y * wingThicknessDeviationRoot, vp[i].z * wingEdgeWidthTrailingRootDeviation + sharedBaseWidthRoot / 2f); // Root edge
                        if (nm[i].x == 0f && sharedEdgeTypeTrailing != 1)
                        {
                            cl[i] = GetVertexColor (2);
                            uv2[i] = GetVertexUV2 (sharedMaterialET);
                        }
                    }

                    meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt].mesh.vertices = vp;
                    meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt].mesh.uv = uv;
                    meshFiltersWingEdgeTrailing[wingEdgeTypeTrailingInt].mesh.uv2 = uv2;
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
                    Vector2[] uv2 = new Vector2[length];

                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge leading | Passed array setup");
                    for (int i = 0; i < vp.Length; ++i)
                    {
                        if (vp[i].x < -0.1f)
                        {
                            vp[i] = new Vector3 (-sharedBaseLength, vp[i].y * wingThicknessDeviationTip, vp[i].z * wingEdgeWidthLeadingTipDeviation + sharedBaseWidthTip / 2f - sharedBaseOffsetTip); // Tip edge
                            if (nm[i].x == 0f) uv[i] = new Vector2 (sharedBaseLength, uv[i].y);
                        }
                        else vp[i] = new Vector3 (0f, vp[i].y * wingThicknessDeviationRoot, vp[i].z * wingEdgeWidthLeadingRootDeviation + sharedBaseWidthRoot / 2f); // Root edge
                        if (nm[i].x == 0f && sharedEdgeTypeLeading != 1)
                        {
                            cl[i] = GetVertexColor (3);
                            uv2[i] = GetVertexUV2 (sharedMaterialEL);
                        }
                    }

                    meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.vertices = vp;
                    meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.uv = uv;
                    meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.uv2 = uv2;
                    meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.colors = cl;
                    meshFiltersWingEdgeLeading[wingEdgeTypeLeadingInt].mesh.RecalculateBounds ();
                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge leading | Finished");
                }
            }
            else
            {
                // Some reusable values

                float ctrlOffsetRootLimit = (sharedBaseLength / 2f) / (sharedBaseWidthRoot + sharedEdgeWidthTrailingRoot);
                float ctrlOffsetTipLimit = (sharedBaseLength / 2f) / (sharedBaseWidthTip + sharedEdgeWidthTrailingTip);

                float ctrlOffsetRootClamped = Mathf.Clamp (sharedBaseOffsetRoot, -ctrlOffsetRootLimit, ctrlOffsetRootLimit);
                float ctrlOffsetTipClamped = Mathf.Clamp (sharedBaseOffsetTip, -ctrlOffsetTipLimit, ctrlOffsetTipLimit);

                float ctrlThicknessDeviationRoot = sharedBaseThicknessRoot / 0.24f;
                float ctrlThicknessDeviationTip = sharedBaseThicknessTip / 0.24f;

                float ctrlEdgeWidthDeviationRoot = sharedEdgeWidthTrailingRoot / 0.24f;
                float ctrlEdgeWidthDeviationTip = sharedEdgeWidthTrailingTip / 0.24f;

                // float widthDifference = sharedBaseWidthRoot - sharedBaseWidthTip;
                // float edgeLengthTrailing = Mathf.Sqrt (Mathf.Pow (sharedBaseLength, 2) + Mathf.Pow (widthDifference, 2));
                // float sweepTrailing = 90f - Mathf.Atan (sharedBaseLength / widthDifference) * Mathf.Rad2Deg;

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
                    Vector2[] uv2 = new Vector2[length];

                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface frame | Passed array setup");
                    for (int i = 0; i < vp.Length; ++i)
                    {
                        // Span-based shift & thickness correction
                        if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationTip, vp[i].y, vp[i].z + 0.5f - sharedBaseLength / 2f);
                        else vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationRoot, vp[i].y, vp[i].z - 0.5f + sharedBaseLength / 2f);

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
                                        vp[i] = new Vector3 (vp[i].x, -sharedBaseWidthTip, vp[i].z);
                                        uv[i] = new Vector2 (sharedBaseWidthTip, uv[i].y);
                                    }
                                    else
                                    {
                                        vp[i] = new Vector3 (vp[i].x, -sharedBaseWidthRoot, vp[i].z);
                                        uv[i] = new Vector2 (sharedBaseWidthRoot, uv[i].y);
                                    }
                                }
                            }
                        }

                        // Root (only needs UV adjustment)
                        else if (nm[i] == new Vector3 (0f, 1f, 0f))
                        {
                            if (vp[i].z < 0f) uv[i] = new Vector2 (sharedBaseLength, uv[i].y);
                        }

                        // Trailing edge
                        else
                        {
                            // Filtering out root neighbours
                            if (vp[i].y < -0.1f)
                            {
                                if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - sharedBaseWidthTip, vp[i].z);
                                else vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - sharedBaseWidthRoot, vp[i].z);
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

                        // Just blanks
                        cl[i] = new Color (0f, 0f, 0f, 0f);
                        uv2[i] = Vector2.zero;
                    }

                    meshFilterCtrlFrame.mesh.vertices = vp;
                    meshFilterCtrlFrame.mesh.uv = uv;
                    meshFilterCtrlFrame.mesh.uv2 = uv2;
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

                int ctrlEdgeTypeInt = Mathf.RoundToInt (sharedEdgeTypeTrailing - 1);
                for (int i = 0; i < meshTypeCountEdgeCtrl; ++i)
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
                    Vector2[] uv2 = new Vector2[length];

                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface edge | Passed array setup");
                    for (int i = 0; i < vp.Length; ++i)
                    {
                        // Thickness correction (X), edge width correction (Y) and span-based offset (Z)
                        if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationTip, ((vp[i].y + 0.5f) * ctrlEdgeWidthDeviationTip) - 0.5f, vp[i].z + 0.5f - sharedBaseLength / 2f);
                        else vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationRoot, ((vp[i].y + 0.5f) * ctrlEdgeWidthDeviationRoot) - 0.5f, vp[i].z - 0.5f + sharedBaseLength / 2f);

                        // Left/right sides
                        if (nm[i] == new Vector3 (0f, 0f, 1f) || nm[i] == new Vector3 (0f, 0f, -1f))
                        {
                            if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - sharedBaseWidthTip, vp[i].z);
                            else vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - sharedBaseWidthRoot, vp[i].z);
                        }

                        // Trailing edge
                        else
                        {
                            // Filtering out root neighbours
                            if (vp[i].y < -0.1f)
                            {
                                if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - sharedBaseWidthTip, vp[i].z);
                                else vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - sharedBaseWidthRoot, vp[i].z);
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
                            cl[i] = GetVertexColor (2);
                            uv2[i] = GetVertexUV2 (sharedMaterialET);
                        }
                    }

                    meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.vertices = vp;
                    meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.uv = uv;
                    meshFiltersCtrlEdge[ctrlEdgeTypeInt].mesh.uv2 = uv2;
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
                    Vector2[] uv2 = new Vector2[length];

                    if (logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface top | Passed array setup");
                    for (int i = 0; i < vp.Length; ++i)
                    {
                        // Span-based shift
                        if (vp[i].z < 0f)
                        {
                            vp[i] = new Vector3 (vp[i].x, vp[i].y, vp[i].z + 0.5f - sharedBaseLength / 2f);
                            uv[i] = new Vector2 (0f, uv[i].y);
                        }
                        else
                        {
                            vp[i] = new Vector3 (vp[i].x, vp[i].y, vp[i].z - 0.5f + sharedBaseLength / 2f);
                            uv[i] = new Vector2 (sharedBaseLength / 4f, uv[i].y);
                        }

                        // Width-based shift
                        if (vp[i].y < -0.1f)
                        {
                            if (vp[i].z < 0f)
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - sharedBaseWidthTip, vp[i].z);
                                uv[i] = new Vector2 (uv[i].x, sharedBaseWidthTip / 4f);
                            }
                            else
                            {
                                vp[i] = new Vector3 (vp[i].x, vp[i].y + 0.5f - sharedBaseWidthRoot, vp[i].z);
                                uv[i] = new Vector2 (uv[i].x, sharedBaseWidthRoot / 4f);
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
                        if (vp[i].x > 0f)
                        {
                            cl[i] = GetVertexColor (0);
                            uv2[i] = GetVertexUV2 (sharedMaterialST);
                        }
                        else
                        {
                            cl[i] = GetVertexColor (1);
                            uv2[i] = GetVertexUV2 (sharedMaterialSB);
                        }
                    }
                    meshFilterCtrlSurface.mesh.vertices = vp;
                    meshFilterCtrlSurface.mesh.uv = uv;
                    meshFilterCtrlSurface.mesh.uv2 = uv2;
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
        public static Texture materialLayeredSurfaceTextureMain;
        public static Texture materialLayeredSurfaceTextureMask;

        public static Material materialLayeredEdge;
        public static Texture materialLayeredEdgeTextureMain;
        public static Texture materialLayeredEdgeTextureMask;

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
                    for (int i = 0; i < meshTypeCountEdgeWing; ++i)
                    {
                        SetMaterial (meshFiltersWingEdgeTrailing[i], materialLayeredEdge);
                        SetMaterial (meshFiltersWingEdgeLeading[i], materialLayeredEdge);
                    }
                }
                else
                {
                    SetMaterial (meshFilterCtrlSurface, materialLayeredSurface);
                    SetMaterial (meshFilterCtrlFrame, materialLayeredEdge);
                    for (int i = 0; i < meshTypeCountEdgeCtrl; ++i)
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

            if (materialLayeredSurfaceTextureMain != null && materialLayeredSurfaceTextureMask != null)
            {
                materialLayeredSurface.SetTexture ("_MainTex", materialLayeredSurfaceTextureMain);
                materialLayeredSurface.SetTexture ("_Emissive", materialLayeredSurfaceTextureMask);
                materialLayeredSurface.SetFloat ("_Shininess", materialPropertyShininess);
                materialLayeredSurface.SetColor ("_SpecColor", materialPropertySpecular);
            }
            else if (logUpdateMaterials) DebugLogWithID ("SetMaterialReferences", "Surface textures not found");

            if (materialLayeredEdgeTextureMain != null && materialLayeredEdgeTextureMask != null)
            {
                materialLayeredEdge.SetTexture ("_MainTex", materialLayeredEdgeTextureMain);
                materialLayeredEdge.SetTexture ("_Emissive", materialLayeredEdgeTextureMask);
                materialLayeredEdge.SetFloat ("_Shininess", materialPropertyShininess);
                materialLayeredEdge.SetColor ("_SpecColor", materialPropertySpecular);
            }
            else if (logUpdateMaterials) DebugLogWithID ("SetMaterialReferences", "Edge textures not found");
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
                if (r != null)
                {
                    materialLayeredSurfaceTextureMain = r.sharedMaterial.GetTexture ("_MainTex");
                    materialLayeredSurfaceTextureMask = r.sharedMaterial.GetTexture ("_Emissive");
                    if (logUpdateMaterials) DebugLogWithID ("SetTextures", "Main: " + materialLayeredSurfaceTextureMain.ToString () + " | Mask: " + materialLayeredSurfaceTextureMask);
                }
            }
            if (sourceEdge != null)
            {
                Renderer r = sourceEdge.gameObject.GetComponent<Renderer> ();
                if (r != null)
                {
                    materialLayeredEdgeTextureMain = r.sharedMaterial.GetTexture ("_MainTex");
                    materialLayeredEdgeTextureMask = r.sharedMaterial.GetTexture ("_Emissive");
                    if (logUpdateMaterials) DebugLogWithID ("SetTextures", "Main: " + materialLayeredEdgeTextureMain.ToString () + " | Mask: " + materialLayeredEdgeTextureMask);
                }
            }
        }




        // Setup

        public void Setup ()
        {
            isStartingNow = true;
            SetupMeshFilters ();
            SetupFields ();
            SetupMeshReferences ();
            ReportOnMeshReferences ();
            SetupRecurring ();
            isStartingNow = false;
        }

        public void SetupRecurring ()
        {
            UpdateMaterials ();
            UpdateGeometry (true);
            UpdateCollidersForFAR ();
            UpdateWindow ();
        }

        public void UpdateCounterparts ()
        {
            for (int i = 0; i < this.part.symmetryCounterparts.Count; ++i)
            {
                var clone = this.part.symmetryCounterparts[i].Modules.OfType<WingProcedural> ().FirstOrDefault ();
                clone.updateCounterpartsAllowed = false;

                clone.sharedBaseLength = clone.sharedBaseLengthCached = sharedBaseLength;
                clone.sharedBaseWidthRoot = clone.sharedBaseWidthRootCached = sharedBaseWidthRoot;
                clone.sharedBaseWidthTip = clone.sharedBaseWidthTipCached = sharedBaseWidthTip;
                clone.sharedBaseThicknessRoot = clone.sharedBaseThicknessRootCached = sharedBaseThicknessRoot;
                clone.sharedBaseThicknessTip = clone.sharedBaseThicknessTipCached = sharedBaseThicknessTip;
                clone.sharedBaseOffsetRoot = clone.sharedBaseOffsetRootCached = sharedBaseOffsetRoot;
                clone.sharedBaseOffsetTip = clone.sharedBaseOffsetTipCached = sharedBaseOffsetTip;

                clone.sharedEdgeTypeLeading = clone.sharedEdgeTypeLeadingCached = sharedEdgeTypeLeading;
                clone.sharedEdgeWidthLeadingRoot = clone.sharedEdgeWidthLeadingRootCached = sharedEdgeWidthLeadingRoot;
                clone.sharedEdgeWidthLeadingTip = clone.sharedEdgeWidthLeadingTipCached = sharedEdgeWidthLeadingTip;

                clone.sharedEdgeTypeTrailing = clone.sharedEdgeTypeTrailingCached = sharedEdgeTypeTrailing;
                clone.sharedEdgeWidthTrailingRoot = clone.sharedEdgeWidthTrailingRootCached = sharedEdgeWidthTrailingRoot;
                clone.sharedEdgeWidthTrailingTip = clone.sharedEdgeWidthTrailingTipCached = sharedEdgeWidthTrailingTip;

                clone.sharedMaterialST = clone.sharedMaterialSTCached = sharedMaterialST;
                clone.sharedMaterialSB = clone.sharedMaterialSBCached = sharedMaterialSB;
                clone.sharedMaterialET = clone.sharedMaterialETCached = sharedMaterialET;
                clone.sharedMaterialEL = clone.sharedMaterialELCached = sharedMaterialEL;

                clone.sharedColorSTBrightness = clone.sharedColorSTBrightnessCached = sharedColorSTBrightness;
                clone.sharedColorSBBrightness = clone.sharedColorSBBrightnessCached = sharedColorSBBrightness;
                clone.sharedColorETBrightness = clone.sharedColorETBrightnessCached = sharedColorETBrightness;
                clone.sharedColorELBrightness = clone.sharedColorELBrightnessCached = sharedColorELBrightness;

                clone.sharedColorSTOpacity = clone.sharedColorSTOpacityCached = sharedColorSTOpacity;
                clone.sharedColorSBOpacity = clone.sharedColorSBOpacityCached = sharedColorSBOpacity;
                clone.sharedColorETOpacity = clone.sharedColorETOpacityCached = sharedColorETOpacity;
                clone.sharedColorELOpacity = clone.sharedColorELOpacityCached = sharedColorELOpacity;

                clone.sharedColorSTHue = clone.sharedColorSTHueCached = sharedColorSTHue;
                clone.sharedColorSBHue = clone.sharedColorSBHueCached = sharedColorSBHue;
                clone.sharedColorETHue = clone.sharedColorETHueCached = sharedColorETHue;
                clone.sharedColorELHue = clone.sharedColorELHueCached = sharedColorELHue;

                clone.sharedColorSTSaturation = clone.sharedColorSTSaturationCached = sharedColorSTSaturation;
                clone.sharedColorSBSaturation = clone.sharedColorSBSaturationCached = sharedColorSBSaturation;
                clone.sharedColorETSaturation = clone.sharedColorETSaturationCached = sharedColorETSaturation;
                clone.sharedColorELSaturation = clone.sharedColorELSaturationCached = sharedColorELSaturation;

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
                for (int i = 0; i < meshTypeCountEdgeWing; ++i)
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
                for (int i = 0; i < meshTypeCountEdgeCtrl; ++i)
                {
                    MeshFilter meshFilterCtrlEdge = CheckMeshFilter ("edge_type" + i);
                    meshFiltersCtrlEdge.Add (meshFilterCtrlEdge);
                }
            }
        }

        private Vector2 GetLimitsFromType (Vector2 limitsWing, Vector2 limitsCtrl)
        {
            if (!isCtrlSrf) return limitsWing;
            else return limitsCtrl;
        }

        private int GetFieldMode ()
        {
            if (!isCtrlSrf) return 1;
            else return 2;
        }

        private void SetupFields ()
        {
            SetFieldVisibility ("sharedFieldGroupBase", true);
            SetFieldType ("sharedBaseLength", 1, sharedLengthLimits, sharedIncrementMain, false);
            SetFieldType ("sharedBaseWidthRoot", GetFieldMode (), GetLimitsFromType (sharedWidthLimitsWing, sharedWidthLimitsCtrl), sharedIncrementMain, false);
            SetFieldType ("sharedBaseWidthTip", GetFieldMode (), GetLimitsFromType (sharedWidthLimitsWing, sharedWidthLimitsCtrl), sharedIncrementMain, false);
            SetFieldType ("sharedBaseThicknessRoot", 2, sharedThicknessLimits, sharedIncrementSmall, false);
            SetFieldType ("sharedBaseThicknessTip", 2, sharedThicknessLimits, sharedIncrementSmall, false);
            SetFieldType ("sharedBaseOffsetRoot", GetFieldMode (), GetLimitsFromType (sharedOffsetLimitsWing, sharedOffsetLimitsCtrl), sharedIncrementMain, false);
            SetFieldType ("sharedBaseOffsetTip", GetFieldMode (), GetLimitsFromType (sharedOffsetLimitsWing, sharedOffsetLimitsCtrl), sharedIncrementMain, false);

            SetFieldVisibility ("sharedFieldGroupEdgeTrailing", true);
            SetFieldType ("sharedEdgeTypeTrailing", 2, GetLimitsFromType (sharedEdgeTypeLimitsWing, sharedEdgeTypeLimitsCtrl), sharedIncrementInt, false);
            SetFieldType ("sharedEdgeWidthTrailingRoot", 2, sharedEdgeWidthLimits, sharedIncrementSmall, false);
            SetFieldType ("sharedEdgeWidthTrailingTip", 2, sharedEdgeWidthLimits, sharedIncrementSmall, false);

            SetFieldVisibility ("sharedFieldGroupEdgeLeading", !isCtrlSrf);
            SetFieldType ("sharedEdgeTypeLeading", 2, GetLimitsFromType (sharedEdgeTypeLimitsWing, sharedEdgeTypeLimitsCtrl), sharedIncrementInt, false);
            SetFieldType ("sharedEdgeWidthLeadingRoot", 2, sharedEdgeWidthLimits, sharedIncrementSmall, false);
            SetFieldType ("sharedEdgeWidthLeadingTip", 2, sharedEdgeWidthLimits, sharedIncrementSmall, false);

            SetFieldVisibility ("sharedFieldGroupColorST", true);
            SetFieldType ("sharedMaterialST", 2, sharedMaterialLimits, sharedIncrementInt, false);
            SetFieldType ("sharedColorSTOpacity", 2, sharedColorLimits, sharedIncrementColor, false);
            SetFieldType ("sharedColorSTHue", 2, sharedColorLimits, sharedIncrementColor, false);
            SetFieldType ("sharedColorSTSaturation", 2, sharedColorLimits, sharedIncrementColor, false);
            SetFieldType ("sharedColorSTBrightness", 2, sharedColorLimits, sharedIncrementColor, false);

            SetFieldVisibility ("sharedFieldGroupColorSB", true);
            SetFieldType ("sharedMaterialSB", 2, sharedMaterialLimits, sharedIncrementInt, false);
            SetFieldType ("sharedColorSBOpacity", 2, sharedColorLimits, sharedIncrementColor, false);
            SetFieldType ("sharedColorSBHue", 2, sharedColorLimits, sharedIncrementColor, false);           
            SetFieldType ("sharedColorSBSaturation", 2, sharedColorLimits, sharedIncrementColor, false);            
            SetFieldType ("sharedColorSBBrightness", 2, sharedColorLimits, sharedIncrementColor, false);

            SetFieldVisibility ("sharedFieldGroupColorET", true);
            SetFieldType ("sharedMaterialET", 2, sharedMaterialLimits, sharedIncrementInt, false);
            SetFieldType ("sharedColorETOpacity", 2, sharedColorLimits, sharedIncrementColor, false);
            SetFieldType ("sharedColorETHue", 2, sharedColorLimits, sharedIncrementColor, false);
            SetFieldType ("sharedColorETSaturation", 2, sharedColorLimits, sharedIncrementColor, false);
            SetFieldType ("sharedColorETBrightness", 2, sharedColorLimits, sharedIncrementColor, false);

            SetFieldVisibility ("sharedFieldGroupColorEL", !isCtrlSrf);
            SetFieldType ("sharedMaterialEL", 2, sharedMaterialLimits, sharedIncrementInt, false);
            SetFieldType ("sharedColorELOpacity", 2, sharedColorLimits, sharedIncrementColor, false);
            SetFieldType ("sharedColorELHue", 2, sharedColorLimits, sharedIncrementColor, false);
            SetFieldType ("sharedColorELSaturation", 2, sharedColorLimits, sharedIncrementColor, false);
            SetFieldType ("sharedColorELBrightness", 2, sharedColorLimits, sharedIncrementColor, false);
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
                    ui.minValue = limits.x;
                    ui.maxValue = limits.y;
                    ui.stepIncrement = increment;
                    ui.scene = UI_Scene.Editor;
                    ui.controlEnabled = true;
                    ui.Setup (field);
                    field.uiControlEditor = ui;
                }
                if (type == 1)
                {
                    UI_FloatEdit ui = new UI_FloatEdit ();
                    field.guiFormat = "S4";
                    ui.minValue = limits.x;
                    ui.maxValue = limits.y;
                    ui.incrementSlide = increment;
                    ui.incrementLarge = 1f;
                    ui.controlEnabled = true;
                    ui.Setup (field);
                    field.uiControlEditor = ui;
                }
                if (type == 2)
                {
                    UI_FloatEdit ui = new UI_FloatEdit ();
                    field.guiFormat = "S3";
                    ui.minValue = limits.x;
                    ui.maxValue = limits.y;
                    ui.incrementSlide = increment;
                    ui.incrementLarge = 0f;
                    ui.controlEnabled = true;
                    ui.Setup (field);
                    field.uiControlEditor = ui;
                }
            }
            field.uiControlEditor.controlEnabled = visible;
            field.guiActiveEditor = visible;
        }

        public void SetupMeshReferences ()
        {
            bool required = true;
            if (!isCtrlSrf)
            {
                if (meshReferenceWingSection != null && meshReferenceWingSurface != null && meshReferencesWingEdge[meshTypeCountEdgeWing - 1] != null)
                {
                    if (meshReferenceWingSection.vp.Length > 0 && meshReferenceWingSurface.vp.Length > 0 && meshReferencesWingEdge[meshTypeCountEdgeWing - 1].vp.Length > 0)
                    {
                        required = false;
                    }
                }
            }
            else
            {
                if (meshReferenceCtrlFrame != null && meshReferenceCtrlSurface != null && meshReferencesCtrlEdge[meshTypeCountEdgeCtrl - 1] != null)
                {
                    if (meshReferenceCtrlFrame.vp.Length > 0 && meshReferenceCtrlSurface.vp.Length > 0 && meshReferencesCtrlEdge[meshTypeCountEdgeCtrl - 1].vp.Length > 0)
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
                for (int i = 0; i < meshTypeCountEdgeWing; ++i)
                {
                    MeshReference meshReferenceWingEdge = FillMeshRefererence (meshFiltersWingEdgeTrailing[i]);
                    meshReferencesWingEdge.Add (meshReferenceWingEdge);
                }
            }
            else
            {
                WingProcedural.meshReferenceCtrlFrame = FillMeshRefererence (meshFilterCtrlFrame);
                WingProcedural.meshReferenceCtrlSurface = FillMeshRefererence (meshFilterCtrlSurface);
                for (int i = 0; i < meshTypeCountEdgeCtrl; ++i)
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

        [KSPField (guiActiveEditor = false, guiName = "Volume", guiFormat = "F3")]
        public float aeroStatVolume = 0f;

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

        public void CalculateAerodynamicValues ()
        {
            if (isAttached || HighLogic.LoadedSceneIsFlight)
            {
                if (logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Started");
                CheckAssemblies (false);

                float sharedWidthTipSum = sharedBaseWidthTip + sharedEdgeWidthTrailingTip;
                if (!isCtrlSrf) sharedWidthTipSum += sharedEdgeWidthLeadingTip;

                float sharedWidthRootSum = sharedBaseWidthRoot + sharedEdgeWidthTrailingRoot;
                if (!isCtrlSrf) sharedWidthTipSum += sharedEdgeWidthLeadingRoot;

                float ctrlOffsetRootLimit = (sharedBaseLength / 2f) / (sharedBaseWidthRoot + sharedEdgeWidthTrailingRoot);
                float ctrlOffsetTipLimit = (sharedBaseLength / 2f) / (sharedBaseWidthTip + sharedEdgeWidthTrailingTip);

                float ctrlOffsetRootClamped = Mathf.Clamp (sharedBaseOffsetRoot, -ctrlOffsetRootLimit, ctrlOffsetRootLimit);
                float ctrlOffsetTipClamped = Mathf.Clamp (sharedBaseOffsetTip, -ctrlOffsetTipLimit, ctrlOffsetTipLimit);

                // Base four values

                if (!isCtrlSrf)
                {
                    aeroStatSemispan = (double) sharedBaseLength;
                    aeroStatTaperRatio = (double) sharedWidthTipSum / (double) sharedWidthRootSum;
                    aeroStatMeanAerodynamicChord = (double) (sharedWidthTipSum + sharedWidthRootSum) / 2.0;
                    aeroStatMidChordSweep = MathD.Atan ((double) sharedBaseOffsetTip / (double) sharedBaseLength) * MathD.Rad2Deg;

                    aeroStatVolume = (sharedWidthTipSum * sharedBaseThicknessTip * sharedBaseLength) +
                    ((sharedWidthRootSum - sharedWidthTipSum) / 2f * sharedBaseThicknessTip * sharedBaseLength) +
                    (sharedWidthTipSum * (sharedBaseThicknessRoot - sharedBaseThicknessTip) / 2f * sharedBaseLength) +
                    ((sharedWidthRootSum - sharedWidthTipSum) / 2f * (sharedBaseThicknessRoot - sharedBaseThicknessTip) / 2f * sharedBaseLength);
                }
                else
                {
                    aeroStatSemispan = (double) sharedBaseLength;
                    aeroStatTaperRatio = (double) (sharedBaseLength + sharedWidthTipSum * ctrlOffsetTipClamped - sharedWidthRootSum * ctrlOffsetRootClamped) / (double) sharedBaseLength;
                    aeroStatMeanAerodynamicChord = (double) (sharedWidthTipSum + sharedWidthRootSum) / 2.0;
                    aeroStatMidChordSweep = MathD.Atan ((double) Mathf.Abs (sharedWidthRootSum - sharedWidthTipSum) / (double) sharedBaseLength) * MathD.Rad2Deg;
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
                    part.CoMOffset = new Vector3 (sharedBaseLength / 2f, -sharedBaseOffsetTip / 2f, 0f);
                }
                else
                {
                    aeroUICost = (float) aeroStatMass * (1f + (float) aeroStatAspectRatioSweepScale / 4f) * aeroConstCostDensity * (1f - aeroConstControlSurfaceFraction);
                    aeroUICost += (float) aeroStatMass * (1f + (float) aeroStatAspectRatioSweepScale / 4f) * aeroConstCostDensityControl * aeroConstControlSurfaceFraction;
                    aeroUICost = Mathf.Round (aeroUICost / 5f) * 5f;
                    part.CoMOffset = new Vector3 (0f, -(sharedWidthRootSum + sharedWidthTipSum) / 4f, 0f);
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
                Fields["aeroStatVolume"].guiActiveEditor = showWingData;

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

        public static bool uiDropdownOpen = false;
        public float uiValueTestA = 0f;

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
            //int m = Mathf.RoundToInt (uiMouseDeltaCache);
            //if (m != 0)
            //{
            //    uiMouseDeltaCache = 0f;
            //    if (!isCtrlSrf)
            //    {
            //        if (uiPropertySelectionWing == 0)       AdjustProperty (ref sharedBaseSemispan, m, sharedIncrementMain, wingSpanLimits);
            //        else if (uiPropertySelectionWing == 1)  AdjustProperty (ref sharedBaseWidthRoot, m, sharedIncrementMain, wingWidthLimits);
            //        else if (uiPropertySelectionWing == 2)  AdjustProperty (ref sharedBaseWidthTip, m, sharedIncrementMain,  wingWidthLimits);
            //        else if (uiPropertySelectionWing == 3)  AdjustProperty (ref sharedBaseOffsetUnified, m, sharedIncrementMain, wingOffsetLimits);
            //        else if (uiPropertySelectionWing == 4)  AdjustProperty (ref sharedBaseThicknessRoot, m, sharedIncrementSmall, wingThicknessLimits);
            //        else if (uiPropertySelectionWing == 5)  AdjustProperty (ref sharedBaseThicknessTip, m, sharedIncrementSmall, wingThicknessLimits);
            //        else if (uiPropertySelectionWing == 6)  AdjustProperty (ref wingSurfaceTextureTop, m, sharedIncrementInt, wingTextureLimits);
            //        else if (uiPropertySelectionWing == 7)  AdjustProperty (ref wingSurfaceTextureBottom, m, sharedIncrementInt, wingTextureLimits);
            //        else if (uiPropertySelectionWing == 8)  AdjustProperty (ref sharedEdgeTypeLeading, m, sharedIncrementInt, wingEdgeTypeLimits);
            //        else if (uiPropertySelectionWing == 9)  AdjustProperty (ref sharedEdgeTypeTrailing, m, sharedIncrementInt, wingEdgeTypeLimits);
            //        else if (uiPropertySelectionWing == 10) AdjustProperty (ref wingEdgeTextureLeading, m, sharedIncrementInt, wingTextureLimits);
            //        else if (uiPropertySelectionWing == 11) AdjustProperty (ref wingEdgeTextureTrailing, m, sharedIncrementInt, wingTextureLimits);
            //        else if (uiPropertySelectionWing == 12) AdjustProperty (ref sharedEdgeWidthLeadingRoot, m, sharedIncrementSmall, wingEdgeWidthLimits);
            //        else if (uiPropertySelectionWing == 13) AdjustProperty (ref sharedEdgeWidthLeadingTip, m, sharedIncrementSmall, wingEdgeWidthLimits);
            //        else if (uiPropertySelectionWing == 14) AdjustProperty (ref sharedEdgeWidthTrailingRoot, m, sharedIncrementSmall, wingEdgeWidthLimits);
            //        else if (uiPropertySelectionWing == 15) AdjustProperty (ref sharedEdgeWidthTrailingTip, m, sharedIncrementSmall, wingEdgeWidthLimits);
            //    }
            //    else
            //    {
            //        if (uiPropertySelectionSurface == 0)       AdjustProperty (ref ctrlSpan, m, sharedIncrementMain, ctrlSpanLimits);
            //        else if (uiPropertySelectionSurface == 1)  AdjustProperty (ref ctrlWidthRoot, m, sharedIncrementMain, ctrlWidthLimits);
            //        else if (uiPropertySelectionSurface == 2)  AdjustProperty (ref ctrlWidthTip, m, sharedIncrementMain, ctrlWidthLimits);
            //        else if (uiPropertySelectionSurface == 3)  AdjustProperty (ref ctrlEdgeWidthRoot, m, sharedIncrementSmall, ctrlEdgeWidthLimits);
            //        else if (uiPropertySelectionSurface == 4)  AdjustProperty (ref ctrlEdgeWidthTip, m, sharedIncrementSmall, ctrlEdgeWidthLimits);
            //        else if (uiPropertySelectionSurface == 5)  AdjustProperty (ref ctrlThicknessRoot, m, sharedIncrementSmall, ctrlThicknessLimits);
            //        else if (uiPropertySelectionSurface == 6)  AdjustProperty (ref ctrlThicknessTip, m, sharedIncrementSmall, ctrlThicknessLimits);
            //        else if (uiPropertySelectionSurface == 7)  AdjustProperty (ref ctrlOffsetRoot, m, sharedIncrementMain, ctrlOffsetLimits);
            //        else if (uiPropertySelectionSurface == 8)  AdjustProperty (ref ctrlOffsetTip, m, sharedIncrementMain, ctrlOffsetLimits);
            //        else if (uiPropertySelectionSurface == 9)  AdjustProperty (ref ctrlSurfaceTextureTop, m, sharedIncrementInt, ctrlTextureLimits);
            //        else if (uiPropertySelectionSurface == 10) AdjustProperty (ref ctrlSurfaceTextureBottom, m, sharedIncrementInt, ctrlTextureLimits);
            //        else if (uiPropertySelectionSurface == 11) AdjustProperty (ref ctrlEdgeTexture, m, sharedIncrementInt, ctrlTextureLimits);
            //        else if (uiPropertySelectionSurface == 12) AdjustProperty (ref ctrlEdgeType, m, sharedIncrementInt, ctrlEdgeTypeLimits);
            //    }
            //}
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
            //if (!isCtrlSrf && id <= 15 && id >= 0)
            //{
            //    if (id == 0)       return "Semispan\n"            + sharedBaseSemispan.ToString ("F3");
            //    else if (id == 1)  return "Width (root)\n"        + sharedBaseWidthRoot.ToString ("F3");
            //    else if (id == 2)  return "Width (tip)\n"         + sharedBaseWidthTip.ToString ("F3");
            //    else if (id == 3)  return "Offset\n"              + sharedBaseOffsetUnified.ToString ("F3");
            //    else if (id == 4)  return "Thickness (root)\n"    + sharedBaseThicknessRoot.ToString ("F2");
            //    else if (id == 5)  return "Thickness (tip)\n"     + sharedBaseThicknessTip.ToString ("F2");
            //    else if (id == 6)  return "Side A (material)\n"   + GetValueTranslationForMaterials (wingSurfaceTextureTop);
            //    else if (id == 7)  return "Side B (material)\n"   + GetValueTranslationForMaterials (wingSurfaceTextureBottom);
            //    else if (id == 8)  return "Edge L (shape)\n"      + GetValueTranslationForEdges (sharedEdgeTypeLeading);
            //    else if (id == 9)  return "Edge T (shape)\n"      + GetValueTranslationForEdges (sharedEdgeTypeTrailing);
            //    else if (id == 10) return "Edge L (material)\n"   + GetValueTranslationForMaterials (wingEdgeTextureLeading);
            //    else if (id == 11) return "Edge T (material)\n"   + GetValueTranslationForMaterials (wingEdgeTextureTrailing);
            //    else if (id == 12) return "Edge L (root width)\n" + sharedEdgeWidthLeadingRoot.ToString ();
            //    else if (id == 13) return "Edge L (tip width)\n"  + sharedEdgeWidthLeadingTip.ToString ();
            //    else if (id == 14) return "Edge T (root width)\n" + sharedEdgeWidthTrailingRoot.ToString ();
            //    else               return "Edge T (tip width)\n"  + sharedEdgeWidthTrailingTip.ToString ();
            //}
            //else if (isCtrlSrf && id <= 12 && id >= 0)
            //{
            //    if (id == 0)       return "Length\n"             + ctrlSpan.ToString ("F3");
            //    else if (id == 1)  return "Width (main, root)\n" + ctrlWidthRoot.ToString ("F3");
            //    else if (id == 2)  return "Width (main, tip)\n"  + ctrlWidthTip.ToString ("F3");
            //    else if (id == 3)  return "Width (edge, root)\n" + ctrlEdgeWidthRoot.ToString ("F3");
            //    else if (id == 4)  return "Width (edge, tip)\n"  + ctrlEdgeWidthTip.ToString ("F3"); 
            //    else if (id == 5)  return "Thickness (root)\n"   + ctrlThicknessRoot.ToString ("F2");
            //    else if (id == 6)  return "Thickness (tip)\n"    + ctrlThicknessTip.ToString ("F2");
            //    else if (id == 7)  return "Offset R\n"           + ctrlOffsetRoot.ToString ("F3");
            //    else if (id == 8)  return "Offset T\n"           + ctrlOffsetTip.ToString ("F3");
            //    else if (id == 9)  return "Side A (material)\n"  + GetValueTranslationForMaterials (ctrlSurfaceTextureTop);
            //    else if (id == 10) return "Side B (material)\n"  + GetValueTranslationForMaterials (ctrlSurfaceTextureBottom);
            //    else if (id == 11) return "Edge (material)\n"    + GetValueTranslationForMaterials (ctrlEdgeTexture);
            //    else               return "Edge (shape)\n"       + GetValueTranslationForEdges (ctrlEdgeType);
            //}
            //else 
                return "Invalid property ID";
        }

        private string GetPropertyDescription (int id)
        {
            //if (!isCtrlSrf && id <= 15 && id >= 0)
            //{
            //    if (id == 0)       return "Lateral measurement of the wing";
            //    else if (id == 1)  return "Longitudinal measurement of the wing \nat the root cross section";
            //    else if (id == 2)  return "Longitudinal measurement of the wing \nat the tip cross section";
            //    else if (id == 3)  return "Distance between midpoints of the cross \nsections on the longitudinal axis";
            //    else if (id == 4)  return "Thickness at the root cross section";
            //    else if (id == 5)  return "Thickness at the tip cross section";
            //    else if (id == 6)  return "Material of the wing surface A \n(usually it's the one on top)";
            //    else if (id == 7)  return "Material of the wing surface B \n(usually it's the bottom one)";
            //    else if (id == 8)  return "Leading edge cross section shape";
            //    else if (id == 9)  return "Trailing edge cross section shape";
            //    else if (id == 10) return "Leading edge material";
            //    else if (id == 11) return "Trailing edge material";
            //    else if (id == 12) return "Leading edge width at the root cross \nsection on the longitudinal axis";
            //    else if (id == 13) return "Leading edge width at the tip cross \nsection on the longitudinal axis";
            //    else if (id == 14) return "Trailing edge width at the root cross \nsection on the longitudinal axis";
            //    else               return "Trailing edge width at the tip cross \nsection on the longitudinal axis";
            //}
            //else if (isCtrlSrf && id <= 12 && id >= 0)
            //{
            //    if (id == 0)       return "Lateral measurement of the root \ncross section of the control surface";
            //    else if (id == 1)  return "Longitudinal measurement of the surface \nat the left (root) cross section";
            //    else if (id == 2)  return "Longitudinal measurement of the surface \nat the right (tip) cross section";
            //    else if (id == 3)  return "Longitudinal measurement of the edge \nat the left (root) cross section";
            //    else if (id == 4)  return "Longitudinal measurement of the edge \nat the right (tip) cross section";
            //    else if (id == 5)  return "Thickness at the left cross section";
            //    else if (id == 6)  return "Thickness at the right cross section";
            //    else if (id == 7)  return "Offset of the trailing edge left corner \non the lateral axis";
            //    else if (id == 8)  return "Offset of the trailing edge right corner \non the lateral axis";
            //    else if (id == 9)  return "Material of the flat surface A \n(typically top of the control surface)";
            //    else if (id == 10) return "Material of the flat surface B \n(typically bottom of the control surface)";
            //    else if (id == 11) return "Material of the trailing edge";
            //    else               return "Trailing edge cross section shape";
            //}
            //else 
                return "Invalid property ID";
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




        // Coloration

        // XYZ
        // HSB
        // RGB

        private Color GetVertexColor (int side)
        {
            if (side == 0) 
                return ColorHSBToRGB (new Vector4 (sharedColorSTHue, sharedColorSTSaturation, sharedColorSTBrightness, sharedColorSTOpacity));
            else if (side == 1) 
                return ColorHSBToRGB (new Vector4 (sharedColorSBHue, sharedColorSBSaturation, sharedColorSBBrightness, sharedColorSBOpacity));
            else if (side == 2) 
                return ColorHSBToRGB (new Vector4 (sharedColorETHue, sharedColorETSaturation, sharedColorETBrightness, sharedColorETOpacity));
            else 
                return ColorHSBToRGB (new Vector4 (sharedColorELHue, sharedColorELSaturation, sharedColorELBrightness, sharedColorELOpacity));
        }

        private Vector2 GetVertexUV2 (float selectedLayer)
        {
            if (selectedLayer == 0) return new Vector2 (0f, 1f);
            return new Vector2 ((selectedLayer - 1f) / 3f, 0f);
        }

        private Color ColorHSBToRGB (Vector4 hsbColor)
        {
            float r = hsbColor.z;
            float g = hsbColor.z;
            float b = hsbColor.z;
            if (hsbColor.y != 0)
            {
                float max = hsbColor.z;
                float dif = hsbColor.z * hsbColor.y;
                float min = hsbColor.z - dif;
                float h = hsbColor.x * 360f;
                if (h < 60f)
                {
                    r = max;
                    g = h * dif / 60f + min;
                    b = min;
                }
                else if (h < 120f)
                {
                    r = -(h - 120f) * dif / 60f + min;
                    g = max;
                    b = min;
                }
                else if (h < 180f)
                {
                    r = min;
                    g = max;
                    b = (h - 120f) * dif / 60f + min;
                }
                else if (h < 240f)
                {
                    r = min;
                    g = -(h - 240f) * dif / 60f + min;
                    b = max;
                }
                else if (h < 300f)
                {
                    r = (h - 240f) * dif / 60f + min;
                    g = min;
                    b = max;
                }
                else if (h <= 360f)
                {
                    r = max;
                    g = min;
                    b = -(h - 360f) * dif / 60 + min;
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }
            return new Color (Mathf.Clamp01 (r), Mathf.Clamp01 (g), Mathf.Clamp01 (b), hsbColor.w);
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
