using UnityEngine;
using System.Collections; 

public class BasicEnemy : MonoBehaviour
{
    public float health = 10f;
    public float moveSpeed = 2f;

    public Animator animator;

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
        animator.SetTrigger("Die"); 

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
