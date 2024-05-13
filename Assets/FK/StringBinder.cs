using System;

namespace Panty
{
    public class StringBinder
    {
        private Action<string> mCallBack;
        private string mValue;
        public string Value
        {
            get => mValue;
            set
            {
                if (mValue == value) return;
                mValue = value;
                mCallBack?.Invoke(value);
            }
        }
        public StringBinder(string value = default) => mValue = value;
        public void RegisterWithInitValue(Action<string> onValueChanged)
        {
            mCallBack += onValueChanged;
            onValueChanged(mValue);
        }
        public void Register(Action<string> onValueChanged) => mCallBack += onValueChanged;
        public void UnRegister(Action<string> onValueChanged) => mCallBack -= onValueChanged;
        public override string ToString() => mValue;
        public void Set(string value) => mValue = value;
    }
}