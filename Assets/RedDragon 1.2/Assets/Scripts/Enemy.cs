using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private float rotspeed = 80;
    private bool isFlying = true;
    private bool target_locked = false;
    bool isAlive = true;
    Rigidbody rb;
    bool isAttacking = false;
    private Animator anim;

    private int health = 100;

    private string[] animatedStates = {"IdleSimple", "IdleAgressive", "Walk",
    "IdleRestless", "BattleStance", "Bite", "Drakaris", "FlyingFWD", "FlyingAttack", "Hover", "Lands",
    "TakeOff", "Die"};

    private Vector3 lookahead;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject nest = GameObject.Find("DragonNest");
        Vector3 nest_location = nest.transform.position;
        FlyTothenOrbit(nest_location);
    }

    void Circle(Vector3 location, float radius)
    {

    }

    void FlyTo(Vector3 location)
    {
        Vector3 goal = transform.InverseTransformPoint(location);
        goal.y = 0;

        float angle_error = Mathf.Atan2(goal.x, goal.z);
        //Debug.Log(angle_error * 180 / 3.141);

        if (Math.Abs(angle_error) > Math.PI / 2 * 0.7 && !target_locked)
        {
            // Rotate to face the target if target is behind
            clear_anim();
            anim.SetBool(Animator.StringToHash("Hover"), true);
            transform.Rotate(Vector3.up, rotspeed * Time.deltaTime);
            return;
        }

        // Fly to target with PurePursuit
        if (!target_locked)
        {
            clear_anim();
            anim.SetBool(Animator.StringToHash("IdleSimple"), true);
            anim.SetBool(Animator.StringToHash("FlyingFWD"), true);
        }

        target_locked = Math.Abs(angle_error) < (Math.PI / 2);


        float arc_radius = goal.magnitude / (2 * (float)Math.Sin(angle_error));
        Vector3 v = new Vector3(); v.x = 0; v.y = 0;
        v.z = 20;
        Vector3 v_world = transform.TransformVector(v);

        rb.MovePosition(rb.position + v_world * Time.deltaTime);
        transform.Rotate(Vector3.up, 20 / arc_radius * 180 / (float)Math.PI * Time.deltaTime);
    }

    void FlyTothenOrbit(Vector3 location, float radius = 6)
    {
        Vector3 heading = transform.position - location;
        heading.y = 0;
        Vector3 heading90 = Quaternion.Euler(0, -90f, 0) * heading;
        heading90 = heading90.normalized * radius;
        Vector3 destination = location + heading90;
        FlyTo(destination);
    }

    void LookAt(Vector3 location)
    {

    }

    void TakeDamage(int dam)
    {
        if (health > dam) { health -= dam; }
        else { health = 0; Dies(); }
    }

    void Dies()
    {

    }

    void clear_anim()
    {
        foreach (string a in animatedStates)
        {
            anim.SetBool(Animator.StringToHash(a), false);
        }
    }
}
