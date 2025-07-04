using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public abstract class AnimationController {
    public abstract void set_animation(string a);
    public abstract void clear_animation();
}
public class DragonAnimationController : AnimationController {
    Animator animator;
    public DragonAnimationController(Animator a)
    {
        animator = a;
    }
    private string[] animatedStates = {"IdleSimple", "IdleAgressive", "Walk",
    "IdleRestless", "BattleStance", "Bite", "Drakaris", "FlyingFWD", "FlyingAttack", "Hover", "Lands",
    "TakeOff", "Die"};

    public override void set_animation(string a)
    {
        if (!animator.GetBool(Animator.StringToHash("IdleSimple")) && !animator.GetBool(Animator.StringToHash(a)))
        {
            clear_animation();
            animator.SetBool(Animator.StringToHash("IdleSimple"), true);
            return;
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("IdleSimple"))
        {
            clear_animation();
            animator.SetBool(Animator.StringToHash(a), true);
            return;
            //animator.Play(Animator.StringToHash(a));
        }
       
    }
    public override void clear_animation()
    {
        foreach (string a in animatedStates)
        {
            animator.SetBool(Animator.StringToHash(a), false);
        }
    }
}
public abstract class Routine
{
    public Dragon dragon;
    public Routine(Dragon d)
    {
        dragon = d;
    }
    public bool isActive = true;
    public abstract void act();
}

public class AirPursuitAttack : Routine
{
    public float fire_timer = 4f;
    public float alt = 9;
    private float range = 20f;
    private bool in_shooting_range = false;

    private float time_limit = 15f;

    public AirPursuitAttack(Dragon dragon) : base(dragon) { }
    public override void act()
    {
        fire_timer -= Time.deltaTime;
        time_limit -= Time.deltaTime;
        if (time_limit < 0)
        {
            isActive = false;
            return;
        }
        Vector3 goal = dragon.transform.InverseTransformPoint(dragon.target.transform.position);
        goal.y = 0;
        if (!dragon.AscendTo(dragon.nest.transform.position.y + alt)) { return; }
        if (!in_shooting_range) { in_shooting_range = dragon.FlyTo(dragon.target.transform.position, 15f);  return; }
        if (!dragon.RotateToFace(dragon.target.transform.position)){  return;}
        dragon.ac.set_animation("Hover");
        dragon.target_locked = false;
        in_shooting_range = goal.magnitude < range * 5;
        float angle_error = Mathf.Atan2(goal.x, goal.z);
        if (Math.Abs(angle_error) < 5 && goal.magnitude < range*5 && fire_timer < 0)
        {
            dragon.firebreather.transform.LookAt(dragon.target.transform.position);
            dragon.ShootFireball();
            fire_timer = 4f;
        }
    }
}
public class FlyToHome : Routine
{
    public bool arrived = false;
    public bool ascend_complete = false;
    public FlyToHome(Dragon dragon) : base(dragon){}
    public override void act()
    {
        if (!ascend_complete) { ascend_complete = dragon.AscendTo(dragon.nest.transform.position.y + 17f); return; }
        if (!arrived) { arrived = dragon.FlyTo(dragon.nest.transform.position, 1); Debug.Log("Flying Home"); return; }
        if (!dragon.Land()) { return; }
        isActive = false;
        
    }
}
public class Idle : Routine
{
    public Idle(Dragon dragon) : base(dragon){}
    public override void act()
    {
        dragon.ac.set_animation("IdleSimple");
        isActive = false;
    }
}
public class GroundPursuitAttack : Routine
{
    public float fire_timer = 4f;
    public float alt = 13;
    private float range = 17f;
    private bool in_shooting_range = false;
    private bool at_home = false;

