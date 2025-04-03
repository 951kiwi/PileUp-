using UnityEngine;
using System.IO;

public class ScreenShot : MonoBehaviour
{
    public static ScreenShot Instance { get; private set; }
    public Camera cameraToCapture;  // キャプチャするカメラ
    public string screenshotName;  // 保存するファイル名

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TakeScreenshot(Camera cameraToCapture)
    {
        // カメラの設定が無ければエラーメッセージを出す
        if (cameraToCapture == null)
        {
            Debug.LogError("カメラが設定されていません！");
            return;
        }

        // カメラの解像度に合わせたRenderTextureを作成
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cameraToCapture.targetTexture = renderTexture;

        // 一時的なTexture2Dを作成
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        // カメラでシーンをレンダリング
        cameraToCapture.Render();

        // レンダリングした内容をTexture2Dにコピー
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();

        // RenderTextureをリセット
        cameraToCapture.targetTexture = null;
        RenderTexture.active = null;
        Destroy(renderTexture);
        //スクリーンショットの名前を指定
        int nextID = RankingManager.Instance.nextGameDataID();  // 正しいメソッドの呼び出し
        screenshotName = $"{nextID}.png";

        // スクリーンショットをPNGとして保存
        byte[] bytes = screenShot.EncodeToPNG();
        string filePath = Path.Combine(Application.persistentDataPath, screenshotName);
        File.WriteAllBytes(filePath, bytes);
        Debug.Log("スクリーンショットが保存されました: " + filePath);
    }
}