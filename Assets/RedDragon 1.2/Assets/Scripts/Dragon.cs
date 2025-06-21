using System;
using Unity.VisualScripting;
using UnityEngine;
public class Dragon : MonoBehaviour, LivingBeing
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private float rotspeed = 80;
    private bool isFlying = true;
    private bool target_locked = false;
    bool isAlive = true;
    Rigidbody rb;
    bool isAttacking = false;
    private Animator anim;

    private float health = 100;

    private string[] animatedStates = {"IdleSimple", "IdleAgressive", "Walk",
    "IdleRestless", "BattleStance", "Bite", "Drakaris", "FlyingFWD", "FlyingAttack", "Hover", "Lands",
    "TakeOff", "Die"};

    private Vector3 lookahead;
    private UnityEngine.UI.Slider healthbar;

    private Transform snout_transform;
    public GameObject FireballPrefab;
    private float timer = 5f;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        Canvas canvas = GetComponentInChildren<Canvas>();
        healthbar = canvas.GetComponentInChildren<UnityEngine.UI.Slider>();
        healthbar.value = health / 100.0f;
        snout_transform = GameObject.FindWithTag("Firebreather").transform;
        Debug.Log("Found snout");
        Debug.Log(snout_transform);
    }

    // Update is called once per frame
    void Update()
    {
        GameObject nest = GameObject.Find("DragonNest");
        Vector3 nest_location = nest.transform.position;
        if (isAlive)
        {
            FlyTothenOrbit(nest_location);
        }
        //TakeDamage((int)(Time.deltaTime * 160));
        timer -= Time.deltaTime;
       
        if (timer < 0)
        {
            Debug.Log(snout_transform);
            ShootFireball();
            timer = 10f;
        }
    }

    void ShootFireball()
    {
        GameObject fbo = Instantiate(FireballPrefab, snout_transform.position, Quaternion.identity);
        Fireball fb = fbo.GetComponent<Fireball>();
        fb.SetDirection(snout_transform.TransformDirection(Vector3.forward));
        
    }

    void FlyTo(Vector3 location)
    {
        /*
            Fly to the set point with the PurePursuit algo. Will glitch once arrive 
            recommended instead: FlyToandOrbit
        */
        Vector3 goal = transform.InverseTransformPoint(location);
        goal.y = 0;

        float angle_error = Mathf.Atan2(goal.x, goal.z);

        if (Math.Abs(angle_error) > Math.PI / 2 * 0.7 && !target_locked)
        {
            // Rotate to face the target if target is behind
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("IdleSimple"))
            { 
                clear_anim();
                anim.SetBool(Animator.StringToHash("Hover"), true);
            }
            transform.Rotate(Vector3.up, rotspeed * Math.Sign(angle_error)* Time.deltaTime);
            return;
        }

        // Fly to target with PurePursuit
        if (!target_locked)
        {
            clear_anim();
            anim.SetBool(Animator.StringToHash("IdleSimple"), true);
        }
        target_locked = Math.Abs(angle_error) < (Math.PI / 2);
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("IdleSimple"))
        {
            anim.SetBool(Animator.StringToHash("FlyingFWD"), true);
            anim.SetBool(Animator.StringToHash("IdleSimple"), false);
        }
        
        float arc_radius = goal.magnitude / (2 * (float)Math.Sin(angle_error));
        Vector3 v = new Vector3(); v.x = 0; v.y = 0;
        v.z = 20;
        Vector3 v_world = transform.TransformVector(v);

        rb.MovePosition(rb.position + v_world * Time.deltaTime);
        transform.Rotate(Vector3.up, 20 / arc_radius * 180 / (float)Math.PI * Time.deltaTime);
    }

    void FlyTothenOrbit(Vector3 location, float radius = 6, float dest_height=-1.0f)
    {
        // Fly to the set location using pure pursuit, then orbit the set point
        // if height is set, descend/ascend to correct height

        Vector3 heading = transform.position - location;

        Vector3 goal = transform.InverseTransformPoint(location);
        goal.y = 0;

        float angle_error = Mathf.Atan2(goal.x, goal.z);

        if (dest_height > 0)
        {
            float v_up = 2.0f * Math.Sign(dest_height - transform.localRotation.y);
            v_up = Math.Clamp(v_up, -8.0f, 8.0f);
            rb.MovePosition(rb.position + Vector3.up * v_up * Time.deltaTime);
        }
        
        heading.y = 0;
        Vector3 heading90 = Quaternion.Euler(0, 90f, 0) * heading;
        heading90 = heading90.normalized * radius;
        Vector3 destination = location + heading90;
        FlyTo(destination);
    }

    void LookAt(Vector3 location)
    {

    }

    public void TakeDamage(float dam)
    {
        if (health > dam)
        {
            health -= dam;
        }
        else { health = 0; Dies(); }
        healthbar.value = health / 100.0f;
    }

    void Dies()
    {
        rb.useGravity = true;
        isAlive = false;

        clear_anim();
        anim.Play(Animator.StringToHash("Die"));

    }

    void clear_anim()
    {
        foreach (string a in animatedStates)
        {
            anim.SetBool(Animator.StringToHash(a), false);
        }
    }
}
