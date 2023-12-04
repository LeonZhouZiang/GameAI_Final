using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [HideInInspector]
    public LinkedList<WolfAgent> allAgents = new();
    [HideInInspector]
    public LinkedList<GameObject> allSheeps = new();
    [HideInInspector]
    public LinkedList<GameObject> allBushes = new();

    private float rewardTimer = 0;
    private float rewardCD = 3;
    [Header("bush")]
    public GameObject bush;
    private float bushTimer = 0;
    public float bushCD = 5;

    public int bushIncrement = 10;
    [HideInInspector]
    public int totalBushAmount = 0;
    public int bushLimit = 30;
    [Header("Sheep")]
    public GameObject sheep;
    [Header("Wolf")]
    public GameObject wolf;

    [Space(14)]
    [HideInInspector]
    public float changeGrowRateTimer;
    public float changeGrowRateCD = 100;

    [Header("UI")]
    public TextMeshProUGUI sheepPopulation;
    public TextMeshProUGUI wolfPopulation;

    private void Awake()
    {
        instance = this;
    }

    private void FixedUpdate()
    {
        sheepPopulation.text = Sheep.population + " sheep";
        wolfPopulation.text = WolfAgent.population + " wolves";
        if (WolfAgent.population <= 0)
        {
            Initialize();
        }
    }
    void Start()
    {
        GenerateBush();
        SpawnSheep();
    }

    public void Initialize()
    {
        foreach(var wolf in allAgents)
        {
            Destroy(wolf.gameObject);
        }
        allAgents.Clear();
        foreach(var sheep in allSheeps)
        {
            Destroy(sheep);
        }
        allSheeps.Clear();
        foreach(var bush in allBushes)
        {
            Destroy(bush);
        }
        allBushes.Clear();

        GenerateBush();
        SpawnSheep();
    }

    void SpawnSheep()
    {
        for(int i = 0; i < 8; i++)
        {
            Vector3 randomPoint = GetRandomPointOnNavMesh(Vector3.zero, 12f);
            GameObject obj = Instantiate(sheep, randomPoint, Quaternion.identity);
            allSheeps.AddLast(obj);
        }

        for (int i = 0; i < 2; i++)
        {
            GameObject obj = Instantiate(wolf);
            allAgents.AddLast(obj.GetComponent<WolfAgent>());
        }
        
    }

    private Vector3 GetRandomPointOnNavMesh(Vector3 center, float range)
    {
        NavMeshHit hit;
        Vector3 randomPoint = center + Random.insideUnitSphere * range;

        if (NavMesh.SamplePosition(randomPoint, out hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Vector3.positiveInfinity;
    }
    // Update is called once per frame
    void Update()
    {
        if(rewardTimer < rewardCD)
        {
            rewardTimer += Time.deltaTime;
        }
        else
        {
            rewardTimer -= rewardCD;
            GiveReward();
        }

        if(bushTimer < bushCD)
        {
            bushTimer += Time.deltaTime;
        }
        else
        {
            bushTimer -= bushCD;
            if (totalBushAmount <= bushLimit)
                GenerateBush();
        }

    }

    private void GiveReward()
    {
        Debug.Log("give reward");
        foreach(var wolf in allAgents)
        {
            wolf.AddReward(allAgents.Count / 20f);
        }
    }

    public void GenerateBush()
    {
        totalBushAmount += bushIncrement;
        int count = bushIncrement;
        while(count > 0)
        {
            Vector3 position = new Vector3(Random.Range(-19f, 19f), 1, Random.Range(-19f, 19f));
          
            // Direction pointing down (negative Y direction)
            Vector3 raycastDirection = Vector3.down;

            float raycastLength = 1.5f;

            // Perform the raycast
            RaycastHit hit;
            if (Physics.Raycast(position, raycastDirection, out hit, raycastLength, LayerMask.GetMask("Land")))
            {
                GameObject obj = Instantiate(bush, hit.point, Quaternion.identity);
                allBushes.AddLast(obj);
                count--;
            }  
        }
        
    }

    public void ReproduceSheep(Vector3 position)
    {
        var obj = Instantiate(sheep, position, Quaternion.identity);
        allSheeps.AddLast(obj);
    }

    public void ReproduceWolf(Vector3 position)
    {
        Debug.Log("New wolf!");
        GameObject newWolf = Instantiate(wolf, position, Quaternion.identity);
        WolfAgent agent = newWolf.GetComponent<WolfAgent>();
        allAgents.AddLast(agent);
    }
}
