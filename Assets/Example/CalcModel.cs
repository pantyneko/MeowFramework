using UnityEngine;

namespace Panty.Test
{
    public struct CalcResultQuery : IQuery<float>
    {
        public float Do(IModuleHub hub)
        {
            var model = hub.Module<ICalcModel>();
            string op = hub.Module<IOpSystem>().Op;
            int a = model.NumA;
            int b = model.NumB;
            return op switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => (float)a * b,
                "/" => (float)a / b,
                _ => int.MaxValue,
            };
        }
    }
    public struct NextOpIndexCmd : ICmd
    {
        public void Do(IModuleHub hub)
        {
            hub.Module<IOpSystem>().NextOpIndex();
            hub.SendCmd<CalcCmd>();
        }
    }
    public struct RandomCalcCmd : ICmd
    {
        public void Do(IModuleHub hub)
        {
            var model = hub.Module<ICalcModel>();
            model.NumA.Value = Random.Range(1, 100);
            model.NumB.Value = Random.Range(1, 100);
            hub.SendCmd<CalcCmd>();
        }
    }
    public struct CalcCmd : ICmd
    {
        public void Do(IModuleHub hub)
        {
            var result = hub.Query<CalcResultQuery, float>();
            hub.SendEvent(new CalcEvent() { result = result });
        }
    }
    public struct OpChangeEvent
    {
        public string op;
    }
    public struct CalcEvent
    {
        public float result;
    }
    public interface IOpSystem : IModule
    {
        string Op { get; }
        void NextOpIndex();
    }
    public class OpSystem : AbsModule, IOpSystem
    {
        private int opIndex;
        private string[] ops;
        public string Op => ops[opIndex];
        protected override void OnInit()
        {
            ops = new string[4] { "+", "-", "*", "/" };
            opIndex = 0;
        }
        public void NextOpIndex()
        {
            opIndex = (opIndex + 1) % ops.Length;
            this.SendEvent(new OpChangeEvent() { op = ops[opIndex] });
        }
    }
    public interface ICalcModel : IModule
    {
        ValueBinder<int> NumA { get; }
        ValueBinder<int> NumB { get; }
    }
    public class CalcModel : AbsModule, ICalcModel
    {
        public ValueBinder<int> NumA { get; } = 1;
        public ValueBinder<int> NumB { get; } = 2;
        protected override void OnInit() { }
    }
}