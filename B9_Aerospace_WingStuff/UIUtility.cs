using KSP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace WingProcedural
{
    public static class UIUtility
    {
        public static float FieldSlider (float value, float increment, Vector2 limits, string name, out bool changed, GUIStyle styleSlider, GUIStyle styleThumb, GUIStyle styleText, Color backgroundColor, int valueType)
        {
            float valueOld = value;
            value = GUILayout.HorizontalSlider (value, limits.x, limits.y, styleSlider, styleThumb);
            changed = false;

            if (valueOld != value)
            {
                value = Mathf.Clamp (value, limits.x, limits.y);
                if (valueOld != value)
                {
                    float excess = value % increment;
                    // Debug.Log ("S | Values old/new/excess: " + valueOld + " / " + value + " / " + excess);

                    if (value > valueOld)
                    {
                        if (excess > increment / 2f) value = Mathf.Min (value - excess + increment, limits.y);
                        else value = value - excess;
                    }

                    else if (value < valueOld)
                    {
                        if (excess < increment / 2f) value = Mathf.Max (limits.x, value - excess);
                        else value = value - excess + increment;
                    }

                    value = Mathf.Clamp (value, limits.x, limits.y);
                    if (valueOld != value) changed = true;
                }
            }

            Rect lastRect = GUILayoutUtility.GetLastRect ();
            Rect sliderBgrnd = new Rect (lastRect.xMin, lastRect.yMin - 1f, (lastRect.xMax - lastRect.xMin), lastRect.yMax - lastRect.yMin + 2f);
            Rect sliderValue = new Rect (lastRect.xMin, lastRect.yMin - 1f, (lastRect.xMax - lastRect.xMin) * ((value - limits.x) / (limits.y - limits.x)), lastRect.yMax - lastRect.yMin + 2f);
            // if (changed) Debug.Log ("S | Width: " + (lastRect.xMax - lastRect.xMin).ToString () + " | Multiplier: " + ((value - limits.x) / (limits.y - limits.x)).ToString ("F4") + " | Final: " + ((lastRect.xMax - lastRect.xMin) * ((value - limits.x) / (limits.y - limits.x))).ToString ());

            GUI.DrawTexture (lastRect, new Color (0.15f, 0.15f, 0.15f).GetTexture2D ());
            GUI.DrawTexture (sliderValue, backgroundColor.GetTexture2D ());

            Rect labelRectName = new Rect (lastRect.xMin, lastRect.yMin - 2f, lastRect.width, lastRect.height);
            GUI.Label (labelRectName, "  " + name, styleText);
            Rect labelRectValue = new Rect (labelRectName.xMin + labelRectName.width * 0.75f, labelRectName.yMin, labelRectName.width * 0.25f, labelRectName.height); 
            GUI.Label (labelRectValue, GetValueTranslation (value, valueType), styleText);
            return value;
        }

        public static Rect ClampToScreen (Rect window)
        {
            window.x = Mathf.Clamp (window.x, -window.width + 20, Screen.width - 20);
            window.y = Mathf.Clamp (window.y, -window.height + 20, Screen.height - 20);

            return window;
        }

        public static Rect SetToScreenCenter (this Rect r)
        {
            if (r.width > 0 && r.height > 0)
            {
                r.x = Screen.width / 2f - r.width / 2f;
                r.y = Screen.height / 2f - r.height / 2f;
            }
            return r;
        }

        public static double TextEntryForDouble (string label, int labelWidth, double prevValue)
        {
            string valString = prevValue.ToString ();
            UIUtility.TextEntryField (label, labelWidth, ref valString);

            if (!Regex.IsMatch (valString, @"^[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?$"))
                return prevValue;

            return double.Parse (valString);
        }

        public static void TextEntryField (string label, int labelWidth, ref string inputOutput)
        {
            GUILayout.BeginHorizontal ();
            GUILayout.Label (label, GUILayout.Width (labelWidth));
            inputOutput = GUILayout.TextField (inputOutput);
            GUILayout.EndHorizontal ();
        }


        private static Vector3 mousePos = Vector3.zero;

        public static Vector3 GetMousePos ()
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;
            return mousePos;
        }

        public static string GetValueTranslation (float value, int type)
        {
            if (type == 1)
            {
                if (value == 0f) return "Uniform";
                else if (value == 1f) return "Standard";
                else if (value == 2f) return "Reinforced";
                else if (value == 3f) return "LRSI";
                else if (value == 4f) return "HRSI";
                else return "Unknown material";
            }
            else if (type == 2)
            {
                if (value == 1f) return "No edge";
                else if (value == 2f) return "Rounded";
                else if (value == 3f) return "Biconvex";
                else if (value == 4f) return "Triangular";
                else return "Unknown";
            }
            else if (type == 3)
            {
                if (value == 1f) return "Rounded";
                else if (value == 2f) return "Biconvex";
                else if (value == 3f) return "Triangular";
                else return "Unknown";
            }
            else return value.ToString ("F3");
        }
    }
}
