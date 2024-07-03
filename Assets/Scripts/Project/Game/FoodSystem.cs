using UnityEngine;

namespace Panty.Project
{
    public struct EatFoodEvent { }
    public interface IFoodSystem : IModule
    {
        void Create(Vector2 pos);        
    }
    //public class FoodSystem : AbsModule, IFoodSystem
    //{
    //    protected override void OnInit()
    //    {

    //    }
    //}
}