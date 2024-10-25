using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;
public class Enemy : MonoBehaviour
{
    public float m_moveSpeed = (0.05f * 60.0f);
    public float m_changeSpeed = 0.2f * 60.0f;
    public float m_moveDuration = 3.0f;
    public float m_holdDuration = 0.5f;
    public float m_chargeCooldownDuration = 2.0f;
    public float m_chargeMinRange = 1.0f;
    public float m_maxHealth = 4.0f;

    public Player m_player = null;

    private Rigidbody2D m_rigidBody = null;
    private SpriteRenderer m_sprite = null;
    private Animator m_animator = null;

    private float m_health = 100.0f;
    private float m_timer = 0.0f;
    private float m_lastPlayerDiff = 0.0f;

   [SerializeField] private bool m_isDead = false;  
    private bool m_isBulletHit = false;  
    private bool m_playOneShot = false;

    private Vector2 m_vel = new Vector2(0, 0);

    private Sequence bulletHitSequence;


    public  class PlayerAnimationsStrings
    {
        public static string m_idle = "Idle";
        public static string m_bottomHit = "BottomHit";
        public static string m_leftHit = "LeftHit";
        public static string m_rightHit = "RightHit";
    }

    public enum WallCollision
    {
        None = 0,
        Left,
        Right
    };
    public WallCollision m_wallFlags = WallCollision.None;

    public enum State
    {
        Idle = 0,
        Walking,
        Charging,
        ChargingCooldown,
        Bullet_Hit,
        Death
    };
    [SerializeField] private State m_currentState = State.Idle;

    // Start is called before the first frame update
    void Start()
    {
        m_health = m_maxHealth;
        m_rigidBody = transform.GetComponent<Rigidbody2D>();
        m_sprite = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        #region OldCode
        switch (m_currentState)
        {
            case State.Idle:
                Idle();
                break;
            case State.Walking:
                Walking();
                break;
            case State.Charging:
                Charging();
                break;
            case State.ChargingCooldown:
                ChargingCooldown();
                break;
            case State.Death:
                Dead();
                break;
            case State.Bullet_Hit:
                BulletHit();
                break;

            default:
                break;
        }

        m_wallFlags = WallCollision.None;

        #endregion


        

    }


    public void ChangeEnemyState(State enemyState)
    {
        if (m_currentState != enemyState)
        {
            m_currentState = enemyState;
        }
    }

    public void InflictDamage(float damageAmount)
    {
        m_isBulletHit = true;
        m_health -= damageAmount;

        if (m_health <= 0.0f)
        {
            print("Player should die");
            m_vel = Vector2.zero;
            m_timer = 0;
            ChangeEnemyState(State.Death);
            return;
        }
        else
        {
            m_vel = Vector2.zero;
            m_timer = 0;

            m_isBulletHit = true ;
            ChangeEnemyState(State.Bullet_Hit);
            return;
        }
    }

    void Idle()
    {
        m_vel = Vector2.zero;

        float yDiff = m_player.transform.position.y - transform.position.y;
        if(Mathf.Abs(yDiff) <= m_chargeMinRange)
        {
            //Charge at the player!
            m_lastPlayerDiff = m_player.transform.position.x - transform.position.x;
            m_vel.x = m_changeSpeed * Mathf.Sign(m_lastPlayerDiff);
            m_timer = 0;
            m_currentState = State.Charging;
            return;
        }

        m_timer += Time.deltaTime;
        if(m_timer >= m_holdDuration)
        {
            m_timer = 0;
            m_currentState = State.Walking;

            if(m_wallFlags == WallCollision.None)
            {
                //Randomly choose.
                m_vel.x = (Random.Range(0.0f, 100.0f) < 50.0f) ? m_moveSpeed : -m_moveSpeed;
            }
            else
            {
                m_vel.x = (m_wallFlags == WallCollision.Left) ? m_moveSpeed : -m_moveSpeed;
            }
            return;
        }

        PlayAnimation(PlayerAnimationsStrings.m_idle);
    }

    void Walking()
    {
        ApplyVelocity();

        float yDiff = m_player.transform.position.y - transform.position.y;
        if (Mathf.Abs(yDiff) <= m_chargeMinRange)
        {
            //Charge at the player!
            m_lastPlayerDiff = m_player.transform.position.x - transform.position.x;
            m_vel.x = m_changeSpeed * Mathf.Sign(m_lastPlayerDiff);
            m_timer = 0;
            m_currentState = State.Charging;
            return;
        }

        m_timer += Time.deltaTime;
        if (m_timer >= m_moveDuration)
        {
            //No longer on the ground, fall.
            m_timer = 0.0f;
            m_currentState = State.Idle;
            return;
        }
    }

