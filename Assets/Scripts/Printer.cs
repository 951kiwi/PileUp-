using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System;

public class CameraPrintManager : MonoBehaviour
{
    public Camera targetCamera;
    public int width = 512;
    public int height = 512;
    public string printAppPath = @"C:\Users\aimus\source\repos\printer\printer\bin\Debug\net8.0\printer.exe"; // あなたのプリンタ用exeのパスに書き換えてください

    private void Start()
    {
        StartCoroutine(DelayedCapture());
    }

    private IEnumerator DelayedCapture()
    {
        yield return new WaitForSeconds(1f); // 2秒待つ
        Texture2D captured = CaptureFromCamera(targetCamera, width, height);
        Texture2D processed = ApplyGrayscaleAndEdge(captured);
        string imagePath = SaveToPNG(captured, "print_image.png");
        StartPrint(imagePath);
    }

    Texture2D CaptureFromCamera(Camera cam, int w, int h)
    {
        RenderTexture rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);

        cam.Render();
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        return tex;
    }

    Texture2D ApplyGrayscaleAndEdge(Texture2D original)
    {
        int width = original.width;
        int height = original.height;
        Texture2D result = new Texture2D(width, height);

        Color[] pixels = original.GetPixels();
        float[] grayPixels = new float[pixels.Length];
        Color[] edgePixels = new Color[pixels.Length];

        // グレースケール化を別配列に保存
        for (int i = 0; i < pixels.Length; i++)
        {
            grayPixels[i] = pixels[i].grayscale;
        }

        // Sobelフィルターでエッジ検出
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int i = y * width + x;

                float gx =
                    -grayPixels[(y - 1) * width + (x - 1)] + grayPixels[(y - 1) * width + (x + 1)] +
                    -2 * grayPixels[y * width + (x - 1)] + 2 * grayPixels[y * width + (x + 1)] +
                    -grayPixels[(y + 1) * width + (x - 1)] + grayPixels[(y + 1) * width + (x + 1)];

                float gy =
                    -grayPixels[(y - 1) * width + (x - 1)] - 2 * grayPixels[(y - 1) * width + x] - grayPixels[(y - 1) * width + (x + 1)] +
                     grayPixels[(y + 1) * width + (x - 1)] + 2 * grayPixels[(y + 1) * width + x] + grayPixels[(y + 1) * width + (x + 1)];

                float magnitude = Mathf.Clamp01(Mathf.Sqrt(gx * gx + gy * gy));
                edgePixels[i] = new Color(magnitude, magnitude, magnitude, 1f);
            }
        }

        result.SetPixels(edgePixels);
        result.Apply();
        return result;
    }

    string SaveToPNG(Texture2D tex, string filename)
    {
        byte[] bytes = tex.EncodeToPNG();
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, bytes);
        return path;
    }


    void StartPrint(string imagePath)
    {
        if (File.Exists(printAppPath))
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = printAppPath,
                Arguments = $"\"{imagePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var process = Process.Start(psi))
            {
                if (process != null)
                {
                    // 出力を受け取る（必要なら）
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit(); // 完了するまで待機

                    Console.WriteLine("標準出力:\n" + output);
                    Console.WriteLine("エラー出力:\n" + error);
                }
            }
        }
        else
        {
            UnityEngine.Debug.LogError("印刷アプリが見つかりません: " + printAppPath);
        }
    }
}
