using UnityEngine.UI;

namespace Panty.Test
{
    public class CalcPanel : CalcUI
    {
        [FindComponent("Op")] private Text mOPText;
        [FindComponent("Result")] private Text mResultText;
        [FindComponent("InputA")] private Text mInputA;
        [FindComponent("InputB")] private Text mInputB;

        private ICalcModel mModel;

        private void Start()
        {
            mModel = this.Module<ICalcModel>();

            mModel.NumA.RegisterWithInitValue(v => mInputA.text = v.ToString()).RmvOnDestroy(this);
            mModel.NumB.RegisterWithInitValue(v => mInputB.text = v.ToString()).RmvOnDestroy(this);

            this.AddEvent<CalcEvent>(e => mResultText.text = e.result.ToString()).RmvOnDestroy(this);
            this.AddEvent<OpChangeEvent>(e => mOPText.text = e.op).RmvOnDestroy(this);
        }
        protected override void OnClick(string btnName)
        {
            switch (btnName)
            {
                case "Op":
                    this.SendCmd<NextOpIndexCmd>();
                    break;
                case "Eq":
                    this.SendCmd<CalcCmd>();
                    break;
                case "Add_NumA":
                    mModel.NumA.Value++;
                    break;
                case "Add_NumB":
                    mModel.NumB.Value++;
                    break;
                case "Sub_NumA":
                    mModel.NumA.Value--;
                    break;
                case "Sub_NumB":
                    mModel.NumB.Value--;
                    break;
                case "Random":
                    this.SendCmd<RandomCalcCmd>();
                    break;
            }
        }
    }
}