using System;

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

        public void AddTask(Action task) => mTask += task;
        public DelayTask SetTask(Action task) { mTask = task; return this; }
        public void Execute() => mTask?.Invoke();
        public void Clear() => mTask = null;
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
                    if (Loop) Reset();
                    else Stop();
                    Execute();
                }
                else Skip(delta);
            }
        }
    }
}