using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Panda;
using System.Linq;

public class NPCAI : MonoBehaviour
{
    // this agent
    NavMeshAgent agent;
    // the player
    GameObject player;
    // the ground with Navmesh
    GameObject ground;
    // all NPCs (Non Player Character)
    GameObject[] npcs;

    // fleeing....
    // don't worry about player until in this range
    float visibleRange = 30f;
    // if player gets this close, run to another safe space
    float safeDistance = 10f;

    // player distance from this NPC (updated in Update loop)
    float playerDistance;

    // attacking ....
    // how many NPC friends you need to begin attacking
    int friendsNeeded = 2; // including yourself makes 3
    // distance to consider them friends
    float friendRange = 10f;
    // how close player needs to be before they start attacking
    float beginAttackRange = 10f;

    // killing ...
    // how many NPCs needed to swarm the player and kill him
    int killersNeeded = 3;
    // range to be considered killing
    float killRange = 2f;

    // keep moving to player destination
    bool moveToPlayer = false;

    // state object located on the player
    PlayerState playerState;

    void Awake()
    {
        // get stuff
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        ground = GameObject.FindGameObjectWithTag("Ground");
        npcs = GameObject.FindGameObjectsWithTag("NPC");
        playerState = player.GetComponent<PlayerState>();

    }

    // Update is called once per frame
    void LateUpdate()
    {
        // this is used a lot so keep it updated
        playerDistance = (transform.position - player.transform.position).magnitude;
        
        // keep this NPC moving to player if true
        if (moveToPlayer)
        {
            agent.SetDestination(player.transform.position);
        }
    }

    // typically move to static destination
    [Task]
    void MoveToPosition()
    {

        if (Task.isInspected)
            Task.current.debugInfo = "npc dest:" + agent.transform.position.ToString();

        if (agent.remainingDistance <= agent.stoppingDistance
            && agent.velocity.sqrMagnitude < 0.1f
            && !agent.pathPending)
        {
            Task.current.Succeed();
        }
    }

    // is NPC in attack mode
    [Task]
    bool IsAttacking()
    {
        return moveToPlayer;
    }

    // is NPC in visible range to the player
    [Task]
    void InVisibleRange()
    {

        var inRange = playerDistance <= visibleRange;

        if (Task.isInspected)
            Task.current.debugInfo = "inRange:" + inRange.ToString();

        Task.current.Complete(inRange);
    }

    // tell NPC to find a nice hiding place
    // and not too close to the player
    [Task]
    void FindHidingPlace()
    {
        // get all the vectors on the edge of the Navmesh
        var borderVectors = ground.GetComponent<GetNavmeshEdges>().BorderVectors;

        // load into objects with distance from player
        var bvects = borderVectors.Select(v => new NavEdge
        {
            Distance = (v - transform.position).magnitude,
            Position = v
        }).OrderBy(a => a.Distance); ;

        foreach (var e in bvects)
        {            
            // is hidden and a safe distance from the player
            var dist = (e.Position - player.transform.position).magnitude;
            if (IsPosHidden(e.Position))
            {
                if (Task.isInspected)
                    Task.current.debugInfo = "found hiding place:" + e.Position.ToString();

                // move to static location
                agent.SetDestination(e.Position);
                Task.current.Succeed();
                return;
            }
            
        }
        // could not find a hiding place
        Task.current.Fail();

    }

    // is the NPC in range to attack
    [Task]
    void InAttackRange()
    {

        var inRange = playerDistance <= beginAttackRange;

        if (Task.isInspected)
            Task.current.debugInfo = "in range:" + inRange.ToString();

        Task.current.Complete(inRange);
    }


    // set a flag to begin attacking
    [Task]
    void StartAttacking()
    {
        // converge on the player
        moveToPlayer = true;

        // add this NPC to the list of attacking NPCs
        playerState.AddAttackingNPC(this.gameObject);
        Task.current.Succeed();
    }

    [Task]
    void EndAttacking()
    {
        // stop converge on the player
        moveToPlayer = false;
        // remove NPC from list of attacking NPCs
        playerState.RemoveAttackingNPC(this.gameObject);

        Task.current.Succeed();
    }

    // Is the NPC out of visible range of the player
    [Task]
    bool IsHidden()
    {
        var ishidden = IsPosHidden(transform.position);
        return ishidden;
    }

    // is the NPC too close (even when hiding) to the player
    [Task]
    bool IsTooClose()
    {
        var tooClose = playerDistance < safeDistance;

        if (Task.isInspected)
            Task.current.debugInfo = "is too close:" + tooClose.ToString();

        return tooClose;
    }

    // is the player under attack
    [Task]
    bool IsPlayerUnderAttack()
    {
        return playerState.UnderAttack();
    }

    // restart the Unity scene
    [Task]
    void RestartScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        Task.current.Succeed();
    }

    // Check if the player is dead
    // Either this NPC has killed player, or player is already dead
    [Task]
    bool HasKilledPlayer()
    {      
        if (playerState.GetDead())
        {
            // player is already killed from some other NPCs
            return true;
        }
        else
        {

            int scnt = 0;
            var playerDead = false;
            foreach (var f in npcs)
            {
                var dist = (f.transform.position - player.transform.position).magnitude;
                if (dist < killRange)
                {
                    scnt++;
                }
                if (Task.isInspected)
                    Task.current.debugInfo = "swarm count:" + scnt.ToString();
                if (scnt >= killersNeeded)
                {
                    playerDead = true;
                    // player is killed once and now dead for all NPCs
                    playerState.SetDead();
                    break;
                }
            }

            return playerDead;
        }
    }

    // Check if NPC is together with N number of NPCs
    [Task]
    bool HasFriends()
    {
        int fcnt = 0;
        var hasFriends = false;
        foreach (var f in npcs)
        {
            if (f != this.gameObject)
            {
                var dist = (transform.position - f.transform.position).magnitude;
                if (dist < friendRange)
                {
                    fcnt++;
                }
                if (fcnt >= friendsNeeded)
                {
                    hasFriends = true;
                    break;
                }
            }
        }

        if (Task.isInspected)
            Task.current.debugInfo = "has friends:" + hasFriends.ToString();

        return hasFriends;
    }

    // Check if this position is...
    //   in visible range of player   
    //   not blocked by object tagged as "wall"
    //   not too close to player
    bool IsPosHidden(Vector3 pos)
    {
        var distToPlayer = player.transform.position - pos;

        RaycastHit hit;
        bool seeWall = false;
        //Debug.DrawRay(pos, distance, Color.green);

        if (Physics.Raycast(pos, distToPlayer, out hit))
        {
            seeWall = (hit.collider.gameObject.tag == "Wall");
        }

        var seePlayer = distToPlayer.magnitude < visibleRange && !seeWall;
        var tooClose = distToPlayer.magnitude < safeDistance;

        return !seePlayer && !tooClose;
    }

}

// data storage objectUnderAttack
public class NavEdge
{
    public Vector3 Position { set; get; }
    public float Distance { set; get; }
}
