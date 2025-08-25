// System
using System.Collections;
using System.Collections.Generic;

// Unity
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton

    private static GameManager instance;
    public static GameManager Instance // 없으면 자동생성
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    var go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
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

    //Temp
    public int playerLV = 1;

    private Scene scene = new Scene();
    private ResourceManager resourceManager = new ResourceManager();
    private SoundManager soundManager = new SoundManager();


    public static Scene Scene { get { return Instance.scene; } }
    public static ResourceManager Resource { get { return Instance.resourceManager; } }
    public static SoundManager Sound { get { return Instance.soundManager; } }
}
