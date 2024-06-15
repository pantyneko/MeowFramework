using System;

namespace Panty
{
    public class Fade
    {
        private enum State : byte { Close, In, Out, }
        /// <summary>
        /// 淡入状态
        /// </summary>
        private State mState = State.Close;
        /// <summary>
        /// 淡入结束后要做的事情
        /// </summary>
        private Action mOnEvent;
        public Fade(float min = 0, float max = 1)
        {
            Min = min;
            Max = max;
        }
        public float Cur, Min, Max;
        public void Set(Action action) => mOnEvent = action;
        public bool IsClose => mState == State.Close;
        public void Close() => mState = State.Close;
        public void In() => mState = State.In;
        public void Out() => mState = State.Out;
        public void Update(float step)
        {
            switch (mState)
            {
                //如果是渐入状态 0 - 1
                case State.In:
                    if (Cur == Max) OnEnd();
                    else Cur = Cur < Max ? Cur + step : Max;
                    break;
                //如果是渐出状态 1 - 0
                case State.Out:
                    if (Cur == Min) OnEnd();
                    else Cur = Cur > Min ? Cur - step : Min;
                    break;
            }
        }
        private void OnEnd()
        {
            mState = State.Close;
            mOnEvent?.Invoke();
        }
    }
}