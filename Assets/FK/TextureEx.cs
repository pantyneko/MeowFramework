using UnityEngine;

namespace Panty
{
    public class TextureEx
    {
        /// <summary>
        /// 标准透明背景网格纹理，使用浅灰色和白色
        /// </summary>
        public static readonly Texture2D BaseTPG = GetCheckerboardTex(4, 4, ColorEx.LightGrey32, ColorEx.white32);
        /// <summary>
        /// 反转透明背景网格纹理，使用深色的主要和次要颜色
        /// </summary>
        public static readonly Texture2D InvTPG = GetCheckerboardTex(4, 4, ColorEx.PrimaryDarkColor, ColorEx.SecondaryDarkColor);
        /// <summary>
        /// 生成纯色纹理
        /// </summary>
        public static Texture2D GetSolidTex(int w, int h, Color32 col)
        {
            var result = new Texture2D(w, h);
            if (w == h && w == 1)
            {
                result.SetPixel(0, 0, col, 0);
            }
            else
            {
                int len = w * h;
                var pix = new Color32[len];
                for (int i = 0; i < len; i++) pix[i] = col;
                result.SetPixels32(pix, 0);
            }
            result.Apply();
            return result;
        }
        /// <summary>
        /// 生成棋盘格纹理
        /// </summary>
        public static Texture2D GetCheckerboardTex(int w, int h, Color c1, Color32 c2)
        {
            var pixs = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    pixs[y * w + x] = (((x + y) & 1) == 0) ? c1 : c2;
            var tex = new Texture2D(w, h);
            tex.filterMode = FilterMode.Point;
            tex.SetPixels32(pixs, 0);
            tex.Apply();
            return tex;
        }
    }
}