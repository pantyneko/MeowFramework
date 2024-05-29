using System;
using UnityEngine;

namespace Panty
{
    public interface ITaskScheduler : IModule
    {
        DelayTask AddDelayTask(float duration, Action onFinished, bool isLoop = false, bool isUnScaled = false);
        DelayTask AddTemporaryTask(float duration, Action onUpdate, bool isUnScaled = false);
    }
    public class TaskScheduler : AbsModule, ITaskScheduler
    {
        private PArray<DelayTask> mAvailable;
        private PArray<DelayTask> mDelayTasks;
        private PArray<DelayTask> mUnScaledTasks;

        public DelayTask AddDelayTask(float duration, Action onFinished, bool isLoop, bool isUnScaled)
        {
            var task = mAvailable.IsEmpty ? new DelayTask() : mAvailable.Pop();
            task.Init(duration, isLoop, onFinished).Start();
            if (isUnScaled) mUnScaledTasks.Push(task);
            else mDelayTasks.Push(task);
            return task;
        }
        DelayTask ITaskScheduler.AddTemporaryTask(float duration, Action onUpdate, bool isUnScaled)
        {
#if DEBUG
            if (onUpdate == null) throw new ArgumentNullException("Task is Empty");
#endif
            MonoKit.OnUpdate += onUpdate;
            return AddDelayTask(duration, () => MonoKit.OnUpdate -= onUpdate, false, isUnScaled);
        }
        protected override void OnInit()
        {
            mUnScaledTasks = new PArray<DelayTask>();
            mDelayTasks = new PArray<DelayTask>();
            mAvailable = new PArray<DelayTask>();
            MonoKit.OnUpdate += Update;
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
            if (mDelayTasks.IsEmpty || Time.timeScale <= 0f) return;
            i = 0; delta = Time.deltaTime;
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
        public void Execute() => mTask?.Invoke();
        public void Clear() => mTask = null;
        /// <summary>
        /// 初始化任务 该阶段会自动调用 Reset
        /// </summary>
        public DelayTask Init(float delayTime, bool isLoop, Action task)
        {
#if DEBUG
            if (task == null) throw new ArgumentNullException("Task is Empty");
#endif
            mRemTime = DelayTime = delayTime;
            mTask = task;
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