using UnityEngine;

namespace Panty
{
    public class SO_SpriteArr : ScriptableObject
    {
        [SerializeField] private Sprite[] arr;
        public Sprite[] Arr => arr;
    }
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SO_SpriteArr))]
    public class SpriteArrEditor : ArrayGridEditor { }
#endif
}