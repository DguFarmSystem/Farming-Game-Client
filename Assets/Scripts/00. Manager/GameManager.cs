// System
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public int money = 0; //돈



    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    #endregion

    private Scene scene = new Scene();
    private ResourceManager resourceManager = new ResourceManager();
    private SoundManager soundManager = new SoundManager();


    public static Scene Scene { get { return Instance.scene; } }
    public static ResourceManager Resource { get { return Instance.resourceManager; } }
    public static SoundManager Sound { get { return Instance.soundManager; } }
}
