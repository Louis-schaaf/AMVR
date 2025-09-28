using UnityEngine;
using TMPro;   // only if you’re using TextMeshPro

public class Scoreboard : MonoBehaviour
{
    public int currentScore = 0;
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Start()
    {
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";
    }
}
