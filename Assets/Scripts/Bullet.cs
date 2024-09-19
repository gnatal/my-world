using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f; // How long the bullet lasts before being destroyed
    public int damage = 1; // The damage the bullet inflicts

    void Start()
    {
        // Destroy the bullet after a set time to avoid memory leaks
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move the bullet forward continuously
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Add collision logic here, for example, if the bullet hits an enemy
        if (collision.CompareTag("Enemy"))
        {
            // Assume the enemy has a script with a TakeDamage method
            // collision.GetComponent<Enemy>().TakeDamage(damage);
            
            // Destroy the bullet upon collision
            Destroy(gameObject);
        }
        
        if (collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
