using UnityEngine;
using UnityEngine.UI;

namespace Panty
{
    /// <summary>
    /// UI基类 封装找组件功能 以及 注册委托简化使用[子类实现 IController 接口]
    /// 提供显示或隐藏的行为 默认Awake会注册所有子对象的按钮
    /// </summary>
    public class UIPanel : MonoBehaviour
    {
        public enum Layer : byte { Top, Mid, Bot, Sys }
        public virtual void OnShow() { }
        public virtual void OnHide() { }
        // 这里最好不要写销毁
        public virtual void Activate(bool active) =>
            gameObject.SetActive(active);
        protected virtual void OnClick(string btnName) { }
        public virtual bool IsOpen => gameObject.activeSelf;
        protected virtual void Awake()
        {
            this.FindChildrenControl<Button>((objName, control) =>
            control.onClick.AddListener(() => OnClick(objName)));
        }
    }
}