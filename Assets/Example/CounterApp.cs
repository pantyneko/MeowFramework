using System;
using UnityEngine;

namespace Panty.Test
{
    public interface ICounterModel : IModule
    {
        ValueBinder<int> A { get; }
        ValueBinder<int> B { get; }
        string GetOpIcon(int id);
        string[] GetItems();
    }
    public class CounterModel : AbsModule, ICounterModel
    {
        ValueBinder<int> ICounterModel.A { get; } = new ValueBinder<int>();
        ValueBinder<int> ICounterModel.B { get; } = new ValueBinder<int>();

        private string[] Items;

        protected override void OnInit()
        {
            "第一次调用 该模块 时 执行".Log();
            Items = new string[] { "+", "-", "*", "/" };
        }
        protected override void OnDeInit()
        {
            "当应用或编辑器退出 时 执行".Log();
        }
        string ICounterModel.GetOpIcon(int id)
        {
            return Items[id];
        }
        string[] ICounterModel.GetItems()
        {
            return Items;
        }
    }
    public struct ChangeOpCmd : ICmd<int>
    {
        public void Do(IModuleHub hub, int id)
        {
            var model = hub.Module<ICounterModel>();
            hub.SendEvent(new ChangeOpIconEvent() { icon = model.GetOpIcon(id) });
            hub.SendNotify<OperationSuccessfulNotify>();
        }
    }
    public struct RandomValueCmd : ICmd
    {
        public void Do(IModuleHub hub)
        {
            var model = hub.Module<ICounterModel>();
            model.A.Value = UnityEngine.Random.Range(1, 100);
            model.B.Value = UnityEngine.Random.Range(1, 100);
        }
    }
    public struct ResultQuery : IQuery<CounterApp.Op, int>
    {
        public int Do(IModuleHub hub, CounterApp.Op op)
        {
            var model = hub.Module<ICounterModel>();
            var a = model.A.Value;
            var b = model.B.Value;
            return op switch
            {
                CounterApp.Op.Add => a + b,
                CounterApp.Op.Sub => a - b,
                CounterApp.Op.Mul => a * b,
                CounterApp.Op.Div => a / b,
                _ => throw new Exception("未识别运算符"),
            };
        }
    }
    public struct ChangeOpIconEvent
    {
        public string icon;
    }
    public struct OperationSuccessfulNotify { }
    public struct OperationFailedNotify { }

    public class CounterApp : CounterGame
    {
        public enum Op : byte
        {
            Add, Sub, Mul, Div
        }
        private float startW, startH;
        private GUIStyle style, btnStyle, inputStyle;
        private string A, B, R;
        private string opText = "+";

        private int mSelect;
        private bool ShowList;

        private ICounterModel model;

        private void Start()
        {
            startW = Screen.width >> 1;
            startH = Screen.height >> 1;

            model = this.Model<ICounterModel>();
            model.A.RegisterWithInitValue(OnAChange);
            model.B.RegisterWithInitValue(OnBChange);

            this.AddEvent<ChangeOpIconEvent>(OnChangeOp);
            this.AddNotify<OperationSuccessfulNotify>(OnOperationSuccessful);
            this.AddNotify<OperationFailedNotify>(OnOperationFailed);
        }
        private void OnOperationSuccessful()
        {
            "操作成功".Log();
        }
        private void OnOperationFailed()
        {
            "操作失败".Log();
        }
        private void OnChangeOp(ChangeOpIconEvent e)
        {
            opText = e.icon;
        }
        private void OnDestroy()
        {
            var model = this.Model<ICounterModel>();
            model.A.UnRegister(OnAChange);
            model.B.UnRegister(OnBChange);

            this.RmvEvent<ChangeOpIconEvent>(OnChangeOp);
            this.RmvNotify<OperationSuccessfulNotify>(OnOperationSuccessful);
            this.RmvNotify<OperationFailedNotify>(OnOperationFailed);
        }
        private void OnAChange(int a)
        {
            A = a.ToString();
        }
        private void OnBChange(int b)
        {
            B = b.ToString();
        }
        private void OnGUI()
        {
            if (style == null)
            {
                style = GUI.skin.label;
                style.fontSize = 30;
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.white;

                btnStyle = GUI.skin.button;
                btnStyle.fontSize = style.fontSize;
                btnStyle.alignment = TextAnchor.MiddleCenter;

                inputStyle = GUI.skin.textField;
                inputStyle.fontSize = style.fontSize;
                inputStyle.alignment = TextAnchor.MiddleCenter;
            }
            float size = 50f;
            var startX = startW - size * 4f;
            var rect = new Rect(startX, startH - size, size * 5f, size);
            if (GUI.Button(rect, "RandomNum", btnStyle))
            {
                this.SendCmd<RandomValueCmd>();
            }
            rect = new Rect(startX, startH, size, size);
            string a = GUI.TextField(rect, A, inputStyle);
            if (a != A)
            {
                if (int.TryParse(a, out int r))
                {
                    A = a;
                    model.A.Value = r;
                }
                else
                {
                    this.SendNotify<OperationFailedNotify>();
                }
            }
            rect.x += size;
            GUI.Label(rect, opText, style);
            rect.x += size;
            string b = GUI.TextField(rect, B, inputStyle);
            if (b != B)
            {
                if (int.TryParse(b, out int r))
                {
                    B = b;
                    model.B.Value = r;
                }
                else
                {
                    this.SendNotify<OperationFailedNotify>();
                }
            }
            rect.x += size;
            GUI.Label(rect, "=", style);
            rect.x += size;
            GUI.enabled = false;
            GUI.TextArea(rect, R, inputStyle);
            GUI.enabled = true;
            rect.y += size;
            rect.width = size * 2f;
            rect.x = startW - size;
            if (GUI.Button(rect, "Calc", btnStyle))
            {
                R = this.Query<ResultQuery, Op, int>((Op)mSelect).ToString();
                this.SendNotify<OperationSuccessfulNotify>();
            }
            rect.x = startX;
            rect.width = size * 3f;
            if (GUI.Button(rect, "Operator", btnStyle))
            {
                ShowList = !ShowList;
            }
            if (ShowList)
            {
                rect.height = size * 3f;
                rect.y += size;
                var sel = GUI.SelectionGrid(rect, mSelect, Enum.GetNames(typeof(Op)), 1);
                if (mSelect == sel) return;
                mSelect = sel;
                this.SendCmd<ChangeOpCmd, int>(sel);
                ShowList = false;
            }
        }
    }
}