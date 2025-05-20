using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Trap : MonoBehaviour
{
    [Tooltip("Bu tuzak tetiklendiğinde vereceği hasar miktarı")]
    public int damage = 20;

    void Awake()
    {
        // Collider'i otomatik trigger yao
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    // Bir başka Collider bu tetikleyiciye girdğinde çağrılır
    private void OnTriggerEnter(Collider other)
    {
        // Sadece Player tag'ine sahip objelere etki et
        if (!other.CompareTag("Player")) return;

        // Player'ın Health component'ini al
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            // Hasarı uygula
            health.TakeDamage(damage);
        }

        // Tuzak tek kullanımlık, sahneden sil
        Destroy(gameObject);
    }
}
