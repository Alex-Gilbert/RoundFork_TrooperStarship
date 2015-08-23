using UnityEngine;
using System.Collections;

public enum SoldierState
{
    Patrol,
    Alerted,
    Aggro,
    Hurt,
    Dead,
    ComingToAid,

}

public class Soldier : MonoBehaviour 
{


    public Transform[] patrolRoute;

    int curWP = 0;

    public float PatrolSpeed = 2;

    public Transform playerT;

    public Animator animator;
    AnimatorStateInfo stateInfo;
    public Transform spriteTransform;

    public float sightArcDegree;
    public float raysToCast;

    public float rayCheckLength = 10;
    public float TimeToRayCheck;
    float rayCheckTime;

    NavMeshAgent agent;

    private SoldierState state = SoldierState.Patrol;

    public float playerShootRange = 10;

    public float TimeToShoot;
    float shootTime;

    bool chasing = false;
    public float timeToChase = 5;
    float chaseTime;

    bool alerted = false;
    public float timeToStayAlert = 5;
    float alertTime;

    public float shotDamage = .25f;

    public int health = 4;

    Vector3 woundedSoldierPosition;

	void Start () 
    {
        GameBroadcaster.Instance.SoldierNeedsHelp += HandleSoldierNeedsHelp;
        GameBroadcaster.Instance.PlayerMadeNoise += HandlePlayerMadeNoise;

        agent = GetComponent<NavMeshAgent>();
        rayCheckTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (state != SoldierState.Dead)
        {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        chasing = false;
        switch(state)
        { 
            case SoldierState.Patrol:
                if (Vector3.Distance(transform.position, patrolRoute[curWP].position) < 1.5f)
                {
                    curWP = (curWP + 1) % patrolRoute.Length;
                }

                agent.SetDestination(patrolRoute[curWP].position);
                agent.speed = PatrolSpeed;
                LookForPlayer();
                break;
            case SoldierState.Alerted:
                print("I am alerted");
                animator.SetBool("Walking", false);
                agent.SetDestination(transform.position);
                LookForPlayer();
                RotateTowardsPlayer();
                break;
            case SoldierState.Aggro:
                print("I am aggroed");
                bool canSeePlayer = CanSeePlayer();
                if(Vector3.Distance(transform.position, playerT.position) <= playerShootRange && canSeePlayer)
                {
                    animator.SetBool("Walking", false);
                    agent.SetDestination(transform.position);
                    LookAtPlayer();
                    Fire();
                }
                else
                {
                    chasing = true;
                    animator.SetBool("Walking", true);
                    agent.SetDestination(playerT.position);
                }
                break;
            case SoldierState.ComingToAid:
                animator.SetBool("Walking", true);
                agent.speed = PatrolSpeed * 2;
                if(Vector3.Distance(transform.position, woundedSoldierPosition) <= 5)
                {
                    animator.SetBool("Walking", true);
                    SwitchState(SoldierState.Alerted);
                }
                LookForPlayer();
                break;
            case SoldierState.Hurt:
                if(!stateInfo.IsName("Hurt"))
                {
                    SwitchState(SoldierState.Alerted);
                }
                break;

        }

        
            if (chasing)
            {
                if (Time.time - chaseTime >= timeToChase)
                {
                    SwitchState(SoldierState.Alerted);
                    chasing = false;
                }
            }
            else
            {
                chaseTime = Time.time;
            }

            if (state == SoldierState.Alerted)
            {
                if (Time.time - alertTime >= timeToStayAlert)
                {
                    SwitchState(SoldierState.Patrol);
                    animator.SetTrigger("Calmed");
                }
            }
            else
            {
                alertTime = Time.time;
            }

            float angleDif = 0;

            Debug.DrawRay(transform.position, transform.forward, Color.cyan);
            Debug.DrawRay(transform.position, new Vector3(spriteTransform.forward.x, 0, spriteTransform.forward.z), Color.magenta);

            angleDif = Quaternion.FromToRotation(transform.forward, new Vector3(spriteTransform.forward.x, 0, spriteTransform.forward.z)).eulerAngles.y;

            animator.SetFloat("AngleDif", angleDif);
        }
	}

    void Executed()
    {
        Invoke("Delete", .2f);
    }

    void Delete()
    {
        gameObject.SetActive(false);
    }

    void OnHit()
    {
        if (state != SoldierState.Dead)
        {
            GameBroadcaster.Instance.OnSoldierNeedsHelp(gameObject, 100);

            SwitchState(SoldierState.Hurt);
            animator.SetTrigger("Hurt");

            health--;

            if (health <= 0)
            {
                SwitchState(SoldierState.Dead);
                animator.SetBool("Death", true);
            }

            print("ow! Ive been hit");
        }
    }

