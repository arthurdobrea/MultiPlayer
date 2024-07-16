using Unity.Netcode;
using UnityEngine;

public class DisplayOnlyClientUiItems : NetworkBehaviour
{

    [SerializeField] private GameObject control;
    
    public override void OnNetworkSpawn()
    {
        control.SetActive(IsOwner);
    }
}