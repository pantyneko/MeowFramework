using UnityEngine;

namespace Panty
{
    public static class ColorEx
    {
        /// <summary>
        /// 黑灰色 (0.2f, 0.2f, 0.2f, 1f)
        /// </summary>
        public static Color BlackGray => new Color(0.2f, 0.2f, 0.2f, 1f);
        /// <summary>
        /// 纯白色 (255, 255, 255, 255)
        /// </summary>
        public static Color32 white32 => new Color32(255, 255, 255, 255);
        /// <summary>
        /// 浅灰色 (186, 186, 186, 255)
        /// </summary>
        public static Color32 LightGrey32 => new Color32(186, 186, 186, 255);
        /// <summary>
        /// 主要深色 (64, 64, 64, 255)
        /// </summary>
        public static Color32 PrimaryDarkColor => new Color32(64, 64, 64, 255);
        /// <summary>
        /// 次要深色 (32, 32, 32, 255)
        /// </summary>
        public static Color32 SecondaryDarkColor => new Color32(50, 50, 50, 255);
        /// <summary>
        /// 将RGBA整数值转换为Color32对象
        /// </summary>
        public static Color32 ToColor32(this int rgba)
        {
            byte r = (byte)((rgba >> 24) & 0xFF);
            byte g = (byte)((rgba >> 16) & 0xFF);
            byte b = (byte)((rgba >> 8) & 0xFF);
            byte a = (byte)(rgba & 0xFF);
            return new Color32(r, g, b, a);
        }
        /// <summary>
        /// 将Color32对象转换为RGBA整数值
        /// </summary>
        public static int ToInt(this Color32 color)
        {
            return (color.r << 24) | (color.g << 16) | (color.b << 8) | color.a;
        }
        /// <summary>
        /// 将RGBA整数值转换为Color对象
        /// </summary>
        public static Color ToColor(this int rgba)
        {
            float r = ((rgba >> 24) & 0xFF) / 255f;
            float g = ((rgba >> 16) & 0xFF) / 255f;
            float b = ((rgba >> 8) & 0xFF) / 255f;
            float a = (rgba & 0xFF) / 255f;
            return new Color(r, g, b, a);
        }
    }
}