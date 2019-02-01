using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Assets.Assets.Scripts;
using TMPro;

public class GameManager : MonoBehaviour
{
    TextMeshProUGUI coinsText = null;
    TextMeshProUGUI timeText = null;
    TextMeshProUGUI nameText = null;

    XmlFileWriter xmlFileWriter;
    Task coinsGenerator;
    BoardManager _boardManager;
    private Enemy[] enemies = new Enemy[3];
    private int enemyAmountOfSteps;
    private int currentAdditionalSpeedCoins;
    private int ableToSmellThePlayerUnits;
    private int spawnDistanceBetweenPlayerAndEnemies;
    private float additionalSpeedValue;

    private float _seconds = 0;
    private int _minutes = 0;
    private int _hours = 0;

    public static GameManager instance; // this instance.
    public int coinsToSpawnAnEnemy;
    public int coinsToStartHardMode;

    private DateTime StartingToPlayDate { get; set; }

    private string TimeLeft
    {
        get
        {
            // if less than 10, add 0 before the second digit, e.g 09:08:07

            int secondsValue = (int)_seconds;

            string hours = _hours / 10 < 1 ? "0" + _hours.ToString() : _hours.ToString();
            string minutes = _minutes / 10 < 1 ? "0" + _minutes.ToString() : _minutes.ToString();
            string seconds = secondsValue / 10 < 1 ? "0" + secondsValue.ToString() : secondsValue.ToString();

            return String.Format("{0}:{1}:{2}", hours, minutes, seconds);
        }
    }

    [HideInInspector] public Player Player { private get; set; }
    [HideInInspector] public int PlayerTotalCoins { get; private set; }

    private void Awake()
    {
        instance = this;
        _boardManager = GetComponent<BoardManager>();
        xmlFileWriter = GetComponent<XmlFileWriter>();

        InitGame();
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            LoseGame(true);
            return;
        }

        UpdateTimer();

        if (coinsGenerator == null || !coinsGenerator.Running)
        {
            coinsGenerator = new Task(_boardManager.UpdateCoins());
        }
        UpdateEnemies();

        MoveEnemies();

