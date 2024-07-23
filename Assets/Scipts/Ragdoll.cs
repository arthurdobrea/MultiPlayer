using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D.IK;

public class Ragdoll : NetworkBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private List<Collider2D> colliders;
    [SerializeField] private List<HingeJoint2D> joints;
    [SerializeField] private List<Rigidbody2D> rbs;
    [SerializeField] private List<LimbSolver2D> solvers;


    public override void OnNetworkSpawn()
    {
        ToggleRagdoll(false);
    }
    
    public void ToggleRagdoll(bool ragdollOn)
    {
        anim.enabled = !ragdollOn;

        foreach (var col in colliders)
        {
            col.enabled = ragdollOn;
        }

        foreach (var join in joints)
        {
            join.enabled = ragdollOn;
        }

        foreach (var rb in rbs)
        {
            rb.simulated = ragdollOn;
        }

        foreach (var solver in solvers)
        {
            solver.weight = ragdollOn ? 0 : 1;
        }
    }
}