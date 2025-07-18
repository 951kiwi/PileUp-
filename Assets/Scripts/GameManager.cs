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

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField, Header("デバッグ")]
    private GameObject DebugObject;
    private GameObject RowImageObjects;
    private int cameraIndex;

    public WebCamTexture webCamTexture; // WebCamTextureを使ってカメラ映像を取得

    bool isBackgroundCaptured = false; // 背景画像がキャプチャされたかを示すフラグ

    public int sabun1 = 40;
    public int fallKernelSize = 5;
    public int NoiseKernelSize = 3;

    public float maxScore;
    public Texture2D maxScoreScreenshot;

    // フィールド変数に追加しておく
    private Texture2D backgroundTexture, binaryTexture, initialFrameTexture, diffTexture;
    public Texture2D resultTexture;
    Texture2D binaryTexture1, binaryTexture2, binaryTexture3, binaryTexture4, binaryTexture5, binaryTexture6;



    private Pipeline pipeline;
    public Texture2D depthTexture;
    private Texture2D Cam_ColorTexture;
    public RawImage Cam_ColorImage;
    public RawImage Cam_displayImage;
    public int width = 640;
    public int height = 480;
    public float maxDistance = 2.0f; // 距離の最大値（2mなど）
    private Align align;


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
        if (webCamTexture)
        {
            webCamTexture.Stop();
        }
    }


    void Start()
    {

        depthTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        Cam_ColorTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Cam_displayImage.texture = depthTexture;
        Cam_ColorImage.texture = Cam_ColorTexture;

        backgroundTexture = new Texture2D(640, 480, TextureFormat.RGBA32, false);
        resultTexture = new Texture2D(640, 480, TextureFormat.RGBA32, false);
        binaryTexture = new Texture2D(640, 480, TextureFormat.RGBA32, false);
        binaryTexture1 = new Texture2D(640, 480, TextureFormat.RGBA32, false);
        binaryTexture2 = new Texture2D(640, 480, TextureFormat.RGBA32, false);
        binaryTexture3 = new Texture2D(640, 480, TextureFormat.RGBA32, false);
        binaryTexture4 = new Texture2D(640, 480, TextureFormat.RGBA32, false);
        binaryTexture5 = new Texture2D(640, 480, TextureFormat.RGBA32, false);
        binaryTexture6 = new Texture2D(640, 480, TextureFormat.RGBA32, false);
        initialFrameTexture = new Texture2D(640, 480, TextureFormat.RGBA32, false);
        diffTexture = new Texture2D(640, 480, TextureFormat.RGBA32, false);


        RowImageObjects = DebugObject.transform.Find("DebugCanvas").Find("RowImage").gameObject;

        // スライダー初期値の同期
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider").GetComponent<Slider>().value = sabun1;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider2").GetComponent<Slider>().value = NoiseKernelSize;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider3").GetComponent<Slider>().value = fallKernelSize;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider").Find("valueText").GetComponent<TextMeshProUGUI>().text = sabun1.ToString();
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider2").Find("valueText").GetComponent<TextMeshProUGUI>().text = NoiseKernelSize.ToString();
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider3").Find("valueText").GetComponent<TextMeshProUGUI>().text = fallKernelSize.ToString();

        //RowImageObjects.transform.Find("H1").Find("R1").GetComponent<RawImage>().texture = backgroundTexture;
        //RowImageObjects.transform.Find("H1").Find("R2").GetComponent<RawImage>().texture = webCamTexture;
        //RowImageObjects.transform.Find("H1").Find("R3").GetComponent<RawImage>().texture = diffTexture;
        //RowImageObjects.transform.Find("H2").Find("H3").Find("H1").Find("R5").GetComponent<RawImage>().texture = binaryTexture;
        //RowImageObjects.transform.Find("H2").Find("H3").Find("H1").Find("R6").GetComponent<RawImage>().texture = binaryTexture1;
        //RowImageObjects.transform.Find("H2").Find("H3").Find("H1").Find("R7").GetComponent<RawImage>().texture = binaryTexture2;
        //RowImageObjects.transform.Find("H2").Find("H3").Find("H2").Find("R8").GetComponent<RawImage>().texture = binaryTexture3;
        //RowImageObjects.transform.Find("H2").Find("H3").Find("H2").Find("R9").GetComponent<RawImage>().texture = binaryTexture4;
        //RowImageObjects.transform.Find("H2").Find("H3").Find("H2").Find("R10").GetComponent<RawImage>().texture = binaryTexture5;
        //RowImageObjects.transform.Find("H2").Find("R5").GetComponent<RawImage>().texture = resultTexture;


        // 全ピクセルを緑に
        Color32[] greenPixels = new Color32[640 * 480];
        for (int i = 0; i < greenPixels.Length; i++)
        {
            greenPixels[i] = new Color32(0, 255, 0, 255); // RGBA = 緑
        }

        backgroundTexture.SetPixels32(greenPixels);
        backgroundTexture.Apply();
        Debug.Log("aaaaaaaaaaaaaaaaaaaaaaa");
    }


    void InitRealSenceCam()
    {
        pipeline = new Pipeline();
        var cfg = new Config();
        cfg.EnableStream(Stream.Depth, 640, 480, Format.Z16, 30);
        cfg.EnableStream(Stream.Color, 640, 480, Format.Rgba8, 30);
        try
        {
            pipeline.Start(cfg);
        }
        catch (Exception e)
        {
            Debug.LogError("RealSense Start failed: " + e.Message);
        }
    }


    public void changed_Sabun1(int value)
    {
        sabun1 = (int)RowImageObjects.transform.Find("H2").Find("Other").Find("Slider").GetComponent<Slider>().value;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider").Find("valueText").GetComponent<TextMeshProUGUI>().text = sabun1.ToString();
    }
    public void changed_Sabun2(int value)
    {
        NoiseKernelSize = (int)RowImageObjects.transform.Find("H2").Find("Other").Find("Slider2").GetComponent<Slider>().value;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider2").Find("valueText").GetComponent<TextMeshProUGUI>().text = NoiseKernelSize.ToString();
    }
    public void changed_Sabun3(int value)
    {
        fallKernelSize = (int)RowImageObjects.transform.Find("H2").Find("Other").Find("Slider3").GetComponent<Slider>().value;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider3").Find("valueText").GetComponent<TextMeshProUGUI>().text = fallKernelSize.ToString();
    }

}
