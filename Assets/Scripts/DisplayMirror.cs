using UnityEngine;
using UnityEngine.UI;

public class Display1Mirror : MonoBehaviour
{
    public RawImage rawImage;   // ミラー表示するRawImage
    private RenderTexture mirrorTexture;
    private Camera currentCamera;

    void Start()
    {
        SetupMirror();
    }

    void OnEnable()
    {
        // シーン遷移後に再取得
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    void OnSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
    {
        SetupMirror();
    }

    void SetupMirror()
    {
        currentCamera = Camera.main;
        if (currentCamera == null)
        {
            Debug.LogWarning("MainCamera not found in this scene.");
            return;
        }

        if (mirrorTexture != null)
        {
            mirrorTexture.Release();
        }

        mirrorTexture = new RenderTexture(Screen.width, Screen.height, 24);
        mirrorTexture.Create();

        currentCamera.targetTexture = mirrorTexture;
        rawImage.texture = mirrorTexture;
    }

    void OnDestroy()
    {
        if (mirrorTexture != null)
        {
            mirrorTexture.Release();
        }
    }
}
