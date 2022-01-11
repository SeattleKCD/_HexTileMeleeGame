using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;


public class MapManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap map;

    [SerializeField]
    Fighter pfFighterF;
    [SerializeField]
    int startXfF, startYfF;
    [SerializeField]
    GameObject fighterFUI;

    [SerializeField]
    Fighter pfFighterP;
    [SerializeField]
    int startXfP, startYfP;
    [SerializeField]
    GameObject fighterPUI;

    public static event Action<bool> FighterFEngaged = delegate { };
    public static event Action<bool> FighterPEngaged = delegate { };

    Fighter fighterF, fighterP, curFighter, enemyFighter;
    private Vector3Int curPos;
    private int highlightRange;
    bool performedMove, performedFace, fFMoving, fPMoving, curEngaged = false;

    private readonly static List<Vector3Int> evenNeighborDirs
        = new()
        {
        new Vector3Int(1,0,0),
        new Vector3Int(0,1,0),
        new Vector3Int(-1,1,0),
        new Vector3Int(-1,0,0),
        new Vector3Int(-1,-1,0),
        new Vector3Int(0,-1,0)
    };
    
    private readonly static List<Vector3Int> oddNeighborDirs
     = new()
     {
        new Vector3Int(1,0,0),
        new Vector3Int(1,1,0),
        new Vector3Int(0,1,0),
        new Vector3Int(-1,0,0),
        new Vector3Int(0,-1,0),
        new Vector3Int(1,-1,0)
    };

    private List<Vector3Int> visitedNodes = new();
    private List<Vector3Int> engagedNodes = new();
    private List<Vector3Int> adjacentNodes = new();

    private List<Vector3Int> DetermineNeighborDirections(bool isEven)
    {
        if (isEven)
            return evenNeighborDirs;
        else
            return oddNeighborDirs;
    }

    private struct NeighborNode
    {
        public Vector3Int nodeLocation;
        public int nodeDistance;
    }

    private void Start()
    {
        highlightRange = 1;
        curPos = new Vector3Int(startXfF, startYfF, 0);
        fighterF = Instantiate(pfFighterF);
        fighterF.transform.position = map.CellToWorld(curPos);
        fighterF.transform.rotation = Quaternion.Euler(0f, 0f, 30f);
        curPos.x = startXfP;
        curPos.y = startYfP;
        fighterP = Instantiate(pfFighterP);
        fighterP.transform.position = map.CellToWorld(curPos);
        fighterP.transform.rotation = Quaternion.Euler(0f, 0f, 30f);
    }

    // Update is called once per frame
    private void Update()
    {
        if(!(fFMoving || fPMoving))
        {
            curFighter = fighterF;
            enemyFighter = fighterP;
            curPos = map.WorldToCell(curFighter.transform.position);
            SetHighlight(curPos, Color.green);
            fFMoving = true;
            performedMove = false;
            performedFace = false;
            if (engagedNodes.Contains(curPos))
            {
                FighterFEngaged(true);
                curEngaged = true;
            }
            else
            {
                FighterFEngaged(false);
                curEngaged = false;
            }
            fighterFUI.SetActive(true);
        }
        else if (fFMoving && performedMove && performedFace)
        {
            fFMoving = false;
            fighterFUI.SetActive(false);
            curFighter = fighterP;
            enemyFighter = fighterF;
            curPos = map.WorldToCell(curFighter.transform.position);
            SetHighlight(curPos, Color.green);
            fPMoving = true;
            performedMove = false;
            performedFace = false;
            fighterPUI.SetActive(true);
            if (engagedNodes.Contains(curPos))
            {
                FighterPEngaged?.Invoke(true);
                curEngaged = true;
            }
            else
            {
                FighterPEngaged?.Invoke(false);
                curEngaged = false;
            }
        }
        else if (fPMoving && performedMove && performedFace)
        {
            fPMoving = false;
            fighterPUI.SetActive(false);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = map.WorldToCell(mousePosition);

            TileBase clickedTile = map.GetTile(gridPosition);

            if (map.HasTile(gridPosition) && visitedNodes.Contains(gridPosition))
            {
                if (performedMove && !performedFace && gridPosition != curPos)
                {
                    curFighter.transform.rotation = Quaternion.Euler(0f, 0f, CalculateRotation(gridPosition, curFighter.rotationBase));
                    ClearHighlight();
                    performedFace = true;
                }
                else
                {
                    curFighter.transform.position = map.CellToWorld(gridPosition);
                    curPos = gridPosition;
                    performedMove = true;

                    curFighter.isEngaged = false;
                    if (engagedNodes.Contains(curPos))
                    {
                        curFighter.isEngaged = true;
                        curFighter.enemyPos = map.WorldToCell(enemyFighter.transform.position);
                    }

                    highlightRange = 1;
                    ShowRangeHighlight(Color.yellow);
                }
            }

            Debug.Log("Clicked " + clickedTile + " at position " + gridPosition);
        }        
    }

    private float CalculateRotation(Vector3Int facingLocation, int rotationBase)
    {
        int rotationIndex = 0;

        engagedNodes.Clear();
        adjacentNodes.Clear();

        foreach (Vector3Int direction in DetermineNeighborDirections(curPos.y % 2 == 0))
        {
            adjacentNodes.Add(curPos + direction);
        }

        rotationIndex = adjacentNodes.IndexOf(facingLocation);
        Debug.Log("Rotation Index is " + rotationIndex);

        engagedNodes.Add(adjacentNodes[rotationIndex]);
        if (rotationIndex == 0)
        {
            engagedNodes.Add(adjacentNodes[1]);
            engagedNodes.Add(adjacentNodes[5]);
        }
        else if (rotationIndex == 5)
        {
            engagedNodes.Add(adjacentNodes[0]);
            engagedNodes.Add(adjacentNodes[4]);
        }
        else
        {
            engagedNodes.Add(adjacentNodes[rotationIndex-1]);
            engagedNodes.Add(adjacentNodes[rotationIndex+1]);
        }

        return (float)(rotationBase - 60 * rotationIndex);
    }

    private void ShowRangeHighlight(Color highlightColor)
    {
        Vector3Int newPosition;
        Queue<NeighborNode> nodesToVisit = new();
        NeighborNode curNode = new();
        NeighborNode newNeighbor = new();
        int neighborDistance;

        ClearHighlight();

        curNode.nodeLocation = curPos;
        curNode.nodeDistance = 0;

        nodesToVisit.Enqueue(curNode);
        visitedNodes.Add(curNode.nodeLocation);

        while (nodesToVisit.Count > 0)
        {
            curNode = nodesToVisit.Dequeue();
            neighborDistance = curNode.nodeDistance + 1;

            foreach (Vector3Int direction in DetermineNeighborDirections(curNode.nodeLocation.y % 2 == 0))
            {
                newPosition = curNode.nodeLocation + direction;

                if (!visitedNodes.Contains(newPosition) && map.HasTile(newPosition)
                    && (performedMove 
                        || (newPosition != map.WorldToCell(enemyFighter.transform.position)
                            && (!curEngaged || adjacentNodes.Contains(newPosition)))))
                {
                    visitedNodes.Add(newPosition);
                    SetHighlight(newPosition, highlightColor);

                    if (neighborDistance < highlightRange && !engagedNodes.Contains(newPosition))
                    {
                        newNeighbor.nodeLocation = newPosition;
                        newNeighbor.nodeDistance = neighborDistance;
                        nodesToVisit.Enqueue(newNeighbor);
                    }
                }
            }
        }
    }

    private void ClearHighlight()
    {
        foreach (Vector3Int location in visitedNodes)
        {
            SetHighlight(location, Color.white);
        }
        visitedNodes.Clear();
    }

    private void SetHighlight(Vector3Int atPos, Color highlight)
    {
        map.SetTileFlags(atPos, TileFlags.None);
        map.SetColor(atPos, highlight);
        map.SetTileFlags(atPos, TileFlags.LockColor);
    }

    public void StepMove()
    {
        if (!performedMove)
        {
            highlightRange = 1;
            ShowRangeHighlight(Color.green);
        }
        Debug.Log("Clicked StepMoveButton, Move Range is " + highlightRange);
    }
    public void HalfMove()
    {
        if (!performedMove)
        {
            highlightRange = 5;
            ShowRangeHighlight(Color.green);
        }
        Debug.Log("Clicked HalfMoveButton, Move Range is " + highlightRange);
    }
    public void FullMove()
    {
        if (!performedMove)
        {
            highlightRange = 10;
            ShowRangeHighlight(Color.green);
        }
        Debug.Log("Clicked FullMoveButton, Move Range is " + highlightRange);
    }

    public void QuitButtonPressed()
    {
        Application.Quit();
    }
}