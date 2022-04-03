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
	RoomSystemPanelScript rsps;

	void Start()
	{
		myID = UnityEngine.GameObject.Find("PlayFabController").GetComponent<PlayFabController>().myID; //playfab user id as the username
		myOwnIpAddress = GetPublicIpAddress(); //this is the ip address that the user connects to the server with.

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
		rsps = UnityEngine.GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("PlayerRoomSystem").GetComponent<RoomSystemPanelScript>();
	}

	public string GetLocalIPv4() //this returns user's current ip address!
	{
		return Dns.GetHostEntry(Dns.GetHostName())
			.AddressList.First(
				f => f.AddressFamily == AddressFamily.InterNetwork)
			.ToString();
	}

	public string GetPublicIpAddress()
    {
		//string url = "http://checkip.dyndns.org";
		string url = "https://api.ipify.org?format=json";
		WebRequest req = WebRequest.Create(url);
		WebResponse resp = req.GetResponse();
		System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
		string response = sr.ReadToEnd().Trim();
		string[] ipAddressWithText = response.Split(':');
		string mainIP = ipAddressWithText[1].Substring(1, ipAddressWithText[1].Length - 3); //remove " and "}
		/*string ipAddressWithHTMLEnd = ipAddressWithText[1].Substring(1);
		string[] ipAddress = ipAddressWithHTMLEnd.Split('<');
		string mainIP = ipAddress[0];*/
		Debug.Log("My public ip address is: " + mainIP);
		return mainIP;
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

		//The comments below are for testing:
	}

	//int i = 0;
    private void Update()
    {
		if(Input.GetKeyDown(KeyCode.G))
		{
			/*AddNewPlayer("7A98A976DE472605", "jaxmasterofleague@gmail.com");
			AddNewPlayer("B5EF892E35CD7E86", "nickhe2003@gmail.com");
			AddNewPlayer("FCC363B28E64818C", "nhe21siprep@gmail.com");
			AddNewPlayer("1807DB258420A50A", "fenixking1994@gmail.com");*/ //data patching for tester accounts
																		   //hopefully in future I don't lose service to server all the sudden... else a pain in the ass... 
																		   //maybe prep a safe check? no registeration if cannot connect to dbc, or fix later?

			//gonna remove everything where id = a, for testing.
			//StartCoroutine(testAdd());
			/*CreateNewVCRoom("IDK" + i, "helo", true);
			i += 1;*/
		}
    }

	IEnumerator testAdd()
    {
		AddNewPlayerWithFullDetail("Tester1", "a", "c", true, 1);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("Tester2", "a", "c", true, 2);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("Tester7", "a", "c", true, 7);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("Tester6", "a", "c", true, 6);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("Tester3", "a", "c", true, 3);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("Tester4", "a", "c", true, 4);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("Tester8", "a", "c", true, 8);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("Tester5", "a", "c", true, 5);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("AEG", "a", "c", true, 2);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("B", "a", "c", true, 13);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("ABC", "a", "c", true, 6);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("C", "a", "c", true, 3);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("DE", "a", "c", true, 4);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("Z", "a", "c", true, 4);
		yield return new WaitForSeconds(2);
		AddNewPlayerWithFullDetail("KSK", "a", "c", true, 5);
	}


    #region Video Chat System
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
			else if(queuerName == "ShowInvitedRooms")
            {
				vcs.ShowAllInvitedVCRoomSecondHalf(rows);
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
	#endregion

	SQLResult getUserIdResult;
	bool getUserIdResultReady;
	//we know that username / id / email are unique, but just in case username = someone's id... so will have user enter
	//to search by id or by name or by email.
	//Nvm the logic above; I thought of a good one, pasted in discord.
	public void GetUserIdFromInfo(string input)
    {

    }

	SQLResult ChangeUserNameResult;
	bool changeUserNameResultReady;
	//Since username is primary key, I assume it would be a good double-safe check.
	//Also should call this after user registeratoin, in a name entering box.
	public void ChangeUserName(string userId, string newUserName, ContentChangerScript nameChangerObj) 
    {
		changeUserNameResultReady = false;

		string query = "UPDATE UserDataStorage SET UserName = %newUserName% WHERE UserId = %userId%";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("newUserName", newUserName);
		parameters.SetValue("userId", userId);

		sql.Command(query, null, parameters, ChangeUserNameCallback); //or can do if(sql.Command(query, result, parameters))

		StartCoroutine(ReturnChangeUserNameResult((wentThru) =>
		{
			nameChangerObj.ChangeUserNameResultCallback(wentThru);
		}
		));// StartCoroutine on Main Thread
	}
	private void ChangeUserNameCallback(bool ok, SQLResult result)
    {
		ChangeUserNameResult = result;
		changeUserNameResultReady = true;
    }
	IEnumerator ReturnChangeUserNameResult(Action<bool> callback)
	{
		yield return new WaitUntil(() => changeUserNameResultReady);

		// Back on Main Thread
		if (ChangeUserNameResult.status)
		{
			try
			{
				if (ChangeUserNameResult.rowsAffected == 0)
				{
					callback(false); //0 rows affected if name already exists
				}
				else
				{
					callback(true); //went through! A unique username indeed.
				}
			}
			catch (Exception ex)
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.LogError(ex.Message);
			}
		}
	}


	//This is called on player registeration, where a unique account name should already be ready, as well as userEmail for record keeping
	//Not gonna add a UserName.. want the default to be empty for us to immediately set after ;D
	public void AddNewPlayer(string userId, string userEmail, bool roomPublic = false, int numPlayersInRm = 1)
	{
		string query = "INSERT INTO UserDataStorage (UserName,UserId,Email,IsRoomPublic,NumPlayersInRoom) VALUES (%userName%, %userId%, %email%, %isRmPublic%, %numPInRm%)";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("userName", ",,,...;;;" + userId); //,,,...;;; is a special syntax, doesn't matter XD just a placeholder 
		parameters.SetValue("userId", userId);
		parameters.SetValue("email", userEmail);
		parameters.SetValue("isRmPublic", roomPublic);
		parameters.SetValue("numPInRm", numPlayersInRm);

		sql.Command(query, null, parameters, AddNewPlayerCallback); 
	}
	private void AddNewPlayerCallback(bool ok, SQLResult result)
    {
		Debug.Log("New player registration! Adding in data: " + result.message.ToString());
		//Don't forget to trigger a "enter new name plz" for new players right after!
    }
	//This is a variation of the above, but for internal testing only
	public void AddNewPlayerWithFullDetail(string userName, string userId, string userEmail, bool roomPublic = false, int numPlayersInRm = 1)
	{
		string query = "INSERT INTO UserDataStorage (UserName,UserId,Email,IsRoomPublic,NumPlayersInRoom) VALUES (%userName%, %userId%, %email%, %isRmPublic%, %numPInRm%)";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("userName", userName); //,,,...;;; is a special syntax, doesn't matter XD just a placeholder 
		parameters.SetValue("userId", userId);
		parameters.SetValue("email", userEmail);
		parameters.SetValue("isRmPublic", roomPublic);
		parameters.SetValue("numPInRm", numPlayersInRm);

		sql.Command(query, null, parameters, AddNewPlayerCallback);
	}


	SQLResult grabAllPublicRoomsResult;
	bool grabAllPublicRoomsResultReady;
	public void GrabAllPublicRooms(int sortType) //0 is normal, 1 is numCount, 2 is alphabet
    {
		grabAllPublicRoomsResultReady = false;

		string query = "";
		if(sortType == 0 || sortType == 3) 
			query = "SELECT UserName, UserId, NumPlayersInRoom FROM UserDataStorage WHERE IsRoomPublic = true"; //public-only for now
		else if(sortType == 1) 
			query = "SELECT UserName, UserId, NumPlayersInRoom FROM UserDataStorage WHERE IsRoomPublic = true ORDER BY NumPlayersInRoom";
		else if(sortType == 2)
			query = "SELECT UserName, UserId, NumPlayersInRoom FROM UserDataStorage WHERE IsRoomPublic = true ORDER BY UserName";

		sql.Command(query, null, GrabAllPublicRoomsCallback);

		StartCoroutine(ReturnAllPublicRoomsResult((rows) =>
		{
			//Do something with the rows.

			switch(sortType)
            {
				case 0:
					rsps.UpdatePlayerRoomList(rows, 0);
					break;
				case 1:
					rsps.UpdatePlayerRoomList(rows, 1);
					break;
				case 2:
					rsps.UpdatePlayerRoomList(rows, 2);
					break;
				case 3:
					rsps.UpdatePlayerRoomList(rows, 3);
					break;
			}
			
		}
		));// StartCoroutine on Main Thread
	}
	private void GrabAllPublicRoomsCallback(bool ok, SQLResult result)
    {
		grabAllPublicRoomsResultReady = true;
		grabAllPublicRoomsResult = result;
    }
	IEnumerator ReturnAllPublicRoomsResult(Action<hirebeatprojectdb_userdatastorage[]> callback)
	{
		yield return new WaitUntil(() => grabAllPublicRoomsResultReady);

		Debug.Log("Retrieving all room info! Here's the result: " + grabAllPublicRoomsResult.resultType.ToString() + " " +
			grabAllPublicRoomsResult.status.ToString() + " " + grabAllPublicRoomsResult.message);
		if (grabAllPublicRoomsResult.status)
		{
			try
			{
				hirebeatprojectdb_userdatastorage[] rows = grabAllPublicRoomsResult.Get<hirebeatprojectdb_userdatastorage>();
				callback(rows);
				//grabAllPublicRoomsResult.Clear();
				//grabAllPublicRoomsResultReady = false;
			}
			catch (Exception ex)
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.LogError("Grabbing player room database failed: " + ex.Message);
				callback(null);
			}
		}
		else
		{
			Debug.LogError("Retrieving ALL player room info failed!");
			callback(null);
		}
	}



	//This simply changes room's public status. Can only be called from the official owner.
	public void ChangeRoomPublicStatus(string userId, bool isPublic) //we know id is 100% unique.
    {
		string query = "UPDATE UserDataStorage SET IsRoomPublic = %isPublic% WHERE UserId = %userId%";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("isPublic", isPublic); //,,,...;;; is a special syntax, doesn't matter XD just a placeholder 
		parameters.SetValue("userId", userId);

		sql.Command(query, null, parameters, ChangeRoomPublicStatusCallback);
	}
	private void ChangeRoomPublicStatusCallback(bool ok, SQLResult result)
    {
		Debug.Log("Changed room status! Result: " + result.message.ToString());
	}

	//This simply updates numPlayersInRoom (based off of photon room count, simple) //can access with PhotonCurrentRoom.roomName etc
	public void UpdateNumPlayersInRoom(string roomOwnerName, int numPlayers) //hopefully our effort has ensured name's uniqueness.
    {
		string query = "UPDATE UserDataStorage SET NumPlayersInRoom = %numPlayers% WHERE UserName = %roomOwnerName%";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("numPlayers", numPlayers); //,,,...;;; is a special syntax, doesn't matter XD just a placeholder 
		parameters.SetValue("roomOwnerName", roomOwnerName);

		sql.Command(query, null, parameters, UpdateNumPlayersInRoomCallback);
	}
	private void UpdateNumPlayersInRoomCallback(bool ok, SQLResult result)
    {
		Debug.Log("Update Num players in current room! Result: " + result.message.ToString());
	}

	private void OnDestroy()
    {
		sql.Close();
	}

}