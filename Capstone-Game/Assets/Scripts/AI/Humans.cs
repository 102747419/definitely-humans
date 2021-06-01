using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum HumanStates
{
    PathFollowing,
    Chase,
    Catch,
    Friendly,
}

public class Humans : MonoBehaviour
{
    //------------States------------------//
    private HumanStates currentState;

    //----------Path Following -----------//
    [Tooltip("Do you want to npc to pause on each point?")]
    [SerializeField]
    bool walkingPause;

    [Tooltip("added change for NPC to turn around")]
    [SerializeField]
    float turnAroundChance = 0.2f;

    [Tooltip("Add ammount of path points for human to walk to")]
    [SerializeField]
    List<WayPoints> pathPoints;

    [Tooltip("adds a home area that acts as boundary")]
    public HomePoint homePoint;

    public GameObject burger;

    NavMeshAgent navMesh;
    int currentPathPt;
    bool walking;
    bool waiting;
    bool walkForward;
    float waitTimer;

    //--------------Friendly----------------//
    [Tooltip("Is the NPC friendly?")]
    [SerializeField]
    public bool isFriendly;

    bool givenfood = false;
    float watchedFor = 0.0f;
    float watchTimer = 10.0f;

    //--------------Chase----------------//
    [Tooltip("Detection Radius")]
    [SerializeField]
    public float range = 2f;

    [Tooltip("How long does the human chase the player")]
    [SerializeField]
    float chaseTime = 10f;

    float chaseTimer;
    Transform target;

    public void Start() 
    {
        navMesh = this.GetComponent<NavMeshAgent>();
        chaseTimer = chaseTime;
        
        target = GameObject.FindWithTag("Player").transform;

        currentState = HumanStates.PathFollowing;
        if(navMesh == null)
        {
            Debug.Log("No nav mesh");
        }
        else
        {
            if (pathPoints != null && pathPoints.Count >= 2)
            {
                currentPathPt = 0;
                SetDest();
            }
            else
            {
                SetDest();
                Debug.Log("Add more points to walk between");
            }
        }


    }

    public void Update() 
    {
        
        float distance = Vector3.Distance(target.position, transform.position);
    
        // -----States------
        switch(currentState)
        {
            case HumanStates.PathFollowing:
            {
                PathFollowingState();
                break;
            }

            case HumanStates.Chase:
            {
                ChaseState();
                break;
            }

            case HumanStates.Friendly:
            {
                FriendlyState();
                break;
            }

            case HumanStates.Catch:
            {
                CatchingState();
                break;
            }
        }
    }

    private void CatchingState()
    {
        //
    }

    private void ChaseState()
    {  
        if(checkBoundry() == true)
        {
            SeePlayer();
            navMesh.SetDestination(target.position);

            if (chaseTimer > 0f)
            {   
                chaseTimer -= Time.deltaTime;
                    
            }
            else
            {
                chaseTimer = chaseTime;
                        
                SetDest();
                currentState = HumanStates.PathFollowing;
            }
        }
        else
        {
            SetDest();
            currentState = HumanStates.PathFollowing;
        }
                
    }

    private void FriendlyState()
    {
        watchedFor += Time.deltaTime;
        facePlayer();
        navMesh.SetDestination(transform.position);
        if (!givenfood)
        {
           Instantiate(burger, new Vector3(transform.position.x, transform.position.y , transform.position.z), Quaternion.identity); 
           givenfood = true;
        }
        if(watchedFor > watchTimer )
        {
            currentState = HumanStates.PathFollowing;
        }
    }

    private void PathFollowingState()
    {
        bool canSee = SeePlayer();
        
        if(canSee)
        {
            if(isFriendly)
            {
                currentState = HumanStates.Friendly;
            }
            else
            {
                currentState = HumanStates.Chase;
            }
        }

        if(walking && navMesh.remainingDistance <= 1.0f)
        {
            walking = false;
                    
            if(walkingPause)
            {
                waiting = true;
                waitTimer = 0f;
            }
            else
            {
                ChangePathPt();
                SetDest();
            }
        }

        if(waiting)
        {
            waitTimer += Time.deltaTime;

            if(waitTimer >= pathPoints[currentPathPt].waitForThisLong)
            {
                waiting = false;

                ChangePathPt();
                SetDest();
            }
        }

        
    }
    public void facePlayer()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private bool SeePlayer()
    {
        
        float distance = Vector3.Distance(target.position, transform.position);
        Vector3 targetDir = target.position - transform.position;
        float angle = 45f;
        float angleToPlayer = (Vector3.Angle(targetDir, transform.forward));

        RaycastHit hit;
 
        if ((angleToPlayer >= -angle && angleToPlayer <= angle) && (distance <= range))
        {
            if(Physics.Linecast (transform.position, target.transform.position, out hit))
            {
                if(hit.transform.tag == "Player")
                {
                    return true;
                }
            }
        }
        return false;
    } 

    bool checkBoundry()
    {
        float dist = Vector3.Distance(homePoint.transform.position, transform.position);
        if (dist > homePoint.boundary)
        {
            return false;
        }
        else
        {
             return true;
        } 
    }

    private void SetDest()
    {
        if (pathPoints != null)
        {
            Vector3 targetVector = pathPoints[currentPathPt].transform.position;
            navMesh.SetDestination(targetVector);
            
            walking = true;
        }
    }

    private void ChangePathPt()
    {
        if (UnityEngine.Random.Range(0f, 1f) <= turnAroundChance)
        {
            walkForward = !walkForward;
        }

        if (walkForward)
        {
            currentPathPt = (currentPathPt + 1) % pathPoints.Count;
            
        }
        else 
        {
            currentPathPt--;
            if (currentPathPt < 0)
            {
                currentPathPt = pathPoints.Count - 1;
                
            }
        }
    }

    void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

