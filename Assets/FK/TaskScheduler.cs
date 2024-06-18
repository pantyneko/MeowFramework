using System;
using System.Collections.Generic;
using UnityEngine;

namespace Panty
{
    public interface ITaskScheduler : IModule
    {
        void ConditionalTask(Func<bool> exitCondition, Action call);
        TaskScheduler.Step Task();
    }
    public class TaskScheduler : AbsModule, ITaskScheduler
    {
        private static Func<DelayTask> OnGetDelay;
        private static Action<DelayTask, bool> OnAddDelay;
        private static Action<Step, DelayTask, Func<bool>> OnAddSequence;

        private PArray<DelayTask> mAvailable, mDelayTasks, mUnScaledTasks;
        private PArray<(Func<bool> isEnd, Action call)> mConditionalTasks;
        private Dictionary<Step, Queue<(Func<bool>, DelayTask)>> mSequenceGroup;

        private PArray<Step> mRmvStep;

        public class Step
        {
            private Action call;
            private float duration;
            private bool loop, ignoreTimeScale, isTemporary;

            public Step Call(Action call) { this.call = call; return this; }
            public Step Temporary() { isTemporary = true; return this; }
            public Step Delay(float duration) { this.duration = duration; return this; }
            public Step IgnoreTimeScale() { ignoreTimeScale = true; return this; }
            public Step Loop() { loop = true; return this; }
            public Step ToUpdate()
            {
                OnAddDelay(GetTask(), ignoreTimeScale);
                return this;
            }
            public Step Enqueue(Func<bool> exit = null)
            {
                OnAddSequence(this, GetTask(), exit);
                return this;
            }
            private DelayTask GetTask()
            {
#if DEBUG
                if (OnGetDelay == null) throw new Exception("无法获取任务池");
                if (call == null) throw new Exception("无意义回调函数");
#endif
                var task = OnGetDelay.Invoke();
                task.Init(duration, loop).Start();
                if (isTemporary)
                {
                    MonoKit.OnUpdate += call;
                    task.SetTask(() => MonoKit.OnUpdate -= call);
                }
                else
                {
                    task.SetTask(call);
                }
                return task;
            }
        }
        Step ITaskScheduler.Task() => new Step();
        void ITaskScheduler.ConditionalTask(Func<bool> exitCondition, Action call)
        {
#if DEBUG
            if (call == null) throw new Exception("Task is Empty");
#endif
            mConditionalTasks.Push((exitCondition, call));
        }
        private DelayTask GetDelay()
        {
            return mAvailable.IsEmpty ? new DelayTask() : mAvailable.Pop().Clear();
        }
        private void AddDelay(DelayTask task, bool ignoreTimeScale)
        {
            if (ignoreTimeScale)
                mUnScaledTasks.Push(task);
            else
                mDelayTasks.Push(task);
        }
        private void AddSequence(Step step, DelayTask task, Func<bool> end)
        {
            if (mSequenceGroup.TryGetValue(step, out var q))
            {
                q.Enqueue((end, task));
            }
            else
            {
                q = new Queue<(Func<bool>, DelayTask)>();
                q.Enqueue((end, task));
                mSequenceGroup.Add(step, q);
            }
        }
        protected override void OnInit()
        {
            mSequenceGroup = new Dictionary<Step, Queue<(Func<bool>, DelayTask)>>();
            mConditionalTasks = new PArray<(Func<bool>, Action)>();
            mUnScaledTasks = new PArray<DelayTask>();
            mDelayTasks = new PArray<DelayTask>();
            mAvailable = new PArray<DelayTask>();
            mRmvStep = new PArray<Step>();
            MonoKit.OnUpdate += Update;

            OnGetDelay = GetDelay;
            OnAddDelay = AddDelay;
            OnAddSequence = AddSequence;
        }
        private void Update()
        {
            int i = 0;
            float delta = Time.unscaledDeltaTime;
            while (i < mUnScaledTasks.Count)
            {
                var task = mUnScaledTasks[i];
                task.Update(delta);
                if (task.IsEnd())
                {
                    mAvailable.Push(task);
                    mUnScaledTasks.RmvAt(i);
                }
                else i++;
            }
            if (Time.timeScale <= 0f) return;
            i = 0;
            while (i < mConditionalTasks.Count)
            {
                var task = mConditionalTasks[i];
                if (task.isEnd())
                {
                    task.call?.Invoke();
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
                    var (onFinish, task) = pair.Value.Peek();
                    if (onFinish == null)
                    {
                        task.Update(delta);
                        if (task.IsEnd())
                        {
                            pair.Value.Dequeue();
                            if (pair.Value.Count == 0)
                                mRmvStep.Push(pair.Key);
                        }
                    }
                    else if (onFinish())
                    {
                        task.Execute();
                        pair.Value.Dequeue();
                        if (pair.Value.Count == 0)
                            mRmvStep.Push(pair.Key);
                    }
                }
                while (mRmvStep.Count > 0)
                {
                    mSequenceGroup.Remove(mRmvStep.Pop());
                }
            }
            i = 0;
            while (i < mDelayTasks.Count)
            {
                var task = mDelayTasks[i];
                task.Update(delta);
                if (task.IsEnd())
                {
                    mAvailable.Push(task);
                    mDelayTasks.RmvAt(i);
                }
                else i++;
            }
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
        public void SetTask(Action task) => mTask = task;
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