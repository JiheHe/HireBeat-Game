using UnityEngine;
using System;
using SQL4Unity;
using System.Collections.Generic;
using System.Linq;

public class DataBaseCommunicator : MonoBehaviour
{
	public SQL4Unity.Server.Protocol Protocol = SQL4Unity.Server.Protocol.TCP;
	public string Database = "HireBeatProjectDB";
	public string UserName = string.Empty;
	public string IpAddress = "54.229.65.122"; // Local IP for testing. Updated to TCP

	public bool secure = false;
	int Port = 19390; // Default Client TCP Port. Replaced
	internal string UUID = "6657add8-97b8-41ce-a8cd-b8b48b83d489"; // Default UUID. Replace with yours. I did
	SQLExecute sql = null;

	VideoChatRoomSearch vcs; //where info will be published for video chat

	void Start()
	{
		//UserName = UnityEngine.GameObject.Find("PlayFabController").GetComponent<PlayFabController>().myID; //playfab user id as the username

		// Must be WebSocket for WebGL
		if (Application.platform == RuntimePlatform.WebGLPlayer) Protocol = SQL4Unity.Server.Protocol.WebSocket;
		if (Protocol == SQL4Unity.Server.Protocol.WebSocket)
		{
			Port = 19391; // Default Client WebSocket Port. Replaced
			IpAddress = "ec2-54-229-65-122.eu-west-1.compute.amazonaws.com"; //Note: For Websocket protocol IpAddress should be the server domain name.
		}

		Debug.Log("SQL database connection: Using Protocol " + Protocol + " to " + IpAddress + ":" + Port);
		// Monobehaviour required for Websocket and TCP Async Connections. Secure Socket = True/False required for WebSocket Protocol
		sql = new SQLExecute(this);
		sql.Connect(Protocol, IpAddress, Port, UUID, secure, false, UserName, ConnectCallback);

		vcs = UnityEngine.GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("VidCRoomSearch").GetComponent<VideoChatRoomSearch>();
	}

