using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class inSleep : MonoBehaviour
{
    public float inactivityThreshold = 5f;  // 操作がないとみなす時間（秒）
    public float lastInputTime;            // 最後の入力時間

    // Start is called before the first frame update
    void Start()
    {
        lastInputTime = Time.time;  // ゲーム開始時に現在の時間を設定
                                    
    }

    // Update is called once per frame
    void Update()
    {
        // ユーザーが操作したかどうかを確認
        if (Input.anyKey || Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            lastInputTime = Time.time;  // 入力があったら最後の操作時間を更新
                                        // 入力があったら重ねたシーンを消す
            RemoveOverlayScene();
        }

        // 一定時間操作がなければシーンを重ねる
        if (Time.time - lastInputTime >= inactivityThreshold)
        {
            AdditiveSceneOverlay();
        }

        
    }
    // シーンを重ねる（Additiveシーンをロードする）
    void AdditiveSceneOverlay()
    {
        // 例としてシーン「OverlayScene」を重ねる
        if (!SceneManager.GetSceneByName("SleepShow").isLoaded)
        {
            SceneManager.LoadScene("SleepShow", LoadSceneMode.Additive);
        }
    }
    // シーンを消す（アンロードする）
    void RemoveOverlayScene()
    {
        // シーンがロードされている場合にアンロードする
        if (SceneManager.GetSceneByName("SleepShow").isLoaded)
        {
            SceneManager.UnloadSceneAsync("SleepShow");
        }
    }
}
