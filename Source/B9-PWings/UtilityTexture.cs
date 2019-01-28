﻿using System.Collections.Generic;
using UnityEngine;

namespace B9_Aerospace_ProceduralWings
{
    public static class UtilityTexture
    {
        private static Dictionary<int, Texture2D> textures = new Dictionary<int, Texture2D>();
        static public Texture2D GetTexture2D(this Color c)
        {
            int colorKey = c.ToInt();
            if (textures == null)
            {
                textures = new Dictionary<int, Texture2D>();
            }

            if (textures.ContainsKey(colorKey) && textures[colorKey] != null)
            {
                return textures[colorKey];
            }

			Texture2D tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tex.SetPixel(0, 0, c);
            tex.Apply();
            textures.Add(colorKey, tex);
            return tex;
        }

        public static int ToInt(this Color c)
        {
            Color32 c32 = c;
            return (c32.a << 24) | (c32.r << 16) | (c32.g << 8) | c32.b;
        }

        public static Color WithAlpha(this Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }
        
        public static Texture2D Load(string filename, int x, int y = 0, TextureFormat format = TextureFormat.RGBA32, bool mipmap = false)
		{
            y = y < 1 ? x : y;
            Texture2D tex = new Texture2D(x, y, format, mipmap);
            tex.LoadImage(KSPe.IO.File<WingProcedural>.Asset.ReadAllBytes(filename));
            return tex;
		}
	}
}