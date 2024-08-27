using System;
using UnityEngine;

namespace Panty.Test
{
    public interface ICounterModel : IModule
    {
        ValueBinder<float> A { get; }
        ValueBinder<float> B { get; }
        string GetOpIcon(int id);
        string[] GetItems();
    }
    public class CounterModel : AbsModule, ICounterModel
    {
        ValueBinder<float> ICounterModel.A { get; } = 0f;
        ValueBinder<float> ICounterModel.B { get; } = 0f;

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
            hub.SendEvent<OperationSuccessfulNotify>();
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
    public struct ResultQuery : IQuery<CounterApp.Op, float>
    {
        public float Do(IModuleHub hub, CounterApp.Op op)
        {
            var model = hub.Module<ICounterModel>();
            float a = model.A;
            float b = model.B;
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

            model = this.Module<ICounterModel>();
            model.A.RegisterWithInitValue(OnAChange);
            model.B.RegisterWithInitValue(OnBChange);

            this.AddEvent<ChangeOpIconEvent>(OnChangeOp);
            this.AddEvent<OperationSuccessfulNotify>(OnOperationSuccessful);
            this.AddEvent<OperationFailedNotify>(OnOperationFailed);
        }
        private void OnOperationSuccessful(OperationSuccessfulNotify e)
        {
            "操作成功".Log();
        }
        private void OnOperationFailed(OperationFailedNotify e)
        {
            "操作失败".Log();
        }
        private void OnChangeOp(ChangeOpIconEvent e)
        {
            opText = e.icon;
        }
        private void OnDestroy()
        {
            var model = this.Module<ICounterModel>();
            model.A.Unregister(OnAChange);
            model.B.Unregister(OnBChange);

            this.RmvEvent<ChangeOpIconEvent>(OnChangeOp);
            this.RmvEvent<OperationSuccessfulNotify>(OnOperationSuccessful);
            this.RmvEvent<OperationFailedNotify>(OnOperationFailed);
        }
        private void OnAChange(float a)
        {
            A = a.ToString();
        }
        private void OnBChange(float b)
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
            var rect = new Rect(startX, startH - size, size * 6f, size);
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
                    this.SendEvent<OperationFailedNotify>();
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
                    this.SendEvent<OperationFailedNotify>();
                }
            }
            rect.x += size;
            GUI.Label(rect, "=", style);
            rect.x += size;
            rect.width = size * 2f;
            GUI.Label(rect, R, inputStyle);
            rect.y += size;
            rect.x = startW - size;
            rect.width = size * 3f;
            if (GUI.Button(rect, "Calc", btnStyle))
            {
                var op = (Op)mSelect;
                float r = this.Query<ResultQuery, Op, float>(op);
                R = op == Op.Div ? r.ToString("F2") : r.ToString();
                this.SendEvent<OperationSuccessfulNotify>();
            }
            rect.x = startX;
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
                this.SendCmdP<ChangeOpCmd, int>(sel);
                ShowList = false;
            }
        }
    }
}