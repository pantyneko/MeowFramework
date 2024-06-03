using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Panty
{
    public interface IUIManager : IModule
    {
        void ShowPanel<T>(UIPanel.Layer layer = UIPanel.Layer.Mid, Action<T> call = null) where T : UIPanel;
        void HidePanel<T>() where T : UIPanel;
        T GetPanel<T>() where T : UIPanel;
        RectTransform Canvas { get; }
        Vector2 Resolution { get; set; }
        Transform GetParent(UIPanel.Layer layer);
    }
    public interface IUIPathBuilder
    {
        (Type type, string path)[] Get();
    }
    public class UIManager<B> : AbsModule, IUIManager where B : IUIPathBuilder, new()
    {
        private Dictionary<Type, string> mPaths;
        private Dictionary<Type, UIPanel> mUIPool;
        private Transform mBot, mMid, mTop, mSystem;
        private RectTransform mCanvas;
        private CanvasScaler mScaler;
        private IResLoader mLoader;

        protected override void OnInit()
        {
            var list = new B().Get();
#if UNITY_EDITOR
            if (list == null) throw new Exception("路径未初始化");
#endif
            mPaths = new Dictionary<Type, string>();
            foreach (var v in list) mPaths.Add(v.type, v.path);

            mUIPool = new Dictionary<Type, UIPanel>();
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            var obj = new GameObject("Canvas", typeof(Canvas), typeof(GraphicRaycaster));

            GameObject.DontDestroyOnLoad(obj);

            obj.layer = LayerMask.NameToLayer("UI");
            mCanvas = obj.transform as RectTransform;

            obj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            mScaler = obj.AddComponent<CanvasScaler>();
            mScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            mScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            mScaler.referenceResolution = new Vector2(Screen.width, Screen.height);

            mBot = new GameObject("Bot").transform;
            mBot.SetParent(mCanvas);
            mBot.localPosition = Vector2.zero;

            mMid = new GameObject("Mid").transform;
            mMid.SetParent(mCanvas);
            mMid.localPosition = Vector2.zero;

            mTop = new GameObject("Top").transform;
            mTop.SetParent(mCanvas);
            mTop.localPosition = Vector2.zero;

            mSystem = new GameObject("System").transform;
            mSystem.SetParent(mCanvas);
            mSystem.localPosition = Vector2.zero;

            mLoader = this.Module<IResLoader>();
        }
        private void OnSceneUnloaded(Scene scene)
        {
            if (mUIPool.Count == 0) return;
            foreach (var item in mUIPool.Values)
                GameObject.Destroy(item.gameObject);
            mUIPool.Clear();
        }
        public Transform GetParent(UIPanel.Layer layer) => layer switch
        {
            UIPanel.Layer.Bot => mBot,
            UIPanel.Layer.Top => mTop,
            UIPanel.Layer.Mid => mMid,
            UIPanel.Layer.Sys => mSystem,
            _ => null
        };
        T IUIManager.GetPanel<T>() =>
            mUIPool.TryGetValue(typeof(T), out var ui) ? ui as T : null;
        Vector2 IUIManager.Resolution
        {
            get => mScaler.referenceResolution;
            set => mScaler.referenceResolution = value;
        }
        RectTransform IUIManager.Canvas => mCanvas;

        void IUIManager.ShowPanel<T>(UIPanel.Layer layer, Action<T> call)
        {
            var type = typeof(T);
            // 检查是否有合适的缓存
            if (mUIPool.TryGetValue(type, out var panel))
            {
                if (panel == null || panel.IsOpen) return;
                panel.Activate(true);
                panel.OnShow();
                call?.Invoke(panel as T);
                return;
            }
            if (mPaths.TryGetValue(type, out string shortPath))
            {
                mLoader.AsyncLoad<GameObject>("Panel/" + shortPath, o =>
                {
                    o = GameObject.Instantiate(o);
                    o.transform.SetParent(GetParent(layer));
                    o.transform.localPosition = Vector3.zero;
                    o.transform.localScale = Vector3.one;

                    T p = o.GetComponent<T>();
                    p.OnShow();
                    call?.Invoke(p);
                    mUIPool[type] = p;
                });
                mUIPool.Add(type, null);
            }
#if UNITY_EDITOR
            else throw new Exception($"{type}路径不存在");
#endif
        }
        void IUIManager.HidePanel<T>()
        {
            if (mUIPool.TryGetValue(typeof(T), out var panel))
            {
                panel.Activate(false);
                panel.OnHide();
            }
        }
    }
}