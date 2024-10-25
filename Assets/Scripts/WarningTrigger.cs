using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningTrigger : MonoBehaviour
{
    public Spikes m_spike;

    private const int PLAYER_LAYER = 1 << 6;

    private bool m_playerTriggered;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((PLAYER_LAYER & (1 << collision.gameObject.layer)) != 0)
        {
            m_playerTriggered = true;

            m_spike.PlayAnimation(Spikes.SpikeAnimationStrings.m_spikeUp);
        }
    }
}