	// Called once a connection to the server has been made
	void ConnectCallback(bool ok)
	{
		Debug.Log("SQL database Connected:" + ok);

		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			sql.OpenAsync(Database, OpenCallback); // Copy database from StreamingAssets to PersisentData then open // Even if the remote connection failed SQL4Unity will fallback to using a local database
		}
		else
		{
			sql.Open(Database);
		}
	}

	// Called once the database has been Opened
	void OpenCallback(bool ok)
	{
		//sql.SyncWithServer(true); //DOn't sync with server! Everything will be server based if don't sync, which is good!
	}


    #region VideoChatRooms
	//Create a new vc room in the database
    public void CreateNewVCRoom(string roomName, string creatorID, bool isPublic)
    {
		string query = "execute CreateNewVCRoom";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("roomName", roomName);
		parameters.SetValue("creatorID", creatorID);
		parameters.SetValue("isPublic", isPublic);

		SQLResult result = new SQLResult();
		sql.Command(query, result, parameters); //or can do if(sql.Command(query, result, parameters))

		Debug.LogError("Creating a new room! Here's the result: " + result.message);
	}

	//this new method can help with room name check at create or find at search!
	public bool CheckVCRoomExists(string roomName) 
    {
		string query = "Select rowid from VideoChatsAvailable where RoomName = %roomName%";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("roomName", roomName);

		SQLResult result = new SQLResult();
		sql.Command(query, result, parameters);

		if (result.status) //this if statement might not be necessarily
		{
			try
			{
				if (result.rowsAffected == 0) return false; //0 rows selected if doesn't exist.
				else return true;
			}
			catch (Exception ex)
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.LogError(ex.Message);
				return false;
			}
		}
		Debug.LogError("Wrong output for vc room exists check");
		return false;
	}

	//Retrieve one vc room info in the database
	public hirebeatprojectdb_videochatsavailable RetrieveVCRoomInfo(string roomName)
    {
		string query = "execute RetrieveVCRoomInfo";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("roomName", roomName);

		//sql.Command(query, null, parameters, RetrieveVCRoomInfoCallback); //no tis more. 

		SQLResult result = new SQLResult();
		sql.Command(query, result, parameters);

		Debug.LogError("Retrieving room info! Here's the result: " + result.resultType.ToString() + " " + result.status.ToString() + " " + result.message);
		if (result.status) //this if statement might not be necessarily
		{
			try
			{
				hirebeatprojectdb_videochatsavailable row = result.Get<hirebeatprojectdb_videochatsavailable>()[0]; //result contains only 1 info of 1 room
																													//then here's a vc room display object
				return row;
			}
			catch (Exception ex) //this could be possible if the room no longer exists, cuz the top one will error.
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.Log(ex.Message);
				return null;
			}
		}
		Debug.LogError("Error during room info retrieving");
		return null;
	}

	//Grab all vc room infos in the database
	public void GrabAllVCRoomInfo()
    {
		string query = "execute GrabAllVCRoomInfo";

		//sql.Command(query, null, GrabAllVCRoomInfoCallback);

		SQLResult result = new SQLResult();
		sql.Command(query, result);

		Debug.Log("Retrieving all VC room info! Here's the result: " + result.resultType.ToString() + " " + result.status.ToString() + " " + result.message);
		if (result.status) 
		{
			try
			{
				hirebeatprojectdb_videochatsavailable[] rows = result.Get<hirebeatprojectdb_videochatsavailable>();
				vcs.UpdateVCRoomList(rows);
			}
			catch (Exception ex)
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.Log("Grabbing database failed: " + ex.Message);
			}
		}
	}

	//Gonna keep this here as a reminder: 
	//apparently when you call constructor-related and instantiate functions from callback functions (even in other objs),
	//these functions are no longer on the main thread, thus the error happens
	//if you don't do callbacks and just call them directly after command, then they count as main thread and thus register
	/*
	void GrabAllVCRoomInfoCallback(bool ok, SQLResult result)
    {
		if (ok)
		{
			Debug.Log("Retrieving all VC room info! Here's the result: " + result.resultType.ToString() + " " + result.status.ToString() + " " + result.message);
			if (result.status) //this if statement might not be necessarily
			{
				try
				{
					//Debug.Log("Rest are ffine.");
					hirebeatprojectdb_videochatsavailable[] rows = result.Get<hirebeatprojectdb_videochatsavailable>();
					vcs.UpdateVCRoomList(rows);

					//var rowDicts = result.Get(); 
					foreach(var rowDict in rowDicts.Values) //can do some arrangements here maybe
                    {
						string roomName = (string)rowDict["RoomName"];
						string currOwnerID = (string)rowDict["CurrOwnerID"];
						int numMembers = (int)rowDict["NumMembers"];
						bool isPublic = (bool)rowDict["IsPublic"];

						//if it's public then create a public display object

						//if it's private then create a private display object (hidden or not show?)

					}
				}
				catch (Exception ex)
				{
					// May throw an Illegal Cast Exception if the local database is missing
					Debug.Log("Grabbing database failed: " + ex.Message);
				}
			}
		}
		else
		{
			Debug.Log("Failed to retrieve all VC Room info.");
		}
	}*/

	//Update a VC room property in the database
	public void UpdateVCRoomProps(string roomName, string newOwnerID, int newNumMembers, bool newIsPublic)
    {
		string query = "execute UpdateVCRoomProps";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("roomName", roomName);
		parameters.SetValue("newOwnerID", newOwnerID);
		parameters.SetValue("newNumMembers", newNumMembers);
		parameters.SetValue("newIsPublic", newIsPublic);

		SQLResult result = new SQLResult();
		sql.Command(query, result, parameters);

		Debug.LogError("Updating room info! Here's the result: " + result.message);
	}

	//a quick function I wrote, might be slightly more efficient
	public void UpdateVCRoomNumMembers(string roomName, int numMembers)
    {
		string query = "update VideoChatsAvailable set NumMembers = %newNumMembers% where RoomName = %roomName%";
		SQLParameter parameter = new SQLParameter();

		parameter.SetValue("newNumMembers", numMembers);
		parameter.SetValue("roomName", roomName);

		SQLResult result = new SQLResult();
		sql.Command(query, result, parameter);

		Debug.LogError("Updating room numMembers! Here's the result: " + result.message);
	}

	//Delete a VC room in the database
	public void DeleteVCRoom(string roomName)
    {
		string query = "execute DeleteVCRoom";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("roomName", roomName);

		SQLResult result = new SQLResult();
		sql.Command(query, result, parameters);

		Debug.LogError("Deleting the selected VC Room! Here's the result: ");// + result.message);
	}

	//Delete every VC room in the database. If you are a user PLEASE DON'T ABUSE THIS; this is for internal only.
	public void DeleteALLVCRooms()
	{
		string query = "Delete from VideoChatsAvailable";

		SQLResult result = new SQLResult();
		sql.Command(query, result);

		Debug.LogError("Deleting every single VC Room!");// + result.message);
	}
	#endregion

	private void OnDestroy()
    {
		sql.Close();
	}

}