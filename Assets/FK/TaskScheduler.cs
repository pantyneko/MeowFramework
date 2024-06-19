using System;
using System.Collections.Generic;
using UnityEngine;

namespace Panty
{
    public interface ITaskScheduler : IModule
    {
        void ConditionalTask(Func<bool> exitCondition, Action call);
        TaskScheduler.Step Register();
    }
    public class TaskScheduler : AbsModule, ITaskScheduler
    {
        private class Item
        {
            public bool isTemporary;
            public Func<bool> exitCondition;
            public DelayTask task;

            public Item(bool isTemporary, Func<bool> exit, DelayTask task)
            {
                this.isTemporary = isTemporary;
                this.exitCondition = exit;
                this.task = task;
            }
        }

        private static Action<Step> OnAddDelay;
        private static Action<Step, Func<bool>> OnAddSequence;

        private PArray<DelayTask> mAvailable;
        private PArray<(bool, DelayTask)> mDelayTasks, mUnScaledTasks;
        private PArray<(Func<bool> onExit, Action call)> mConditionalTasks;
        private Dictionary<Step, Queue<Item>> mSequenceGroup;

        private PArray<Step> mRmvStep;

        public class Step
        {
            public Action call;
            public float duration;
            public bool loop, ignoreTimeScale, isTemporary;

            public Step Call(Action call = null)
            { this.call = call; return this; }
            public Step Temporary(bool isTemporary = false)
            { this.isTemporary = isTemporary; return this; }
            public Step Delay(float duration)
            { this.duration = duration; return this; }
            public Step IgnoreTimeScale(bool ignoreTimeScale = false)
            { this.ignoreTimeScale = ignoreTimeScale; return this; }
            public Step Loop(bool loop = false)
            { this.loop = loop; return this; }
            public void Execute()
            {
#if DEBUG
                if (call == null) throw new Exception("无意义回调函数");
#endif
                OnAddDelay(this);
            }
            public Step Submit(Func<bool> exit = null)
            {
                OnAddSequence(this, exit);
                return this;
            }
        }
        void ITaskScheduler.ConditionalTask(Func<bool> exitCondition, Action call)
        {
#if DEBUG
            if (call == null) throw new Exception("无意义回调函数");
#endif
            mConditionalTasks.Push((exitCondition, call));
        }
        Step ITaskScheduler.Register() => new Step();
        private DelayTask GetDelay() => mAvailable.IsEmpty ? new DelayTask() : mAvailable.Pop().Clear();
        private void AddDelay(Step step)
        {
            var task = GetDelay();
            task.Init(step.duration, step.loop);
            task.SetTask(step.call).Start();
            if (step.ignoreTimeScale)
                mUnScaledTasks.Push((step.isTemporary, task));
            else
                mDelayTasks.Push((step.isTemporary, task));
        }
        private void AddSequence(Step step, Func<bool> exitCondition)
        {
            var task = GetDelay();
            task.Init(step.duration, step.loop);
            task.SetTask(step.call).Start();

            if (!mSequenceGroup.TryGetValue(step, out var q))
            {
                q = new Queue<Item>();
                mSequenceGroup.Add(step, q);
            }
            q.Enqueue(new Item(step.isTemporary, exitCondition, task));
        }
        protected override void OnInit()
        {
            mSequenceGroup = new Dictionary<Step, Queue<Item>>();
            mConditionalTasks = new PArray<(Func<bool>, Action)>();
            mUnScaledTasks = new PArray<(bool, DelayTask)>();
            mDelayTasks = new PArray<(bool, DelayTask)>();
            mAvailable = new PArray<DelayTask>();
            mRmvStep = new PArray<Step>();
            MonoKit.OnUpdate += Update;

            OnAddDelay = AddDelay;
            OnAddSequence = AddSequence;
        }
        private void Update()
        {
            float delta = Time.unscaledDeltaTime;
            UpdateTasks(mUnScaledTasks, delta);
            if (Time.timeScale <= 0f) return;
            int i = 0;
            while (i < mConditionalTasks.Count)
            {
                var item = mConditionalTasks[i];
                if (item.onExit())
                {
                    item.call.Invoke();
                    mConditionalTasks.RmvAt(i);
                }
                else i++;
            }
            delta = Time.deltaTime;
            if (mSequenceGroup.Count > 0)
            {
                mRmvStep.ToFirst();
                foreach (var pair in mSequenceGroup)
                {
                    var q = pair.Value;
                    var x = q.Peek();
                    // 如果没有定义退出条件 就关注计时器任务
                    if (x.exitCondition == null)
                    {
                        // 如果是临时任务 在计时完成前执行
                        x.task.Update(delta);
                        if (x.isTemporary)
                        {
                            if (x.task.IsEnd())
                            {
                                RmvStep(q, pair.Key);
                            }
                            else x.task.Execute();
                        }
                        else if (x.task.IsEnd())
                        {
                            RmvStep(q, pair.Key);
                        }
                    }
                    // 如果有退出条件
                    else if (x.exitCondition())
                    {
                        x.task.Execute();
                        RmvStep(q, pair.Key);
                    }
                    // 如果没有满足退出条件 就执行
                    else if (x.isTemporary)
                    {
                        x.task.Execute();
                    }
                }
                while (mRmvStep.Count > 0)
                {
                    mSequenceGroup.Remove(mRmvStep.Pop());
                }
            }
            UpdateTasks(mDelayTasks, delta);
        }
        private void UpdateTasks(PArray<(bool, DelayTask)> tasks, float delta)
        {
            int i = 0;
            while (i < tasks.Count)
            {
                var (tmp, task) = tasks[i];
                task.Update(delta);
                if (task.IsEnd())
                {
                    mAvailable.Push(task);
                    tasks.RmvAt(i);
                }
                else
                {
                    if (tmp) task.Execute();
                    i++;
                }
            }
        }
        private void RmvStep(Queue<Item> q, Step step)
        {
            q.Dequeue();
            if (q.Count == 0)
                mRmvStep.Push(step);
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
        public DelayTask Clear()
        {
            mTask = null;
            return this;
        }
        /// <summary>
        /// 初始化任务 该阶段会自动调用 Reset
        /// </summary>
        public DelayTask Init(float delayTime, bool isLoop)
        {
            mRemTime = DelayTime = delayTime;
            Loop = isLoop;
            return this;
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
                    Execute();
                    if (Loop) Reset();
                    else Stop();
                }
                else Skip(delta);
            }
        }
    }
}