using UnityEngine;

namespace Panty.Test
{
    public class CounterHub : ModuleHub<CounterHub>
    {
        protected override void BuildModule()
        {
            // 这里记得注册模块进去
            AddModule<ICounterModel>(new CounterModel());

            MonoKit.GetIns().OnDeInit += Deinit;
        }
    }
    public class CounterGame : MonoBehaviour, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => CounterHub.GetIns();
    }
    public class CounterUI : UIPanel, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => CounterHub.GetIns();
    }
}