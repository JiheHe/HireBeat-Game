using UnityEngine;
using System;
using SQL4Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class DataBaseCommunicator : MonoBehaviour
{
	public SQL4Unity.Server.Protocol Protocol = SQL4Unity.Server.Protocol.TCP;
	public string Database = "HireBeatProjectDB";
	public string UserName = string.Empty;
	public string IpAddress = "54.229.65.122"; // Local IP for testing. Updated to TCP

	public string myOwnIpAddress = string.Empty;
	public string myID = string.Empty;

	public bool secure = false;
	int Port = 19390; // Default Client TCP Port. Replaced
	internal string UUID = "6657add8-97b8-41ce-a8cd-b8b48b83d489"; // Default UUID. Replace with yours. I did
	SQLExecute sql = null;

	bool isOpen = false;

	VideoChatRoomSearch vcs; //where info will be published for video chat

	void Start()
	{
		myID = UnityEngine.GameObject.Find("PlayFabController").GetComponent<PlayFabController>().myID; //playfab user id as the username
		myOwnIpAddress = GetLocalIPv4(); //this is the ip address that the user connects to the server with.

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

	public string GetLocalIPv4() //this returns user's current ip address!
	{
		return Dns.GetHostEntry(Dns.GetHostName())
			.AddressList.First(
				f => f.AddressFamily == AddressFamily.InterNetwork)
			.ToString();
	}

	// Called once a connection to the server has been made
	void ConnectCallback(bool ok)
	{
		// NOT on Main Thread
		Debug.Log("SQL database Connected:" + ok);

		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			sql.OpenAsync(Database, OpenCallback); // Copy database from StreamingAssets to PersisentData then open // Even if the remote connection failed SQL4Unity will fallback to using a local database
		}
		else
		{
			sql.Open(Database); //no callback here...
			OpenCallback(true);
		}
	}

	// Called once the database has been Opened
	void OpenCallback(bool ok)
	{
		// Still not on Main Thread
		Debug.Log("Database Open:" + ok);
		//sql.SyncWithServer(true); //DOn't sync with server! Everything will be server based if don't sync, which is good!
		isOpen = true;

		AddIPAddressToUniqueID(myOwnIpAddress, myID);
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

		sql.Command(query, null, parameters, CreateNewVCRoomCallBack); //or can do if(sql.Command(query, result, parameters))
		//Debug.LogError("Creating a new room! Here's the result: " + result.message);
	}

	private void CreateNewVCRoomCallBack(bool ok, SQLResult result)
    {
		Debug.Log("New VC Room created"); //I'm not interested in the callback.
    }



	SQLResult CheckVCRoomResult;
	bool checkVCRoomResultReady;
	//this new method can help with room name check at create or find at search!
	public void CheckVCRoomExists(string roomName, string queuerName) 
    {
		// Yes on Main Thread
		checkVCRoomResultReady = false;

		string query = "Select rowid from VideoChatsAvailable where RoomName = %roomName%";

		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("roomName", roomName);

		sql.Command(query, null, parameters, CheckVCRoomExistsCallback);

		StartCoroutine(ReturnIfVCRoomExists((doesExist) =>
        {
			if(queuerName == "CreateRoomCheck")
            {
				vcs.OnCreateNewVCRoomSecondHalf(doesExist);
            }
			else if(queuerName == "SearchRoomCheck")
            {
				vcs.SearchSpecificRoomSecondHalf(doesExist);
            }
        }
		));// StartCoroutine on Main Thread
	}

	void CheckVCRoomExistsCallback(bool ok, SQLResult result)
    {
		// NOT on Main Thread
		CheckVCRoomResult = result;
		checkVCRoomResultReady = true;
	}

	IEnumerator ReturnIfVCRoomExists(Action<bool> callback)
    {
		yield return new WaitUntil(() => checkVCRoomResultReady);

		// Back on Main Thread
		if (CheckVCRoomResult.status) 
		{
			try
			{
				if (CheckVCRoomResult.rowsAffected == 0)
				{
					callback(false); //0 rows selected if doesn't exist.
				}
				else
				{
					callback(true);
				}
			}
			catch (Exception ex)
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.LogError(ex.Message);
			}
		}
	}



	SQLResult retrieveVCRoomInfoResult;
	bool retrieveVCRoomInfoReady;
	//Retrieve one vc room info in the database
	public void RetrieveVCRoomInfo(string roomName, string queuerName)
    {
		retrieveVCRoomInfoReady = false;

		string query = "execute RetrieveVCRoomInfo";

		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("roomName", roomName);

		sql.Command(query, null, parameters, RetrieveVCRoomInfoCallback); //no tis more. 

		StartCoroutine(ReturnVCRoomInfoRetrieved((row) =>
		{
			if (queuerName == "SearchRoomCheck")
			{
				vcs.SearchSpecificRoomThirdHalf(row);
			}
			else if (queuerName == "ConnectRoomCheck")
			{
				vcs.OnConnectPressedSecondHalf(row);
			}
		}
		));
	}

	void RetrieveVCRoomInfoCallback(bool ok, SQLResult result)
	{
		// NOT on Main Thread
		retrieveVCRoomInfoResult = result;
		retrieveVCRoomInfoReady = true;
	}

	IEnumerator ReturnVCRoomInfoRetrieved(Action<hirebeatprojectdb_videochatsavailable> callback)
    {
		yield return new WaitUntil(() => retrieveVCRoomInfoReady);

		Debug.Log("Retrieving room info! Here's the result: " + retrieveVCRoomInfoResult.resultType.ToString() +
			" " + retrieveVCRoomInfoResult.status.ToString() + " " + retrieveVCRoomInfoResult.message);
		if (retrieveVCRoomInfoResult.status) //this if statement might not be necessarily
		{
			try
			{
				hirebeatprojectdb_videochatsavailable row = retrieveVCRoomInfoResult.Get<hirebeatprojectdb_videochatsavailable>()[0]; 
				//result contains only 1 info of 1 room //then here's a vc room display object
				callback(row);
			}
			catch (Exception ex) //this could be possible if the room no longer exists, cuz the top one will error.
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.Log(ex.Message);
				callback(null);
			}
		}
		else
        {
			Debug.LogError("Error during room info retrieving");
			callback(null);
		}
	}



	//This is a simple function for vCC to grab current room owner, we are sure that the room exists.
	SQLResult retrieveVCRoomCurrentOwnerResult;
	bool retrieveVCRoomCurrentOwnerReady;
	//Retrieve one vc room info in the database
	public void RetrieveVCRoomCurrentOwner(string roomName) //this will only be used by vCC, so....
	{
		retrieveVCRoomCurrentOwnerReady = false;

		string query = "Select CurrOwnerID from VideoChatsAvailable where RoomName = %roomName%";

		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("roomName", roomName);

		sql.Command(query, null, parameters, RetrieveVCRoomCurrentOwnerCallback); //no tis more. 

		StartCoroutine(SendVCRoomOwnerInfo());
	}

	void RetrieveVCRoomCurrentOwnerCallback(bool ok, SQLResult result)
	{
		// NOT on Main Thread
		retrieveVCRoomCurrentOwnerResult = result;
		retrieveVCRoomCurrentOwnerReady = true;
	}

	IEnumerator SendVCRoomOwnerInfo()
	{
		yield return new WaitUntil(() => retrieveVCRoomCurrentOwnerReady);
		if (retrieveVCRoomCurrentOwnerResult.status) //this if statement might not be necessarily
		{
			try
			{
				string roomOwnerID = retrieveVCRoomCurrentOwnerResult.Get<hirebeatprojectdb_videochatsavailable>()[0].CurrOwnerID;
				//No need for callback since only vCC will be needing this.
				if(vcs.vCC != null) vcs.vCC.OnDisconnectPressedSecondHalf(roomOwnerID); //we know vCC won't be null, but it could be null after first call
			}
			catch (Exception ex) //this could be possible if the room no longer exists, cuz the top one will error.
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.LogError(ex.Message);
			}
		}
		else
		{
			Debug.LogError("Error during room owner info retrieving");
		}
	}



	SQLResult grabAllVCRoomInfoResult;
	bool grabAllVCRoomInfoReady;
	//Grab all vc room infos in the database
	public void GrabAllVCRoomInfo(string queuerName)
    {
		grabAllVCRoomInfoReady = false;

		string query = "execute GrabAllVCRoomInfo";

		sql.Command(query, null, GrabAllVCRoomInfoCallback);

		StartCoroutine(ReturnAllVCRoomInfoGrabbed((rows) =>
		{
			if (queuerName == "VCRoomListTotalUpdate")
			{
				vcs.UpdateVCRoomList(rows); 
			}
			else if (queuerName == "VCRoomPublicity")
			{
				vcs.SortVCRoomsByPublicitySecondHalf(rows);
			}
			else if(queuerName == "VCRoomKeyword")
            {
				vcs.ListVCRoomsWithKeywordSecondHalf(rows);
            }
		}));
	}

	void GrabAllVCRoomInfoCallback(bool ok, SQLResult result)
	{
		// NOT on Main Thread
		grabAllVCRoomInfoResult = result;
		grabAllVCRoomInfoReady = true;
	}

	IEnumerator ReturnAllVCRoomInfoGrabbed(Action<hirebeatprojectdb_videochatsavailable[]> callback)
	{
		yield return new WaitUntil(() => grabAllVCRoomInfoReady);

		Debug.Log("Retrieving all VC room info! Here's the result: " + grabAllVCRoomInfoResult.resultType.ToString() + " " +
			grabAllVCRoomInfoResult.status.ToString() + " " + grabAllVCRoomInfoResult.message);
		if (grabAllVCRoomInfoResult.status)
		{
			try
			{
				hirebeatprojectdb_videochatsavailable[] rows = grabAllVCRoomInfoResult.Get<hirebeatprojectdb_videochatsavailable>();
				callback(rows);
			}
			catch (Exception ex)
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.LogError("Grabbing database failed: " + ex.Message);
			}
		} 
		else
        {
			Debug.LogError("Retrieving ALL VC room info failed!");
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

	public void AddIPAddressToUniqueID(string ipAddress, string uniqueID)
    {
		string query = "insert into IPAdressToUniqueID (IPAddress, UniqueID) values (%ipAddress%, %uniqueID%)";

		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("ipAddress", ipAddress);
		parameters.SetValue("uniqueID", uniqueID);

		sql.Command(query, null, parameters, AddIPAddressToUniqueIDCallback); //or can do if(sql.Command(query, result, parameters))
	}
	private void AddIPAddressToUniqueIDCallback(bool ok, SQLResult result)
    {
		Debug.Log("Adding ip address to unique id! Here's the result: " + result.message); //I'm not interested in the callback.
	}

	//Update a VC room property in the database
	public void UpdateVCRoomProps(string roomName, string newOwnerID, int newNumMembers, bool newIsPublic)
    {
		string query = "execute UpdateVCRoomProps";

		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("roomName", roomName);
		parameters.SetValue("newOwnerID", newOwnerID);
		parameters.SetValue("newNumMembers", newNumMembers);
		parameters.SetValue("newIsPublic", newIsPublic);

		sql.Command(query, null, parameters, UpdateVCRoomPropsCallback);
	}
	private void UpdateVCRoomPropsCallback(bool ok, SQLResult result)
	{
		Debug.Log("Updating room info! Here's the result: " + result.message); //I'm not interested in the callback.
	}

	//a quick function I wrote, might be slightly more efficient
	public void UpdateVCRoomNumMembers(string roomName, int numMembers)
    {
		string query = "update VideoChatsAvailable set NumMembers = %newNumMembers% where RoomName = %roomName%";

		SQLParameter parameter = new SQLParameter();
		parameter.SetValue("newNumMembers", numMembers);
		parameter.SetValue("roomName", roomName);

		sql.Command(query, null, parameter, UpdateVCRoomNumMembersCallback);
	}
	private void UpdateVCRoomNumMembersCallback(bool ok, SQLResult result)
	{
		Debug.Log("Updating room numMembers! Here's the result: " + result.message); //I'm not interested in the callback.
	}

	public void UpdateVCRoomOwner(string roomName, string newOwnerID)
	{
		string query = "update VideoChatsAvailable set CurrOwnerID = %newCurrOwnerID% where RoomName = %roomName%";

		SQLParameter parameter = new SQLParameter();
		parameter.SetValue("newCurrOwnerID", newOwnerID);
		parameter.SetValue("roomName", roomName);

		sql.Command(query, null, parameter, UpdateVCRoomOwnerCallback);
	}
	private void UpdateVCRoomOwnerCallback(bool ok, SQLResult result)
	{
		Debug.Log("Updating room owner! Here's the result: " + result.message); //I'm not interested in the callback.
	}

	//Delete a VC room in the database
	public void DeleteVCRoom(string roomName)
    {
		string query = "execute DeleteVCRoom";

		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("roomName", roomName);

		sql.Command(query, null, parameters, DeleteVCRoomCallback);
	}
	private void DeleteVCRoomCallback(bool ok, SQLResult result)
	{
		Debug.Log("Deleting the selected VC Room!"); //I'm not interested in the callback.
	}

	//Delete every VC room in the database. If you are a user PLEASE DON'T ABUSE THIS; this is for internal only.
	public void DeleteALLVCRooms()
	{
		string query = "Delete from VideoChatsAvailable";

		sql.Command(query, null, DeleteAllVCRoomCallback);
	}
	private void DeleteAllVCRoomCallback(bool ok, SQLResult result)
	{
		Debug.Log("Deleting ALL VC Rooms!"); //I'm not interested in the callback.
	}
	#endregion

	private void OnDestroy()
    {
		sql.Close();
	}

}