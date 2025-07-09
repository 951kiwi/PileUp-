using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using System;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.VisualScripting;
using Google.Protobuf.WellKnownTypes;
using static UnityEngine.GraphicsBuffer;

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
    public float nowObjectHeight = 0f; // 積まれた画像の最大高さ

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
    public GameObject nowRanking;
    public GameObject heightBarObject;

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
        Debug.Log(gameManager);
        //コルーチン動作
        //cameraDisplay.enabled = false;
        StartCoroutine(StartCountdown());
    }


    void Update()
    {
        
        //ゲームオーバー判定
        if (CheckGameOver(people) && people.Count > 0)
        {
            gameManager.maxScore = maxObjectHeight;
            ScreenShot screenshot = gameManager.gameObject.GetComponent<ScreenShot>();
            screenshot.setScreenshotSaved();
            gameManager.gameObject.GetComponent<RankingManager>().SaveDataAppend(gameManager.maxScore);
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
            //Debug.Log("撮影前カウントダウン中: " + countdownTime);
            countdownText.text = "人型撮影まで: " + countdownTime.ToString("F0");
            yield return new WaitForSeconds(1f);
            countdownTime--;
        }

        countdownText.text = "撮影中...";
        //Debug.Log("撮影中...");
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
        previewObject.transform.position = new Vector2(9.3f, nowObjectHeight + 15f);

        if (isCountingDown)
            yield break;

        isCountingDown = true;

        countdownTime = 1f; // カウントダウンをリセット

        while (countdownTime > 0)
        {
            //Debug.Log("落下前カウントダウン: " + countdownTime);
            countdownText.text = "落下まで: " + countdownTime.ToString("F0");
            yield return new WaitForSeconds(1f);
            countdownTime--;
        }

        // すべての積まれたオブジェクトの中で最も高いオブジェクトのY座標を取得
        if (people.Count > 0)
        {
            MaxHeightController();
            Vector3 mousePos = Input.mousePosition;
            Vector2 v2 = mainCamera.ScreenToWorldPoint(mousePos);

            if (previewObject != null)
            {
                previewObject.transform.position = v2;
            }
        }

        countdownText.text = "落下中...";
        //Debug.Log("落下中...");
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

            if (waitTime == 5)
            {
                // すべての積まれたオブジェクトの中で最も高いオブジェクトのY座標を取得
                if (people.Count > 0)
                {
                    MaxHeightController();
                    // カメラの高さを設定
                    StartCoroutine(SlideCameraToY(nowObjectHeight + 7, cameraRiseSpeed));

                    Vector2 v2 = new Vector2(5, nowObjectHeight + 15);

                    if (previewObject != null)
                    {
                        previewObject.transform.position = v2;
                    }

                    // RawImage の RectTransform を取得
                    RectTransform rectTransform = rawImage.GetComponent<RectTransform>();

                    // 現在の Y 座標を保持
                    float currentXPosition = rectTransform.anchoredPosition.x;

                    // 新しい位置を設定 (例: 新しい x 座標を指定し、y 座標はそのままにする)
                    Vector2 newPosition = new Vector2(currentXPosition, nowObjectHeight + 400);
                    rectTransform.anchoredPosition = newPosition;

                }

                // ここでスコアを加算
                score = maxObjectHeight;
                string scoreStr = scoreText.text.Replace("m", "");
                StartCoroutine(SlideHeightBarToY(nowObjectHeight,1f));
                float old_score = float.Parse(scoreStr);
                rankingController(score);

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
    public void HeightBarShow(bool flag)
    {
        if (flag)
        {
            heightBarObject.SetActive(true);
        }
        else
        {
            heightBarObject.SetActive(false);
        }
    }

    void MaxHeightController()
    {
        nowObjectHeight = people.Max(obj =>
        {
            var collider = obj.GetComponent<Collider2D>(); // or Collider for 3D
            return collider.bounds.max.y;
        });

        if(maxObjectHeight < nowObjectHeight)
        {
            maxObjectHeight = nowObjectHeight;
            gameManager.gameObject.GetComponent<ScreenShot>().setScreenShotTexture();
        }
        Debug.Log(nowObjectHeight);
    }
    int oldRanking = 1000;
    private void rankingController(float score)
    {
        RankingManager rankingManager = gameManager.gameObject.GetComponent<RankingManager>();
        int nowRankingnNum = rankingManager.GetNowRanking(score);
        Debug.Log(nowRankingnNum); 
        setRanking(oldRanking, nowRankingnNum);
        oldRanking = nowRankingnNum;
        void setRanking(int from, int to)
        {

            StartCoroutine(AnimateRankingCoroutine(from, to, 1.0f));

            IEnumerator AnimateRankingCoroutine(int from, int to, float duration)
            {
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    foreach (Transform child in nowRanking.transform)//子要素をすべて削除
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;

                    // イージング（smootherstep）
                    t = t * t * t * (t * (6f * t - 15f) + 10f);

                    int currentValue = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
                    foreach (char c in currentValue.ToString())
                    {
                        int digit = c - '0'; // 文字 → 数値
                        generateNumber(digit);
                    }
                    yield return null;
                }

                // 最後はピッタリ目標値に
                scoreText.text = to.ToString() + "m";
            }

            void generateNumber(int num)
            {
                float fixedHeight = 150f;
                string path = $"Number/{num}"; // 例: Resources/
                Sprite sprite = Resources.Load<Sprite>(path);
                GameObject imageObj = new GameObject("num", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                // Canvas の子として配置
                imageObj.transform.SetParent(nowRanking.transform, false);
                // Image コンポーネントにスプライトを設定
                Image imageComp = imageObj.GetComponent<Image>();
                imageComp.sprite = sprite;
                // アスペクト比計算（width ÷ height）
                float aspect = sprite.rect.width / sprite.rect.height;

                // 高さは固定、横幅はアスペクト比に基づいて計算
                float width = fixedHeight * aspect;
                RectTransform rt = imageObj.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(width, fixedHeight);
            }
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

    IEnumerator SlideHeightBarToY(float targetTopY, float duration)
    {
        float elapsed = 0f;
        Vector3 start = heightBarObject.transform.position;
        Vector3 end = new Vector3(start.x, targetTopY, start.z);
        Debug.Log("カメラ変動");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // イージング：イーズインアウト（加減速）
            t = t * t * (3f - 2f * t);  // SmoothStep

            heightBarObject.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        heightBarObject.transform.position = end; // 最後にピタリ
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
                MaxHeightController();

            }

            // プレビューオブジェクトの位置を固定
            if (previewObject != null)
            {
                previewObject.transform.position = new Vector3(9.3f, maxObjectHeight + 15, 0); // 固定位置に設定
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
