using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class FruitBoard : MonoBehaviour
{
    //Độ to của bảng hiển thị
    public int width = 6;
    public int height = 8;

    //Khoảng cách giữa các fruit trong bảng
    public float spacingX;
    public float spacingY;

    //Các fruit prefabs
    public GameObject[] fruitPrefabs;

    //Các node của bảng và game object
    public Node[,] fruitBoard;
    public GameObject fruitBoardGO;

    public List<GameObject> fruitToDestroy = new();
    public GameObject fruitParent;

    [SerializeField]
    private Fruit selectedFruit;

    [SerializeField]
    private bool isMove;

    [SerializeField]
    List<Fruit> fruitToRemove = new();

    //Layout array
    public Array_layout array_Layout;

    //Public static của bảng
    public static FruitBoard instance;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        InitializeBoard();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<Fruit>())
            {
                if (isMove)
                {
                    return;
                }
                Fruit fruit = hit.collider.gameObject.GetComponent<Fruit>();
                Debug.Log("Đã chọn: " +  fruit.gameObject);
                
                SelectFruit(fruit);
            } 
        } 
            
    }
    void InitializeBoard()
    {
        DestroyFruit();
        fruitBoard = new Node[width, height];

        spacingX = (float)(width-1)/2;
        spacingY = (float)(height-1)/2;

        for (int y = 0; y < height; y++) 
        {
            for (int x = 0; x < width; x++) 
            {
                Vector2 position = new Vector2(x - spacingX, y - spacingY);
                if (array_Layout.rows[y].row[x])
                {
                    fruitBoard[x, y] = new Node(false, null);
                }
                else
                {
                    int randomIndex = Random.Range(0, fruitPrefabs.Length);

                    GameObject fruit = Instantiate(fruitPrefabs[randomIndex], position, Quaternion.identity);
                    fruit.transform.SetParent(fruitParent.transform);
                    fruit.GetComponent<Fruit>().SetIndicies(x, y);
                    fruitBoard[x, y] = new Node(true, fruit);
                    fruitToDestroy.Add(fruit);
                }
            }
        }
        if (CheckBoard())
        {
            Debug.Log("Tạo lại bảng hiển thị");
            InitializeBoard();
        }  
        else
        {
            Debug.Log("Bắt đầu trò chơi");
        }    
    }

    private void DestroyFruit()
    {
        if (fruitToDestroy != null)
        {
            foreach (GameObject fruit in fruitToDestroy)
            {
                Destroy(fruit);
            }    
            fruitToDestroy.Clear();
        }    
    }

    public bool CheckBoard()
    {
        if (GameManager.Instance.isGameEnded)
            return false;
        Debug.Log("CheckingBoard");
        bool hasMatched = false;

        fruitToRemove.Clear();

        foreach (Node nodeFruit in fruitBoard)
        {
            if (nodeFruit.fruit != null)
            {
                nodeFruit.fruit.GetComponent<Fruit>().isMatched = false;
            }    
        }    

        for (int x = 0; x < width; x++) 
        {
            for (int y = 0; y < height; y++) 
            {
                //Ktra xem node được dùng hay chưa
                if (fruitBoard[x, y].isUsable)
                {
                    Fruit fruit = fruitBoard[x, y].fruit.GetComponent<Fruit>();
                    //Ktra xem đã matched chưa
                    if (!fruit.isMatched)
                    {
                        //Matching logic
                        MatchResult matchFruit = IsConnected(fruit);
                        if (matchFruit.connectedFruit.Count >= 3)
                        {
                            MatchResult superMatchedFruit = SuperMatch(matchFruit);

                            fruitToRemove.AddRange(superMatchedFruit.connectedFruit);

                            foreach (Fruit f in superMatchedFruit.connectedFruit)
                                f.isMatched = true;

                            hasMatched = true;
                        }    
                    }
                }
            }
        }
        return hasMatched;
    }
    public IEnumerator ProcessTurnOnMatchedBoard(bool subtractMoves)
    {
        foreach (Fruit fToRemove in fruitToRemove)
        {
            fToRemove.isMatched = false;
        }

        RemoveAndRefill(fruitToRemove);
        GameManager.Instance.ProcessTurn(fruitToRemove.Count, subtractMoves);
        yield return new WaitForSeconds(0.4f);

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }    
    }
    #region Cascading Fruit
    private void RemoveAndRefill(List<Fruit> fruitToRemove)
    {
        //Xóa và thay thế danh sách fruit (List<>)
        foreach (Fruit fruit in fruitToRemove)
        {
            int tmp_xIndex = fruit.xIndex;
            int tmp_yIndex = fruit.yIndex;

            Debug.Log("Đã xóa: " + fruit.gameObject);
            Destroy(fruit.gameObject);

            fruitBoard[tmp_xIndex, tmp_yIndex] = new Node(true, null);
        }    
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (fruitBoard[x, y].fruit == null)
                {
                    Debug.Log("Vị trí X: " + x + "Y: " + y + "đang trống, thay thế...");
                    RefillFruit(x, y);
                }    
            }   
        }    
    }

    //Thêm fruit mới
    private void RefillFruit(int x, int y)
    {
        //y Offset++ 
        int yOffset = 1;
        while (y + yOffset < height && fruitBoard[x, y + yOffset].fruit == null)
        {
            Debug.Log("yOffset++");
            yOffset++;
        } 
        if (y + yOffset < height && fruitBoard[x, y + yOffset].fruit != null)
        {
            Fruit fruitAbove = fruitBoard[x,y + yOffset].fruit.GetComponent<Fruit>();

            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, fruitAbove.transform.position.z);
            fruitAbove.MoveToTarget(targetPos);
            fruitAbove.SetIndicies(x, y);
            fruitBoard[x,y] = fruitBoard[x, y + yOffset];
            fruitBoard[x, y + yOffset] = new Node(true, null);
        } 
        if (y + yOffset == height)
        {
            SpawnPotionAtTop(x);
        }    
    }

    //Spawn fruit mới ở đỉnh của bảng hiện thị
    private void SpawnPotionAtTop(int x)
    {
        int index = FindIndexOfLowestNull(x);
        int locationToMoveTo = 8 - index;
        //Tạo random fruit mới
        int randomIndex = Random.Range(0, fruitPrefabs.Length);
        GameObject newFruit = Instantiate(fruitPrefabs[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
        newFruit.transform.SetParent(fruitParent.transform);
        newFruit.GetComponent<Fruit>().SetIndicies(x, index);
        fruitBoard[x, index] = new Node(true, newFruit);
        Vector3 targetPosition = new Vector3(newFruit.transform.position.x, newFruit.transform.position.y - locationToMoveTo, newFruit.transform.position.z);
        newFruit.GetComponent<Fruit>().MoveToTarget(targetPosition);
    }
    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;
        for (int y = 7; y >=0; y--)
        {
            if (fruitBoard[x,y].fruit == null)
            {
                lowestNull = y; 
            }    
        }    
        return lowestNull;
    }    
    #endregion

    private MatchResult SuperMatch(MatchResult matchF)
    {
        //TH1: Có Horizontal và LongHorizontal match
        if (matchF.direction == MatchDirection.Horizontal ||  matchF.direction == MatchDirection.LongHorizontal)
        {
            foreach (Fruit f in matchF.connectedFruit)
            {
                List<Fruit> extraConnectedFruit = new();

                CheckDirection(f, new Vector2Int(0,1), extraConnectedFruit);
                CheckDirection(f, new Vector2Int(0,-1), extraConnectedFruit);

                if (extraConnectedFruit.Count >= 2)
                {
                    Debug.Log("Có một SuperHorizontal match");
                    extraConnectedFruit.AddRange(matchF.connectedFruit);
                    return new MatchResult
                    {
                        connectedFruit = extraConnectedFruit,
                        direction = MatchDirection.Super
                    };
                }    
            }
            return new MatchResult
            {
                connectedFruit = matchF.connectedFruit,
                direction = matchF.direction
            };
        }
        //TH2: Có Vertical và LongVertical match
        else if (matchF.direction == MatchDirection.Vertical || matchF.direction == MatchDirection.LongVertical)
        {
            foreach (Fruit f in matchF.connectedFruit)
            {
                List<Fruit> extraConnectedFruit = new();

                CheckDirection(f, new Vector2Int(1, 0), extraConnectedFruit);
                CheckDirection(f, new Vector2Int(-1, 0), extraConnectedFruit);

                if (extraConnectedFruit.Count >= 2)
                {
                    Debug.Log("Có một SuperVertical match");
                    extraConnectedFruit.AddRange(matchF.connectedFruit);
                    return new MatchResult
                    {
                        connectedFruit = extraConnectedFruit,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedFruit = matchF.connectedFruit,
                direction = matchF.direction
            };
        }
        return null;
    }

    MatchResult IsConnected(Fruit fruit) 
    {
        List<Fruit> connectedFruit = new();
        FruitType fruitTypes = fruit.fruitType;
        connectedFruit.Add(fruit);
        //Check bên phải
        CheckDirection(fruit, new Vector2Int(1,0), connectedFruit);
        //Check bên trái
        CheckDirection(fruit, new Vector2Int(-1, 0), connectedFruit);
        //3 matched (Horizontal match)
        if(connectedFruit.Count == 3)
        {
            Debug.Log("Có một Horizontal match, loại: " + connectedFruit[0].fruitType);
            return new MatchResult
            {
                connectedFruit = connectedFruit,
                direction = MatchDirection.Horizontal
            };
        }    
        //>3 matched (LongHorizontal match)
        else if (connectedFruit.Count > 3)
        {
            Debug.Log("Có một LongHorizontal match, loại: " + connectedFruit[0].fruitType);
            return new MatchResult
            {
                connectedFruit = connectedFruit,
                direction = MatchDirection.LongHorizontal
            };
        }    
        //Xóa connectedFruit
        connectedFruit.Clear();
        //Readd
        connectedFruit.Add(fruit);
        //Check bên trên
        CheckDirection(fruit, new Vector2Int(0, 1), connectedFruit);
        //Check bên dưới
        CheckDirection(fruit, new Vector2Int(0, -1), connectedFruit);
        //3 matched (Vertical match)
        if (connectedFruit.Count == 3)
        {
            Debug.Log("Có một Vertical match, loại: " + connectedFruit[0].fruitType);
            return new MatchResult
            {
                connectedFruit = connectedFruit,
                direction = MatchDirection.Vertical
            };
        }
        //>3 matched (LongVertical match)
        else if (connectedFruit.Count > 3)
        {
            Debug.Log("Có một LongVertical match, loại: " + connectedFruit[0].fruitType);
            return new MatchResult
            {
                connectedFruit = connectedFruit,
                direction = MatchDirection.LongVertical
            };
        }
        else
        {
            return new MatchResult
            {
                connectedFruit = connectedFruit,
                direction = MatchDirection.None
            };
        } 
            
    }
    void CheckDirection(Fruit f, Vector2Int direction, List<Fruit> connectedFruit)
    {
        FruitType fruitType = f.fruitType;
        int x = f.xIndex + direction.x;
        int y = f.yIndex + direction.y;
        //Chạy trong giới hạn của bảng
        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (fruitBoard[x, y].isUsable)
            {
                Fruit nearFruit = fruitBoard[x, y].fruit.GetComponent<Fruit>();
                if (!nearFruit.isMatched && nearFruit.fruitType == fruitType)
                {
                    connectedFruit.Add(nearFruit);
                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }
            }
            else 
            {
                break; 
            }
        }
    }
    #region Swapping Fruit
    //Chọn fruit
    public void SelectFruit(Fruit fruit)
    {
        if (selectedFruit == null)
        {
            Debug.Log(fruit);
            selectedFruit = fruit;
        }
        else if (selectedFruit == fruit)
        {
            selectedFruit = null;
        }    
        else if (selectedFruit != null)
        {
            SwapFruit(selectedFruit, fruit);
            selectedFruit = null;
        }    
    }    
    //Đổi vị trí (logic)
    private void SwapFruit(Fruit currentFruit,  Fruit targetFruit)
    {
        if (!IsAdjacent(currentFruit, targetFruit))
        {
            return;
        }
        DoSwap(currentFruit, targetFruit);
        isMove = true;
        StartCoroutine(ProcessMatches(currentFruit, targetFruit));
    }    
    //Đổi vị trí (thực hiện)
    private void DoSwap(Fruit currentF,  Fruit targetF)
    {
        GameObject temp = fruitBoard[currentF.xIndex, currentF.yIndex].fruit;

        fruitBoard[currentF.xIndex, currentF.yIndex].fruit = fruitBoard[targetF.xIndex, targetF.yIndex].fruit;
        fruitBoard[targetF.xIndex, targetF.yIndex].fruit = temp;

        int tempXIndex = currentF.xIndex;
        int tempYIndex = currentF.yIndex;
        currentF.xIndex = targetF.xIndex;
        currentF.yIndex = targetF.yIndex;
        targetF.xIndex = tempXIndex;
        targetF.yIndex = tempYIndex;

        currentF.MoveToTarget(fruitBoard[targetF.xIndex, targetF.yIndex].fruit.transform.position);
        targetF.MoveToTarget(fruitBoard[currentF.xIndex, currentF.yIndex].fruit.transform.position);
    }    
    private IEnumerator ProcessMatches(Fruit currentF, Fruit targetF)
    {
        yield return new WaitForSeconds(0.2f);

        if (CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
        else
        {
            DoSwap(currentF, targetF);
        }
        isMove = false;
    } 
        
    //Ktra
    private bool IsAdjacent(Fruit currentF,  Fruit targetF)
    {
        return Mathf.Abs(currentF.xIndex - targetF.xIndex) + Mathf.Abs(currentF.yIndex - targetF.yIndex) == 1;
    }    
    #endregion
}
public class MatchResult
{
    public List<Fruit> connectedFruit;
    public MatchDirection direction;

}
public enum MatchDirection
{
    Vertical, 
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}