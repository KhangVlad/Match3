using UnityEngine;
using System.Collections;

public class ClearAxisLine : MonoBehaviour
{
    private LineRenderer _lr;

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
    }


    public void ActiveAxisLine(Vector2 p, Vector2 targetA, Vector2 targetB, float duration)
    {
        StartCoroutine(ActiveAxisLineCoroutine(p, targetA, targetB, duration));
    }
    private IEnumerator ActiveAxisLineCoroutine(Vector2 p, Vector2 targetA, Vector2 targetB, float duration)
    {
        _lr.positionCount = 3;
        _lr.SetPosition(0, targetA);
        _lr.SetPosition(1, p);
        _lr.SetPosition(2, targetB);

        // wait a little bit when line reach max length
        yield return new WaitForSeconds(duration);
        _lr.positionCount = 0;
        Destroy(this.gameObject);
    }
}
