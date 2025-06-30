using System;
using System.Collections.Generic;
using System.Xml;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ActionIDLock
{
    string lockID = "";
    public bool accquire(string withID)
    {
        if (lockID == "" || lockID == withID)
        {
            lockID = withID;
            return true;
        }
        return false;
    }

    public void release(string withID)
    {
        if (withID == lockID)
        {
            lockID = "";
        }
    }

    public bool force_accquire(string withID)
    {
        lockID = withID; return true;
    }
    public bool force_release()
    {
        lockID = ""; return true;
    }
}

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
            //anim.Play(Animator.StringToHash(a));
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
public abstract class Routine {
    public Rigidbody rb;
    public AnimationController ac;
    public Routine(Rigidbody rgbd, AnimationController anctl)
    {
        rb = rgbd;
        ac = anctl;
    }
    public bool isActive = true;
    public abstract bool act();
}

class Landing : Routine
{
    public Landing(Rigidbody rb, DragonAnimationController ac)
    : base(rb, ac)
    {

    }
    public override bool act()
    {
        if (CheckGround())
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            ac.set_animation("IdleSimple");
            isActive = false;
            return true;
        }
        ac.set_animation("Hover");
        rb.linearVelocity = Vector3.up * -6;
        rb.useGravity = true;
        return false;
    }
    
    bool CheckGround()
    {
        bool grounded = Physics.Raycast(rb.transform.position, UnityEngine.Vector3.down,
        (float)(rb.transform.localScale.y + 0.05f));
        return grounded;
    }
}
class Ascend : Routine
{
    float target_alt;
    public Ascend(Rigidbody rb, DragonAnimationController ac, float set_alt)
    : base(rb, ac)
    {
        target_alt = set_alt;
    }
    public override bool act()
    {
        float alt_error = rb.transform.position.y - target_alt;

        if (Math.Abs(alt_error) < 1e-2)
        {
            rb.linearVelocity = Vector3.zero;
            isActive = false;
            return true;
        }
        ac.set_animation("Hover");
        rb.useGravity = false;
        float set_vel = Math.Clamp(alt_error * -6, -6, 6);
        rb.linearVelocity = Vector3.up * set_vel;
        return false;
    }
}

class FlyTothenOrbit : Routine
{
    public Vector3 target_pos;
    public float radius = 10;
    private bool target_locked = false;
    private float rotspeed = 40;
    public bool keep_orbit = false;
    public FlyTothenOrbit(Rigidbody rb, DragonAnimationController ac, Vector3 target)
    : base(rb, ac)
    {
        target_pos = target;
    }
    public override bool act()
    {
        Vector3 heading = rb.transform.position - target_pos;

        Vector3 goal = rb.transform.InverseTransformPoint(target_pos);
        goal.y = 0;

        heading.y = 0;
        Vector3 heading60 = Quaternion.Euler(0, 60f, 0) * heading;
        heading60 = heading60.normalized * radius;
        Vector3 destination = target_pos + heading60;
        FlyTo(destination);
        if (goal.magnitude <= radius * 5.05) { if (!keep_orbit) { isActive = false; }  return true; }
        return false;
    }

    public void FlyTo(Vector3 location)
    {
        /*
            Fly to the set point with the PurePursuit algo. Will glitch once arrive 
            recommended instead: FlyToandOrbit
        */
        Vector3 goal = rb.transform.InverseTransformPoint(location);
        goal.y = 0;

        float angle_error = Mathf.Atan2(goal.x, goal.z);

        if (Math.Abs(angle_error) > Math.PI / 2 * 0.7 && !target_locked)
        {
            // Rotate to face the target if target is behind
            ac.set_animation("Hover");
            rb.transform.Rotate(Vector3.up, rotspeed * Math.Sign(angle_error) * Time.deltaTime);
            return;
        }

        // Fly to target with PurePursuit
        ac.set_animation("FlyingFWD");
        target_locked = Math.Abs(angle_error) < (Math.PI / 2);

        float arc_radius = goal.magnitude / (2 * (float)Math.Sin(angle_error));
        Vector3 v = new Vector3(); v.x = 0; v.y = 0;
        v.z = 35;
        Vector3 v_world = rb.transform.TransformVector(v);
        v_world.y = 0;

        rb.linearVelocity = v_world;
        rb.angularVelocity = 20 / arc_radius * Vector3.up;
        //transform.Rotate(Vector3.up, 20 / arc_radius * 180 / (float)Math.PI * Time.deltaTime);
    }
}

class FlyToExactSpot : Routine
{
    Vector3 target;
    private bool target_locked = false;
    private float rotspeed = 40;
    public FlyToExactSpot(Rigidbody rb, DragonAnimationController ac, Vector3 target_pos) : base(rb, ac)
    {
        target = target_pos;
    }

