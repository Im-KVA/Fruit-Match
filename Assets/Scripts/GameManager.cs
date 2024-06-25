using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; //static

    public GameObject backGround; //white
    public GameObject victoryPanel;
    public GameObject losePanel;

    public int goal; //Điểm cần để win
    public int moves; //Số lượt di chuyển được cho phép
    public int points; //Điểm

    public TMP_Text pointsTxt;
    public TMP_Text movesTxt;
    public TMP_Text goalTxt;
    public TMP_Text winSmallTxt;
    public TMP_Text loseSmallTxt;

    public bool isGameEnded;

    private void Awake()
    {
        Instance = this;
    }
    public void Initialized(int tmp_moves, int tmp_goal)
    {
        moves = tmp_moves;
        goal = tmp_goal;
    }

    // Update is called once per frame
    void Update()
    {
        pointsTxt.text = "Points: " + points.ToString();
        movesTxt.text = "Moves: " + moves.ToString();
        goalTxt.text = "Goal: " + goal.ToString(); 
    }
    public void ProcessTurn(int pointsToWin, bool subtractMoves)
    {
        points += pointsToWin;
        if (subtractMoves)
        {
            moves--;
        } 
        //Win
        if (points >= goal)
        {
            isGameEnded = true;
            backGround.SetActive(true);
            victoryPanel.SetActive(true);
            FruitBoard.instance.fruitParent.SetActive(false);
            winSmallTxt.text = "You are so good, you win the game with " + points.ToString() + " points and still have " + moves.ToString() + " moves!";
            return;
        } 
        //Lose
        if (moves == 0)
        {
            isGameEnded = true;
            backGround.SetActive(true);
            losePanel.SetActive(true);
            FruitBoard.instance.fruitParent.SetActive(false);
            loseSmallTxt.text = "Oh no, you lose the game, you only have " + points.ToString() + " points, your goal is " + goal.ToString() + ", it's ok, better luck next time!";
            return;
        } 
    }    
    public void WinGame()
    {
        SceneManager.LoadScene(0);
    }
    public void LoseGame()
    {
        SceneManager.LoadScene(0);
    }
}
