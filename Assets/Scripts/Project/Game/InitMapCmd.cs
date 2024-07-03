using UnityEngine;

namespace Panty.Project
{
    public struct InitMapCmd : ICmd
    {
        public void Do(IModuleHub hub)
        {
            var info = hub.Module<ILevelModel>().Cur;
            var grid = hub.Module<IGridMapSystem>();
            grid.Create(info);
            Vector2 pos = grid.GetCenterPos();
            hub.Module<ISnakeSystem>().Create(pos);
            Vector2 ramdomPos = grid.GetRandomPos();
            hub.Module<IFoodSystem>().Create(ramdomPos);
        }
    }
}