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
    public RawImage rawImage; // RawImage UI 要素　撮影前
    public RawImage cameraDisplay; // カメラ映像を表示する RawImage
    public WebCamTexture webCamTexture; // WebCamTextureを使ってカメラ映像を取得

    public static float FinalResult = 0;//最終結果
    public static string FinalformattedNumber;

    // カメラとプレビューオブジェクトの上昇速度
    [SerializeField,Tooltip("カメラが上昇する速さ(秒)")]
    private float cameraRiseSpeed = 1f;
    // カメラの調整に関する変数

    public List<GameObject> people;
    public float pivotHeight = 15; // 生成位置の基準
    public Camera mainCamera;

    Texture2D dstTexture;
    private Sprite capturedSprite;
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
    private float backgroundCountdown = 5f;// 背景撮影までの遅延時間（秒）
    private bool isCountingDown = false;
    private bool DropCountDownStart = false;
    public Text countdownText;
    public Camera ScreenShotCamera;

    //SE再生用変数
    public AudioSource audioSource;
    public AudioSource camera;
    public AudioSource count;

    private GameManager gameManager;

    void Awake()
    {
    }
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //コルーチン動作
        //cameraDisplay.enabled = false;
        StartCoroutine(StartCountdown());
    }


    void Update()
    {
        //ゲームオーバー判定
        if (CheckGameOver(people) && people.Count > 0)
        {
            FinalResult = maxObjectHeight;
            FinalformattedNumber = FinalResult.ToString("F2");
            ScreenShot screenshot = GetComponent<ScreenShot>();
            screenshot.TakeScreenshot(ScreenShotCamera);
            SceneManager.LoadScene("GameOver");
        }

        if (rawImage != null)
        {
            rawImage.texture = gameManager.resultTexture;
        }
    }

    IEnumerator StartCountdown()
    {
        isCountingDown = true;

        while (countdownTime > 0)
        {
            Debug.Log("撮影前カウントダウン中: " + countdownTime);
            countdownText.text = "人型撮影まで: " + countdownTime.ToString("F0");
            yield return new WaitForSeconds(1f);
            countdownTime--;
        }

        countdownText.text = "撮影中...";
        Debug.Log("撮影中...");
        CaptureImage(); // 画像を撮影
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
        previewObject.transform.position = new Vector2(9.3f, maxObjectHeight + 19f);

        if (isCountingDown)
            yield break;

        isCountingDown = true;

        countdownTime = 1f; // カウントダウンをリセット

        while (countdownTime > 0)
        {
            Debug.Log("落下前カウントダウン: " + countdownTime);
            countdownText.text = "落下まで: " + countdownTime.ToString("F0");
            yield return new WaitForSeconds(1f);
            countdownTime--;
        }

        // すべての積まれたオブジェクトの中で最も高いオブジェクトのY座標を取得
        if (people.Count > 0)
        {
            maxObjectHeight = people.Max(obj => obj.transform.position.y + obj.GetComponent<SpriteRenderer>().bounds.size.y / 2);
            // カメラの高さを設定
            Vector3 cameraPosition = mainCamera.transform.position;
            // 目標位置（Y座標だけ変更）
            Vector3 targetPos = new Vector3(cameraPosition.x, maxObjectHeight + 15, cameraPosition.z);
            // 徐々に近づく（0.1fは補間速度。値を調整して滑らかさを制御）
            mainCamera.transform.position = Vector3.Lerp(cameraPosition, targetPos, Time.deltaTime * 3f);

            Vector3 mousePos = Input.mousePosition;
            Vector2 v2 = mainCamera.ScreenToWorldPoint(mousePos);
            v2.y = maxObjectHeight + 15;

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
        float waitTime = 7f; // 7秒の待機時間

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

            if (waitTime == 6)
            {
                // すべての積まれたオブジェクトの中で最も高いオブジェクトのY座標を取得
                if (people.Count > 0)
                {
                    maxObjectHeight = people.Max(obj => obj.transform.position.y + obj.GetComponent<SpriteRenderer>().bounds.size.y / 2);
                    // カメラの高さを設定
                    StartCoroutine(SlideCameraToY(maxObjectHeight + 15, cameraRiseSpeed));

                    Vector2 v2 = new Vector2(5, 7 + maxObjectHeight);

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
                string scoreStr = scoreText.text.Replace("m", "");
                float old_score = float.Parse(scoreStr);
                StartCoroutine(AnimateScoreCoroutine(old_score, score, 3.0f));
            }

            waitTime--;
        }


        // 待機時間後に撮影カウントダウンを開始
        if (!CheckGameOver(people))
        {
            StartCoroutine(StartCountdown());
        }
    }
    IEnumerator SlideCameraToY(float targetY, float duration)
    {
        float elapsed = 0f;
        Vector3 start = mainCamera.transform.position;
        Vector3 end = new Vector3(start.x, targetY, start.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // イージング：イーズインアウト（加減速）
            t = t * t * (3f - 2f * t);  // SmoothStep

            mainCamera.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        mainCamera.transform.position = end; // 最後にピタリ
    }

    IEnumerator AnimateScoreCoroutine(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // イージング（加減速）をかけたいならここでtを変形：
            t = t * t * t * (t * (6f * t - 15f) + 10f);  // smootherstep

            float currentValue = Mathf.Lerp(from, to, t);
            scoreText.text = currentValue.ToString("F2") + "m";
            yield return null;
        }

        // 最後はピッタリ目標値に
        scoreText.text = to.ToString("F2") + "m";
    }

    void CaptureImage()
    {
        Mat resultMat = gameManager.resultMat;

        if (this.dstTexture == null || this.dstTexture.width != resultMat.Width || this.dstTexture.height != resultMat.Height)
        {
            this.dstTexture = new Texture2D(resultMat.Width, resultMat.Height, TextureFormat.RGBA32, false);
        }
        OpenCvSharp.Unity.MatToTexture(resultMat, this.dstTexture);
        capturedSprite = Sprite.Create(this.dstTexture, new UnityEngine.Rect(0, 0, this.dstTexture.width, this.dstTexture.height), Vector2.zero);

        CreatePreviewObject(capturedSprite);
        

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
        previewObject.transform.localScale = new Vector3(-2.9f, 2.9f, 1f);

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
                cameraPosition.y = maxObjectHeight + 15; // 適切なオフセットを追加
                mainCamera.transform.position = cameraPosition;
            }

            // プレビューオブジェクトの位置を固定
            if (previewObject != null)
            {
                previewObject.transform.position = new Vector3(9.3f, maxObjectHeight + 19, 0); // 固定位置に設定
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
}
