using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class Player : MonoSingleton<Player>
{
    public float m_moveAccel = (0.12f * 60.0f);
    public float m_groundFriction = 0.85f;
    public float m_gravity = (-0.05f * 60.0f);
    public float m_jumpVel = 0.75f;
    public float m_jumpMinTime = 0.06f;
    public float m_jumpMaxTime = 0.20f;
    public float m_airFallFriction = 0.975f;
    public float m_airMoveFriction = 0.85f;
    public float m_muzzleOffset;
   
    public float m_maxAccelationDuration = 2;
    public AnimationCurve m_accelerationCurve;
    public Camera m_camera;
    public Animator m_muzzleFlashAnimator;

    public Animator m_Animator;
    public Transform m_Skin;
    private Vector3 m_SkinScaleRight = new Vector3(0, 0);
    private Vector3 m_SkinScaleLeft = new Vector3(0, 0);

    public CinemachineImpulseSource m_impulseSource;
    public Vector2 m_shootImpulse = new Vector2(0,0);


    private Rigidbody2D m_rigidBody = null;
    private bool m_jumpPressed = false;
    private bool m_jumpHeld = false;
    private bool m_wantsRight = false;
    private bool m_wantsLeft = false;
    private bool m_shootPressed = false;
    private bool m_fireRight = true;
    private bool m_hasWeapon = false;
    private float m_stateTimer = 0.0f;
    private float m_AccelerationTimer = 0;
    private Vector2 m_vel = new Vector2(0, 0);
    private List<GameObject> m_groundObjects = new List<GameObject>();


    private enum State
    {
        Idle = 0,
        Falling,
        Jumping,
        Walking
    };

    private State m_state = State.Idle;

    // Use this for initialization
    void Start ()
    {
        m_SkinScaleRight = m_Skin.localScale;
        m_SkinScaleLeft = m_SkinScaleRight;
        m_SkinScaleLeft.x = -m_SkinScaleLeft.x;
        m_rigidBody = transform.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        UpdateInput();
        UpdateRender();

        if (m_shootPressed && m_hasWeapon)
        {
            //Fire
            GameObject projectileGO = ObjectPooler.Instance.GetObject("Bullet");
            if (projectileGO)
            {

                projectileGO.GetComponent<Bullet>().Fire(transform.position, m_fireRight);

                Vector3 muzzleOffsetPosition = transform.position + ((m_fireRight ? Vector3.right  :  Vector3.left) * m_muzzleOffset);
                PlayMuzzleFlash(muzzleOffsetPosition, m_fireRight);
            }
        }
    }

    private void UpdateRender()
    {
        if(m_wantsLeft)
        {
            m_Skin.localScale = m_SkinScaleLeft;
        }
        else if(m_wantsRight)
        {
            m_Skin.localScale = m_SkinScaleRight;
        }
    }

    void PlayMuzzleFlash( Vector2 position, bool isRight)
    {
        m_muzzleFlashAnimator.transform.position = position;
        m_muzzleFlashAnimator.gameObject.SetActive(true);
        m_muzzleFlashAnimator.GetComponent<SpriteRenderer>().flipX = isRight;
        m_muzzleFlashAnimator.SetTrigger("Fire");

        DoCameraShake(position, m_shootImpulse);
    }

    private void SwitchState(State state)
    {
        m_Animator.CrossFade(state.ToString(),0.1f);
        m_state = state;
    }

    void FixedUpdate()
    {
        switch (m_state)
        {
            case State.Idle:
                Idle();
                break;
            case State.Falling:
                Falling();
                break;
            case State.Jumping:
                Jumping();
                break;
            case State.Walking:
                Walking();
                break;
            default:
                break;
        }

        if(m_wantsRight == true)
        {
            m_fireRight = true;
        }
        else if(m_wantsLeft == true)
        {
            m_fireRight = false;
        }

        if (m_wantsRight && m_wantsLeft)
        {
            ResetAccelerationTime();
        }
    }

    public void GiveWeapon()
    {
        m_hasWeapon = true;
    }

    #region States
    void Idle()
    {
        m_vel = Vector2.zero;

        ResetAccelerationTime();

        //Check to see whether to go into movement of some sort
        if (m_groundObjects.Count == 0)
        {
            //No longer on the ground, fall.
            SwitchState(State.Falling);
            return;
        }

        //Check input for other state transitions
        if (m_jumpPressed || m_jumpHeld)
        {
            m_stateTimer = 0;
            SwitchState(State.Jumping);
            return;
        }

        //Test for input to move
        if (m_wantsLeft || m_wantsRight)
        {
            SwitchState(State.Walking);
            return;
        }
    }

    void Falling()
    {
        m_vel.y += m_gravity * Time.fixedDeltaTime;
        m_vel.y *= m_airFallFriction /*+ ((!m_wantsLeft && !m_wantsRight) ?  0.1f : 0)*/;
        if (m_wantsLeft)
        {
            m_vel.x -= m_moveAccel * Time.fixedDeltaTime;
        }
        else if (m_wantsRight)
        {
            m_vel.x += m_moveAccel  * Time.fixedDeltaTime;
        }

        m_vel.x *= m_airMoveFriction * GetDragAcceleration();

        ApplyVelocity();
    }

    void Jumping()
    {
        m_stateTimer += Time.fixedDeltaTime;

        if (m_stateTimer < m_jumpMinTime || (m_jumpHeld && m_stateTimer < m_jumpMaxTime))
        {
            m_vel.y = m_jumpVel;
        }

        m_vel.y += m_gravity * Time.fixedDeltaTime;

        if (m_vel.y <= 0)
        {
           // ResetAccelerationTime(0.5f);
            SwitchState(State.Falling);
        }

        if (m_wantsLeft)
        {
            m_vel.x -= m_moveAccel * Time.fixedDeltaTime;
        }
        else if (m_wantsRight)
        {
            m_vel.x += m_moveAccel  * Time.fixedDeltaTime;
        }

        m_vel.x *= m_airMoveFriction * GetDragAcceleration();

        ApplyVelocity();
    }

    void Walking()
    {
        if (m_wantsLeft)
        {
            m_vel.x -= m_moveAccel * Time.fixedDeltaTime;
        }
        else if (m_wantsRight)
        {
            m_vel.x += m_moveAccel  * Time.fixedDeltaTime;
        }
        else if (m_vel.x >= -0.05f && m_vel.x <= 0.05)
        {
            SwitchState(State.Idle);
            m_vel.x = 0;
        }

        m_vel.y = 0;
        m_vel.x *= m_groundFriction * GetDragAcceleration();

        ApplyVelocity();

        if (m_groundObjects.Count == 0)
        {
            //No longer on the ground, fall.
            SwitchState(State.Falling);
            return;
        }

        if (m_jumpPressed || m_jumpHeld)
        {
            m_stateTimer = 0;
            SwitchState(State.Jumping);
            return;
        }
    }
    #endregion

    #region MovmentMethods
    void ApplyVelocity()
    {
        Vector3 pos = m_rigidBody.transform.position;
        pos.x += m_vel.x;
        pos.y += m_vel.y;
        m_rigidBody.transform.position = pos;
    }
    public void AddAdditionalJumpForce(float jumpForce)
    {
        m_vel.y += jumpForce;

        ApplyVelocity();
    }

    void ResetAccelerationTime(float accerationTime = 0)
    {
        m_AccelerationTimer = accerationTime;
    }

    private float GetDragAcceleration()
    {
        m_AccelerationTimer += Time.fixedDeltaTime;

        float clampedValue = Mathf.Clamp01((m_AccelerationTimer / m_maxAccelationDuration));

        float speed = m_accelerationCurve.Evaluate(clampedValue);

        return speed;
    }

    #endregion

    #region Inputs
    void UpdateInput()
    {
        m_wantsLeft = Input.GetKey(KeyCode.LeftArrow);
        m_wantsRight = Input.GetKey(KeyCode.RightArrow);
        m_jumpPressed = Input.GetKeyDown(KeyCode.UpArrow);
        m_jumpHeld = Input.GetKey(KeyCode.UpArrow);
        m_shootPressed = Input.GetKeyDown(KeyCode.Space);
    }
    #endregion

    #region Collisions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision);

        if (CheckEnemyHit(out Enemy enemy))
        {
            if (enemy != null)
            {
                enemy.ChangeEnemyState(Enemy.State.Death);
                AddAdditionalJumpForce(6.0f);
                return;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ProcessCollision(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        m_groundObjects.Remove(collision.gameObject);
    }

    private void ProcessCollision(Collision2D collision)
    {
        m_groundObjects.Remove(collision.gameObject);
        Vector3 pos = m_rigidBody.transform.position;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            //Push back out
            Vector2 impulse = contact.normal * (contact.normalImpulse / Time.fixedDeltaTime);
            pos.x += impulse.x;
            pos.y += impulse.y;

            if (Mathf.Abs(contact.normal.y) > Mathf.Abs(contact.normal.x))
            {
                //Hit ground
                if (contact.normal.y > 0)
                {
                    if (m_groundObjects.Contains(contact.collider.gameObject) == false)
                    {
                        m_groundObjects.Add(contact.collider.gameObject);
                    }
                    if (m_state == State.Falling)
                    {
                        //If we've been pushed up, we've hit the ground.  Go to a ground-based state.
                        if (m_wantsRight || m_wantsLeft)
                        {
                            SwitchState(State.Walking);
                        }
                        else
                        {
                            SwitchState(State.Idle);
                        }
                    }
                }
                //Hit Roof
                else
                {
                    m_vel.y = 0;
                    SwitchState(State.Falling);
                }
            }
            else
            {
                if ((contact.normal.x > 0 && m_vel.x < 0) || (contact.normal.x < 0 && m_vel.x > 0))
                {
                    m_vel.x = 0;
                }
            }
        }
        m_rigidBody.transform.position = pos;
    }

    private bool CheckEnemyHit( out Enemy obj)
    {
        foreach (var item in m_groundObjects)
        {
            bool result = item.TryGetComponent(out Enemy b);

            obj = b;
            return result;
        }
        obj = null;
        return false;
    }

    #endregion

    public void DoCameraShake(Vector3 position, Vector2 velocity)
    {
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        float randomMagnitude = Random.Range(velocity.x, velocity.y);
        m_impulseSource.GenerateImpulseAt(new Vector3(position.x, position.y, 0), randomDirection * randomMagnitude);
    }
}
