using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpawnPlayers : MonoBehaviour
{

    public GameObject player;
    public float minX, minY, maxX, maxY;

    public GameObject manager;
    public GameObject background; //also unique to individual

    //THIS IS TO INDIVIDUAL AS WELL!
    //probably is?

    //GameObject newPlayer;


    // Start is called before the first frame update
    void Start()
    {
        Vector2 randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        GameObject newPlayer = PhotonNetwork.Instantiate(player.name, randomPosition, Quaternion.identity);
        Debug.Log("A new player has joined!");

        //shouldn't be on the same frame else null, give it some time to spawn in then change
        StartCoroutine(InstantiateAppearence(newPlayer, 0.0001f));

        //assign the camera to the current, local manager
        manager.GetComponent<cameraController>().zoomCamera = newPlayer.transform.Find("PlayerCamera").GetComponent<Camera>();
        
        //assign background's camera to point towards the new player's
        background.GetComponent<Canvas>().worldCamera = newPlayer.transform.Find("PlayerCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //use this to spawn multiplayer with their char data!
    IEnumerator InstantiateAppearence(GameObject newPlayer, float time)
    {
        yield return new WaitForSeconds(time);

        //render your data
        newPlayer.GetComponent<CompositionSetter>().RetrieveCharInfo("Jokester", true, true, true, true, "Dark",
            "Jojo", "Green", "ArmorTop", "Purple", "Pants", "Purple", "Navy"); //can grab these from char info data base
        newPlayer.GetComponent<CompositionSetter>().updateChar();
    }
}
