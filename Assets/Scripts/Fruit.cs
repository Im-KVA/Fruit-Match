using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public FruitType fruitType;

    public int xIndex;
    public int yIndex;

    public bool isMatched;
    private Vector2 currentPos;
    private Vector2 targetPos;

    public bool isMoving;

    public Fruit(int x, int y)
    {
        xIndex = x;
        yIndex = y;    
    }
    public void SetIndicies(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }
    //Di chuyển
    public void MoveToTarget(Vector2 targetF)
    {
        StartCoroutine(MoveCroutine(targetF));
    } 
    private IEnumerator MoveCroutine(Vector2 targetPos)
    {
        isMoving = true;
        float duration = 0.2f;

        Vector2 startPosition = transform.position;
        float elaspedTime = 0f;

        while (elaspedTime < duration)
        {
            float t = elaspedTime/duration;
            transform.position = Vector2.Lerp(startPosition, targetPos, t);
            elaspedTime += Time.deltaTime;
            yield return null;
        }    
        transform.position = targetPos;
        isMoving = false;
    }    
        
}

public enum FruitType
{
    Meat,
    Carrot,
    Apple,
    Radish
}