using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class MapManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap map;

    [SerializeField]
    Fighter pfFighterP;
    [SerializeField]
    int startX, startY;

    Fighter fighterP;
    private Vector3Int fighterPos;
    private int highlightRange;
    bool performedMove = false;

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

    private TileBase prevClickedTile = null;
    private List<Vector3Int> visitedNodes = new();

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
        fighterPos = new Vector3Int(startX, startY, 0);
        fighterP = Instantiate(pfFighterP);
        fighterP.transform.position = map.CellToWorld(fighterPos);
        fighterP.transform.rotation = Quaternion.Euler(0f, 0f, 210f);
    }

    // Update is called once per frame
    private void Update()
    {
        if(Input.GetMouseButtonUp(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = map.WorldToCell(mousePosition);
            int rotationIndex;

            TileBase clickedTile = map.GetTile(gridPosition);

            if (map.HasTile(gridPosition) && visitedNodes.Contains(gridPosition))
            {
                if (performedMove)
                {
                    rotationIndex = visitedNodes.IndexOf(gridPosition);
                    Debug.Log("Rotation Index is " + rotationIndex);
                    fighterP.transform.rotation = Quaternion.Euler(0f, 0f, (float)(330 - 60*rotationIndex));
                    ClearHighlight();
                    performedMove = false;
                }
                else
                {
                    fighterP.transform.position = map.CellToWorld(gridPosition);
                    fighterPos = gridPosition;
                    prevClickedTile = clickedTile;
                    performedMove = true;
                    highlightRange = 1;
                    ShowRangeHighlight(Color.yellow);
                }
            }

            Debug.Log("Clicked " + clickedTile + " at position " + gridPosition);
        }        
    }

    private void ShowRangeHighlight(Color highlightColor)
    {
        Vector3Int newPosition;
        Queue<NeighborNode> nodesToVisit = new();
        NeighborNode curNode = new();
        NeighborNode newNeighbor = new();
        int neighborDistance;

        ClearHighlight();

        curNode.nodeLocation = fighterPos;
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

                if (!visitedNodes.Contains(newPosition) && map.HasTile(newPosition))
                {
                    visitedNodes.Add(newPosition);
                    SetHighlight(newPosition, highlightColor);

                    if (neighborDistance < highlightRange)
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