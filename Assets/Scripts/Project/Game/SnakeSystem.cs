using UnityEngine;

namespace Panty.Project
{
    public interface ISnakeSystem : IModule
    {
        void Create(Vector2 pos);
        int Head { get; set; }
        Dir4 GetDir();
        void Move(Vector2 pos);
        void Bigger(Vector2 pos);
    }
    public class SnakeSystem : AbsModule, ISnakeSystem
    {
        public int Head { get; set; }

        public void Bigger(Vector2 pos)
        {
            throw new System.NotImplementedException();
        }

        public Dir4 GetDir()
        {
            return Dir4.Up;
        }

        public void Move(Vector2 pos)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnInit()
        {

        }
        void ISnakeSystem.Create(Vector2 pos)
        {

        }
    }
}