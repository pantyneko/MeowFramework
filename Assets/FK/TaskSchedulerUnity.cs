using System;
using System.Collections.Generic;
using UnityEngine;

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
        private readonly Stack<DelayTask> tasks = new Stack<DelayTask>();
        /// <summary>
        /// 注册延迟任务到移除栈 方便在脚本销毁时统一停止
        /// </summary>
        public void Add(DelayTask task) => tasks.Push(task);
        private void OnDestroy()
        {
            while (tasks.Count > 0) tasks.Pop().Stop();
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
    public abstract partial class Anim
    {
        /// <summary>
        /// 用来封存动画抽象类 请不要主动使用
        /// </summary>
        public class No
        {
            public abstract class Trans<C, T> : TaskScheduler.IState
            {
                [SerializeField] protected C cur;
                [SerializeField] protected T target;
                [ReadOnly][SerializeField] protected float t;

                public float Progress => t;
                public void Exit() => t = 256f;
                public virtual void Reset() => t = 0f;
                public bool IsExit() => t >= 1f;
            }
            public abstract class Linear<T, K> : Trans<T, K>, TaskScheduler.IAction
            {
                [SerializeField] private float vel;
                protected Vector3 start;
                protected virtual float GetT() => t;
                protected abstract void SetCur(Vector3 pos);
                protected abstract Vector3 GetTarget();
                public void Update(float delta)
                {
                    t = (t + delta * vel).Clamp_01();
                    SetCur(Vector3.LerpUnclamped(start, GetTarget(), GetT()));
                }
            }
            public abstract class ElasticV2<T> : TaskScheduler.IAction, INeedInit
            {
                [SerializeField] protected T cur;
                [SerializeField] private float spring = 0.97f, fric = 0.95f;
                [SerializeField] private Vector2 maxVel = Vector2.one;
                [ReadOnly][SerializeField] private Vector2 Vel, org;

                public void Init()
                {
                    Vel = maxVel;
                    org = Cur;
                }
                public void Reset() => Vel = maxVel;
                public void Exit() => Vel = Vector2.zero;
                public bool IsExit() => Vel.sqrMagnitude <= 0.0001f;
                protected abstract Vector2 Cur { get; set; }
                public void Update(float delta)
                {
                    var pos = Cur;
                    // 当前速度 加上源点-当前位置 速度乘以摩擦系数
                    Vel = (Vel + (org - pos) * spring) * fric;
                    // 让当前坐标加上最终速度
                    Cur = pos + Vel * delta;
                }
            }
            public abstract class ElasticV3<T> : TaskScheduler.IAction, INeedInit
            {
                [SerializeField] protected T cur;
                [SerializeField] private float spring = 0.97f, fric = 0.95f;
                [SerializeField] private Vector3 maxVel = Vector3.one;
                [ReadOnly][SerializeField] private Vector3 Vel, org;

                public void Init()
                {
                    Vel = maxVel;
                    org = Cur;
                }
                public void Reset() => Vel = maxVel;
                public void Exit() => Vel = Vector3.zero;
                public bool IsExit() => Vel.sqrMagnitude <= 0.0001f;
                protected abstract Vector3 Cur { get; set; }
                public void Update(float delta)
                {
                    var pos = Cur;
                    // 当前速度 加上源点-当前位置 速度乘以摩擦系数
                    Vel = (Vel + (org - pos) * spring) * fric;
                    // 让当前坐标加上最终速度
                    Cur = pos + Vel * delta;
                }
            }
            public abstract class Bezier<T, K> : Trans<T, K>, TaskScheduler.IAction, INeedInit
            {
                [SerializeField] private float vel;
                [SerializeField] protected Vector3 c1;
                protected Vector3 start;
                public void Init() => start = Cur;
                protected virtual float GetT() => t;
                protected abstract Vector3 Cur { get; set; }
                protected abstract Vector3 GetTarget();
                protected abstract Vector3 GetPos(Vector3 end, float _t);
                public void Update(float delta)
                {
                    t = (t + vel * delta).Clamp_01();
                    Cur = GetPos(GetTarget(), GetT());
                }
                public override void Reset()
                {
                    base.Reset();
                    Cur = start;
                }
                public void Draw()
                {
                    var end = GetTarget();
                    float i = 0f;
                    while (i < 1f)
                    {
                        var a = GetPos(end, i);
                        var b = GetPos(end, i += 0.1f);
                        Gizmos.DrawLine(a, b);
                    }
                }
            }
            public abstract class Bezier2<T, K> : Bezier<T, K>
            {
                protected override Vector3 GetPos(Vector3 end, float _t) =>
                    VecEx.Get3DBezier2(start, c1, end, _t);
            }
            public abstract class Bezier3<T, K> : Bezier<T, K>
            {
                [SerializeField] protected Vector3 c2;
                protected override Vector3 GetPos(Vector3 end, float _t) =>
                    VecEx.Get3DBezier3(start, c1, c2, end, _t);
            }
        }
        /// <summary>
        /// 单次运动的动画
        /// </summary>
        public class Single
        {
            /// <summary>
            /// 基于 RectTransform 的 2次贝塞尔 线性移动 属于2D移动
            /// </summary>
            [Serializable]
            public class UI_Bezier2_Linear_Move_V2 : No.Bezier2<RectTransform, RectTransform>
            {
                protected override Vector3 Cur
                {
                    get => cur.anchoredPosition;
                    set => cur.anchoredPosition = value;
                }
                protected override Vector3 GetTarget() => target.anchoredPosition;
            }
            /// <summary>
            /// 基于 RectTransform 的 2次贝塞尔 根据动画曲线移动 属于2D移动
            /// </summary>
            [Serializable]
            public class UI_Bezier2_Curve_Move_V2 : UI_Bezier2_Linear_Move_V2
            {
                [SerializeField] private AnimationCurve curve;
                protected override float GetT() => curve.Evaluate(t);
            }
            /// <summary>
            /// 基于 RectTransform 的 2次贝塞尔 线性移动 属于3D移动
            /// </summary>
            [Serializable]
            public class UI_Bezier2_Linear_Move_V3 : No.Bezier2<RectTransform, RectTransform>
            {
                protected override Vector3 Cur
                {
                    get => cur.anchoredPosition3D;
                    set => cur.anchoredPosition3D = value;
                }
                protected override Vector3 GetTarget() => target.anchoredPosition3D;
            }
            /// <summary>
            /// 基于 RectTransform 的 2次贝塞尔 根据动画曲线移动 属于3D移动
            /// </summary>
            [Serializable]
            public class UI_Bezier2_Curve_Move_V3 : UI_Bezier2_Linear_Move_V3
            {
                [SerializeField] private AnimationCurve curve;
                protected override float GetT() => curve.Evaluate(t);
            }
            /// <summary>
            /// 基于 RectTransform 的 3次贝塞尔 线性移动 属于2D移动
            /// </summary>
            [Serializable]
            public class UI_Bezier3_Linear_Move_V2 : No.Bezier3<RectTransform, RectTransform>
            {
                protected override Vector3 Cur
                {
                    get => cur.anchoredPosition;
                    set => cur.anchoredPosition = value;
                }
                protected override Vector3 GetTarget() => target.anchoredPosition;
            }
            /// <summary>
            /// 基于 RectTransform 的 3次贝塞尔 根据动画曲线移动 属于2D移动
            /// </summary>
            [Serializable]
            public class UI_Bezier3_Curve_Move_V2 : UI_Bezier3_Linear_Move_V2
            {
                [SerializeField] private AnimationCurve curve;
                protected override float GetT() => curve.Evaluate(t);
            }
            /// <summary>
            /// 基于 RectTransform 的 3次贝塞尔 线性移动 属于3D移动
            /// </summary>
            [Serializable]
            public class UI_Bezier3_Linear_Move_V3 : No.Bezier3<RectTransform, RectTransform>
            {
                protected override Vector3 Cur
                {
                    get => cur.anchoredPosition3D;
                    set => cur.anchoredPosition3D = value;
                }
                protected override Vector3 GetTarget() => target.anchoredPosition3D;
            }
            /// <summary>
            /// 基于 RectTransform 的 3次贝塞尔 根据动画曲线移动 属于3D移动
            /// </summary>
            [Serializable]
            public class UI_Bezier3_Curve_Move_V3 : UI_Bezier3_Linear_Move_V3
            {
                [SerializeField] private AnimationCurve curve;
                protected override float GetT() => curve.Evaluate(t);
            }
            /// <summary>
            /// 基于 Transform 的 2次贝塞尔 线性移动 属于3D移动
            /// </summary>
            [Serializable]
            public class Bezier2_Linear_Move_V3 : No.Bezier2<Transform, Transform>
            {
                [SerializeField] private bool local;
                protected override Vector3 Cur
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
                protected override Vector3 GetTarget() =>
                    local ? target.localPosition : target.position;
            }
            /// <summary>
            /// 基于 Transform 的 2次贝塞尔 根据动画曲线移动 属于3D移动
            /// </summary>
            [Serializable]
            public class Bezier2_Curve_Move_V3 : Bezier2_Linear_Move_V3
            {
                [SerializeField] private AnimationCurve curve;
                protected override float GetT() => curve.Evaluate(t);
            }
            /// <summary>
            /// 基于 Transform 的 3次贝塞尔 线性移动 属于3D移动
            /// </summary>
            [Serializable]
            public class Bezier3_Linear_Move_V3 : No.Bezier3<Transform, Transform>
            {
                [SerializeField] private bool local;
                protected override Vector3 Cur
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
                protected override Vector3 GetTarget() =>
                    local ? target.localPosition : target.position;
            }
            /// <summary>
            /// 基于 Transform 的 3次贝塞尔 根据动画曲线移动 属于3D移动
            /// </summary>
            [Serializable]
            public class Bezier3_Curve_Move_V3 : Bezier3_Linear_Move_V3
            {
                [SerializeField] private AnimationCurve curve;
                protected override float GetT() => curve.Evaluate(t);
            }
            /// <summary>
            /// 基于 Transform 的 线性位移动画 3D
            /// </summary>
            [Serializable]
            public class Linear_Move_V3 : No.Linear<Transform, Transform>, INeedInit
            {
                [SerializeField] protected bool local;
                protected override Vector3 GetTarget() => local ? target.localPosition : target.position;
                public void Init() => start = local ? cur.localPosition : cur.position;
                protected override void SetCur(Vector3 v)
                {
                    if (local)
                        cur.localPosition = v;
                    else
                        cur.position = v;
                }
            }
            /// <summary>
            /// 基于 Transform 的 根据曲线做位移动画 3D
            /// </summary>
            [Serializable]
            public class Curve_Move_V3 : Linear_Move_V3
            {
                [SerializeField] private AnimationCurve curve;
                protected override float GetT() => curve.Evaluate(t);
            }
            /// <summary>
            /// 基于 Transform 的 线性缩放动画 3D
            /// </summary>
            [Serializable]
            public class Linear_Scale_V3 : No.Linear<Transform, Transform>, INeedInit
            {
                [SerializeField] protected bool local;
                protected override Vector3 GetTarget() => local ? target.localScale : target.lossyScale;
                public void Init() => start = local ? cur.localScale : cur.lossyScale;
                protected override void SetCur(Vector3 v) => cur.localScale = v;
            }
            /// <summary>
            /// 基于 Transform 的 根据曲线做缩放动画 3D
            /// </summary>
            [Serializable]
            public class Curve_Scale_V3 : Linear_Scale_V3
            {
                [SerializeField] private AnimationCurve curve;
                protected override float GetT() => curve.Evaluate(t);
            }
            /// <summary>
            /// 基于 RectTransform 的 线性位移动画 2D
            /// </summary>
            [Serializable]
            public class UI_Linear_Move_V2 : No.Linear<RectTransform, RectTransform>, INeedInit
            {
                public void Init() => start = cur.anchoredPosition;
                protected override void SetCur(Vector3 pos) => cur.anchoredPosition = pos;
                protected override Vector3 GetTarget() => target.anchoredPosition;
            }
            /// <summary>
            /// 基于 RectTransform 的 根据曲线做位移动画 2D
            /// </summary>
            [Serializable]
            public class UI_Curve_Move_V2 : UI_Linear_Move_V2
            {
                [SerializeField] private AnimationCurve curve;
                protected override float GetT() => curve.Evaluate(t);
            }
            /// <summary>
            /// 基于 RectTransform 的 线性缩放动画 2D
            /// </summary>
            [Serializable]
            public class UI_Linear_Scale_V2 : No.Linear<RectTransform, RectTransform>, INeedInit
            {
                public void Init() => start = cur.sizeDelta;
                protected override void SetCur(Vector3 v) => cur.sizeDelta = v;
                protected override Vector3 GetTarget() => target.sizeDelta;
            }
            /// <summary>
            /// 基于 RectTransform 的 根据曲线做缩放动画 2D
            /// </summary>
            [Serializable]
            public class UI_Curve_Scale_V2 : UI_Linear_Scale_V2
            {
                [SerializeField] private AnimationCurve curve;
                protected override float GetT() => curve.Evaluate(t);
            }
            /// <summary>
            /// 基于 RectTransform 的 线性位移动画 3D
            /// </summary>
            [Serializable]
            public class UI_Linear_Move_V3 : No.Linear<RectTransform, RectTransform>, INeedInit
            {
                public void Init() => start = cur.anchoredPosition3D;
                protected override void SetCur(Vector3 pos) => cur.anchoredPosition3D = pos;
                protected override Vector3 GetTarget() => target.anchoredPosition3D;
            }
            /// <summary>
            /// 基于 RectTransform 的 根据曲线做位移动画 3D
            /// </summary>
            [Serializable]
            public class UI_Curve_Move_V3 : UI_Linear_Move_V3
            {
                [SerializeField] private AnimationCurve curve;
                protected override float GetT() => curve.Evaluate(t);
            }
        }
        /// <summary>
        /// 连续动画 使用 CustomAnimNoState 调用
        /// </summary>
        public class Continuity
        {
            /// <summary>
            /// 基于 Transform 的 持续旋转 2D
            /// </summary>
            [Serializable]
            public class Rotate_V2 : TaskScheduler.IAction
            {
                [SerializeField] private Transform tr;
                [SerializeField] private Space space = Space.Self;
                [SerializeField] protected float zVel;
                [ReadOnly][SerializeField] private bool isExit;

                public void SetRot(Quaternion q)
                {
                    if (space == Space.World)
                        tr.rotation = q;
                    else
                        tr.localRotation = q;
                }
                public void Reset() => isExit = false;
                public void Exit() => isExit = true;
                public bool IsExit() => isExit;
                public void Update(float delta) => tr.Rotate(GetEulers(delta), space);
                protected virtual Vector3 GetEulers(float delta) => new Vector3(0f, 0f, zVel * delta);
            }
            /// <summary>
            /// 基于 Transform 的 持续旋转 3D
            /// </summary>
            [Serializable]
            public class Rotate_V3 : Rotate_V2
            {
                [SerializeField] private float xVel, yVel;
                protected override Vector3 GetEulers(float delta) => new Vector3(xVel, yVel, zVel) * delta;
            }
        }
        /// <summary>
        /// 弹性动画 使用 CustomAnimNoState 调用
        /// </summary>
        public class Elastic
        {
            /// <summary>
            /// 基于 Transform 的 弹性旋转动画 2D
            /// </summary>
            [Serializable]
            public class Rot_V2 : TaskScheduler.IAction, INeedInit
            {
                [SerializeField] private Transform cur;
                [SerializeField] private bool local;
                [SerializeField] private float spring = 0.97f; // 弹性系数
                [SerializeField] private float fric = 0.95f; // 摩擦力系数
                [SerializeField] private float maxVel = 0f;
                [ReadOnly][SerializeField] private float Vel, off; // 旋转速度向量
                [ReadOnly][SerializeField] private Quaternion org; // 原始旋转四元数

                public void Init()
                {
                    Vel = maxVel; // 初始化速度
                    org = Cur; // 记录原始旋转
                }
                public void Reset()
                {
                    Vel = maxVel; // 重置速度
                    off = 0f;
                }
                public void Exit() => Vel = 0f; // 停止时将速度设置为零
                public bool IsExit() => Vel.Abs() <= 0.0001f; // 检查速度是否接近零

                private Quaternion Cur
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
                public void Update(float delta)
                {
                    Vel = (Vel - off * spring) * fric;
                    off += Vel * delta;
                    Cur = org * Quaternion.AngleAxis(off, Vector3.forward);
                }
            }
            /// <summary>
            /// 基于 Transform 的 弹性旋转动画 3D
            /// </summary>
            [Serializable]
            public class Rot_V3 : TaskScheduler.IAction, INeedInit
            {
                [SerializeField] private Transform cur;
                [SerializeField] private bool local;
                [SerializeField] private float spring = 0.97f; // 弹性系数
                [SerializeField] private float fric = 0.95f; // 摩擦力系数
                [SerializeField] private Vector3 maxVel = Vector3.one;
                [ReadOnly][SerializeField] private Vector3 Vel, off; // 旋转速度向量
                [ReadOnly][SerializeField] private Quaternion org; // 原始旋转四元数

                public void Init()
                {
                    Vel = maxVel; // 初始化速度
                    org = Cur; // 记录原始旋转
                }
                public void Reset()
                {
                    Vel = maxVel; // 重置速度
                    off = Vector3.zero;
                }
                public void Exit() => Vel = Vector3.zero; // 停止时将速度设置为零
                public bool IsExit() => Vel.sqrMagnitude <= 0.0001f; // 检查速度是否接近零

                private Quaternion Cur
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
                public void Update(float delta)
                {
                    Vel = (Vel - off * spring) * fric;
                    off += Vel * delta;
                    Cur = org * Quaternion.Euler(off);
                }
            }
            /// <summary>
            /// 基于 Transform 的 弹性位移动画 2D
            /// </summary>
            [Serializable]
            public class Move_V2 : No.ElasticV2<Transform>
            {
                [SerializeField] private bool local;
                protected override Vector2 Cur
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
            /// 基于 Transform 的 弹性位移动画 3D
            /// </summary>
            [Serializable]
            public class Move_V3 : No.ElasticV3<Transform>
            {
                [SerializeField] private bool local;
                protected override Vector3 Cur
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
            public class Scale_V2 : No.ElasticV2<Transform>
            {
                [SerializeField] private bool local;
                protected override Vector2 Cur
                {
                    get => local ? cur.localScale : cur.lossyScale;
                    set => cur.localScale = value;
                }
            }
            /// <summary>
            /// 基于 Transform 的 弹性缩放动画 3D
            /// </summary>
            [Serializable]
            public class Scale_V3 : No.ElasticV3<Transform>
            {
                [SerializeField] private bool local;
                protected override Vector3 Cur
                {
                    get => local ? cur.localScale : cur.lossyScale;
                    set => cur.localScale = value;
                }
            }
            /// <summary>
            /// 基于 RectTransform 的 弹性缩放动画 2D
            /// </summary>
            [Serializable]
            public class UI_Scale_V2 : No.ElasticV2<RectTransform>
            {
                protected override Vector2 Cur { get => cur.sizeDelta; set => cur.sizeDelta = value; }
            }
            /// <summary>
            /// 基于 RectTransform 的 弹性位移动画 2D
            /// </summary>
            [Serializable]
            public class UI_Move_V2 : No.ElasticV2<RectTransform>
            {
                protected override Vector2 Cur { get => cur.anchoredPosition; set => cur.anchoredPosition = value; }
            }
            /// <summary>
            /// 基于 RectTransform 的 弹性位移动画 3D
            /// </summary>
            [Serializable]
            public class UI_Move_V3 : No.ElasticV3<RectTransform>
            {
                protected override Vector3 Cur { get => cur.anchoredPosition3D; set => cur.anchoredPosition3D = value; }
            }
        }
    }
    public partial interface ITaskScheduler
    {

    }
    public partial class TaskScheduler
    {
        public interface IState
        {
            float Progress { get; }
        }
        private class ProgressGreaterAction : IAction
        {
            private bool exit;
            private float target;
            private IState state;
            private Step step;
            private Action<Step> call;
            public ProgressGreaterAction(Step step, IState state)
            {
                this.step = step;
                this.state = state;
            }
            public IAction Init(float target, Action<Step> call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                this.target = target;
                this.call = call;
                return this;
            }
            public void Exit() => exit = true;
            public bool IsExit() => exit;
            public virtual void Update(float delta)
            {
                if (state.Progress > target)
                {
                    call.Invoke(step);
                    exit = true;
                }
            }
            public void Reset() => exit = false;
        }
        public partial class Step
        {
            private static IState state;

            public Step CustomAnim<T>(T act) where T : IAction, INeedInit, IState
            {
                act.Init();
                mScheduler.ToSequence(this, act);
                state = act;
                return this;
            }
            public Step CustomAnimNoState<T>(T act) where T : IAction, INeedInit
            {
                act.Init();
                mScheduler.ToSequence(this, act);
                return this;
            }
            private ProgressGreaterAction GreaterAction() =>
                new ProgressGreaterAction(Clone(), state);
            /// <summary>
            /// 进度的大于事件 检测到大于就执行一次事件
            /// </summary>
            public Step OnGreater(float target, Action<Step> onFinished)
            {
                mScheduler.ToSequence(this, GreaterAction().Init(target, onFinished));
                return this;
            }
            /// <summary>
            /// 进度完成 执行一次事件
            /// </summary>
            public Step OnFinish(Action<Step> onFinished)
            {
                mScheduler.ToSequence(this, GreaterAction().Init(0.99999994f, onFinished));
                return this;
            }
        }
    }
}