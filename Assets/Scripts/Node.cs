using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    //Ktra xem vị trí có chèn vào được không
    public bool isUsable;

    public GameObject fruit;

    public Node(bool IsUsable, GameObject Fruit)
    {
        isUsable = IsUsable;
        fruit = Fruit;
    }
}
