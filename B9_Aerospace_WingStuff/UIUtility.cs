﻿using KSP;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WingProcedural
{
    public static class UIUtility
    {
        public static float FieldSlider (float value, float increment, Vector2 limits, string name, string format, out bool changed, GUIStyle styleSlider, GUIStyle styleThumb, GUIStyle styleText, Color backgroundColor)
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
            GUI.Label (labelRectValue, value.ToString (format), styleText);
            return value;
        }
    }
}
