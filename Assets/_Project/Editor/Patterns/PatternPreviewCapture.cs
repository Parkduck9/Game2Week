using System.IO;
using Game2Week.Data.Patterns;
using UnityEngine;

namespace Game2Week.EditorTools.Patterns
{
    public static class PatternPreviewCapture
    {
        public static void Capture()
        {
            Directory.CreateDirectory("Logs");
            using(var preview=new PatternPreview())
                foreach(var d in PatternAssetStore.List())
                {
                    var image=preview.RenderFrame(new Rect(0,0,960,600),d,.4f,20,45,false,new Vector2(8,10),Vector3.forward*3,Vector3.back*3);
                    var rt=RenderTexture.GetTemporary(960,600,0);
                    var previous=RenderTexture.active;
                    var pixels=new Texture2D(960,600,TextureFormat.RGB24,false);
                    try
                    {
                        Graphics.Blit(image,rt);RenderTexture.active=rt;
                        pixels.ReadPixels(new Rect(0,0,960,600),0,0);pixels.Apply();
                        File.WriteAllBytes("Logs/preview_"+d.name+".png",pixels.EncodeToPNG());
                    }
                    finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(pixels);}
                }
            Debug.Log("Pattern Editor preview captures complete.");
        }
    }
}
