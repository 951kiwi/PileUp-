using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SleepShow : MonoBehaviour
{   // �摜��\������Image�R���|�[�l���g
    public Image imageDisplay;
    public Text ScoreText;
    public Text DateText;
    public float ShowPlay = 1f;
    // �摜���t�F�[�h���邽�߂�CanvasGroup
    private CanvasGroup canvasGroup;

    // �摜�t�@�C���̃p�X
    private string[] imagePaths;

    // ���ݕ\�����Ă���摜�̃C���f�b�N�X
    private int currentImageIndex = 0;

    // �t�F�[�h����
    public float fadeDuration = 1f;
    public float displayDuration = 1f;

    void Start()
    {
        // Image�̐e��CanvasGroup��ǉ����ăt�F�[�h���ʂ��g����悤�ɂ���
        canvasGroup = imageDisplay.GetComponentInParent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = imageDisplay.gameObject.AddComponent<CanvasGroup>();
        }

        // Application.persistentDataPath����.png�t�@�C�����擾
        string directoryPath = Application.persistentDataPath;
        imagePaths = Directory.GetFiles(directoryPath, "*.png");

        // �摜��1�ȏ㑶�݂���ꍇ�A�X���C�h�V���[���J�n
        if (imagePaths.Length > 0)
        {
            StartCoroutine(SlideshowWithFade());
        }
        else
        {
            Debug.LogError("No .png files found in the persistent data path.");
        }
    }

    // �t�F�[�h���ʂ��������X���C�h�V���[
    private IEnumerator SlideshowWithFade()
    {
        while (true)
        {
            // �摜�̃p�X���擾
            string imagePath = imagePaths[currentImageIndex];

            // �摜�����[�h����Sprite�ɕϊ�
            byte[] imageData = File.ReadAllBytes(imagePath);
            // �t�@�C�����i�g���q�܂ށj���擾
            string fileName = Path.GetFileName(imagePath);
            fileName = fileName.Replace(".png", "");
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageData);  // PNG�f�[�^�����[�h���ăe�N�X�`���ɕϊ�
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // �摜��ݒ�
            imageDisplay.sprite = sprite;
            float ScoreData = RankingManager.Instance.LoadScoreData(int.Parse(fileName));
            string DateData = RankingManager.Instance.LoadDateData(int.Parse(fileName));
            // ����������� DateTime �^�ɕϊ�
            DateTime dateTime = DateTime.Parse(DateData);
            //�X�R�A��ݒ�
            ScoreText.text = $"Score : {ScoreData}M";
            //������ݒ�
            DateText.text = dateTime.ToString("MM��dd�� HH��mm��");


            // �t�F�[�h�C��
            yield return Fade(0f, 1f); // 0����1�փt�F�[�h�C��

            // �摜��\��������A�w�肵�����ԑҋ@
            yield return new WaitForSeconds(displayDuration);

            // �t�F�[�h�A�E�g
            yield return Fade(1f, 0f); // 1����0�փt�F�[�h�A�E�g

            // ���̉摜�C���f�b�N�X�ɍX�V�i�z�j
            currentImageIndex = (currentImageIndex + 1) % imagePaths.Length;
        }
    }

    // �t�F�[�h���ʂ��������郁�\�b�h
    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;

        // ���݂̃A���t�@�l�i�����x�j��startAlpha�ɐݒ�
        canvasGroup.alpha = startAlpha;

        // �t�F�[�h����������܂ŃA���t�@�l��ω�������
        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �ŏI�I�ȃA���t�@�l���m���ɐݒ�
        canvasGroup.alpha = endAlpha;
    }
}
