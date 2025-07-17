using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RPSUIManager : MonoBehaviour
{
    public RPSInputHandler inputHandler;
    public Button rockBtn;
    public Button paperBtn;
    public Button scissorsBtn;

    public TMP_Text remainingTime;

    public TMP_Text myChoiceText;
    public TMP_Text otherChoiceText;
    public TMP_Text resultText;

    void Awake()
    {
        inputHandler = FindObjectOfType<RPSInputHandler>();
    }
    
    void Start()
    {
        rockBtn.onClick.AddListener(() => inputHandler.SetChoice(RPSChoice.ROCK));
        paperBtn.onClick.AddListener(() => inputHandler.SetChoice(RPSChoice.PAPER));
        scissorsBtn.onClick.AddListener(() => inputHandler.SetChoice(RPSChoice.SCISSORS));
    }

    public void PrintRemainingTime(float rtime)
    {
        remainingTime.text = $"remaining time : {(int)rtime}";
    }

    public void ShowResult(RPSChoice myChoice, RPSChoice otherChoice, string result)
    {
        myChoiceText.text = $"me : {myChoice}";
        otherChoiceText.text = $"opponent : {otherChoice}";
        resultText.text = result;
    }
}