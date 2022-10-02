using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMover : MonoBehaviour
{
    public float speed;
    public Vector2 offset;
    private float timer;
    private Vector3 startPos;
    private void Start()
    {
        timer = BaseUtils.RandomFloat(0, 1);
        startPos = transform.localPosition;
    }
    private void Update()
    {
        transform.localPosition = new Vector3(
            Mathf.Lerp(startPos.x, startPos.x + offset.x, timer.Evaluate(CurveType.SmoothParabolInverted)),
            Mathf.Lerp(startPos.y, startPos.y + offset.y, timer.Evaluate(CurveType.SmoothParabol)),
            startPos.z);
        timer += Time.deltaTime * speed;
        if (timer > 1)
        {
            timer = 0;
        }
    }
}
