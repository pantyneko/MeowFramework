using System;

namespace Panty
{
    public class ValueBinder<V> where V : struct, IEquatable<V>
    {
        private Action<V> mCallBack;
        private V mValue;
        public V Value
        {
            get => mValue;
            set
            {
                if (mValue.Equals(value)) return;
                mValue = value;
                mCallBack?.Invoke(value);
            }
        }
        public ValueBinder(V value = default) => mValue = value;
        public void RegisterWithInitValue(Action<V> onValueChanged)
        {
            mCallBack += onValueChanged;
            onValueChanged(mValue);
        }
        public void Register(Action<V> onValueChanged) => mCallBack += onValueChanged;
        public void UnRegister(Action<V> onValueChanged) => mCallBack -= onValueChanged;
        public override string ToString() => mValue.ToString();
        public void Set(V value) => mValue = value;
    }
}