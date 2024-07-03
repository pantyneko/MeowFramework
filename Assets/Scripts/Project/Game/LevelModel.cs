namespace Panty.Project
{
    public class LevelInfo
    {
        public int Num; // 网格的个数
    }
    public interface ILevelModel : IModule
    {
        void SelectLevel(int level);
        LevelInfo Cur { get; } // 定义一个获取当前关卡的属性
    }
    //public class LevelModel : AbsModule, ILevelModel
    //{
    //    protected override void OnInit()
    //    {

    //    }
    //}
}