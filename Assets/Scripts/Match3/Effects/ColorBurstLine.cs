using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ColorBurstLine : MonoBehaviour
{
    private LineRenderer _lr;

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
    }

    public void SetLine(Vector2 p1, Vector2 p2, float duration)
    {
        StartCoroutine(SetLineCoroutine(p1, p2, duration));
    }
    private IEnumerator SetLineCoroutine(Vector2 p1, Vector2 p2, float duration)
    {
        _lr.positionCount = 2;
        _lr.SetPosition(0, p1);
        _lr.SetPosition(1, p2);

        // wait a little bit when line reach max length
        yield return new WaitForSeconds(duration);
        _lr.positionCount = 0;
        Destroy(this.gameObject);
    }
}
