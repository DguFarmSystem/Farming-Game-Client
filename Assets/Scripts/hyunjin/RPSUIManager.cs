using UnityEngine;
using UnityEngine.UI;

public class RPSUIManager : MonoBehaviour
{
    public InputHandler inputHandler;
    public Button rockBtn;
    public Button paperBtn;
    public Button scissorsBtn;
    
    void Start()
    {
        rockBtn.onClick.AddListener(() => inputHandler.SetChoice(RPSChoice.ROCK));
        paperBtn.onClick.AddListener(() => inputHandler.SetChoice(RPSChoice.PAPER));
        scissorsBtn.onClick.AddListener(() => inputHandler.SetChoice(RPSChoice.SCISSORS));
    }
}