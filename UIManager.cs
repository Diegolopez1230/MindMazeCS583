using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject titlePanel; // main menu / title screen panel
    public GameObject hudPanel;   // in-game HUD panel
    public GameObject winPanel;   // panel when player completes a round
    public GameObject losePanel;  // panel when player fails a round

    [Header("HUD Elements")]
    public TextMeshProUGUI roundText; // shows current round number
    public TextMeshProUGUI revealCountdownText; // shows "Memorize!" countdown before play starts

    [Header("Win Panel")]
    public TextMeshProUGUI winRoundText; // shows completed round message

    [Header("Lose Panel")]
    public TextMeshProUGUI loseRoundText; // shows failed round message

    // panel control: hides all UI panels so only the one we want is shown
    public void HideAll()
    {
        titlePanel?.SetActive(false);
        hudPanel?.SetActive(false);
        winPanel?.SetActive(false);
        losePanel?.SetActive(false);
    }

    // title screen and hides everything else
    public void ShowTitleScreen()
    {
        HideAll();
        titlePanel?.SetActive(true);
    }

    // in-game HUD and updates the round label
    public void ShowHUD(int round)
    {
        hudPanel?.SetActive(true);

        // update round number text
        if (roundText)
            roundText.text = $"Round {round}";

        // get rid of any leftover countdown text
        if (revealCountdownText)
            revealCountdownText.text = "";
    }

    // starts on-screen reveal countdown ("Memorize! 3, 2, 1...")
    public void ShowRevealCountdown(float seconds)
    {
        if (revealCountdownText)
            StartCoroutine(CountdownRoutine(seconds));
    }

    // coroutine that handles the countdown display over time
    private System.Collections.IEnumerator CountdownRoutine(float total)
    {
        float remaining = total;

        // count down until time reaches 0
        while (remaining > 0f)
        {
            if (revealCountdownText)
                revealCountdownText.text = $"Memorize! {Mathf.CeilToInt(remaining)}";

            remaining -= Time.deltaTime;
            yield return null; // wait till next frame
        }

        // show "Go!" before clearing the text
        if (revealCountdownText)
            revealCountdownText.text = "Go!";

        yield return new UnityEngine.WaitForSeconds(0.5f);

        if (revealCountdownText)
            revealCountdownText.text = "";
    }

    // shows win panel and updates its message
    public void ShowWinScreen(int completedRound)
    {
        winPanel?.SetActive(true);

        if (winRoundText)
            winRoundText.text = $"Round {completedRound} Complete!";
    }

    // shows lose panel and updates its message
    public void ShowLoseScreen(int failedRound)
    {
        losePanel?.SetActive(true);

        if (loseRoundText)
            loseRoundText.text = $"Round Failed! Press Button to Start Over";
    }

    public void ShowGameCompleteScreen()
{
    HideAll();
    winPanel?.SetActive(true);
    if (winRoundText) winRoundText.text = "You Won!\nAll rounds complete!";
}

}