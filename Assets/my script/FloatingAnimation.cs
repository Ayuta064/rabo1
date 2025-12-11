using UnityEngine;

public class FloatingAnimation : MonoBehaviour
{
    [Header("浮遊の設定")]
    public float floatSpeed = 2.0f;  // 上下する速さ
    public float floatHeight = 0.05f; // 上下する幅 (5cm)

    [Header("回転の設定")]
    public float rotateSpeed = 50.0f; // 回る速さ (0なら回らない)

    private Vector3 startPos;

    void Start()
    {
        // 最初の位置を覚えておく
        startPos = transform.localPosition;
    }

    void Update()
    {
        // 1. フワフワ上下させる (Sin波を使う)
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        // 2. クルクル回す
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}