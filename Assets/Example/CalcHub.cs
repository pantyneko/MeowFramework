using UnityEngine;

namespace Panty.Test
{
    public class CalcHub : ModuleHub<CalcHub>
    {
        protected override void BuildModule()
        {
            AddModule<ICalcModel>(new CalcModel());
            AddModule<IOpSystem>(new OpSystem());
        }
    }
    public class CalcGame : MonoBehaviour, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => CalcHub.GetIns();
    }
    public class CalcUI : UIPanel, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => CalcHub.GetIns();
    }
}