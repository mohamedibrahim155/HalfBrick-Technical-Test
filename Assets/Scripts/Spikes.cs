using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Spikes : MonoBehaviour {

    private SpriteRenderer m_sprite = null;
    private Sequence spikeSequence;
    private Animator m_animator;
    private Color m_defaultColor = new Color();
    private Color m_damagedColor;
    // Use this for initialization

    private float m_intialAnimationStartY;
    private bool m_playerHit = false;

    private const int PLAYER_MASK = 1 << 6;

    [SerializeField] private Vector2 m_Impulse = Vector2.zero;

    public class SpikeAnimationStrings
    {
        public static string m_spikeUp = "SpikeUp";
    }

    void Start()
    {
        m_sprite = transform.GetComponent<SpriteRenderer>();
        m_animator = transform.GetComponent<Animator>();
        m_defaultColor = m_sprite.color;
        m_damagedColor =Color.red;

        Initialise();

        //SpikeAnimation();
    }

    void Initialise()
    {
        m_intialAnimationStartY = m_sprite.transform.localPosition.y;

        m_sprite.transform.localPosition = new Vector3(m_sprite.transform.localPosition.x,
            m_intialAnimationStartY,
            m_sprite.transform.localPosition.z);
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    #region Animation Handlers
    void SpikeAnimation()
    {
         spikeSequence = DOTween.Sequence();
       
         
        spikeSequence.Append(m_sprite.transform.DOMoveY(m_intialAnimationStartY, 0.1f)  // Fast spike up (flash)
            .SetEase(Ease.Linear))                              
            .AppendInterval(1)                                      
                                                                    
            .Append(m_sprite.transform.DOMoveY(-12, 0.25f)           
            .SetEase(Ease.InOutFlash))                              
            //.AppendInterval(2)
            .Append(m_sprite.transform.DOMoveX(-31.2f, 0.25f))
            .SetEase(Ease.InOutFlash)
            .Append(m_sprite.transform.DOMoveY(m_intialAnimationStartY, 0.25f))
            .SetEase(Ease.InOutFlash)
            .Append(m_sprite.transform.DOMoveX(-39.0f, 0.25f))
            .SetEase(Ease.InOutFlash)
            .SetLoops(-1);                                          
    }

    void PauseSpikeAnimation()
    {
        spikeSequence.Pause();
        
    }
    void PlayAnimation()
    {
        spikeSequence.Play();
    }
    public void PlayAnimation(string animationName)
    {
        m_animator.Play(animationName);
    }
    #endregion


    #region Collisions
    private void OnTriggerEnter2D(Collider2D collision)
    {

        ChangeSpriteColor(new Color(m_defaultColor.r / 2, m_defaultColor.g / 2, m_defaultColor.b / 2, 1));

        if ((PLAYER_MASK & (1 << collision.gameObject.layer)) != 0)
        {
            HandleDamage(collision.gameObject);

            PauseSpikeAnimation();

            Player.Instance.CallPlayerDieExternally();
            
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        ChangeSpriteColor(m_playerHit ? (m_damagedColor) : m_defaultColor);

        PlayAnimation();
    }

    private void HandleDamage(GameObject player)
    {
        Player mPlayer = player.GetComponent<Player>();
        
        if (mPlayer != null)
        {
            m_playerHit = true;
            ChangeSpriteColor(m_damagedColor);

            // Restart player Die
            mPlayer.DoCameraShake(transform.position, m_Impulse);
        }

    }
    #endregion

    private void ChangeSpriteColor(Color color)
    {
        m_sprite.color = color; 
    }

    
}
