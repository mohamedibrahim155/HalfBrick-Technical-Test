using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class ScrollingBackground : MonoBehaviour
{
    public GameObject m_background;
    public float m_speed;
    
    private float m_endPosition;
    private Vector3 m_startPosition;

    private void Start()
    {
        m_startPosition =  m_background.transform.position;
        m_endPosition =  m_background.transform.position.y - 120;
    }
    void Update() => MoveBackgorund();
    void RestOldPosition() => m_background.transform.position = m_startPosition;
    private void MoveBackgorund()
    {
        
        m_background.transform.position += Vector3.down * m_speed * Time.deltaTime;

        if (m_background.transform.position.y <= m_endPosition)
            RestOldPosition();
    }
}
