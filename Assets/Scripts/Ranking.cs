using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

public class Ranking : MonoBehaviour
{
    [SerializeField, Header("数値")]
    string finalScoreFormatted;

    string[] ranking = { "1", "2", "3", "4", "5" };
    float[] rankingValue = new float[5];

    [SerializeField, Header("表示させるテキスト")]
    Text[] rankingText = new Text[5];

    void Start()
    {
        GetRanking();

        // CreateManager から最終スコアを取得して表示
        if (CreateManager.FinalformattedNumber != null && !string.IsNullOrEmpty(CreateManager.FinalformattedNumber))
        {
            finalScoreFormatted = CreateManager.FinalformattedNumber;
            Debug.Log("Final Score: " + finalScoreFormatted); // ログに出力
        }
        SetRanking(float.Parse(finalScoreFormatted));

        // ランキングの表示
        for (int i = 0; i < rankingText.Length; i++)
        {
            rankingText[i].text = ranking[i] + ": " + rankingValue[i].ToString("F2") + "M";
        }
    }

    /// <summary>
    /// ランキング呼び出し
    /// </summary>
    void GetRanking()
    {
        for (int i = 0; i < ranking.Length; i++)
        {
            // 以前のスコアを float として取得
            rankingValue[i] = PlayerPrefs.GetFloat(ranking[i], 0.00f);
            Debug.Log(rankingValue[i]);
        }
    }

    /// <summary>
    /// ランキング書き込み
    /// </summary>
    void SetRanking(float newValue)
    {
        // 新しいスコアを含める
        var updatedRanking = rankingValue.Append(newValue).ToArray();

        // スコアを降順にソートし、トップ5を取得
        rankingValue = updatedRanking.OrderByDescending(v => v).Take(5).ToArray();

        // ソートされたスコアをPlayerPrefsに保存
        for (int i = 0; i < ranking.Length; i++)
        {
            PlayerPrefs.SetFloat(ranking[i], rankingValue[i]);
        }
    }
}
