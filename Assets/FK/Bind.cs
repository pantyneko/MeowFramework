using System;
using UnityEngine;
using UnityEngine.UI;

namespace Panty
{
    public interface IBindRoot { }
    public class Bind : MonoBehaviour
    {
#if UNITY_EDITOR
        public static Type ToType(E_Type type)
        {
            return type switch
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

                E_Type.RectTransform => typeof(RectTransform),
                E_Type.SpriteRenderer => typeof(SpriteRenderer),
                _ => null,
            };
        }
        public enum E_Type : byte
        {
            SpriteRenderer,
            RectTransform,

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
            RawImage,

            TextMeshPro,
            TextMeshProUGUI,
            TMP_InputField,
            TMP_Dropdown,
        }
        public bool usePrefix = true;
        [SerializeField] private E_Type type;
        public E_Type CType => type;
        [SerializeField] private GameObject root;
        public GameObject Root => root;
        public void Init(GameObject root)
        {
            this.root = root;
            if (GetComponent<Image>()) type = E_Type.Image;
            else if (GetComponent<Text>()) type = E_Type.Text;
            else if (GetComponent<RawImage>()) type = E_Type.RawImage;
            else if (GetComponent<RectTransform>()) type = E_Type.RectTransform;
            else if (GetComponent<SpriteRenderer>()) type = E_Type.SpriteRenderer;
            else type = E_Type.Transform;
        }
        private void OnValidate()
        {
            if (ToType(type) == null)
            {
                $"{type}无法检视 注意准确性".Log();
                return;
            }
            if (GetComponent(ToType(type)) == null)
            {
                $"{type}组件不存在 将设置为默认变换组件".Log();
                type = E_Type.Transform;
            }
        }
#endif
    }
}