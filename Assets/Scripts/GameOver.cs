using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public static bool isGameOver = false;
    public float delayBeforeReturnToTitle = 7f; // タイトル画面に戻るまでの時間
    public Text finalScoreText; // UI テキストを参照するためのフィールド
    public Text finalScoreRankingText; // UI テキストを参照するためのフィールド
    public Image timerBarImage;  // Fill Image を指定
    public RawImage Screenshot;
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
         // CreateManager から最終スコアを取得
        float finalScoreFormatted = gameManager.maxScore;
         // スコアを表示
        finalScoreText.text = $"Final Score: {finalScoreFormatted.ToString("F1")} M";
        finalScoreRankingText.text = $"あなたのランキングは {gameManager.gameObject.GetComponent<RankingManager>().GetNowRanking(finalScoreFormatted)} 位です！！";
        if (gameManager.maxScoreScreenshot)
        {
            Screenshot.GetComponent<RawImage>().texture = gameManager.maxScoreScreenshot;
        }

        StartCoroutine(ScrollCountTimer());
        StartCoroutine(ReturnToTitleAfterDelay());
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

        }
    }

      IEnumerator ReturnToTitleAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeReturnToTitle);
        SceneManager.LoadScene("GameTitle"); // タイトル画面のシーン名に変更
    }
    IEnumerator ScrollCountTimer()
    {
        float elapsed = 0f;

        while (elapsed < delayBeforeReturnToTitle)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / delayBeforeReturnToTitle);
            timerBarImage.fillAmount = 1f - t; // 右から左へ減る

            yield return null;
        }

        timerBarImage.fillAmount = 0f;
    }
}
