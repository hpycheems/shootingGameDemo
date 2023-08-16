using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score { get; set; }
    private float lastEnemyKillTime;
    private int streakCount;
    private float streakExpriyTime = 1;

    private void Start()
    {
        Enemy.onDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player>().onDeath += OnPlayerDeath;
    }

    void OnEnemyKilled()
    {
        if (Time.time < lastEnemyKillTime + streakExpriyTime)
        {
            streakCount++;
        }
        else
        {
            streakCount = 0;
        }

        lastEnemyKillTime = Time.time;

        score += 5 + (int)Mathf.Pow(2, streakCount);
    }

    void OnPlayerDeath()
    {
        Enemy.onDeathStatic -= OnPlayerDeath;
    }
}
