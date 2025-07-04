using UnityEngine;

public class Stone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision Stone");
        LivingBeing enemy = collision.gameObject.GetComponentInParent<LivingBeing>();
        if (enemy != null)
        {
            Debug.Log("Enemy Hit!");
            enemy.TakeDamage(8);
        }
    }
}
