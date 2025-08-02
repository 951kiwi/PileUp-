using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Intel.RealSense;
using Unity.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField, Header("デバッグ")]
    private GameObject DebugObject;
    private GameObject RowImageObjects;
    private int cameraIndex;

    bool isBackgroundCaptured = false; // 背景画像がキャプチャされたかを示すフラグ

    public int fallKernelSize = 5;

    public float maxScore;
    public Texture2D maxScoreScreenshot;
    public TextMeshProUGUI depthText; // Unityエディタでアサイン

    // フィールド変数に追加しておく
    public Texture2D resultTexture;
    Texture2D floorRGBTexture, floorDepthTexture, depthTexture, outputTexture;
    [SerializeField]
    private RawImage RGBImage, depthImage, floorRGBImage, floorDepthImage, sampleImage, syncImage;
    [SerializeField]
    private int floorBorderY = 50;
    [SerializeField]
    private int floorDifference = 5;
    [SerializeField, Header("floorの上下を変更しますか")]
    private bool inversionFloorY = false;

    [SerializeField]
    private int minDepth, MaxDepth;

    public event Action<ushort[]> OnDepthFrameReceived;
    private const int width = 640;
    private const int height = 480;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void HandleDepthFrame(ushort[] depthData)
    {

        if (depthData == null)
        {
            Debug.Log("HandleDepthFrame called");
            return;
        }

        Color32[] pixels = new Color32[width * height];

        // 基準の床デプスデータを z16 ushort[] に変換しておく
        Color32[] floorPixels = floorDepthTexture.GetPixels32();
        ushort[] floorDepths = new ushort[width * height];

        for (int i = 0; i < floorPixels.Length; i++)
        {
            // z16 は16bitの深度値：ここでは R + G<<8 に変換（保存時の方式に依存）
            floorDepths[i] = (ushort)(floorPixels[i].r | (floorPixels[i].g << 8));
        }


        for (int i = 0; i < width * height; i++)
        {
            ushort current = depthData[i];
            ushort floor = floorDepths[i];

            if(floor != 0 && current != 0)//黒の部分じゃなければ
                {
                int diff = Mathf.Abs(current - floor);
                if (diff >= floorDifference)
                {
                    // 差分が大きければ緑
                    pixels[i] = new Color32(0, 255, 0, 255);
                }
                else
                {
                    pixels[i] = new Color32(0, 0, 0, 255); // 黒
                }
            }
            else if(current > minDepth && current <= MaxDepth)
            {

                // 通常の青
                pixels[i] = new Color32(0, 0, 255, 255);
            }
            else
            {
                pixels[i] = new Color32(0, 0, 0, 255); // 黒
            }
        }

        depthTexture.SetPixels32(pixels);
        depthTexture.Apply();
    }

    void Start()
    {
        depthTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        outputTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        floorRGBTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        floorDepthTexture = new Texture2D(width, height, TextureFormat.R16, false);



        RowImageObjects = DebugObject.transform.Find("DebugCanvas").Find("RowImage").gameObject;

        // スライダー初期値の同期
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider").GetComponent<Slider>().value = floorBorderY;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider2").GetComponent<Slider>().value = floorDifference;
        RowImageObjects.transform.Find("H2").Find("Other").Find("DepthSlider").Find("min").GetComponent<Slider>().value = minDepth;
        RowImageObjects.transform.Find("H2").Find("Other").Find("DepthSlider").Find("max").GetComponent<Slider>().value = MaxDepth;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider").Find("valueText").GetComponent<TextMeshProUGUI>().text = floorBorderY.ToString();
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider2").Find("valueText").GetComponent<TextMeshProUGUI>().text = floorDifference.ToString();
        RowImageObjects.transform.Find("H2").Find("Other").Find("DepthSlider").Find("minText").GetComponent<TextMeshProUGUI>().text = minDepth.ToString();
        RowImageObjects.transform.Find("H2").Find("Other").Find("DepthSlider").Find("maxText").GetComponent<TextMeshProUGUI>().text = MaxDepth.ToString();

        sampleImage.texture = depthTexture;
        syncImage.texture = outputTexture;
        floorRGBImage.texture = floorRGBTexture;
        floorDepthImage.texture = floorDepthTexture;
        resultTexture = outputTexture;
        //RowImageObjects.transform.Find("H2").Find("R5").GetComponent<RawImage>().texture = resultTexture;

    }

    public void TakeFloorBorder()
    {
        Color32[] originalPixels = ((Texture2D)RGBImage.texture).GetPixels32();
        ushort[] originalDepthData = ((Texture2D)depthImage.texture).GetRawTextureData<ushort>().ToArray();
        Color32[] resultPixels = new Color32[originalPixels.Length];
        ushort[] resultDepthData = new ushort[originalDepthData.Length];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (inversionFloorY)
                {
                    int flippedY = height - 1 - y;
                    int i = flippedY * width + x;

                    if (y > floorBorderY)
                    {
                        // 床部分 → コピー
                        resultPixels[i] = originalPixels[i];
                        resultDepthData[i] = originalDepthData[i];
                    }
                    else if (y == floorBorderY || y + 1 == floorBorderY || y + 2 == floorBorderY || y + 3 == floorBorderY || y + 4 == floorBorderY || y + 5 == floorBorderY)
                    {
                        // 境界線 → 赤
                        resultPixels[i] = new Color32(255, 0, 0, 255);
                    }
                    else
                    {
                        // それより上 → 完全に透明
                        resultPixels[i] = new Color32(0, 0, 0, 0);
                        resultDepthData[i] = 0;
                    }
                }
                else
                {
                    int i = y * width + x;

                    if (y > floorBorderY)
                    {
                        // 床部分 → コピー
                        resultPixels[i] = originalPixels[i];
                        resultDepthData[i] = originalDepthData[i];
                    }
                    else if (y == floorBorderY || y + 1 == floorBorderY || y + 2 == floorBorderY || y + 3 == floorBorderY || y + 4 == floorBorderY || y + 5 == floorBorderY)
                    {
                        // 境界線 → 赤
                        resultPixels[i] = new Color32(255, 0, 0, 255);
                    }
                    else
                    {
                        // それより上 → 完全に透明
                        resultPixels[i] = new Color32(0, 0, 0, 0);
                        resultDepthData[i] = 0;
                    }
                }

            }
        }

        floorRGBTexture.SetPixels32(resultPixels);
        floorRGBTexture.Apply();
        var rawData = floorDepthTexture.GetRawTextureData<ushort>();
        NativeArray<ushort>.Copy(resultDepthData, rawData);
        floorDepthTexture.Apply();

    }
    private void Update()
    {
        ObjectGenerater();
    }
    void ObjectGenerater()
    {
        if (depthTexture == null)
            return;

        if (RGBImage == null || RGBImage.texture == null)
        {
            Debug.LogWarning("RGBImage またはそのテクスチャがセットされていません");
            return;
        }

        // R3 から青いピクセル情報を取得
        Color32[] depthPixels = depthTexture.GetPixels32(); // 青領域マスク（R3）
        Color32[] colorPixels = ((Texture2D)RGBImage.texture).GetPixels32(); // RGB画像
        Color32[] resultPixels = new Color32[depthPixels.Length];

        for (int i = 0; i < depthPixels.Length; i++)
        {
            // R3のピクセルが青（0, 0, 255）なら、R1のRGB値をそのまま使う
            if (depthPixels[i].b == 255 && depthPixels[i].r == 0 && depthPixels[i].g == 0 || (depthPixels[i].b == 0 && depthPixels[i].g == 255 && depthPixels[i].r == 0))
            {
                resultPixels[i] = colorPixels[i];
                resultPixels[i].a = 255; // 不透明
            }
            else
            {
                resultPixels[i] = new Color32(0, 0, 0, 0); // 完全に透明
            }
        }

        outputTexture.SetPixels32(resultPixels);
        outputTexture.Apply();
    }


    public void changed_floorBorderY(int value)
    {
        floorBorderY = (int)RowImageObjects.transform.Find("H2").Find("Other").Find("Slider").GetComponent<Slider>().value;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider").Find("valueText").GetComponent<TextMeshProUGUI>().text = floorBorderY.ToString();
    }
    public void changed_floorDifference(int value)
    {
        floorDifference = (int)RowImageObjects.transform.Find("H2").Find("Other").Find("Slider2").GetComponent<Slider>().value;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider2").Find("valueText").GetComponent<TextMeshProUGUI>().text = floorDifference.ToString();
    }
    public void changed_minDepth(int value)
    {
        minDepth = (int)RowImageObjects.transform.Find("H2").Find("Other").Find("DepthSlider").Find("min").GetComponent<Slider>().value;
        RowImageObjects.transform.Find("H2").Find("Other").Find("DepthSlider").Find("minText").GetComponent<TextMeshProUGUI>().text = minDepth.ToString();
    }
    public void changed_maxDepth(int value)
    {
        MaxDepth = (int)RowImageObjects.transform.Find("H2").Find("Other").Find("DepthSlider").Find("max").GetComponent<Slider>().value;
        RowImageObjects.transform.Find("H2").Find("Other").Find("DepthSlider").Find("maxText").GetComponent<TextMeshProUGUI>().text = MaxDepth.ToString();
    }

}
