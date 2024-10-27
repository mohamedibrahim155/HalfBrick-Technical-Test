using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
  private Animator m_animator;
    void Start()
    {
        m_animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        Player.Instance.OnPlayerDead += (_) => OnSceneTransition();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void OnSceneTransition()
    {
        StartCoroutine(PlayTransition());
    }

    IEnumerator PlayTransition()
    {
        yield return new WaitForSeconds(0.5f);
        m_animator.CrossFade("FadeIn",0.1f);
        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
