#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GrishaGuWorkshop
{
    public class ScreenshotTaker : MonoBehaviour
    {
        [SerializeField] private new Camera camera;

        private void TakeScreenshot()
        {
            var rt = camera.targetTexture;

            camera.Render();
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA64, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            RenderTexture.active = null;

            byte[] bytes;
            bytes = tex.EncodeToPNG();

            string path = Application.dataPath + "/tempScreenshot.png";
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
//        TextureImporter importer = (TextureImporter) TextureImporter.GetAtPath($"Assets/tempScreenshot.png");
//
//        importer.isReadable = true;
//        importer.textureType = TextureImporterType.Sprite;
//
//        EditorUtility.SetDirty(importer);
//        importer.SaveAndReimport();
        }
    }
}
#endif