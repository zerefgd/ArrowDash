using UnityEngine;
using DG.Tweening;

public class Score : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer m_spriteRenderer;

    [SerializeField]
    private float _destroyTime, _speed;

    [SerializeField]
    private Vector3 _startSpawnPos, _endPos;

    private Tween moveTween;

    private void Awake()
    {
        transform.position = _startSpawnPos;
        float timeToMove = -(_endPos - _startSpawnPos).y / _speed;
        moveTween = transform.DOMove(_endPos, timeToMove).SetEase(Ease.Linear);
        moveTween.onComplete = DestroySprite;
        moveTween.Play();
    }

    private void OnEnable()
    {
        GameManager.Instance.GameEnded += DestroySprite;
    }

    private void OnDisable()
    {
        GameManager.Instance.GameEnded -= DestroySprite;
    }

    public void DestroySprite()
    {
        GetComponent<Collider2D>().enabled = false;
        var scaleTween = transform.DOScale(Vector3.zero, _destroyTime).SetEase(Ease.InSine);
        var destroyTween = m_spriteRenderer.DOFade(0, _destroyTime).SetEase(Ease.InSine);
        destroyTween.onComplete = () =>
        {
            if (moveTween.IsActive()) moveTween.Kill();
            if (scaleTween.IsActive()) scaleTween.Kill();
            Destroy(gameObject);
        };

        destroyTween.Play();
        scaleTween.Play();
    }
}
