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
        int displayIndex = 1; // Display3�i0-based index�j

        if (Display.displays.Length > displayIndex)
        {
            Display.displays[displayIndex].Activate(); // Display3��L����

            camera3.targetDisplay = displayIndex; // camera3 �� Display3 �Ɋ��蓖�āi�ی��j
            camera3.gameObject.SetActive(true);   // �J��������A�N�e�B�u�Ȃ�L����
            Debug.Log("Display 3 �� camera3 ��\�����܂���");
        }
        else
        {
            Debug.LogWarning($"Display {displayIndex + 1} �͎g�p�ł��܂���i���݂̕\����: {Display.displays.Length}�j");
        }
    }
}
