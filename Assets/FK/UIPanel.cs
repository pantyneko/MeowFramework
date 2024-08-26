using UnityEngine;
using UnityEngine.UI;

namespace Panty
{
    /// <summary>
    /// UI基类 封装找组件功能 以及 注册委托简化使用[子类实现 IPermissionProvider 接口]
    /// 提供显示或隐藏的行为 默认Awake会注册所有子对象的按钮
    /// OnClick 由子类去重写 使用switch来区分不同的按钮名字 需要 base.Awake() 被调用
    /// </summary>
    public class UIPanel : MonoBehaviour
    {
        public enum Layer : byte { Top, Mid, Bot, Sys }
        // 由 UI管理器 调用 重写来执行一些当前面板所需要做的事情
        public virtual void OnShow() { }
        public virtual void OnHide() { }
        public virtual bool IsOpen => gameObject.activeSelf;
        public virtual void Active(bool active) => gameObject.SetActive(active);
        protected virtual void OnClick(string btnName) { }
        protected virtual void Awake()
        {
            this.FindChildrenControl<Button>((objName, control) =>
                control.onClick.AddListener(() => OnClick(objName)));
        }
    }
}