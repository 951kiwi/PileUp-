using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public static bool isGameOver = false;
    public float delayBeforeReturnToTitle = 5f; // タイトル画面に戻るまでの時間
    public Text finalScoreText; // UI テキストを参照するためのフィールド

    void Start()
    {
         // CreateManager から最終スコアを取得
        string finalScoreFormatted = CreateManager.FinalformattedNumber;
         // スコアを表示
        finalScoreText.text = $"Final Score: {finalScoreFormatted} M";
        RankingManager.Instance.SaveDataAppend(float.Parse(finalScoreFormatted));
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
}
