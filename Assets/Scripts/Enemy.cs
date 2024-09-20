using UnityEngine;
using System.Collections;

public class BasicEnemy : MonoBehaviour
{
    public float health = 10f;
    public float moveSpeed = 2f;

    public Animator animator;

    public AudioClip deathSound;
    private AudioSource audioSource;

    private bool isDead = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


    void Update()
    {
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }
        isDead = true;
        animator.SetTrigger("Die");
        if (deathSound != null && audioSource.clip != null)
        {
            audioSource.clip = deathSound;
            audioSource.loop = false;
            audioSource.Play();        
        }
        StartCoroutine(WaitForDeathAnimation());
    }

    private IEnumerator WaitForDeathAnimation()
    {
        AnimatorStateInfo animationState = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = animationState.length;

        yield return new WaitForSeconds(animationLength);

        Destroy(gameObject);
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
    }
}
