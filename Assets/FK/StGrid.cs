using System;
using System.Collections.Generic;

namespace Panty
{
    [Flags]
    public enum Dir4 : byte
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
        All = 16,
    }
    // 静态网格
    [Serializable]
    public partial class StGrid
    {
        // 网格左下角的坐标和每个格子的宽度、高度
        public float xMin, yMin, cw, ch;
        // 网格的行数和列数
        public int row, colm;
        // 计算网格的总大小（行数*列数）
        public int Size => row * colm;
        // 计算网格的一半行数和列数
        public int SubRow => row >> 1;
        public int SubColm => colm >> 1;
        // 计算网格的总宽度和高度
        public float W => colm * cw;
        public float H => row * ch;
        // 计算网格中心的X和Y坐标
        public float CenterX => xMin + W * 0.5f;
        public float CenterY => yMin + H * 0.5f;
        // 计算网格右上角的X和Y坐标
        public float xMax => xMin + W;
        public float yMax => yMin + H;
        // 计算网格的对角线长度
        public float Hypotenuse => MathF.Sqrt(cw * cw + ch * ch);
        /// <summary>
        /// 构造函数，初始化网格的行列数和每个格子的宽度、高度
        /// </summary>
        /// <param name="row">行数</param>
        /// <param name="colm">列数</param>
        /// <param name="gw">网格总宽度</param>
        /// <param name="gh">网格总高度</param>
        /// <param name="isCenter">是否以中心为原点</param>
        public StGrid(int row, int colm, float gw, float gh, bool isCenter = true)
        {
            this.row = row;
            this.colm = colm;
            if (isCenter)
            {
                xMin = -gw * 0.5f;
                yMin = -gh * 0.5f;
            }
            else
            {
                xMin = 0f;
                yMin = 0f;
            }
            cw = gw / colm;
            ch = gh / row;
        }
        /// <summary>
        /// 另一个构造函数，使用左下角坐标和每个格子的宽度、高度初始化网格
        /// </summary>
        /// <param name="xMin">左下角X坐标</param>
        /// <param name="yMin">左下角Y坐标</param>
        /// <param name="cellW">每个格子的宽度</param>
        /// <param name="cellH">每个格子的高度</param>
        /// <param name="numX">列数</param>
        /// <param name="numY">行数</param>
        public StGrid(float xMin, float yMin, float cellW, float cellH, int numX, int numY)
        {
            this.xMin = xMin;
            this.yMin = yMin;
            cw = cellW;
            ch = cellH;
            colm = numX;
            row = numY;
        }
        /// <summary>
        /// 获取网格中心点的线性索引
        /// </summary>
        /// <returns>网格中心点的线性索引</returns>
        public int CenterIndex_RowMajor()
        {
            return CellIndexToLinearIndex_RowMajor(row >> 1, colm >> 1);
        }
        /// <summary>
        /// 从网格中心点缩放网格大小
        /// </summary>
        /// <param name="deltaX">X方向的缩放比例</param>
        /// <param name="deltaY">Y方向的缩放比例</param>
        public void ScaleFromCenter(float deltaX, float deltaY)
        {
            float w = W;
            float h = H;
            // 使用向量的平方和来避免开根号
            float dragMagnitudeSquared = deltaX * deltaX + deltaY * deltaY;
            // 通过比较平方和来决定是缩小还是放大 正向放大，反向缩小
            int sign = (deltaX * w + deltaY * h > 0 ? 1 : -1);
            // 计算缩放比例，避免开根号，直接用平方的比例来计算
            float scale = sign * MathF.Sqrt(dragMagnitudeSquared / (w * w + h * h)) + 1;
            // 应用缩放
            float newWidth = w * scale;
            float newHeight = h * scale;
            // 更新网格大小
            cw = newWidth / colm;
            ch = newHeight / row;
            // 根据调整方向更新起始点
            xMin = xMin + w * 0.5f - newWidth * 0.5f;
            yMin = yMin + h * 0.5f - newHeight * 0.5f;
        }
        /// <summary>
        /// 拖动调整网格大小
        /// </summary>
        /// <param name="dir">调整的方向</param>
        /// <param name="deltaX">X方向的拖动距离</param>
        /// <param name="deltaY">Y方向的拖动距离</param>
        public void DragResize(Dir4 dir, float deltaX, float deltaY)
        {
            switch (dir)
            {
                case Dir4.Left | Dir4.Up:
                    cw -= deltaX / colm;
                    ch += deltaY / row;
                    xMin += deltaX;
                    break;
                case Dir4.Right | Dir4.Up:
                    cw += deltaX / colm;
                    ch += deltaY / row;
                    break;
                case Dir4.Left | Dir4.Down:
                    cw -= deltaX / colm;
                    ch -= deltaY / row;
                    xMin += deltaX;
                    yMin += deltaY;
                    break;
                case Dir4.Right | Dir4.Down:
                    cw += deltaX / colm;
                    ch -= deltaY / row;
                    yMin += deltaY;
                    break;
            }
        }
        /// <summary>
        /// 直接调整网格大小
        /// </summary>
        /// <param name="dir">调整的方向</param>
        /// <param name="deltaX">X方向的调整量</param>
        /// <param name="deltaY">Y方向的调整量</param>
        public void Resize(Dir4 dir, float deltaX, float deltaY)
        {
            switch (dir)
            {
                case Dir4.Up:
                    ch += deltaY / row;
                    break;
                case Dir4.Down:
                    ch += deltaY / row;
                    yMin -= deltaY;
                    break;
                case Dir4.Left:
                    cw += deltaX / colm;
                    xMin -= deltaX;
                    break;
                case Dir4.Right:
                    cw += deltaX / colm;
                    break;
                case Dir4.Left | Dir4.Up:
                    cw += deltaX / colm;
                    ch += deltaY / row;
                    xMin -= deltaX;
                    break;
                case Dir4.Right | Dir4.Up:
                    cw += deltaX / colm;
                    ch += deltaY / row;
                    break;
                case Dir4.Left | Dir4.Down:
                    cw += deltaX / colm;
                    ch += deltaY / row;
                    xMin -= deltaX;
                    yMin -= deltaY;
                    break;
                case Dir4.Right | Dir4.Down:
                    cw += deltaX / colm;
                    ch += deltaY / row;
                    yMin -= deltaY;
                    break;
                case Dir4.All:
                    cw += deltaX * 2f / colm;
                    ch += deltaY * 2f / row;
                    xMin -= deltaX;
                    yMin -= deltaY;
                    break;
            }
        }
        /// <summary>
        /// 获取某个方向上的邻居索引
        /// </summary>
        /// <param name="r">行索引</param>
        /// <param name="c">列索引</param>
        /// <param name="direction">方向</param>
        /// <returns>是否存在邻居索引</returns>
        public bool GetNeighborIndex(ref int r, ref int c, Dir4 direction)
        {
            switch (direction)
            {
                case Dir4.Up: r += 1; break;
                case Dir4.Down: r -= 1; break;
                case Dir4.Left: c -= 1; break;
                case Dir4.Right: c += 1; break;
                case Dir4.Left | Dir4.Up:
                    r += 1;
                    c -= 1;
                    break;
                case Dir4.Left | Dir4.Down:
                    r -= 1;
                    c -= 1;
                    break;
                case Dir4.Right | Dir4.Up:
                    r += 1;
                    c += 1;
                    break;
                case Dir4.Right | Dir4.Down:
                    r -= 1;
                    c += 1;
                    break;
                default: return false;
            }
            return r >= 0 && r < row && c >= 0 && c < colm;
        }
        /// <summary>
        /// 获取环绕网格的邻居索引（用于边界穿越）
        /// </summary>
        /// <param name="index">当前索引</param>
        /// <param name="direction">方向</param>
        public void GetWrappedNeighborIndex(ref int index, Dir4 direction)
        {
            LinearIndexToCellIndex_RowMajor(index, out int r, out int c);
            GetWrappedNeighborIndex(ref r, ref c, direction);
            index = CellIndexToLinearIndex_RowMajor(r, c);
        }
        /// <summary>
        /// 获取环绕网格的邻居索引（用于边界穿越）
        /// </summary>
        /// <param name="r">行索引</param>
        /// <param name="c">列索引</param>
        /// <param name="direction">方向</param>
        public void GetWrappedNeighborIndex(ref int r, ref int c, Dir4 direction)
        {
            switch (direction)
            {
                case Dir4.Up: r = (r + 1) % row; break;
                case Dir4.Down: r = (r - 1 + row) % row; break;
                case Dir4.Left: c = (c - 1 + colm) % colm; break;
                case Dir4.Right: c = (c + 1) % colm; break;

                case Dir4.Left | Dir4.Up:
                    r = (r + 1) % row;
                    c = (c - 1 + colm) % colm;
                    break;
                case Dir4.Left | Dir4.Down:
                    r = (r - 1 + row) % row;
                    c = (c - 1 + colm) % colm;
                    break;
                case Dir4.Right | Dir4.Up:
                    r = (r + 1) % row;
                    c = (c + 1) % colm;
                    break;
                case Dir4.Right | Dir4.Down:
                    r = (r - 1 + row) % row;
                    c = (c + 1) % colm;
                    break;
            }
        }
        /// <summary>
        /// 将实际坐标转换为网格中的行列索引
        /// </summary>
        /// <param name="x">实际X坐标</param>
        /// <param name="y">实际Y坐标</param>
        /// <param name="r">行索引</param>
        /// <param name="c">列索引</param>
        public void CoordToCellIndex(float x, float y, out int r, out int c)
        {
            r = (int)((y - yMin) / ch);
            c = (int)((x - xMin) / cw);
        }
        /// <summary>
        /// 将网格的行列索引转换为线性索引（行主序，行索引从底部开始计数）
        /// </summary>
        /// <param name="rIndex">行索引</param>
        /// <param name="cIndex">列索引</param>
        /// <returns>线性索引</returns>
        public int InvCellIndexToLinearIndex_RowMajor(int rIndex, int cIndex) =>
                    (row - 1 - rIndex) * colm + cIndex;
        /// <summary>
        /// 将网格的行列索引转换为线性索引（行主序）
        /// </summary>
        /// <param name="rIndex">行索引</param>
        /// <param name="cIndex">列索引</param>
        /// <returns>线性索引</returns>
        public int CellIndexToLinearIndex_RowMajor(int rIndex, int cIndex) =>
            rIndex * colm + cIndex;
        /// <summary>
        /// 将网格的行列索引转换为线性索引（列主序）
        /// </summary>
        /// <param name="rIndex">行索引</param>
        /// <param name="cIndex">列索引</param>
        /// <returns>线性索引</returns>
        public int CellIndexToLinearIndex_ColMajor(int rIndex, int cIndex) =>
            cIndex * row + rIndex;
        /// <summary>
        /// 将线性索引转换为网格的行列索引（行主序）
        /// </summary>
        /// <param name="index">线性索引</param>
        /// <param name="r">行索引</param>
        /// <param name="c">列索引</param>
        public void LinearIndexToCellIndex_RowMajor(int index, out int r, out int c)
        {
            r = index / colm;
            c = index % colm;
        }
        /// <summary>
        /// 将线性索引转换为网格中心坐标（行主序）
        /// </summary>
        /// <param name="index">线性索引</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public void LinearIndexToCoordCenter_RowMajor(int index, out float x, out float y)
        {
            LinearIndexToCellIndex_RowMajor(index, out int r, out int c);
            CellIndexToCoordCenter(r, c, out x, out y);
        }
        /// <summary>
        /// 将线性索引转换为网格左下角坐标（行主序）
        /// </summary>
        /// <param name="index">线性索引</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public void LinearIndexToWorldCoord_RowMajor(int index, out float x, out float y)
        {
            LinearIndexToCellIndex_RowMajor(index, out int r, out int c);
            CellIndexToWorldCoord(r, c, out x, out y);
        }
        public void InvLinearIndexToWorldCoord_RowMajor(int index, out float x, out float y)
        {
            LinearIndexToCellIndex_RowMajor(index, out int r, out int c);
            CellIndexToWorldCoord(row - 1 - r, c, out x, out y);
        }
        /// <summary>
        /// 将网格的行列索引转换为中心坐标
        /// </summary>
        /// <param name="rIndex">行索引</param>
        /// <param name="cIndex">列索引</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public void CellIndexToCoordCenter(int rIndex, int cIndex, out float x, out float y)
        {
            x = xMin + (cIndex + 0.5f) * cw;
            y = yMin + (rIndex + 0.5f) * ch;
        }
        /// <summary>
        /// 将网格的行列索引转换为实际坐标
        /// </summary>
        /// <param name="rIndex">行索引</param>
        /// <param name="cIndex">列索引</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        public void CellIndexToWorldCoord(int rIndex, int cIndex, out float x, out float y)
        {
            x = xMin + cIndex * cw;
            y = yMin + rIndex * ch;
        }
        /// <summary>
        /// 检查某个坐标是否在网格内，并返回对应的边角信息
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="half">边角的距离</param>
        /// <returns>方向信息</returns>
        public Dir4 CheckEdgeCorner(float x, float y, float half)
        {
            // 计算左下角的矩形左下角点
            float minx = xMin - half;
            float miny = yMin - half;
            float maxx = xMin + half;
            float maxy = yMin + half;
            if (x >= minx && x < maxx && y >= miny && y < maxy)
                return Dir4.Left | Dir4.Down;
            float maxY = yMax;
            minx = xMin - half;
            miny = maxY - half;
            maxx = xMin + half;
            maxy = maxY + half;
            if (x >= minx && x < maxx && y >= miny && y < maxy)
                return Dir4.Left | Dir4.Up;
            float maxX = xMax;
            minx = maxX - half;
            miny = yMin - half;
            maxx = maxX + half;
            maxy = yMin + half;
            if (x >= minx && x < maxx && y >= miny && y < maxy)
                return Dir4.Right | Dir4.Down;
            minx = maxX - half;
            miny = maxY - half;
            maxx = maxX + half;
            maxy = maxY + half;
            return x >= minx && x < maxx && y >= miny && y < maxy ?
                Dir4.Right | Dir4.Up : Dir4.None;
        }
        /// <summary>
        /// 检查某个点是否在网格内
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns>是否在网格内</returns>
        public bool Contains(float x, float y) =>
            x >= xMin && x < xMax && y >= yMin && y < yMax;
        /// <summary>
        /// 水平镜像某个列索引
        /// </summary>
        /// <param name="cIndex">列索引</param>
        public void HorMirror(ref int cIndex) => cIndex = colm - 1 - cIndex;
        /// <summary>
        /// 垂直镜像某个行索引
        /// </summary>
        /// <param name="rIndex">行索引</param>
        public void VerMirror(ref int rIndex) => rIndex = row - 1 - rIndex;
        /// <summary>
        /// 按行主序生成线性索引的枚举器
        /// </summary>
        /// <returns>线性索引的枚举</returns>
        public IEnumerable<int> RowMajorLinear()
        {
            for (int i = 0, len = row * colm; i < len; i++)
                yield return i;
        }
        /// <summary>
        /// 按行主序生成行列索引的枚举器
        /// </summary>
        /// <returns>行列索引的枚举</returns>
        public IEnumerable<(int r, int c)> RowMajorIndices()
        {
            for (int r = 0; r < row; r++)
                for (int c = 0; c < colm; c++)
                    yield return (r, c);
        }
        /// <summary>
        /// 按行主序从左上角开始生成行列索引的枚举器
        /// </summary>
        /// <returns>行列索引的枚举</returns>
        public IEnumerable<(int r, int c)> RowMajorIndicesByLeftUp()
        {
            for (int r = row - 1; r >= 0; r--)
                for (int c = 0; c < colm; c++)
                    yield return (r, c);
        }
        /// <summary>
        /// 按行主序从左上角开始生成坐标的枚举器
        /// </summary>
        /// <returns>坐标的枚举</returns>
        public IEnumerable<(float x, float y)> RowMajorCoordsByLeftUp()
        {
            for (int r = row - 1; r >= 0; r--)
                for (int c = 0; c < colm; c++)
                    yield return (xMin + c * cw, yMin + (r + 1) * ch);
        }
        /// <summary>
        /// 按行主序生成坐标的枚举
        /// </summary>
        /// <returns>坐标的枚举</returns>
        public IEnumerable<(float x, float y)> RowMajorCoords()
        {
            for (int r = 0; r < row; r++)
                for (int c = 0; c < colm; c++)
                    yield return (xMin + c * cw, yMin + r * ch);
        }
        /// <summary>
        /// 按行主序创建一维数组
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="creator">创建函数</param>
        /// <returns>创建的数组</returns>
        public T[] CreateArrayRowMajor<T>(Func<int, int, T> creator)
        {
            T[] arr = new T[row * colm];
            for (int r = 0, index = 0; r < row; r++)
                for (int c = 0; c < colm; c++)
                    arr[index++] = creator(r, c);
            return arr;
        }
        /// <summary>
        /// 按行主序创建二维数组
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="creator">创建函数</param>
        /// <returns>创建的二维数组</returns>
        public T[,] Create2DArrayRowMajor<T>(Func<int, int, T> creator)
        {
            T[,] array = new T[row, colm];
            for (int r = 0; r < row; r++)
                for (int c = 0; c < colm; c++)
                    array[r, c] = creator(r, c);
            return array;
        }
    }
}