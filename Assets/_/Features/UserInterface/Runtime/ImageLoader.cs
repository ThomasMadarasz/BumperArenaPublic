using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface.Runtime
{
    public class ImageLoader : MonoBehaviour
    {
        #region Exposed

        [SerializeField] private Image _img;
        [SerializeField] private string _imagePath;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            if (!File.Exists(_imagePath))
            {
                _img.gameObject.SetActive(false);
                return;
            }

            byte[] bytes = File.ReadAllBytes(_imagePath);

            Texture2D tex = new Texture2D(2, 2);
            ImageConversion.LoadImage(tex, bytes);
            _img.material.mainTexture = tex;
        }

        #endregion
    }
}