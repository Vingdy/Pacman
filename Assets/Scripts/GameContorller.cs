using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameContorller : MonoBehaviour
{
    public TextMeshProUGUI  scoreText;
    public TextMeshProUGUI  highScoreText;

    private int score;

    private static int highScore = 0;

    public static GameContorller instance;

    public GameObject gameOverImg;
    public GameObject WinImg;
    public GameObject HintText;


    public bool gameOver = false;

    public Transform beans;

    public GameObject powerUpPrefab;
    public GameObject powerUps;
    public float powerUpSpawnSpeed = 4f;
    public float powerUpSpawnSpeedMax = 8f;
    public float powerUpSpawnSpeedMin = 4f;
    public float powerUpEffectSec = 4f;
    public bool isPlayerPowerUp = false;
    
    [HideInInspector] public List<Vector2> walkableList;

    public Vector2 leftBottom = new Vector2(12.5f, 14f);
    
    public Transform[] cornersTrans; 
    private Vector3[] corners = new Vector3[4]; // 记录逃跑的终点

    public Transform playerTrans;

    public GameObject BlinkyPrefab;
    public Transform BlinkyTrans;
    private Vector3 BlinkyStartPos = new Vector3();
    public GameObject ClydePrefab;
    public Transform ClydeTrans;
    private Vector3 ClydeStartPos = new Vector3();
    public GameObject InkyPrefab;
    public Transform InkyTrans;
    private Vector3 InkyStartPos = new Vector3();
    public GameObject PinkyPrefab;
    public Transform PinkyTrans;
    private Vector3 PinkyStartPos = new Vector3();
    
    public void Start()
    {
        walkableList = GenWalkableList();
        // foreach (var w in walkableList)
        // {
        //     Debug.Log(w);
        // }
        // 
        Invoke("SpawnPowerUp", powerUpSpawnSpeed);

        for (int i = 0; i < cornersTrans.Length; i++)
        {
            corners[i] = cornersTrans[i].position;
        }

        BlinkyStartPos = BlinkyTrans.position;
        ClydeStartPos = ClydeTrans.position;
        InkyStartPos = InkyTrans.position;
        PinkyStartPos = PinkyTrans.position;
    }

    private float curPowerUpEffectTime = 0;
    private float totalPowerUpEffectTime = 0;
    public void Update()
    {
        if (isPlayerPowerUp)
        {
            curPowerUpEffectTime += Time.deltaTime;
            if (curPowerUpEffectTime >= totalPowerUpEffectTime)
            {
                isPlayerPowerUp = false;
                curPowerUpEffectTime = 0;
                totalPowerUpEffectTime = 0;
            }
        }
        if (gameOver&& Input.anyKeyDown)
        {
            SceneManager.LoadScene("Game");
        }
    }

    public void SpawnPowerUp()
    {
        GameObject powerUp = Instantiate(powerUpPrefab);
        powerUp.transform.position = walkableList[Random.Range(0, walkableList.Count)]-leftBottom;
        
        powerUp.transform.SetParent(powerUps.transform);
        
        Invoke("SpawnPowerUp", Random.Range(powerUpSpawnSpeedMin, powerUpSpawnSpeedMax));
    }
    private void Awake()
    {
        instance = this;
    }

    public void OnEatBean()
    {
        score += 1;
        scoreText.text = string.Format("Score:{0}",score);
        SetHighScore();
    }

    public void SetHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            highScoreText.text = string.Format("HighScore:{0}",highScore);
        }
    }

    public void OnMeetMonster(GameObject pacman,GameObject monster)
    {
        if (isPlayerPowerUp)
        {
            Destroy(monster.gameObject);
            switch (monster.gameObject.tag)
            {
                case "Blinky":
                    Invoke("SpawnBlinky", 10f);
                    break;
                case "Clyde":
                    Invoke("SpawnClyde", 10f);
                    break;
                case "Inky":
                    Invoke("SpawnInky", 10f);
                    break;
                case "Pinky":
                    Invoke("SpawnPinky", 10f);
                    break;
            }

        }
        else
        {
            
            gameOver = true;
        
            gameOverImg.SetActive(true);
            HintText.SetActive(true);
        }
    }

    public bool IsWin()
    {
        // Debug.Log(beans.childCount);
        // Debug.Log(beans.childCount > 0);
        return beans.childCount == 0;
    }

    public void OnGameWin()
    {
        gameOver = true;
        WinImg.SetActive(true);
    }
    
    public List<Vector2> GenWalkableList()
    {
        // 生成障碍图
        List<Vector2> walkable = new List<Vector2>();
        // row = 30;
        // col = 26;
        
        for (int beanIndex = 0; beanIndex < beans.childCount; beanIndex++)
        {
            Transform beanTrans = beans.GetChild(beanIndex);
            Vector2 p = beanTrans.position;
            p += leftBottom;
            // Debug.Log(p);
            walkable.Add(p);
        }

        return walkable;
        // Debug.Log(finder);
    }

    public void OnMeetPowerUp(GameObject powerUp)
    {
        isPlayerPowerUp = true;
        totalPowerUpEffectTime += powerUpEffectSec;
        
        Destroy(powerUp.gameObject);
        // 倒计时：1.自己倒计时 2.unity计时器
        // Invoke("LostPowerUp", powerUpEffectSec);
    }

    // private void LostPowerUp()
    // {
    //     isPlayerPowerUp = false;
    // }

    public Vector2 GetEscapeCorner(Vector3 monsterPos)
    {
        List<Vector2> escapable = new List<Vector2>();
        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 playerPos = playerTrans.position;
            float dPlayer = (playerPos - corners[i]).magnitude; // 获取两向量相减长度 -> 终点
            float dMonster = (monsterPos - corners[i]).magnitude;
            if (dMonster < dPlayer)
            {
                escapable.Add(corners[i]);
            }
        }

        Vector2 selectedCorner = escapable[Random.Range(0, escapable.Count)];
        return selectedCorner;
    }
    
    private string[] Monsters = new string[] {"Blinky", "Clyde", "Inky", "Pinky"};
    
    public bool isMonsters(string name)
    {
        foreach (var m in Monsters)
        {
            if (m == name)
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnBlinky()
    {
        Instantiate(BlinkyPrefab, BlinkyStartPos, Quaternion.identity);
    }

    private void SpawnClyde()
    {
        Instantiate(ClydePrefab, ClydeStartPos, Quaternion.identity);
    }
    private void SpawnInky()
    {
        Instantiate(InkyPrefab, InkyStartPos, Quaternion.identity);
    }
    private void SpawnPinky()
    {
        Instantiate(PinkyPrefab, PinkyStartPos, Quaternion.identity);
    }
}
