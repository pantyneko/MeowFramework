using UnityEngine;

namespace Panty.Test
{
    public interface ICounterModel : IModule
    {
        ValueBinder<int> Counter { get; }
        StringBinder AddIcon { get; }
        StringBinder SubIcon { get; }
    }
    public class CounterModel : AbsModule, ICounterModel
    {
        ValueBinder<int> ICounterModel.Counter { get; } = new ValueBinder<int>();
        StringBinder ICounterModel.AddIcon { get; } = new StringBinder("+");
        StringBinder ICounterModel.SubIcon { get; } = new StringBinder("-");

        protected override void OnInit()
        {
            "第一次调用 该模块 时 执行".Log();
        }
        protected override void OnDeInit()
        {
            "当应用或编辑器退出 时 执行".Log();
        }
    }
    public enum OpIcon
    {
        Add, Sub
    }
    public struct AddCounterCmd : ICmd<int>
    {
        public void Do(IModuleHub hub, int value)
        {
            hub.Module<ICounterModel>().Counter.Value += value;
        }
    }
    public struct SubCounterCmd : ICmd
    {
        public void Do(IModuleHub hub)
        {
            hub.Module<ICounterModel>().Counter.Value--;
        }
    }
    public struct CounterOpQuery : IQuery<OpIcon, string>
    {
        public string Do(IModuleHub hub, OpIcon type)
        {
            return type switch
            {
                OpIcon.Add => hub.Module<ICounterModel>().AddIcon.Value,
                OpIcon.Sub => hub.Module<ICounterModel>().SubIcon.Value,
                _ => throw new System.Exception("没有更多Icon")
            };
        }
    }
    public struct CounterQuery : IQuery<string>
    {
        public string Do(IModuleHub hub)
        {
            return hub.Module<ICounterModel>().Counter.Value.ToString();
        }
    }
    public class CounterApp : CounterGame
    {
        private float startW, startH;
        private GUIStyle style;

        private void Start()
        {           
            startW = Screen.width >> 1;
            startH = Screen.height >> 1;

            style = new GUIStyle()
            {
                fontSize = 30,
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = Color.white;
        }
        private void OnGUI()
        {
            float size = 50f;
            var btnStyle = new GUISkin().button;
            btnStyle.fontSize = style.fontSize;
            btnStyle.alignment = TextAnchor.MiddleCenter;
            var rect = new Rect(startW, startH, size, size);
            GUI.Label(rect, this.Query<CounterQuery, string>(), style);
            rect.y = startH - size - 10f;
            if (GUI.Button(rect, this.Query<CounterOpQuery, OpIcon, string>(OpIcon.Add), btnStyle))
            {
                this.SendCmd<AddCounterCmd, int>(2);
            }
            rect.y = startH + size + 10f;
            if (GUI.Button(rect, this.Query<CounterOpQuery, OpIcon, string>(OpIcon.Sub), btnStyle))
            {
                this.SendCmd<SubCounterCmd>();
            }
        }
    }
}