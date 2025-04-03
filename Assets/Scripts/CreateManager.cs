using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using System;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateManager : MonoBehaviour
{
    private Vector2 lastFacePosition = Vector2.zero;
    private float smoothingFactor = 0.5f; // スムージングの係数
    public GameObject MaskPreviewObject;
    private SpriteRenderer maskSpriteRenderer;
    public TextAsset faces;
    public RectTransform canvasRectTransform;
     private CascadeClassifier cascadeFaces;
      public RawImage rawImage; // RawImage UI 要素　撮影前
      public RawImage cameraDisplay; // カメラ映像を表示する RawImage
      public Image Warning;//警告表示の背景
    public WebCamTexture webCamTexture; // WebCamTextureを使ってカメラ映像を取得
     private bool isCountdownActive = false; // カウントダウンがアクティブかどうかを示すフラグ
     
    private Texture2D previewTexture;
    public static float FinalResult = 0;//最終結果
    public static string FinalformattedNumber; 
    private Mat backgroundMat; // 背景画像のMatを保持
    private Mat initialFrame;
    private Mat diffMat;

    // カメラとプレビューオブジェクトの上昇速度
    public float cameraRiseSpeed = 0.1f;
    // カメラの調整に関する変数
    public float cameraOffset = 5f; // カメラが積まれた画像の上に位置するオフセット
    public float previewObjectOffset = 0.5f; // プレビューオブジェクトのオフセット

    private GameObject obj;
    public List<GameObject> people;
    public bool isFall;
    int file_length;
    public float pivotHeight = 15; // 生成位置の基準
    public Camera mainCamera;
    private Vector3 initialCameraPosition;
    public GameObject cameracontroller;

    Texture2D dstTexture;
    public Sprite capturedSprite;
    bool isBackgroundCaptured = false; // 背景画像がキャプチャされたかを示すフラグ
    public float maxObjectHeight = 0f; // 積まれた画像の最大高さ

    // スコア表示用
    public Text scoreText;
    private float score = 0;
    private HashSet<GameObject> scoredAnimals = new HashSet<GameObject>();

    private bool isPreviewing = false; // プレビュー中を管理するフラグ
    private GameObject previewObject; // プレビュー用オブジェクトを保持

    private Dictionary<GameObject, Vector2> lastPosition = new Dictionary<GameObject, Vector2>(); // 動物の最後の位置を記録
    private bool isAnyAnimalMoving = false; // どれかの動物が動いているかどうかを記録

    // カウントダウン機能のための変数
    private float countdownTime = 2f;
    private float countdownTime2 = 1f;
    public float captureDelay = 1f; // 背景撮影までの遅延時間（秒）
    private bool isCountingDown = false;
    private bool DropCountDownStart = false;
    public Text countdownText;
    public Text countdownTextBackground;     // 背景撮影指示メッセージ
    public Text countdownTextBackground2;     // 背景撮影指示カウントダウン
    public Camera ScreenShotCamera;

    //SE再生用変数
    public AudioSource audioSource;
    public AudioSource camera;
    public AudioSource count;

    void Init()
    {
        string[] files = Directory.GetFiles(
            @"Assets/Resources", "*.png", SearchOption.AllDirectories
            ).ToArray();
        file_length = files.Length;
    }
    void Awake()
    {
        if (webCamTexture)
        {
            webCamTexture.Stop();
        }
        // classifier
        FileStorage storageFaces = new FileStorage(faces.text, FileStorage.Mode.Read | FileStorage.Mode.Memory);
        cascadeFaces = new CascadeClassifier();
        if (!cascadeFaces.Read(storageFaces.GetFirstTopLevelNode()))
        {
            throw new System.Exception("CascadeRecognizer.Initialize: Failed to load faces cascade classifier");
        }
    }
    void Start()
    {

        // 初期化処理
    initialCameraPosition = mainCamera.transform.position;


    // Webカメラの開始
        InitWebCam();

        backgroundMat = new Mat();
        initialFrame = new Mat();
        diffMat = new Mat();
    
        Init();

 // 15秒後に自動で撮影を開始
 
        StartCoroutine(DelayedCaptureAndCountdown(0f));
    }

     IEnumerator DelayedCaptureAndCountdown(float delay)
    {
         isCountdownActive = true;
        yield return new WaitForSeconds(delay);

while (countdownTime2 > 0)
        {
            countdownTextBackground.text = $"背景の撮影をします！カメラ外に出てください！";
            countdownTextBackground2.text = $"撮影まで\n   {countdownTime2.ToString("F0")} 秒";
            yield return new WaitForSeconds(1f);
            countdownTime2--;
        }
        count.Play();//開始のSE

        CaptureBackground();
         // カメラ映像の RawImage を非表示にする
    if (cameraDisplay != null)
    {
        cameraDisplay.enabled = false; // RawImage を非表示にする
        countdownTextBackground.enabled = false;
        countdownTextBackground2.enabled = false;
        Warning.enabled = false;
    }
         isCountdownActive = false;

        StartCoroutine(StartCountdown());
    }

   void Update()
{
    if (CheckGameOver(people))
    {
        if (people.Count > 0)
        {
        maxObjectHeight = people.Max(obj => obj.transform.position.y + obj.GetComponent<SpriteRenderer>().bounds.size.y / 2);
        FinalResult = maxObjectHeight;
        FinalformattedNumber = FinalResult.ToString("F2"); 
        }
        StopCamera();
            ScreenShot screenshot = GetComponent<ScreenShot>();
            screenshot.TakeScreenshot(ScreenShotCamera);
            SceneManager.LoadScene("GameOver");
    }

    if (!DropCountDownStart)
    {
        Mat srcMat = OpenCvSharp.Unity.TextureToMat(this.webCamTexture);

        Mat grayMat = new Mat();
        Cv2.CvtColor(srcMat, grayMat, ColorConversionCodes.BGR2GRAY);

        if (!initialFrame.Empty())
        {
            Cv2.Absdiff(grayMat, initialFrame, diffMat);
            Mat binaryMat = new Mat();
            Cv2.Threshold(diffMat, binaryMat, 70, 255, ThresholdTypes.Binary);
            ReduceNoise(binaryMat);
            Mat resultMat = new Mat();
            Cv2.BitwiseAnd(srcMat, srcMat, resultMat, binaryMat);
            AdjustSaturation(resultMat, 1.0);
            Cv2.CvtColor(resultMat, resultMat, ColorConversionCodes.RGB2RGBA);
            Mat[] rgbaChannels = Cv2.Split(resultMat);
            rgbaChannels[3] = binaryMat;
            Cv2.Merge(rgbaChannels, resultMat);

            if (this.dstTexture == null || this.dstTexture.width != resultMat.Width || this.dstTexture.height != resultMat.Height)
            {
                this.dstTexture = new Texture2D(resultMat.Width, resultMat.Height, TextureFormat.RGBA32, false);
            }
            OpenCvSharp.Unity.MatToTexture(resultMat, this.dstTexture);
            capturedSprite = Sprite.Create(this.dstTexture, new UnityEngine.Rect(0, 0, this.dstTexture.width, this.dstTexture.height), Vector2.zero);

            if (rawImage != null)
            {
                rawImage.texture = this.dstTexture;
            }
        }
    }

    if (this.webCamTexture == null || this.webCamTexture.width <= 16 || this.webCamTexture.height <= 16) return;

}


   private void CaptureBackground()
    {
        if (!isBackgroundCaptured)
        {
            backgroundMat = OpenCvSharp.Unity.TextureToMat(this.webCamTexture);
            Cv2.CvtColor(backgroundMat, initialFrame, ColorConversionCodes.BGR2GRAY);
            isBackgroundCaptured = true;
        }
    }

    IEnumerator StartCountdown()
{
    isCountingDown = true;

    while (countdownTime > 0)
    {
        Debug.Log("撮影前カウントダウン中: " + countdownTime);
        countdownText.text = "人型撮影まで: " +  countdownTime.ToString("F0");
        yield return new WaitForSeconds(1f);
        countdownTime--;
    }

    countdownText.text = "撮影中...";
    Debug.Log("撮影中...");
    CaptureImage(); // 画像を撮影
    camera.Play();
    isCountingDown = false;

    // 画像を撮影した後、すぐに落下カウントダウンを開始
    if (!CheckGameOver(people))
    {
        // RawImage を非表示にする
if (rawImage != null)
{
    rawImage.gameObject.SetActive(false);
}
        DropCountDownStart = true; // 落下前カウントダウン開始
        StartCoroutine(StartDropCountdown());
    }
}

   IEnumerator StartDropCountdown()
{
    // プレビューオブジェクトの位置を更新
        previewObject.transform.position = new Vector2(9, maxObjectHeight + 15);

    if (isCountingDown)
        yield break;

    isCountingDown = true;

    countdownTime = 1f; // カウントダウンをリセット

    while (countdownTime > 0)
    {
        Debug.Log("落下前カウントダウン: " + countdownTime);
        countdownText.text = "落下まで: " +  countdownTime.ToString("F0");
        yield return new WaitForSeconds(1f);
        countdownTime--;
    }

    // すべての積まれたオブジェクトの中で最も高いオブジェクトのY座標を取得
    if (people.Count > 0)
        {
        maxObjectHeight = people.Max(obj => obj.transform.position.y + obj.GetComponent<SpriteRenderer>().bounds.size.y / 2);
         // カメラの高さを設定
        Vector3 cameraPosition = mainCamera.transform.position;
        cameraPosition.y = maxObjectHeight + 7; // 適切なオフセットを追加
        mainCamera.transform.position = cameraPosition;

        Vector2 v2 = new Vector2(mainCamera.ScreenToWorldPoint(Input.mousePosition).x, 15 + maxObjectHeight);

        if (previewObject != null)
        {
            previewObject.transform.position = v2;
        }
        }

    countdownText.text = "落下中...";
    Debug.Log("落下中...");
    StartDrop(); // 落下を開始
    audioSource.Play();//落下のSE

    isCountingDown = false;

    // 次の撮影までの待機時間
    float waitTime = 7f; // 10秒の待機時間

   // RawImage を表示する
if (rawImage != null)
{
    rawImage.gameObject.SetActive(true);
}
    DropCountDownStart = false; // 落下前カウントダウン終了


    while (waitTime > 0)
    {
        countdownText.text = "次の人型撮影まで: " + waitTime.ToString("F0") + "秒";
        yield return new WaitForSeconds(1f);

        if(waitTime == 5){
             // すべての積まれたオブジェクトの中で最も高いオブジェクトのY座標を取得
    if (people.Count > 0)
        {
        maxObjectHeight = people.Max(obj => obj.transform.position.y + obj.GetComponent<SpriteRenderer>().bounds.size.y / 2);
         // カメラの高さを設定
        Vector3 cameraPosition = mainCamera.transform.position;
        cameraPosition.y = maxObjectHeight + 7; // 適切なオフセットを追加
        mainCamera.transform.position = cameraPosition;

        Vector2 v2 = new Vector2(5, 15 + maxObjectHeight);

        if (previewObject != null)
        {
            previewObject.transform.position = v2;
        }

        // RawImage の RectTransform を取得
            RectTransform rectTransform = rawImage.GetComponent<RectTransform>();

            // 現在の Y 座標を保持
            float currentXPosition = rectTransform.anchoredPosition.x;

            // 新しい位置を設定 (例: 新しい x 座標を指定し、y 座標はそのままにする)
            Vector2 newPosition = new Vector2(currentXPosition, maxObjectHeight + 400);
            rectTransform.anchoredPosition = newPosition;
        
        }

        // ここでスコアを加算
        score = maxObjectHeight;
        string formattedNumber = score.ToString("F2"); 
        scoreText.text = "Score: " + formattedNumber + "M";
        }

        waitTime--;
    }


    // 待機時間後に撮影カウントダウンを開始
    if (!CheckGameOver(people))
    {
        StartCoroutine(StartCountdown());
}
}

    void CaptureImage()
{
    Mat srcMat = OpenCvSharp.Unity.TextureToMat(this.webCamTexture);

    Mat grayMat = new Mat();
    Cv2.CvtColor(srcMat, grayMat, ColorConversionCodes.BGR2GRAY);

    if (!initialFrame.Empty())
    {
        Cv2.Absdiff(grayMat, initialFrame, diffMat);
            Mat binaryMat = new Mat();
            Cv2.Threshold(diffMat, binaryMat, 70, 255, ThresholdTypes.Binary);
            ReduceNoise(binaryMat);
            Mat resultMat = new Mat();
            Cv2.BitwiseAnd(srcMat, srcMat, resultMat, binaryMat);
            AdjustSaturation(resultMat, 1.0);
            Cv2.CvtColor(resultMat, resultMat, ColorConversionCodes.RGB2RGBA);
            Mat[] rgbaChannels = Cv2.Split(resultMat);
            rgbaChannels[3] = binaryMat;
            Cv2.Merge(rgbaChannels, resultMat);

            if (this.dstTexture == null || this.dstTexture.width != resultMat.Width || this.dstTexture.height != resultMat.Height)
            {
                this.dstTexture = new Texture2D(resultMat.Width, resultMat.Height, TextureFormat.RGBA32, false);
            }
            OpenCvSharp.Unity.MatToTexture(resultMat, this.dstTexture);
            capturedSprite = Sprite.Create(this.dstTexture, new UnityEngine.Rect(0, 0, this.dstTexture.width, this.dstTexture.height), Vector2.zero);

        CreatePreviewObject(capturedSprite);

        grayMat.Dispose();
        binaryMat.Dispose();
        resultMat.Dispose();
    }
}

void CreatePreviewObject(Sprite img)
{
    isPreviewing = true;
    previewObject = new GameObject("PreviewObject");

    // テクスチャのコピーを作成
    Texture2D textureCopy = new Texture2D(img.texture.width, img.texture.height, img.texture.format, false);
    textureCopy.SetPixels(img.texture.GetPixels());
    textureCopy.Apply();

    // 新しいスプライトを作成
    Sprite newSprite = Sprite.Create(textureCopy, new UnityEngine.Rect(0, 0, textureCopy.width, textureCopy.height), Vector2.zero);

    // スプライトレンダラーを追加してスプライトを設定
    SpriteRenderer spriteRenderer = previewObject.AddComponent<SpriteRenderer>();
    spriteRenderer.sprite = newSprite;

    // サイズのスケールを設定（例: 0.5倍のサイズにする）
    previewObject.transform.localScale = new Vector3(-2.5f, 2.5f, 1f);

    // PolygonColliderを追加し、画像の透明部分を無視する
    PolygonCollider2D polygonCollider = previewObject.AddComponent<PolygonCollider2D>();
    polygonCollider.isTrigger = false; // 必要に応じて変更

    PhysicsMaterial2D highFrictionMaterial = Resources.Load<PhysicsMaterial2D>("New Physic Material");
    if (highFrictionMaterial != null)
    {
        polygonCollider.sharedMaterial = highFrictionMaterial;
    }

    // Rigidbody2Dを追加
    Rigidbody2D rb2D = previewObject.AddComponent<Rigidbody2D>();
    rb2D.gravityScale = 0.0f; // 初期値として重力を無効にする
    rb2D.isKinematic = false; // 物理シミュレーションを有効にする
}


void StartDrop()
{
    if (previewObject != null)
    {
        var rb2D = previewObject.GetComponent<Rigidbody2D>();
        if (rb2D != null)
        {
            rb2D.gravityScale = 1.0f; // 重力を有効にする
            rb2D.mass = 10f; // 適切な質量値を設定
            rb2D.isKinematic = false; // 物理シミュレーションを有効にする
            rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 衝突検出モードを Continuous に設定
        }
        else
        {
            Debug.LogError("Rigidbody2D が見つかりません。");
        }

        // 積まれた画像の中で最も高いオブジェクトのY座標を取得
        if (people.Count > 0)
        {
            maxObjectHeight = people.Max(obj => obj.transform.position.y + obj.GetComponent<SpriteRenderer>().bounds.size.y / 2);
            
            // カメラの高さを設定
            Vector3 cameraPosition = mainCamera.transform.position;
            cameraPosition.y = maxObjectHeight + 7; // 適切なオフセットを追加
            mainCamera.transform.position = cameraPosition;
        }

        // プレビューオブジェクトの位置を固定
        if (previewObject != null)
        {
            previewObject.transform.position = new Vector3(9, maxObjectHeight + 15, 0); // 固定位置に設定
        }

        people.Add(previewObject);
        previewObject = null;
    }
    countdownText.text = ""; // カウントダウン表示をリセット
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
void ReduceNoise(Mat binaryMat)
{
    // モルフォロジー演算に使うカーネルのサイズ
    int kernelSize = 3; // 適切なサイズを選択

    // カーネルの作成
    Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(kernelSize, kernelSize));

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
}
public Vector2 GetFacePosition()
{
    if (cascadeFaces == null) return Vector2.zero;

    WebCamTexture input = FindObjectOfType<CreateManager>().webCamTexture;
    Mat image = OpenCvSharp.Unity.TextureToMat(input);
    Mat gray = image.CvtColor(ColorConversionCodes.BGR2GRAY);
    Cv2.EqualizeHist(gray, gray);
    OpenCvSharp.Rect[] rawFaces = cascadeFaces.DetectMultiScale(gray, 1.1, 6);

    if (rawFaces.Length > 0)
    {
        var face = rawFaces[0];
        var cx = face.TopLeft.X + (face.Width / 2f);
        var cy = face.TopLeft.Y + (face.Height / 2f);

        Vector2 detectedPosition = new Vector2(cx / gray.Width, 1 - cy / gray.Height);
        // 平滑化
        lastFacePosition = Vector2.Lerp(lastFacePosition, detectedPosition, smoothingFactor);
        return lastFacePosition;
    }

    return Vector2.zero;
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

    // RawImage にカメラ映像を設定
    if (cameraDisplay != null)
    {
        cameraDisplay.texture = webCamTexture;
    }
}

    // ゲームオブジェクトが非アクティブ化されたときに呼ばれる
    private void OnDisable()
    {
        StopCamera();
    }
    // シーン移行時に呼ばれる
    private void OnDestroy()
    {
        StopCamera();
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
}
