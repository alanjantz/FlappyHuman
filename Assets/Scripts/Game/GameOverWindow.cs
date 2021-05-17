using UnityEngine;
using UnityEngine.UI;

public class GameOverWindow : MonoBehaviour
{
    private static GameOverWindow _instance;

    private const string SCORE_VALUE = "ScoreValue";
    private const string HIGHSCORE_VALUE = "HighscoreValue";
    private const string NEW_HIGHSCORE = "NewHighscore";
    private const string BLOCK = "Block";
    private const string SCORE = "Score";
    private const string HIGHSCORE = "Highscore";
    private Text _scoreText;
    private Text _highscoreText;
    private Image _newHighscore;

    private void Awake()
    {
        _instance = this;
        _scoreText = GetScoreText();
        _highscoreText = GetHighscoreText();
        _newHighscore = GetNewHighscoreImage();
        _newHighscore.gameObject.SetActive(false);

        Hide();
    }

    private Text GetScoreText()
        => transform.Find(BLOCK).transform.Find(SCORE).transform.Find(SCORE_VALUE).GetComponent<Text>();

    private Text GetHighscoreText()
        => transform.Find(BLOCK).transform.Find(HIGHSCORE).transform.Find(HIGHSCORE_VALUE).GetComponent<Text>();

    private Image GetNewHighscoreImage()
        => transform.Find(BLOCK).transform.Find(HIGHSCORE).transform.Find(NEW_HIGHSCORE).GetComponent<Image>();

    private void Update()
    {
        if (Level.IsInitializingOrDead)
        {
            _scoreText.text = Level.GetInstance()?.GetPoints().ToString();
            _highscoreText.text = Score.GetHighScore().ToString();
        }
    }

    public static GameOverWindow GetInstance() => _instance;

    public void Hide()
    {
        ScoreWindow.GetInstance()?.Show();
        gameObject.SetActive(false);
    }

    public void Show(bool showNewHighscore = false)
    {
        _newHighscore.gameObject.SetActive(showNewHighscore);
        ScoreWindow.GetInstance()?.Hide();
        gameObject.SetActive(true);
    }
}
