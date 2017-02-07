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
    public Text scoreUI;
    public Text gameOverScore;
    public RectTransform healthBar;

    Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
    }

    void Awake()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void Update()
    {
        scoreUI.text = ScoreKeeper.score.ToString("D6");
        float healthPercent = 0;
        if (player != null)
        {
            healthPercent = player.health / (float)player.startingHealth;
        }
        healthBar.localScale = new Vector3(healthPercent, 1, 1);
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
        while (percent >= 0)
        {
            //From -150
            //To 0
            percent += Time.deltaTime * speed * dir;

            if (percent >= 1)
            {
                percent = 1;
                if (Time.time > endDelayTime)
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
        Cursor.visible = true;
        StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, 0.85f), 1));
        gameOverScore.text = "Score: " + ScoreKeeper.score;
        scoreUI.gameObject.SetActive(false);
        healthBar.transform.parent.gameObject.SetActive(false);
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
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

    public void ReturnMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
