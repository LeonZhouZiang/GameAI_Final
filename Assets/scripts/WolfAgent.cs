using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;

public class WolfAgent : Agent
{
    public static int population = 0;

    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool trainingMode;

    [Tooltip("Hunger rate")]
    public float hungerRate = 1.2f;

    [Tooltip("Detect range")]
    public float detectRange;

    public float satiationIncrement = 30;
    public float satiation = 100;

    [SerializeField]
    private bool isHunting = false;

    private GameObject nearestSheep;
    private NavMeshAgent navMeshAgent;

    private float decisionTimer = 0;
    private float decisionCD = 1;
    private void Awake()
    {
        isHunting = false;
        population++;
        navMeshAgent = GetComponent<NavMeshAgent>();
        satiation = 50 + Random.value;
    }

    private void FixedUpdate()
    {
        RequestDecision();
        RequestAction();
    }

    public void Update()
    {
        if(satiation <= 0)
        {
            StarveToDeath();
            return;
        }
        else
        {
            satiation -= Time.deltaTime * hungerRate;
        }
        if(satiation >= 100)
        {
            satiation = 100;
        }

        if (isHunting && nearestSheep == null)
        {
            if (!HuntSheep())
            {
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.2f)
                    SetRandomDestination();
            }
        }
        else if(isHunting && nearestSheep != null)
        {
            navMeshAgent.SetDestination(nearestSheep.transform.position);
        }
        else
        {
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.2f)
                SetRandomDestination();
        }
        
        if(decisionTimer < decisionCD)
        {
            decisionTimer += Time.deltaTime;
        }
        else
        {
            decisionTimer -= decisionCD;
        }
    }

    private bool HuntSheep()
    {
        Collider[] sheeps = Physics.OverlapSphere(transform.position, detectRange, LayerMask.GetMask("Sheep"));
        if (sheeps.Length > 0)
        {
            nearestSheep = sheeps[Random.Range(0, sheeps.Length)].gameObject;
            navMeshAgent.SetDestination(nearestSheep.transform.position);
            
            return true;
        }
        else
        {
            nearestSheep = null;
            return false;
        }
    }

    private void SetRandomDestination()
    {
        Vector3 randomPoint = GetRandomPointOnNavMesh(transform.position, 12f);

        // Set the destination for the NavMeshAgent
        navMeshAgent.SetDestination(randomPoint);
    }

    private void Reproduce()
    {
        GameManager.instance.ReproduceWolf(transform.position);
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

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKeyDown(KeyCode.H))
        {
            discreteActions[0] = 1;
        }
        else discreteActions[0] = 0;

        if (Input.GetKeyDown(KeyCode.R))
        {
            discreteActions[1] = 1;
        }
        else discreteActions[1] = 0;
    }

    public override void Initialize()
    {
        if (!trainingMode) MaxStep = 0;
    }

    private void StarveToDeath()
    {
        //if (population == 1)
        //{
        //    Debug.Log("restart");
        //    EndEpisode();
        //    GameManager.instance.Initialize();
        //    return;
        //}
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        GameManager.instance.allAgents.Remove(this);
        EndEpisode();
        population--;
    }

    /// <summary>
    /// discrete index 0: hunting or not
    ///  0 is false
    /// discrete index 1: reproduce or not
    /// 0 is false
    /// </summary>
    /// <param name="actions"></param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        if(actions.DiscreteActions[0] == 1)
        {
            Debug.Log("switch hunting mode");
            isHunting = true;
        }

        if (actions.DiscreteActions[1] == 1)
        {
            Debug.Log("try reproduce");
            if (satiation >= 80)
            {
                satiation -= 50;
                Reproduce();
            }
        }

    }

    /// <summary>
    /// 1 + 1 + 1 + 1 = 4
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(isHunting ? 1.0f : 0f);
        sensor.AddObservation(satiation);
        //wolf total population
        sensor.AddObservation(population);
        sensor.AddObservation(Sheep.population);

    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if(isHunting && other.CompareTag("Sheep"))
    //    {
    //        //eat sheep
    //        Destroy(other.gameObject);
    //        satiation += satiationIncrement;
    //        isHunting = false;
    //    }
    //}


    private void OnTriggerStay(Collider other)
    {
        if (isHunting && other.CompareTag("Sheep"))
        {
            Debug.Log("Eat");
            //eat sheep
            Destroy(other.gameObject);
            satiation += satiationIncrement;
            isHunting = false;
        }
    }
}
