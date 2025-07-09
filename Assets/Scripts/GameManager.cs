using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using System;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Google.Protobuf.WellKnownTypes;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField, Header("デバッグ")]
    private GameObject DebugObject;
    private GameObject RowImageObjects;

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
    public Mat grayMat, binaryMat, noiseMat;
    public Mat resultMat;
    private Mat backgroundMat, initialFrame, diffMat, binaryMat1, binaryMat2, binaryMat3, binaryMat4, binaryMat5, binaryMat6;
    private Mat[] rgbaChannels = new Mat[4];


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
        InitWebCam();
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
        
        // Mat 初期化
        backgroundMat = new Mat();
        initialFrame = new Mat();
        diffMat = new Mat();
        grayMat = new Mat();
        binaryMat = new Mat();
        binaryMat1 = new Mat();
        binaryMat2 = new Mat();
        binaryMat3 = new Mat();
        binaryMat4 = new Mat();
        binaryMat5 = new Mat();
        binaryMat6 = new Mat();
        noiseMat = new Mat();
        resultMat = new Mat();

        CaptureBackground();

        RowImageObjects = DebugObject.transform.Find("DebugCanvas").Find("RowImage").gameObject;

        // スライダー初期値の同期
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider").GetComponent<Slider>().value = sabun1;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider2").GetComponent<Slider>().value = NoiseKernelSize;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider3").GetComponent<Slider>().value = fallKernelSize;
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider").Find("valueText").GetComponent<TextMeshProUGUI>().text = sabun1.ToString();
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider2").Find("valueText").GetComponent<TextMeshProUGUI>().text = NoiseKernelSize.ToString();
        RowImageObjects.transform.Find("H2").Find("Other").Find("Slider3").Find("valueText").GetComponent<TextMeshProUGUI>().text = fallKernelSize.ToString();

        RowImageObjects.transform.Find("H1").Find("R1").GetComponent<RawImage>().texture = backgroundTexture;
        RowImageObjects.transform.Find("H1").Find("R2").GetComponent<RawImage>().texture = webCamTexture;
        RowImageObjects.transform.Find("H1").Find("R3").GetComponent<RawImage>().texture = diffTexture;
        RowImageObjects.transform.Find("H2").Find("H3").Find("H1").Find("R5").GetComponent<RawImage>().texture = binaryTexture;
        RowImageObjects.transform.Find("H2").Find("H3").Find("H1").Find("R6").GetComponent<RawImage>().texture = binaryTexture1;
        RowImageObjects.transform.Find("H2").Find("H3").Find("H1").Find("R7").GetComponent<RawImage>().texture = binaryTexture2;
        RowImageObjects.transform.Find("H2").Find("H3").Find("H2").Find("R8").GetComponent<RawImage>().texture = binaryTexture3;
        RowImageObjects.transform.Find("H2").Find("H3").Find("H2").Find("R9").GetComponent<RawImage>().texture = binaryTexture4;
        RowImageObjects.transform.Find("H2").Find("H3").Find("H2").Find("R10").GetComponent<RawImage>().texture = binaryTexture5;
        RowImageObjects.transform.Find("H2").Find("R5").GetComponent<RawImage>().texture = resultTexture;


        // 全ピクセルを緑に
        Color32[] greenPixels = new Color32[640 * 480];
        for (int i = 0; i < greenPixels.Length; i++)
        {
            greenPixels[i] = new Color32(0, 255, 0, 255); // RGBA = 緑
        }

        backgroundTexture.SetPixels32(greenPixels);
        backgroundTexture.Apply();
    }

    void Update()
    {
        // カメラ準備チェック
        if (this.webCamTexture == null || this.webCamTexture.width <= 16 || this.webCamTexture.height <= 16)
            return;

        // フレーム取得と前処理
        Mat srcMat = OpenCvSharp.Unity.TextureToMat(this.webCamTexture);
        Cv2.CvtColor(srcMat, grayMat, ColorConversionCodes.BGR2GRAY);

        if (!initialFrame.Empty())
        {
            Cv2.Absdiff(grayMat, initialFrame, diffMat);
            Cv2.Threshold(diffMat, binaryMat, sabun1, 255, ThresholdTypes.Binary);

            // ノイズをモルフォロジーで除去
            binaryMat1 = ReduceNoise(binaryMat);

            binaryMat2 = binaryMat1;
            //穴埋め
            binaryMat3 =  CloseSmallHoles(binaryMat2);

            // マスク適用
            Cv2.BitwiseAnd(srcMat, srcMat, resultMat, binaryMat3);

            // 彩度調整
            AdjustSaturation(resultMat, 1.0);

            // RGBAに変換し、アルファにbinaryMatを設定
            Cv2.CvtColor(resultMat, resultMat, ColorConversionCodes.RGB2RGBA);
            Cv2.Split(resultMat, out rgbaChannels);
            rgbaChannels[3] = binaryMat3;
            Cv2.Merge(rgbaChannels, resultMat);


            // binaryTexture 初期化＆更新
            if (binaryTexture == null || binaryTexture.width != binaryMat.Width || binaryTexture.height != binaryMat.Height)
            {
                if (binaryTexture != null) Destroy(binaryTexture);
                binaryTexture = new Texture2D(binaryMat.Width, binaryMat.Height, TextureFormat.RGBA32, false);
            }
            

            // resultTexture 初期化＆更新
            if (resultTexture == null || resultTexture.width != resultMat.Width || resultTexture.height != resultMat.Height)
            {
                if (resultTexture != null) Destroy(resultTexture);
                resultTexture = new Texture2D(resultMat.Width, resultMat.Height, TextureFormat.RGBA32, false);
            }



            OpenCvSharp.Unity.MatToTexture(binaryMat, binaryTexture);
            OpenCvSharp.Unity.MatToTexture(binaryMat1, binaryTexture1);
            OpenCvSharp.Unity.MatToTexture(binaryMat2, binaryTexture2);
            OpenCvSharp.Unity.MatToTexture(binaryMat3, binaryTexture3);
            OpenCvSharp.Unity.MatToTexture(resultMat, resultTexture);
            OpenCvSharp.Unity.MatToTexture(diffMat, diffTexture);
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


    public void CaptureBackground()
    {

        backgroundMat = OpenCvSharp.Unity.TextureToMat(this.webCamTexture);
        OpenCvSharp.Unity.MatToTexture(backgroundMat, backgroundTexture);
        Cv2.CvtColor(backgroundMat, initialFrame, ColorConversionCodes.BGR2GRAY);

        isBackgroundCaptured = true;
    }

    Mat SmoothMask(Mat binaryMat)
    {
        int kernelSize = 5; // 3〜7くらいで試してみて
        Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(kernelSize, kernelSize));
        Mat result = new Mat();
        // モルフォロジー処理「クロージング」: 膨張 → 収縮
        Cv2.MorphologyEx(binaryMat, result, MorphTypes.Close, kernel);

        kernel.Dispose();
        return result;
    }

    bool CheckGameOver(List<GameObject> animals)
    {
        foreach (var animal in animals)
        {
            if (animal != null && animal.transform.position.y < -10)
            {
                return true;
            }
        }
        return false;
    }

    void AdjustSaturation(Mat imgMat, double scale)
    {
        // BGR から HSV へ変換
        Mat hsvMat = new Mat();
        Cv2.CvtColor(imgMat, hsvMat, ColorConversionCodes.BGR2HSV);

        // 彩度チャンネルを取得
        Mat[] hsvChannels = Cv2.Split(hsvMat);
        hsvChannels[1] *= scale; // 彩度チャンネルのスケーリング

        // HSV チャンネルをマージして画像を再構築
        Mat adjustedHsvMat = new Mat();
        Cv2.Merge(hsvChannels, adjustedHsvMat);

        // HSV から BGR へ戻す
        Cv2.CvtColor(adjustedHsvMat, imgMat, ColorConversionCodes.HSV2BGR);

        hsvMat.Dispose();
        adjustedHsvMat.Dispose();
    }
    // ノイズを減らすための処理
    Mat ReduceNoise(Mat binaryMat)
    {
        // モルフォロジー演算に使うカーネルのサイズ

        // カーネルの作成
        Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(NoiseKernelSize, NoiseKernelSize));

        // 収縮処理
        Mat erodedMat = new Mat();
        Cv2.Erode(binaryMat, erodedMat, kernel, iterations: 1);

        // 膨張処理
        Mat dilatedMat = new Mat();
        Cv2.Dilate(erodedMat, dilatedMat, kernel, iterations: 1);

        // 処理結果を使用する
        // ... (例えば、結果を表示したり、他の処理に渡したり)

        // リソースの解放
        kernel.Dispose();
        erodedMat.Dispose();

        // 結果を binaryMat にコピー
        return dilatedMat;

    }
    Mat CloseSmallHoles(Mat binaryMat)
    {
        Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(fallKernelSize, fallKernelSize));
        Mat result = new Mat();
        Cv2.MorphologyEx(binaryMat, result, MorphTypes.Close, kernel);
        kernel.Dispose();
        return result;
    }
    void InitWebCam()
    {
        if (webCamTexture)
        {
            webCamTexture.Stop();
        }
        WebCamDevice[] devices = WebCamTexture.devices;
        bool cameraFound = false;

        foreach (var device in devices)
        {
            Debug.Log("Available camera: " + device.name);

            if (device.name == "HD Webcam eMeet C960")
            {
                webCamTexture = new WebCamTexture(device.name, 640, 480, 30);
                cameraFound = true;
                break;
            }
        }

        // 指定されたカメラが見つからない場合、内蔵カメラを使用する
        if (!cameraFound)
        {
            if (devices.Length > 0)
            {
                // 最初のカメラを選択（通常は内蔵カメラ）
                webCamTexture = new WebCamTexture(devices[0].name, 640, 480, 30);
                Debug.Log("外付けカメラが見つからなかったため、内蔵カメラを使用します。");
            }
            else
            {
                Debug.LogError("カメラが接続されていません。");
                return;
            }
        }

        webCamTexture.Play();
    }

    // アプリケーションが終了する直前に呼ばれる
    private void OnApplicationQuit()
    {
        StopCamera();
    }

    // カメラ停止処理
    private void StopCamera()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
            Debug.Log("カメラを停止しました。");
        }
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (webCamTexture != null && !webCamTexture.isPlaying)
        {
            webCamTexture.Play();
            Debug.Log("シーン遷移後にカメラを再起動しました。");
        }
    }
}
