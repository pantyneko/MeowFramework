using UnityEngine;

namespace Panty.Project
{
    public enum CellType
    {
        None, Food, Body
    }
    public interface IGridMapSystem : IModule
    {
        void Create(LevelInfo info); // 关卡数据作为依据生成地图
        Vector2 GetPos(int index);
        Vector2 GetCenterPos();
        Vector2 GetRandomPos();
        CellType GetCellType(int head);
        bool TryMove(ref int head, Dir4 dir);
    }
    //public class GridMapSystem : AbsModule, IGridMapSystem
    //{
    //    private StGrid mGrid;

    //    protected override void OnInit()
    //    {

    //    }
    //}
}