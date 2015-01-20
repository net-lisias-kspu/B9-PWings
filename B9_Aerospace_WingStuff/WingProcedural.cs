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
        private static Vector4 sharedBaseWidthLimits = new Vector4 (0.25f, 16f, 0.25f, 1.5f);
        private static Vector4 sharedBaseOffsetLimits = new Vector4 (-8f, 8f, -2f, 2f);
        private static Vector4 sharedEdgeTypeLimits = new Vector4 (1f, 4f, 1f, 3f);
        private static Vector2 sharedEdgeWidthLimits = new Vector2 (0f, 1f);
        private static Vector2 sharedMaterialLimits = new Vector2 (0f, 4f);
        private static Vector2 sharedColorLimits = new Vector2 (0f, 1f);

        private static float sharedIncrementColor = 0.01f;
        private static float sharedIncrementMain = 0.125f;
        private static float sharedIncrementSmall = 0.04f;
        private static float sharedIncrementInt = 1f;




        // Shared properties / Base

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Base"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupBase = false;
        public bool sharedFieldGroupBaseCached = false;
        public static bool sharedFieldGroupBaseStatic = false;
        private static string[] sharedFieldGroupBaseArray = new string[] { "sharedBaseLength", "sharedBaseWidthRoot", "sharedBaseWidthTip", "sharedBaseThicknessRoot", "sharedBaseThicknessTip", "sharedBaseOffsetTip" };
        private static string[] sharedFieldGroupBaseArrayCtrl = new string[] { "sharedBaseOffsetRoot" };

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Length", guiFormat = "S4"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float sharedBaseLength = 4f;
        public float sharedBaseLengthCached = 4f;
        public static Vector4 sharedBaseLengthDefaults = new Vector4 (4f, 1f, 4f, 1f);

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Width (root)", guiFormat = "S4"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float sharedBaseWidthRoot = 4f;
        public float sharedBaseWidthRootCached = 4f;
        public static Vector4 sharedBaseWidthRootDefaults = new Vector4 (4f, 0.5f, 4f, 0.5f);

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Width (tip)", guiFormat = "S4"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float sharedBaseWidthTip = 4f;
        public float sharedBaseWidthTipCached = 4f;
        public static Vector4 sharedBaseWidthTipDefaults = new Vector4 (4f, 0.5f, 4f, 0.5f);

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Offset (root)", guiFormat = "S4"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = -2.5f, maxValue = 2.5f, incrementSlide = 0.125f)]
        public float sharedBaseOffsetRoot = 0f;
        public float sharedBaseOffsetRootCached = 0f;
        public static Vector4 sharedBaseOffsetRootDefaults = new Vector4 (0f, 0f, 0f, 0f);

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Offset (tip)", guiFormat = "S4"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = -2.5f, maxValue = 2.5f, incrementSlide = 0.125f)]
        public float sharedBaseOffsetTip = 0f;
        public float sharedBaseOffsetTipCached = 0f;
        public static Vector4 sharedBaseOffsetTipDefaults = new Vector4 (0f, 0f, 0f, 0f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Thickness (root)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.08f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedBaseThicknessRoot = 0.24f;
        public float sharedBaseThicknessRootCached = 0.24f;
        public static Vector4 sharedBaseThicknessRootDefaults = new Vector4 (0.24f, 0.24f, 0.24f, 0.24f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Thickness (tip)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.08f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedBaseThicknessTip = 0.24f;
        public float sharedBaseThicknessTipCached = 0.24f;
        public static Vector4 sharedBaseThicknessTipDefaults = new Vector4 (0.24f, 0.24f, 0.24f, 0.24f);




        // Shared properties / Edge / Leading

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Lead. edge"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupEdgeLeading = false;
        public bool sharedFieldGroupEdgeLeadingCached = false;
        public static bool sharedFieldGroupEdgeLeadingStatic = false;
        private static string[] sharedFieldGroupEdgeLeadingArray = new string[] { "sharedEdgeTypeLeading", "sharedEdgeWidthLeadingRoot", "sharedEdgeWidthLeadingTip" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Shape", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 1f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedEdgeTypeLeading = 2f;
        public float sharedEdgeTypeLeadingCached = 2f;
        public static Vector4 sharedEdgeTypeLeadingDefaults = new Vector4 (2f, 1f, 2f, 1f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (root)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedEdgeWidthLeadingRoot = 0.24f;
        public float sharedEdgeWidthLeadingRootCached = 0.24f;
        public static Vector4 sharedEdgeWidthLeadingRootDefaults = new Vector4 (0.24f, 0.24f, 0.24f, 0.24f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (tip)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedEdgeWidthLeadingTip = 0.24f;
        public float sharedEdgeWidthLeadingTipCached = 0.24f;
        public static Vector4 sharedEdgeWidthLeadingTipDefaults = new Vector4 (0.24f, 0.24f, 0.24f, 0.24f);




        // Shared properties / Edge / Trailing

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Trail. edge"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupEdgeTrailing = false;
        public bool sharedFieldGroupEdgeTrailingCached = false;
        public static bool sharedFieldGroupEdgeTrailingStatic = false;
        private static string[] sharedFieldGroupEdgeTrailingArray = new string[] { "sharedEdgeTypeTrailing", "sharedEdgeWidthTrailingRoot", "sharedEdgeWidthTrailingTip" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Shape", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 1f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedEdgeTypeTrailing = 3f;
        public float sharedEdgeTypeTrailingCached = 3f;
        public static Vector4 sharedEdgeTypeTrailingDefaults = new Vector4 (3f, 2f, 3f, 2f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (root)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedEdgeWidthTrailingRoot = 0.48f;
        public float sharedEdgeWidthTrailingRootCached = 0.48f;
        public static Vector4 sharedEdgeWidthTrailingRootDefaults = new Vector4 (0.48f, 0.48f, 0.48f, 0.48f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Width (tip)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.04f)]
        public float sharedEdgeWidthTrailingTip = 0.48f;
        public float sharedEdgeWidthTrailingTipCached = 0.48f;
        public static Vector4 sharedEdgeWidthTrailingTipDefaults = new Vector4 (0.48f, 0.48f, 0.48f, 0.48f);




        // Shared properties / Surface / Top

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Material A"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupColorST = false;
        public bool sharedFieldGroupColorSTCached = false;
        public bool sharedFieldGroupColorSTStatic = false;
        private static string[] sharedFieldGroupColorSTArray = new string[] { "sharedMaterialST", "sharedColorSTOpacity", "sharedColorSTHue", "sharedColorSTSaturation", "sharedColorSTBrightness" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Material", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedMaterialST = 1f;
        public float sharedMaterialSTCached = 1f;
        public static Vector4 sharedMaterialSTDefaults = new Vector4 (1f, 1f, 1f, 1f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Opacity", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSTOpacity = 0f;
        public float sharedColorSTOpacityCached = 0f;
        public static Vector4 sharedColorSTOpacityDefaults = new Vector4 (0f, 0f, 0f, 0f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (H)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSTHue = 0.10f;
        public float sharedColorSTHueCached = 0.10f;
        public static Vector4 sharedColorSTHueDefaults = new Vector4 (0.1f, 0.1f, 0.1f, 0.1f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (S)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSTSaturation = 0.75f;
        public float sharedColorSTSaturationCached = 0.75f;
        public static Vector4 sharedColorSTSaturationDefaults = new Vector4 (0.75f, 0.75f, 0.75f, 0.75f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (B)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSTBrightness = 0.6f;
        public float sharedColorSTBrightnessCached = 0.6f;
        public static Vector4 sharedColorSTBrightnessDefaults = new Vector4 (0.6f, 0.6f, 0.6f, 0.6f);




        // Shared properties / Surface / bottom

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Material B"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupColorSB = false;
        public bool sharedFieldGroupColorSBCached = false;
        public bool sharedFieldGroupColorSBStatic = false;
        private static string[] sharedFieldGroupColorSBArray = new string[] { "sharedMaterialSB", "sharedColorSBOpacity", "sharedColorSBHue", "sharedColorSBSaturation", "sharedColorSBBrightness" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Material", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedMaterialSB = 4f;
        public float sharedMaterialSBCached = 4f;
        public static Vector4 sharedMaterialSBDefaults = new Vector4 (4f, 4f, 4f, 4f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Opacity", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSBOpacity = 0f;
        public float sharedColorSBOpacityCached = 0f;
        public static Vector4 sharedColorSBOpacityDefaults = new Vector4 (0f, 0f, 0f, 0f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (H)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSBHue = 0.10f;
        public float sharedColorSBHueCached = 0.10f;
        public static Vector4 sharedColorSBHueDefaults = new Vector4 (0.1f, 0.1f, 0.1f, 0.1f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (S)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSBSaturation = 0.75f;
        public float sharedColorSBSaturationCached = 0.75f;
        public static Vector4 sharedColorSBSaturationDefaults = new Vector4 (0.75f, 0.75f, 0.75f, 0.75f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (B)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorSBBrightness = 0.6f;
        public float sharedColorSBBrightnessCached = 0.6f;
        public static Vector4 sharedColorSBBrightnessDefaults = new Vector4 (0.6f, 0.6f, 0.6f, 0.6f);




        // Shared properties / Surface / trailing edge

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Material T"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupColorET = false;
        public bool sharedFieldGroupColorETCached = false;
        public bool sharedFieldGroupColorETStatic = false;
        private static string[] sharedFieldGroupColorETArray = new string[] { "sharedMaterialET", "sharedColorETOpacity", "sharedColorETHue", "sharedColorETSaturation", "sharedColorETBrightness" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Material", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedMaterialET = 4f;
        public float sharedMaterialETCached = 4f;
        public static Vector4 sharedMaterialETDefaults = new Vector4 (4f, 4f, 4f, 4f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Opacity", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorETOpacity = 0f;
        public float sharedColorETOpacityCached = 0f;
        public static Vector4 sharedColorETOpacityDefaults = new Vector4 (0f, 0f, 0f, 0f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (H)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorETHue = 0.10f;
        public float sharedColorETHueCached = 0.10f;
        public static Vector4 sharedColorETHueDefaults = new Vector4 (0.1f, 0.1f, 0.1f, 0.1f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (S)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorETSaturation = 0.75f;
        public float sharedColorETSaturationCached = 0.75f;
        public static Vector4 sharedColorETSaturationDefaults = new Vector4 (0.75f, 0.75f, 0.75f, 0.75f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (B)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorETBrightness = 0.6f;
        public float sharedColorETBrightnessCached = 0.6f;
        public static Vector4 sharedColorETBrightnessDefaults = new Vector4 (0.6f, 0.6f, 0.6f, 0.6f);




        // Shared properties / Surface / leading edge

        [KSPField (guiActiveEditor = true, guiActive = false, guiName = "| Material L"),
        UI_Toggle (scene = UI_Scene.Editor, disabledText = "", enabledText = "")]
        public bool sharedFieldGroupColorEL = false;
        public bool sharedFieldGroupColorELCached = false;
        public bool sharedFieldGroupColorELStatic = false;
        private static string[] sharedFieldGroupColorELArray = new string[] { "sharedMaterialEL", "sharedColorELOpacity", "sharedColorELHue", "sharedColorELSaturation", "sharedColorELBrightness" };

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Material", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 4f, incrementSlide = 1f)]
        public float sharedMaterialEL = 4f;
        public float sharedMaterialELCached = 4f;
        public static Vector4 sharedMaterialELDefaults = new Vector4 (4f, 4f, 4f, 4f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Opacity", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorELOpacity = 0f;
        public float sharedColorELOpacityCached = 0f;
        public static Vector4 sharedColorELOpacityDefaults = new Vector4 (0f, 0f, 0f, 0f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (H)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorELHue = 0.10f;
        public float sharedColorELHueCached = 0.10f;
        public static Vector4 sharedColorELHueDefaults = new Vector4 (0.1f, 0.1f, 0.1f, 0.1f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (S)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorELSaturation = 0.75f;
        public float sharedColorELSaturationCached = 0.75f;
        public static Vector4 sharedColorELSaturationDefaults = new Vector4 (0.75f, 0.75f, 0.75f, 0.75f);

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Color (B)", guiFormat = "F3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 1f, incrementSlide = 0.01f)]
        public float sharedColorELBrightness = 0.6f;
        public float sharedColorELBrightnessCached = 0.6f;
        public static Vector4 sharedColorELBrightnessDefaults = new Vector4 (0.6f, 0.6f, 0.6f, 0.6f);




        // Default values
        // Vector4 (defaultWing, defaultCtrl, defaultWingBackup, defaultCtrlBackup)

        [KSPEvent (guiActive = false, guiActiveEditor = true, guiName = "Replace defaults")]
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

        [KSPEvent (guiActive = false, guiActiveEditor = true, guiName = "Restore defaults")]
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




        // Some handy bools

        [KSPField] public bool isCtrlSrf = false;
        [KSPField] public bool isWingAsCtrlSrf = false;

        [KSPField (isPersistant = true)] public bool isAttached = false;
        [KSPField (isPersistant = true)] public bool isSetToDefaultValues = false;

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
        private bool logFieldSetup = false;




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

                float ctrlOffsetRootClamped = Mathf.Clamp (sharedBaseOffsetRoot, sharedBaseOffsetLimits.z, ctrlOffsetRootLimit - 0.075f);
                float ctrlOffsetTipClamped = Mathf.Clamp (sharedBaseOffsetTip, -ctrlOffsetTipLimit + 0.075f, sharedBaseOffsetLimits.w);

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
            // Woah, looks like this is unnecessary
            /*
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
            */
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

        private int GetFieldMode ()
        {
            if (!isCtrlSrf) return 1;
            else return 2;
        }

        private void SetupFields ()
        {
            SetFieldVisibility ("sharedFieldGroupBase", true);
            SetFieldType ("sharedBaseLength", 1, GetLimitsFromType (sharedBaseLengthLimits), sharedIncrementMain, false, GetDefault (sharedBaseLengthDefaults));
            SetFieldType ("sharedBaseWidthRoot", GetFieldMode (), GetLimitsFromType (sharedBaseWidthLimits), sharedIncrementMain, false, GetDefault (sharedBaseWidthRootDefaults));
            SetFieldType ("sharedBaseWidthTip", GetFieldMode (), GetLimitsFromType (sharedBaseWidthLimits), sharedIncrementMain, false, GetDefault (sharedBaseWidthTipDefaults));
            SetFieldType ("sharedBaseThicknessRoot", 2, sharedBaseThicknessLimits, sharedIncrementSmall, false, GetDefault (sharedBaseThicknessRootDefaults));
            SetFieldType ("sharedBaseThicknessTip", 2, sharedBaseThicknessLimits, sharedIncrementSmall, false, GetDefault (sharedBaseThicknessTipDefaults));
            SetFieldType ("sharedBaseOffsetRoot", GetFieldMode (), GetLimitsFromType (sharedBaseOffsetLimits), GetIncrementFromType (sharedIncrementMain, sharedIncrementSmall), false, GetDefault (sharedBaseOffsetRootDefaults));
            SetFieldType ("sharedBaseOffsetTip", GetFieldMode (), GetLimitsFromType (sharedBaseOffsetLimits), GetIncrementFromType (sharedIncrementMain, sharedIncrementSmall), false, GetDefault (sharedBaseOffsetTipDefaults));

            SetFieldVisibility ("sharedFieldGroupEdgeTrailing", true);
            SetFieldType ("sharedEdgeTypeTrailing", 2, GetLimitsFromType (sharedEdgeTypeLimits), sharedIncrementInt, false, GetDefault (sharedEdgeTypeTrailingDefaults));
            SetFieldType ("sharedEdgeWidthTrailingRoot", 2, sharedEdgeWidthLimits, sharedIncrementSmall, false, GetDefault (sharedEdgeWidthTrailingRootDefaults));
            SetFieldType ("sharedEdgeWidthTrailingTip", 2, sharedEdgeWidthLimits, sharedIncrementSmall, false, GetDefault (sharedEdgeWidthTrailingTipDefaults));

            SetFieldVisibility ("sharedFieldGroupEdgeLeading", !isCtrlSrf);
            SetFieldType ("sharedEdgeTypeLeading", 2, GetLimitsFromType (sharedEdgeTypeLimits), sharedIncrementInt, false, GetDefault (sharedEdgeTypeLeadingDefaults));
            SetFieldType ("sharedEdgeWidthLeadingRoot", 2, sharedEdgeWidthLimits, sharedIncrementSmall, false, GetDefault (sharedEdgeWidthLeadingRootDefaults));
            SetFieldType ("sharedEdgeWidthLeadingTip", 2, sharedEdgeWidthLimits, sharedIncrementSmall, false, GetDefault (sharedEdgeWidthLeadingTipDefaults));

            SetFieldVisibility ("sharedFieldGroupColorST", true);
            SetFieldType ("sharedMaterialST", 2, sharedMaterialLimits, sharedIncrementInt, false, GetDefault (sharedMaterialSTDefaults));
            SetFieldType ("sharedColorSTOpacity", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSTOpacityDefaults));
            SetFieldType ("sharedColorSTHue", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSTHueDefaults));
            SetFieldType ("sharedColorSTSaturation", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSTSaturationDefaults));
            SetFieldType ("sharedColorSTBrightness", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSTBrightnessDefaults));

            SetFieldVisibility ("sharedFieldGroupColorSB", true);
            SetFieldType ("sharedMaterialSB", 2, sharedMaterialLimits, sharedIncrementInt, false, GetDefault (sharedMaterialSBDefaults));
            SetFieldType ("sharedColorSBOpacity", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSBOpacityDefaults));
            SetFieldType ("sharedColorSBHue", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSBHueDefaults));
            SetFieldType ("sharedColorSBSaturation", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSBSaturationDefaults));
            SetFieldType ("sharedColorSBBrightness", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorSBBrightnessDefaults));

            SetFieldVisibility ("sharedFieldGroupColorET", true);
            SetFieldType ("sharedMaterialET", 2, sharedMaterialLimits, sharedIncrementInt, false, GetDefault (sharedMaterialETDefaults));
            SetFieldType ("sharedColorETOpacity", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorETOpacityDefaults));
            SetFieldType ("sharedColorETHue", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorETHueDefaults));
            SetFieldType ("sharedColorETSaturation", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorETSaturationDefaults));
            SetFieldType ("sharedColorETBrightness", 2, sharedColorLimits, sharedIncrementColor, false, GetDefault (sharedColorETBrightnessDefaults));

            SetFieldVisibility ("sharedFieldGroupColorEL", !isCtrlSrf);
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
            BaseField field = Fields[name];
            UI_FloatEdit ui = (UI_FloatEdit) field.uiControlEditor;

            float value = (float) this.GetType ().GetField (name).GetValue (this);
            if (logFieldSetup) DebugLogWithID ("SetFieldType", "Started for field " + name + " | UI type: " + type + " | Limits: " + limits.x + "-" + limits.y + " | Increment: " + increment + " | Current value: " + value + " | Current limits: " + ui.minValue + "-" + ui.maxValue);

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

            if (!isSetToDefaultValues) this.GetType ().GetField (name).SetValue (this, defaultValue);
            else this.GetType ().GetField (name).SetValue (this, Mathf.Clamp (value, limits.x, limits.y));

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
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (!uiStyleConfigured) InitStyle ();
                RenderingManager.AddToPostDrawQueue (0, OnDraw);
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

        public KeyCode uiKeyCodeEdit = KeyCode.H;
        // public KeyCode uiKeyCodeNext = KeyCode.J;
        // public KeyCode uiKeyCodePrev = KeyCode.H;
        // public KeyCode uiKeyCodeAdd = KeyCode.N;
        // public KeyCode uiKeyCodeSubtract = KeyCode.B;

        public static Rect uiRect = new Rect ();
        public static bool uiStyleConfigured = false;
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
                if (uiWindowActive)
                {
                    mousePos = GetMousePos ();
                    EditorLogic EdLogInstance = EditorLogic.fetch;
                    bool cursorInGUI = false;

                    if (uiAdjustWindow)
                    {
                        uiAdjustWindow = false;
                        uiRect = GUILayout.Window (273, uiRect, OnWindow, GetWindowTitle (), uiStyleWindow, GUILayout.Height (0));
                    }
                    else uiRect = GUILayout.Window (273, uiRect, OnWindow, GetWindowTitle (), uiStyleWindow);
                    if (uiRect.x == 0f && uiRect.y == 0f) uiRect = uiRect.SetToScreenCenter ();

                    // Thanks to ferram4
                    // Following section lock the editor, preventing window clickthrough

                    cursorInGUI = uiRect.Contains (mousePos);
                    if (cursorInGUI)
                    {
                        EdLogInstance.Lock (false, false, false, "WingProceduralWindow");
                        EditorTooltip.Instance.HideToolTip ();
                    }
                    else if (!cursorInGUI)
                    {
                        EdLogInstance.Unlock ("WingProceduralWindow");
                    }
                }
            }
        }

        public static GUIStyle uiStyleWindow = new GUIStyle ();
        public static GUIStyle uiStyleLabelMedium = new GUIStyle ();
        public static GUIStyle uiStyleLabelHint = new GUIStyle ();
        public static GUIStyle uiStyleButton = new GUIStyle ();
        public static GUIStyle uiStyleSlider = new GUIStyle ();
        public static GUIStyle uiStyleSliderThumb = new GUIStyle ();

        public static Vector4 uiColorSliderBase = new Vector4 (0.25f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderEdgeL = new Vector4 (0.20f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderEdgeT = new Vector4 (0.15f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsST = new Vector4 (0.10f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsSB = new Vector4 (0.05f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsET = new Vector4 (0.00f, 0.5f, 0.4f, 1f);
        public static Vector4 uiColorSliderColorsEL = new Vector4 (0.95f, 0.5f, 0.4f, 1f);

        private static Vector3 mousePos = Vector3.zero;

        public static Vector3 GetMousePos ()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;
            return mousePos;
        }

        private void OnWindow (int window)
        {
            if (uiEditMode)
            {
                bool returnEarly = false;
                GUILayout.BeginHorizontal ();
                GUILayout.BeginVertical ();
                if (uiLastFieldName.Length > 0) GUILayout.Label ("Last: " + uiLastFieldName, uiStyleLabelMedium);
                else GUILayout.Label ("Property editor", uiStyleLabelMedium);
                if (uiLastFieldTooltip.Length > 0) GUILayout.Label (uiLastFieldTooltip + "\nPress H to exit edit mode\n__________________", uiStyleLabelHint);
                GUILayout.EndVertical ();
                if (GUILayout.Button ("Close", uiStyleButton, GUILayout.MaxWidth (50f)))
                {
                    EditorLogic.fetch.Unlock ("WingProceduralWindow");
                    uiWindowActive = false;
                    returnEarly = true;
                }
                GUILayout.EndHorizontal ();
                if (returnEarly) return;

                DrawFieldGroupHeader (ref sharedFieldGroupBaseStatic, ref sharedFieldGroupBase, "Base");
                if (sharedFieldGroupBaseStatic)
                {
                    DrawField (ref sharedBaseLength, sharedIncrementMain, GetLimitsFromType (sharedBaseLengthLimits), "Length", uiColorSliderBase);
                    DrawField (ref sharedBaseWidthRoot, sharedIncrementMain, GetLimitsFromType (sharedBaseWidthLimits), "Width (root)", uiColorSliderBase);
                    DrawField (ref sharedBaseWidthTip, sharedIncrementMain, GetLimitsFromType (sharedBaseWidthLimits), "Width (tip)", uiColorSliderBase);
                    if (isCtrlSrf) DrawField (ref sharedBaseOffsetRoot, GetIncrementFromType (sharedIncrementMain, sharedIncrementSmall), GetLimitsFromType (sharedBaseOffsetLimits), "Offset (root)", uiColorSliderBase);
                    DrawField (ref sharedBaseOffsetTip, GetIncrementFromType (sharedIncrementMain, sharedIncrementSmall), GetLimitsFromType (sharedBaseOffsetLimits), "Offset (tip)", uiColorSliderBase);
                    DrawField (ref sharedBaseThicknessRoot, sharedIncrementSmall, sharedBaseThicknessLimits, "Thickness (root)", uiColorSliderBase);
                    DrawField (ref sharedBaseThicknessTip, sharedIncrementSmall, sharedBaseThicknessLimits, "Thickness (tip)", uiColorSliderBase);
                }

                if (!isCtrlSrf)
                {
                    DrawFieldGroupHeader (ref sharedFieldGroupEdgeLeadingStatic, ref sharedFieldGroupEdgeLeading, "Edge (leading)");
                    if (sharedFieldGroupEdgeLeadingStatic)
                    {
                        DrawField (ref sharedEdgeTypeLeading, sharedIncrementInt, GetLimitsFromType (sharedEdgeTypeLimits), "Shape", uiColorSliderEdgeL);
                        DrawField (ref sharedEdgeWidthLeadingRoot, sharedIncrementSmall, sharedEdgeWidthLimits, "Width (root)", uiColorSliderEdgeL);
                        DrawField (ref sharedEdgeWidthLeadingTip, sharedIncrementSmall, sharedEdgeWidthLimits, "Width (tip)", uiColorSliderEdgeL);
                    }
                }

                DrawFieldGroupHeader (ref sharedFieldGroupEdgeTrailingStatic, ref sharedFieldGroupEdgeTrailing, "Edge (trailing)");
                if (sharedFieldGroupEdgeTrailingStatic)
                {
                    DrawField (ref sharedEdgeTypeTrailing, sharedIncrementInt, GetLimitsFromType (sharedEdgeTypeLimits), "Shape", uiColorSliderEdgeT);
                    DrawField (ref sharedEdgeWidthTrailingRoot, sharedIncrementSmall, sharedEdgeWidthLimits, "Width (root)", uiColorSliderEdgeT);
                    DrawField (ref sharedEdgeWidthTrailingTip, sharedIncrementSmall, sharedEdgeWidthLimits, "Width (tip)", uiColorSliderEdgeT);
                }

                DrawFieldGroupHeader (ref sharedFieldGroupColorSTStatic, ref sharedFieldGroupColorST, "Surface (top)");
                if (sharedFieldGroupColorSTStatic)
                {
                    DrawField (ref sharedMaterialST, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsST);
                    DrawField (ref sharedColorSTOpacity, sharedIncrementColor, sharedColorLimits, "Opacity", uiColorSliderColorsST);
                    DrawField (ref sharedColorSTHue, sharedIncrementColor, sharedColorLimits, "Hue", uiColorSliderColorsST);
                    DrawField (ref sharedColorSTSaturation, sharedIncrementColor, sharedColorLimits, "Saturation", uiColorSliderColorsST);
                    DrawField (ref sharedColorSTBrightness, sharedIncrementColor, sharedColorLimits, "Brightness", uiColorSliderColorsST);
                }

                DrawFieldGroupHeader (ref sharedFieldGroupColorSBStatic, ref sharedFieldGroupColorSB, "Surface (bottom)");
                if (sharedFieldGroupColorSBStatic)
                {
                    DrawField (ref sharedMaterialSB, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsSB);
                    DrawField (ref sharedColorSBOpacity, sharedIncrementColor, sharedColorLimits, "Opacity", uiColorSliderColorsSB);
                    DrawField (ref sharedColorSBHue, sharedIncrementColor, sharedColorLimits, "Hue", uiColorSliderColorsSB);
                    DrawField (ref sharedColorSBSaturation, sharedIncrementColor, sharedColorLimits, "Saturation", uiColorSliderColorsSB);
                    DrawField (ref sharedColorSBBrightness, sharedIncrementColor, sharedColorLimits, "Brightness", uiColorSliderColorsSB);
                }

                DrawFieldGroupHeader (ref sharedFieldGroupColorETStatic, ref sharedFieldGroupColorET, "Surface (trailing edge)");
                if (sharedFieldGroupColorETStatic)
                {
                    DrawField (ref sharedMaterialET, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsET);
                    DrawField (ref sharedColorETOpacity, sharedIncrementColor, sharedColorLimits, "Opacity", uiColorSliderColorsET);
                    DrawField (ref sharedColorETHue, sharedIncrementColor, sharedColorLimits, "Hue", uiColorSliderColorsET);
                    DrawField (ref sharedColorETSaturation, sharedIncrementColor, sharedColorLimits, "Saturation", uiColorSliderColorsET);
                    DrawField (ref sharedColorETBrightness, sharedIncrementColor, sharedColorLimits, "Brightness", uiColorSliderColorsET);
                }

                if (!isCtrlSrf)
                {
                    DrawFieldGroupHeader (ref sharedFieldGroupColorELStatic, ref sharedFieldGroupColorEL, "Surface (leading edge)");
                    if (sharedFieldGroupColorELStatic)
                    {
                        DrawField (ref sharedMaterialEL, sharedIncrementInt, sharedMaterialLimits, "Material", uiColorSliderColorsEL);
                        DrawField (ref sharedColorELOpacity, sharedIncrementColor, sharedColorLimits, "Opacity", uiColorSliderColorsEL);
                        DrawField (ref sharedColorELHue, sharedIncrementColor, sharedColorLimits, "Hue", uiColorSliderColorsEL);
                        DrawField (ref sharedColorELSaturation, sharedIncrementColor, sharedColorLimits, "Saturation", uiColorSliderColorsEL);
                        DrawField (ref sharedColorELBrightness, sharedIncrementColor, sharedColorLimits, "Brightness", uiColorSliderColorsEL);
                    }
                }

                GUILayout.BeginHorizontal ();
                if (GUILayout.Button ("Save to defaults", uiStyleButton)) ReplaceDefaults ();
                if (GUILayout.Button ("Restore defaults", uiStyleButton)) RestoreDefaults ();
                GUILayout.EndHorizontal ();
            }
            else
            {
                if (uiEditModeTimeout) GUILayout.Label ("Exiting edit mode...\n", uiStyleLabelMedium);
                else
                {
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label ("Press H while pointing at a\nprocedural part to edit it", uiStyleLabelHint);
                    if (GUILayout.Button ("Close", uiStyleButton, GUILayout.MaxWidth (50f)))
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

        private void DrawField (ref float field, float increment, Vector2 limits, string name, Vector4 hsbColor)
        {
            bool changed = false;
            float value = UIUtility.FieldSlider (field, increment, limits, name, "F3", out changed, uiStyleSlider, uiStyleSliderThumb, uiStyleLabelHint, ColorHSBToRGB (hsbColor));
            if (changed)
            {
                field = value;
                uiLastFieldName = name;
                uiLastFieldTooltip = UpdateTooltipText (ref field);
            }
        }

        private void DrawFieldGroupHeader (ref bool fieldGroupBoolStatic, ref bool fieldGroupBool, string header)
        {
            GUILayout.BeginHorizontal ();
            if (GUILayout.Button (header, uiStyleLabelHint))
            {
                fieldGroupBoolStatic = !fieldGroupBoolStatic;
                fieldGroupBool = fieldGroupBoolStatic;
                uiAdjustWindow = true;
            }
            if (fieldGroupBoolStatic) GUILayout.Label ("|", uiStyleLabelHint, GUILayout.MaxWidth (15f));
            else GUILayout.Label ("+", uiStyleLabelHint, GUILayout.MaxWidth (15f));
            GUILayout.EndHorizontal ();
        }

        private static string uiLastFieldName = "";
        private static string uiLastFieldTooltip = "Additional info on edited \nproperties is displayed here";

        private string UpdateTooltipText (ref float field)
        {
            // Base descriptions
            if (field == sharedBaseLength)
            {
                if (!isCtrlSrf) return "Lateral measurement of the wing \n";
                else            return "Lateral measurement of the root \ncross section of the control surface";
            }
            else if (field == sharedBaseWidthRoot)
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the wing \nat the root cross section";
                else            return "Longitudinal measurement of \nthe root chord";
            }
            else if (field == sharedBaseWidthTip)
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the wing \nat the tip cross section";
                else            return "Longitudinal measurement of \nthe tip chord";
            }
            else if (field == sharedBaseOffsetRoot)
            {
                if (!isCtrlSrf) return "This property shouldn't be accessible \non a wing";
                else            return "Offset of the trailing edge \nroot corner on the lateral axis";
            }
            else if (field == sharedBaseOffsetTip)
            {
                if (!isCtrlSrf) return "Distance between midpoints of the cross \nsections on the longitudinal axis";
                else            return "Offset of the trailing edge \ntip corner on the lateral axis";
            }
            else if (field == sharedBaseThicknessRoot)
            {
                if (!isCtrlSrf) return "Thickness at the root cross section \nUsually kept proportional to edge width";
                else            return "Thickness at the root cross section \nUsually kept proportional to edge width";
            }
            else if (field == sharedBaseThicknessTip)
            {
                if (!isCtrlSrf) return "Thickness at the tip cross section \nUsually kept proportional to edge width";
                else            return "Thickness at the tip cross section \nUsually kept proportional to edge width";
            }

            // Edge descriptions
            else if (field == sharedEdgeTypeTrailing)
            {
                if (!isCtrlSrf) return "Shape of the trailing edge cross \nsection (round/biconvex/sharp)";
                else            return "Shape of the trailing edge cross \nsection (round/biconvex/sharp)";
            }
            else if (field == sharedEdgeWidthTrailingRoot)
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the trailing \nedge cross section at wing root";
                else            return "Longitudinal measurement of the trailing \nedge cross section at with root";
            }
            else if (field == sharedEdgeWidthTrailingTip)
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the trailing \nedge cross section at wing tip";
                else            return "Longitudinal measurement of the trailing \nedge cross section at with tip";
            }
            else if (field == sharedEdgeTypeLeading)
            {
                if (!isCtrlSrf) return "Shape of the leading edge cross \nsection (round/biconvex/sharp)";
                else            return "Shape of the leading edge cross \nsection (round/biconvex/sharp)";
            }
            else if (field == sharedEdgeWidthLeadingRoot)
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the leading \nedge cross section at wing root";
                else            return "Longitudinal measurement of the leading \nedge cross section at wing root";
            }
            else if (field == sharedEdgeWidthLeadingTip)
            {
                if (!isCtrlSrf) return "Longitudinal measurement of the leading \nedge cross section at with tip";
                else            return "Longitudinal measurement of the leading \nedge cross section at with tip";
            }

            // Surface descriptions
            else if (field == sharedMaterialST || field == sharedMaterialSB || field == sharedMaterialET || field == sharedMaterialEL)
            {
                if (!isCtrlSrf) return "Surface material (uniform fill, plating, \nLRSI/HRSI tiles and so on)";
                else            return "Surface material (uniform fill, plating, \nLRSI/HRSI tiles and so on)";
            }
            else if (field == sharedColorSTOpacity || field == sharedColorSBOpacity || field == sharedColorETOpacity || field == sharedColorELOpacity)
            {
                if (!isCtrlSrf) return "Fairly self-explanatory, controls the paint \nopacity: no paint at 0, full coverage at 1";
                else            return "Fairly self-explanatory, controls the paint \nopacity: no paint at 0, full coverage at 1";
            }
            else if (field == sharedColorSTHue || field == sharedColorSBHue || field == sharedColorETHue || field == sharedColorELHue)
            {
                if (!isCtrlSrf) return "Controls the paint hue (HSB axis): \nvalues from zero to one make full circle";
                else            return "Controls the paint hue (HSB axis): \nvalues from zero to one make full circle";
            }
            else if (field == sharedColorSTSaturation || field == sharedColorSBSaturation || field == sharedColorETSaturation || field == sharedColorELSaturation)
            {
                if (!isCtrlSrf) return "Controls the paint saturation (HSB axis): \ncolorless at 0, full color at 1";
                else            return "Controls the paint saturation (HSB axis): \ncolorless at 0, full color at 1";
            }
            else if (field == sharedColorSTBrightness || field == sharedColorSTBrightness || field == sharedColorETBrightness || field == sharedColorELBrightness)
            {
                if (!isCtrlSrf) return "Controls the paint brightness (HSB axis): \nblack at 0, white at 1, primary colors at 0.5";
                else            return "Controls the paint brightness (HSB axis): \nblack at 0, white at 1, primary colors at 0.5";
            }

            // This should not really happen
            else return "Unknown field\n";
        }

        private void InitStyle ()
        {
            uiStyleWindow = new GUIStyle (HighLogic.Skin.window);
            uiStyleWindow.fixedWidth = 250f;
            uiStyleWindow.wordWrap = true;
            uiStyleWindow.normal.textColor = Color.white;
            uiStyleWindow.fontStyle = FontStyle.Normal;
            uiStyleWindow.fontSize = 13;
            uiStyleWindow.alignment = TextAnchor.UpperLeft;

            uiStyleLabelMedium = new GUIStyle (HighLogic.Skin.label);
            uiStyleLabelMedium.stretchWidth = true;
            uiStyleLabelMedium.fontSize = 13;
            uiStyleLabelMedium.normal.textColor = Color.white;

            uiStyleLabelHint = new GUIStyle (HighLogic.Skin.label);
            uiStyleLabelHint.stretchWidth = true;
            uiStyleLabelHint.fontSize = 11;
            uiStyleLabelHint.normal.textColor = Color.white;

            uiStyleButton = new GUIStyle (HighLogic.Skin.button);
            uiStyleButton.font = uiStyleLabelMedium.font;
            uiStyleButton.fontSize = 13;
            uiStyleButton.fontStyle = FontStyle.Normal;
            uiStyleButton.normal.textColor = Color.white;

            uiStyleSlider = HighLogic.Skin.horizontalSlider;

            uiStyleSliderThumb = new GUIStyle (HighLogic.Skin.horizontalSliderThumb);
            uiStyleSliderThumb.fixedWidth = 0f;

            uiStyleConfigured = true;
            uiAdjustWindow = true;
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
                            uiAdjustWindow = true;
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
                    uiAdjustWindow = true;
                    uiEditModeTimeout = false;
                    uiEditModeTimer = 0.0f;
                }
            }
            else
            {
                if (uiEditMode)
                {
                    if (Input.GetKeyDown (uiKeyCodeEdit))
                    {
                        uiEditMode = false;
                        uiEditModeTimeout = true;
                        uiAdjustWindow = true;
                        return;
                    }
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
