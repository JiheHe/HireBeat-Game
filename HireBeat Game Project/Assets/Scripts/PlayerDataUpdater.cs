using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayerDataUpdater : MonoBehaviour, IPunInstantiateMagicCallback
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instanceData = info.photonView.InstantiationData;

        ProfileUpdater updater = gameObject.transform.Find("PlayerMouseDetector").transform.Find("SkillCategoryFrame").transform.Find("PFP").GetComponent<ProfileUpdater>();
        string imgString = SpawnPlayers.ConnectArrayOfSubstrings((string[])instanceData[13]); //need to break img string down to n parts, else too long
        byte[] pfpByteArr = Convert.FromBase64String(imgString);
        updater.changeSpriteRPC(pfpByteArr);

        gameObject.GetComponent<CompositionSetter>().RetrieveCharInfo(
            (string)instanceData[0],
            PersistentData.strToBool((string)instanceData[1]),
            PersistentData.strToBool((string)instanceData[2]),
            PersistentData.strToBool((string)instanceData[3]),
            PersistentData.strToBool((string)instanceData[4]),
            (string)instanceData[5],
            (string)instanceData[6],
            (string)instanceData[7],
            (string)instanceData[8],
            (string)instanceData[9],
            (string)instanceData[10],
            (string)instanceData[11],
            (string)instanceData[12]); 
        gameObject.GetComponent<CompositionSetter>().initializeCharData();
    }
}