    void Fire()
    {
        if(Time.time - shootTime >= TimeToShoot)
        {
            shootTime = Time.time;

            if (Random.Range(0f, 1f) < .75f)
            {
                print("I shot the player!");


                Debug.DrawLine(transform.position, transform.position + transform.forward * playerShootRange, Color.red, .25f);

                Ray ray = new Ray(transform.position, transform.forward);
                RaycastHit hit;

                animator.SetTrigger("Fire");

                if (Physics.Raycast(ray, out hit, rayCheckLength))
                {
                    if (hit.collider.gameObject.tag == "Player")
                    {
                        
                        GameBroadcaster.Instance.OnPlayerHit(gameObject, shotDamage);
                    }
                }

            }
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 toLookAt = playerT.position - transform.position;
        toLookAt.y = 0;

        Quaternion lr = Quaternion.LookRotation(toLookAt);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lr, 50 * Time.deltaTime);
    }

    void LookAtPlayer()
    {
        Vector3 fromRot = transform.forward;
        Vector3 toRot = playerT.position - transform.position;

        fromRot.y = 0;
        toRot.y = 0;

        Quaternion refShift = Quaternion.FromToRotation(fromRot, toRot);
        transform.rotation *= refShift;
    }

    bool CanSeePlayer()
    {
        Vector3 dir = transform.forward;
        Debug.DrawLine(transform.position, transform.position + dir * rayCheckLength, Color.green, .25f);

        Ray ray = new Ray(transform.position, dir);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayCheckLength))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                print("I see the player!!");
                return true;
            }
        }

        return false;
    }

    void LookForPlayer()
    {
        if(Time.time - rayCheckTime >= TimeToRayCheck)
        {
            rayCheckTime = Time.time;
            
            Vector3 dir = transform.forward;
            dir = Quaternion.Euler(0, sightArcDegree * .5f, 0) * dir;

            Quaternion refShift = Quaternion.Euler(0, -(sightArcDegree/ raysToCast), 0);

            for (int i = 0; i < raysToCast; ++i)
            {
                
                Debug.DrawLine(transform.position, transform.position + dir * rayCheckLength, Color.green, .25f);

                Ray ray = new Ray(transform.position, dir);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, rayCheckLength))
                {
                    if (hit.collider.gameObject.tag == "Player")
                    {
                        print("I see the player!!");
                        SwitchState(SoldierState.Aggro);
                    }
                }

                dir = refShift * dir;
            }
        }
    }

    void SwitchState(SoldierState newState)
    {
        curWP = 0;
        if ((state == SoldierState.Patrol ))
        {
            animator.SetTrigger("Alerted");
            agent.SetDestination(transform.position);
        }

        if(newState == SoldierState.Alerted)
        {
            alertTime = Time.time;
        }

        if(newState == SoldierState.Patrol)
        {
            agent.SetDestination(patrolRoute[curWP].position);
            animator.SetTrigger("Calmed");
        }

        state = newState;
    }

    void HandleSoldierNeedsHelp(GameObject soldier, float noise)
    {
        print("I can hear my brethren!");
        if(state != SoldierState.Dead && !soldier.Equals(gameObject))
        {
            switch(state)
            {
                case SoldierState.Patrol:
                    print("I can hear my brethren!");
                    float distance = Vector3.Distance(soldier.transform.position, transform.position);
                    if(distance <= noise)
                    {
                        if(distance <= noise * .5f)
                        {
                            SwitchState(SoldierState.Aggro);
                        }
                        else
                        {
                            woundedSoldierPosition = soldier.transform.position;
                            SwitchState(SoldierState.ComingToAid);
                            agent.SetDestination(woundedSoldierPosition);
                        }
                        
                    }
                    break;
                case SoldierState.Alerted:
                    SwitchState(SoldierState.Aggro);
                    break;
            }
        }
    }

    void HandlePlayerMadeNoise(GameObject player, float noiseRange)
    {
        if (state != SoldierState.Dead)
        {
            if (state != SoldierState.Aggro)
            {
                if (state == SoldierState.Alerted)
                    noiseRange *= 1.5f;

                float distance = Vector3.Distance(player.transform.position, transform.position);
                if (distance <= noiseRange)
                {
                    if (distance <= noiseRange * .5f)
                        SwitchState(SoldierState.Aggro);
                    else
                        SwitchState(SoldierState.Alerted);

                    print("I hear the player");
                }
            }
        }
    }
}
