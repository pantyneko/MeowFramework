using UnityEngine;

namespace Panty
{
    public class SO_Texture2DArr : ScriptableObject
    {
        [SerializeField] private Texture2D[] arr;
        public Texture2D[] Arr => arr;
    }
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SO_Texture2DArr))]
    public class Texture2DArrEditor : ArrayGridEditor { }
#endif
}