using UnityEngine;
using UnityEngine.UI;

public class ScoreWindow : MonoBehaviour
{
    private static ScoreWindow _instance;

    private const string SCORE_VALUE = "ScoreValue";

    private Text _scoreText;

    private void Awake()
    {
        _instance = this;
        _scoreText = transform.Find(SCORE_VALUE).GetComponent<Text>();
    }

    private void Update()
    {
        _scoreText.text = (Level.GetInstance()?.GetPoints() ?? 0).ToString();
    }

    public static ScoreWindow GetInstance() => _instance;

    public void Hide() => gameObject.SetActive(false);

    public void Show() => gameObject.SetActive(true);
}
