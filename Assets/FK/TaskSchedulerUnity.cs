using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Panty
{
    public static class TaskSchedulerUnity
    {
        /// <summary>
        /// 标记为物体被销毁时停止任务
        /// </summary>
        public static void StopOnDestroy(this DelayTask task, Component c) =>
            c.GetOrAddComponent<TaskOnDestroyStopTrigger>().Add(task);
        /// <summary>
        /// 在脚本销毁时 停止当前脚本中的某一个任务序列
        /// </summary>
        public static TaskScheduler.Step StopOnDestroy<T>(this TaskScheduler.Step step, T view) where T : Component, IPermissionProvider
        {
            view.GetOrAddComponent<SequenceOnDestroyStopTrigger>().Add(step, view);
            return step;
        }
    }
    public class TaskOnDestroyStopTrigger : MonoBehaviour
    {
        private readonly Stack<DelayTask> arr = new Stack<DelayTask>();
        /// <summary>
        /// 注册延迟任务到移除栈 方便在脚本销毁时统一停止
        /// </summary>
        public void Add(DelayTask task) => arr.Push(task);
        private void OnDestroy()
        {
            while (arr.Count > 0) arr.Pop().Stop();
        }
    }
    public class SequenceOnDestroyStopTrigger : MonoBehaviour
    {
        private IPermissionProvider view;
        private readonly Stack<TaskScheduler.Step> arr = new Stack<TaskScheduler.Step>();
        /// <summary>
        /// 注册任务序列到移除栈 方便在脚本销毁时统一停止
        /// </summary>
        public void Add(TaskScheduler.Step step, IPermissionProvider view)
        {
            this.view = view;
            arr.Push(step);
        }
        private void OnDestroy()
        {
            var scheduler = view.Hub.Module<ITaskScheduler>();
            while (arr.Count > 0) scheduler.StopSequence(arr.Pop());
        }
    }
    public class Anim
    {
        public class Single
        {
            [Serializable]
            public class UI_Bezier2_TV_V2 : Bs.Bezier2_V2<Vector2, RectTransform>
            {
                public override Vector2 Cur { get => cur.anchoredPosition; set => cur.anchoredPosition = value; }
                protected override Vector2 GetTarget() => target;
#if UNITY_EDITOR
                protected override void SetTarget(Vector2 pos) => target = pos;
#endif
            }
            [Serializable]
            public class UI_Bezier3_TV_V2 : Bs.Bezier3_V2<Vector2, RectTransform>
            {
                public override Vector2 Cur { get => cur.anchoredPosition; set => cur.anchoredPosition = value; }
                protected override Vector2 GetTarget() => target;
#if UNITY_EDITOR
                protected override void SetTarget(Vector2 pos) => target = pos;
#endif
            }
            /// <summary>
            /// 基于 RectTransform 线性移动 2D
            /// </summary>

            [Serializable]
            public class UI_Linear_Move_TV_V2 : Bs.Linear_Move_V2<RectTransform, Vector2>
            {
                public override Vector2 Cur { get => cur.anchoredPosition; set => cur.anchoredPosition = value; }
                protected override Vector2 GetTarget() => target;
#if UNITY_EDITOR
                protected override void SetTarget(Vector2 pos) => target = pos;
#endif
            }
            /// <summary>
            /// 基于 RectTransform 线性移动 2D
            /// </summary>
            [Serializable]
            public class UI_Linear_Move_TT_V2 : Bs.Linear_Move_V2<RectTransform, RectTransform>
            {
                public override Vector2 Cur { get => cur.anchoredPosition; set => cur.anchoredPosition = value; }
                protected override Vector2 GetTarget() => target.anchoredPosition;
#if UNITY_EDITOR
                protected override void SetTarget(Vector2 pos) => target.anchoredPosition = pos;
#endif
            }
            /// <summary>
            /// 基于 RectTransform 线性缩放 2D
            /// </summary>
            [Serializable]
            public class UI_Linear_Scale_TV_V2 : Bs.Linear_V2<RectTransform, Vector2>
            {
                public override Vector2 Cur { get => cur.sizeDelta; set => cur.sizeDelta = value; }
                protected override Vector2 GetTarget() => target;
            }
            /// <summary>
            /// 基于 RectTransform 线性缩放 2D
            /// </summary>
            [Serializable]
            public class UI_Linear_Scale_TT_V2 : Bs.Linear_V2<RectTransform, RectTransform>
            {
                public override Vector2 Cur { get => cur.sizeDelta; set => cur.sizeDelta = value; }
                protected override Vector2 GetTarget() => target.sizeDelta;
            }
            /// <summary>
            /// 基于 Transform 线性旋转 2D
            /// </summary>
            [Serializable]
            public class Linear_Rot_TV_V3 : Bs.RotateEx
            {
#if UNITY_EDITOR
                [PropLabel("目标值")]
#endif
                public Quaternion target = Quaternion.identity;
                protected override void _Update(float _t)
                {
                    Cur = Quaternion.LerpUnclamped(start, target, _t);
                }
            }
            /// <summary>
            /// 基于 Transform 偏移旋转 2D
            /// </summary>
            [Serializable]
            public class Linear_Rot_Off_V2 : Bs.RotateEx
            {
#if UNITY_EDITOR
                [PropLabel("偏移值")]
#endif
                public float off;
                protected override void _Update(float _t) =>
                    Cur = start * Quaternion.AngleAxis(Mathf.LerpUnclamped(0f, off, _t), Vector3.forward);
            }
            [Serializable]
            public class Linear_Rot_Off_V3 : Bs.RotateEx
            {
#if UNITY_EDITOR
                [PropLabel("偏移值")]
#endif
                public Vector3 off;
                protected override void _Update(float _t) =>
                    Cur = start * Quaternion.Euler(Vector3.LerpUnclamped(Vector3.zero, off, _t));
            }
            /// <summary>
            /// 基于 Transform 线性移动 2D
            /// </summary>
            [Serializable]
            public class Linear_Move_TV_V2 : Bs.Linear_Move_V2<Vector2>
            {
                protected override Vector2 GetTarget() => target;
#if UNITY_EDITOR
                protected override void SetTarget(Vector2 pos) => target = pos;
#endif
            }
            /// <summary>
            /// 基于 Transform 线性移动 2D
            /// </summary>
            [Serializable]
            public class Linear_Move_TT_V2 : Bs.Linear_Move_V2<Transform>
            {
                protected override Vector2 GetTarget() =>
                    local ? target.localPosition : target.position;
#if UNITY_EDITOR
                protected override void SetTarget(Vector2 pos)
                {
                    if (local)
                        target.localPosition = pos;
                    else
                        target.position = pos;
                }
#endif
            }
            /// <summary>
            /// 基于 Transform 线性缩放 2D
            /// </summary>
            [Serializable]
            public class Linear_Scale_TV_V2 : Bs.Linear_Trans_V2<Vector2>
            {
                protected override Vector2 GetTarget() => target;
            }
            /// <summary>
            /// 基于 Transform 线性缩放 2D
            /// </summary>
            [Serializable]
            public class Linear_Scale_TT_V2 : Bs.Linear_Trans_V2<Transform>
            {
                protected override Vector2 GetTarget() =>
                    local ? target.localScale : target.lossyScale;
            }
            /// <summary>
            /// 基于 Transform 的 弹性位移动画 2D
            /// </summary>
            [Serializable]
            public class Elastic_Move_TV_V2 : Bs.ElasticV2<Transform>
            {
#if UNITY_EDITOR
                [PropLabel("本地变换")]
#endif
                [SerializeField] protected bool local = true;
                public override Vector2 Cur
                {
                    get => local ? cur.localPosition : cur.position;
                    set
                    {
                        if (local)
                            cur.localPosition = value;
                        else
                            cur.position = value;
                    }
                }
            }
            /// <summary>
            /// 基于 Transform 的 弹性缩放动画 2D
            /// </summary>
            [Serializable]
            public class Elastic_Scale_TV_V2 : Bs.ElasticV2<Transform>
            {
#if UNITY_EDITOR
                [PropLabel("本地变换")]
#endif
                [SerializeField] protected bool local = true;
                public override Vector2 Cur
                {
                    get => local ? cur.localScale : cur.lossyScale;
                    set => cur.localScale = value;
                }
            }
            /// <summary>
            /// 基于 Transform 的 弹性旋转动画 2D
            /// </summary>
            [Serializable]
            public class Elastic_Rot_TV_V2 : Bs.Elastic_Rot<float>, TaskScheduler.IInitOnlyAnim
            {
                private float off; // 旋转速度向量
                public bool IsExit() => Vel.Abs() <= 0.0001f; // 检查速度是否接近零
                public override void Reset()
                {
                    base.Reset();
                    off = 0f;
                }
                public void Update(float delta)
                {
                    Vel = (Vel - off * spring) * fric;
                    off += Vel * delta;
                    Cur = start * Quaternion.AngleAxis(off, Vector3.forward);
                }
            }
            /// <summary>
            /// 基于 Transform 的 弹性旋转动画 3D
            /// </summary>
            [Serializable]
            public class Elastic_Rot_TV_V3 : Bs.Elastic_Rot<Vector3>, TaskScheduler.IInitOnlyAnim
            {
                private Vector3 off; // 旋转速度向量
                public bool IsExit() => Vel.sqrMagnitude <= 0.0001f; // 检查速度是否接近零
                public override void Reset()
                {
                    base.Reset();
                    off = Vector3.zero;
                }
                public void Update(float delta)
                {
                    Vel = (Vel - off * spring) * fric;
                    off += Vel * delta;
                    Cur = start * Quaternion.Euler(off);
                }

            }
            /// <summary>
            /// 基于 RectTransform 的 弹性位移动画 2D
            /// </summary>
            [Serializable]
            public class UI_Elastic_Move_TV_V2 : Bs.ElasticV2<RectTransform>
            {
                public override Vector2 Cur { get => cur.anchoredPosition; set => cur.anchoredPosition = value; }
            }
            /// <summary>
            /// 基于 RectTransform 的 弹性缩放动画 2D
            /// </summary>
            [Serializable]
            public class UI_Elastic_Scale_TV_V2 : Bs.ElasticV2<RectTransform>
            {
                public override Vector2 Cur { get => cur.sizeDelta; set => cur.sizeDelta = value; }
            }
            [Serializable]
            public class UI_Linear_Color : Bs.LinearColor<Image>
            {
                public override Color Cur { get => cur.color; set => cur.color = value; }
            }
            [Serializable]
            public class Spr_Linear_Color : Bs.LinearColor<SpriteRenderer>
            {
                public override Color Cur { get => cur.color; set => cur.color = value; }
            }
            [Serializable]
            public class UI_Linear_Alpha : Bs.LinearAlpha<Image>
            {
                public override float Cur
                {
                    get => cur.color.a;
                    set
                    {
                        Color c = cur.color;
                        c.a = value;
                        cur.color = c;
                    }
                }
            }
            [Serializable]
            public class Spr_Linear_Alpha : Bs.LinearAlpha<SpriteRenderer>
            {
                public override float Cur
                {
                    get => cur.color.a;
                    set
                    {
                        Color c = cur.color;
                        c.a = value;
                        cur.color = c;
                    }
                }
            }
        }
        public class Bs
        {
            public abstract class Bezier2_V2<T, K> : Linear_Move_V2<K, T>
            {
#if UNITY_EDITOR
                [PropLabel("控制点A")]
#endif
                public Vector2 ctrl1;
                protected override void _Update(float _t)
                {
                    Cur = GetPos(GetTarget(), _t);
#if UNITY_EDITOR
                    CanDraw();
#endif
                }
                public virtual Vector2 GetPos(Vector2 end, float _t)
                {
                    return VecEx.Get2DBezier2(start, start + ctrl1, end, _t);
                }
#if UNITY_EDITOR
                protected override void Check(Vector2 std, Vector2 end, Vector2 size, Vector2 m)
                {
                    base.Check(std, end, size, m);
                    var c1 = ctrl1 + start;
                    if (typeof(K) == typeof(RectTransform))
                    {
                        c1.x += Screen.width >> 1;
                        c1.y += Screen.height >> 1;
                        c1 = Camera.main.ScreenToWorldPoint(c1);
                    }
                    if (new Rect(c1 - size * 0.5f, size).Contains(m)) code = 2;
                }
                protected override void WdCtrl(Vector2 m)
                {
                    switch (code)
                    {
                        case 0:
                            start = m;
                            break;
                        case 1:
                            SetTarget(m);
                            break;
                        case 2:
                            ctrl1 = m - start;
                            break;
                    }
                }
                protected override void DrawLine(Vector2 std, Vector2 end, Color c)
                {
                    var _ctrl = ctrl1 + start;
                    if (typeof(K) == typeof(RectTransform))
                    {
                        _ctrl.x += Screen.width >> 1;
                        _ctrl.y += Screen.height >> 1;
                        _ctrl = Camera.main.ScreenToWorldPoint(_ctrl);
                    }
                    Debug.DrawLine(std, _ctrl, c);
                    Debug.DrawLine(end, _ctrl, c);
                    HubTool.Box(_ctrl, Vector2.one * 0.5f, c);
                    float i = 0f;
                    while (i < 1f)
                    {
                        var a = VecEx.Get2DBezier2(std, _ctrl, end, i);
                        var b = VecEx.Get2DBezier2(std, _ctrl, end, i += 0.05f);
                        Debug.DrawLine(a, b, Color.white);
                    }
                }
#endif
            }
            public abstract class Bezier3_V2<T, K> : Bezier2_V2<T, K>
            {
#if UNITY_EDITOR
                [PropLabel("控制点B")]
#endif
                public Vector2 ctrl2;
                public override Vector2 GetPos(Vector2 end, float _t)
                {
                    return VecEx.Get2DBezier3(start, start + ctrl1, end + ctrl2, end, _t);
                }
#if UNITY_EDITOR
                protected override void Check(Vector2 std, Vector2 end, Vector2 size, Vector2 m)
                {
                    base.Check(std, end, size, m);
                    var c2 = ctrl2 + GetTarget();
                    if (typeof(K) == typeof(RectTransform))
                    {
                        c2.x += Screen.width >> 1;
                        c2.y += Screen.height >> 1;
                        c2 = Camera.main.ScreenToWorldPoint(c2);
                    }
                    if (new Rect(c2 - size * 0.5f, size).Contains(m)) code = 3;
                }
                protected override void WdCtrl(Vector2 m)
                {
                    base.WdCtrl(m);
                    if (code == 3) ctrl2 = m - GetTarget();
                }
                protected override void DrawLine(Vector2 std, Vector2 end, Color c)
                {
                    var _ctrl01 = ctrl1 + start;
                    var _ctrl02 = ctrl2 + GetTarget();
                    if (typeof(K) == typeof(RectTransform))
                    {
                        float w = Screen.width >> 1;
                        float h = Screen.height >> 1;
                        _ctrl01.x += w;
                        _ctrl01.y += h;
                        _ctrl02.x += w;
                        _ctrl02.y += h;
                        _ctrl01 = Camera.main.ScreenToWorldPoint(_ctrl01);
                        _ctrl02 = Camera.main.ScreenToWorldPoint(_ctrl02);
                    }
                    var size = Vector2.one * 0.5f;
                    Debug.DrawLine(std, _ctrl01, c);
                    Debug.DrawLine(end, _ctrl02, c);
                    HubTool.Box(_ctrl01, size, c);
                    HubTool.Box(_ctrl02, size, c);
                    float i = 0f;
                    while (i < 1f)
                    {
                        var a = VecEx.Get2DBezier3(std, _ctrl01, _ctrl02, end, i);
                        var b = VecEx.Get2DBezier3(std, _ctrl01, _ctrl02, end, i += 0.05f);
                        Debug.DrawLine(a, b, Color.white);
                    }
                }
#endif
            }
            public abstract class LinearColor<T> : Linear<Color, T>
            {
#if UNITY_EDITOR
                [PropLabel("目标颜色")]
#endif
                public Color target;
                protected override void _Update(float _t) => Cur = Color.LerpUnclamped(start, target, _t);
            }
            public abstract class LinearAlpha<T> : Linear<float, T>
            {
#if UNITY_EDITOR
                [PropLabel("目标透明度")]
#endif
                public float target;
                protected override void _Update(float _t) => Cur = Mathf.LerpUnclamped(start, target, _t);
            }
            public abstract class Linear_Move_V2<K> : Linear_Move_V2<Transform, K>
            {
#if UNITY_EDITOR
                [PropLabel("本地变换")]
#endif
                [SerializeField] protected bool local = true;
                public override Vector2 Cur
                {
                    get => local ? cur.localPosition : cur.position;
                    set
                    {
                        if (local)
                            cur.localPosition = value;
                        else
                            cur.position = value;
                    }
                }
            }
            public abstract class Linear_Move_V2<T, K> : Linear_V2<T, K>
            {
#if UNITY_EDITOR
                [SerializeField][PropLabel("绘制路径")] private bool isDraw = true;
                protected byte code = 255;
                private bool state = false;
                protected abstract void SetTarget(Vector2 pos);
                protected virtual void Check(Vector2 std, Vector2 end, Vector2 size, Vector2 m)
                {
                    var sub = size * 0.5f;
                    if (new Rect(std - sub, size).Contains(m)) code = 0;
                    else if (new Rect(end - sub, size).Contains(m)) code = 1;
                }
                protected virtual void WdCtrl(Vector2 m)
                {
                    switch (code)
                    {
                        case 0: start = m; break;
                        case 1: SetTarget(m); break;
                    }
                }
                protected virtual void DrawLine(Vector2 std, Vector2 end, Color c)
                {
                    Debug.DrawLine(std, end, c);
                }
                private void Draw()
                {
                    var sc = Vector2.one * 0.5f;
                    var std = Vector2.zero;
                    var end = Vector2.zero;
                    if (typeof(T) == typeof(RectTransform))
                    {
                        var half = new Vector2(Screen.width >> 1, Screen.height >> 1);
                        std = Camera.main.ScreenToWorldPoint(start + half);
                        end = Camera.main.ScreenToWorldPoint(GetTarget() + half);
                    }
                    else
                    {
                        std = start;
                        end = GetTarget();
                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        Vector2 m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                        Check(std, end, sc, m);
                    }
                    else if (Input.GetMouseButtonUp(0)) code = 255;
                    else if (code < 255)
                    {
                        if (typeof(T) == typeof(RectTransform))
                        {
                            WdCtrl((Vector2)Input.mousePosition - new Vector2(Screen.width >> 1, Screen.height >> 1));
                        }
                        else
                        {
                            WdCtrl(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                        }
                    }
                    DrawLine(std, end, Color.red);
                    HubTool.Box(std, sc, Color.green);
                    HubTool.Box(end, sc, Color.yellow);
                }
                protected void CanDraw()
                {
                    if (isDraw == state) return;
                    if (isDraw) MonoKit.OnUpdate += Draw;
                    else MonoKit.OnUpdate -= Draw;
                    state = isDraw;
                }
                protected override void _Update(float _t)
                {
                    base._Update(_t);
                    CanDraw();
                }
#endif
            }
            public abstract class Linear_Trans_V2<K> : Linear_V2<Transform, K>
            {
#if UNITY_EDITOR
                [PropLabel("本地变换")]
#endif
                [SerializeField] protected bool local = true;
                public override Vector2 Cur
                {
                    get => local ? cur.localScale : cur.lossyScale;
                    set => cur.localScale = value;
                }
            }
            public abstract class Linear_V2<T, K> : Linear<Vector2, T>
            {
#if UNITY_EDITOR
                [PropLabel("目标值")]
#endif
                public K target;
                protected abstract Vector2 GetTarget();
                protected override void _Update(float _t)
                {
                    Cur = Vector2.LerpUnclamped(start, GetTarget(), _t);
                }
            }
            public abstract class RotateEx : Linear<Quaternion, Transform>
            {
#if UNITY_EDITOR
                [PropLabel("本地变换")]
#endif
                [SerializeField] protected bool local;
                public override Quaternion Cur
                {
                    get => local ? cur.localRotation : cur.rotation;
                    set
                    {
                        if (local)
                            cur.localRotation = value;
                        else
                            cur.rotation = value;
                    }
                }
            }
            /// <summary>
            /// 线性运动基类
            /// </summary>
            /// <typeparam name="T">操作对象</typeparam>
            /// <typeparam name="K">操作类型</typeparam>
            public abstract class Linear<T, K> : TaskScheduler.IStateAnim
            {
#if UNITY_EDITOR
                [PropLabel("控制对象")]
#endif
                public K cur;
#if UNITY_EDITOR
                [PropLabel("起始值")]
#endif
                [SerializeField] protected T start;
#if UNITY_EDITOR
                [PropLabel("持续时间")]
#endif
                [SerializeField] private float duration = 1f;
#if UNITY_EDITOR
                [PropLabel("反转动画")]
#endif
                [SerializeField] private bool reverse;
#if UNITY_EDITOR
                [PropLabel("动画曲线")]
#endif
                [SerializeField] private AnimationCurve curve;
#if UNITY_EDITOR
                [PropLabel("预缓存")]
#endif
                [SerializeField] private bool isCache;

                private float t, time, mLeave;
                private float mLeaveTime = -1f;
                private bool exit;
#if UNITY_EDITOR
                private bool rev;
                private float dur;
#endif               
                public float Progress => t;
                public abstract T Cur { get; set; }
                protected abstract void _Update(float _t);
                public void Init()
                {
#if UNITY_EDITOR
                    dur = duration;
#endif
                    time = reverse ? duration : 0f;
                }
                public void SetLeaveTime(float time, bool percentageMode)
                {
                    if (time >= 0f)
                        mLeave = percentageMode ? time.Clamp_01().ToRange(1f, duration) : time;
                    else mLeave = reverse ? 0f : duration;
                    // 下面这个确保 mLeaveTime 只赋值一次
                    if (mLeaveTime < 0f) mLeaveTime = mLeave;
                    exit = false;
                }
                public void Reverse()
                {
                    reverse = !reverse;
                    _Reverse();
                    exit = false;
                    Init();
#if UNITY_EDITOR
                    rev = reverse;
#endif
                }
                private void _Reverse()
                {
                    if (reverse && mLeave == duration) mLeave = 0f;
                    else if (!reverse && mLeave == 0f) mLeave = duration;
                }
                public void TryCache()
                {
                    exit = false;
                    if (isCache) return;
                    isCache = true;
                    start = Cur;
                }
                public void Exit() => exit = true;
                public bool IsExit() => exit;
                public void Update(float delta)
                {
#if UNITY_EDITOR
                    if (rev != reverse)
                    {
                        _Reverse();
                        rev = reverse;
                    }
                    if (dur != duration)
                    {
                        _Reverse();
                        dur = duration;
                    }
#endif
                    time += reverse ? -delta : delta;
                    t = (time / duration).Clamp_01();
                    if (reverse ? time <= mLeave : time >= mLeave) exit = true;
                    _Update(curve.Evaluate(t));
                }
                public void Reset()
                {
                    exit = false;
                    if (mLeaveTime >= 0f)
                        mLeave = mLeaveTime;
                    if (!reverse) Cur = start;
                    Init();
                }
            }
            public abstract class Elastic<T, V, O>
            {
#if UNITY_EDITOR
                [PropLabel("目标值")]
#endif
                [SerializeField] protected T cur;
#if UNITY_EDITOR
                [PropLabel("弹性值")]
#endif
                [SerializeField] protected float spring = 0.97f;
#if UNITY_EDITOR
                [PropLabel("摩擦系数")]
#endif
                [SerializeField] protected float fric = 0.95f;
#if UNITY_EDITOR
                [PropLabel("最大速度")]
#endif
                [SerializeField] private V maxVel;

                protected O start;
                protected V Vel;

                public void TryCache() => start = Cur;
                public void Init() => Vel = maxVel;
                public abstract O Cur { get; set; }
                public virtual void Reset() => Vel = maxVel;
                public void Exit() => Vel = default;
            }
            public abstract class ElasticV2<T> : Elastic<T, Vector2, Vector2>, TaskScheduler.IInitOnlyAnim
            {
                public bool IsExit() => Vel.sqrMagnitude <= 0.0001f;
                public void Update(float delta)
                {
                    var pos = Cur;
                    // 当前速度 加上源点-当前位置 速度乘以摩擦系数
                    Vel = (Vel + (start - pos) * spring) * fric;
                    // 让当前坐标加上最终速度
                    Cur = pos + Vel * delta;
                }
            }
            public abstract class Elastic_Rot<T> : Elastic<Transform, T, Quaternion>
            {
#if UNITY_EDITOR
                [PropLabel("本地变换")]
#endif
                [SerializeField] private bool local = true;
                public override Quaternion Cur
                {
                    get => local ? cur.localRotation : cur.rotation;
                    set
                    {
                        if (local)
                            cur.localRotation = value;
                        else
                            cur.rotation = value;
                    }
                }
            }
        }
    }
    public partial class TaskScheduler
    {
        private class LoopAnim : ActionGrp, IAction
        {
            private int cur = 0;
            private bool pingpong, neg, isInit;
            private Func<bool> exitCondition;
            public LoopAnim(bool pingpong, Func<bool> exit)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(exit);
#endif
                this.exitCondition = exit;
                this.pingpong = pingpong;
            }
            public bool IsExit() => exitCondition();
            public override void Reset()
            {
                base.Reset();
                cur = 0;
            }
            public void Update(float delta)
            {
                var s = actions[cur];
                if (s.IsExit())
                {
                    if (pingpong)
                    {
                        if (isInit)
                        {
                            // 如果是负向运动
                            if (neg)
                            {
                                // 如果负向运动完成
                                if (cur == 0)
                                {
                                    neg = false;
                                    for (int i = 0; i < actions.Count; i++)
                                        (actions[i] as IStateAnim)?.Reverse();
                                }
                                else
                                {
                                    (actions[--cur] as ICache)?.TryCache();
                                }
                            }
                            else LoopPos();
                        }
                        else // 如果第一次进入 默认是正向 第一个动画完成后
                        {
                            LoopPos();
                            isInit = true;
                        }
                    }
                    else
                    {
                        if (++cur == actions.Count) Reset();
                        else (actions[cur] as ICache)?.TryCache();
                    }
                }
                else s.Update(delta);
            }
            private void LoopPos()
            {
                int len = actions.Count;
                // 如果只有一个动画 或者 有多个动画
                if (cur + 1 == len)
                {
                    neg = true;
                    for (int i = 0; i < len; i++)
                        (actions[i] as IStateAnim)?.Reverse();
                }
                else (actions[++cur] as ICache)?.TryCache();
            }
        }
        public interface IInitOnlyAnim : IAction, ICache
        {
            void Init();
        }
        public interface IStateAnim : IAction, ICache
        {
            void Init();
            void SetLeaveTime(float time, bool percentageMode);
            void Reverse();
        }
        public partial class Step
        {
            public ICache cache;
            public Step CustomAnim(IInitOnlyAnim act)
            {
                act.Init();
                cache = act;
                mScheduler.ToSequence(this, act);
                return this;
            }
            public Step CustomAnim(IStateAnim act, float leaveTime = -1F, bool percentageMode = true)
            {
                act.Init();
                act.SetLeaveTime(leaveTime, percentageMode);
                cache = act;
                mScheduler.ToSequence(this, act);
                return this;
            }
            public Step InitValuePreCache()
            {
                cache.TryCache();
                return this;
            }
            public Step ContinueAnim(IStateAnim act, float leaveTime = -1F, bool percentageMode = true)
            {
                mScheduler.ToSequence(this, new DoAction(() => act.SetLeaveTime(leaveTime, percentageMode)));
                mScheduler.ToSequence(this, act);
                return this;
            }
            public Step LoopAnim(bool pingpong, Func<bool> onExit, Action<Step> call = null)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(onExit);
#endif
                mScheduler.NextGroup(new LoopAnim(pingpong, onExit));
                if (call == null) return this;
                call.Invoke(this);
                return End();
            }
        }
    }
}