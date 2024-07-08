using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Transform spawnedObjectPrefab;
    private Transform objectSpawned;

    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData { _int = 1, _bool = true }, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + " random number:" + newValue._int + " bool:" + newValue._bool + " message:" +
                      newValue.message);
        };
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateObjectServerRpc();
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            DestroyObjectServerRpc();
        }


        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.y = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.y = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;

        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    [ServerRpc]
    private void CreateObjectServerRpc()
    {
        objectSpawned = Instantiate(spawnedObjectPrefab);
        objectSpawned.GetComponent<NetworkObject>().Spawn(true);
    }
    
    [ServerRpc]
    private void DestroyObjectServerRpc()
    {
        Destroy(objectSpawned.gameObject);
    }
    

    [ClientRpc]
    private void TestClientRpc(ClientRpcParams pRpcParams)
    {
        Debug.Log("TestClientRpc " + OwnerClientId);
    }
}