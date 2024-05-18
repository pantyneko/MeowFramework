using System;
using UnityEngine;

namespace Panty
{
    public static class MonoEx
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
        /// <summary>
        /// 找到面板父节点下所有对应控件
        /// </summary>
        /// <typeparam name="T">控件类型</typeparam>
        public static void FindChildrenControl<T>(this Component mono, Action<string, T> callback = null) where T : Component
        {
#if UNITY_EDITOR
            if (callback == null) throw new Exception("无效回调");
#endif
            // 得到所有子控件
            T[] controls = mono.GetComponentsInChildren<T>(true);
            // 如果没有找到组件 直接 return
            if (controls.Length == 0) return;
            // 遍历所有子控件
            foreach (T control in controls)
                callback.Invoke(control.gameObject.name, control);
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