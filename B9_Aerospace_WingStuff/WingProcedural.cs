using KSP;
using KSP.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace WingProcedural
{
    // TODO
    // Default edge ID for surface is incorrect due to shifted limits
    // Wing/edge limit difference assignment isn't working
    // Alternative UI
    
    public class WingProcedural : PartModule, IPartCostModifier, IPartSizeModifier
    {
        // Some handy bools

        [KSPField] public bool isCtrlSrf = false;
        [KSPField] public bool isWingAsCtrlSrf = false;

        [KSPField (isPersistant = true)] public bool isAttached = false;
        [KSPField (isPersistant = true)] public bool isSetToDefaultValues = false;

        public bool isStarted = false;
        public bool isStartingNow = false;
        public bool justDetached = false;




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


        // Mesh properties

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

        private static int meshTypeCountEdgeWing = 4;
        private static int meshTypeCountEdgeCtrl = 3;




        // Shared properties / Limits and increments

        private Vector2 GetLimitsFromType (Vector4 set)
        {
            if (WingProceduralDebugValues.logLimits) DebugLogWithID ("GetLimitsFromType", "Using set: " + set);
            if (!isCtrlSrf) return new Vector2 (set.x, set.y);
            else return new Vector2 (set.z, set.w);
        }

        private float GetIncrementFromType (float incrementWing, float incrementCtrl)
        {
            if (!isCtrlSrf) return incrementWing;
            else return incrementCtrl;
        }

        private static Vector4 sharedBaseLengthLimits = new Vector4 (0.25f, 16f, 0.25f, 8f);
        private static Vector2 sharedBaseThicknessLimits = new Vector2 (0.08f, 1f);
        private static Vector4 sharedBaseWidthLimits = new Vector4 (0.25f, 16f, 0.125f, 1.5f);
        private static Vector4 sharedBaseOffsetLimits = new Vector4 (-8f, 8f, -2f, 2f);
        private static Vector4 sharedEdgeTypeLimits = new Vector4 (1f, 4f, 1f, 3f);
        private static Vector4 sharedEdgeWidthLimits = new Vector4 (0f, 1f, 0.24f, 1f);
        private static Vector2 sharedMaterialLimits = new Vector2 (0f, 4f);
        private static Vector2 sharedColorLimits = new Vector2 (0f, 1f);

        private static float sharedIncrementColor = 0.01f;
        private static float sharedIncrementMain = 0.125f;
        private static float sharedIncrementSmall = 0.04f;
        private static float sharedIncrementInt = 1f;




        // Shared properties / Base

        [KSPField (guiActiveEditor = false, guiActive = false, guiName = "| Base")]
        // UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        // public bool sharedFieldGroupBase = true;
        // public bool sharedFieldGroupBaseCached = true;
        public static bool sharedFieldGroupBaseStatic = true;
        private static string[] sharedFieldGroupBaseArray = new string[] { "sharedBaseLength", "sharedBaseWidthRoot", "sharedBaseWidthTip", "sharedBaseThicknessRoot", "sharedBaseThicknessTip", "sharedBaseOffsetTip" };
        private static string[] sharedFieldGroupBaseArrayCtrl = new string[] { "sharedBaseOffsetRoot" };

        [KSPField (isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Length", guiFormat = "S4")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float sharedBaseLength = 4f;
        public float sharedBaseLengthCached = 4f;
        public static Vector4 sharedBaseLengthDefaults = new Vector4 (4f, 1f, 4f, 1f);

        [KSPField (isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Width (root)", guiFormat = "S4")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float sharedBaseWidthRoot = 4f;
        public float sharedBaseWidthRootCached = 4f;
        public static Vector4 sharedBaseWidthRootDefaults = new Vector4 (4f, 0.5f, 4f, 0.5f);

        [KSPField (isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Width (tip)", guiFormat = "S4")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float sharedBaseWidthTip = 4f;
        public float sharedBaseWidthTipCached = 4f;
        public static Vector4 sharedBaseWidthTipDefaults = new Vector4 (4f, 0.5f, 4f, 0.5f);

        [KSPField (isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Offset (root)", guiFormat = "S4")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = -2.5f, maxValue = 2.5f, incrementSlide = 0.125f)]
        public float sharedBaseOffsetRoot = 0f;
        public float sharedBaseOffsetRootCached = 0f;
        public static Vector4 sharedBaseOffsetRootDefaults = new Vector4 (0f, 0f, 0f, 0f);

        [KSPField (isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "Offset (tip)", guiFormat = "S4")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = -2.5f, maxValue = 2.5f, incrementSlide = 0.125f)]
        public float sharedBaseOffsetTip = 0f;
        public float sharedBaseOffsetTipCached = 0f;
        public static Vector4 sharedBaseOffsetTipDefaults = new Vector4 (0f, 0f, 0f, 0f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Thickness (root)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.08f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedBaseThicknessRoot = 0.24f;
        public float sharedBaseThicknessRootCached = 0.24f;
        public static Vector4 sharedBaseThicknessRootDefaults = new Vector4 (0.24f, 0.24f, 0.24f, 0.24f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Thickness (tip)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.08f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedBaseThicknessTip = 0.24f;
        public float sharedBaseThicknessTipCached = 0.24f;
        public static Vector4 sharedBaseThicknessTipDefaults = new Vector4 (0.24f, 0.24f, 0.24f, 0.24f);




        // Shared properties / Edge / Leading

        [KSPField (guiActiveEditor = false, guiActive = false, guiName = "| Lead. edge")] 
        // UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        // public bool sharedFieldGroupEdgeLeading = false;
        // public bool sharedFieldGroupEdgeLeadingCached = false;
        public static bool sharedFieldGroupEdgeLeadingStatic = false;
        private static string[] sharedFieldGroupEdgeLeadingArray = new string[] { "sharedEdgeTypeLeading", "sharedEdgeWidthLeadingRoot", "sharedEdgeWidthLeadingTip" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Shape", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 1f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedEdgeTypeLeading = 2f;
        public float sharedEdgeTypeLeadingCached = 2f;
        public static Vector4 sharedEdgeTypeLeadingDefaults = new Vector4 (2f, 1f, 2f, 1f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Width (root)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedEdgeWidthLeadingRoot = 0.24f;
        public float sharedEdgeWidthLeadingRootCached = 0.24f;
        public static Vector4 sharedEdgeWidthLeadingRootDefaults = new Vector4 (0.24f, 0.24f, 0.24f, 0.24f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Width (tip)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedEdgeWidthLeadingTip = 0.24f;
        public float sharedEdgeWidthLeadingTipCached = 0.24f;
        public static Vector4 sharedEdgeWidthLeadingTipDefaults = new Vector4 (0.24f, 0.24f, 0.24f, 0.24f);




        // Shared properties / Edge / Trailing

        [KSPField (guiActiveEditor = false, guiActive = false, guiName = "| Trail. edge")]
        // UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        // public bool sharedFieldGroupEdgeTrailing = false;
        // public bool sharedFieldGroupEdgeTrailingCached = false;
        public static bool sharedFieldGroupEdgeTrailingStatic = false;
        private static string[] sharedFieldGroupEdgeTrailingArray = new string[] { "sharedEdgeTypeTrailing", "sharedEdgeWidthTrailingRoot", "sharedEdgeWidthTrailingTip" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Shape", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 1f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedEdgeTypeTrailing = 3f;
        public float sharedEdgeTypeTrailingCached = 3f;
        public static Vector4 sharedEdgeTypeTrailingDefaults = new Vector4 (3f, 2f, 3f, 2f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Width (root)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedEdgeWidthTrailingRoot = 0.48f;
        public float sharedEdgeWidthTrailingRootCached = 0.48f;
        public static Vector4 sharedEdgeWidthTrailingRootDefaults = new Vector4 (0.48f, 0.48f, 0.48f, 0.48f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Width (tip)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedEdgeWidthTrailingTip = 0.48f;
        public float sharedEdgeWidthTrailingTipCached = 0.48f;
        public static Vector4 sharedEdgeWidthTrailingTipDefaults = new Vector4 (0.48f, 0.48f, 0.48f, 0.48f);



        // Shared properties / Surface / Top

        [KSPField (guiActiveEditor = false, guiActive = false, guiName = "| Material A")]
        // UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        // public bool sharedFieldGroupColorST = false;
        // public bool sharedFieldGroupColorSTCached = false;
        public static bool sharedFieldGroupColorSTStatic = false;
        private static string[] sharedFieldGroupColorSTArray = new string[] { "sharedMaterialST", "sharedColorSTOpacity", "sharedColorSTHue", "sharedColorSTSaturation", "sharedColorSTBrightness" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Material", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedMaterialST = 1f;
        public float sharedMaterialSTCached = 1f;
        public static Vector4 sharedMaterialSTDefaults = new Vector4 (1f, 1f, 1f, 1f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Opacity", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSTOpacity = 0f;
        public float sharedColorSTOpacityCached = 0f;
        public static Vector4 sharedColorSTOpacityDefaults = new Vector4 (0f, 0f, 0f, 0f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Color (H)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSTHue = 0.10f;
        public float sharedColorSTHueCached = 0.10f;
        public static Vector4 sharedColorSTHueDefaults = new Vector4 (0.1f, 0.1f, 0.1f, 0.1f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Color (S)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSTSaturation = 0.75f;
        public float sharedColorSTSaturationCached = 0.75f;
        public static Vector4 sharedColorSTSaturationDefaults = new Vector4 (0.75f, 0.75f, 0.75f, 0.75f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Color (B)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSTBrightness = 0.6f;
        public float sharedColorSTBrightnessCached = 0.6f;
        public static Vector4 sharedColorSTBrightnessDefaults = new Vector4 (0.6f, 0.6f, 0.6f, 0.6f);




        // Shared properties / Surface / bottom

        [KSPField (guiActiveEditor = false, guiActive = false, guiName = "| Material B")]
        // UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        // public bool sharedFieldGroupColorSB = false;
        //public bool sharedFieldGroupColorSBCached = false;
        public static bool sharedFieldGroupColorSBStatic = false;
        private static string[] sharedFieldGroupColorSBArray = new string[] { "sharedMaterialSB", "sharedColorSBOpacity", "sharedColorSBHue", "sharedColorSBSaturation", "sharedColorSBBrightness" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Material", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedMaterialSB = 4f;
        public float sharedMaterialSBCached = 4f;
        public static Vector4 sharedMaterialSBDefaults = new Vector4 (4f, 4f, 4f, 4f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Opacity", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSBOpacity = 0f;
        public float sharedColorSBOpacityCached = 0f;
        public static Vector4 sharedColorSBOpacityDefaults = new Vector4 (0f, 0f, 0f, 0f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Color (H)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSBHue = 0.10f;
        public float sharedColorSBHueCached = 0.10f;
        public static Vector4 sharedColorSBHueDefaults = new Vector4 (0.1f, 0.1f, 0.1f, 0.1f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Color (S)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSBSaturation = 0.75f;
        public float sharedColorSBSaturationCached = 0.75f;
        public static Vector4 sharedColorSBSaturationDefaults = new Vector4 (0.75f, 0.75f, 0.75f, 0.75f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Color (B)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSBBrightness = 0.6f;
        public float sharedColorSBBrightnessCached = 0.6f;
        public static Vector4 sharedColorSBBrightnessDefaults = new Vector4 (0.6f, 0.6f, 0.6f, 0.6f);




        // Shared properties / Surface / trailing edge

        [KSPField (guiActiveEditor = false, guiActive = false, guiName = "| Material T")]
        // UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        // public bool sharedFieldGroupColorET = false;
        // public bool sharedFieldGroupColorETCached = false;
        public static bool sharedFieldGroupColorETStatic = false;
        private static string[] sharedFieldGroupColorETArray = new string[] { "sharedMaterialET", "sharedColorETOpacity", "sharedColorETHue", "sharedColorETSaturation", "sharedColorETBrightness" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Material", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedMaterialET = 4f;
        public float sharedMaterialETCached = 4f;
        public static Vector4 sharedMaterialETDefaults = new Vector4 (4f, 4f, 4f, 4f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Opacity", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorETOpacity = 0f;
        public float sharedColorETOpacityCached = 0f;
        public static Vector4 sharedColorETOpacityDefaults = new Vector4 (0f, 0f, 0f, 0f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Color (H)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorETHue = 0.10f;
        public float sharedColorETHueCached = 0.10f;
        public static Vector4 sharedColorETHueDefaults = new Vector4 (0.1f, 0.1f, 0.1f, 0.1f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Color (S)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorETSaturation = 0.75f;
        public float sharedColorETSaturationCached = 0.75f;
        public static Vector4 sharedColorETSaturationDefaults = new Vector4 (0.75f, 0.75f, 0.75f, 0.75f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Color (B)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorETBrightness = 0.6f;
        public float sharedColorETBrightnessCached = 0.6f;
        public static Vector4 sharedColorETBrightnessDefaults = new Vector4 (0.6f, 0.6f, 0.6f, 0.6f);




        // Shared properties / Surface / leading edge

        [KSPField (guiActiveEditor = false, guiActive = false, guiName = "| Material L")]
        // UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        // public bool sharedFieldGroupColorEL = false;
        // public bool sharedFieldGroupColorELCached = false;
        public static bool sharedFieldGroupColorELStatic = false;
        private static string[] sharedFieldGroupColorELArray = new string[] { "sharedMaterialEL", "sharedColorELOpacity", "sharedColorELHue", "sharedColorELSaturation", "sharedColorELBrightness" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Material", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedMaterialEL = 4f;
        public float sharedMaterialELCached = 4f;
        public static Vector4 sharedMaterialELDefaults = new Vector4 (4f, 4f, 4f, 4f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Opacity", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorELOpacity = 0f;
        public float sharedColorELOpacityCached = 0f;
        public static Vector4 sharedColorELOpacityDefaults = new Vector4 (0f, 0f, 0f, 0f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Color (H)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorELHue = 0.10f;
        public float sharedColorELHueCached = 0.10f;
        public static Vector4 sharedColorELHueDefaults = new Vector4 (0.1f, 0.1f, 0.1f, 0.1f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Color (S)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorELSaturation = 0.75f;
        public float sharedColorELSaturationCached = 0.75f;
        public static Vector4 sharedColorELSaturationDefaults = new Vector4 (0.75f, 0.75f, 0.75f, 0.75f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Color (B)", guiFormat = "F3")]
        // UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorELBrightness = 0.6f;
        public float sharedColorELBrightnessCached = 0.6f;
        public static Vector4 sharedColorELBrightnessDefaults = new Vector4 (0.6f, 0.6f, 0.6f, 0.6f);




        // Default values
        // Vector4 (defaultWing, defaultCtrl, defaultWingBackup, defaultCtrlBackup)

        private void ReplaceDefaults ()
        {
            ReplaceDefault (ref sharedBaseLengthDefaults, sharedBaseLength);
            ReplaceDefault (ref sharedBaseWidthRootDefaults, sharedBaseWidthRoot);
            ReplaceDefault (ref sharedBaseWidthTipDefaults, sharedBaseWidthTip);
            ReplaceDefault (ref sharedBaseOffsetRootDefaults, sharedBaseOffsetRoot);
            ReplaceDefault (ref sharedBaseOffsetTipDefaults, sharedBaseOffsetTip);
            ReplaceDefault (ref sharedBaseThicknessRootDefaults, sharedBaseThicknessRoot);
            ReplaceDefault (ref sharedBaseThicknessTipDefaults, sharedBaseThicknessTip);

            ReplaceDefault (ref sharedEdgeTypeLeadingDefaults, sharedEdgeTypeLeading);
            ReplaceDefault (ref sharedEdgeWidthLeadingRootDefaults, sharedEdgeWidthLeadingRoot);
            ReplaceDefault (ref sharedEdgeWidthLeadingTipDefaults, sharedEdgeWidthLeadingTip);

            ReplaceDefault (ref sharedEdgeTypeTrailingDefaults, sharedEdgeTypeTrailing);
            ReplaceDefault (ref sharedEdgeWidthTrailingRootDefaults, sharedEdgeWidthTrailingRoot);
            ReplaceDefault (ref sharedEdgeWidthTrailingTipDefaults, sharedEdgeWidthTrailingTip);

            ReplaceDefault (ref sharedMaterialSTDefaults, sharedMaterialST);
            ReplaceDefault (ref sharedColorSTOpacityDefaults, sharedColorSTOpacity);
            ReplaceDefault (ref sharedColorSTHueDefaults, sharedColorSTHue);
            ReplaceDefault (ref sharedColorSTSaturationDefaults, sharedColorSTSaturation);
            ReplaceDefault (ref sharedColorSTBrightnessDefaults, sharedColorSTBrightness);

            ReplaceDefault (ref sharedMaterialSBDefaults, sharedMaterialSB);
            ReplaceDefault (ref sharedColorSBOpacityDefaults, sharedColorSBOpacity);
            ReplaceDefault (ref sharedColorSBHueDefaults, sharedColorSBHue);
            ReplaceDefault (ref sharedColorSBSaturationDefaults, sharedColorSBSaturation);
            ReplaceDefault (ref sharedColorSBBrightnessDefaults, sharedColorSBBrightness);

            ReplaceDefault (ref sharedMaterialETDefaults, sharedMaterialET);
            ReplaceDefault (ref sharedColorETOpacityDefaults, sharedColorETOpacity);
            ReplaceDefault (ref sharedColorETHueDefaults, sharedColorETHue);
            ReplaceDefault (ref sharedColorETSaturationDefaults, sharedColorETSaturation);
            ReplaceDefault (ref sharedColorETBrightnessDefaults, sharedColorETBrightness);

            ReplaceDefault (ref sharedMaterialELDefaults, sharedMaterialEL);
            ReplaceDefault (ref sharedColorELOpacityDefaults, sharedColorELOpacity);
            ReplaceDefault (ref sharedColorELHueDefaults, sharedColorELHue);
            ReplaceDefault (ref sharedColorELSaturationDefaults, sharedColorELSaturation);
            ReplaceDefault (ref sharedColorELBrightnessDefaults, sharedColorELBrightness);
        }

        private void ReplaceDefault (ref Vector4 set, float value)
        {
            if (!isCtrlSrf) set = new Vector4 (value, set.w, set.z, set.w);
            else set = new Vector4 (set.z, value, set.z, set.w);
        }

        private void RestoreDefaults ()
        {
            RestoreDefault (ref sharedBaseLengthDefaults);
            RestoreDefault (ref sharedBaseWidthRootDefaults);
            RestoreDefault (ref sharedBaseWidthTipDefaults);
            RestoreDefault (ref sharedBaseOffsetRootDefaults);
            RestoreDefault (ref sharedBaseOffsetTipDefaults);
            RestoreDefault (ref sharedBaseThicknessRootDefaults);
            RestoreDefault (ref sharedBaseThicknessTipDefaults);

            RestoreDefault (ref sharedEdgeTypeLeadingDefaults);
            RestoreDefault (ref sharedEdgeWidthLeadingRootDefaults);
            RestoreDefault (ref sharedEdgeWidthLeadingTipDefaults);

            RestoreDefault (ref sharedEdgeTypeTrailingDefaults);
            RestoreDefault (ref sharedEdgeWidthTrailingRootDefaults);
            RestoreDefault (ref sharedEdgeWidthTrailingTipDefaults);

            RestoreDefault (ref sharedMaterialSTDefaults);
            RestoreDefault (ref sharedColorSTOpacityDefaults);
            RestoreDefault (ref sharedColorSTHueDefaults);
            RestoreDefault (ref sharedColorSTSaturationDefaults);
            RestoreDefault (ref sharedColorSTBrightnessDefaults);

            RestoreDefault (ref sharedMaterialSBDefaults);
            RestoreDefault (ref sharedColorSBOpacityDefaults);
            RestoreDefault (ref sharedColorSBHueDefaults);
            RestoreDefault (ref sharedColorSBSaturationDefaults);
            RestoreDefault (ref sharedColorSBBrightnessDefaults);

            RestoreDefault (ref sharedMaterialETDefaults);
            RestoreDefault (ref sharedColorETOpacityDefaults);
            RestoreDefault (ref sharedColorETHueDefaults);
            RestoreDefault (ref sharedColorETSaturationDefaults);
            RestoreDefault (ref sharedColorETBrightnessDefaults);

            RestoreDefault (ref sharedMaterialELDefaults);
            RestoreDefault (ref sharedColorELOpacityDefaults);
            RestoreDefault (ref sharedColorELHueDefaults);
            RestoreDefault (ref sharedColorELSaturationDefaults);
            RestoreDefault (ref sharedColorELBrightnessDefaults);
        }

        private void RestoreDefault (ref Vector4 set)
        {
            set = new Vector4 (set.z, set.w, set.z, set.w);
        }

        private float GetDefault (Vector4 set)
        {
            if (!isCtrlSrf) return set.x;
            else return set.y;
        }




        // Fuel configuration switching
        // Has to be situated here as this KSPEvent is not correctly added Part.Events otherwise

        [KSPEvent (guiActive = false, guiActiveEditor = false, guiName = "Next configuration", active = false)]
        public void NextConfiguration ()
        {
            if (WingProceduralDebugValues.logFuel) DebugLogWithID ("NextConfiguration", "Started");
            if (isCtrlSrf || isWingAsCtrlSrf || assemblyRFUsed) return;
            fuelSelectedTankSetup++;
            if (fuelSelectedTankSetup >= fuelTankList.Count)
            {
                fuelSelectedTankSetup = 0;
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                fuelCurrentAmount = Vector4.zero;
            }
            FuelAssignResourcesToPart (true);
        }




        // Inheritance

        private bool inheritancePossibleOnShape = false;
        private bool inheritancePossibleOnMaterials = false;

        private void InheritanceStatusUpdate ()
        {
            if (this.part.parent != null)
            {
                if (part.parent.Modules.Contains ("WingProcedural"))
                {
                    var parentModule = part.parent.Modules.OfType<WingProcedural> ().FirstOrDefault ();
                    if (parentModule != null)
                    {
                        if (!parentModule.isCtrlSrf)
                        {
                            inheritancePossibleOnMaterials = true;
                            if (!isCtrlSrf) inheritancePossibleOnShape = true;
                        }
                    }
                }
            }
        }

        private void InheritParentValues (int mode)
        {
            if (this.part.parent != null)
            {
                if (part.parent.Modules.Contains ("WingProcedural"))
                {
                    var parentModule = part.parent.Modules.OfType<WingProcedural> ().FirstOrDefault ();
                    if (parentModule != null)
                    {
                        if (mode == 0 && !parentModule.isCtrlSrf && !isCtrlSrf)
                        {
                            float widthDelta = parentModule.sharedBaseWidthTip - sharedBaseWidthRoot;
                            sharedBaseWidthTip = ((parentModule.sharedBaseWidthTip - parentModule.sharedBaseWidthRoot) / (parentModule.sharedBaseLength)) * (sharedBaseLength + parentModule.sharedBaseLength) + parentModule.sharedBaseWidthRoot - widthDelta; // All thanks to ferram4
                            
                            // Only perform offset adjustment if width wasn't clipped, otherwise there is no point
                            if (sharedBaseWidthTip < sharedBaseWidthLimits.x) sharedBaseWidthTip = sharedBaseWidthLimits.x;
                            else sharedBaseOffsetTip = parentModule.sharedBaseOffsetTip * (sharedBaseLength / parentModule.sharedBaseLength); 

                            sharedBaseThicknessRoot = parentModule.sharedBaseThicknessTip;
                            sharedBaseThicknessTip = Mathf.Min (sharedBaseThicknessRoot, sharedBaseThicknessTip);
                        }
                        if (mode == 1 && !parentModule.isCtrlSrf && !isCtrlSrf)
                        {
                            sharedBaseWidthRoot = parentModule.sharedBaseWidthTip;
                        }
                        if (mode == 2 && !parentModule.isCtrlSrf && !isCtrlSrf)
                        {
                            sharedEdgeTypeLeading = parentModule.sharedEdgeTypeLeading;
                            sharedEdgeWidthLeadingRoot = parentModule.sharedEdgeWidthLeadingTip;
                            sharedEdgeWidthLeadingTip = Mathf.Min (sharedEdgeWidthLeadingRoot, sharedEdgeWidthLeadingTip);

                            sharedEdgeTypeTrailing = parentModule.sharedEdgeTypeTrailing;
                            sharedEdgeWidthTrailingRoot = parentModule.sharedEdgeWidthTrailingTip;
                            sharedEdgeWidthTrailingTip = Mathf.Min (sharedEdgeWidthTrailingRoot, sharedEdgeWidthTrailingTip);
                        }
                        else if (mode == 3)
                        {
                            sharedMaterialST = parentModule.sharedMaterialST;
                            sharedColorSTOpacity = parentModule.sharedColorSTOpacity;
                            sharedColorSTHue = parentModule.sharedColorSTHue;
                            sharedColorSTSaturation = parentModule.sharedColorSTSaturation;
                            sharedColorSTBrightness = parentModule.sharedColorSTBrightness;

                            sharedMaterialSB = parentModule.sharedMaterialSB;
                            sharedColorSBOpacity = parentModule.sharedColorSBOpacity;
                            sharedColorSBHue = parentModule.sharedColorSBHue;
                            sharedColorSBSaturation = parentModule.sharedColorSBSaturation;
                            sharedColorSBBrightness = parentModule.sharedColorSBBrightness;

                            sharedMaterialET = parentModule.sharedMaterialET;
                            sharedColorETOpacity = parentModule.sharedColorETOpacity;
                            sharedColorETHue = parentModule.sharedColorETHue;
                            sharedColorETSaturation = parentModule.sharedColorETSaturation;
                            sharedColorETBrightness = parentModule.sharedColorETBrightness;

                            sharedMaterialEL = parentModule.sharedMaterialEL;
                            sharedColorELOpacity = parentModule.sharedColorELOpacity;
                            sharedColorELHue = parentModule.sharedColorELHue;
                            sharedColorELSaturation = parentModule.sharedColorELSaturation;
                            sharedColorELBrightness = parentModule.sharedColorELBrightness;
                        }
                    }
                }
            }
        }




        // Mod detection

        public static bool assembliesChecked = false;
        public static bool assemblyFARUsed = false;
        public static bool assemblyNEARUsed = false;
        public static bool assemblyFARMass = false;
        public static bool assemblyDREUsed = false;
        public static bool assemblyRFUsed = false;

        public void CheckAssemblies (bool forced)
        {
            if (!assembliesChecked || forced)
            {
                assemblyFARUsed = AssemblyLoader.loadedAssemblies.Any (a => a.assembly.GetName ().Name.Equals ("FerramAerospaceResearch", StringComparison.InvariantCultureIgnoreCase));
                assemblyNEARUsed = AssemblyLoader.loadedAssemblies.Any (a => a.assembly.GetName ().Name.Equals ("NEAR", StringComparison.InvariantCultureIgnoreCase));
                assemblyRFUsed = AssemblyLoader.loadedAssemblies.Any (a => a.assembly.GetName ().Name.Equals ("RealFuels", StringComparison.InvariantCultureIgnoreCase));
                assemblyDREUsed = AssemblyLoader.loadedAssemblies.Any (a => a.assembly.GetName ().Name.Equals ("DeadlyReentry", StringComparison.InvariantCultureIgnoreCase));
                if (assemblyFARUsed || assemblyNEARUsed)
                {
                    ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes ("FARAeroData");
                    for (int i = 0; i < nodes.Length; ++i)
                    {
                        if (nodes[i] == null) continue;
                        if (nodes[i].HasValue ("massPerWingAreaSupported")) assemblyFARMass = true;
                    }
                }
                if (WingProceduralDebugValues.logEvents) DebugLogWithID ("CheckAssemblies", "Search results | FAR: " + assemblyFARUsed + " | NEAR: " + assemblyNEARUsed + " | FAR mass: " + assemblyFARMass + " | DRE: " + assemblyDREUsed + " | RF: " + assemblyRFUsed);
                if (isCtrlSrf && isWingAsCtrlSrf && WingProceduralDebugValues.logEvents) DebugLogWithID ("CheckAssemblies", "WARNING | PART IS CONFIGURED INCORRECTLY, BOTH BOOL PROPERTIES SHOULD NEVER BE SET TO TRUE");
                assembliesChecked = true;
            }
        }




        // Startup

        public override void OnStart (PartModule.StartState state)
        {
            if (WingProceduralDebugValues.logEvents) DebugLogWithID ("OnStart", "Invoked");
            base.OnStart (state);
            CheckAssemblies (true);
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

        public void Start ()
        {
            if (WingProceduralDebugValues.logEvents) DebugLogWithID ("Start", "Invoked");
            if (HighLogic.LoadedSceneIsEditor)
            {
                uiInstanceIDLocal = uiInstanceIDTarget = 0;
                if (!WingProceduralManager.uiStyleConfigured) WingProceduralManager.ConfigureStyles ();
                RenderingManager.AddToPostDrawQueue (0, OnDraw);
            }
        }




        // Update

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
                    // Compare the properties to cached values
                    // If there is a mismatch, then update is required

                    // CheckFieldGroup (sharedFieldGroupBase, ref sharedFieldGroupBaseCached, ref sharedFieldGroupBaseStatic, sharedFieldGroupBaseArray, false, groupEntriesCtrl: sharedFieldGroupBaseArrayCtrl);
                    CheckFieldValue (sharedBaseLength, ref sharedBaseLengthCached, true);
                    CheckFieldValue (sharedBaseWidthRoot, ref sharedBaseWidthRootCached, true);
                    CheckFieldValue (sharedBaseWidthTip, ref sharedBaseWidthTipCached, true);
                    CheckFieldValue (sharedBaseThicknessRoot, ref sharedBaseThicknessRootCached, true);
                    CheckFieldValue (sharedBaseThicknessTip, ref sharedBaseThicknessTipCached, true);
                    CheckFieldValue (sharedBaseOffsetRoot, ref sharedBaseOffsetRootCached, true);
                    CheckFieldValue (sharedBaseOffsetTip, ref sharedBaseOffsetTipCached, true);

                    // CheckFieldGroup (sharedFieldGroupEdgeTrailing, ref sharedFieldGroupEdgeTrailingCached, ref sharedFieldGroupEdgeTrailingStatic, sharedFieldGroupEdgeTrailingArray, false);
                    CheckFieldValue (sharedEdgeTypeTrailing, ref sharedEdgeTypeTrailingCached, false);
                    CheckFieldValue (sharedEdgeWidthTrailingRoot, ref sharedEdgeWidthTrailingRootCached, true);
                    CheckFieldValue (sharedEdgeWidthTrailingTip, ref sharedEdgeWidthTrailingTipCached, true);

                    // CheckFieldGroup (sharedFieldGroupEdgeLeading, ref sharedFieldGroupEdgeLeadingCached, ref sharedFieldGroupEdgeLeadingStatic, sharedFieldGroupEdgeLeadingArray, false);
                    CheckFieldValue (sharedEdgeTypeLeading, ref sharedEdgeTypeLeadingCached, false);
                    CheckFieldValue (sharedEdgeWidthLeadingRoot, ref sharedEdgeWidthLeadingRootCached, true);
                    CheckFieldValue (sharedEdgeWidthLeadingTip, ref sharedEdgeWidthLeadingTipCached, true);

                    // CheckFieldGroup (sharedFieldGroupColorST, ref sharedFieldGroupColorSTCached, ref sharedFieldGroupColorSTStatic, sharedFieldGroupColorSTArray, false);
                    CheckFieldValue (sharedMaterialST, ref sharedMaterialSTCached, false);
                    CheckFieldValue (sharedColorSTOpacity, ref sharedColorSTOpacityCached, false);
                    CheckFieldValue (sharedColorSTHue, ref sharedColorSTHueCached, false);
                    CheckFieldValue (sharedColorSTSaturation, ref sharedColorSTSaturationCached, false);
                    CheckFieldValue (sharedColorSTBrightness, ref sharedColorSTBrightnessCached, false);

                    // CheckFieldGroup (sharedFieldGroupColorSB, ref sharedFieldGroupColorSBCached, ref sharedFieldGroupColorSBStatic, sharedFieldGroupColorSBArray, false);
                    CheckFieldValue (sharedMaterialSB, ref sharedMaterialSBCached, false);
                    CheckFieldValue (sharedColorSBOpacity, ref sharedColorSBOpacityCached, false);
                    CheckFieldValue (sharedColorSBHue, ref sharedColorSBHueCached, false);
                    CheckFieldValue (sharedColorSBSaturation, ref sharedColorSBSaturationCached, false);
                    CheckFieldValue (sharedColorSBBrightness, ref sharedColorSBBrightnessCached, false);

                    // CheckFieldGroup (sharedFieldGroupColorET, ref sharedFieldGroupColorETCached, ref sharedFieldGroupColorETStatic, sharedFieldGroupColorETArray, false);
                    CheckFieldValue (sharedMaterialET, ref sharedMaterialETCached, false);
                    CheckFieldValue (sharedColorETOpacity, ref sharedColorETOpacityCached, false);
                    CheckFieldValue (sharedColorETHue, ref sharedColorETHueCached, false);
                    CheckFieldValue (sharedColorETSaturation, ref sharedColorETSaturationCached, false);
                    CheckFieldValue (sharedColorETBrightness, ref sharedColorETBrightnessCached, false);

                    // CheckFieldGroup (sharedFieldGroupColorEL, ref sharedFieldGroupColorELCached, ref sharedFieldGroupColorELStatic, sharedFieldGroupColorELArray, false);
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
                if (!isCtrlSrf && !isWingAsCtrlSrf) FuelOnUpdate ();
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
                if (WingProceduralDebugValues.logUpdate) DebugLogWithID ("Update", "Detected value change");
                updateRequiredOnGeometry = true;
                if (affectsAerodynamics) updateRequiredOnAerodynamics = true;
                fieldCache = fieldValue;
            }
        }

        /*
        private void CheckFieldGroup (bool groupStatus, ref bool groupCache, ref bool groupStatic, string[] groupEntries, bool skipCheck, string[] groupEntriesWing = null, string[] groupEntriesCtrl = null) 
        {
            groupCache = groupStatus;
            groupStatic = groupStatus;
            if (!skipCheck)
            {
                if (groupStatus != groupCache)
                {
                    if (WingProceduralDebugValues.logUpdate) DebugLogWithID ("Update", "Detected field group state change");
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
        */




        // Attachment handling

        private Callback CachedOnEditorAttach;
        private Callback CachedOnEditorDetach;

        public void UpdateOnEditorAttach ()
        {
            isAttached = true;
            if (WingProceduralDebugValues.logEvents) DebugLogWithID ("UpdateOnEditorAttach", "Setup started");
            Setup ();
            isStarted = true;
            if (WingProceduralDebugValues.logEvents) DebugLogWithID ("UpdateOnEditorAttach", "Setup ended");
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
            if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Started | isCtrlSrf: " + isCtrlSrf);
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
                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing section | Passed array setup");

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
                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing section | Finished");
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

                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing surface top | Passed array setup");
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
                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing surface | Finished");
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

                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge trailing | Passed array setup");
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
                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge trailing | Finished");
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

                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge leading | Passed array setup");
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
                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Wing edge leading | Finished");
                }
            }
            else
            {
                // Some reusable values

                // float ctrlOffsetRootLimit = (sharedBaseLength / 2f) / (sharedBaseWidthRoot + sharedEdgeWidthTrailingRoot);
                // float ctrlOffsetTipLimit = (sharedBaseLength / 2f) / (sharedBaseWidthTip + sharedEdgeWidthTrailingTip);

                float ctrlOffsetRootClamped = Mathf.Clamp (sharedBaseOffsetRoot, sharedBaseOffsetLimits.z, sharedBaseOffsetLimits.w + 0.15f); // Mathf.Clamp (sharedBaseOffsetRoot, sharedBaseOffsetLimits.z, ctrlOffsetRootLimit - 0.075f);
                float ctrlOffsetTipClamped = Mathf.Clamp (sharedBaseOffsetTip, Mathf.Max (sharedBaseOffsetLimits.z - 0.15f, ctrlOffsetRootClamped - sharedBaseLength), sharedBaseOffsetLimits.w); // Mathf.Clamp (sharedBaseOffsetTip, -ctrlOffsetTipLimit + 0.075f, sharedBaseOffsetLimits.w);

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

                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface frame | Passed array setup");
                    for (int i = 0; i < vp.Length; ++i)
                    {
                        // Thickness correction (X), edge width correction (Y) and span-based offset (Z)
                        if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationTip, vp[i].y, vp[i].z + 0.5f - sharedBaseLength / 2f); // if (vp[i].z < 0f) vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationTip, ((vp[i].y + 0.5f) * ctrlEdgeWidthDeviationTip), vp[i].z + 0.5f - sharedBaseLength / 2f);
                        else vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationRoot, vp[i].y, vp[i].z - 0.5f + sharedBaseLength / 2f); // else vp[i] = new Vector3 (vp[i].x * ctrlThicknessDeviationRoot, ((vp[i].y + 0.5f) * ctrlEdgeWidthDeviationRoot), vp[i].z - 0.5f + sharedBaseLength / 2f);
                        

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
                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface frame | Finished");
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

                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface edge | Passed array setup");
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
                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface edge | Finished");
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

                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface top | Passed array setup");
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
                    if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Control surface top | Finished");
                }
            }
            if (WingProceduralDebugValues.logUpdateGeometry) DebugLogWithID ("UpdateGeometry", "Finished");
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
            else if (WingProceduralDebugValues.logUpdateMaterials) DebugLogWithID ("UpdateMaterials", "Material creation failed");
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
            else if (WingProceduralDebugValues.logUpdateMaterials) DebugLogWithID ("SetMaterialReferences", "Surface textures not found");

            if (materialLayeredEdgeTextureMain != null && materialLayeredEdgeTextureMask != null)
            {
                materialLayeredEdge.SetTexture ("_MainTex", materialLayeredEdgeTextureMain);
                materialLayeredEdge.SetTexture ("_Emissive", materialLayeredEdgeTextureMask);
                materialLayeredEdge.SetFloat ("_Shininess", materialPropertyShininess);
                materialLayeredEdge.SetColor ("_SpecColor", materialPropertySpecular);
            }
            else if (WingProceduralDebugValues.logUpdateMaterials) DebugLogWithID ("SetMaterialReferences", "Edge textures not found");
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
                    if (WingProceduralDebugValues.logUpdateMaterials) DebugLogWithID ("SetTextures", "Main: " + materialLayeredSurfaceTextureMain.ToString () + " | Mask: " + materialLayeredSurfaceTextureMask);
                }
            }
            if (sourceEdge != null)
            {
                Renderer r = sourceEdge.gameObject.GetComponent<Renderer> ();
                if (r != null)
                {
                    materialLayeredEdgeTextureMain = r.sharedMaterial.GetTexture ("_MainTex");
                    materialLayeredEdgeTextureMask = r.sharedMaterial.GetTexture ("_Emissive");
                    if (WingProceduralDebugValues.logUpdateMaterials) DebugLogWithID ("SetTextures", "Main: " + materialLayeredEdgeTextureMain.ToString () + " | Mask: " + materialLayeredEdgeTextureMask);
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
            FuelOnStart ();
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
            // Woah, looks like this is unnecessary
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
                    meshFiltersWingEdgeTrailing.Add (meshFilterWingEdgeTrailing);

                    MeshFilter meshFilterWingEdgeLeading = CheckMeshFilter ("edge_leading_type" + i);
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

        private int GetFieldMode ()
        {
            if (!isCtrlSrf) return 1;
            else return 2;
        }

        private void SetupFields ()
        {
            // SetFieldVisibility ("sharedFieldGroupBase", true);
            SetFieldType ("sharedBaseLength", 1, GetLimitsFromType (sharedBaseLengthLimits), sharedIncrementMain, false, GetDefault (sharedBaseLengthDefaults));
            SetFieldType ("sharedBaseWidthRoot", GetFieldMode (), GetLimitsFromType (sharedBaseWidthLimits), sharedIncrementMain, false, GetDefault (sharedBaseWidthRootDefaults));
            SetFieldType ("sharedBaseWidthTip", GetFieldMode (), GetLimitsFromType (sharedBaseWidthLimits), sharedIncrementMain, false, GetDefault (sharedBaseWidthTipDefaults));
            SetFieldType ("sharedBaseThicknessRoot", 2, sharedBaseThicknessLimits, sharedIncrementSmall, false, GetDefault (sharedBaseThicknessRootDefaults));
            SetFieldType ("sharedBaseThicknessTip", 2, sharedBaseThicknessLimits, sharedIncrementSmall, false, GetDefault (sharedBaseThicknessTipDefaults));
            SetFieldType ("sharedBaseOffsetRoot", GetFieldMode (), GetLimitsFromType (sharedBaseOffsetLimits), GetIncrementFromType (sharedIncrementMain, sharedIncrementSmall), false, GetDefault (sharedBaseOffsetRootDefaults));
            SetFieldType ("sharedBaseOffsetTip", GetFieldMode (), GetLimitsFromType (sharedBaseOffsetLimits), GetIncrementFromType (sharedIncrementMain, sharedIncrementSmall), false, GetDefault (sharedBaseOffsetTipDefaults));

            // SetFieldVisibility ("sharedFieldGroupEdgeTrailing", true);
            SetFieldType ("sharedEdgeTypeTrailing", 2, GetLimitsFromType (sharedEdgeTypeLimits), sharedIncrementInt, false, GetDefault (sharedEdgeTypeTrailingDefaults));
            SetFieldType ("sharedEdgeWidthTrailingRoot", 2, GetLimitsFromType (sharedEdgeWidthLimits), sharedIncrementSmall, false, GetDefault (sharedEdgeWidthTrailingRootDefaults));
            SetFieldType ("sharedEdgeWidthTrailingTip", 2, GetLimitsFromType (sharedEdgeWidthLimits), sharedIncrementSmall, false, GetDefault (sharedEdgeWidthTrailingTipDefaults));

            // SetFieldVisibility ("sharedFieldGroupEdgeLeading", !isCtrlSrf);
            SetFieldType ("sharedEdgeTypeLeading", 2, GetLimitsFromType (sharedEdgeTypeLimits), sharedIncrementInt, false, GetDefault (sharedEdgeTypeLeadingDefaults));
            SetFieldType ("sharedEdgeWidthLeadingRoot", 2, GetLimitsFromType (sharedEdgeWidthLimits), sharedIncrementSmall, false, GetDefault (sharedEdgeWidthLeadingRootDefaults));
            SetFieldType ("sharedEdgeWidthLeadingTip", 2, GetLimitsFromType (sharedEdgeWidthLimits), sharedIncrementSmall, false, GetDefault (sharedEdgeWidthLeadingTipDefaults));

            // SetFieldVisibility ("sharedFieldGroupColorST", true);
            SetFieldType ("sharedMaterialST", 2, sharedMaterialLimits, sharedIncrementInt, false, GetDefault (sharedMaterialSTDefaults));
            SetFieldType ("sharedColorSTOpacity", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSTOpacityDefaults));
            SetFieldType ("sharedColorSTHue", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSTHueDefaults));
            SetFieldType ("sharedColorSTSaturation", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSTSaturationDefaults));
            SetFieldType ("sharedColorSTBrightness", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSTBrightnessDefaults));

            // SetFieldVisibility ("sharedFieldGroupColorSB", true);
            SetFieldType ("sharedMaterialSB", 2, sharedMaterialLimits, sharedIncrementInt, false, GetDefault (sharedMaterialSBDefaults));
            SetFieldType ("sharedColorSBOpacity", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSBOpacityDefaults));
            SetFieldType ("sharedColorSBHue", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSBHueDefaults));
            SetFieldType ("sharedColorSBSaturation", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSBSaturationDefaults));
            SetFieldType ("sharedColorSBBrightness", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSBBrightnessDefaults));

            // SetFieldVisibility ("sharedFieldGroupColorET", true);
            SetFieldType ("sharedMaterialET", 2, sharedMaterialLimits, sharedIncrementInt, false, GetDefault (sharedMaterialETDefaults));
            SetFieldType ("sharedColorETOpacity", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorETOpacityDefaults));
            SetFieldType ("sharedColorETHue", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorETHueDefaults));
            SetFieldType ("sharedColorETSaturation", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorETSaturationDefaults));
            SetFieldType ("sharedColorETBrightness", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorETBrightnessDefaults));

            // SetFieldVisibility ("sharedFieldGroupColorEL", !isCtrlSrf);
            SetFieldType ("sharedMaterialEL", 2, sharedMaterialLimits, sharedIncrementInt, false, GetDefault (sharedMaterialELDefaults));
            SetFieldType ("sharedColorELOpacity", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorELOpacityDefaults));
            SetFieldType ("sharedColorELHue", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorELHueDefaults));
            SetFieldType ("sharedColorELSaturation", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorELSaturationDefaults));
            SetFieldType ("sharedColorELBrightness", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorELBrightnessDefaults));

            updateRequiredOnWindow = true;
            isSetToDefaultValues = true;
        }

        private void SetFieldType (string name, int type, Vector2 limits, float increment, bool visible, float defaultValue)
        {
            // BaseField field = Fields[name];
            // UI_FloatEdit ui = (UI_FloatEdit) field.uiControlEditor;

            float value = (float) this.GetType ().GetField (name).GetValue (this);
            if (WingProceduralDebugValues.logFieldSetup) DebugLogWithID ("SetFieldType", "Started for field " + name + " | UI type: " + type + " | Limits: " + limits.x + "-" + limits.y + " | Increment: " + increment + " | Current value: " + value);

            /*
            if (type == 1)
            {
                field.guiFormat = "S4";
                ui.minValue = limits.x;
                ui.maxValue = limits.y;
                ui.incrementSlide = increment;
                ui.incrementLarge = 1f;
                ui.scene = UI_Scene.Editor;
            }
            if (type == 2)
            {
                field.guiFormat = "F3";
                ui.minValue = limits.x;
                ui.maxValue = limits.y;
                ui.incrementSlide = increment;
                ui.incrementLarge = 0f;
                ui.scene = UI_Scene.Editor;
            }
            */

            if (!isSetToDefaultValues) this.GetType ().GetField (name).SetValue (this, defaultValue);
            else this.GetType ().GetField (name).SetValue (this, Mathf.Clamp (value, limits.x, limits.y));

            // field.uiControlEditor.controlEnabled = visible;
            // field.guiActiveEditor = visible;
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
                if (WingProceduralDebugValues.logMeshReferences) DebugLogWithID ("SetupMeshReferences", "References missing | isCtrlSrf: " + isCtrlSrf);
                SetupMeshReferencesFromScratch ();
            }
            else
            {
                if (WingProceduralDebugValues.logMeshReferences) DebugLogWithID ("SetupMeshReferences", "Skipped, all references seem to be in order");
            }
        }

        public void ReportOnMeshReferences ()
        {
            if (isCtrlSrf)
            {
                if (WingProceduralDebugValues.logMeshReferences) DebugLogWithID
                (
                    "ReportOnMeshReferences",
                    "Control surface reference length check"
                    + " | Edge: " + meshReferenceCtrlFrame.vp.Length
                    + " | Surface: " + meshReferenceCtrlSurface.vp.Length
                );
            }
            else
            {
                if (WingProceduralDebugValues.logMeshReferences) DebugLogWithID
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
            if (WingProceduralDebugValues.logMeshReferences) DebugLogWithID ("SetupMeshReferencesFromScratch", "No sources found, creating new references");
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
                if (WingProceduralDebugValues.logCheckMeshFilter) DebugLogWithID ("CheckMeshFilter", "Looking for object: " + name);
                Transform parent = part.transform.GetChild (0).GetChild (0).GetChild (0).Find (name);
                if (parent != null)
                {
                    parent.localPosition = Vector3.zero;
                    if (WingProceduralDebugValues.logCheckMeshFilter) DebugLogWithID ("CheckMeshFilter", "Object " + name + " was found");
                    reference = parent.gameObject.GetComponent<MeshFilter> ();
                    if (disable) parent.gameObject.SetActive (false);
                }
                else { if (WingProceduralDebugValues.logCheckMeshFilter) DebugLogWithID ("CheckMeshFilter", "Object " + name + " was not found!"); }
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
            else if (WingProceduralDebugValues.logMeshReferences) DebugLogWithID ("FillMeshReference", "Mesh filter reference is null, unable to set up reference arrays");
            return reference;
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
                    if (WingProceduralDebugValues.logFlightSetup) DebugLogWithID ("SetupReorderedForFlight", "Vessel " + vesselID + " found in the status list");
                    vesselListInclusive = true;
                    vesselStatusIndex = i;
                }
            }

            // If it was not included, we add it to the list
            // Correct index is then fairly obvious

            if (!vesselListInclusive)
            {
                if (WingProceduralDebugValues.logFlightSetup) DebugLogWithID ("SetupReorderedForFlight", "Vessel " + vesselID + " was not found in the status list, adding it");
                vesselList.Add (new VesselStatus (vessel, false));
                vesselStatusIndex = vesselList.Count - 1;
            }

            // Using the index for the status list we obtained, we check whether it was updated yet
            // So that only one part can run the following part

            if (!vesselList[vesselStatusIndex].isUpdated)
            {
                if (WingProceduralDebugValues.logFlightSetup) DebugLogWithID ("SetupReorderedForFlight", "Vessel " + vesselID + " was not updated yet (this message should only appear once)");
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

                if (WingProceduralDebugValues.logFlightSetup) DebugLogWithID ("SetupReorderedForFlight", "Vessel " + vesselID + " contained " + vesselPartsCount + " parts, of which " + moduleList.Count + " should be set up");
                int moduleListCount = moduleList.Count;
                for (int i = 0; i < moduleListCount; ++i)
                {
                    moduleList[i].Setup ();
                }

                yield return new WaitForFixedUpdate ();
                yield return new WaitForFixedUpdate ();

                if (WingProceduralDebugValues.logFlightSetup) DebugLogWithID ("SetupReorderedForFlight", "Vessel " + vesselID + " waited for updates, starting aero value calculation");
                for (int i = 0; i < moduleListCount; ++i)
                {
                    moduleList[i].CalculateAerodynamicValues ();
                }
                yield return null;
            }
        }




        // Aerodynamics value calculation
        // More or less lifted from pWings, so credit goes to DYJ and Taverius

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
        public float aeroStatVolume = 3.84f;

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
                if (WingProceduralDebugValues.logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Started");
                CheckAssemblies (false);

                float sharedWidthTipSum = sharedBaseWidthTip;
                if (!isCtrlSrf)
                {
                    if (sharedEdgeTypeLeading != 1) sharedWidthTipSum += sharedEdgeWidthLeadingTip;
                    if (sharedEdgeTypeTrailing != 1) sharedWidthTipSum += sharedEdgeWidthTrailingTip;
                }
                else sharedWidthTipSum += sharedEdgeWidthTrailingTip;

                float sharedWidthRootSum = sharedBaseWidthRoot;
                if (!isCtrlSrf)
                {
                    if (sharedEdgeTypeLeading != 1) sharedWidthRootSum += sharedEdgeWidthLeadingRoot;
                    if (sharedEdgeTypeTrailing != 1) sharedWidthRootSum += sharedEdgeWidthTrailingRoot;
                }
                else sharedWidthRootSum += sharedEdgeWidthTrailingRoot;

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

                    // This is a proper full volume calculation
                    // But since we're only currently using volume for fuel and it's usually stored in the midsection, width sums are unnecessary

                    // aeroStatVolume = (sharedWidthTipSum * sharedBaseThicknessTip * sharedBaseLength) +
                    // ((sharedWidthRootSum - sharedWidthTipSum) / 2f * sharedBaseThicknessTip * sharedBaseLength) +
                    // (sharedWidthTipSum * (sharedBaseThicknessRoot - sharedBaseThicknessTip) / 2f * sharedBaseLength) +
                    // ((sharedWidthRootSum - sharedWidthTipSum) / 2f * (sharedBaseThicknessRoot - sharedBaseThicknessTip) / 2f * sharedBaseLength);

                    aeroStatVolume = (sharedBaseWidthTip * sharedBaseThicknessTip * sharedBaseLength) +
                    ((sharedBaseWidthRoot - sharedBaseWidthTip) / 2f * sharedBaseThicknessTip * sharedBaseLength) +
                    (sharedBaseWidthTip * (sharedBaseThicknessRoot - sharedBaseThicknessTip) / 2f * sharedBaseLength) +
                    ((sharedBaseWidthRoot - sharedBaseWidthTip) / 2f * (sharedBaseThicknessRoot - sharedBaseThicknessTip) / 2f * sharedBaseLength);

                    if (!isWingAsCtrlSrf) FuelUpdateAmountsFromVolume (aeroStatVolume, true);
                }
                else
                {
                    aeroStatSemispan = (double) sharedBaseLength;
                    aeroStatTaperRatio = (double) (sharedBaseLength + sharedWidthTipSum * ctrlOffsetTipClamped - sharedWidthRootSum * ctrlOffsetRootClamped) / (double) sharedBaseLength;
                    aeroStatMeanAerodynamicChord = (double) (sharedWidthTipSum + sharedWidthRootSum) / 2.0;
                    aeroStatMidChordSweep = MathD.Atan ((double) Mathf.Abs (sharedWidthRootSum - sharedWidthTipSum) / (double) sharedBaseLength) * MathD.Rad2Deg;
                }
                if (WingProceduralDebugValues.logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Passed B2/TR/MAC/MCS");

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
                if (WingProceduralDebugValues.logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Passed SR/AR/ARSS/mass/Cl/Cd/connection");

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
                if (WingProceduralDebugValues.logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Passed cost/force/torque");

                // Stock-only values

                if ((!assemblyFARUsed && !assemblyNEARUsed) || !assemblyFARMass)
                {
                    if (WingProceduralDebugValues.logCAV) DebugLogWithID ("CalculateAerodynamicValues", "FAR/NEAR is inactive or FAR mass is not enabled, calculating stock part mass");
                    part.mass = Mathf.Round ((float) aeroStatMass * 100f) / 100f;
                }
                if (!assemblyFARUsed && !assemblyNEARUsed)
                {
                    if (!isCtrlSrf && !isWingAsCtrlSrf)
                    {
                        if (WingProceduralDebugValues.logCAV) DebugLogWithID ("CalculateAerodynamicValues", "FAR/NEAR is inactive, calculating values for winglet part type");
                        ((Winglet) this.part).deflectionLiftCoeff = Mathf.Round ((float) aeroStatCl * 100f) / 100f;
                        ((Winglet) this.part).dragCoeff = Mathf.Round ((float) aeroStatCd * 100f) / 100f;
                    }
                    else
                    {
                        if (WingProceduralDebugValues.logCAV) DebugLogWithID ("CalculateAerodynamicValues", "FAR/NEAR is inactive, calculating stock control surface module values");
                        var mCtrlSrf = part.Modules.OfType<ModuleControlSurface> ().FirstOrDefault ();
                        mCtrlSrf.deflectionLiftCoeff = Mathf.Round ((float) aeroStatCl * 100f) / 100f;
                        mCtrlSrf.dragCoeff = Mathf.Round ((float) aeroStatCd * 100f) / 100f;
                        mCtrlSrf.ctrlSurfaceArea = aeroConstControlSurfaceFraction;
                    }
                }
                if (WingProceduralDebugValues.logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Passed stock drag/deflection/area");

                // FAR values
                // With reflection stuff from r4m0n

                if (assemblyFARUsed || assemblyNEARUsed)
                {
                    if (WingProceduralDebugValues.logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Got into FAR/NEAR condition");
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
                        if (assemblyNEARUsed) typeFAR.GetMethod ("MathAndFunctionInitialization").Invoke (moduleFAR, null);
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
                        if (assemblyNEARUsed) typeFAR.GetMethod ("MathAndFunctionInitialization").Invoke (moduleFAR, null);
                        else typeFAR.GetMethod ("StartInitialization").Invoke (moduleFAR, null);
                    }
                }
                if (WingProceduralDebugValues.logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Passed FAR/NEAR parameter setting");

                // Update GUI values and finish

                if (!assemblyFARUsed && !assemblyNEARUsed)
                {
                    aeroUICd = Mathf.Round ((float) aeroStatCd * 100f) / 100f;
                    aeroUICl = Mathf.Round ((float) aeroStatCl * 100f) / 100f;
                }
                if ((!assemblyFARUsed && !assemblyNEARUsed) || !assemblyFARMass) aeroUIMass = part.mass;

                aeroUIMeanAerodynamicChord = (float) aeroStatMeanAerodynamicChord;
                aeroUISemispan = (float) aeroStatSemispan;
                aeroUIMidChordSweep = (float) aeroStatMidChordSweep;
                aeroUITaperRatio = (float) aeroStatTaperRatio;
                aeroUISurfaceArea = (float) aeroStatSurfaceArea;
                aeroUIAspectRatio = (float) aeroStatAspectRatio;
                if (HighLogic.LoadedSceneIsEditor) GameEvents.onEditorShipModified.Fire (EditorLogic.fetch.ship);
                if (WingProceduralDebugValues.logCAV) DebugLogWithID ("CalculateAerodynamicValues", "Finished");
            }
        }

        private void UpdateCollidersForFAR ()
        {
            if (assemblyFARUsed && assemblyNEARUsed)
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
            if (isAttached && !assemblyFARUsed)
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
                if (!assemblyFARUsed && !assemblyNEARUsed)
                {
                    Fields["aeroUICd"].guiActiveEditor = showWingData;
                    Fields["aeroUICl"].guiActiveEditor = showWingData;
                }

                // If FAR|NEAR are not present, or its a version without wing mass calculations, toggle wing mass
                if ((!assemblyFARUsed && !assemblyNEARUsed) || !assemblyFARMass)
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




        // [KSPEvent (guiActive = true, guiActiveEditor = true, guiName = "Dump interaction data")]
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

        public KeyCode uiKeyCodeEdit = KeyCode.J;
        public static Rect uiRect = new Rect ();
        public static bool uiWindowActive = true;
        public static float uiMouseDeltaCache = 0f;

        public static int uiInstanceIDTarget = 0;
        private int uiInstanceIDLocal = 0;

        public static int uiPropertySelectionWing = 0;
        public static int uiPropertySelectionSurface = 0;

        public static bool uiEditMode = false;
        public static bool uiAdjustWindow = true;
        public static bool uiEditModeTimeout = false;
        private float uiEditModeTimeoutDuration = 0.25f;
        private float uiEditModeTimer = 0f;

        private void OnDraw ()
        {
            if (uiInstanceIDLocal == 0) uiInstanceIDLocal = part.GetInstanceID ();
            if (uiInstanceIDTarget == uiInstanceIDLocal || uiInstanceIDTarget == 0)
            {
                if (!WingProceduralManager.uiStyleConfigured) WingProceduralManager.ConfigureStyles ();
                if (WingProceduralManager.uiStyleConfigured)
                {
                    if (uiWindowActive)
                    {
                        if (uiAdjustWindow)
                        {
                            uiAdjustWindow = false;
                            if (WingProceduralDebugValues.logPropertyWindow) DebugLogWithID ("OnDraw", "Window forced to adjust");
                            uiRect = GUILayout.Window (273, uiRect, OnWindow, GetWindowTitle (), WingProceduralManager.uiStyleWindow, GUILayout.Height (0));
                        }
                        else
                            uiRect = GUILayout.Window (273, uiRect, OnWindow, GetWindowTitle (), WingProceduralManager.uiStyleWindow);
                        if (uiRect.x == 0f && uiRect.y == 0f) uiRect = uiRect.SetToScreenCenter ();

                        // Thanks to ferram4
                        // Following section lock the editor, preventing window clickthrough

                        EditorLogic editorLogicInstance = EditorLogic.fetch;
                        bool cursorInGUI = false;
                        cursorInGUI = uiRect.Contains (UIUtility.GetMousePos ());
                        if (cursorInGUI)
                        {
                            editorLogicInstance.Lock (false, false, false, "WingProceduralWindow");
                            EditorTooltip.Instance.HideToolTip ();
                        }
                        else if (!cursorInGUI) editorLogicInstance.Unlock ("WingProceduralWindow");
                    }
                }
            }
        }

        public static Vector4 uiColorSliderBase = new Vector4 (0.25f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderEdgeL = new Vector4 (0.20f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderEdgeT = new Vector4 (0.15f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsST = new Vector4 (0.10f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsSB = new Vector4 (0.05f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsET = new Vector4 (0.00f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsEL = new Vector4 (0.95f, 0.5f, 0.4f, 1f);

        private void OnWindow (int window)
        {
            if (uiEditMode)
            {

                bool returnEarly = false;
                GUILayout.BeginHorizontal ();
                GUILayout.BeginVertical ();
                if (uiLastFieldName.Length > 0) GUILayout.Label ("Last: " + uiLastFieldName, WingProceduralManager.uiStyleLabelMedium);
                else GUILayout.Label ("Property editor", WingProceduralManager.uiStyleLabelMedium);
                if (uiLastFieldTooltip.Length > 0) GUILayout.Label (uiLastFieldTooltip + "\n_________________________", WingProceduralManager.uiStyleLabelHint, GUILayout.MaxHeight (44f), GUILayout.MinHeight (44f)); // 58f for four lines
                GUILayout.EndVertical ();
                if (GUILayout.Button ("Close", WingProceduralManager.uiStyleButton, GUILayout.MaxWidth (50f)))
                {
                    EditorLogic.fetch.Unlock ("WingProceduralWindow");
                    uiWindowActive = false;
                    returnEarly = true;
                }
                GUILayout.EndHorizontal ();
                if (returnEarly) return;

                DrawFieldGroupHeader (ref sharedFieldGroupBaseStatic, "Base");
                if (sharedFieldGroupBaseStatic)
                {
                    DrawField (ref sharedBaseLength, sharedIncrementMain, GetLimitsFromType (sharedBaseLengthLimits), "Length", uiColorSliderBase, 0, 0);
                    DrawField (ref sharedBaseWidthRoot, sharedIncrementMain, GetLimitsFromType (sharedBaseWidthLimits), "Width (root)", uiColorSliderBase, 1, 0);
                    DrawField (ref sharedBaseWidthTip, sharedIncrementMain, GetLimitsFromType (sharedBaseWidthLimits), "Width (tip)", uiColorSliderBase, 2, 0);
                    if (isCtrlSrf) DrawField (ref sharedBaseOffsetRoot, GetIncrementFromType (sharedIncrementMain, sharedIncrementSmall), GetLimitsFromType (sharedBaseOffsetLimits), "Offset (root)", uiColorSliderBase, 3, 0);
                    DrawField (ref sharedBaseOffsetTip, GetIncrementFromType (sharedIncrementMain, sharedIncrementSmall), GetLimitsFromType (sharedBaseOffsetLimits), "Offset (tip)", uiColorSliderBase, 4, 0);
                    DrawField (ref sharedBaseThicknessRoot, sharedIncrementSmall, sharedBaseThicknessLimits, "Thickness (root)", uiColorSliderBase, 5, 0);
                    DrawField (ref sharedBaseThicknessTip, sharedIncrementSmall, sharedBaseThicknessLimits, "Thickness (tip)", uiColorSliderBase, 6, 0);
                }

                if (!isCtrlSrf)
                {
                    DrawFieldGroupHeader (ref sharedFieldGroupEdgeLeadingStatic, "Edge (leading)");
                    if (sharedFieldGroupEdgeLeadingStatic)
                    {
                        DrawField (ref sharedEdgeTypeLeading, sharedIncrementInt, GetLimitsFromType (sharedEdgeTypeLimits), "Shape", uiColorSliderEdgeL, 7, 2);
                        DrawField (ref sharedEdgeWidthLeadingRoot, sharedIncrementSmall, GetLimitsFromType (sharedEdgeWidthLimits), "Width (root)", uiColorSliderEdgeL, 8, 0);
                        DrawField (ref sharedEdgeWidthLeadingTip, sharedIncrementSmall, GetLimitsFromType (sharedEdgeWidthLimits), "Width (tip)", uiColorSliderEdgeL, 9, 0);
                    }
                }

                DrawFieldGroupHeader (ref sharedFieldGroupEdgeTrailingStatic, "Edge (trailing)");
                if (sharedFieldGroupEdgeTrailingStatic)
                {
                    DrawField (ref sharedEdgeTypeTrailing, sharedIncrementInt, GetLimitsFromType (sharedEdgeTypeLimits), "Shape", uiColorSliderEdgeT, 10, isCtrlSrf ? 3 : 2);
                    DrawField (ref sharedEdgeWidthTrailingRoot, sharedIncrementSmall, GetLimitsFromType (sharedEdgeWidthLimits), "Width (root)", uiColorSliderEdgeT, 11, 0);
                    DrawField (ref sharedEdgeWidthTrailingTip, sharedIncrementSmall, GetLimitsFromType (sharedEdgeWidthLimits), "Width (tip)", uiColorSliderEdgeT, 12, 0);
                }

                DrawFieldGroupHeader (ref sharedFieldGroupColorSTStatic, "Surface (top)");
                if (sharedFieldGroupColorSTStatic)
                {
                    DrawField (ref sharedMaterialST, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsST, 13, 1);
                    DrawField (ref sharedColorSTOpacity, sharedIncrementColor, sharedColorLimits, "Opacity", uiColorSliderColorsST, 14, 0);
                    DrawField (ref sharedColorSTHue, sharedIncrementColor, sharedColorLimits, "Hue", uiColorSliderColorsST, 15, 0);
                    DrawField (ref sharedColorSTSaturation, sharedIncrementColor, sharedColorLimits, "Saturation", uiColorSliderColorsST, 16, 0);
                    DrawField (ref sharedColorSTBrightness, sharedIncrementColor, sharedColorLimits, "Brightness", uiColorSliderColorsST, 17, 0);
                }

                DrawFieldGroupHeader (ref sharedFieldGroupColorSBStatic, "Surface (bottom)");
                if (sharedFieldGroupColorSBStatic)
                {
                    DrawField (ref sharedMaterialSB, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsSB, 13, 1);
                    DrawField (ref sharedColorSBOpacity, sharedIncrementColor, sharedColorLimits, "Opacity", uiColorSliderColorsSB, 14, 0);
                    DrawField (ref sharedColorSBHue, sharedIncrementColor, sharedColorLimits, "Hue", uiColorSliderColorsSB, 15, 0);
                    DrawField (ref sharedColorSBSaturation, sharedIncrementColor, sharedColorLimits, "Saturation", uiColorSliderColorsSB, 16, 0);
                    DrawField (ref sharedColorSBBrightness, sharedIncrementColor, sharedColorLimits, "Brightness", uiColorSliderColorsSB, 17, 0);
                }

                DrawFieldGroupHeader (ref sharedFieldGroupColorETStatic, "Surface (trailing edge)");
                if (sharedFieldGroupColorETStatic)
                {
                    DrawField (ref sharedMaterialET, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsET, 13, 1);
                    DrawField (ref sharedColorETOpacity, sharedIncrementColor, sharedColorLimits, "Opacity", uiColorSliderColorsET, 14, 0);
                    DrawField (ref sharedColorETHue, sharedIncrementColor, sharedColorLimits, "Hue", uiColorSliderColorsET, 15, 0);
                    DrawField (ref sharedColorETSaturation, sharedIncrementColor, sharedColorLimits, "Saturation", uiColorSliderColorsET, 16, 0);
                    DrawField (ref sharedColorETBrightness, sharedIncrementColor, sharedColorLimits, "Brightness", uiColorSliderColorsET, 17, 0);
                }

                if (!isCtrlSrf)
                {
                    DrawFieldGroupHeader (ref sharedFieldGroupColorELStatic, "Surface (leading edge)");
                    if (sharedFieldGroupColorELStatic)
                    {
                        DrawField (ref sharedMaterialEL, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsEL, 13, 1);
                        DrawField (ref sharedColorELOpacity, sharedIncrementColor, sharedColorLimits, "Opacity", uiColorSliderColorsEL, 14, 0);
                        DrawField (ref sharedColorELHue, sharedIncrementColor, sharedColorLimits, "Hue", uiColorSliderColorsEL, 15, 0);
                        DrawField (ref sharedColorELSaturation, sharedIncrementColor, sharedColorLimits, "Saturation", uiColorSliderColorsEL, 16, 0);
                        DrawField (ref sharedColorELBrightness, sharedIncrementColor, sharedColorLimits, "Brightness", uiColorSliderColorsEL, 17, 0);
                    }
                }

                GUILayout.Label ("_________________________\n\nPress J to exit edit mode\nOptions below allow you to change default values", WingProceduralManager.uiStyleLabelHint);
                if (!isCtrlSrf && !isWingAsCtrlSrf && !assemblyRFUsed)
                {
                    if (GUILayout.Button (GetTankSetupName () + " | Next tank setup", WingProceduralManager.uiStyleButton)) NextConfiguration ();
                }

                GUILayout.BeginHorizontal ();
                if (GUILayout.Button ("Save as default", WingProceduralManager.uiStyleButton)) ReplaceDefaults ();
                if (GUILayout.Button ("Restore default", WingProceduralManager.uiStyleButton)) RestoreDefaults ();
                GUILayout.EndHorizontal ();
                if (inheritancePossibleOnShape || inheritancePossibleOnMaterials)
                {
                    GUILayout.Label ("_________________________\n\nOptions options allow you to match the part properties to it's parent", WingProceduralManager.uiStyleLabelHint);
                    GUILayout.BeginHorizontal ();
                    if (inheritancePossibleOnShape) 
                    { 
                        if (GUILayout.Button ("Shape", WingProceduralManager.uiStyleButton)) InheritParentValues (0);
                        if (GUILayout.Button ("Width", WingProceduralManager.uiStyleButton)) InheritParentValues (1);
                        if (GUILayout.Button ("Edges", WingProceduralManager.uiStyleButton)) InheritParentValues (2); 
                    }
                    if (inheritancePossibleOnMaterials) { if (GUILayout.Button ("Color", WingProceduralManager.uiStyleButton)) InheritParentValues (3); }
                    GUILayout.EndHorizontal ();
                }
                // if (GUILayout.Button ("Dump state", WingProceduralManager.uiStyleButton)) DumpState ();
            }
            else
            {
                if (uiEditModeTimeout) GUILayout.Label ("Exiting edit mode...\n", WingProceduralManager.uiStyleLabelMedium);
                else
                {
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ("Press J while pointing at a\nprocedural part to edit it", WingProceduralManager.uiStyleLabelHint);
                    if (GUILayout.Button ("Close", WingProceduralManager.uiStyleButton, GUILayout.MaxWidth (50f)))
                    {
                        uiWindowActive = false;
                        uiAdjustWindow = true;
                        EditorLogic.fetch.Unlock ("WingProceduralWindow");
                    }
                    GUILayout.EndHorizontal ();
                }
            }
            GUI.DragWindow ();
        }

        private void DrawField (ref float field, float increment, Vector2 limits, string name, Vector4 hsbColor, int fieldID, int fieldType)
        {
            bool changed = false;
            float value = UIUtility.FieldSlider (field, increment, limits, name, out changed, WingProceduralManager.uiStyleSlider, WingProceduralManager.uiStyleSliderThumb, WingProceduralManager.uiStyleLabelHint, ColorHSBToRGB (hsbColor), fieldType);
            if (changed)
            {
                field = value;
                uiLastFieldName = name;
                uiLastFieldTooltip = UpdateTooltipText (fieldID);
            }
        }

        private void DrawFieldGroupHeader (ref bool fieldGroupBoolStatic, string header)
        {
            GUILayout.BeginHorizontal ();
            if (GUILayout.Button (header, WingProceduralManager.uiStyleLabelHint))
            {
                fieldGroupBoolStatic = !fieldGroupBoolStatic;
                if (WingProceduralDebugValues.logPropertyWindow) DebugLogWithID ("DrawFieldGroupHeader", "Header of " + header + " pressed | Group state: " + fieldGroupBoolStatic);
                uiAdjustWindow = true;
            }
            if (fieldGroupBoolStatic) GUILayout.Label ("|", WingProceduralManager.uiStyleLabelHint, GUILayout.MaxWidth (15f));
            else GUILayout.Label ("+", WingProceduralManager.uiStyleLabelHint, GUILayout.MaxWidth (15f));
            GUILayout.EndHorizontal ();
        }

        private static string uiLastFieldName = "";
        private static string uiLastFieldTooltip = "Additional info on edited \nproperties is displayed here";

        private string UpdateTooltipText (int fieldID)
        {
            // Base descriptions
            if (fieldID == 0) // sharedBaseLength))
            {
                if (!isCtrlSrf) return "Lateral measurement of the wing, \nalso referred to as semispan";
                else            return "Lateral measurement of the control \nsurface at it's root";
            }
            else if (fieldID == 1) // sharedBaseWidthRoot))
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the wing \nat the root cross section";
                else            return "Longitudinal measurement of \nthe root chord";
            }
            else if (fieldID == 2) // sharedBaseWidthTip))
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the wing \nat the tip cross section";
                else            return "Longitudinal measurement of \nthe tip chord";
            }
            else if (fieldID == 3) // sharedBaseOffsetRoot))
            {
                if (!isCtrlSrf) return "This property shouldn't be accessible \non a wing";
                else            return "Offset of the trailing edge \nroot corner on the lateral axis";
            }
            else if (fieldID == 4) // sharedBaseOffsetTip))
            {
                if (!isCtrlSrf) return "Distance between midpoints of the cross \nsections on the longitudinal axis";
                else            return "Offset of the trailing edge \ntip corner on the lateral axis";
            }
            else if (fieldID == 5) // sharedBaseThicknessRoot))
            {
                if (!isCtrlSrf) return "Thickness at the root cross section \nUsually kept proportional to edge width";
                else            return "Thickness at the root cross section \nUsually kept proportional to edge width";
            }
            else if (fieldID == 6) // sharedBaseThicknessTip))
            {
                if (!isCtrlSrf) return "Thickness at the tip cross section \nUsually kept proportional to edge width";
                else            return "Thickness at the tip cross section \nUsually kept proportional to edge width";
            }

            // Edge descriptions
            else if (fieldID == 7) // sharedEdgeTypeTrailing))
            {
                if (!isCtrlSrf) return "Shape of the trailing edge cross \nsection (round/biconvex/sharp)";
                else            return "Shape of the trailing edge cross \nsection (round/biconvex/sharp)";
            }
            else if (fieldID == 8) // sharedEdgeWidthTrailingRoot))
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the trailing \nedge cross section at wing root";
                else            return "Longitudinal measurement of the trailing \nedge cross section at with root";
            }
            else if (fieldID == 9) // sharedEdgeWidthTrailingTip))
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the trailing \nedge cross section at wing tip";
                else            return "Longitudinal measurement of the trailing \nedge cross section at with tip";
            }
            else if (fieldID == 10) // sharedEdgeTypeLeading))
            {
                if (!isCtrlSrf) return "Shape of the leading edge cross \nsection (round/biconvex/sharp)";
                else            return "Shape of the leading edge cross \nsection (round/biconvex/sharp)";
            }
            else if (fieldID == 11) // sharedEdgeWidthLeadingRoot))
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the leading \nedge cross section at wing root";
                else            return "Longitudinal measurement of the leading \nedge cross section at wing root";
            }
            else if (fieldID == 12) // sharedEdgeWidthLeadingTip))
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the leading \nedge cross section at with tip";
                else            return "Longitudinal measurement of the leading \nedge cross section at with tip";
            }

            // Surface descriptions
            else if (fieldID == 13)
            {
                if (!isCtrlSrf) return "Surface material (uniform fill, plating, \nLRSI/HRSI tiles and so on)";
                else            return "Surface material (uniform fill, plating, \nLRSI/HRSI tiles and so on)";
            }
            else if (fieldID == 14)
            {
                if (!isCtrlSrf) return "Fairly self-explanatory, controls the paint \nopacity: no paint at 0, full coverage at 1";
                else            return "Fairly self-explanatory, controls the paint \nopacity: no paint at 0, full coverage at 1";
            }
            else if (fieldID == 15)
            {
                if (!isCtrlSrf) return "Controls the paint hue (HSB axis): \nvalues from zero to one make full circle";
                else            return "Controls the paint hue (HSB axis): \nvalues from zero to one make full circle";
            }
            else if (fieldID == 16)
            {
                if (!isCtrlSrf) return "Controls the paint saturation (HSB axis): \ncolorless at 0, full color at 1";
                else            return "Controls the paint saturation (HSB axis): \ncolorless at 0, full color at 1";
            }
            else if (fieldID == 17)
            {
                if (!isCtrlSrf) return "Controls the paint brightness (HSB axis): black at 0, white at 1, primary at 0.5";
                else            return "Controls the paint brightness (HSB axis): black at 0, white at 1, primary at 0.5";
            }

            // This should not really happen
            else return "Unknown field\n";
        }

        private void OnMouseOver ()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                // if (logPropertyWindow) DebugLogWithID ("OnMouseOver", "Parent: " + this.part.parent + " | Attached: " + isAttached + " | Timeout: " + uiEditModeTimeout + " | ID (local): " + uiInstanceIDLocal + " | ID (static): " + uiInstanceIDTarget);
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
                    if (Input.GetKeyDown (uiKeyCodeEdit))
                    {
                        uiInstanceIDTarget = part.GetInstanceID ();
                        uiEditMode = true;
                        uiEditModeTimeout = true;
                        uiAdjustWindow = true;
                        uiWindowActive = true;
                        InheritanceStatusUpdate ();
                    }
                }
            }
        }

        private void UpdateUI ()
        {
            if (stockButton == null) OnStockButtonSetup ();
            if (uiEditModeTimeout && uiInstanceIDTarget == 0)
            {
                if (WingProceduralDebugValues.logPropertyWindow) DebugLogWithID ("UpdateUI", "Window timeout was left active on scene reload, resetting the window state");
                StopWindowTimeout ();
            }
            if (uiInstanceIDLocal != uiInstanceIDTarget) return;
            if (uiEditModeTimeout)
            {
                // if (logPropertyWindow) DebugLogWithID ("UpdateUI", "Timeout triggered, current state: " + uiEditModeTimeout + " | Time: " + uiEditModeTimer);
                uiEditModeTimer += Time.deltaTime;
                if (uiEditModeTimer > uiEditModeTimeoutDuration)
                {
                    StopWindowTimeout ();
                }
            }
            else
            {
                if (uiEditMode)
                {
                    if (Input.GetKeyDown (uiKeyCodeEdit)) ExitEditMode ();
                    else
                    {
                        EditorLogic EdLogInstance = EditorLogic.fetch;
                        bool cursorInGUI = false;
                        cursorInGUI = uiRect.Contains (UIUtility.GetMousePos ());
                        if (!cursorInGUI)
                        {
                            if (Input.GetKeyDown (KeyCode.Mouse0)) ExitEditMode ();
                        }
                    }
                }
            }
        }

        private void StopWindowTimeout ()
        {
            uiAdjustWindow = true;
            uiEditModeTimeout = false;
            uiEditModeTimer = 0.0f;
        }

        private void ExitEditMode ()
        {
            uiEditMode = false;
            uiEditModeTimeout = true;
            uiAdjustWindow = true;
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




        // Resources
        // Original code by Snjo
        // Modified to remove config support and string parsing and to add support for arbitrary volumes

        public class WPResource
        {
            public string name;
            public int ID;
            public float ratio;
            public double currentSupply = 0f;
            public float amount = 0f;
            public float maxAmount = 0f;

            public WPResource(string _name, float _ratio)
            {
                name = _name;
                ID = _name.GetHashCode();
                ratio = _ratio;
            }

            public WPResource(string _name)
            {
                name = _name;
                ID = _name.GetHashCode();
                ratio = 1f;
            }
        }

        public class WPInnerTank
        {
            public List<WPResource> resources = new List<WPResource> ();
        }

        private List<WPInnerTank> fuelTankList = new List<WPInnerTank> ();

        // Reference values for 3.25m3 tank and 1.0m3 tank
        // LF    | LFO
        // 420.0 | 189.0, 231.0
        // 134.4 | 60.48, 73.92

        public string[][] fuelResourceNames = new string[][] { new string[] { "Structural" }, new string[] { "LiquidFuel" }, new string[] { "LiquidFuel", "Oxidizer"}, new string[] { "MonoPropellant" } };
        public float[][] fuelPerCubicMeter = new float[][] { new float[] { 0.0f }, new float[] { 134.4f }, new float[] { 60.48f, 73.92f }, new float[] { 134.4f } };
        public float[] fuelCostPerUnit = new float[] { 0.0f, 0.6f, 0.875f, 0.750f };

        public bool fuelDisplayCurrentTankCost = false;
        public bool fuelGUI = true;
        public bool fuelShowInfo = false;

        [KSPField (isPersistant = true)] public Vector4 fuelCurrentAmount = Vector4.zero;
        [KSPField (isPersistant = true)] public float fuelMutliplier = 1.0f;
        [KSPField (isPersistant = true)] public int fuelSelectedTankSetup = 0;
        [KSPField (isPersistant = true)] public bool fuelFlightSceneStarted = false;
        [KSPField (isPersistant = true)] private bool fuelBrandNewPart = true;

        [KSPField (guiActive = false, guiActiveEditor = false, guiName = "Added cost")] public float fuelAddedCost = 0f;
        [KSPField (guiActive = false, guiActiveEditor = false, guiName = "Dry mass")] public float fuelDryMassInfo = 0f;

        private bool fuelInitialized = false;
        private float fuelVolumeOld = 0f;

        private string GetTankSetupName ()
        {
            if (assemblyRFUsed) return "";
            else
            {
                string units = "";
                if (fuelSelectedTankSetup == 1) units += "LF (";
                else if (fuelSelectedTankSetup == 2) units += "LFO (";
                else if (fuelSelectedTankSetup == 3) units += "RCS (";
                else units += "STR (";
                if (fuelTankList.Count > 0)
                {
                    for (int i = 0; i < fuelTankList[fuelSelectedTankSetup].resources.Count; ++i)
                    {
                        units += ((int) fuelTankList[fuelSelectedTankSetup].resources[i].maxAmount).ToString ();
                        if (i == fuelTankList[fuelSelectedTankSetup].resources.Count - 1) units += ")";
                        else units += "/";
                    }
                }
                return units;
            }
        }

        private void FuelUpdateAmountsFromVolume (float volume, bool reassignAfter)
        {
            if (isCtrlSrf || isWingAsCtrlSrf) return;
            if (assemblyRFUsed)
            {
                if (WingProceduralDebugValues.logFuel) DebugLogWithID ("FuelUpdateAmountsFromVolume", "Started for RF");
                if (part.Modules.Contains ("ModuleFuelTanks"))
                {
                    PartModule module = part.Modules["ModuleFuelTanks"];
                    Type type = module.GetType ();

                    double volumeRF = (double) volume * 1000; // liters instead of cubic meters
                    type.GetField ("volume").SetValue (module, volumeRF); 
                    type.GetMethod ("ChangeVolume").Invoke (module, new object[] { volumeRF } );
                }
                else if (WingProceduralDebugValues.logFuel) DebugLogWithID ("FuelUpdateAmountsFromVolume", "Module not found");
            }
            else
            {
                if (WingProceduralDebugValues.logFuel) DebugLogWithID ("FuelUpdateAmountsFromVolume", "Started for stock fuel");
                for (int i = 0; i < fuelTankList.Count; ++i)
                {
                    for (int r = 0; r < fuelTankList[i].resources.Count; ++r)
                    {
                        float newAmount = fuelPerCubicMeter[i][r] * volume * 0.7f; // since not all volume is used
                        fuelTankList[i].resources[r].maxAmount = newAmount;
                        fuelTankList[i].resources[r].amount = newAmount;
                    }
                }
                fuelVolumeOld = volume;
                if (reassignAfter) FuelAssignResourcesToPart (true);
            }
        }

        private void FuelOnStart ()
        {
            if (WingProceduralDebugValues.logFuel) DebugLogWithID ("FuelOnStart", "Started");
            if (isCtrlSrf || isWingAsCtrlSrf || assemblyRFUsed) return;
            else
            {
                if (Events.Contains ("NextConfiguration"))
                {
                    if (WingProceduralDebugValues.logFuel) DebugLogWithID ("FuelOnStart", "Event found and enabled");
                    Events["NextConfiguration"].active = true;
                    Events["NextConfiguration"].guiActiveEditor = true;
                }
                else if (WingProceduralDebugValues.logFuel) DebugLogWithID ("FuelOnStart", "Event not found");
                FuelInitializeData ();
                FuelAssignResourcesToPart (false);
                fuelBrandNewPart = false;
                FuelUpdateAmountsFromVolume (aeroStatVolume, true);
            }
        }

        private void FuelInitializeData ()
        {
            if (WingProceduralDebugValues.logFuel) DebugLogWithID ("FuelInitializeData", "Started");
            if (isCtrlSrf || isWingAsCtrlSrf) return;
            if (!fuelInitialized)
            {
                FuelSetupTankList (false);
                if (HighLogic.LoadedSceneIsFlight) fuelFlightSceneStarted = true;

                if (fuelGUI) Events["NextConfiguration"].guiActiveEditor = true;
                else Events["NextConfiguration"].guiActiveEditor = false;
                fuelInitialized = true;
            }
        }

        public void FuelSelectTankSetup (int i, bool calledByPlayer)
        {
            if (WingProceduralDebugValues.logFuel) DebugLogWithID ("FuelSelectTankSetup", "Started");
            if (isCtrlSrf || isWingAsCtrlSrf) return;
            FuelInitializeData ();
            fuelSelectedTankSetup = i;
            FuelAssignResourcesToPart (calledByPlayer);
        }

        private void FuelAssignResourcesToPart (bool calledByPlayer)
        {
            if (WingProceduralDebugValues.logFuel) DebugLogWithID ("FuelAssignResourcesToPart", "Started");
            if (isCtrlSrf || isWingAsCtrlSrf || assemblyRFUsed) return;
            FuelSetupTankInPart (part, calledByPlayer);
            if (HighLogic.LoadedSceneIsEditor)
            {
                for (int s = 0; s < part.symmetryCounterparts.Count; s++)
                {
                    FuelSetupTankInPart (part.symmetryCounterparts[s], calledByPlayer);
                    WingProcedural wing = part.symmetryCounterparts[s].GetComponent<WingProcedural> ();
                    if (wing != null)
                    {
                        wing.fuelSelectedTankSetup = fuelSelectedTankSetup;
                    }
                }
            }
            if (fuelSelectedTankSetup != 0) updateRequiredOnWindow = true;
        }

        private void FuelSetupTankInPart (Part currentPart, bool calledByPlayer)
        {
            if (WingProceduralDebugValues.logFuel) DebugLogWithID ("FuelSetupTankInPart", "Started");
            if (isCtrlSrf || isWingAsCtrlSrf || assemblyRFUsed) return;
            currentPart.Resources.list.Clear ();
            PartResource[] partResources = currentPart.GetComponents<PartResource> ();
            for (int i = 0; i < partResources.Length; i++)
            {
                DestroyImmediate (partResources[i]);
            }
            if (fuelVolumeOld != aeroStatVolume) FuelUpdateAmountsFromVolume (aeroStatVolume, false);
            for (int tankIndex = 0; tankIndex < fuelTankList.Count; tankIndex++)
            {
                if (fuelSelectedTankSetup == tankIndex)
                {
                    for (int resourceIndex = 0; resourceIndex < fuelTankList[tankIndex].resources.Count; resourceIndex++)
                    {
                        if (fuelTankList[tankIndex].resources[resourceIndex].name != "Structural")
                        {
                            ConfigNode newResourceNode = new ConfigNode ("RESOURCE");
                            newResourceNode.AddValue ("name", fuelTankList[tankIndex].resources[resourceIndex].name);
                            if (calledByPlayer || fuelBrandNewPart)
                            {
                                newResourceNode.AddValue ("amount", fuelTankList[tankIndex].resources[resourceIndex].maxAmount);
                                FuelSetResource (resourceIndex, fuelTankList[tankIndex].resources[resourceIndex].amount);
                            }
                            else
                            {
                                newResourceNode.AddValue ("amount", FuelGetResource (resourceIndex));
                            }
                            newResourceNode.AddValue ("maxAmount", fuelTankList[tankIndex].resources[resourceIndex].maxAmount);
                            currentPart.AddResource (newResourceNode);
                        }
                    }
                }
            }
            currentPart.Resources.UpdateList ();
            fuelAddedCost = FuelGetAddedCost ();
        }

        private float FuelGetAddedCost ()
        {
            float result = 0f;
            if (fuelSelectedTankSetup < fuelCostPerUnit.Length && fuelSelectedTankSetup < fuelTankList.Count)
            {
                for (int i = 0; i < fuelTankList[fuelSelectedTankSetup].resources.Count; ++i)
                {
                    result += fuelCostPerUnit[fuelSelectedTankSetup] * fuelTankList[fuelSelectedTankSetup].resources[i].maxAmount;
                }
            }
            return result;
        }

        private void FuelOnUpdate ()
        {
            if (fuelSelectedTankSetup < fuelTankList.Count) 
            {
                if (fuelTankList[fuelSelectedTankSetup] != null)
                {
                    for (int i = 0; i < fuelTankList[fuelSelectedTankSetup].resources.Count; i++)
                    {
                        if (fuelTankList[fuelSelectedTankSetup].resources[i].name == "Structural")
                        {

                        }
                        else
                        {
                            FuelSetResource (i, (float) part.Resources[fuelTankList[fuelSelectedTankSetup].resources[i].name].amount);
                        }
                    }
                }
            }
        }

        private float FuelGetResource (int number)
        {
            switch (number)
            {
                case 0:
                    return fuelCurrentAmount.x;
                case 1:
                    return fuelCurrentAmount.y;
                case 2:
                    return fuelCurrentAmount.z;
                case 3:
                    return fuelCurrentAmount.w;
                default:
                    return 0f;
            }
        }

        private void FuelSetResource (int number, float amount)
        {
            switch (number)
            {
                case 0:
                    fuelCurrentAmount.x = amount;
                    break;
                case 1:
                    fuelCurrentAmount.y = amount;
                    break;
                case 2:
                    fuelCurrentAmount.z = amount;
                    break;
                case 3:
                    fuelCurrentAmount.w = amount;
                    break;
            }
        }

        private void FuelSetupTankList (bool calledByPlayer)
        {
            if (WingProceduralDebugValues.logFuel) DebugLogWithID ("FuelSetupTankList", "Started");
            if (isCtrlSrf || isWingAsCtrlSrf || assemblyRFUsed) return;
            fuelTankList.Clear ();

            // First find the amounts each tank type is filled with

            List<List<float>> resourceList = new List<List<float>> ();
            for (int tankIndex = 0; tankIndex < fuelPerCubicMeter.Length; tankIndex++)
            {
                resourceList.Add (new List<float> ());
                for (int amountIndex = 0; amountIndex < fuelPerCubicMeter[tankIndex].Length; amountIndex++)
                {
                    resourceList[tankIndex].Add (fuelPerCubicMeter[tankIndex][amountIndex]);
                }
            }

            // Then find the kinds of resources each tank holds, and fill them with the amounts found previously, or the amount hey held last (values kept in save persistence/craft)

            for (int tankIndex = 0; tankIndex < fuelResourceNames.Length; tankIndex++)
            {
                WPInnerTank newTank = new WPInnerTank ();
                fuelTankList.Add (newTank);
                for (int nameIndex = 0; nameIndex < fuelResourceNames[tankIndex].Length; nameIndex++)
                {
                    WPResource newResource = new WPResource (fuelResourceNames[tankIndex][nameIndex].Trim (' '));
                    if (resourceList[tankIndex] != null)
                    {
                        if (nameIndex < resourceList[tankIndex].Count)
                        {
                            newResource.maxAmount = resourceList[tankIndex][nameIndex];
                            if (calledByPlayer)
                            {
                                newResource.amount = resourceList[tankIndex][nameIndex]; 
                            }
                            else
                            {
                                newResource.amount = FuelGetResource (nameIndex);
                            }
                        }
                    }
                    newTank.resources.Add (newResource);
                }
            }
        }




        // Interfaces

        public float GetModuleCost ()
        {
            if (assemblyRFUsed) return aeroUICost;
            else return FuelGetAddedCost () + aeroUICost;
        }

        public float GetModuleCost (float modifier)
        {
            if (assemblyRFUsed) return aeroUICost;
            else return FuelGetAddedCost () + aeroUICost;
        }

        private Renderer meshRendererForBounds = null;

        public Vector3 GetModuleSize (Vector3 defaultSize)
        {
            Vector3 size = Vector3.zero;
            if (!isCtrlSrf)
            {
                if (meshFilterWingSection != null)
                {
                    if (meshRendererForBounds == null) meshRendererForBounds = meshFilterWingSection.gameObject.GetComponent<Renderer> ();
                    if (meshRendererForBounds != null)
                    {
                        Vector3 extents = meshRendererForBounds.bounds.extents;
                        size = new Vector3 (extents.y, extents.x, extents.z);
                    }
                }
                if (size == Vector3.zero) size = new Vector3 (Mathf.Max (sharedBaseThicknessRoot, sharedBaseThicknessTip), sharedBaseLength, Mathf.Max (sharedBaseWidthRoot, sharedBaseWidthTip));
            }
            else
            {
                if (meshFilterCtrlFrame != null)
                {
                    if (meshRendererForBounds == null) meshRendererForBounds = meshFilterCtrlFrame.gameObject.GetComponent<Renderer> ();
                    if (meshRendererForBounds != null)
                    {
                        Vector3 extents = meshRendererForBounds.bounds.extents;
                        size = new Vector3 (extents.x, extents.z, extents.y);
                    }
                }
                if (size == Vector3.zero) size = new Vector3 (Mathf.Max (sharedBaseThicknessRoot, sharedBaseThicknessTip), Mathf.Max (sharedBaseWidthRoot, sharedBaseWidthTip), sharedBaseLength);
            }
            return size;
        }




        // Stock toolbar integration

        public static ApplicationLauncherButton stockButton = null;

        private void OnStockButtonSetup ()
        {
            stockButton = ApplicationLauncher.Instance.AddModApplication (OnStockButtonClick, OnStockButtonClick, OnStockButtonVoid, OnStockButtonVoid, OnStockButtonVoid, OnStockButtonVoid, ApplicationLauncher.AppScenes.SPH, (Texture) GameDatabase.Instance.GetTexture ("B9_Aerospace/Plugins/icon_stock", false));
        }

        public void OnStockButtonClick ()
        {
            uiWindowActive = !uiWindowActive;
        }

        private void OnStockButtonVoid ()
        {

        }

        public void OnDestroy ()
        {
            bool stockButtonCanBeRemoved = true;
            WingProcedural[] components = GameObject.FindObjectsOfType<WingProcedural> ();
            if (WingProceduralDebugValues.logEvents) DebugLogWithID ("OnDestroy", "Invoked, with " + components.Length + " remaining components in the scene");
            for (int i = 0; i < components.Length; ++i)
            {
                if (components[i] != null) stockButtonCanBeRemoved = false;
            }
            if (stockButtonCanBeRemoved)
            {
                uiInstanceIDTarget = 0;
                ApplicationLauncher.Instance.RemoveModApplication (stockButton);
            }
        }




        // Dump state

        public void DumpState ()
        {
            // string[] fieldNames = Array.ConvertAll(fields, field => field.Name);
            string report = "State report on part " + this.GetInstanceID () + ":\n\n";
            Type type = this.GetType ();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            List<string> fieldNames = fields.Select(field => field.Name).ToList();
            List<object> fieldValues = fields.Select(field => field.GetValue(this)).ToList();
            if (fieldNames.Count == fieldValues.Count && fieldNames.Count == fields.Length)
            {
                for (int i = 0; i < fields.Length; ++i)
                {
                    if (!string.IsNullOrEmpty (fieldNames[i]))
                    {
                        if (fieldValues[i] != null) report += fieldNames[i] + ": " + fieldValues[i].ToString () + "\n";
                        else report += fieldNames[i] + ": null\n";
                    }
                    else report += "Field " + i.ToString () + " name not available\n";
                }
            }
            else
            {
                report += "Field info size mismatch, list can't be printed";
            }
            Debug.Log (report);
        }
    }
}
