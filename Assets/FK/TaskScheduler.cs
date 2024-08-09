using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Panty
{
    public static class TaskSchedulerEx
    {
        /// <summary>
        /// 标记为物体被销毁时停止任务
        /// </summary>
        public static void StopOnDestroy(this DelayTask task, Component c) => c.GetOrAddComponent<TaskOnDestroyStopTrigger>().Add(task);
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
    public class DelayTask
    {
        private Action mTask;

        public float DelayTime;
        private float mRemTime;
        public bool Loop;

        private enum E_State : byte { Start, Pause, Finished }
        private E_State mState = E_State.Finished;
        public bool IsEnd() => mState == E_State.Finished;
        public void Skip(float time) => mRemTime -= time;
        public float RemTime() => mRemTime;

        public void AddTask(Action task) => mTask += task;
        public DelayTask SetTask(Action task) { mTask = task; return this; }
        public void Execute() => mTask?.Invoke();
        public void Clear() => mTask = null;
        /// <summary>
        /// 初始化任务 该阶段会自动调用 Reset
        /// </summary>
        public void Init(float delayTime, bool isLoop)
        {
            mRemTime = DelayTime = delayTime;
            Loop = isLoop;
        }
        public void Reset() => mRemTime = DelayTime;
        public void Start() => mState = E_State.Start;
        public void Pause() => mState = E_State.Pause;
        public void Stop() => mState = E_State.Finished;
        public void Complete() => mRemTime = 0f;

        public void Continue()
        {
            if (mState == E_State.Pause)
            {
                if (Loop) Reset();
                Start();
            }
        }
        public void Update(float delta)
        {
            if (mState == E_State.Start)
            {
                if (mRemTime <= 0f)
                {
                    if (Loop) Reset();
                    else Stop();
                    Execute();
                }
                else Skip(delta);
            }
        }
    }
    public interface ITaskScheduler : IModule
    {
        /// <summary>
        /// 添加一个延时任务
        /// </summary>
        /// <param name="duration">持续时间</param>
        /// <param name="onFinished">当延时完成时做的事情</param>
        /// <param name="isLoop">是否循环执行 当完成后重新调用该任务</param>
        /// <param name="ignoreTimeScale">是否忽略时间缩放</param>
        /// <returns>计时器</returns>
        DelayTask AddDelayTask(float duration, Action onFinished, bool isLoop = false, bool ignoreTimeScale = false);
        /// <summary>
        /// 添加一个临时任务 在持续时间内 进行帧更新
        /// </summary>
        /// <param name="duration">持续时间</param>
        /// <param name="onUpdate">当在持续时间内执行的任务</param>
        /// <param name="ignoreTimeScale">是否忽略时间缩放</param>
        /// <returns>计时器</returns>
        DelayTask AddTemporaryTask(float duration, Action onUpdate, bool ignoreTimeScale = false);
        /// <summary>
        /// 任务序列 可以自定义任务组
        /// </summary>
        /// <param name="ignoreTimeScale">是否忽略时间缩放</param>
        /// <returns>任务步骤</returns>
        TaskScheduler.Step Sequence(bool ignoreTimeScale = false);
        TaskScheduler.Step Sequence<G>(G group = default, bool ignoreTimeScale = false) where G : struct, TaskScheduler.IGroup;
        TaskScheduler.Step Sequence<G, T>(G group, T data, bool ignoreTimeScale = false) where G : struct, TaskScheduler.IGroup<T>;
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
        public interface IGroup
        {
            void Execute(Step step);
        }
        public interface IGroup<T>
        {
            void Execute(Step step, T data);
        }
        private class SequenceGrp
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
            public WaitAction(Func<bool> exit)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(exit);
#endif
                exitCondition = exit;
            }
            public bool IsExit() => exitCondition();
            public void Reset() { }
            public void Update(float delta) { }
        }
        // 单纯延迟N秒触发
        private class DelayAction : IAction
        {
            private float duration, cur;
            public DelayAction(float duration) =>
                this.duration = duration < 0f ? 0f : duration;
            public bool IsExit() => cur >= duration;
            public void Reset() => cur = 0;
            public void Update(float delta) => cur += delta;
        }
        private class DelayRunAction : IAction
        {
            private float duration, cur;
            private Action call;
            public DelayRunAction(float duration, Action call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                this.duration = duration < 0f ? 0f : duration;
                this.call = call;
            }
            public bool IsExit()
            {
                if (cur >= duration)
                {
                    call.Invoke();
                    return true;
                }
                return false;
            }
            public void Reset() => cur = 0;
            public void Update(float delta) => cur += delta;
        }
        // 指定次数内重复执行的任务
        private class RepeatAction : IAction
        {
            private Action call;
            private byte repeatCount, currentCount;
            public RepeatAction(byte count, Action call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                this.repeatCount = count == 0 ? (byte)1 : count;
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
            private Func<bool> exit;
            public UntilConditionAction(Func<bool> exit, Action call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
                ThrowEx.EmptyCallback(exit);
#endif
                this.exit = exit;
                this.call = call;
            }
            public bool IsExit() => exit();
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
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                this.call = call;
                this.duration = duration < 0f ? 0f : duration;
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
            public DoAction(Action call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                this.call = call;
            }
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
            }
            public bool IsExit()
            {
                if (select == null)
                    select = actions.RandomGet();
                return select.IsExit();
            }
            public void Update(float delta)
            {
                select.Update(delta);
            }
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
            public RepeatGroup(PArray<IAction> actions, byte count)
            {
                this.repeatCount = count == 0 ? (byte)1 : count;
                this.actions = actions;
            }
            public bool IsExit() => current >= repeatCount;
            public void ResetAll() { foreach (var s in actions) s.Reset(); }
            public void Reset()
            {
                cur = 0;
                current = 0;
                ResetAll();
            }
            public void Update(float delta)
            {
                var sq = actions[cur];
                if (sq.IsExit())
                {
                    actions.LoopPosN(ref cur);
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
            public LoopGroup(PArray<IAction> actions, Func<bool> exit)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(exit);
#endif
                this.actions = actions;
                this.exitCondition = exit;
            }
            public bool IsExit() => exitCondition();
            public void Reset() { foreach (var s in actions) s.Reset(); }
            public void Update(float delta)
            {
                var s = actions[cur];
                if (s.IsExit())
                {
                    s.Reset();
                    actions.LoopPosN(ref cur);
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
            private Action mEvt;
            public Action evt => mEvt;
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
            { mEvt = evt; return this; }
            /// <summary>
            /// 插入一个等待事件 等待不需要执行内容
            /// </summary>
            public Step Wait(Func<bool> onExit)
            {
                mScheduler.ToSequence(this, new WaitAction(onExit));
                return this;
            }
            /// <summary>
            /// 插入一个自定义动作
            /// </summary>
            public Step CustomAct(IAction action)
            {
                mScheduler.ToSequence(this, action);
                return this;
            }
            /// <summary>
            /// 插入一个自定义动作
            /// </summary>
            public Step CustomAct<T>() where T : IAction, new()
            {
                mScheduler.ToSequence(this, new T());
                return this;
            }
            /// <summary>
            /// 插入一个自定义组
            /// </summary>
            public Step Custom<G>(G group = default) where G : struct, IGroup
            {
                group.Execute(this);
                return this;
            }
            /// <summary>
            /// 插入一个自定义组
            /// </summary>
            public Step Custom<T, G>(G group, T data) where G : struct, IGroup<T>
            {
                group.Execute(this, data);
                return this;
            }
            /// <summary>
            /// 插入一个延迟事件 
            /// </summary>
            public Step Delay(float duration)
            {
                mScheduler.ToSequence(this, new DelayAction(duration));
                return this;
            }
            /// <summary>
            /// 插入一个有任务的延迟事件
            /// </summary>
            public Step Delay(float duration, Action call)
            {
                mScheduler.ToSequence(this, new DelayRunAction(duration, call));
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
            public Step RandomGroup(Action<Step> call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                mScheduler.NextGroup(E_Type.Random);
                call.Invoke(this);
                mScheduler.EndGroup(this);
                return this;
            }
            /// <summary>
            /// 启用连续循环模式 在End调用前 将会以缓存来处理每次的任务
            /// 注意 ：onExit不能为null
            /// </summary>
            public Step LoopGroup(Func<bool> onExit, Action<Step> call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(onExit);
                ThrowEx.EmptyCallback(call);
#endif
                mOnExit = onExit;
                mScheduler.NextGroup(E_Type.Loop);
                call.Invoke(this);
                mScheduler.EndGroup(this);
                return this;
            }
            /// <summary>
            /// 启用次数循环模式 在End调用前 将会以缓存来处理每次的任务
            /// </summary>
            /// <param name="repeatCount">循环次数，必须大于 0</param>
            public Step RepeatGroup(byte repeatCount, Action<Step> call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                mCounter = repeatCount == 0 ? (byte)1 : repeatCount;
                mScheduler.NextGroup(E_Type.Repeat);
                call.Invoke(this);
                mScheduler.EndGroup(this);
                return this;
            }
            /// <summary>
            /// 启用并行组模式 在End调用前 将会以缓存来处理每次的任务
            /// </summary>
            public Step ParallelGroup(Action<Step> call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                mScheduler.NextGroup(E_Type.Parallel);
                call.Invoke(this);
                mScheduler.EndGroup(this);
                return this;
            }
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
                ThrowEx.EmptyCallback(onExit);
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
        private Dictionary<Step, SequenceGrp> mSequenceGroup;
        private Dictionary<Step, SequenceGrp> mUnscaledSequence;

        protected override void OnInit()
        {
            mSequenceGroup = new Dictionary<Step, SequenceGrp>();
            mUnscaledSequence = new Dictionary<Step, SequenceGrp>();
            mAvailable = new PArray<DelayTask>();
            mDelayTasks = new PArray<DelayTask>();
            mUnScaledTasks = new PArray<DelayTask>();

            mRmvStep = new PArray<Step>();
            MonoKit.OnUpdate += Update;
        }
        void ITaskScheduler.StopSequence(Step step) => GetSequence(step).Exit();
        public DelayTask AddDelayTask(float duration, Action onFinished, bool isLoop, bool ignoreTimeScale)
        {
            var task = GetTask();
            task.Init(duration, isLoop);
            task.SetTask(onFinished).Start();

            if (ignoreTimeScale)
                mUnScaledTasks.Push(task);
            else
                mDelayTasks.Push(task);
            return task;
        }
        DelayTask ITaskScheduler.AddTemporaryTask(float duration, Action onUpdate, bool ignoreTimeScale)
        {
#if DEBUG
            ThrowEx.EmptyCallback(onUpdate);
#endif
            MonoKit.OnUpdate += onUpdate;
            return AddDelayTask(duration, () => MonoKit.OnUpdate -= onUpdate, false, ignoreTimeScale);
        }
        public Step Sequence(bool ignoreTimeScale)
        {
#if UNITY_EDITOR
            if (stepGroup != null)
                throw new Exception($"存在未封闭序列{stepGroup}");
#endif
            return new Step(this, ignoreTimeScale);
        }
        Step ITaskScheduler.Sequence<G>(G group, bool ignoreTimeScale)
        {
            var step = Sequence(ignoreTimeScale);
            group.Execute(step);
            return step;
        }
        Step ITaskScheduler.Sequence<G, T>(G group, T data, bool ignoreTimeScale)
        {
            var step = Sequence(ignoreTimeScale);
            group.Execute(step, data);
            return step;
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
        private SequenceGrp GetSequence(Step step)
        {
            SequenceGrp q = null;
            if (step.ignoreTimeScale)
            {
                if (!mUnscaledSequence.TryGetValue(step, out q))
                {
                    q = new SequenceGrp();
                    mUnscaledSequence.Add(step, q);
                }
            }
            else if (!mSequenceGroup.TryGetValue(step, out q))
            {
                q = new SequenceGrp();
                mSequenceGroup.Add(step, q);
            }
            return q;
        }
        private void Update()
        {
            float delta = Time.unscaledDeltaTime;
            Update(mUnScaledTasks, delta);
            Update(mUnscaledSequence, delta);
            if (Time.timeScale <= 0f) return;
            Update(mDelayTasks, Time.deltaTime);
            Update(mSequenceGroup, Time.deltaTime);
        }
        private void Update(Dictionary<Step, SequenceGrp> dic, float delta)
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
        private void Update(PArray<DelayTask> tasks, float delta)
        {
            int i = 0;
            while (i < tasks.Count)
            {
                var task = tasks[i];
                task.Update(delta);
                if (task.IsEnd())
                {
                    mAvailable.Push(task);
                    tasks.RmvAt(i);
                }
                else i++;
            }
        }
    }
}