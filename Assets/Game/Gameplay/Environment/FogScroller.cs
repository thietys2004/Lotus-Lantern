using UnityEngine;

public class FogScroller : MonoBehaviour
{
    [Header("Tốc độ trôi")]
    public float speedX = 0.5f;
    public float speedY = 0.2f;

    [Header("Khoảng cách Reset (Giữ cho sương không bay mất)")]
    public float resetDistance = 5f;

    private Vector3 startPos;

    void Start()
    {
        // Lưu lại vị trí ban đầu của sương mù
        startPos = transform.position;
    }

    void Update()
    {
        // Di chuyển đám sương mù từ từ
        transform.position += new Vector3(speedX, speedY, 0) * Time.deltaTime;

        // Nếu sương mù trôi đi quá xa (quá resetDistance), lập tức kéo nó về chỗ cũ
        // Vì ảnh Noise của bạn lặp lại lộn xộn, người chơi sẽ không nhận ra khoảnh khắc nó giật về.
        if (Vector3.Distance(startPos, transform.position) > resetDistance)
        {
            transform.position = startPos;
        }
    }
}
