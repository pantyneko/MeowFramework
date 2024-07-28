using UnityEngine;

namespace Panty
{
    public static class MeshEx
    {
        public static readonly Mesh Square = RectangleMesh(1f, 1f);
        public static readonly Mesh LD_Square = LD_RectMesh(1f, 1f);
        public static Mesh RectangleMesh(float w, float h)
        {
            w *= 0.5f; h *= 0.5f;
            return new Mesh()
            {
                vertices = new Vector3[]
                {
                    new (-w, h, 0f), // 左上角
                    new (w, h, 0f), // 右上角
                    new (w, -h, 0f), // 右下角
                    new (-w, -h, 0f) // 左下角
                },
                uv = new Vector2[]
                {
                    new (0f, 1f), // 左上角
                    new (1f, 1f), // 右上角
                    new (1f, 0f), // 右下角
                    new (0f, 0f), // 左下角
                },
                triangles = new int[] { 0, 1, 2, 0, 2, 3 },
            };
        }
        public static Mesh LD_RectMesh(float w, float h)
        {
            return new Mesh()
            {
                vertices = new Vector3[]
                {
                    new Vector3(0f, 0f, 0f), // 左下角
                    new Vector3(w, 0f, 0f), // 右下角
                    new Vector3(w, h, 0f), // 右上角
                    new Vector3(0f, h, 0f) // 左上角
                },
                uv = new Vector2[]
                {
                    new Vector2(0f, 0f), // 左下角
                    new Vector2(1f, 0f), // 右下角
                    new Vector2(1f, 1f), // 右上角
                    new Vector2(0f, 1f) // 左上角
                },
                triangles = new int[] { 0, 2, 1, 0, 3, 2 },
            };
        }
    }
}