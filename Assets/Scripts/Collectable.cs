using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;

public class Collectable : MonoBehaviourPunCallbacks


{
    PhotonView view;
    public bool isOnline;

    public static event Action OnCollected;

    private GamePlayManager gamePlayManager;

    void Start()
    {
        gamePlayManager = GamePlayManager.instance;
    }
    void Update()
    {
        transform.localRotation = Quaternion.Euler(90f, Time.time * 100f, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.find);
        if (PhotonNetwork.InRoom) // Online mode
        {
            PhotonView otherView = other.GetComponent<PhotonView>();

            if (otherView != null && otherView.IsMine)
            {
                OnCollected?.Invoke();                
                               
                // Request MasterClient to destroy the object              
                photonView.RPC("RequestDestroy", RpcTarget.MasterClient, otherView.OwnerActorNr);
            }
        }
        else // Local multiplayer mode
        {
            if (other.name == "CharacterController1(Clone)")
            {
                OnCollected?.Invoke();
                Destroy(gameObject);
                gamePlayManager.UpdateAmmo(1, 1);
            }
            if (other.name == "CharacterController2(Clone)")
            {
                OnCollected?.Invoke();
                Destroy(gameObject);
                gamePlayManager.UpdateAmmo(2, 1);
            }
        }
    }
    [PunRPC]
    void RequestDestroy(int playerId)
    {
        // Ensure only the MasterClient handles this
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
            gamePlayManager.view.RPC("UpdateAmmo", RpcTarget.All, playerId, 1);  // Call the UpdateAmmo RPC
        }
    }

}