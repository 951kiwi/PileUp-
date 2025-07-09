using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SleepShow : MonoBehaviour
{   // 画像を表示するImageコンポーネント
    public Image imageDisplay;
    public Text ScoreText;
    public Text DateText;
    public float ShowPlay = 1f;
    // 画像をフェードするためのCanvasGroup
    private CanvasGroup canvasGroup;

    // 画像ファイルのパス
    private string[] imagePaths;

    // 現在表示している画像のインデックス
    private int currentImageIndex = 0;
    Texture2D texture;

    // フェード時間
    public float fadeDuration = 1f;
    public float displayDuration = 1f;
    private GameManager gameManager;
    public GameObject nowRanking;

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    void Start()
    {
        // Imageの親にCanvasGroupを追加してフェード効果を使えるようにする
        canvasGroup = imageDisplay.GetComponentInParent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = imageDisplay.gameObject.AddComponent<CanvasGroup>();
        }

        // Application.persistentDataPath内の.pngファイルを取得
        string directoryPath = Application.persistentDataPath;
        imagePaths = Directory.GetFiles(directoryPath, "*.png");

        // 画像が1つ以上存在する場合、スライドショーを開始
        if (imagePaths.Length > 0)
        {
            StartCoroutine(SlideshowWithFade());
        }
        else
        {
            Debug.LogError("No .png files found in the persistent data path.");
        }
    }

    // フェード効果を加えたスライドショー
    private IEnumerator SlideshowWithFade()
    {
        while (true)
        {
            // 画像のパスを取得
            string imagePath = imagePaths[currentImageIndex];

            // 画像をロードしてSpriteに変換
            byte[] imageData = File.ReadAllBytes(imagePath);
            // ファイル名（拡張子含む）を取得
            string fileName = Path.GetFileName(imagePath);
            fileName = fileName.Replace(".png", "");
            texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);  // PNGデータをロードしてテクスチャに変換
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // 画像を設定
            imageDisplay.sprite = sprite;
            float ScoreData = gameManager.gameObject.GetComponent<RankingManager>().LoadScoreData(int.Parse(fileName));
            Debug.Log(ScoreData);
            string DateData = gameManager.gameObject.GetComponent<RankingManager>().LoadDateData(int.Parse(fileName));
            rankingController(ScoreData);
            // 日時文字列を DateTime 型に変換
            DateTime dateTime = DateTime.Parse(DateData);
            //スコアを設定
            ScoreText.text = $"Score : {ScoreData}M";
            //時刻を設定
            DateText.text = dateTime.ToString("MM月dd日 HH時mm分");


            // フェードイン
            yield return Fade(0f, 1f); // 0から1へフェードイン

            // 画像を表示した後、指定した時間待機
            yield return new WaitForSeconds(displayDuration);

            // フェードアウト
            yield return Fade(1f, 0f); // 1から0へフェードアウト

            // 次の画像インデックスに更新（循環）
            currentImageIndex = (currentImageIndex + 1) % imagePaths.Length;
            //画像のtextureを削除
            UnityEngine.Object.Destroy(texture);
        }
    }
    private void rankingController(float score)
    {
        RankingManager rankingManager = gameManager.gameObject.GetComponent<RankingManager>();
        int nowRankingnNum = rankingManager.GetNowRanking(score);
        Debug.Log(nowRankingnNum);
        setRanking( nowRankingnNum);
        void setRanking(int to)
        {
            foreach (Transform child in nowRanking.transform)//子要素をすべて削除
            {
                GameObject.Destroy(child.gameObject);
            }

            foreach (char c in nowRankingnNum.ToString())
                    {
                        int digit = c - '0'; // 文字 → 数値
                        generateNumber(digit);
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

    // フェード効果を実装するメソッド
    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        // 現在のアルファ値（透明度）をstartAlphaに設定
        canvasGroup.alpha = startAlpha;

        // フェードが完了するまでアルファ値を変化させる
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 最終的なアルファ値を確実に設定
        canvasGroup.alpha = endAlpha;
    }
    private void OnDestroy()
    {
        if (texture != null)
        {
            UnityEngine.Object.Destroy(texture);
            texture = null;
        }
    }
}
