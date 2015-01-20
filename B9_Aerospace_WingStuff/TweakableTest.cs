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
    class TweakableTest: PartModule
    {
        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Test 01"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float test01 = 0f;
        public float test01Cached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Test 02"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float test02 = 0f;
        public float test02Cached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Test 03"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float test03 = 0f;
        public float test03Cached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Test 04"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float test04 = 0f;
        public float test04Cached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Test 05"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float test05 = 0f;
        public float test05Cached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Test 06"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float test06 = 0f;
        public float test06Cached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Test 07"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float test07 = 0f;
        public float test07Cached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Test 08"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float test08 = 0f;
        public float test08Cached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Test 09"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float test09 = 0f;
        public float test09Cached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Test 10"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float test10 = 0f;
        public float test10Cached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Test 11"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float test11 = 0f;
        public float test11Cached = 0f;

        [KSPField (isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Test 12"),
        UI_FloatRange (minValue = 0.08f, maxValue = 1f, scene = UI_Scene.Editor, stepIncrement = 0.04f)]
        public float test12 = 0f;
        public float test12Cached = 0f;
    }
}
