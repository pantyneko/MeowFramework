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
        public static TaskScheduler.Step StopOnDestroy<T>(this TaskScheduler.Step step, T view) where T : Component, IPermissionProvider
        {
            view.GetOrAddComponent<SequenceOnDestroyStopTrigger>().Add(step, view);
            return step;
        }
    }
    public class TaskOnDestroyStopTrigger : MonoBehaviour
    {
        private readonly Stack<DelayTask> tasks = new Stack<DelayTask>();
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
    [Serializable]
    public class TransInfo2D
    {
        public Vector2 pos, scale;
        public Quaternion rot;
    }
    [Serializable]
    public class TransInfo3D
    {
        public Vector3 pos, scale;
        public Quaternion rot;
    }
    [Flags] public enum E_Trans : byte { T = 1, R = 2, S = 4, All = T | R | S }
    public enum E_Motion : byte { Linear, Lerp, Slerp }
    public partial interface ITaskScheduler
    {
        // 需要一个类型 去确定我需要哪些变换 
        TaskScheduler.IHandle Trans3(E_Trans trans, E_Motion motion, Transform cur, Transform target, float maxDistance, bool ignoreTimeScale = false);
        TaskScheduler.IHandle Trans3(E_Trans trans, E_Motion motion, Transform cur, TransInfo3D target, float maxDistance, bool ignoreTimeScale = false);
        TaskScheduler.IHandle UITrans2(E_Trans trans, E_Motion motion, RectTransform cur, RectTransform target, float maxDistance, bool ignoreTimeScale = false);
        TaskScheduler.IHandle UITrans2(E_Trans trans, E_Motion motion, RectTransform cur, TransInfo2D target, float maxDistance, bool ignoreTimeScale = false);
    }
    public partial class TaskScheduler
    {
        public interface IHandle
        {
            void Exit();
        }
        private abstract class TransAnimAction<T, K> : IHandle where T : Transform
        {
            protected E_Trans trans;
            protected E_Motion motion;
            protected T cur;
            protected K target;
            protected float t;
            protected bool isExit;

            public void Init(E_Trans trans, E_Motion motion, T cur, K target, float t)
            {
                this.trans = trans;
                this.motion = motion;
                this.cur = cur;
                this.target = target;
                this.t = t;
            }
            public void Exit() => isExit = true;
            public void Reset() => isExit = false;
            public bool IsExit() => isExit;
            protected Quaternion CurRot { get => cur.rotation; set => cur.rotation = value; }
        }
        private abstract class TransAnimAction3D<T> : TransAnimAction<Transform, T>
        {
            protected Vector3 CurPos { get => cur.position; set => cur.position = value; }
            protected Vector3 CurScale { get => cur.localScale; set => cur.localScale = value; }

            protected abstract Vector3 GetTargetPos();
            protected abstract Vector3 GetTargetScale();
            protected abstract Quaternion GetTargetRot();

            public void Update(float delta)
            {
                delta *= t;
                bool exit = true;

                if ((trans & E_Trans.T) == E_Trans.T)
                {
                    var targetPos = GetTargetPos();
                    if ((CurPos - targetPos).sqrMagnitude > delta * delta)
                    {
                        CurPos = motion switch
                        {
                            E_Motion.Linear => Vector3.MoveTowards(CurPos, targetPos, delta),
                            E_Motion.Lerp => Vector3.Lerp(CurPos, targetPos, delta),
                            E_Motion.Slerp => Vector3.Slerp(CurPos, targetPos, delta),
                            _ => CurPos
                        };
                        exit = false;
                    }
                    else CurPos = targetPos;
                }
                if ((trans & E_Trans.R) == E_Trans.R)
                {
                    var targetRot = GetTargetRot();
                    if (Quaternion.Angle(CurRot, targetRot) > delta)
                    {
                        CurRot = motion switch
                        {
                            E_Motion.Linear => Quaternion.RotateTowards(CurRot, targetRot, delta),
                            E_Motion.Lerp => Quaternion.Lerp(CurRot, targetRot, delta),
                            E_Motion.Slerp => Quaternion.Slerp(CurRot, targetRot, delta),
                            _ => CurRot
                        };
                        exit = false;
                    }
                    else CurRot = targetRot;
                }
                if ((trans & E_Trans.S) == E_Trans.S)
                {
                    var targetScale = GetTargetScale();
                    if ((CurScale - targetScale).sqrMagnitude > delta * delta)
                    {
                        CurScale = motion switch
                        {
                            E_Motion.Linear => Vector3.MoveTowards(CurScale, targetScale, delta),
                            E_Motion.Lerp => Vector3.Lerp(CurScale, targetScale, delta),
                            E_Motion.Slerp => Vector3.Slerp(CurScale, targetScale, delta),
                            _ => CurScale
                        };
                        exit = false;
                    }
                    else CurScale = targetScale;
                }
                if (exit) isExit = true;
            }
        }
        private abstract class UITransAnimAction2D<T> : TransAnimAction<RectTransform, T>
        {
            protected Vector2 CurPos { get => cur.anchoredPosition; set => cur.anchoredPosition = value; }
            protected Vector2 CurScale { get => cur.sizeDelta; set => cur.sizeDelta = value; }
            protected abstract Vector2 GetTargetPos();
            protected abstract Vector2 GetTargetScale();
            protected abstract Quaternion GetTargetRot();

            public void Update(float delta)
            {
                delta *= t;
                bool exit = true;

                if ((trans & E_Trans.T) == E_Trans.T)
                {
                    var targetPos = GetTargetPos();
                    if ((CurPos - targetPos).sqrMagnitude > delta * delta)
                    {
                        CurPos = motion switch
                        {
                            E_Motion.Linear => Vector3.MoveTowards(CurPos, targetPos, delta),
                            E_Motion.Lerp => Vector3.Lerp(CurPos, targetPos, delta),
                            E_Motion.Slerp => Vector3.Slerp(CurPos, targetPos, delta),
                            _ => CurPos
                        };
                        exit = false;
                    }
                    else CurPos = targetPos;
                }
                if ((trans & E_Trans.R) == E_Trans.R)
                {
                    var targetRot = GetTargetRot();
                    if (Quaternion.Angle(CurRot, targetRot) > delta)
                    {
                        CurRot = motion switch
                        {
                            E_Motion.Linear => Quaternion.RotateTowards(CurRot, targetRot, delta),
                            E_Motion.Lerp => Quaternion.Lerp(CurRot, targetRot, delta),
                            E_Motion.Slerp => Quaternion.Slerp(CurRot, targetRot, delta),
                            _ => CurRot
                        };
                        exit = false;
                    }
                    else CurRot = targetRot;
                }
                if ((trans & E_Trans.S) == E_Trans.S)
                {
                    var targetScale = GetTargetScale();
                    if ((CurScale - targetScale).sqrMagnitude > delta * delta)
                    {
                        CurScale = motion switch
                        {
                            E_Motion.Linear => Vector3.MoveTowards(CurScale, targetScale, delta),
                            E_Motion.Lerp => Vector3.Lerp(CurScale, targetScale, delta),
                            E_Motion.Slerp => Vector3.Slerp(CurScale, targetScale, delta),
                            _ => CurScale
                        };
                        exit = false;
                    }
                    else CurScale = targetScale;
                }
                if (exit) isExit = true;
            }
        }
        private class TransAnimAction3D : TransAnimAction3D<Transform>, IAction
        {
            protected override Vector3 GetTargetPos() => target.position;
            protected override Quaternion GetTargetRot() => target.rotation;
            protected override Vector3 GetTargetScale() => target.localScale;
        }
        private class TransVecAnimAction3D : TransAnimAction3D<TransInfo3D>, IAction
        {
            protected override Vector3 GetTargetPos() => target.pos;
            protected override Quaternion GetTargetRot() => target.rot;
            protected override Vector3 GetTargetScale() => target.scale;
        }
        private class UITransAnimAction2D : UITransAnimAction2D<RectTransform>, IAction
        {
            protected override Vector2 GetTargetPos() => target.anchoredPosition;
            protected override Quaternion GetTargetRot() => target.rotation;
            protected override Vector2 GetTargetScale() => target.sizeDelta;
        }
        private class UITransVecAnimAction2D : UITransAnimAction2D<TransInfo2D>, IAction
        {
            protected override Vector2 GetTargetPos() => target.pos;
            protected override Quaternion GetTargetRot() => target.rot;
            protected override Vector2 GetTargetScale() => target.scale;
        }
        IHandle ITaskScheduler.Trans3(E_Trans trans, E_Motion motion, Transform cur, Transform target, float maxDistance, bool ignoreTimeScale)
        {
            var act = new TransAnimAction3D();
            act.Init(trans, motion, cur, target, maxDistance);
            (ignoreTimeScale ? mUnscaledQueue : mActionQueue).Push(act);
            return act;
        }
        IHandle ITaskScheduler.Trans3(E_Trans trans, E_Motion motion, Transform cur, TransInfo3D target, float maxDistance, bool ignoreTimeScale)
        {
            var act = new TransVecAnimAction3D();
            act.Init(trans, motion, cur, target, maxDistance);
            (ignoreTimeScale ? mUnscaledQueue : mActionQueue).Push(act);
            return act;
        }
        IHandle ITaskScheduler.UITrans2(E_Trans trans, E_Motion motion, RectTransform cur, RectTransform target, float maxDistance, bool ignoreTimeScale)
        {
            var act = new UITransAnimAction2D();
            act.Init(trans, motion, cur, target, maxDistance);
            (ignoreTimeScale ? mUnscaledQueue : mActionQueue).Push(act);
            return act;
        }
        IHandle ITaskScheduler.UITrans2(E_Trans trans, E_Motion motion, RectTransform cur, TransInfo2D target, float maxDistance, bool ignoreTimeScale)
        {
            var act = new UITransVecAnimAction2D();
            act.Init(trans, motion, cur, target, maxDistance);
            (ignoreTimeScale ? mUnscaledQueue : mActionQueue).Push(act);
            return act;
        }
        public partial class Step
        {

        }
    }
}