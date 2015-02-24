using UnityEngine;
using System.Collections;

public class ChangeScene : MonoBehaviour {

    public int sceneNumber;
    public float fadeOutSeconds=5;
    public float fadeInSeconds= 5;
    public GameObject fadeOut;
    public GameObject fadein;
    

    private static ChangeScene m_Instance = null;

    private static ChangeScene Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = (new GameObject("ChangeScene")).AddComponent<ChangeScene>();
            }
            return m_Instance;
        }
    }
    private void Awake()
    {
        DontDestroyOnLoad(this);
        m_Instance = this;
    }

    public void stuff()
    {
        StartCoroutine("change");
    }

    private IEnumerator change()
    {
        
        Instantiate(fadeOut);
        yield return new WaitForSeconds(fadeOutSeconds);
        Application.LoadLevel(sceneNumber);
        Debug.Log("instantiate circleIN");
        Instantiate(fadein);
        yield return new WaitForSeconds(fadeInSeconds);
       
        Destroy(this.gameObject);
    }
}

