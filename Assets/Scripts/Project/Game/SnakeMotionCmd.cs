using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panty.Project
{
    public struct SnakeMotionCmd : ICmd
    {
        public void Do(IModuleHub hub)
        {
            var grid = hub.Module<IGridMapSystem>();
            var snake = hub.Module<ISnakeSystem>();
            var food = hub.Module<IFoodSystem>();
            int head = snake.Head;
            Dir4 dir = snake.GetDir();
            if (grid.TryMove(ref head, dir))
            {
                // 判断当前的头部位置 
                switch (grid.GetCellType(head))
                {
                    case CellType.Body:
                        // 发送死亡事件
                        break;
                    case CellType.Food:
                        // 增长
                        snake.Bigger(grid.GetPos(head));
                        hub.SendEvent<EatFoodEvent>();
                        food.Create(grid.GetRandomPos());
                        // 更新头部在网格中的位置
                        snake.Head = head;
                        break;
                    default:
                        // 移动
                        snake.Move(grid.GetPos(head));
                        // 更新头部在网格中的位置
                        snake.Head = head;
                        break;
                }
            }
            else
            {
                // 撞到了边界
            }
        }
    }
}