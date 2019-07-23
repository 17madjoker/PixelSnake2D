using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManagerScr : MonoBehaviour
{
    private int maxWidth = 18;
    private int maxHeight = 14;
    
    private Color32 evenColor = new Color32(55, 71, 79, 255);
    private Color32 unevenColor = new Color32(69, 90, 100, 255);  
    private Color32 playerColor = new Color32(129, 199, 132, 255);
    private List<Color32> foodColorList = new List<Color32>()
    {
        new Color32(239, 83, 80, 255),
        new Color32(236, 64, 122, 255),
        new Color32(171, 71, 188, 255),
        new Color32(171, 71, 188, 255),
        new Color32(255, 112, 67, 255),
    };

    private Node[,] mapGrid;
    public List<Node> availableNodes = new List<Node>();
    public List<TailNode> tailNodes = new List<TailNode>();
    private Node playerNode;
    private Node foodNode;
    private float rotationAngle = 0f;
    
    [SerializeField]
    private Sprite playerSprite;
    [SerializeField]
    private GameObject food;
    [SerializeField]
    private GameObject cameraHolder;
    [SerializeField]
    private GameObject fullTail;

    public bool isGameOver = false;
    public bool isFirstInput = false;

    public UnityEvent onStartGame;
    public UnityEvent onGameOver;
    public UnityEvent onFirstInput;

    public Node PlayerNode
    {
        get { return playerNode; }
        set { playerNode = value; }
    }  
    
    public Node FoodNode
    {
        get { return foodNode; }
        set { foodNode = value; }
    }  
    
    private void Start()
    {
        onStartGame.Invoke();
        CreateMap();
        PlaceCamera();
        PlacePlayer();
        CreateFood();
    }

    private void Update()
    {
        RotateFood();
        CheckExit();
            
        if (isGameOver)
            if (Input.GetKeyDown(KeyCode.Space))
                SceneManager.LoadScene("Game");
            return;
    }

    private void CreateMap()
    {
        Texture2D texture = new Texture2D(maxWidth, maxHeight);
        
        mapGrid = new Node[maxWidth, maxHeight];
        
        for (int x = 0; x < maxWidth; x++)
        {
            for (int y = 0; y < maxHeight; y++)
            {
                Node node = new Node()
                {
                    x = x,
                    y = y,
                    worldPosition = new Vector3(x, y, 0)
                };

                mapGrid[x, y] = node;
                availableNodes.Add(node);
                
                if (x % 2 == 0)
                {
                    if (y % 2 == 0)
                    {
                        texture.SetPixel(x, y, evenColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, unevenColor);
                    }
                }
                else
                {
                    if (y % 2 == 0)
                    {
                        texture.SetPixel(x, y, unevenColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, evenColor);
                    }
                }
            }
            
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            
            Rect rectangle = new Rect(0, 0, maxWidth, maxHeight);

            Sprite sprite = Sprite.Create(texture, rectangle, Vector2.zero, 1, 0,  SpriteMeshType.FullRect);
            GetComponent<SpriteRenderer>().sprite = sprite;
        }
    }
    
    private Sprite CreateSprite(Color32 spriteColor)
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, spriteColor);
        t.filterMode = FilterMode.Point;
        t.Apply();

        Rect figure = new Rect(0, 0, 1, 1);
        
        return Sprite.Create(t, figure, Vector2.one * 0.5f, 1, 0,  SpriteMeshType.FullRect);
    }

    public Node GetNode(int x, int y)
    {
        if (x < 0 || y < 0 || x > maxWidth - 1 || y > maxHeight - 1)
            return null;

        return mapGrid[x, y];
    }

    private void PlacePlayer()
    {
        GameObject player = GameObject.Find("Player");

        playerSprite = CreateSprite(playerColor);
        player.GetComponent<SpriteRenderer>().sprite = playerSprite;
        player.GetComponent<SpriteRenderer>().sortingOrder = 1;

        PlayerNode = GetNode(Random.Range(4, maxWidth - 4), Random.Range(4, maxHeight - 4));
        player.transform.localScale = Vector3.one * 1.1f; 
        player.transform.position = PlayerNode.worldPosition;
        player.transform.position += Vector3.one * 0.5f; 
    }

    private void PlaceCamera()
    {             
        Node cameraNode = GetNode(maxWidth / 2, maxHeight / 2);

        cameraHolder.transform.position = cameraNode.worldPosition;
    }

    private void CreateFood()
    {
        Color32 foodColor = foodColorList[Random.Range(0, foodColorList.Count)];

        food.GetComponent<SpriteRenderer>().sprite = CreateSprite(foodColor);
        food.GetComponent<SpriteRenderer>().sortingOrder = 1;
        
        food.GetComponent<Transform>().localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        RandomFood();
    }

    public void RandomFood()
    {
        Node randFood = availableNodes[Random.Range(0, availableNodes.Count)];

        food.transform.position = new Vector3(randFood.x + 0.5f, randFood.y + 0.5f, 0);
        foodNode = randFood;
    }

    private void RotateFood()
    {       
        food.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
        rotationAngle += 2f;
    }

    public TailNode CreateTail(int x, int y)
    {
        TailNode t = new TailNode();
        t.tailNode = GetNode(x, y);
        t.tailObject = new GameObject();
        t.tailObject.transform.parent = fullTail.transform;
        t.tailObject.transform.localScale = Vector3.one * 0.7f;
        t.tailObject.transform.position = t.tailNode.worldPosition;

        SpriteRenderer sr = t.tailObject.AddComponent<SpriteRenderer>();
        sr.sprite = playerSprite;
        sr.sortingOrder = 1;

        return t;
    }

    public void GameOver()
    {
        isGameOver = true;
        isFirstInput = false;
    }

    private void CheckExit()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