    public override bool act()
    {
        FlyTo(target);
        Vector3 delta = rb.transform.position - target;
        delta.y = 0;
        if (delta.magnitude < 1)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            isActive = false;
            return true;
        }
        return false;
    }

    public void FlyTo(Vector3 location)
    {
        /*
            Fly to the set point with the PurePursuit algo. Will glitch once arrive 
            recommended instead: FlyToandOrbit
        */
        Vector3 goal = rb.transform.InverseTransformPoint(location);
        goal.y = 0;

        float angle_error = Mathf.Atan2(goal.x, goal.z);

        if (Math.Abs(angle_error) > Math.PI / 2 * 0.7 && !target_locked)
        {
            // Rotate to face the target if target is behind
            ac.set_animation("Hover");
            rb.transform.Rotate(Vector3.up, rotspeed * Math.Sign(angle_error) * Time.deltaTime);
            return;
        }

        // Fly to target with PurePursuit
        ac.set_animation("FlyingFWD");
        target_locked = Math.Abs(angle_error) < (Math.PI / 2);

        float arc_radius = goal.magnitude / (2 * (float)Math.Sin(angle_error));
        Vector3 v = new Vector3(); v.x = 0; v.y = 0;
        v.z = 35;
        Vector3 v_world = rb.transform.TransformVector(v);
        v_world.y = 0;

        rb.linearVelocity = v_world;
        rb.angularVelocity = 20 / arc_radius * Vector3.up;
        //transform.Rotate(Vector3.up, 20 / arc_radius * 180 / (float)Math.PI * Time.deltaTime);
    }
}
class DefaultRoutine : Routine
{
    public DefaultRoutine(Rigidbody rgbd, AnimationController anctl) : base(rgbd, anctl) { }
    public override bool act()
    {
        ac.set_animation("IdleSimple");
        isActive = false;
        return true;
    }
}
class ChaseAttack : FlyTothenOrbit
{
    GameObject target;
    public ChaseAttack(Rigidbody rgbd, DragonAnimationController anctl, GameObject go) : base(rgbd, anctl, go.transform.position)
    {
        target = go;
        keep_orbit = true;
    }
    public override bool act()
    {
        target_pos = target.transform.position;
        return base.act();
    }
}
class Sequential : Routine
{
    int current_routine = 0;
    public List<Routine> routines;
    public Sequential(Rigidbody rb, DragonAnimationController ac,
    List<Routine> sequence)
    : base(rb, ac)
    {
        routines = sequence;
        isActive = true;
    }

    public override bool act()
    {
        if (routines.Count == current_routine) { isActive = false; return true; }
        routines[current_routine].act();
        if (!routines[current_routine].isActive)
        {
            current_routine += 1;
        }
        return false;
    }

}

public class Dragon : MonoBehaviour, LivingBeing
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    bool isAlive = true;
    Rigidbody rb;
    bool isAttacking = false;
    private Animator anim;

    private float health = 100;
    public float health_max = 100;

    private UnityEngine.UI.Slider healthbar;

    private Transform snout_transform;
    public GameObject FireballPrefab;
    private float timer = 5f;

    private Routine routine;
    private DragonAnimationController ac;
    private float player_detection_range = 30;

    private Vector3 nest_location;
    private float protect_perimeter = 60;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        ac = new DragonAnimationController(anim);
        Canvas canvas = GetComponentInChildren<Canvas>();
        healthbar = canvas.GetComponentInChildren<UnityEngine.UI.Slider>();
        healthbar.value = health / 100.0f;
        snout_transform = GameObject.FindWithTag("Firebreather").transform;
        GameObject nest = GameObject.Find("DragonNest");
        nest_location = nest.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive) { return; }
        if (!CheckPerimeter())
            {
                routine = new Sequential(rb, ac,
            new List<Routine>
            {
                new FlyToExactSpot(rb, ac,nest_location),
                new Landing(rb, ac),
                new DefaultRoutine(rb,ac)
            }
            );
        }
        if (routine == null || !routine.isActive)
        {
            if (CheckPlayerClose())
            {
                GameObject player = GameObject.Find("Player");
                routine = new Sequential(rb, ac,
                new List<Routine>
                {
                    new Ascend(rb, ac, 18f),
                    new ChaseAttack(rb, ac, player)
                }
                );
            }
            else
            {
                routine = new DefaultRoutine(rb, ac);
            }

        }
        routine.act();
    }

    void ShootFireball()
    {
        GameObject fbo = Instantiate(FireballPrefab, snout_transform.position, Quaternion.identity);
        Fireball fb = fbo.GetComponent<Fireball>();
        fb.SetDirection(snout_transform.TransformDirection(Vector3.forward));

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
            Destroy(gameObject);
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
        GameObject player = GameObject.FindWithTag("Player");
        Vector3 delta = rb.position - player.transform.position;
        delta.y = 0;
        return delta.magnitude < player_detection_range;
    }
    bool CheckPerimeter()
    {
        Vector3 delta = rb.position - nest_location;
        delta.y = 0;
        return delta.magnitude < protect_perimeter;
    }
}
