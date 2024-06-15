using UnityEngine;
using UnityEngine.UI;

namespace Panty
{
    public class Bind : MonoBehaviour
    {
#if UNITY_EDITOR
        public enum E_Type : byte
        {
            Transform,
            Button,
            Canvas,
            Image,
            Text,
            InputField,
            Toggle,
            Slider,
            Dropdown,
            Scrollbar,
            ScrollRect,
            RawImage
        }
        public E_Type type;
        public GameObject root;
        private void OnValidate()
        {
            var tp = type switch
            {
                E_Type.Text => typeof(Text),
                E_Type.Image => typeof(Image),
                E_Type.Button => typeof(Button),
                E_Type.Canvas => typeof(Canvas),
                E_Type.Toggle => typeof(Toggle),
                E_Type.Slider => typeof(Slider),
                E_Type.RawImage => typeof(RawImage),
                E_Type.Dropdown => typeof(Dropdown),
                E_Type.Transform => typeof(Transform),
                E_Type.Scrollbar => typeof(Scrollbar),
                E_Type.ScrollRect => typeof(ScrollRect),
                E_Type.InputField => typeof(InputField),
                _ => typeof(Transform),
            };
            if (GetComponent(tp) == null)
            {
                $"{type}组件不存在 请重新设置".Log();
                type = E_Type.Transform;
            }
        }
#endif
    }
}