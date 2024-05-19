using System;
using UnityEngine;

namespace Panty
{
    public static class Logger
    {
        public static T Error<T>(this T o)
        {
            Debug.unityLogger.Log(LogType.Error, o);
            return o;
        }
        public static T Warning<T>(this T o)
        {
            Debug.unityLogger.Log(LogType.Warning, o);
            return o;
        }
        public static void DrawBox(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color, float duration = 0)
        {
            Debug.DrawLine(a, b, color, duration);
            Debug.DrawLine(b, c, color, duration);
            Debug.DrawLine(c, d, color, duration);
            Debug.DrawLine(d, a, color, duration);
        }
        public static void DrawBox(Vector2 origin, Vector2 size, Color color, float duration = 0)
        {
            Vector2 _half = size * 0.5f;

            float x1 = origin.x - _half.x;
            float x2 = origin.x + _half.x;
            float y1 = origin.y - _half.y;
            float y2 = origin.y + _half.y;

            var a = new Vector2(x1, y2);
            var b = new Vector2(x2, y2);
            var c = new Vector2(x2, y1);
            var d = new Vector2(x1, y1);

            DrawBox(a, b, c, d, color, duration);
        }
    }
    public class MonoKit : MonoSingle<MonoKit>
    {
        public event Action OnUpdate;
        public event Action OnFixedUpdate;
        public event Action OnLateUpdate;
        public event Action OnGuiUpdate;
        public event Action OnDeInit;

        private void Update() => OnUpdate?.Invoke();
        private void FixedUpdate() => OnFixedUpdate?.Invoke();
        private void LateUpdate() => OnLateUpdate?.Invoke();
        private void OnGUI() => OnGuiUpdate?.Invoke();
        protected override void DeInit() => OnDeInit?.Invoke();
    }
}