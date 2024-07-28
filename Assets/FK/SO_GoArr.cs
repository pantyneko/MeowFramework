using UnityEngine;

namespace Panty
{
    public class SO_GoArr : ScriptableObject
    {
        [SerializeField] private GameObject[] arr;
        public GameObject[] Arr => arr;
    }
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(SO_GoArr))]
    public class GoArrEditor : ArrayGridEditor { }
#endif
}