    private float time_limit = 15f;
    public GroundPursuitAttack(Dragon d) : base(d){}
    public override void act()
    {
        fire_timer -= Time.deltaTime;
        time_limit -= Time.deltaTime;
        if (time_limit < 0)
        {
            isActive = false;
            return;
        }
        Vector3 goal = dragon.transform.InverseTransformPoint(dragon.target.transform.position);
        goal.y = 0;

        in_shooting_range = goal.magnitude < range * 5;
        if (!in_shooting_range) { return; }

        dragon.rb.useGravity = true;
        if (!dragon.GroundRotateToFace(dragon.target.transform.position)) { return; }
        dragon.ac.set_animation("Drakaris");
        dragon.target_locked = false;
        
        float angle_error = Mathf.Atan2(goal.x, goal.z);
        if (Math.Abs(angle_error) < 5 && goal.magnitude < range * 4 && fire_timer < 0)
        {
            dragon.firebreather.transform.LookAt(dragon.target.transform.position);
            dragon.ShootFireball();
            fire_timer = 2f;
        }
    }
}
public class Dragon : MonoBehaviour,LivingBeing
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    bool isAlive = true;
    public Rigidbody rb;
    private Animator anim;
    private float health;
    public float health_max = 100;
    private UnityEngine.UI.Slider healthbar;
    public GameObject firebreather;
    private GameObject head;
    public GameObject FireballPrefab;
    private Routine routine;
    public DragonAnimationController ac;
    public float player_detection_range = 30;
    public GameObject nest;
    public GameObject target;
    public float protect_perimeter = 40;
    public bool target_locked = false;
    private float rotspeed = 70;
    private bool rth = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        ac = new DragonAnimationController(anim);
        Canvas canvas = GetComponentInChildren<Canvas>();
        healthbar = canvas.GetComponentInChildren<UnityEngine.UI.Slider>();
        healthbar.value = health / 100.0f;
        firebreather = GameObject.FindWithTag("Firebreather");
        head = GameObject.FindWithTag("DragonHead");
        routine = new Idle(this);
        health = health_max;
    }

    // Update is called once per frame
    void Update()
    {
       if (!isAlive) {return;}
       if (routine.isActive) { routine.act(); return; }
       
       if (!CheckPerimeter()) {  
        routine = new FlyToHome(this); 
        return;
       }

        if (!IsAtHome() && health < 30)
        {
            routine = new FlyToHome(this);
            return;
        }

       if (CheckPlayerClose()) { 
            if (health > 30)
            {
                routine = new AirPursuitAttack(this);
                return;
            }
            else
            {
                routine = new GroundPursuitAttack(this);
                return;
             
            }
       }
    }

    public void ShootFireball()
    {
        GameObject fbo = Instantiate(FireballPrefab, firebreather.transform.position, Quaternion.identity);
        Fireball fb = fbo.GetComponent<Fireball>();
        fb.SetDirection(firebreather.transform.TransformDirection(Vector3.forward));

    }
    public void TakeDamage(float dam)
    {
       
        if (health > dam)
        {
            health -= dam;
        }
        else { health = 0; Dies(); }
        healthbar.value = health / health_max;
    }

    void Dies()
    {
        if (isAlive)
        {
            rb.useGravity = true;
            isAlive = false;

            ac.clear_animation();
            anim.Play(Animator.StringToHash("Die"));
            rb.freezeRotation = false;
        }
    }

    bool CheckGround()
    {
        bool grounded = Physics.Raycast(transform.position, UnityEngine.Vector3.down,
        (float)(transform.localScale.y + 0.05f));
        return grounded;
    }
    bool CheckPlayerClose()
    {
        Vector3 delta = rb.position - target.transform.position;
        delta.y = 0;
        return delta.magnitude < player_detection_range;
    }
    bool CheckPerimeter()
    {
        Vector3 delta = rb.position - nest.transform.position;
        delta.y = 0;
        return delta.magnitude < protect_perimeter;
    }

    bool IsAtHome()
    {
        Vector3 delta = rb.position - nest.transform.position;
        delta.y = 0;
        return delta.magnitude < 5;
    }
    public bool FlyTo(Vector3 location, float within=10)
    {
        /*
            Fly to the set point with the PurePursuit algo. Will glitch once arrive 
            recommended instead: FlyToandOrbit
        */
        Vector3 delta = rb.transform.position - location;
        delta.y = 0;
        if (delta.magnitude < within)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            ac.set_animation("Hover");
            return true;
        }
        if (!RotateToFace(location))
        {
            return false;
        }

        // Fly to target with PurePursuit
        ac.set_animation("FlyingFWD");
        Vector3 v = new Vector3(); v.x = 0; v.y = 0;
        v.z = 35;
        Vector3 v_world = rb.transform.TransformVector(v);
        v_world.y = 0;

        rb.linearVelocity = v_world;
        return false;
    }

    public bool RotateToFace(Vector3 location)
    {
        Vector3 goal = rb.transform.InverseTransformPoint(location);
        goal.y = 0;

        float angle_error = Mathf.Atan2(goal.x, goal.z);

        if (Math.Abs(angle_error) > 0.01 && !target_locked)
        {
            ac.set_animation("Hover");
            Vector3 orientation = rb.transform.rotation.eulerAngles;
            orientation.x = 0;
            orientation.z = 0;
            orientation.y += rotspeed * Math.Sign(angle_error) * Time.deltaTime;
            //rb.transform.Rotate(Vector3.up, rotspeed * Math.Sign(angle_error) * Time.deltaTime);
            rb.transform.rotation = Quaternion.Euler(orientation);
            return false;
        }
        target_locked = Math.Abs(angle_error) > Math.PI / 2 - 0.05;
        return true;
    }
    public bool Land()
    {
        if (CheckGround())
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            ac.set_animation("IdleSimple");
            return true;
        }
        ac.set_animation("Hover");
        rb.linearVelocity = Vector3.up * -6;
        rb.useGravity = true;
        return false;
    }

    public bool GroundRotateToFace(Vector3 location)
    {
        Vector3 goal = rb.transform.InverseTransformPoint(location);
        goal.y = 0;

        float angle_error = Mathf.Atan2(goal.x, goal.z);

        if (Math.Abs(angle_error) > 0.01 && !target_locked)
        {
            ac.set_animation("Walk");
            rb.transform.Rotate(Vector3.up, rotspeed * Math.Sign(angle_error) * Time.deltaTime);
            return false;
        }
        target_locked = Math.Abs(angle_error) > Math.PI / 2 - 0.05;
        return true;
    }
    public bool WalkTo(Vector3 location, float range=5f)
    {
        rb.useGravity = true;
        Vector3 delta = rb.transform.position - location;
        delta.y = 0;
        if (delta.magnitude < range)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            ac.set_animation("BattleStance");
            return true;
        }
        if (!GroundRotateToFace(location))
        {
            return false;
        }
        ac.set_animation("Walk");
        Vector3 v = new Vector3(); v.x = 0; v.y = 0;
        v.z = 15;
        Vector3 v_world = rb.transform.TransformVector(v);
        v_world.y = 0;

        rb.linearVelocity = v_world;
        return false;
    }

    public bool AscendTo(float alt)
    {
        rb.useGravity = false;
        float alt_error = rb.transform.position.y - alt;
        if (Math.Abs(alt_error) < 1e-2)
        {
            Debug.Log("Ascend completed");
            rb.linearVelocity = Vector3.zero;
            return true;
        }
        ac.set_animation("Hover");
        float set_vel = Math.Clamp(alt_error * -5, -5, 5);
        rb.linearVelocity = Vector3.up * set_vel;
        return false;
    }
}
