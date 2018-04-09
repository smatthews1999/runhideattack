using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerState : MonoBehaviour
{

    // list of all NPCs attacking player
    private List<GameObject> NPCAttacking;
    // is player dead
    private bool amDead = false;
    private GameObject UI;

    // Use this for initialization
    void Start()
    {
        NPCAttacking = new List<GameObject>();
        UI = GameObject.FindGameObjectWithTag("UI");
        UI.SetActive(false);
    }

    public void AddAttackingNPC(GameObject npc)
    {
        if (!NPCAttacking.Any(a => a == npc))
        {
            NPCAttacking.Add(npc);
        }
    }
    public void RemoveAttackingNPC(GameObject npc)
    {
        if (NPCAttacking.Any(a => a == npc))
        {
            NPCAttacking.Remove(npc);
        }
    }

    public void SetDead()
    {
        amDead = true;
        UI.SetActive(true);
    }
    public bool GetDead()
    {
        return amDead;
    }

    public bool UnderAttack()
    {
        return NPCAttacking.Any();
    }

}
