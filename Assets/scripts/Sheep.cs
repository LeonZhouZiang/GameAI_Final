using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    [HideInInspector]
    public float satiation;
    public float hungerRate = 1.2f;
    public float searchRange = 5;

    public static int population;

    private void Awake()
    {
        navMeshAgent = GetComponentInChildren<NavMeshAgent>();
        satiation = 55;
        population++;
        SetRandomDestination();
    }

    bool needNewFood = true;
    // Update is called once per frame
    void Update()
    {
        satiation -= Time.deltaTime * hungerRate;
        if (satiation <= 0)
        {
            Destroy(gameObject);
            return;
        }
        else if (satiation >= 95)
        {
            satiation -= 45;
            Reproduce();
        }
        else if (satiation >= 100)
        {
            satiation = 100;
        }

        if (satiation <= 85 && needNewFood)
        {
            if (FindFood())
            {
                needNewFood = false;
            }
        }

        //reached
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.2f)
        {
            SetRandomDestination();
            needNewFood = true;
        }

        
    }

    Vector3 targetFood;
    

    public bool FindFood()
    {
        Collider[] bushes = Physics.OverlapSphere(transform.position, searchRange, LayerMask.GetMask("Bush"));
        if(bushes.Length > 0)
        {
            //float closestDst = float.MaxValue;
            //foreach(var bush in bushes)
            //{
            //    float myDst = Vector3.SqrMagnitude(bush.transform.position - transform.position);
            //    if(myDst < closestDst)
            //    {
            //        closestDst = myDst;
            //        targetFood = bush.transform.position;
            //    }
            //}
            targetFood = bushes[Random.Range(0, bushes.Length)].transform.position;

            //go for food
            navMeshAgent.SetDestination(targetFood);
            return true;
        }

        return false;
    }

    private void SetRandomDestination()
    {
        Vector3 randomPoint = GetRandomPointOnNavMesh(transform.position, 12f);

        // Set the destination for the NavMeshAgent
        navMeshAgent.SetDestination(randomPoint);
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

    private void Reproduce()
    {
        GameManager.instance.ReproduceSheep(transform.position);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bush"))
        {
            satiation += 25;
            GameManager.instance.allBushes.Remove(other.gameObject);
            Destroy(other.gameObject);
            GameManager.instance.totalBushAmount--;
        }
    }

    private void OnDestroy()
    {
        GameManager.instance.allSheeps.Remove(gameObject);
        population--;
    }

}
