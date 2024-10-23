using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Spikes : MonoBehaviour {

    private SpriteRenderer m_sprite = null;
    private Sequence spikeSequence;
    private Color m_defaultColor = new Color();
    private Color m_damagedColor = new Color(1,0,0);
    // Use this for initialization

    private float m_intialAnimationStartY;
    private bool m_playerHit =false;

   

    private const int PLAYER_MASK = 1 << 6;

    void Start()
    {
        m_sprite = transform.GetComponent<SpriteRenderer>();
        m_defaultColor = m_sprite.color;

        m_intialAnimationStartY =  m_sprite.transform.localPosition.y;

        m_sprite.transform.localPosition = new Vector3(m_sprite.transform.localPosition.x,
            m_intialAnimationStartY -2,
            m_sprite.transform.localPosition.z);

        SpikeAnimation();
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    void SpikeAnimation()
    {
         spikeSequence = DOTween.Sequence();
       
         
        spikeSequence.Append(m_sprite.transform.DOMoveY(m_intialAnimationStartY, 0.1f)  // Fast spike up (flash)
            .SetEase(Ease.InOutFlash))                              
            .AppendInterval(2)                                      
                                                                    
            .Append(m_sprite.transform.DOMoveY(-11, 0.5f)           
            .SetEase(Ease.InOutFlash))                              
            .AppendInterval(0.5f)                                
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        m_sprite.color = new Color(m_defaultColor.r / 2, m_defaultColor.g / 2, m_defaultColor.b / 2, 1);

        if ((PLAYER_MASK & (1 << collision.gameObject.layer)) != 0)
        {
            HandleDamage(collision.gameObject);

            PauseSpikeAnimation();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        m_sprite.color = m_playerHit ? (m_damagedColor) : m_defaultColor;

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

            mPlayer.ShakeCamera();
        }

    }

    private void ChangeSpriteColor(Color color)
    {
        m_sprite.color = color; 
    }
}
