using UnityEngine;

namespace Panty.Project
{
    public class ProjectHub : ModuleHub<ProjectHub>
    {
        protected override void BuildModule()
        {

        }
    }
    public class ProjectGame : MonoBehaviour, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => ProjectHub.GetIns();
    }
    public class ProjectUI : UIPanel, IPermissionProvider
    {
        IModuleHub IPermissionProvider.Hub => ProjectHub.GetIns();
    }
}