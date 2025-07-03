using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class inSleep : MonoBehaviour
{
    [SerializeField, Header("���삪�Ȃ��Ƃ݂Ȃ����ԁi�b�j")]
    [Tooltip("���̕b���𒴂��ē��͂��Ȃ���Δ�A�N�e�B�u�ɂȂ�܂�")]
    private float inactivityThreshold = 5f;
    public float lastInputTime;            // �Ō�̓��͎���
    private bool isSleep = true;

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
        if (Time.time - lastInputTime >= inactivityThreshold && isSleep)
        {
            AdditiveSceneOverlay();
        }

        
    }

    public void set_isSleep(bool Bool)
    {
        isSleep = Bool;
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
