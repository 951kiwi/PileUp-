using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static bool isCameraCollision = false; // カメラ衝突フラグ
     public static bool isCollision = false; // 衝突フラグを追加

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 衝突時の処理
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            isCameraCollision = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        // 衝突終了時の処理
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            isCameraCollision = false;
        }
    }
}
