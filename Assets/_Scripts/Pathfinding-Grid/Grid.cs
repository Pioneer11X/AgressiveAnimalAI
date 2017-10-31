using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// This script converts the scene into Grid, which we can use later on for A* or some other pathing.


public class Grid : MonoBehaviour {

    [Header("Set the Layers Masks")]
    public LayerMask unwalkableArea;

    [Tooltip("The grid Size in the Unity")]
    public Vector2 gridWorldSize = new Vector2(50.0f, 50.0f);

    [Tooltip("The node radius. Half of the node Length")]
    public float nodeRadius = 0.5f;

    public int blockAvoidWeight = 200;
    public float collisionCheckCapsuleHeight = 3.0f;

    public Node[,] grid;

    [Space(10)]
    [Header("Animals and their location Data")]
    public string animalTag = "Animal";
    [SerializeField]
    private GameObject[] animals;
    public int animalNodeWeight = 500;


    float nodeDiameter;
    int gridSizeX, gridSizeZ;

    // Use this for initialization
    void Start()
    {

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeZ = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        animals = GameObject.FindGameObjectsWithTag(animalTag);

        CreateGrid();

    }

    public void UpdateGrid()
    {
        CreateGrid();

        //var enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Update the current pathing for the animals.

    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeZ];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeZ; y++)
            {

                int weight = 0;

                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableArea));

                grid[x, y] = new Node(walkable, worldPoint, x, y, weight);
            }
        }

        foreach(GameObject animal in animals)
        {
            if ( animal != null && animal.transform != null)
                GetNodeFromWorldPoint(animal.transform.position).weight += animalNodeWeight;
        }

    }

    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {

        float percentX = Mathf.Clamp01((worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x);
        float percentY = Mathf.Clamp01((worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeZ - 1) * percentY);

        try
        {
            return grid[x, y];
        }
        catch
        {
            Debug.Log(x + "," + y);
            return grid[0,0];
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        CreateGrid();
    }

    public List<Node> GetNeighbours(Node _node)
    {

        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = _node.gridX + x;
                int checkY = _node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeZ)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;

    }

    public List<List<Node>> paths = new List<List<Node>>();
    private void OnDrawGizmos()
    {
        return;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.weight < 1 && n.walkable) ? Color.white : Color.red;

                if ( null != paths && paths.Count >= 1)
                {
                    foreach (List<Node> path in paths)
                    {
                        if (path != null && path.Any(i => i.worldPos == n.worldPos))
                        {
                            Gizmos.color = Color.black;
                        }
                    }
                }
                

                Gizmos.DrawCube(n.worldPos, Vector3.one * (nodeDiameter - 0.1f));
            }
        }

    }
}
