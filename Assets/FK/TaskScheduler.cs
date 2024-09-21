using System;
using System.Collections.Generic;

namespace Panty
{
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

        public void AddEvent(Action task) => mTask += task;
        public DelayTask SetEvent(Action task) { mTask = task; return this; }
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
    public partial interface ITaskScheduler : IModule
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
        /// 任务序列 可以自定义任务组
        /// </summary>
        /// <param name="ignoreTimeScale">是否忽略时间缩放</param>
        /// <returns>任务步骤</returns>
        TaskScheduler.Step Sequence(bool ignoreTimeScale = false);
        TaskScheduler.Step Sequence<G>(G group = default, bool ignoreTimeScale = false) where G : struct, TaskScheduler.IGroup;
        TaskScheduler.Step Sequence<G, T>(G group, T data, bool ignoreTimeScale = false) where G : struct, TaskScheduler.IGroup<T>;
        void StopSequence(TaskScheduler.Step step);
        void DelayQueue(TaskScheduler.IQueueItem[] arr, float offsetTime, bool ignoreTimeScale = false);
        void PeriodicExecute(float duration, Action onUpdate, bool ignoreTimeScale = false);
        void WaitExecute(Func<bool> exit, Action onFinished, bool ignoreTimeScale = false);
        void DelayExecute(float duration, Action onUpdate, bool ignoreTimeScale = false);
        void UntilConditionExecute(Func<bool> onExit, Action onUpdate, bool ignoreTimeScale = false);
    }
    public partial class TaskScheduler : AbsModule, ITaskScheduler
    {
        public interface ICache
        {
            void TryCache();
        }
        public interface IQueueItem
        {
            void Run();
        }
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
                    //(actions.Peek() as IStateAnim)?.Init();
                    if (actions.Count == 0)
                        mExit = true;
                }
                else a.Update(delta);
            }
        }
        // 单纯等待条件退出
        private class WaitAction : IAction
        {
            private Func<bool> exitCondition;
            public WaitAction(Func<bool> onExit)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(onExit);
#endif
                exitCondition = onExit;
            }
            public virtual bool IsExit() => exitCondition();
            public void Update(float delta) { }
            public void Reset() { }
        }
        // 单纯等待条件退出 退出时执行方法
        private class WaitRunAction : WaitAction
        {
            private bool exit;
            private Action call;
            public WaitRunAction(Func<bool> onExit, Action call) : base(onExit)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                this.call = call;
            }
            public override bool IsExit()
            {
                if (exit) return true;

                if (base.IsExit())
                {
                    call.Invoke();
                    exit = true;
                }
                return exit;
            }
        }
        // 单纯延迟N秒退出
        private class DelayAction : IAction
        {
            private float duration, cur;
            public DelayAction(float duration) =>
                this.duration = duration < 0f ? 0f : duration;

            public void Exit() => cur = float.MaxValue;
            public virtual bool IsExit() => cur >= duration;
            public void Reset() => cur = 0;
            public void Update(float delta) => cur += delta;
        }
        // 延迟N秒后执行一件事情 退出
        private class DelayRunAction : DelayAction
        {
            private Action call;
            private bool exit;
            public DelayRunAction(float duration, Action call) : base(duration)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                this.call = call;
            }
            public override bool IsExit()
            {
                if (exit) return true;

                if (base.IsExit())
                {
                    call.Invoke();
                    exit = true;
                }
                return exit;
            }
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
            public void Exit() => currentCount = byte.MaxValue;
            public bool IsExit() => currentCount >= repeatCount;
            public void Reset() => currentCount = 0;
            public void Update(float delta)
            {
                call.Invoke();
                currentCount++;
            }
        }
        // 在条件内进行帧更新 条件满足退出
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
        // 在一定时间内 进行帧更新
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
            public void Exit() => cur = float.MaxValue;
            public bool IsExit() => cur >= duration;
            public void Reset() => cur = 0;
            public void Update(float delta)
            {
                cur += delta;
                call?.Invoke();
            }
        }
        private class ResetAction : IAction
        {
            private IAction act;
            private bool exit;
            public ResetAction(IAction act) => this.act = act;
            public bool IsExit()
            {
                if (!exit)
                {
                    act.Reset();
                    exit = true;
                }
                return exit;
            }
            public void Exit() => exit = true;
            public void Reset() => exit = false;
            public void Update(float delta) { }
        }
        // 直接执行一个逻辑
        private class DoAction : IAction
        {
            private bool exit;
            private Action call;
            public DoAction(Action call)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(call);
#endif
                this.call = call;
            }
            public void Exit() => exit = true;
            public void Reset() => exit = false;
            public void Update(float delta) { }
            public bool IsExit()
            {
                if (!exit)
                {
                    call.Invoke();
                    exit = true;
                }
                return exit;
            }
        }
        private class ActionGrp
        {
            protected PArray<IAction> actions;
            public void Init(PArray<IAction> actions) => this.actions = actions;
            public virtual void Reset()
            {
                for (int i = actions.MaxIndex; i >= 0; i--)
                    actions[i].Reset();
            }
        }
        // 随机执行组 每次从组中随机一个方法执行
        private class RandomGroup : ActionGrp, IAction
        {
            private IAction select;
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
            public override void Reset()
            {
                select = actions.RandomGet();
                select.Reset();
            }
        }
        // 重复组 将组内的逻辑 重复执行N次
        private class RepeatGroup : ActionGrp, IAction
        {
            private int repeatCount, current;
            private int cur;
            public RepeatGroup(int count)
            {
                this.repeatCount = count <= 0 ? 1 : count;
            }
            public void Exit() => current = int.MaxValue;
            public bool IsExit() => current >= repeatCount;
            public override void Reset()
            {
                cur = 0;
                current = 0;
                base.Reset();
            }
            public void Update(float delta)
            {
                var sq = actions[cur];
                if (sq.IsExit())
                {
                    if (++cur == actions.Count)
                    {
                        current++;
                        cur = 0;
                        base.Reset();
                    }
                }
                else sq.Update(delta);
            }
        }
        // 并行组 将组内的逻辑同步执行 直到都完成
        private class ParallelGroup : ActionGrp, IAction, ICache
        {
            public bool IsExit()
            {
                bool r = true;
                foreach (var s in actions)
                    if (!s.IsExit()) r = false;
                return r;
            }
            public void Update(float delta)
            {
                foreach (var s in actions)
                {
                    if (s.IsExit()) continue;
                    s.Update(delta);
                }
            }
            public void TryCache()
            {
                foreach (var action in actions)
                {
                    (action as ICache)?.TryCache();
                }
            }
        }
        // 循环组 在条件未满足的时候 顺序执行组内动作
        private class LoopGroup : ActionGrp, IAction
        {
            private int cur = 0;
            private Func<bool> exitCondition;
            public LoopGroup(Func<bool> exit)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(exit);
#endif
                this.exitCondition = exit;
            }
            public bool IsExit() => exitCondition();
            public override void Reset()
            {
                cur = 0;
                base.Reset();
            }
            public void Update(float delta)
            {
                var s = actions[cur];
                if (s.IsExit())
                {
                    if (++cur == actions.Count) Reset();
                }
                else s.Update(delta);
            }
        }
        private class QueueGroup : ActionGrp, IAction
        {
            private int cur = 0;
            public bool IsExit() => cur == actions.Count;
            public override void Reset()
            {
                cur = 0;
                base.Reset();
            }
            public void Update(float delta)
            {
                var s = actions[cur];
                if (s.IsExit()) cur++;
                else s.Update(delta);
            }
        }
        private class Group
        {
            private IAction grp;
            private Group father;
            public Group Father => father;
            public bool IsRoot => father == null;
            public Group(IAction grp, Group father)
            {
                this.grp = grp;
                this.father = father;
            }
            private PArray<IAction> cache = new PArray<IAction>();
            public void Push(IAction action) => cache.Push(action);
            public PArray<IAction> Cache => cache;
            public IAction Get() => grp;
        }
        public partial class Step
        {
            private static TaskScheduler mScheduler;
            private static Action mEvt;
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
            /// 插入一个等待事件 完成后执行
            /// </summary>
            public Step Wait(Func<bool> onExit, Action onFinished)
            {
                mScheduler.ToSequence(this, new WaitRunAction(onExit, onFinished));
                return this;
            }
            public Step Reset(IAction action)
            {
                mScheduler.ToSequence(this, new ResetAction(action));
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
            /// 插入一个自定义组
            /// </summary>
            public Step CustomGrp<G>(G group = default) where G : struct, IGroup
            {
                group.Execute(this);
                return this;
            }
            /// <summary>
            /// 插入一个自定义组
            /// </summary>
            public Step CustomGrp<T, G>(G group, T data) where G : struct, IGroup<T>
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
            public Step Event() => Event(mEvt);
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
            public Step Repeat(byte repeatCount) => Repeat(repeatCount, mEvt);
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
            public Step Until(Func<bool> exit) => Until(exit, mEvt);
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
            public Step Periodic(float duration) => Periodic(duration, mEvt);
            public Step Clone() => new Step(mScheduler, ignoreTimeScale);

            public Step QueueGroup(Action<Step> call = null)
            {
                mScheduler.NextGroup(new QueueGroup());
                if (call == null) return this;
                call.Invoke(this);
                return End();
            }
            /// <summary>
            /// 启用连续循环模式 在End调用前 将会以缓存来处理每次的任务
            /// 注意 ：onExit不能为null
            /// </summary>
            public Step LoopGroup(Func<bool> onExit, Action<Step> call = null)
            {
#if UNITY_EDITOR
                ThrowEx.EmptyCallback(onExit);
#endif
                mScheduler.NextGroup(new LoopGroup(onExit));
                if (call == null) return this;
                call.Invoke(this);
                return End();
            }
            /// <summary>
            /// 启用次数循环模式 在End调用前 将会以缓存来处理每次的任务
            /// </summary>
            /// <param name="repeatCount">循环次数，必须大于 0</param>
            public Step RepeatGroup(int repeatCount, Action<Step> call = null)
            {
                mScheduler.NextGroup(new RepeatGroup(repeatCount));
                if (call == null) return this;
                call.Invoke(this);
                return End();
            }
            /// <summary>
            /// 启用并行组模式 在End调用前 将会以缓存来处理每次的任务
            /// </summary>
            public Step ParallelGroup(Action<Step> call = null)
            {
                mScheduler.NextGroup(new ParallelGroup());
                if (call == null) return this;
                call.Invoke(this);
                return End();
            }
            /// <summary>
            /// 开始处理随机组 在End调用前 将会以缓存来处理每次的任务
            /// </summary>
            public Step RandomGroup(Action<Step> call = null)
            {
                mScheduler.NextGroup(new RandomGroup());
                if (call == null) return this;
                call.Invoke(this);
                return End();
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
        private ITimeInfo time;

        private Group stepGroup = null;
        private PArray<Step> mRmvStep;
        private PArray<IAction> mActionQueue;
        private PArray<IAction> mUnscaledQueue;
        private PArray<DelayTask> mAvailable, mDelayTasks, mUnScaledTasks;
        private Dictionary<Step, SequenceGrp> mSequenceGroup;
        private Dictionary<Step, SequenceGrp> mUnscaledSequence;
        private Dictionary<Step, SequenceGrp> mTmpDic;
        public TaskScheduler(ITimeInfo time) => this.time = time;
        protected override void OnInit()
        {
            mTmpDic = new Dictionary<Step, SequenceGrp>();
            mSequenceGroup = new Dictionary<Step, SequenceGrp>();
            mUnscaledSequence = new Dictionary<Step, SequenceGrp>();
            mAvailable = new PArray<DelayTask>();
            mDelayTasks = new PArray<DelayTask>();
            mUnScaledTasks = new PArray<DelayTask>();

            mActionQueue = new PArray<IAction>();
            mUnscaledQueue = new PArray<IAction>();

            mRmvStep = new PArray<Step>();
            MonoKit.OnUpdate += Update;
        }
        void ITaskScheduler.StopSequence(Step step) => GetSequence(step).Exit();
        private DelayTask GetTask() => mAvailable.IsEmpty ? new DelayTask() : mAvailable.Pop();
        public DelayTask AddDelayTask(float duration, Action onFinished, bool isLoop, bool ignoreTimeScale)
        {
            var task = GetTask();
            task.Init(duration, isLoop);
            task.SetEvent(onFinished).Start();
            (ignoreTimeScale ? mUnScaledTasks : mDelayTasks).Push(task);
            return task;
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
        private void NextGroup<T>(T grp) where T : ActionGrp, IAction
        {
            if (stepGroup == null)
            {
                stepGroup = new Group(grp, null);
            }
            else
            {
                var cur = stepGroup;
                stepGroup = new Group(grp, cur);
                cur.Push(grp);
            }
            grp.Init(stepGroup.Cache);
        }
        private void EndGroup(Step step)
        {
#if UNITY_EDITOR
            if (stepGroup == null)
                throw new Exception($"多余的End调用");
#endif
            if (stepGroup.IsRoot)
            {
                GetSequence(step).Enqueue(stepGroup.Get());
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
            if (mTmpDic.TryGetValue(step, out var q)) return q;
            var dic = step.ignoreTimeScale ? mUnscaledSequence : mSequenceGroup;
            if (!dic.TryGetValue(step, out q))
            {
                q = new SequenceGrp();
                mTmpDic.Add(step, q);
            }
            return q;
        }
        private void Update()
        {
            float delta = time.unscaledDeltaTime;
            Update(mUnScaledTasks, delta);
            Update(mUnscaledSequence, delta);
            Update(mUnscaledQueue, delta);

            if (time.timeScale > 0f)
            {
                delta = time.deltaTime;
                Update(mDelayTasks, delta);
                Update(mSequenceGroup, delta);
                Update(mActionQueue, delta);
            }
            if (mTmpDic.Count == 0) return;
            foreach (var e in mTmpDic)
            {
                var dic = e.Key.ignoreTimeScale ? mUnscaledSequence : mSequenceGroup;
                dic.Add(e.Key, e.Value);
            }
            mTmpDic.Clear();
        }
        private void Update(Dictionary<Step, SequenceGrp> dic, float delta)
        {
            if (dic.Count == 0) return;
            mRmvStep.ToFirst();
            foreach (var pair in dic)
            {
                var q = pair.Value;
                if (q.IsExit())
                {
                    mRmvStep.Push(pair.Key);
                }
                else q.Update(delta);
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
        private void Update(PArray<IAction> tasks, float delta)
        {
            int i = 0;
            while (i < tasks.Count)
            {
                var task = tasks[i];
                if (task.IsExit())
                {
                    tasks.RmvAt(i);
                }
                else
                {
                    task.Update(delta);
                    i++;
                }
            }
        }
        void ITaskScheduler.PeriodicExecute(float duration, Action onUpdate, bool ignoreTimeScale)
        {
            var act = new PeriodicAction(onUpdate, duration);
            (ignoreTimeScale ? mUnscaledQueue : mActionQueue).Push(act);
        }
        public void DelayExecute(float duration, Action onCompleted, bool ignoreTimeScale)
        {
            var act = new DelayRunAction(duration, onCompleted);
            (ignoreTimeScale ? mUnscaledQueue : mActionQueue).Push(act);
        }
        void ITaskScheduler.UntilConditionExecute(Func<bool> onExit, Action onUpdate, bool ignoreTimeScale)
        {
            var act = new UntilConditionAction(onExit, onUpdate);
            (ignoreTimeScale ? mUnscaledQueue : mActionQueue).Push(act);
        }
        void ITaskScheduler.WaitExecute(Func<bool> onExit, Action onFinished, bool ignoreTimeScale)
        {
            var act = new WaitRunAction(onExit, onFinished);
            (ignoreTimeScale ? mUnscaledQueue : mActionQueue).Push(act);
        }
        void ITaskScheduler.DelayQueue(IQueueItem[] arr, float offsetTime, bool ignoreTimeScale)
        {
#if DEBUG
            ThrowEx.EmptyArray(arr);
#endif
            arr[0].Run();
            for (int i = 1; i < arr.Length; i++)
            {
                int index = i;
                DelayExecute(index * offsetTime, arr[index].Run, ignoreTimeScale);
            }
        }
    }
}