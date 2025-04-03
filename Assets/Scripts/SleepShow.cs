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

    // フェード時間
    public float fadeDuration = 1f;
    public float displayDuration = 1f;

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
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);  // PNGデータをロードしてテクスチャに変換
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // 画像を設定
            imageDisplay.sprite = sprite;
            float ScoreData = RankingManager.Instance.LoadScoreData(int.Parse(fileName));
            string DateData = RankingManager.Instance.LoadDateData(int.Parse(fileName));
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
}
