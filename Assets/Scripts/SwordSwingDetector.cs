using UnityEngine;
using UnityEngine.XR;

public class SwordSwingDetector : MonoBehaviour
{
    [Header("Swing Detection")]
    public XRNode controllerNode = XRNode.RightHand;
    public float swingVelocityThreshold = 1.5f;
    public AudioSource swingSound;

    [Header("Hit Detection")]
    public AudioSource hitSound;
    public ParticleSystem hitEffect;

    private Vector3 lastVelocity;
    private bool isSwinging = false;

    void Update()
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(controllerNode);
        if (device.isValid && device.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity))
        {
            float speed = velocity.magnitude;

            // 检测从未挥动到开始挥动的瞬间
            if (speed > swingVelocityThreshold && lastVelocity.magnitude < swingVelocityThreshold)
            {
                isSwinging = true;
                OnSwordSwing();
            }
            else if (speed < swingVelocityThreshold * 0.8f)
            {
                isSwinging = false;  // 停止检测命中
            }

            lastVelocity = velocity;
        }
    }

    private void OnSwordSwing()
    {
        Debug.Log("⚔️ Sword swing detected!");
        if (swingSound != null)
            swingSound.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isSwinging) return;
        Debug.Log("Collision Sword");
        LivingBeing enemy = other.gameObject.GetComponentInParent<LivingBeing>();
        if (enemy != null)
        {
            Debug.Log("🎯 Enemy Hit!");
            enemy.TakeDamage(8);

            // 播放命中音效
            if (hitSound != null)
                hitSound.Play();

            // 播放命中特效
            if (hitEffect != null)
            {
                hitEffect.transform.position = other.ClosestPoint(transform.position);
                hitEffect.Play();
            }
          
            // 示例：销毁敌人
            // Destroy(other.gameObject);
        }
    }
}
