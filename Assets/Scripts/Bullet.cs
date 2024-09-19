using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f; // How long the bullet lasts before being destroyed
    public int damage = 1; // The damage the bullet inflicts
    private Vector2 direction; // The direction the bullet should move

    void Start()
    {
        // Destroy the bullet after a set time to avoid memory leaks
        Rigidbody2D bulletRb = GetComponent<Rigidbody2D>();
        bulletRb.isKinematic = true;  // Bullet is not affected by physics forces
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move the bullet in the direction it was fired
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized; // Normalize to ensure consistent speed
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Add collision logic here, for example, if the bullet hits an enemy
        if (collision.CompareTag("Enemy"))
        {
            // Assume the enemy has a script with a TakeDamage method
            collision.GetComponent<BasicEnemy>().TakeDamage(damage);

            // Destroy the bullet upon collision
            Destroy(gameObject);
        }

        if (collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
