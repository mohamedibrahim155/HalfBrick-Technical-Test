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
    public float m_stunDuration = 0.5f;
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

    [SerializeField] private Vector2 m_Impulse;
    [SerializeField] private bool m_isDead = false;  
    private bool m_isBulletHit = false;  
    private bool m_playOneShot = false;
    private bool m_isStunned = false;
    private Vector2 m_vel = new Vector2(0, 0);

    private Sequence bulletHitSequence;



    public  class EnemyAnimationStrings
    {
        public static string m_idle = "Idle";
        public static string m_bottomHit = "BottomHit";
        public static string m_leftHit = "LeftHit";
        public static string m_leftHitLoop = "LeftHitLoop";
        public static string m_rightHit = "RightHit";
        public static string m_rightHitLoop = "RightHitLoop";
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
        Death,
        Stun

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
            case State.Stun:
                Stunned();
                break;

            default:
                break;
        }

        m_wallFlags = WallCollision.None;

    }

    


    #region States
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

        PlayAnimation(EnemyAnimationStrings.m_idle);
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

        PlayAnimation(EnemyAnimationStrings.m_idle);
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

        PlayAnimation(EnemyAnimationStrings.m_idle);
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

    #region Dead
    private void Dead()
    {

        DeadAnimation();
        m_isDead = true;
    }
    private void DeadAnimation()
    {
        if (m_isDead) return;

        Sequence deathSequence = DOTween.Sequence();

        deathSequence.AppendCallback(() =>
        {
            PlayOnShot(EnemyAnimationStrings.m_bottomHit, 0.05f);
            m_player.DoCameraShake(transform.position, m_Impulse);
        })
          .AppendInterval(0.08f)
         // .Append(m_sprite.transform.DOScaleY(0.1f, 0.2f))
         // .Join(m_sprite.transform.DOLocalMoveY(-24.0f, 0.2f))
         
          .OnComplete(() => {

              DestroyEnemy();

          });

    }

    void DestroyEnemy()
    {
        Destroy(gameObject);
    }
    #endregion

    void BulletHit()
    {
        if (CheckState(State.Death)) return;

     
        m_timer += Time.deltaTime;

        if (m_timer >= m_holdDuration)
        {
            m_timer = 0;
            ChangeEnemyState(State.Idle);
            return;
        }
    }

    private void Stunned()
    {


        m_timer += Time.deltaTime;
        if (m_timer >= m_stunDuration)
        {
            m_isStunned = false;
            m_timer = 0;
            ChangeEnemyState(State.Idle);
            return;
        }
    }

    #endregion

    void ApplyVelocity()
    {
        Vector3 pos = m_rigidBody.transform.position;
        pos.x += m_vel.x * Time.fixedDeltaTime;
        pos.y += m_vel.y * Time.fixedDeltaTime;
        m_rigidBody.transform.position = pos;
    }


    #region Collisions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision);

        if (m_wallFlags == WallCollision.Right || m_wallFlags == WallCollision.Left)
        {
            if (CheckState(State.Stun)) return;

            ChangeEnemyState(State.Stun);
            PlayOnShot(m_wallFlags == WallCollision.Left ? EnemyAnimationStrings.m_rightHit : EnemyAnimationStrings.m_leftHit, 0.5f);

        }

         const int PLAYER_LAYER = 1 << 6;
        if ((PLAYER_LAYER & (1 << collision.gameObject.layer)) != 0)
        {
            m_player.PlayHitReaction();
        }
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
                    m_currentState = State.Idle;
                    m_timer = 0;
                }
            }
        }
        m_rigidBody.transform.position = pos;
    }

    public void InflictDamage(float damageAmount)
    {
        m_isBulletHit = true;
        m_health -= damageAmount;

        m_player.DoCameraShake(transform.position, m_Impulse);
        if (m_health <= 0.0f)
        {
            m_vel = Vector2.zero;
            m_timer = 0;
            ChangeEnemyState(State.Death);
            return;
        }
        else
        {
            m_vel = Vector2.zero;
            m_timer = 0;

            m_isBulletHit = true;

            float diff = Mathf.Sign(m_lastPlayerDiff);
            PlayOnShot((diff > 0 ? EnemyAnimationStrings.m_rightHit : EnemyAnimationStrings.m_leftHit), 0);

            ChangeEnemyState(State.Bullet_Hit);
            return;
        }
    }
    #endregion

    #region AnimationMethods
    public void PlayAnimation(string animationName)
    {
        if(m_playOneShot) return;

        m_animator.Play(animationName);
    }

    public void PlayOnShot(string animationName, float duration = 0.05f)
    {
        StopCoroutine(WaitForTime(duration));
        if (!m_playOneShot)
        {
            m_playOneShot = true;
            m_animator.Play(animationName);
            
            StartCoroutine(WaitForTime(duration));
        }
       
    }

    private IEnumerator WaitForTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        m_playOneShot = false;
    }
    #endregion

    #region StateHandlers
    private bool CheckState(State state)
    {
        return m_currentState == state;
    }

    public void ChangeEnemyState(State enemyState)
    {
        if (m_currentState != enemyState)
        {
            m_currentState = enemyState;
        }
    }
    #endregion
}
