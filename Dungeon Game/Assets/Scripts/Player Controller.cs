using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oyuncu karakterini kontrol eden script.
/// Birinci şahıs hareket, kamera kontrolü ve zıplama mekanikleri içerir.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;        // Hareket hızı
    public float rotationSpeed = 10f;   // Dönüş hızı
    public float jumpForce = 5f;        // Zıplama kuvveti
    public float gravity = 20f;         // Yerçekimi kuvveti
    
    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f; // Yerden yükseklik kontrolü için mesafe
    public LayerMask groundLayer;           // Yer katmanı
    
    private CharacterController characterController; // Karakter kontrolcüsü bileşeni
    private Vector3 moveDirection = Vector3.zero;  // Hareket yönü vektörü
    private float rotationY = 0f;                 // Y ekseninde dönüş değeri
    private bool isGrounded;                      // Karakterin yerde olup olmadığı
    
    [Header("Camera")]
    public Transform cameraTransform;         // Kamera transform referansı 
    public float mouseSensitivity = 2f;       // Fare hassasiyeti
    private float cameraVerticalAngle = 0f;   // Kamera dikey bakış açısı
    
    /// <summary>
    /// Başlangıç işlevi. Gerekli bileşenleri başlatır ve imleci yapılandırır.
    /// </summary>
    void Start()
    {
        // Karakter kontrol bileşenini al
        characterController = GetComponent<CharacterController>();
        
        // Kamera referansı atanmamışsa, ana kamerayı bulmaya çalış
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
        }
        
        // İmleci kilitle ve gizle (birinci şahıs kontrolü için)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    /// <summary>
    /// Her karede çağrılır. Hareket ve kamera kontrollerini yönetir.
    /// </summary>
    void Update()
    {
        // Yer kontrolü - karakter yerde mi?
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        
        // Fare ile kamera döndürme
        RotateCamera();
        
        // WASD/Ok tuşları ile hareket
        HandleMovement();
    }
    
    /// <summary>
    /// Fare ile kamera rotasyonunu kontrol eder.
    /// Yatay hareket karakteri döndürür, dikey hareket sadece kamerayı döndürür.
    /// </summary>
    void RotateCamera()
    {
        if (cameraTransform == null) return;
        
        // Fare girdisini al
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Karakteri Y ekseni etrafında döndür (sağa/sola)
        transform.Rotate(Vector3.up * mouseX);
        
        // Kamerayı X ekseni etrafında döndür (yukarı/aşağı)
        cameraVerticalAngle -= mouseY;
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -80f, 80f); // Dikey açıyı sınırla
        cameraTransform.localRotation = Quaternion.Euler(cameraVerticalAngle, 0f, 0f);
    }
    
    /// <summary>
    /// Karakter hareketini kontrol eder.
    /// WASD/Ok tuşları, zıplama ve yerçekimi etkilerini yönetir.
    /// </summary>
    void HandleMovement()
    {
        // Eğer karakterimiz yerdeyse ve düşüyorsa, düşme hızını sıfırla
        if (isGrounded && moveDirection.y < 0)
        {
            moveDirection.y = -0.5f;
        }
        
        // Hareket girdilerini al
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        // Karakterin baktığı yöne göre hareket yönünü hesapla
        Vector3 forward = transform.forward * verticalInput;
        Vector3 right = transform.right * horizontalInput;
        Vector3 desiredMoveDirection = (forward + right).normalized;
        
        // Hareket uygula
        if (desiredMoveDirection.magnitude > 0.1f)
        {
            // Baktığımız yönde hareket et
            Vector3 move = desiredMoveDirection * moveSpeed;
            moveDirection.x = move.x;
            moveDirection.z = move.z;
        }
        else
        {
            // Girdi yoksa, yavaşla
            moveDirection.x = 0f;
            moveDirection.z = 0f;
        }
        
        // Zıplama
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            moveDirection.y = jumpForce;
        }
        
        // Yerçekimi uygula
        moveDirection.y -= gravity * Time.deltaTime;
        
        // Karakteri hareket ettir
        characterController.Move(moveDirection * Time.deltaTime);
    }
    
    /// <summary>
    /// İmlec kilitleme durumunu açıp kapatır.
    /// UI etkileşimi için kullanılır.
    /// </summary>
    /// <param name="isLocked">İmlecin kilitli olup olmayacağı</param>
    public void ToggleCursorLock(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}