using UnityEngine;
using System.IO;

public class ScreenShot : MonoBehaviour
{
    public static ScreenShot Instance { get; private set; }
    public Camera cameraToCapture;  // �L���v�`������J����
    public string screenshotName;  // �ۑ�����t�@�C����
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void setScreenShotTexture()
    {
        GameObject.Find("CreateManager").GetComponent<CreateManager>().HeightBarShow(false);
        // �J�����̐ݒ肪������΃G���[���b�Z�[�W���o��
        if (cameraToCapture == null)
        {
            Debug.LogError("�J�������ݒ肳��Ă��܂���I");
            return;
        }
        if (gameManager.maxScoreScreenshot != null)
        {
            Destroy(gameManager.maxScoreScreenshot);
        }

        // �J�����̉𑜓x�ɍ��킹��RenderTexture���쐬
        RenderTexture renderTexture = new RenderTexture(1080, 1920, 24);
        cameraToCapture.targetTexture = renderTexture;

        // �ꎞ�I��Texture2D���쐬
        Texture2D screenShot = new Texture2D(1080, 1920, TextureFormat.RGB24, false);

        // �J�����ŃV�[���������_�����O
        cameraToCapture.Render();

        // �����_�����O�������e��Texture2D�ɃR�s�[
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0);
        screenShot.Apply();

        gameManager.maxScoreScreenshot = screenShot;

        // RenderTexture�����Z�b�g
        cameraToCapture.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        GameObject.Find("CreateManager").GetComponent<CreateManager>().HeightBarShow(true);
    }
    public void setScreenshotSaved()
    {
        Texture2D screenShot = gameManager.maxScoreScreenshot;
        int nextID = gameManager.gameObject.GetComponent<RankingManager>().nextGameDataID();  // ���������\�b�h�̌Ăяo��
        screenshotName = $"{nextID}.png";

        // �X�N���[���V���b�g��PNG�Ƃ��ĕۑ�
        byte[] bytes = screenShot.EncodeToPNG();
        string filePath = Path.Combine(Application.persistentDataPath, screenshotName);
        File.WriteAllBytes(filePath, bytes);
        Debug.Log("�X�N���[���V���b�g���ۑ�����܂���: " + filePath);
    }

    public void TakeScreenshot(Camera cameraToCapture)
    {
        // �J�����̐ݒ肪������΃G���[���b�Z�[�W���o��
        if (cameraToCapture == null)
        {
            Debug.LogError("�J�������ݒ肳��Ă��܂���I");
            return;
        }

        // �J�����̉𑜓x�ɍ��킹��RenderTexture���쐬
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cameraToCapture.targetTexture = renderTexture;

        // �ꎞ�I��Texture2D���쐬
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        // �J�����ŃV�[���������_�����O
        cameraToCapture.Render();

        // �����_�����O�������e��Texture2D�ɃR�s�[
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();

        // RenderTexture�����Z�b�g
        cameraToCapture.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        //�X�N���[���V���b�g�̖��O���w��
        int nextID = gameManager.gameObject.GetComponent<RankingManager>().nextGameDataID();  // ���������\�b�h�̌Ăяo��
        screenshotName = $"{nextID}.png";

        // �X�N���[���V���b�g��PNG�Ƃ��ĕۑ�
        byte[] bytes = screenShot.EncodeToPNG();
        string filePath = Path.Combine(Application.persistentDataPath, screenshotName);
        File.WriteAllBytes(filePath, bytes);
        Debug.Log("�X�N���[���V���b�g���ۑ�����܂���: " + filePath);
    }
}