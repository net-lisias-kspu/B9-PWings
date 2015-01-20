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
    class TweakableTestExtension : PartModule
    {
        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "MaterialMaterial", guiFormat = "S"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 5f, incrementSlide = 1f)]
        public float test01 = 4f;
        public float test01Cached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Material (bottom)", guiFormat = "S"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0f, maxValue = 5f, incrementSlide = 1f)]
        public float test02 = 4f;
        public float test02Cached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Offset (root)", guiFormat = "S3", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementSlide = 0.125f)]
        public float test03 = 4f;
        public float test03Cached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Leading edge shape", guiFormat = "S3"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = -8f, maxValue = 8f, incrementSlide = 0.125f)]
        public float test04 = 0f;
        public float test04Cached = 0f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Test 05", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float test05 = 4f;
        public float test05Cached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Test 06", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float test06 = 4f;
        public float test06Cached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Test 07", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float test07 = 4f;
        public float test07Cached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Test 08", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = -8f, maxValue = 8f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float test08 = 0f;
        public float test08Cached = 0f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Test 09", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float test09 = 4f;
        public float test09Cached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Test 10", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float test10 = 4f;
        public float test10Cached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Test 11", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = 0.25f, maxValue = 16f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float test11 = 4f;
        public float test11Cached = 4f;

        [KSPField (isPersistant = true, guiActiveEditor = true, guiActive = false, guiName = "Test 12", guiFormat = "S4", guiUnits = "m"),
        UI_FloatEdit (scene = UI_Scene.Editor, minValue = -8f, maxValue = 8f, incrementLarge = 1f, incrementSlide = 0.125f)]
        public float test12 = 0f;
        public float test12Cached = 0f;
    }
}
