﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using KSP;

namespace WingProcedural
{
    // Referenced from FAR debug manager
    // Credit goes to ferram4

    [KSPAddon (KSPAddon.Startup.SpaceCentre, false)]
    public class WingProceduralManager : MonoBehaviour
    {
        public static KSP.IO.PluginConfiguration config;
        private ApplicationLauncherButton debugButtonStock = null;

        private bool inputLocked = false;
        private bool windowOpen = false;
        private Rect windowPos = new Rect (50, 50, 700, 250);

        public static GUIStyle uiStyleWindow = new GUIStyle ();
        public static GUIStyle uiStyleLabelMedium = new GUIStyle ();
        public static GUIStyle uiStyleLabelHint = new GUIStyle ();
        public static GUIStyle uiStyleButton = new GUIStyle ();
        public static GUIStyle uiStyleSlider = new GUIStyle ();
        public static GUIStyle uiStyleSliderThumb = new GUIStyle ();
        public static GUIStyle uiStyleToggle = new GUIStyle ();
        public static bool uiStyleConfigured = false;

        private enum MenuTab
        {
            DebugAndData
        }

        private static string[] MenuTabStrings = new string[]
        {
            "Debug Options"
        };

        private MenuTab activeTab = MenuTab.DebugAndData;

        public void Awake ()
        {
            LoadConfigs ();
            GameEvents.onGUIApplicationLauncherReady.Add (OnGUIAppLauncherReady);
        }

        private void OnGUIAppLauncherReady ()
        {
            if (ApplicationLauncher.Ready)
            {
                debugButtonStock = ApplicationLauncher.Instance.AddModApplication 
                (
                    OnAppLaunchToggleOn,
                    OnAppLaunchToggleOff,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    ApplicationLauncher.AppScenes.SPACECENTER,
                    (Texture) GameDatabase.Instance.GetTexture ("B9_Aerospace/Plugins/icon_stock", false)
                );
            }
        }

        private void OnAppLaunchToggleOn ()
        {
            windowOpen = true;
        }

        private void OnAppLaunchToggleOff ()
        {
            windowOpen = false;
        }

        private void DummyVoid () { }


        public static Font uiFont = null;
        private static float alphaNormal = 0.5f;
        private static float alphaHover = 0.35f;
        private static float alphaActive = 0.75f;

        public static void ConfigureStyles ()
        {
            if (uiFont == null)
            {
                uiFont = Resources.GetBuiltinResource (typeof (Font), "Arial.ttf") as Font;
            }
            if (uiFont != null)
            {
                uiStyleWindow = new GUIStyle (HighLogic.Skin.window);
                uiStyleWindow.fixedWidth = 250f;
                uiStyleWindow.wordWrap = true;
                uiStyleWindow.normal.textColor = Color.white;
                uiStyleWindow.font = uiFont;
                uiStyleWindow.fontStyle = FontStyle.Normal;
                uiStyleWindow.fontSize = 13;
                uiStyleWindow.alignment = TextAnchor.UpperLeft;

                uiStyleLabelMedium = new GUIStyle (HighLogic.Skin.label);
                uiStyleLabelMedium.stretchWidth = true;
                uiStyleLabelMedium.font = uiFont;
                uiStyleLabelMedium.fontStyle = FontStyle.Normal;
                uiStyleLabelMedium.fontSize = 13;
                uiStyleLabelMedium.normal.textColor = Color.white;

                uiStyleLabelHint = new GUIStyle (HighLogic.Skin.label);
                uiStyleLabelHint.stretchWidth = true;
                uiStyleLabelHint.font = uiFont;
                uiStyleLabelHint.fontStyle = FontStyle.Normal;
                uiStyleLabelHint.fontSize = 11;
                uiStyleLabelHint.normal.textColor = Color.white;

                uiStyleButton = new GUIStyle (HighLogic.Skin.button);
                uiStyleButton.font = uiFont;
                uiStyleButton.fontStyle = FontStyle.Normal;
                uiStyleButton.fontSize = 11;
                AssignTexturesToStyle (uiStyleButton);
                uiStyleButton.fixedHeight = 16;

                uiStyleSlider = new GUIStyle (HighLogic.Skin.horizontalSlider);
                AssignTexturesToStyle (uiStyleSlider);
                uiStyleButton.fixedHeight = 16;

                uiStyleSliderThumb = new GUIStyle (HighLogic.Skin.horizontalSliderThumb);
                uiStyleSliderThumb.normal.background = Color.black.WithAlpha (0).GetTexture2D ();
                uiStyleSliderThumb.hover.background = Color.black.WithAlpha (0).GetTexture2D ();
                uiStyleSliderThumb.active.background = Color.black.WithAlpha (0).GetTexture2D ();
                uiStyleSliderThumb.onNormal.background = Color.black.WithAlpha (0).GetTexture2D ();
                uiStyleSliderThumb.onHover.background = Color.black.WithAlpha (0).GetTexture2D ();
                uiStyleSliderThumb.onActive.background = Color.black.WithAlpha (0).GetTexture2D ();
                uiStyleSliderThumb.fixedWidth = 0f;
                uiStyleSliderThumb.fixedHeight = 16;

                uiStyleToggle = new GUIStyle (HighLogic.Skin.toggle);
                uiStyleToggle.font = uiFont;
                uiStyleToggle.fontStyle = FontStyle.Normal;
                uiStyleToggle.fontSize = 11;
                uiStyleToggle.normal.textColor = Color.white;
                uiStyleToggle.padding = new RectOffset (4, 4, 4, 4);
                uiStyleToggle.margin = new RectOffset (4, 4, 4, 4);

                uiStyleConfigured = true;
            }
        }

