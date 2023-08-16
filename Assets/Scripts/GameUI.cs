using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Game Override")]
    public Image fadeImage;
    public GameObject gameOverrideUI;
    public TMP_Text gameOverScore;

    [Header("Banner ")]
    public RectTransform newWaveBanner;
    public TMP_Text newWaveTitle;
    public TMP_Text newWaveEnemyCount;
    private Spawner spawner;

    [Header("Heath Bar")] 
    public RectTransform healthBar;

    public TMP_Text score;
    private Player player;
    private void Start()
    {
        player = FindObjectOfType<Player>();
        player.onDeath += () =>
        {
            StartCoroutine(Fade(Color.clear,new Color(0,0,0,.95f), 1));
            gameOverScore.text = score.text;
            score.gameObject.SetActive(false);
            healthBar.transform.parent.gameObject.SetActive(false);
            gameOverrideUI.gameObject.SetActive(true);
        };
    }

    private void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    private void Update()
    {
        score.text = ScoreKeeper.score.ToString("D6");
        float heathPercent = 0;
        if (player != null)
        {
            heathPercent = player.health / player.startingHealth;
        }
        healthBar.localScale = new Vector3(heathPercent, 1, 1);
    }

    private void OnNewWave(int waveNumber)
    {
        string[] numbers = { "One", "Two", "Three", "Four", "Five" };
        newWaveTitle.text = "- Wave " + numbers[waveNumber - 1] + " -";
        string enemyCountString = ((spawner.waves[waveNumber - 1]).infinite)
            ? "Infinite"
            : (spawner.waves[waveNumber - 1].enemyCount + "");
        newWaveEnemyCount.text = "Enemies: " + enemyCountString;
        
        StopCoroutine(nameof(AnimateNewWaveBanner));
        StartCoroutine(nameof(AnimateNewWaveBanner));
    }
    IEnumerator AnimateNewWaveBanner()
    {
        float delayTime = 1.5f;
        float speed = 3f;
        float animatePercent = 0;
        int dir = 1;

        float endDelayTime = Time.time - 1 / speed + delayTime;
        while (animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;
            if (animatePercent >= 1)
            {
                animatePercent = 1;
                if (Time.time > endDelayTime)
                {
                    dir = -1;
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-250, 70, animatePercent);
            yield return null;
        }
    }
    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadeImage.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    public void StartNewGame()
    {
        Cursor.visible = false;
        SceneManager.LoadScene("Game");
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
