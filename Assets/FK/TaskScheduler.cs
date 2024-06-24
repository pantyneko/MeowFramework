using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Panty
{
    public interface ITaskScheduler : IModule
    {
        void AddConditionalTask(Func<bool> exitCondition, Action onFinished);
        DelayTask AddDelayTask(float duration, Action onFinished, bool isLoop = false, bool isUnScaled = false);
        DelayTask AddTemporaryTask(float duration, Action onUpdate, bool isUnScaled = false);
        TaskScheduler.Step Sequence(bool ignoreTimeScale = false);
        void StopSequence(TaskScheduler.Step step);
    }
    public class TaskScheduler : AbsModule, ITaskScheduler
    {
        public interface IAction
        {
            bool IsExit();
            void Reset();
            void Update(float delta);
        }
        private class Sequence
        {
            private bool mExit;
            public bool IsExit() => mExit;
            public void Exit() => mExit = true;
            private Queue<IAction> actions = new Queue<IAction>();
            public void Enqueue(IAction e) => actions.Enqueue(e);
            public void Update(float delta)
            {
                var a = actions.Peek();
                if (a.IsExit())
                {
                    actions.Dequeue();
                    if (actions.Count == 0)
                        mExit = true;
                }
                else a.Update(delta);
            }
        }
        // 单纯等待条件触发
        private class WaitAction : IAction
        {
            private Func<bool> exitCondition;
            public WaitAction(Func<bool> exit) => exitCondition = exit;
            public bool IsExit() => exitCondition();
            public void Reset() { }
            public void Update(float delta) { }
        }
        // 单纯延迟N秒触发
        private class DelayAction : IAction
        {
            private float duration, cur;
            public DelayAction(float duration) => this.duration = duration;
            public bool IsExit() => cur >= duration;
            public void Reset() => cur = 0;
            public void Update(float delta) => cur += delta;
        }
        // 指定次数内重复执行的任务
        private class RepeatAction : IAction
        {
            private Action call;
            private byte repeatCount, currentCount;
            public RepeatAction(byte repeatCount, Action call)
            {
                this.repeatCount = repeatCount;
                this.call = call;
            }
            public bool IsExit() => currentCount >= repeatCount;
            public void Reset() => currentCount = 0;
            public void Update(float delta)
            {
                call.Invoke();
                currentCount++;
            }
        }
        // 在条件内重复执行
        private class UntilConditionAction : IAction
        {
            private Action call;
            private Func<bool> exitCondition;
            public UntilConditionAction(Func<bool> exitCondition, Action call)
            {
                this.exitCondition = exitCondition;
                this.call = call;
            }
            public bool IsExit() => exitCondition();
            public void Reset() { }
            public void Update(float delta) => call.Invoke();
        }
        // 周期内重复执行任务
        private class PeriodicAction : IAction
        {
            private Action call;
            private float duration, cur;
            public PeriodicAction(Action call, float duration)
            {
                this.call = call;
                this.duration = duration;
            }
            public bool IsExit() => cur >= duration;
            public void Reset() => cur = 0;
            public void Update(float delta)
            {
                cur += delta;
                call?.Invoke();
            }
        }
        // 直接执行一个逻辑
        private class DoAction : IAction
        {
            private Action call;
            public DoAction(Action call) => this.call = call;
            public bool IsExit()
            {
                call.Invoke();
                return true;
            }
            public void Reset() { }
            public void Update(float delta) { }
        }
        // 随机执行组 每次从组中随机一个方法执行
        private class RandomGroup : IAction
        {
            private readonly PArray<IAction> actions;
            private IAction select;
            public RandomGroup(PArray<IAction> actions)
            {
                this.actions = actions;
                select = actions.RandomGet();
            }
            public bool IsExit() => select.IsExit();
            public void Update(float delta) => select.Update(delta);
            public void Reset()
            {
                select = actions.RandomGet();
                select.Reset();
            }
        }
        // 重复组 将组内的逻辑 重复执行N次
        private class RepeatGroup : IAction
        {
            private readonly PArray<IAction> actions;
            private byte repeatCount, current;
            private int cur;
            public RepeatGroup(PArray<IAction> actions, byte repeatCount)
            {
                this.repeatCount = repeatCount;
                this.actions = actions;
            }
            public bool IsExit() => current == repeatCount;
            public void ResetAll() { foreach (var s in actions) s.Reset(); }
            public void Reset()
            {
                cur = 0;
                current = 0;
                ResetAll();
            }
            public void Update(float delta)
            {
                var sq = actions[current];
                if (sq.IsExit())
                {
                    actions.LoopPos(ref cur);
                    if (cur == 0)
                    {
                        current++;
                        ResetAll();
                    }
                }
                else sq.Update(delta);
            }
        }
        // 并行组 将组内的逻辑同步执行 直到都完成
        private class ParallelGroup : IAction
        {
            private readonly PArray<IAction> actions = new PArray<IAction>();
            public ParallelGroup(PArray<IAction> actions) => this.actions = actions;
            public bool IsExit() => actions.All(action => action.IsExit());
            public void Reset() { foreach (var s in actions) s.Reset(); }
            public void Update(float delta) { foreach (var s in actions) s.Update(delta); }
        }
        // 循环组 在条件未满足的时候 顺序执行组内动作
        private class LoopGroup : IAction
        {
            private int cur = 0;
            private readonly PArray<IAction> actions;
            private Func<bool> exitCondition;
            public LoopGroup(PArray<IAction> actions, Func<bool> exitCondition)
            {
                this.actions = actions;
                this.exitCondition = exitCondition;
            }
            public bool IsExit() => exitCondition();
            public void Reset() { foreach (var s in actions) s.Reset(); }
            public void Update(float delta)
            {
                var s = actions[cur];
                if (s.IsExit())
                {
                    s.Reset();
                    actions.LoopPos(ref cur);
                }
                else s.Update(delta);
            }
        }
        private enum E_Type : byte
        {
            Loop, Parallel, Repeat, Random
        }
        private class Group
        {
            private Group father;
            private E_Type type;
            public Group Father => father;
            public bool IsRoot => father == null;
            public Group(E_Type type, Group father)
            {
                this.type = type;
                this.father = father;
            }
            private PArray<IAction> cache = new PArray<IAction>();
            public void Push(IAction action) => cache.Push(action);
            public IAction GetAction() => type switch
            {
                E_Type.Repeat => new RepeatGroup(cache, mCounter),
                E_Type.Loop => new LoopGroup(cache, mOnExit),
                E_Type.Parallel => new ParallelGroup(cache),
                E_Type.Random => new RandomGroup(cache),
                _ => throw new Exception("未知动作"),
            };
        }
        public class Step
        {
            private static TaskScheduler mScheduler;
            public Action evt;
            public bool ignoreTimeScale;
            public Step(TaskScheduler scheduler, bool ignoreTimeScale)
            {
                mScheduler = scheduler;
                this.ignoreTimeScale = ignoreTimeScale;
            }
            /// <summary>
            /// 缓存一个任务 在下一次调用前 贯穿当前序列使用
            /// </summary>
            public Step Cache(Action evt)
            { this.evt = evt; return this; }
            /// <summary>
            /// 插入一个等待事件 等待不需要执行内容
            /// </summary>
            public Step Wait(Func<bool> onExit)
            {
#if UNITY_EDITOR
                if (onExit == null) throw new Exception("onExit不能为null");
#endif
                mScheduler.ToSequence(this, new WaitAction(onExit));
                return this;
            }
            /// <summary>
            /// 插入一个延迟事件 
            /// </summary>
            public Step Delay(float duration)
            {
                if (duration < 0f) duration = 0f;
                mScheduler.ToSequence(this, new DelayAction(duration));
                return this;
            }
            /// <summary>
            /// 插入一个事件
            /// </summary>
            public Step Event(Action call)
            {
                mScheduler.ToSequence(this, new DoAction(call));
                return this;
            }
            /// <summary>
            /// 插入一个可执行事件 使用全局事件
            /// </summary>
            public Step Event() => Event(evt);
            /// <summary>
            /// 插入一个重复次数事件
            /// </summary>
            public Step Repeat(byte repeatCount, Action call)
            {
                mScheduler.ToSequence(this, new RepeatAction(repeatCount, call));
                return this;
            }
            /// <summary>
            /// 插入一个重复次数事件 使用全局事件
            /// </summary>
            public Step Repeat(byte repeatCount) => Repeat(repeatCount, evt);
            /// <summary>
            /// 插入一个在条件未成立时重复执行的事件
            /// </summary>
            public Step Until(Func<bool> exit, Action call)
            {
                mScheduler.ToSequence(this, new UntilConditionAction(exit, call));
                return this;
            }
            /// <summary>
            /// 插入一个在条件未成立时重复执行的事件 使用全局的事件
            /// </summary>
            public Step Until(Func<bool> exit) => Until(exit, evt);
            /// <summary>
            /// 插入一个周期内重复执行任务
            /// </summary>
            public Step Periodic(float duration, Action call)
            {
                mScheduler.ToSequence(this, new PeriodicAction(call, duration));
                return this;
            }
            /// <summary>
            /// 插入一个周期内重复执行任务 使用全局事件
            /// </summary>
            public Step Periodic(float duration) => Periodic(duration, evt);
            /// <summary>
            /// 开始处理随机组 在End调用前 将会以缓存来处理每次的任务
            /// </summary>
            public Step RandomGroup()
            {
                mScheduler.NextGroup(E_Type.Random);
                return this;
            }
            /// <summary>
            /// 启用连续循环模式 在End调用前 将会以缓存来处理每次的任务
            /// 注意 ：onExit不能为null
            /// </summary>
            public Step LoopGroup(Func<bool> onExit)
            {
#if UNITY_EDITOR
                if (onExit == null) throw new Exception("onExit不能为null");
#endif
                mOnExit = onExit;
                mScheduler.NextGroup(E_Type.Loop);
                return this;
            }
            /// <summary>
            /// 启用次数循环模式 在End调用前 将会以缓存来处理每次的任务
            /// </summary>
            /// <param name="repeatCount">循环次数，必须大于 0</param>
            public Step RepeatGroup(byte repeatCount)
            {
                mCounter = repeatCount == 0 ? (byte)1 : repeatCount;
                mScheduler.NextGroup(E_Type.Repeat);
                return this;
            }
            /// <summary>
            /// 启用并行组模式 在End调用前 将会以缓存来处理每次的任务
            /// </summary>
            public Step ParallelGroup()
            {
                mScheduler.NextGroup(E_Type.Parallel);
                return this;
            }
            /// <summary>
            /// 用于封闭动作组
            /// </summary>
            public Step End()
            {
                mScheduler.EndGroup(this);
                return this;
            }
        }
        private static byte mCounter;
        private static Func<bool> mOnExit;

        private Group stepGroup = null;
        private PArray<Step> mRmvStep;
        private PArray<DelayTask> mAvailable, mDelayTasks, mUnScaledTasks;
        private PArray<(Func<bool> isEnd, Action call)> mConditionalTasks;
        private Dictionary<Step, Sequence> mSequenceGroup;
        private Dictionary<Step, Sequence> mUnscaledSequence;

        protected override void OnInit()
        {
            mConditionalTasks = new PArray<(Func<bool>, Action)>();
            mSequenceGroup = new Dictionary<Step, Sequence>();
            mUnscaledSequence = new Dictionary<Step, Sequence>();
            mAvailable = new PArray<DelayTask>();
            mDelayTasks = new PArray<DelayTask>();
            mUnScaledTasks = new PArray<DelayTask>();

            mRmvStep = new PArray<Step>();
            MonoKit.OnUpdate += Update;
        }
        void ITaskScheduler.StopSequence(Step step)
        {
            GetSequence(step).Exit();
        }
        public DelayTask AddDelayTask(float duration, Action onFinished, bool isLoop, bool isUnScaled)
        {
            var task = GetTask().Init(duration, isLoop);
            task.SetTask(onFinished).Start();

            if (isUnScaled)
                mUnScaledTasks.Push(task);
            else
                mDelayTasks.Push(task);
            return task;
        }
        void ITaskScheduler.AddConditionalTask(Func<bool> exitCondition, Action onFinished)
        {
            mConditionalTasks.Push((exitCondition, onFinished));
        }
        DelayTask ITaskScheduler.AddTemporaryTask(float duration, Action onUpdate, bool isUnScaled)
        {
#if DEBUG
            if (onUpdate == null) throw new ArgumentNullException("onUpdate is Empty");
#endif
            MonoKit.OnUpdate += onUpdate;
            return AddDelayTask(duration, () => MonoKit.OnUpdate -= onUpdate, false, isUnScaled);
        }
        Step ITaskScheduler.Sequence(bool ignoreTimeScale)
        {
#if UNITY_EDITOR
            if (stepGroup != null)
                throw new Exception($"存在未封闭序列{stepGroup}");
#endif
            return new Step(this, ignoreTimeScale);
        }
        private DelayTask GetTask() => mAvailable.IsEmpty ? new DelayTask() : mAvailable.Pop();
        private void NextGroup(E_Type type)
        {
            if (stepGroup == null)
            {
                stepGroup = new Group(type, null);
            }
            else
            {
                var cur = stepGroup;
                stepGroup = new Group(type, cur);
                cur.Push(stepGroup.GetAction());
            }
        }
        private void EndGroup(Step step)
        {
#if UNITY_EDITOR
            if (stepGroup == null)
                throw new Exception($"多余的End调用");
#endif
            if (stepGroup.IsRoot)
            {
                GetSequence(step).Enqueue(stepGroup.GetAction());
                stepGroup = null;
            }
            else stepGroup = stepGroup.Father;
        }
        private void ToSequence(Step step, IAction action)
        {
            if (stepGroup == null)
                GetSequence(step).Enqueue(action);
            else stepGroup.Push(action);
        }
        private Sequence GetSequence(Step step)
        {
            Sequence q = null;
            if (step.ignoreTimeScale)
            {
                if (!mUnscaledSequence.TryGetValue(step, out q))
                {
                    q = new Sequence();
                    mUnscaledSequence.Add(step, q);
                }
            }
            else if (!mSequenceGroup.TryGetValue(step, out q))
            {
                q = new Sequence();
                mSequenceGroup.Add(step, q);
            }
            return q;
        }
        private void Update()
        {
            Update(mUnscaledSequence, Time.unscaledDeltaTime);
            if (Time.timeScale <= 0f) return;
            Update(mSequenceGroup, Time.deltaTime);
        }
        private void Update(Dictionary<Step, Sequence> dic, float delta)
        {
            if (dic.Count == 0) return;
            mRmvStep.ToFirst();
            foreach (var pair in dic)
            {
                var q = pair.Value;
                if (q.IsExit())
                    mRmvStep.Push(pair.Key);
                else
                    q.Update(delta);
            }
            while (mRmvStep.Count > 0)
            {
                dic.Remove(mRmvStep.Pop());
            }
        }
    }
}