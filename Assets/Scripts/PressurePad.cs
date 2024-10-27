using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class PressurePad : MonoBehaviour {

    private SpriteRenderer m_sprite = null;
    private Animator m_animator;
    private Color m_defaultColor = new Color();

    [SerializeField] private Vector2 m_JumpPadImpulse = new Vector2(0, 0);
    [SerializeField] private float m_forcePush = 5;

    private int JumpHash;
    // Use this for initialization
    void Start () {
        JumpHash = Animator.StringToHash("Jump");
        m_sprite = transform.GetComponent<SpriteRenderer>();
        m_animator = transform.GetComponent<Animator>();
        m_defaultColor = m_sprite.color;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        m_sprite.color = new Color(m_defaultColor.r / 2, m_defaultColor.g / 2, m_defaultColor.b / 2, 1);

        ProcessCollision(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        m_sprite.color = m_defaultColor;
    }

    private void ProcessCollision(Collider2D collision)
    {
        // Unity Player layer
        const int playerMask = 1 << 6;

        // Collision with player
        if ((playerMask & (1 << collision.gameObject.layer)) != 0)
        {
            Player player = collision.gameObject.GetComponent<Player>();

            if (player != null)
            {
                // adding additional force based on jump pad
               player.AddAdditionalJumpForce(m_forcePush);

                var position = collision.gameObject.transform.position;
                player.DoCameraShake(position, m_JumpPadImpulse);

                TriggerAnimation();
            }
        }
    }

    private void TriggerAnimation()
    {
        m_animator.ResetTrigger(JumpHash);
        m_animator.SetTrigger(JumpHash);
    }
}
