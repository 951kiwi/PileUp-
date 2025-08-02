using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class StartButtonTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    public Button StartButton;
    bool start = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !start)
        {
            if (StartButton != null)
            {
                StartButton.onClick.Invoke(); // ボタンクリックを擬似的に実行
                start = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) && start)
        {
            // 現在のシーンを再読み込み
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}
