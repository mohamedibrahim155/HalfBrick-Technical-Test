using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    public static bool exists
    {
        get
        {
            return instance != null;
        }
    }

    private static T instance = null;
    public static T Instance
    {
        get
        {
            //if it has no prior instance, find one
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(T)) as T;
            }

            return instance;
        }
        set
        {
            if (instance == null)
            {
                instance = value;
            }
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void OnApplicationQuit()
    {
        instance = null;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}