        private static void AssignTexturesToStyle (GUIStyle s)
        {
            s.normal.textColor = s.onNormal.textColor = Color.white;
            s.hover.textColor = s.onHover.textColor = Color.white;
            s.active.textColor = s.onActive.textColor = Color.white;

            s.normal.background = Color.black.WithAlpha (alphaNormal).GetTexture2D ();
            s.hover.background = Color.black.WithAlpha (alphaHover).GetTexture2D ();
            s.active.background = Color.black.WithAlpha (alphaActive).GetTexture2D ();
            s.onNormal.background = Color.black.WithAlpha (alphaNormal).GetTexture2D ();
            s.onHover.background = Color.black.WithAlpha (alphaHover).GetTexture2D ();
            s.onActive.background = Color.black.WithAlpha (alphaActive).GetTexture2D ();
            uiStyleButton.border = new RectOffset (0, 0, 0, 0);
        }

        public void OnGUI ()
        {
            GUI.skin = HighLogic.Skin;
            if (!uiStyleConfigured) ConfigureStyles ();
            if (uiStyleConfigured)
            {
                if (windowOpen)
                {
                    windowPos = GUILayout.Window ("WingProceduralManagerWindow".GetHashCode (), windowPos, debugWindow, "B9 Procedural Part Options", GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true));
                    if (!inputLocked && windowPos.Contains (UIUtility.GetMousePos ()))
                    {
                        InputLockManager.SetControlLock (ControlTypes.KSC_ALL, "WingProceduralManagerLock");
                        inputLocked = true;
                    }
                    else if (inputLocked && !windowPos.Contains (UIUtility.GetMousePos ()))
                    {
                        InputLockManager.RemoveControlLock ("WingProceduralManagerLock");
                        inputLocked = false;
                    }
                }
                else if (inputLocked)
                {
                    InputLockManager.RemoveControlLock ("WingProceduralManagerLock");
                    inputLocked = false;
                }
            }
        }


        private void debugWindow (int windowID)
        {
            GUIStyle toggleStyle = new GUIStyle (GUI.skin.toggle);
            toggleStyle.stretchHeight = true;
            toggleStyle.stretchWidth = true;
            toggleStyle.padding = new RectOffset (4, 4, 4, 4);
            toggleStyle.margin = new RectOffset (4, 4, 4, 4);

            GUIStyle boxStyle = new GUIStyle (GUI.skin.box);
            boxStyle.stretchHeight = true;
            boxStyle.stretchWidth = true;
            boxStyle.padding = new RectOffset (4, 4, 4, 4);
            boxStyle.margin = new RectOffset (4, 4, 4, 4);

            activeTab = (MenuTab) GUILayout.SelectionGrid ((int) activeTab, MenuTabStrings, 4);

            if (activeTab == MenuTab.DebugAndData) DebugAndDataTab ();

            GUI.DragWindow ();
            windowPos = UIUtility.ClampToScreen (windowPos);
        }

        private void DebugAndDataTab ()
        {
            GUILayout.BeginHorizontal ();
            GUILayout.BeginVertical ();
            GUILayout.Label 
            (
                "\nThis tab contains a variety of logging modes. If you are encountering problems with the mod, you might be asked to enable one of those and attempt to reproduce the issue again. " + 
                "Upon doing so, check the Debug tab of Alt+F12 window or KSP.log files for results. Be aware that some of these toggles might lead to thousands of messages being generated, so don't keep them enabled in normal play if you are having performance issues."
            , uiStyleLabelHint);

            WingProceduralDebugValues.logCAV = GUILayout.Toggle (WingProceduralDebugValues.logCAV, "  Log aerodynamic setup", uiStyleToggle);
            WingProceduralDebugValues.logUpdate = GUILayout.Toggle (WingProceduralDebugValues.logUpdate, "  Log value detection", uiStyleToggle);
            WingProceduralDebugValues.logUpdateGeometry = GUILayout.Toggle (WingProceduralDebugValues.logUpdateGeometry, "  Log geometry updates", uiStyleToggle);

            WingProceduralDebugValues.logUpdateMaterials = GUILayout.Toggle (WingProceduralDebugValues.logUpdateMaterials, "  Log material updates", uiStyleToggle);
            WingProceduralDebugValues.logMeshReferences = GUILayout.Toggle (WingProceduralDebugValues.logMeshReferences, "  Log mesh reference setup", uiStyleToggle);
            WingProceduralDebugValues.logCheckMeshFilter = GUILayout.Toggle (WingProceduralDebugValues.logCheckMeshFilter, "  Log mesh filter setup", uiStyleToggle);

            WingProceduralDebugValues.logPropertyWindow = GUILayout.Toggle (WingProceduralDebugValues.logPropertyWindow, "  Log property window", uiStyleToggle);
            WingProceduralDebugValues.logFlightSetup = GUILayout.Toggle (WingProceduralDebugValues.logFlightSetup, "  Log pre-flight setup", uiStyleToggle);
            WingProceduralDebugValues.logFieldSetup = GUILayout.Toggle (WingProceduralDebugValues.logFieldSetup, "  Log field setup", uiStyleToggle);

            WingProceduralDebugValues.logFuel = GUILayout.Toggle (WingProceduralDebugValues.logFuel, "  Log fuel setup", uiStyleToggle);
            WingProceduralDebugValues.logLimits = GUILayout.Toggle (WingProceduralDebugValues.logLimits, "  Log field limits", uiStyleToggle);
            WingProceduralDebugValues.logEvents = GUILayout.Toggle (WingProceduralDebugValues.logEvents, "  Log event invocation", uiStyleToggle);

            GUILayout.EndVertical ();
            GUILayout.EndHorizontal ();
        }