        UpdateUI();
    }
    private void MoveEnemies()
    {
        foreach (var i in enemies)
        {
            if (i != null)
            {
                if (!i.HasDirection)
                { // doesn't have a direction it should follow - then give it one.
                    Vector2 playerRoundedCoords =
                                                new Vector2(
                                                Mathf.Round(Player.transform.position.x),
                                                Mathf.Round(Player.transform.position.y)
                                                );
                    Vector2 enemyCoords = i.transform.position;

                    bool enemyCanSmellThePlayer =
                        Vector2.Distance(playerRoundedCoords, enemyCoords) <= ableToSmellThePlayerUnits;

                    if (Player.Coins >= coinsToStartHardMode && enemyCanSmellThePlayer)
                    {
                        Vector2[] path = _boardManager.GetPathFromTo(enemyCoords, playerRoundedCoords);

                        i.MoveEnemy(path);
                    }
                    else
                    {
                        Vector2[] randomPath = _boardManager.GetRandomPathWithin(
                            i.transform.position, enemyAmountOfSteps);

                        i.MoveEnemy(randomPath);
                    }
                }
                else if (i.HasDirection)
                    i.MoveEnemy();
            }
            else
                return;
        }
    }
    private void InitGame()
    {
        StartingToPlayDate = DateTime.Now;

        for (int i = 0; i < enemies.Length; i++)
            enemies[i] = null;

        _boardManager.SetupScene();

        // how far enemies' directions are 
        enemyAmountOfSteps = (int)Mathf.Sqrt(_boardManager.rows + _boardManager.columns);
        // hard mode coins: enemy start smelling the player, hard mode + 1 coins: + to enemy speed
        currentAdditionalSpeedCoins = coinsToStartHardMode + 1;
        // how far the player must be enough for the enemies to smell them.
        ableToSmellThePlayerUnits = 5;
        spawnDistanceBetweenPlayerAndEnemies = 20;
        additionalSpeedValue = 1.05f; // 105%
        PlayerTotalCoins = 0;

        if (GameObject.Find("CoinsText") != null)
            coinsText = GameObject.Find("CoinsText").GetComponent<TextMeshProUGUI>();
        if (GameObject.Find("TimeText") != null)
            timeText = GameObject.Find("TimeText").GetComponent<TextMeshProUGUI>();
        if (GameObject.Find("NameText") != null)
            nameText = GameObject.Find("NameText").GetComponent<TextMeshProUGUI>();
    }
    private void UpdateTimer()
    {
        _seconds += Time.deltaTime;

        if (_seconds >= 60)
        {
            _minutes += (int)(_seconds / 60f);
            _seconds %= 60;
        }
        if (_minutes >= 60)
        {
            _hours += _minutes / 60;
            _minutes %= 60;
        }
    }
    private void UpdateEnemies()
    {
        int enemyIndex = Player.Coins / coinsToSpawnAnEnemy;

        if (enemyIndex < enemies.Length && enemies[enemyIndex] == null)
            AddEnemyAt(enemyIndex);
        else if (Player.Coins > currentAdditionalSpeedCoins)
        {
            UpdateSpeedOfEnemies();
            currentAdditionalSpeedCoins = Player.Coins;
        }
    }
    private void AddEnemyAt(int enemyIndex)
    {
        Vector2 roundedPlayerPosition = new Vector2(
            Mathf.Round(Player.transform.position.x),
            Mathf.Round(Player.transform.position.y)
            );

        Vector2 RandomPoint;
        do
        {
            RandomPoint = _boardManager.GetRandomCellCoords();
        } while (
        Vector2.Distance(
            RandomPoint, roundedPlayerPosition
            ) < spawnDistanceBetweenPlayerAndEnemies
        ); // spawn far away from the Player

        _boardManager.SpawnEnemy(
            RandomPoint, Mummy: enemyIndex == 2 ? true : false); // 3rd enemy is mummy
    }
    private void UpdateSpeedOfEnemies()
    {
        foreach (var i in enemies)
        {
            i.moveTime *= additionalSpeedValue; // *= 105%
        }
    }
    private void LoseGame(bool forcedEnd = false)
    {
        PlayerTotalCoins += Player.Coins;

        coinsGenerator.Stop();

        WriteResultsToXml(Player.Name, Player.Coins, TimeLeft, StartingToPlayDate, forcedEnd);

        SceneManager.LoadScene("MainMenu");
    }
    private void WriteResultsToXml(string playerName, int coins, string timeLeft, DateTime date, bool forcedEnd = false)
    {
        Result result = new Result(playerName, coins, timeLeft, date, forcedEnd);
        xmlFileWriter.WriteResultsToXmlFile(result);
    }
    private void UpdateUI()
    {
        if (coinsText != null)
            coinsText.text = "Coins: " + Player.Coins.ToString();
        if (timeText != null)
            timeText.text = "Time: " + TimeLeft;
        if (nameText != null)
            nameText.text = "Name: " + Player.Name;
    }

    public void SpawnPlayer()
    {
        if (Player != null)
        {
            Vector2 position;
            do
            {
                position = _boardManager.GetRandomCellCoords();
            } while (
            Vector2.Distance(
                position, new Vector2(_boardManager.columns / 2, _boardManager.rows / 2)
                ) > 5
            ); // spawn in center

            Player.transform.position = position;
        }
    }
    public void AddEnemyToList(Enemy enemy)
    {
        enemies[Array.FindIndex(enemies, t => t == null)] = enemy;
    }
    public void OnPlayerHasBeenHit(Enemy enemy)
    {
        if (enemy.tag == "Mummy")
        {
            enemy.SetTrigger("AttackPlayer");
            Player.LoseCoins();
            PlayerTotalCoins = 0;
        }
        LoseGame();
    }

}