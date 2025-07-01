using UnityEngine;

public class Fireball : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float speed = 40f;
    private Vector3 direction;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        LivingBeing b = collision.gameObject.GetComponent<LivingBeing>();
        if (b != null)
        {
            b.TakeDamage(10);
        }
        
        Destroy(gameObject);
    }
}