        public static void LoadConfigs ()
        {
            config = KSP.IO.PluginConfiguration.CreateForType<WingProceduralManager> ();
            config.load ();

            WingProceduralDebugValues.logCAV = Convert.ToBoolean (config.GetValue ("logCAV", "false"));
            WingProceduralDebugValues.logUpdate = Convert.ToBoolean (config.GetValue ("logUpdate", "false"));
            WingProceduralDebugValues.logUpdateGeometry = Convert.ToBoolean (config.GetValue ("logUpdateGeometry", "false"));

            WingProceduralDebugValues.logUpdateMaterials = Convert.ToBoolean (config.GetValue ("logUpdateMaterials", "false"));
            WingProceduralDebugValues.logMeshReferences = Convert.ToBoolean (config.GetValue ("logMeshReferences", "false"));
            WingProceduralDebugValues.logCheckMeshFilter = Convert.ToBoolean (config.GetValue ("logCheckMeshFilter", "false"));

            WingProceduralDebugValues.logPropertyWindow = Convert.ToBoolean (config.GetValue ("logPropertyWindow", "false"));
            WingProceduralDebugValues.logFlightSetup = Convert.ToBoolean (config.GetValue ("logFlightSetup", "false"));
            WingProceduralDebugValues.logFieldSetup = Convert.ToBoolean (config.GetValue ("logFieldSetup", "false"));

            WingProceduralDebugValues.logFuel = Convert.ToBoolean (config.GetValue ("logFuel", "false"));
            WingProceduralDebugValues.logLimits = Convert.ToBoolean (config.GetValue ("logLimits", "false"));
            WingProceduralDebugValues.logEvents = Convert.ToBoolean (config.GetValue ("logEvents", "false"));
        }

        public static void SaveConfigs ()
        {
            config.SetValue ("logCAV", WingProceduralDebugValues.logCAV.ToString ());
            config.SetValue ("logUpdate", WingProceduralDebugValues.logUpdate.ToString ());
            config.SetValue ("logUpdateGeometry", WingProceduralDebugValues.logUpdateGeometry.ToString ());

            config.SetValue ("logUpdateMaterials", WingProceduralDebugValues.logUpdateMaterials.ToString ());
            config.SetValue ("logMeshReferences", WingProceduralDebugValues.logMeshReferences.ToString ());
            config.SetValue ("logCheckMeshFilter", WingProceduralDebugValues.logCheckMeshFilter.ToString ());

            config.SetValue ("logPropertyWindow", WingProceduralDebugValues.logPropertyWindow.ToString ());
            config.SetValue ("logFlightSetup", WingProceduralDebugValues.logFlightSetup.ToString ());
            config.SetValue ("logFieldSetup", WingProceduralDebugValues.logFieldSetup.ToString ());

            config.SetValue ("logFuel", WingProceduralDebugValues.logFuel.ToString ());
            config.SetValue ("logLimits", WingProceduralDebugValues.logLimits.ToString ());
            config.SetValue ("logEvents", WingProceduralDebugValues.logEvents.ToString ());

            config.save ();
        }

        private void OnDestroy ()
        {
            SaveConfigs ();
            GameEvents.onGUIApplicationLauncherReady.Remove (OnGUIAppLauncherReady);
            if (debugButtonStock != null) ApplicationLauncher.Instance.RemoveModApplication (debugButtonStock);
        }

    }

    public static class WingProceduralDebugValues
    {
        public static bool logCAV = false;
        public static bool logUpdate = false;
        public static bool logUpdateGeometry = false;

        public static bool logUpdateMaterials = false;
        public static bool logMeshReferences = false;
        public static bool logCheckMeshFilter = false;

        public static bool logPropertyWindow = false;
        public static bool logFlightSetup = false;
        public static bool logFieldSetup = false;

        public static bool logFuel = false;
        public static bool logLimits = false;
        public static bool logEvents = false;
    }
}