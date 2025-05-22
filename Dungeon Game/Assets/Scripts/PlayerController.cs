using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [Tooltip("Karakterin yürüyüş hızı")]
    public float moveSpeed = 5f;

    [Tooltip("Karakterin dönüşüm yumuşaklığı (saniye cinsinden)")]
    public float turnSmoothTime = 0.1f;

    // Dönüş hızı referansını SmoothDampAngle ile paylaşacağız
    private float turnSmoothVelocity;

    // Karakteri hareket etmek için
    private CharacterController controller;

    // Gerçek kamera (CinemachineBrain tarafından kontrol edilen)
    private Camera mainCam;

    void Awake()
    {
        // CharacterController bileşenini al
        controller = GetComponent<CharacterController>();
        // Sahnedeki ana kamerayı al
        mainCam = Camera.main;
        // Fare imlecini ortala ve gizle
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 1) Klavye input'unu oku (A/D veya Sol/Sağ ve W/S veya Yukarı/Aşağı)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        // Yalnızca bir miktar input varsa işle
        if (inputDir.magnitude >= 0.1f)
        {
            // 2) Kameranın yatay düzlemde baktığı yönü al
            Vector3 camForward = mainCam.transform.forward;
            camForward.y = 0f;          // Dikey bileşeni sıfırla
            camForward.Normalize();     // Normalize et

            Vector3 camRight = mainCam.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            // 3) Input yönünü kamera eksenine dönüştür
            //    Böylece "ilerle" demek, "kamera baktığı yönde ilerle" olur
            Vector3 moveDir = camForward * v + camRight * h;

            // — 4) Sadece ileri (v>0) veya strafing (h≠0) varsa döndür —
            //     Böylece geri (S) tuşuna basınca karakter arkasına bakmadan
            //     geriye doğru hareket eder, dönmez.
            if (v > 0.1f || Mathf.Abs(h) > 0.1f)
            {
                // moveDir’in ZOX düzlemindeki açısını al (radyan→derece)
                float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;

                // Mevcut açıyı yumuşakça targetAngle’a kaydır
                float smoothAngle = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    targetAngle,
                    ref turnSmoothVelocity,
                    turnSmoothTime
                );
                transform.rotation = Quaternion.Euler(0, smoothAngle, 0);
            }

            // — 5) Karakteri hareket ettir —
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
        }
    }
}