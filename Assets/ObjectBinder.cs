using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Panty
{
    public class ObjectBinder<O> where O : class
    {
        private Action<O> mCallBack;
        private O mValue;
        public O Value => mValue;
        public ObjectBinder(O value = null)
        {
#if DEBUG
            if (value == null)
                throw new ArgumentNullException("Value is Empty");
#endif
            mValue = value;
        }
        public void Modify<D>(D newValue, string fieldOrPropName)
        {
#if DEBUG
            if (string.IsNullOrEmpty(fieldOrPropName))
                throw new ArgumentNullException(nameof(fieldOrPropName));
            if (mValue == null) throw new Exception($"{typeof(O)} is null");
#endif
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var memberInfo = typeof(O).GetMember(fieldOrPropName, flags).FirstOrDefault();
#if DEBUG 
            if (memberInfo == null)
                throw new Exception($"Field or property '{fieldOrPropName}' not found in type {typeof(O)}");
#endif
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                    if (propertyInfo.CanWrite)
                    {
                        if (EqualityComparer<D>.Default.Equals((D)propertyInfo.GetValue(mValue), newValue)) return;
                        propertyInfo.SetValue(mValue, newValue);
                        mCallBack?.Invoke(mValue);
                    }
                    break;
                case FieldInfo fieldInfo:
                    if (EqualityComparer<D>.Default.Equals((D)fieldInfo.GetValue(mValue), newValue)) return;
                    fieldInfo.SetValue(mValue, newValue);
                    mCallBack?.Invoke(mValue);
                    break;
            }
        }
        public void Modify<D>(D newValue, Func<O, D> oldValue, Action<O, D> modifyAction)
        {
#if DEBUG
            if (modifyAction == null) throw new Exception($"必须设置[modifyAction]");
            if (mValue == null) throw new Exception($"{typeof(O)}为空");
#endif
            if (EqualityComparer<D>.Default.Equals(oldValue(mValue), newValue)) return;
            modifyAction(mValue, newValue);
            mCallBack?.Invoke(mValue);
        }
        public void RegisterWithInitValue(Action<O> onValueChanged)
        {
            mCallBack += onValueChanged;
            onValueChanged(mValue);
        }
        public void Register(Action<O> onValueChanged) => mCallBack += onValueChanged;
        public void UnRegister(Action<O> onValueChanged) => mCallBack -= onValueChanged;
        public override string ToString() => mValue == null ? "null" : mValue.ToString();
    }
}