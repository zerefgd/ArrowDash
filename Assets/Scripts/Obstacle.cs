using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Obstacle : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer _leftSprite, _rightSprite;

    [SerializeField]
    private BoxCollider2D _leftBox, _rightBox;

    [SerializeField]
    private float _destroyTime,_speed;

    [SerializeField]
    private Vector3 _startSpawnPos, _endPos;

    [SerializeField]
    private float _maxBarSizeX, _minBarSizeX, _barResizeTime;

    private float currentSizeX;
    private float horizontalSpeed;

    private Tween moveTween;
    private bool canUpdate;

    private void Awake()
    {
        transform.position = _startSpawnPos;
        float timeToMove = -(_endPos - _startSpawnPos).y / _speed;
        moveTween = transform.DOMove(_endPos, timeToMove).SetEase(Ease.Linear);
        moveTween.onComplete = DestroySprite;
        moveTween.Play();

        currentSizeX = Random.Range(_minBarSizeX, _maxBarSizeX);

        canUpdate = Random.Range(0, 5) == 0;

        ResizeBoxes();

        horizontalSpeed = (_maxBarSizeX - _minBarSizeX) / Random.Range(timeToMove / 2f, timeToMove);
    }

    private void ResizeBoxes()
    {
        Vector2 tempSize = _leftSprite.size;
        tempSize.y = currentSizeX;
        _leftSprite.size = tempSize;
        _leftBox.size = tempSize;
        tempSize.y = _maxBarSizeX - currentSizeX;
        _rightSprite.size = tempSize;
        _rightBox.size = tempSize;

        tempSize = Vector2.zero;
        tempSize.y = currentSizeX / 2f;
        _leftBox.offset = tempSize;
        tempSize.y = (_maxBarSizeX - currentSizeX) / 2f;
        _rightBox.offset = tempSize;
    }
        


    private void OnEnable()
    {
        GameManager.Instance.GameEnded += DestroySprite;
    }

    private void OnDisable()
    {
        GameManager.Instance.GameEnded -= DestroySprite;
    }

    private void FixedUpdate()
    {
        if (!canUpdate) return;

        currentSizeX += horizontalSpeed * Time.fixedDeltaTime;
        ResizeBoxes();
        if(currentSizeX > _maxBarSizeX || currentSizeX < _minBarSizeX)
        {
            horizontalSpeed *= -1f;
        }
    }

    public void DestroySprite()
    {
        if (moveTween.IsActive()) moveTween.Kill();

        canUpdate = false;
        _leftBox.enabled = false;
        _rightBox.enabled = false;

        var destroyTween = transform.DOScale(Vector3.zero, _destroyTime).SetEase(Ease.InSine);
        destroyTween.onComplete = () =>
        {
            Destroy(gameObject);
        };

        destroyTween.Play();        
    }

}
