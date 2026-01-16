using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI;
using System.IO;

public class debugScript : MonoBehaviour
{

    public RawImage waimage; // Inspectorでセット
    string path = @"C:\Users\aimus\Desktop\imagedebug.png";
    // Start is called before the first frame update
    void Start()
    {
        LoadPNGToRawImage();
    }

    // Update is called once per frame
    void Update()
    {
        LoadPNGToRawImage();
    }
    [ContextMenu("Load PNG to RawImage")]
    public void LoadPNGToRawImage()
    {
        if (!File.Exists(path))
        {
            Debug.LogError("画像ファイルが見つかりません: " + path);
            return;
        }

        byte[] fileData = File.ReadAllBytes(path);

        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
        {
            // 古いテクスチャを破棄
            if (waimage.texture != null)
            {
                Destroy(waimage.texture);
            }

            waimage.texture = tex;
            Debug.Log("RawImage に画像をロードしました: " + path);
        }
        else
        {
            Debug.LogError("画像の読み込みに失敗しました");
            Destroy(tex); // 読み込み失敗時は即破棄
        }
    }
}
