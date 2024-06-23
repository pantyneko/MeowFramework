using System;
using System.Collections.Generic;
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
        private class BaseAction : IAction
        {
            public bool isTemporary;
            public Func<bool> exitCondition;
            public DelayTask task;

            public BaseAction(bool isTemporary, Func<bool> exitCondition, DelayTask task)
            {
                this.task = task;
                this.isTemporary = isTemporary;
                this.exitCondition = exitCondition;
            }
            public bool IsExit() => task.IsEnd();
            public void Reset()
            {
                task.Reset();
                task.Start();
            }
            public void Update(float delta)
            {
                if (exitCondition == null)
                {
                    task.Update(delta);
                    if (isTemporary)
                    {
                        if (task.IsEnd()) return;
                        task.Execute();
                    }
                }
                else if (exitCondition())
                {
                    task.Execute();
                    task.Stop();
                }
                else if (isTemporary)
                {
                    task.Execute();
                }
            }
        }
        private class RepeatAction : IAction
        {
            private PArray<IAction> sequences;
            private int cur = 0;
            private byte counter, maxCount;

            public RepeatAction(PArray<IAction> sequences, byte count)
            {
                this.sequences = sequences;
                maxCount = count;
            }
            public bool IsExit() => counter == 0;
            public void Reset()
            {
                foreach (var s in sequences) s.Reset();
                counter = maxCount;
                cur = 0;
            }
            public void Update(float delta)
            {
                var s = sequences[cur];
                if (s.IsExit())
                {
                    s.Reset();
                    sequences.LoopPos(ref cur);
                    if (cur == 0) counter--;
                }
                else s.Update(delta);
            }
        }
        private class LoopAction : IAction
        {
            private int cur = 0;
            private PArray<IAction> sequences;
            private Func<bool> exitCondition;
            public LoopAction(PArray<IAction> sequences, Func<bool> exitCondition)
            {
                this.sequences = sequences;
                this.exitCondition = exitCondition;
            }
            public bool IsExit() => exitCondition();
            public void Reset()
            {
                foreach (var s in sequences) s.Reset();
                cur = 0;
            }
            public void Update(float delta)
            {
                var s = sequences[cur];
                if (s.IsExit())
                {
                    s.Reset();
                    sequences.LoopPos(ref cur);
                }
                else s.Update(delta);
            }
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
                E_Type.Repeat => new RepeatAction(cache, mCounter),
                E_Type.Loop => new LoopAction(cache, mOnExit),
                _ => null,
            };
        }
        private enum E_Type : byte
        {
            Loop, Repeat, Single
        }
        private static byte mCounter;
        private static Func<bool> mOnExit;

        private Group stepGroup = null;
        private PArray<Step> mRmvStep;
        private PArray<DelayTask> mAvailable, mDelayTasks, mUnScaledTasks;
        private PArray<(Func<bool> isEnd, Action call)> mConditionalTasks;
        private Dictionary<Step, Sequence> mSequenceGroup;
        private Dictionary<Step, Sequence> mUnscaledSequence;

        public class Step
        {
            private static TaskScheduler mScheduler;

            public Action call;
            public float duration;
            public bool loop, ignoreTimeScale, isTemporary;

            public Step(bool ignoreTimeScale) =>
                this.ignoreTimeScale = ignoreTimeScale;
            public Step(TaskScheduler scheduler, bool ignoreTimeScale)
                : this(ignoreTimeScale) => mScheduler = scheduler;

            public Step Clone() => new Step(ignoreTimeScale)
            {
                loop = loop,
                call = call,
                duration = duration,
                isTemporary = isTemporary
            };
            public Step Call(Action call = null)
            { this.call = call; return this; }
            public Step Temporary(bool isTemporary = false)
            { this.isTemporary = isTemporary; return this; }
            public Step Delay(float duration)
            { this.duration = duration; return this; }
            public Step Loop(bool loop = false)
            { this.loop = loop; return this; }
            /// <summary>
            /// 插入无回调常规任务时 当 onExit 为空 将变为计时任务
            /// </summary>
            public Step NotCallInsert(Func<bool> onExit = null)
            {
                call = null;
                return Insert(onExit);
            }
            /// <summary>
            /// 插入常规任务时 当 onExit 为空 将变为计时任务
            /// </summary>
            public Step Insert(Func<bool> onExit = null)
            {
                // 如果组为null 说明没有层级 直接挂到主序列中
                if (mScheduler.stepGroup == null)
                    mScheduler.ToSequence(this, onExit);
                else // 添加进当前序列的缓冲区
                    mScheduler.RepeatCache(Clone(), onExit);
                return this;
            }
            public Step NotExitInsert(Action call)
            {
                this.call = call;
                return Insert(null);
            }
            /// <summary>
            /// 启用连续循环模式 在End调用前 将会以缓存来处理每次的任务
            /// 注意 ：onExit不能为null
            /// </summary>
            public Step Repeat(Func<bool> onExit)
            {
                mOnExit = onExit ?? (() => true);
                mScheduler.NextGroup(E_Type.Loop);
                return this;
            }
            /// <summary>
            /// 启用次数循环模式 在End调用前 将会以缓存来处理每次的任务
            /// </summary>
            /// <param name="count">循环次数，必须大于 0</param>
            public Step Repeat(byte count)
            {
                mCounter = count == 0 ? (byte)1 : count;
                mScheduler.NextGroup(E_Type.Repeat);
                return this;
            }
            public Step End()
            {
                mScheduler.EndGroup(this);
                return this;
            }
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
        private void ToSequence(Step step, Func<bool> onExit)
        {
            GetSequence(step).Enqueue(GetAction(step, step.loop, onExit));
        }
        private void RepeatCache(Step step, Func<bool> onExit)
        {
            stepGroup.Push(GetAction(step, false, onExit));
        }
        private BaseAction GetAction(Step step, bool loop, Func<bool> onExit)
        {
            var task = GetTask().Init(step.duration, loop);
            task.SetTask(step.call).Start();
            return new BaseAction(step.isTemporary, onExit, task);
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
        protected override void OnInit()
        {
            mSequenceGroup = new Dictionary<Step, Sequence>();
            mUnscaledSequence = new Dictionary<Step, Sequence>();
            mAvailable = new PArray<DelayTask>();
            mRmvStep = new PArray<Step>();
            MonoKit.OnUpdate += Update;
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