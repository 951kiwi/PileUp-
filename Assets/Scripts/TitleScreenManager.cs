using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TitleScreenManager : MonoBehaviour
{
    public GameObject rulesPanel; // ルール説明用の Panel
    public Text countdownText;    // カウントダウンを表示する Text
    public Button startButton;    // ゲームスタートボタン

    private void Start()
    {
        // ゲームスタートボタンのクリックイベントに StartGame メソッドを追加
        startButton.onClick.AddListener(StartGame);
        // ルール説明パネルを非表示に設定
        rulesPanel.SetActive(false);
    }

    private void StartGame()
    {
        // ルール説明パネルを表示
        rulesPanel.SetActive(true);
        // カウントダウンを開始
        StartCoroutine(CountdownAndStartGame());
        //スリープを無効化
        this.GetComponent<inSleep>().set_isSleep(false);
    }

    private IEnumerator CountdownAndStartGame()
    {
        int countdownTime = 7; // カウントダウンの開始時間（秒）

        while (countdownTime > 0)
        {
            countdownText.text = $"ゲーム開始まで: {countdownTime}秒"; // カウントダウンの値を更新
            yield return new WaitForSeconds(1f); // 1秒待つ
            countdownTime--;
        }
        
        while (countdownTime > 0)
        {
            countdownText.text = countdownTime.ToString(); // カウントダウンの値を更新
            yield return new WaitForSeconds(1f); // 1秒待つ
            countdownTime--;
        }
        
        // ゲームシーンをロード
        SceneManager.LoadScene("Main"); // "GameSceneName" は実際のシーン名に置き換えてください

        // ルール説明パネルを非表示に設定
        rulesPanel.SetActive(false);
    }
}
