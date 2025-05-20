using UnityEngine;

public class Rotator : MonoBehaviour
{
    // Dönme hızı (derece/saniye)
    public float speed = 180f;

    void Update()
    {
        // Her frame, Y ekseninde döndür
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
    }
}
