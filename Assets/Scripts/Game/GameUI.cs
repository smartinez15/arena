using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;

    public RectTransform levelBanner;
    public Text levelTitle;

    void Start()
    {
        FindObjectOfType<Player>().OnDeath += OnGameOver;
    }

    void Awake()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        levelTitle.text = "Level " + (waveNumber + 1);

        StopCoroutine("AnimateBanner");
        StartCoroutine("AnimateBanner");
    }

    IEnumerator AnimateBanner()
    {
        float speed = 2f;
        float delayTime = 1.5f;
        float dir = 1;
        float endDelayTime = Time.time + 1 / speed + delayTime;

        float percent = 0;
        while(percent >= 0)
        {
            //From -150
            //To 0
            percent += Time.deltaTime * speed * dir;

            if(percent >= 1)
            {
                percent = 1;
                if(Time.time > endDelayTime)
                {
                    dir = -1;
                } 
            }

            levelBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-170, 0, percent);
            yield return null;
        }
    }

    void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, Color.black, 1));
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while(percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    //UI Input
    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }
}
