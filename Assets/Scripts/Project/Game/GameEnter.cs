using UnityEngine;

namespace Panty.Project
{
    //public static class A
    //{
    //    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    //    private static void Run()
    //    {

    //    }
    //}
    public class GameEnter : ProjectGame
    {
        private void Start()
        {
            this.Module<ILevelModel>().SelectLevel(1);
            // 数据相关的内容 最好不要放在mono部分
            // 需要将网格数据 由系统或模块管理 GridSystem
            this.SendCmd<InitMapCmd>();
        }
    }
}