using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class inSleep : MonoBehaviour
{
    public float inactivityThreshold = 5f;  // ���삪�Ȃ��Ƃ݂Ȃ����ԁi�b�j
    public float lastInputTime;            // �Ō�̓��͎���

    // Start is called before the first frame update
    void Start()
    {
        lastInputTime = Time.time;  // �Q�[���J�n���Ɍ��݂̎��Ԃ�ݒ�
                                    
    }

    // Update is called once per frame
    void Update()
    {
        // ���[�U�[�����삵�����ǂ������m�F
        if (Input.anyKey || Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            lastInputTime = Time.time;  // ���͂���������Ō�̑��쎞�Ԃ��X�V
                                        // ���͂���������d�˂��V�[��������
            RemoveOverlayScene();
        }

        // ��莞�ԑ��삪�Ȃ���΃V�[�����d�˂�
        if (Time.time - lastInputTime >= inactivityThreshold)
        {
            AdditiveSceneOverlay();
        }

        
    }
    // �V�[�����d�˂�iAdditive�V�[�������[�h����j
    void AdditiveSceneOverlay()
    {
        // ��Ƃ��ăV�[���uOverlayScene�v���d�˂�
        if (!SceneManager.GetSceneByName("SleepShow").isLoaded)
        {
            SceneManager.LoadScene("SleepShow", LoadSceneMode.Additive);
        }
    }
    // �V�[���������i�A�����[�h����j
    void RemoveOverlayScene()
    {
        // �V�[�������[�h����Ă���ꍇ�ɃA�����[�h����
        if (SceneManager.GetSceneByName("SleepShow").isLoaded)
        {
            SceneManager.UnloadSceneAsync("SleepShow");
        }
    }
}
