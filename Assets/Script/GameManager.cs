using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Networking;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private List<Card> allCards;

    private Card flippedCard;

    private bool isFlipping = false;

    [SerializeField]

    private Slider timeoutSlider;

    [SerializeField]

    private TextMeshProUGUI timeoutText;

    [SerializeField]

    private TextMeshProUGUI gameOverText;

    [SerializeField]

    private GameObject gameOverPanel;
    private bool isGameOver = false;

    [SerializeField]

    private float timeLimit = 30f;
    private float currentTime;
    private int totalMatches = 10;
    private int matchesFound = 0;

    [SerializeField]
    private string googleFormLink = "https://docs.google.com/forms/d/1kayqeWWGIZNLung4Y7HYHnGsLLk2fIxtwfeVIizjPmo/formResponse";



    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Board board = FindObjectOfType<Board>();
        allCards = board.GetCards();

        currentTime = timeLimit;
        SetCurrentTimeText();
        StartCoroutine("FlipAllCardsRoutine");
    }

    void SetCurrentTimeText()
    {
        int TimeSec = Mathf.CeilToInt(currentTime);
        timeoutText.SetText(TimeSec.ToString());;
    }

    IEnumerator FlipAllCardsRoutine()
    {
        isFlipping = true;
        yield return new WaitForSeconds(0.5f);
        FlipAllCards();
        yield return new WaitForSeconds(3f);
        FlipAllCards();
        yield return new WaitForSeconds(0.5f);
        isFlipping = false;

        yield return StartCoroutine("CountDownTimerRoutine");
    }

    IEnumerator CountDownTimerRoutine()
    {
        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            timeoutSlider.value = currentTime / timeLimit;
            SetCurrentTimeText();
            yield return null;
        }

        GameOver(false);
    }

    void FlipAllCards()
    {
        foreach (Card card in allCards)
        {
            card.FlipCard();
        }
    }

    public void CardClicked(Card card)
    {
        if (isFlipping || isGameOver)
        {
            return;
        }

        card.FlipCard();

        if (flippedCard == null)
        {
            flippedCard = card;
        }
        else
        {
            StartCoroutine(CheckMatchRoutine(flippedCard, card));
        }
    }

    IEnumerator CheckMatchRoutine(Card card1, Card card2)
    {
        isFlipping = true;

        if (card1.cardID == card2.cardID)
        {
            card1.SetMatched();
            card2.SetMatched();
            matchesFound++;

            if (matchesFound == totalMatches)
            {
                GameOver(true);
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);

            card1.FlipCard();
            card2.FlipCard();

            yield return new WaitForSeconds(0.4f);
        }

        isFlipping = false;
        flippedCard = null;
    }

    void GameOver(bool success)
    {

        if (!isGameOver)
        {
            isGameOver = true;

            StopCoroutine("CountDownTimerRoutine");

            if (success)
            {
                gameOverText.SetText("Great Job");
                //SendLog(currentTime.ToString("0.00"));
            }
            else
            {
                gameOverText.SetText("GAME OVER");
            }

            Invoke("ShowGameOverPanel", 2f);
        }
        
    }

    void ShowGameOverPanel()
    {   
        gameOverPanel.SetActive(true);
    }

    public void Restart()
    {
        StopCoroutine("Post");
        SceneManager.LoadScene("SampleScene");
    }

    public void OnApplicationQuit()
    {
        SendLog(currentTime.ToString("0.00"));
    }

    public void SendLog(string clearTime) {
        StartCoroutine(Post(Environment.UserName, "EASY", clearTime));
    }

    IEnumerator Post(string name, string gameversion, string playtime) {
        Debug.Log("[LOGGER] Post Called!");
        WWWForm form = new WWWForm();
        form.AddField("entry.1601783318", name);
        form.AddField("entry.1444102313", gameversion);
        form.AddField("entry.1316183563", playtime);

        UnityWebRequest www = UnityWebRequest.Post(googleFormLink, form);

        www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("[LOGGER] Failed form upload:" + www.error);
        }
        else
        {
            Debug.Log("[LOGGER] Uploaded form!");
        }

        yield return null;
    }


}
