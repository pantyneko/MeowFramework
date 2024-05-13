using UnityEngine;

namespace Panty.Test
{
    public class ExampleHub : ModuleHub<ExampleHub>
    {
        protected override void BuildModule()
        {
            // 推荐使用 MonoKit 的 OnDeInit事件 来进行销毁
            MonoKit.GetIns().OnDeInit += Deinit;
        }
    }
    public class ExampleGame : MonoBehaviour, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => ExampleHub.GetIns();
    }
    public class ExampleUI : UIPanel, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => ExampleHub.GetIns();
    }
}