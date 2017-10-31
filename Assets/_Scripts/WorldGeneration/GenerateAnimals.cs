using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public struct gridPoint
{
    public float x;
    public float z;

    public gridPoint(float _x, float _z)
    {
        x = _x;
        z = _z;
    }

}

public class GenerateAnimals : MonoBehaviour {

    public GenerateAnimals instance;

    [Header("Variables for generating Animals")]
    public GameObject animalPrefab;
    public GameObject animalsObject;
    public int animalCount;

    [Space(20)]
    [Header("Variables for generating obstacles")]
    public GameObject obstaclePrefab;
    public GameObject obstaclesObject;
    public int obstacleCount;

    [Space(20)]
    [Header("Variables for generating Food")]
    public GameObject foodPrefab;
    public GameObject foodObject;
    public int foodCount;

    [Space(10)]
    [Header("Variable to make sure you don't initialize a different object on the same point")]
    [SerializeField]
    private List<gridPoint> currentGeneratedPoints = new List<gridPoint>();
    [SerializeField]
    private gridPoint currentGridPoint;

    [Space(20)]
    public Grid grid;


    [Space(20)]
    public Slider mapSize;
    public Slider animalSize;
    public Slider obstacleSize;
    public Slider foodSize;
    public Dropdown aiLevel;

    [Space(10)]
    [SerializeField]
    private int mapSizeInt;
    [SerializeField]
    private int animalInt;
    [SerializeField]
    private int obstacleInt;
    [SerializeField]
    private int foodInt;
    [SerializeField]
    private int aiLevelInt;

    private void Awake()
    {
        if ( null == instance )
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(transform.gameObject);

    }

    // Use this for initialization
    void Start () {
		

    }

    public void GenerateClicked()
    {

        SceneManager.LoadScene("Generated");

    }

    private void OnLevelWasLoaded(int level)
    {
        if ( level == 1)
        {
            GenerateStuff();
        }
    }

    void GenerateStuff()
    {

        grid = GameObject.FindGameObjectWithTag("PathfindingHelper").GetComponent<Grid>();

        animalsObject = GameObject.FindGameObjectWithTag("AnimalsHolder");
        obstaclesObject = GameObject.FindGameObjectWithTag("ObstacleHolder");
        foodObject = GameObject.FindGameObjectWithTag("FoodHolder");

        animalCount = animalInt;
        obstacleCount = obstacleInt;
        foodCount = foodInt;

        MouseSelect.instance.AISelect.value = aiLevelInt;

        for (int i = 0; i < animalCount; i++)
        {

            //int posX = Mathf.RoundToInt(Random.Range(-grid.gridWorldSize.x/2, grid.gridWorldSize.x/2));
            //int posZ = Mathf.RoundToInt(Random.Range(-grid.gridWorldSize.y/2, grid.gridWorldSize.y/2));
            currentGridPoint = RandomGridNumbers();
            Instantiate(animalPrefab, new Vector3(currentGridPoint.x, 0.5f, currentGridPoint.z), Quaternion.identity, animalsObject.transform);
        }

        for (int i = 0; i < obstacleCount; i++)
        {

            //int posX = Mathf.RoundToInt(Random.Range(-grid.gridWorldSize.x / 2, grid.gridWorldSize.x / 2));
            //int posZ = Mathf.RoundToInt(Random.Range(-grid.gridWorldSize.y / 2, grid.gridWorldSize.y / 2));

            currentGridPoint = RandomGridNumbers();
            GameObject g = Instantiate(obstaclePrefab, new Vector3(currentGridPoint.x, 0.5f, currentGridPoint.z), Quaternion.identity);
            g.transform.SetParent(obstaclesObject.transform);

        }

        for (int i = 0; i < foodCount; i++)
        {

            //int posX = Mathf.RoundToInt(Random.Range(-grid.gridWorldSize.x / 2, grid.gridWorldSize.x / 2));
            //int posZ = Mathf.RoundToInt(Random.Range(-grid.gridWorldSize.y / 2, grid.gridWorldSize.y / 2));

            currentGridPoint = RandomGridNumbers();
            GameObject g = Instantiate(foodPrefab, new Vector3(currentGridPoint.x, 0.5f, currentGridPoint.z), Quaternion.identity);
            g.transform.SetParent(foodObject.transform);

        }

    }

    gridPoint RandomGridNumbers()
    {

        int posX = Mathf.RoundToInt(Random.Range(-grid.gridWorldSize.x / 2, grid.gridWorldSize.x / 2));
        int posZ = Mathf.RoundToInt(Random.Range(-grid.gridWorldSize.y / 2, grid.gridWorldSize.y / 2));

        gridPoint returnValue = new gridPoint(posX, posZ);

        if (currentGeneratedPoints.Contains(returnValue))
        {
            return RandomGridNumbers();
        }

        currentGeneratedPoints.Add(returnValue);
        return returnValue;

    }
	
	// Update is called once per frame
	void Update () {

        if ( null != mapSize)
            mapSizeInt = Mathf.RoundToInt(mapSize.value);

        if ( null != animalSize )
            animalInt = Mathf.RoundToInt(animalSize.value);

        if ( null != obstacleSize)
            obstacleInt = Mathf.RoundToInt(obstacleSize.value);

        if ( null != foodSize)
            foodInt = Mathf.RoundToInt(foodSize.value);

        if ( null != aiLevel)
        {
            aiLevelInt = aiLevel.value;
        }

    }
}