    void Charging()
    {
        //Charge towards player until you pass it's x position.
        ApplyVelocity();

        float xDiff = m_player.transform.position.x - transform.position.x;
        if (Mathf.Sign(m_lastPlayerDiff) != Mathf.Sign(xDiff))
        {
            //Charge at the player!
            m_vel.x = 0.0f;
            m_timer = 0;
            m_currentState = State.ChargingCooldown;
            return;
        }
    }

    void ChargingCooldown()
    {
        if (CheckState(State.Death))
        {
            print("Death State called Inside ChargeCooldown");
        }

        m_timer += Time.deltaTime;
        if (m_timer >= m_chargeCooldownDuration)
        {
            //No longer on the ground, fall.
            m_timer = 0.0f;
            m_currentState = State.Idle;
            return;
        }
    }

    private void Dead()
    {
        print("DeadCalled");
        
        DeadAnimation();
        m_isDead = true;
    }


    private bool CheckState(State state)
    {
        return m_currentState == state;
    }

    private void DeadAnimation()
    {
        if (m_isDead) return;

        Sequence deathSequence = DOTween.Sequence();

        Vector3 intialScale = m_sprite.transform.localScale;


        #region Effect1
        //deathSequence.Append(m_sprite.transform.DOLocalJump(
        //    new Vector3(transform.position.x, transform.position.y+3,transform.position.z),
        //    0.2f,1,0.5f))

        //    .SetEase(Ease.InOutBounce)
        //    .Append(m_sprite.transform.DOShakeScale(1,5,20))

        //    .OnComplete(()=> { DestroyEnemy(); });
        #endregion


        #region Effect2
        deathSequence.Append(m_sprite.transform.DOBlendableScaleBy(Vector3.one * 1.5f, 1.5f))
            .Append(m_sprite.transform.DOScale(Vector3.one * 0.2f, 0.5f))
            .SetEase(Ease.InBounce)

            .OnComplete(() => { DestroyEnemy(); });
        #endregion
    }

    void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    
    void BulletHit()
    {
       
        if (CheckState(State.Death))
        {
            return; // Don't run bullet hit logic if dead.
        }

        if (m_isBulletHit)
        {
            print("BulletHit");
            //bulletHitSequence = DOTween.Sequence();

            //bulletHitSequence.Append(m_sprite.DOFade(0.5f, 0.1f))
            //    .Append(m_sprite.DOFade(1f, 0.1f))
            //    .SetLoops(10, LoopType.Yoyo)
            //    .SetEase(Ease.InOutExpo)
            //    .OnComplete(() =>
            //    {


            //        m_sprite.DOFade(1, 0);
            //        ChangeEnemyState(State.Idle);


            //    });

            m_isBulletHit = false;
        }
    }

    void ApplyVelocity()
    {
        Vector3 pos = m_rigidBody.transform.position;
        pos.x += m_vel.x * Time.fixedDeltaTime;
        pos.y += m_vel.y * Time.fixedDeltaTime;
        m_rigidBody.transform.position = pos;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ProcessCollision(collision);    
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
    }

    private void ProcessCollision(Collision2D collision)
    {
        Vector3 pos = m_rigidBody.transform.position;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            //Push back out
            Vector2 impulse = contact.normal * (contact.normalImpulse / Time.fixedDeltaTime);
            pos.x += impulse.x;
            pos.y += impulse.y;

            if (Mathf.Abs(contact.normal.y) < Mathf.Abs(contact.normal.x))
            {
                if ((contact.normal.x > 0 && m_vel.x < 0) || (contact.normal.x < 0 && m_vel.x > 0))
                {
                    m_vel.x = 0;
                    //Stop us.
                    m_wallFlags = (contact.normal.x < 0) ? WallCollision.Left : WallCollision.Right;

                    PlayOnShot(m_wallFlags == WallCollision.Left ? PlayerAnimationsStrings.m_rightHit : PlayerAnimationsStrings.m_leftHit,0.5f);

                    m_currentState = State.Idle;
                    m_timer = 0;
                }
            }
        }
        m_rigidBody.transform.position = pos;
    }


    public void PlayAnimation(string animationName)
    {
        if(m_playOneShot) return;

        m_animator.Play(animationName);
    }

    public void PlayOnShot(string animationName, float duration = 0.1f)
    {
      
        if (!m_playOneShot)
        {
            print("PlayOneShot");
            m_playOneShot = true;
              m_animator.playbackTime = 0;
            m_animator.Play(animationName);
            StartCoroutine(WaitForTime(0.3f));
        }
       
    }

    private IEnumerator WaitForTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        m_playOneShot = false;
    }

    public void InvisbleSprite()
    {
        m_sprite.transform.DOBlendableScaleBy(Vector3.one*0.1f,0.1f).SetEase(Ease.InBounce);
    }
}
