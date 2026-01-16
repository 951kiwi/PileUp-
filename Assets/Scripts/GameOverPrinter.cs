using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPrint : MonoBehaviour
{
    public Text finalScoreText; // UI テキストを参照するためのフィールド
    public Text finalScoreRankingText; // UI テキストを参照するためのフィールド
    public Text Timer;
    public RawImage Screenshot;
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        // CreateManager から最終スコアを取得
        float finalScoreFormatted = gameManager.maxScore;
        // スコアを表示
        finalScoreText.text = $"Your Score: {finalScoreFormatted.ToString("F1")} M";
        finalScoreRankingText.text = $"あなたのランキングは {gameManager.gameObject.GetComponent<RankingManager>().GetNowRanking(finalScoreFormatted)} 位です！！";
        DateTime now = DateTime.Now;
        Timer.text = string.Format("{0}月{1}日 {2}時{3}分",
            now.Month,
            now.Day,
            now.Hour,
            now.Minute.ToString("D2")); // 2桁に整形
        if (gameManager.maxScoreScreenshot)
        {
            Screenshot.GetComponent<RawImage>().texture = gameManager.maxScoreScreenshot;
        }
    }

}
