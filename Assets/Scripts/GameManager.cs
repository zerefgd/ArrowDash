using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private TMP_Text _scoreText, _endScoreText,_highScoreText;

    private int score;

    [SerializeField]
    private GameObject _endPanel;

    [SerializeField]
    private Image _soundImage;

    [SerializeField]
    private Sprite _activeSoundSprite, _inactiveSoundSprite;

    [SerializeField]
    private Transform _player;

    [SerializeField]
    private SpriteRenderer _bgSprite;

    [SerializeField]
    private Vector3 _playerStartPos, _bgStartScale, _bgEndScale;

    [SerializeField]
    private float _startAnimationTime,_spawnInterval;

    [SerializeField]
    private GameObject _scorePrefab, _obstaclePrefab;

    private bool hasGameFinished;

    public UnityAction GameStarted, GameEnded;


    private void Awake()
    {
        Instance = this;

        DOTween.Init();
        DOTween.defaultAutoPlay = AutoPlay.None;
        DOTween.logBehaviour = LogBehaviour.Default;
    }

    private void Start()
    {
        //AudioManager.Instance.AddButtonSound();
        score = 0;
        _scoreText.text = score.ToString();
        hasGameFinished = false;

        var startAnimation = DOTween.Sequence();

        var playerTween = _player.DOMoveY(_playerStartPos.y, _startAnimationTime).SetEase(Ease.InSine);

        var bgTween = DOTween.To(
            () => _bgSprite.size,
            x => _bgSprite.size = x,
            new Vector2(_bgStartScale.x,_bgSprite.size.y),
            _startAnimationTime
            ).SetEase(Ease.InSine);

        startAnimation
            .Append(playerTween)
            .Append(bgTween)
            .AppendCallback(
             () =>
             {
                 GameStarted?.Invoke();
             }
            );

        startAnimation.Play();
    }

    private void OnEnable()
    {
        GameStarted += StartSpawning;
    }

    private void OnDisable()
    {
        GameStarted -= StartSpawning;
    }

    private void StartSpawning()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        while(!hasGameFinished)
        {
            Instantiate(Random.Range(0,6) == 0 ? _scorePrefab : _obstaclePrefab);
            yield return new WaitForSeconds(_spawnInterval);
        }
    }




    public void GoToMainMenu()
    {
        SceneManager.LoadScene(Constants.DATA.MAIN_MENU_SCENE);
    }

    public void ReloadGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ToggleSound()
    {
        bool sound = (PlayerPrefs.HasKey(Constants.DATA.SETTINGS_SOUND) ? PlayerPrefs.GetInt(Constants.DATA.SETTINGS_SOUND)
            : 1) == 1;
        sound = !sound;
        PlayerPrefs.SetInt(Constants.DATA.SETTINGS_SOUND, sound ? 1 : 0);
        _soundImage.sprite = sound ? _activeSoundSprite : _inactiveSoundSprite;
        AudioManager.Instance.ToggleSound();
    }

    public void EndGame()
    {
        StartCoroutine(IEndGame());
    }

    private IEnumerator IEndGame()
    {
        hasGameFinished = true;
        GameEnded?.Invoke();

        _scoreText.gameObject.SetActive(false);
        _endScoreText.text = score.ToString();

        bool sound = (PlayerPrefs.HasKey(Constants.DATA.SETTINGS_SOUND) ?
          PlayerPrefs.GetInt(Constants.DATA.SETTINGS_SOUND) : 1) == 1;
        _soundImage.sprite = sound ? _activeSoundSprite : _inactiveSoundSprite;

        int highScore = PlayerPrefs.HasKey(Constants.DATA.HIGH_SCORE) ? PlayerPrefs.GetInt(Constants.DATA.HIGH_SCORE) : 0;
        if (score > highScore)
        {
            _highScoreText.text = "NEW BEST";
            highScore = score;
            PlayerPrefs.SetInt(Constants.DATA.HIGH_SCORE, highScore);
        }
        else
        {
            _highScoreText.text = "BEST " + highScore.ToString();
        }

        yield return new WaitForSeconds(0.5f);
        _endPanel.SetActive(true);

        var endAnimation = DOTween.Sequence();

        var endPanelTween = _endPanel.GetComponent<RectTransform>()
            .DOAnchorPos(_playerStartPos, _startAnimationTime)
            .SetEase(Ease.InSine);

        var bgTween = DOTween.To(
            () => _bgSprite.size,
            x => _bgSprite.size = x,
            new Vector2(_bgEndScale.x, _bgSprite.size.y),
            _startAnimationTime
            ).SetEase(Ease.InSine);

        endAnimation
            .Append(bgTween)
            .Append(endPanelTween);

        endAnimation.Play();
    }

    public void UpdateScore()
    {
        score++;
        _scoreText.text = score.ToString();

        var scoreTween =
            _scoreText.gameObject.GetComponent<RectTransform>()
            .DOPunchScale(Vector3.one, _startAnimationTime, 2, 0)
            .SetEase(Ease.InSine);
        scoreTween.Play();
    }
}
