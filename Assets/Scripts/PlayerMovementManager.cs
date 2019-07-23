using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerMovementManager : MonoBehaviour
{
    private GameObject player;
    private GameManagerScr gManager;
    private float stepTime = 0.3f;
    private float timer;
    
    private enum direction
    {
        up,left,down,right
    }
    private direction currentDirection;

    private void Start()
    {
        player = GameObject.Find("Player");
        gManager = GameObject.Find("GameManager").GetComponent<GameManagerScr>();
    }

    private void Update()
    {
        if (gManager.isFirstInput)
        {
            SetMoveDirection();
            MovePlayer();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            gManager.isFirstInput = true;
            gManager.onFirstInput.Invoke();
        }
    }

    private void SetMoveDirection()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (!isOppositeDirection(direction.down))
                currentDirection = direction.up;
        }

        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (!isOppositeDirection(direction.right))
                currentDirection = direction.left;
        }

        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (!isOppositeDirection(direction.up))
                currentDirection = direction.down;
        }

        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (!isOppositeDirection(direction.left))
                currentDirection = direction.right;
        }   
    }

    private bool isOppositeDirection(direction checkDirection)
    {
        if (currentDirection == checkDirection)
            return true;

        return false;
    }

    private void MakeStep()
    {
        int x = 0;
        int y = 0;
        bool isFoodAte = false;
        
        if (currentDirection == direction.up)
            y = 1;
        else if (currentDirection == direction.left)
            x = -1;
        else if (currentDirection == direction.down)
            y = -1;
        else if (currentDirection == direction.right)
            x = 1;

        Node targetNode = gManager.GetNode(gManager.PlayerNode.x + x, gManager.PlayerNode.y + y);
        
        if (targetNode == null)
        {
            gManager.onGameOver.Invoke();
        }
        else
        {
            if (!isTailTarget(targetNode))
            {
                if (targetNode == gManager.FoodNode)
                    isFoodAte = true;

                Node previousNode = gManager.PlayerNode;
                gManager.availableNodes.Add(previousNode);
            
                if (isFoodAte)
                {
                    CreateTailPart(previousNode);
                
                    if (gManager.availableNodes.Count > 0)
                    {
                        gManager.RandomFood();
                    }
                    else
                    {
                        // You're win
                    }
                }
            
                MakeTailStep(previousNode);
            
                player.transform.position = targetNode.worldPosition;
                gManager.PlayerNode = targetNode;
                gManager.availableNodes.Remove(gManager.PlayerNode);
            
                player.transform.position += new Vector3(0.5f, 0.5f, 0.5f);
            }
            else
            {
                gManager.onGameOver.Invoke();
            }
        }
    }

    private void MakeTailStep(Node previousNode)
    {
        previousNode = null;

        for (int i = 0; i < gManager.tailNodes.Count; i++)
        {
            TailNode tn = gManager.tailNodes[i];
            gManager.availableNodes.Add(tn.tailNode);

            if (i == 0)
            {
                previousNode = tn.tailNode;
                tn.tailNode = gManager.PlayerNode;
            }
            else
            {
                Node temp = tn.tailNode;
                tn.tailNode = previousNode;
                previousNode = temp;
            }

            gManager.availableNodes.Remove(tn.tailNode);
            
            tn.tailObject.transform.position = tn.tailNode.worldPosition;
            tn.tailObject.transform.position += new Vector3(0.5f, 0.5f, 0.5f);
            tn.tailObject.transform.localScale = Vector3.one * 0.7f;
        }
    }

    private void CreateTailPart(Node previousNode)
    {
        gManager.tailNodes.Add(gManager.CreateTail(previousNode.x, previousNode.y));
        gManager.availableNodes.Remove(previousNode);
    }

    private bool isTailTarget(Node targetNode)
    {
        for (int i = 0; i < gManager.tailNodes.Count; i++)
        {
            if (targetNode == gManager.tailNodes[i].tailNode)
            {
                gManager.onGameOver.Invoke();
                return true;
            }
        }
        
        return false;
    }

    private void MovePlayer()
    {
        timer += Time.deltaTime;

        if (timer > stepTime)
        {
            MakeStep();

            stepTime -= 0.0002f;
            timer = 0;
        }
    }
}
