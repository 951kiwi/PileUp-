using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWindowCreater : MonoBehaviour
{
    public Camera camera3;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void windowShow()
    {
        int displayIndex = 1; // Display3（0-based index）

        if (Display.displays.Length > displayIndex)
        {
            Display.displays[displayIndex].Activate(); // Display3を有効化

            camera3.targetDisplay = displayIndex; // camera3 を Display3 に割り当て（保険）
            camera3.gameObject.SetActive(true);   // カメラが非アクティブなら有効に
            Debug.Log("Display 3 に camera3 を表示しました");
        }
        else
        {
            Debug.LogWarning($"Display {displayIndex + 1} は使用できません（現在の表示数: {Display.displays.Length}）");
        }
    }
}
