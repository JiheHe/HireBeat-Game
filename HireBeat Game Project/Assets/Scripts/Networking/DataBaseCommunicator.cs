using UnityEngine;
using System;
using SQL4Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Collections.Concurrent;
using Photon.Pun;

public class DataBaseCommunicator : MonoBehaviour
{
	public SQL4Unity.Server.Protocol Protocol = SQL4Unity.Server.Protocol.TCP;
	public string Database = "HireBeatProjectDB";
	public string UserName = string.Empty;
	public string IpAddress = "54.229.65.122"; //"10.0.0.32"; is Local IP for testing. Updated to TCP //Change in EDITOR!!!

	public string myOwnIpAddress = string.Empty;
	public string myID = string.Empty;

	public bool secure = false;
	int Port = 19390; // Default Client TCP Port. Replaced
	internal string UUID = "6657add8-97b8-41ce-a8cd-b8b48b83d489"; // Default UUID. Replace with yours. I did
	SQLExecute sql = null;

	public static bool isOpen = false;

	VideoChatRoomSearch vcs; //where info will be published for video chat
	RoomSystemPanelScript rsps;

	//Starting here, gonna try to implement the datastructure.
	public class QueueItem
	{
		public string query;
		public Action<SQLResult> callback;
		public SQLParameter parameters = null;
		public SQLRow row = null;
	}

	public static DataBaseCommunicator instance = null;

	public BlockingCollection<QueueItem> queue = new BlockingCollection<QueueItem>();

	// Make it a Singleton
	/*void Awake()
	{
		if (instance == null)
		{
			instance = this;
			instance.initialise();
		}
		else Destroy(this);
	}*/

	//playfabcontroller will call this upon successfully logging in.
	public void InitializeDBC(string myID)
    {
		instance = this;
		instance.initialise(myID);
	}

	SQLResult result;

	bool busy = false;
	bool done = false;

	Action<SQLResult> callBack;

    void initialise(string myID)
	{
		sql = new SQLExecute(this);

		this.myID = myID;
		//myOwnIpAddress = GetLocalIPv4(); //Switch it back to public after testing admin server check
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

		//StartCoroutine(AssignVcsAndRsps()); //too early now, since dbc first then photon room login
		//vcs = UnityEngine.GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("VidCRoomSearch").GetComponent<VideoChatRoomSearch>();
		//rsps = UnityEngine.GameObject.FindGameObjectWithTag("PlayerHUD").transform.Find("PlayerRoomSystem").GetComponent<RoomSystemPanelScript>();

		sql.Connect(Protocol, IpAddress, Port, UUID, secure, false, UserName, ConnectCallback);
	}

	public IEnumerator AssignVcsAndRsps()
    {
		var playerHud = UnityEngine.GameObject.FindGameObjectWithTag("PlayerHUD");
		if (playerHud == null)
        {
			yield return null;
			StartCoroutine(AssignVcsAndRsps());
        }
		else
        {
			yield return null;
			vcs = playerHud.transform.Find("VidCRoomSearch").GetComponent<VideoChatRoomSearch>();
			rsps = playerHud.transform.Find("PlayerRoomSystem").GetComponent<RoomSystemPanelScript>();
		}
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

		StartCoroutine(processQueue()); //start scanning in inputs

		//adding ip addr to unique id to the table
		AddIPAddressToUniqueID(myOwnIpAddress, myID);

		//check if unique id exists in userdatastorage (i.e. has name ready)
		CheckIfUserHasRegistered(); //this function's callback is below

		/*if (PhotonConnector.isRoomCreator)
        {
			string roomID = PhotonNetwork.CurrentRoom.Name.Substring("USERROOM_".Length);
			int numPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
			UpdateCurrOwnerOfRoom(roomID, myID);
			UpdateNumPlayersInRoom(roomID, numPlayers);
			PhotonConnector.isRoomCreator = false; //avoids retrigger next room because PC is persistent.
		}

		StartCoroutine(processQueue()); //start scanning in inputs*/
	}
	
	void PostRegistrationCheckCallback(SQLResult result)
    {
		//Notice that add new user is only called after a name is ready.
		bool isRegistered = result.rowsAffected != 0;

		//Right now, you connect to dbc first, then join photon room after. So we know dbc is ready when we are in a photon room. so...
		//The update part below should only be called when you are in room. Fix later

		UnityEngine.GameObject.Find("PlayFabController").GetComponent<PlayFabController>().DBCCallbackOnCheckingRegistration(isRegistered);
	}

	IEnumerator processQueue()
	{
		if (!busy)
		{
			QueueItem qi;
			if (queue.TryTake(out qi))
			{
				DoQuery(qi.query, qi.callback, qi.parameters);
			}
		}
		yield return null;
		StartCoroutine(processQueue());
	}

	/*public static void Add(string query, SQLParameter parameters, Action<SQLResult> callback)
	{
		QueueItem qi = new QueueItem();
		qi.query = query;
		qi.parameters = parameters;
		qi.callback = callback;
		instance.queue.Add(qi);
	}*/

	// Call this to add a query to the queue for processing
	public static void Execute(string query, Action<SQL4Unity.SQLResult> callback)
	{
		QueueItem qi = new QueueItem();
		qi.query = query;
		qi.callback = callback;
		instance.queue.Add(qi);
	}
	public static void Execute(string query, Action<SQL4Unity.SQLResult> callback, SQL4Unity.SQLRow row)
	{
		QueueItem qi = new QueueItem();
		qi.query = query;
		qi.callback = callback;
		qi.row = row;
		instance.queue.Add(qi);
	}
	public static void Execute(string query, Action<SQL4Unity.SQLResult> callback, SQL4Unity.SQLParameter parameters)
	{
		QueueItem qi = new QueueItem();
		qi.query = query;
		qi.callback = callback;
		qi.parameters = parameters;
		instance.queue.Add(qi);
	}

	// This is what you call if you want to jump the queue
	public void DoQuery(string query, Action<SQL4Unity.SQLResult> callback)
	{
		StartCoroutine(StartQuery(query, callback));
	}
	public void DoQuery(string query, Action<SQL4Unity.SQLResult> callback, SQL4Unity.SQLRow row)
	{
		StartCoroutine(StartQuery(query, callback, row));
	}
	public void DoQuery(string query, Action<SQL4Unity.SQLResult> callback, SQL4Unity.SQLParameter parameters)
	{
		StartCoroutine(StartQuery(query, callback, null, parameters));
	}

	IEnumerator StartQuery(string query, Action<SQL4Unity.SQLResult> callback, SQL4Unity.SQLRow row = null, SQL4Unity.SQLParameter parameters = null)
	{
		yield return new WaitUntil(() => !busy);
		busy = true;
		done = false;
		callBack = callback;
		StartCoroutine(ExecuteCallback());
		if (row != null) sql.Command(query, null, row, SqlCallback);
		else if (parameters != null) sql.Command(query, null, parameters, SqlCallback);
		else sql.Command(query, null, SqlCallback);
	}

	IEnumerator ExecuteCallback()
	{
		yield return new WaitUntil(() => done);
		callBack(result);
		busy = false;
	}

	void SqlCallback(bool ok, SQL4Unity.SQLResult result)
	{
		if (ok)
		{
			this.result = result;
		}
		else
		{
			this.result = null;
			Debug.Log("Error " + result.message);
		}
		done = true;
	}
	// STOPS HERE, start the conversion now.

	/*void Start()
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
	}*/

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

	

	//int i = 0;
    private void Update()
    {
		if(Input.GetKeyDown(KeyCode.G))
		{
			//AddNewPlayer("YESSIR", "nope@cccs", true, 30);
			//StartCoroutine(AddActualTestPlayers());
			//gonna remove everything where id = a, for testing.
			//StartCoroutine(testAdd());
			/*CreateNewVCRoom("IDK" + i, "helo", true);
			i += 1;*/
		}
		if(Input.GetKeyDown(KeyCode.K))
        {
			//StartCoroutine(AddActualTestPlayers());
			//AddNewPlayerWithFullDetail("BOBSTER", "NOPSIR", "nope@cccs", true, 1);
		}
	}
	IEnumerator AddActualTestPlayers()
    {
		//data patching for tester accounts
		//hopefully in future I don't lose service to server all the sudden... else a pain in the ass... 
		//maybe prep a safe check? no registeration if cannot connect to dbc, or fix later?
		AddNewPlayer("7A98A976DE472605", "jaxmasterofleague@gmail.com");
		yield return new WaitForSeconds(1);
		AddNewPlayer("B5EF892E35CD7E86", "nickhe2003@gmail.com");
		yield return new WaitForSeconds(1);
		AddNewPlayer("FCC363B28E64818C", "nhe21siprep@gmail.com");
		yield return new WaitForSeconds(1);
		AddNewPlayer("1807DB258420A50A", "fenixking1994@gmail.com");
	}

	IEnumerator testAdd()
    {
		AddNewPlayerWithFullDetail("Tester1", "a", "1", true, 1);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("Tester2", "B", "2", true, 2);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("Tester7", "c", "3", true, 7);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("Tester6", "d", "Tester1", true, 6);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("Tester3", "e", "4", true, 3);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("Tester4", "f", "5", true, 4);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("Tester8", "g", "6", true, 8);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("Tester5", "h", "7", true, 5);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("AEG", "i", "8", true, 2);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("B", "j", "9", true, 13);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("ABC", "k", "10", true, 6);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("C", "l", "11", true, 3);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("DE", "m", "12", true, 4);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("Z", "Z", "13", true, 4);
		yield return new WaitForSeconds(1);
		AddNewPlayerWithFullDetail("KSK", "o", "14", true, 5);
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

		Execute(query, CreateNewVCRoomCallBack, parameters);
		//sql.Command(query, null, parameters, CreateNewVCRoomCallBack); //or can do if(sql.Command(query, result, parameters))
		//Debug.LogError("Creating a new room! Here's the result: " + result.message);
	}

	private void CreateNewVCRoomCallBack(SQLResult result)
    {
		Debug.Log("New VC Room created"); //I'm not interested in the callback. 
    }



	//SQLResult CheckVCRoomResult;
	//bool checkVCRoomResultReady;
	//this new method can help with room name check at create or find at search!
	public void CheckVCRoomExists(string roomName, string queuerName) 
    {
		// Yes on Main Thread
		//checkVCRoomResultReady = false;

		string query = "Select rowid from VideoChatsAvailable where RoomName = %roomName%";

		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("roomName", roomName);

		if (queuerName == "CreateRoomCheck")
		{
			Execute(query, vcs.OnCreateNewVCRoomSecondHalf, parameters);
		}
		else if (queuerName == "SearchRoomCheck")
		{
			Execute(query, vcs.SearchSpecificRoomSecondHalf, parameters);
		}

		//sql.Command(query, null, parameters, CheckVCRoomExistsCallback);

		/*StartCoroutine(ReturnIfVCRoomExists((doesExist) =>
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
		));// StartCoroutine on Main Thread */
	}

	/*void CheckVCRoomExistsCallback(bool ok, SQLResult result)
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
	}*/



	//SQLResult retrieveVCRoomInfoResult;
	//bool retrieveVCRoomInfoReady;
	//Retrieve one vc room info in the database
	public void RetrieveVCRoomInfo(string roomName, string queuerName)
    {
		//retrieveVCRoomInfoReady = false;

		string query = "execute RetrieveVCRoomInfo";

		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("roomName", roomName);

		if (queuerName == "SearchRoomCheck")
		{
			Execute(query, vcs.SearchSpecificRoomThirdHalf, parameters);
		}
		else if (queuerName == "ConnectRoomCheck")
		{
			Execute(query, vcs.OnConnectPressedSecondHalf, parameters);
		}

		/*sql.Command(query, null, parameters, RetrieveVCRoomInfoCallback); //no tis more. 

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
		));*/
	}

	/*void RetrieveVCRoomInfoCallback(bool ok, SQLResult result)
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
	}*/



	//This is a simple function for vCC to grab current room owner, we are sure that the room exists.
	//SQLResult retrieveVCRoomCurrentOwnerResult;
	//bool retrieveVCRoomCurrentOwnerReady;
	//Retrieve one vc room info in the database
	public void RetrieveVCRoomCurrentOwner(string roomName, string callerName) //this will only be used by vCC, so....
	{
		//retrieveVCRoomCurrentOwnerReady = false;

		string query = "Select CurrOwnerID from VideoChatsAvailable where RoomName = %roomName%";

		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("roomName", roomName);

		if (vcs.vCC != null)
		{
			if (callerName == "disconnect") Execute(query, vcs.vCC.OnDisconnectPressedSecondHalf, parameters);
			else if (callerName == "leaverCheck") Execute(query, vcs.vCC.OnLeaverCheckCallback, parameters);
		}
		//sql.Command(query, null, parameters, RetrieveVCRoomCurrentOwnerCallback); //no tis more. 
		//StartCoroutine(SendVCRoomOwnerInfo());
	}
	/*
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
	*/


	//SQLResult grabAllVCRoomInfoResult;
	//bool grabAllVCRoomInfoReady;
	//Grab all vc room infos in the database
	public void GrabAllVCRoomInfo(string queuerName)
    {
		//grabAllVCRoomInfoReady = false;

		string query = "execute GrabAllVCRoomInfo";

		switch (queuerName)
        {
			case "VCRoomListTotalUpdate":
				Execute(query, vcs.UpdateVCRoomList);
				break;
			case "VCRoomPublicity":
				Execute(query, vcs.SortVCRoomsByPublicitySecondHalf);
				break;
			case "VCRoomKeyword":
				Execute(query, vcs.ListVCRoomsWithKeywordSecondHalf);
				break;
			case "ShowInvitedRooms":
				Execute(query, vcs.ShowAllInvitedVCRoomSecondHalf);
				break;
		}

		/*sql.Command(query, null, GrabAllVCRoomInfoCallback);

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
		}));*/
	}
	/*
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
	}*/



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

	private void CheckIfUserHasRegistered()
    {
		string query = "SELECT * FROM UserDataStorage WHERE UserId = %userId%";
		SQLParameter parameter = new SQLParameter();
		parameter.SetValue("userId", myID);
		Execute(query, PostRegistrationCheckCallback, parameter);
	}

	public void AddIPAddressToUniqueID(string ipAddress, string uniqueID)
    {
		string query = "insert into IPAdressToUniqueID (IPAddress, UniqueID) values (%ipAddress%, %uniqueID%)";

		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("ipAddress", ipAddress);
		parameters.SetValue("uniqueID", uniqueID);

		Execute(query, AddIPAddressToUniqueIDCallback, parameters);
		//sql.Command(query, null, parameters, AddIPAddressToUniqueIDCallback); //or can do if(sql.Command(query, result, parameters))
	}
	private void AddIPAddressToUniqueIDCallback(SQLResult result)
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

		Execute(query, UpdateVCRoomPropsCallback, parameters);
		//sql.Command(query, null, parameters, UpdateVCRoomPropsCallback);
	}
	private void UpdateVCRoomPropsCallback(SQLResult result)
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

		Execute(query, UpdateVCRoomNumMembersCallback, parameter);
		//sql.Command(query, null, parameter, UpdateVCRoomNumMembersCallback);
	}
	private void UpdateVCRoomNumMembersCallback(SQLResult result)
	{
		Debug.Log("Updating room numMembers! Here's the result: " + result.message); //I'm not interested in the callback.
	}

	public void UpdateVCRoomOwner(string roomName, string newOwnerID)
	{
		string query = "update VideoChatsAvailable set CurrOwnerID = %newCurrOwnerID% where RoomName = %roomName%";

		SQLParameter parameter = new SQLParameter();
		parameter.SetValue("newCurrOwnerID", newOwnerID);
		parameter.SetValue("roomName", roomName);

		Execute(query, UpdateVCRoomOwnerCallback, parameter);
		//sql.Command(query, null, parameter, UpdateVCRoomOwnerCallback);
	}
	private void UpdateVCRoomOwnerCallback(SQLResult result)
	{
		Debug.Log("Updating room owner! Here's the result: " + result.message); //I'm not interested in the callback.
	}

	//Delete a VC room in the database
	public void DeleteVCRoom(string roomName)
    {
		string query = "execute DeleteVCRoom";

		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("roomName", roomName);

		Execute(query, DeleteVCRoomCallback, parameters);
		//sql.Command(query, null, parameters, DeleteVCRoomCallback);
	}
	private void DeleteVCRoomCallback(SQLResult result)
	{
		Debug.Log("Deleting the selected VC Room!"); //I'm not interested in the callback.
	}

    #endregion
    #endregion

    #region Room System
    /*SQLResult getUserIdResult;
	bool getUserIdResultReady;
	//we know that username / id / email are unique, but just in case username = someone's id... so will have user enter
	//to search by id or by name or by email.
	//Nvm the logic above; I thought of a good one, pasted in discord.
	public void GetUserIdFromInfo(string input, string from)
    {
		//A utilitarian function good for most search bars. 
		getUserIdResultReady = false;

		//Make sure that the OR statement links column in order of appearence! Name->Id->Email
		string query = "SELECT UserName, UserId, IsRoomPublic FROM UserDataStorage WHERE UserName = %input% OR UserId = %input% OR Email = %input%";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("input", input);

		sql.Command(query, null, parameters, GetUserIdFromInfoCallback);

		StartCoroutine(ReturnGetUserIdFromInfoResult((rows) =>
		{
			switch (from) {
				case "invplayer": //like an "or"
				case "rsps":
					if (rows == null) rsps.StoreInputSearchResults(rows, "doesntmatter", input);
					else rsps.StoreInputSearchResults(rows, from);
					break;
			}
			//just send rows back, and unity will process from that end.
			//if null then...
		}
		));// StartCoroutine on Main Thread
	}
	private void GetUserIdFromInfoCallback(bool ok, SQLResult result)
	{
		getUserIdResult = result;
		getUserIdResultReady = true;
	}
	IEnumerator ReturnGetUserIdFromInfoResult(Action<hirebeatprojectdb_userdatastorage[]> callback)
	{
		yield return new WaitUntil(() => getUserIdResultReady);

		// Back on Main Thread
		if (getUserIdResult.status)
		{
			try
			{
				if (getUserIdResult.rowsAffected == 0)
				{
					callback(null); //0 rows affected if nothing exists relating to the input.
				}
				else
				{
					hirebeatprojectdb_userdatastorage[] rows = getUserIdResult.Get<hirebeatprojectdb_userdatastorage>();
					callback(rows); //got something, regardless of its length.
				}
			}
			catch (Exception ex)
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.LogError(ex.Message);
			}
		}
		else
        {
			Debug.LogError("Retrieving user id with name/id/email encounters error");
			callback(null);
        }
	}*/

    //This function is an extension of the utility function above, only needed for rsps
    //Can overlap and use this for room invites probably
    /*SQLResult grabAllRoomInfoFromGivenIdsResult;
	bool grabAllRoomInfoFromGivenIdsReady;
	public void GrabAllRoomInfoFromGivenIds(string[] userIds, string cmd) //sort type doesn't matter, max 1-2 results. NOT ANYMORE
	{
		grabAllRoomInfoFromGivenIdsReady = false;

		string query;
		SQLParameter parameters = new SQLParameter();

		if (userIds.Length < 2) //1 if room search, but could be 0 if invite ids!
		{
			query = "SELECT TrueOwnerID, NumPlayersInRoom FROM UserRoomsAssociated WHERE TrueOwnerID = %userId%"; //only 1 element.
			if(userIds.Length == 1) parameters.SetValue("userId", userIds[0]);
			else parameters.SetValue("userId", ";;;,,,"); //no way this is in the database lol

			sql.Command(query, null, parameters, GrabAllRoomInfoFromGivenIdsCallback);
		}
		else //2 
		{
			query = "SELECT TrueOwnerID, NumPlayersInRoom FROM UserRoomsAssociated WHERE TrueOwnerID IN ({0})";

			string inClause = string.Join(",", userIds.Select(id => string.Concat("'", id, "'"))); //'id1','id2'... directly!
			query = string.Format(query, inClause); //replaces {0} with the list of paramNames

			sql.Command(query, null, GrabAllRoomInfoFromGivenIdsCallback);
		}

		StartCoroutine(GrabAllRoomInfoFromGivenIdsResult((rows) =>
		{
			rsps.DisplayInputSearchResults(rows, cmd);
		}
		));// StartCoroutine on Main Thread
	}
	private void GrabAllRoomInfoFromGivenIdsCallback(bool ok, SQLResult result)
	{
		grabAllRoomInfoFromGivenIdsResult = result;
		grabAllRoomInfoFromGivenIdsReady = true;
	}
	IEnumerator GrabAllRoomInfoFromGivenIdsResult(Action<hirebeatprojectdb_userroomsassociated[]> callback)
	{
		yield return new WaitUntil(() => grabAllRoomInfoFromGivenIdsReady);

		Debug.Log("Retrieving all room info from given ids info! Here's the result: " + grabAllRoomInfoFromGivenIdsResult.resultType.ToString() + " " +
			grabAllRoomInfoFromGivenIdsResult.status.ToString() + " " + grabAllRoomInfoFromGivenIdsResult.message);
		if (grabAllRoomInfoFromGivenIdsResult.status)
		{
			try
			{
				hirebeatprojectdb_userroomsassociated[] rows = grabAllRoomInfoFromGivenIdsResult.Get<hirebeatprojectdb_userroomsassociated>();
				callback(rows);
			}
			catch (Exception ex)
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.LogError("Grabbing player room info database failed: " + ex.Message);
				callback(null);
			}
		}
		else
		{
			Debug.LogError("Retrieving player room info failed!");
			callback(null);
		}
	}*/

    //This function is for grabbing user status by checking if that id is present in the ip address table.
    /*SQLResult grabAllUserStatusFromGivenIdsResult;
	bool grabAllUserStatusFromGivenIdsReady;
	public void GrabAllUserStatusFromGivenIds(string[] userIds) //sort type doesn't matter, max 1-2 results.
	{
		grabAllUserStatusFromGivenIdsReady = false;

		string query;
		SQLParameter parameters = new SQLParameter();

		if (userIds.Length < 2) //1 
		{
			query = "SELECT UniqueID FROM IPAdressToUniqueID WHERE UniqueID = %userId%"; //only 1 element.
			parameters.SetValue("userId", userIds[0]);

			sql.Command(query, null, parameters, GrabAllUserStatusFromGivenIdsCallback);
		}
		else //2 
		{
			query = "SELECT UniqueID FROM IPAdressToUniqueID WHERE UniqueID IN ({0})";

			string inClause = string.Join(",", userIds.Select(id => string.Concat("'", id, "'"))); //'id1','id2'... directly!
			query = string.Format(query, inClause); //replaces {0} with the list of paramNames

			sql.Command(query, null, GrabAllUserStatusFromGivenIdsCallback);
		}

		StartCoroutine(GrabAllUserStatusFromGivenIdsResult((rows) =>
		{
			rsps.DisplayUserStatusResults(rows);
		}
		));// StartCoroutine on Main Thread
	}
	private void GrabAllUserStatusFromGivenIdsCallback(bool ok, SQLResult result)
	{
		grabAllUserStatusFromGivenIdsResult = result;
		grabAllUserStatusFromGivenIdsReady = true;
	}
	IEnumerator GrabAllUserStatusFromGivenIdsResult(Action<List<string>> callback)
	{
		yield return new WaitUntil(() => grabAllUserStatusFromGivenIdsReady);

		Debug.Log("Retrieving all user status from given ids info! Here's the result: " + grabAllUserStatusFromGivenIdsResult.resultType.ToString() + " " +
			grabAllUserStatusFromGivenIdsResult.status.ToString() + " " + grabAllUserStatusFromGivenIdsResult.message);
		if (grabAllUserStatusFromGivenIdsResult.status)
		{
			try
			{
				List<string> rows = grabAllUserStatusFromGivenIdsResult.Get<hirebeatprojectdb_ipadresstouniqueid>().Select(r=>r.UniqueID).ToList();
				callback(rows); //hopefully if row is empty, then still wouldn't be error.
			}
			catch (Exception ex)
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.LogError("Grabbing user status database failed: " + ex.Message);
				callback(null);
			}
		}
		else
		{
			Debug.LogError("Retrieving user status info failed!");
			callback(null);
		}
	}*/

    //To avoid search spam, not gonna do the type & filter: just gonna do a collective search at the end.
    // The collective search targets the room entered EXACTLY (name, id, or email). => does not exist vs. room is private.

    //This part is for invites
    /*SQLResult grabAllInvitedRoomNameResult;
	bool grabAllInvitedRoomNameResultReady;
	public void GrabAllInvitedRoomNameFromGivenIds(List<string> userIds) //sort type doesn't matter, max 1-2 results. NOT ANYMORE
	{
		grabAllInvitedRoomNameResultReady = false;

		string query;
		SQLParameter parameters = new SQLParameter();

		if (userIds.Count < 2) //1 or 0
		{
			query = "SELECT UserName, UserId, IsRoomPublic FROM UserDataStorage WHERE UserId = %userId%"; //only 1 element.
			if (userIds.Count == 1) parameters.SetValue("userId", userIds[0]);
			else parameters.SetValue("userId", ";;;,,,"); //this is not possible, so let's see if it returns empty ;D

			sql.Command(query, null, parameters, GrabAllInvitedRoomNameFromGivenIdsCallback);
		}
		else //2 or more invites
		{
			query = "SELECT UserName, UserId, IsRoomPublic FROM UserDataStorage WHERE UserId IN ({0})";

			string inClause = string.Join(",", userIds.Select(id => string.Concat("'", id, "'"))); //'id1','id2'... directly!
			query = string.Format(query, inClause); //replaces {0} with the list of paramNames

			sql.Command(query, null, GrabAllInvitedRoomNameFromGivenIdsCallback);
		}

		StartCoroutine(GrabAllInvitedRoomNameFromGivenIdsResult((rows) =>
		{
			rsps.StoreInputSearchResults(rows, "chkinvts");
		}
		));// StartCoroutine on Main Thread
	}
	private void GrabAllInvitedRoomNameFromGivenIdsCallback(bool ok, SQLResult result)
    {
		grabAllInvitedRoomNameResult = result;
		grabAllInvitedRoomNameResultReady = true;
	}
	IEnumerator GrabAllInvitedRoomNameFromGivenIdsResult(Action<hirebeatprojectdb_userdatastorage[]> callback)
	{
		yield return new WaitUntil(() => grabAllInvitedRoomNameResultReady);

		Debug.Log("Retrieving invited room info from given ids! Here's the result: " + grabAllInvitedRoomNameResult.resultType.ToString() + " " +
			grabAllInvitedRoomNameResult.status.ToString() + " " + grabAllInvitedRoomNameResult.message);
		if (grabAllInvitedRoomNameResult.status)
		{
			try
			{
				hirebeatprojectdb_userdatastorage[] rows = grabAllInvitedRoomNameResult.Get<hirebeatprojectdb_userdatastorage>();
				callback(rows); //hopefully if row is empty, then still wouldn't be error.
			}
			catch (Exception ex)
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.LogError("Grabbing invited rooms database failed: " + ex.Message);
				callback(null);
			}
		}
		else
		{
			Debug.LogError("Retrieving invited room info failed!");
			callback(null);
		}
	}
	*/
    //invites


    /*SQLResult ChangeUserNameResult;
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
	}*/


    //bool addNewPlayerResultReady;
    //This is called on player registeration, where a unique account name should already be ready, as well as userEmail for record keeping
    //Not gonna add a UserName.. want the default to be empty for us to immediately set after ;D
    public void AddNewPlayer(string userId, string userEmail, bool roomPublic = false, int numPlayersInRm = 1)
	{
		//addNewPlayerResultReady = false;

		string infoQuery = "INSERT INTO UserDataStorage (UserName,UserId,Email,IsRoomPublic) VALUES (%userName%, %userId%, %email%, %rmPub%)";
		SQLParameter infoParameters = new SQLParameter();
		infoParameters.SetValue("userName", ",,,...;;;" + userId); //,,,...;;; is a special syntax, doesn't matter XD just a placeholder 
		infoParameters.SetValue("userId", userId);
		infoParameters.SetValue("email", userEmail);
		infoParameters.SetValue("rmPub", roomPublic);

		Execute(infoQuery, AddNewPlayerCallback, infoParameters);
		//sql.Command(infoQuery, null, infoParameters, AddNewPlayerCallback);

		//StartCoroutine(AddNewPlayerSecondPart(userId, numPlayersInRm));

		string roomQuery = "INSERT INTO UserRoomsAssociated (TrueOwnerID,NumPlayersInRoom,CurrOwnerID) " +
				"VALUES (%userId%, %numPInRm%, %currOwnerId%)";
		SQLParameter roomParameters = new SQLParameter();
		roomParameters.SetValue("userId", userId);
		roomParameters.SetValue("numPInRm", numPlayersInRm);
		roomParameters.SetValue("currOwnerId", userId);

		Execute(roomQuery, AddNewPlayerCallback, roomParameters);
	}
	private void AddNewPlayerCallback(SQLResult result)
    {
		//addNewPlayerResultReady = true;
		Debug.Log("New player registration (info or room, twice)! Adding in data: " + result.message.ToString());
		//Don't forget to trigger a "enter new name plz" for new players right after!
    }
	/*IEnumerator AddNewPlayerSecondPart(string userId, int numPlayersInRm)
	{
		yield return new WaitUntil(() => addNewPlayerResultReady);

		try
		{
			string roomQuery = "INSERT INTO UserRoomsAssociated (TrueOwnerID,NumPlayersInRoom,CurrOwnerID) " +
				"VALUES (%userId%, %numPInRm%, %currOwnerId%)";
			SQLParameter roomParameters = new SQLParameter();
			roomParameters.SetValue("userId", userId);
			roomParameters.SetValue("numPInRm", numPlayersInRm);
			roomParameters.SetValue("currOwnerId", userId);

			sql.Command(roomQuery, null, roomParameters, AddNewPlayerCallback);
		}
		catch (Exception ex)
		{
			// May throw an Illegal Cast Exception if the local database is missing
			Debug.LogError(ex.Message);
		}
	}*/
	//This is a variation of the above, but for internal testing only
	public void AddNewPlayerWithFullDetail(string userName, string userId, string userEmail, bool roomPublic = false, int numPlayersInRm = 1)
	{
		//addNewPlayerResultReady = false;

		string infoQuery = "INSERT INTO UserDataStorage (UserName,UserId,Email,IsRoomPublic) VALUES (%userName%, %userId%, %email%, %rmPub%)";
		SQLParameter infoParameters = new SQLParameter();
		infoParameters.SetValue("userName", userName); 
		infoParameters.SetValue("userId", userId);
		infoParameters.SetValue("email", userEmail);
		infoParameters.SetValue("rmPub", roomPublic);

		Execute(infoQuery, AddNewPlayerCallback, infoParameters);

		string roomQuery = "INSERT INTO UserRoomsAssociated (TrueOwnerID,NumPlayersInRoom,CurrOwnerID) " +
				"VALUES (%userId%, %numPInRm%, %currOwnerId%)";
		SQLParameter roomParameters = new SQLParameter();
		roomParameters.SetValue("userId", userId);
		roomParameters.SetValue("numPInRm", numPlayersInRm);
		roomParameters.SetValue("currOwnerId", userId);

		Execute(roomQuery, AddNewPlayerCallback, roomParameters);

		//sql.Command(infoQuery, null, infoParameters, AddNewPlayerCallback);

		//StartCoroutine(AddNewPlayerSecondPart(userId, numPlayersInRm));
	}

	/// <summary>
	/// Not ready grab usernames anymore... grabbing room info actually. Flipped cuz I moved room public to userdatastorage.
	/// </summary>
	/*SQLResult grabAllUsernamesFromGivenIdsResult;
	bool grabAllUsernamesFromGivenIdsReady;
	public void GrabAllUsernamesFromGivenIds(string[] userIds, int sortType)
    {
		grabAllUsernamesFromGivenIdsReady = false;

		string query;
		SQLParameter parameters = new SQLParameter();

		if (userIds.Length < 2) //1 or 0, rare case.
		{
			query = "SELECT TrueOwnerID, NumPlayersInRoom FROM UserRoomsAssociated WHERE TrueOwnerID = %userId%"; //only 1 element.
			if (userIds.Length == 1) parameters.SetValue("userId", userIds[0]); 
			else parameters.SetValue("userId", ",,,"); //,,, is not possible, meaning the result will be empty.

			sql.Command(query, null, parameters, GrabAllUsernamesFromGivenIdsCallback);
		}
		else
		{
			if (sortType == 1) //sort by num
				query = "SELECT TrueOwnerID, NumPlayersInRoom FROM UserRoomsAssociated WHERE TrueOwnerID IN ({0}) ORDER BY NumPlayersInRoom"; //sort num here!
			else
				query = "SELECT TrueOwnerID, NumPlayersInRoom FROM UserRoomsAssociated WHERE TrueOwnerID IN ({0})";


			/*string[] paramNames = userIds.Select(
				(s, i) => "@id" + i.ToString() //string, index 
			).ToArray(); //this assigns each tag an index based on userIds length

			string inClause = string.Join(", ", paramNames); //@id0, @id1, @id2... format
			query = string.Format(query, inClause); //replaces {0} with the list of paramNames

			for (int i = 0; i < paramNames.Length; i++)
			{
				parameters.SetValue(paramNames[i], userIds[i]); //@id0 = firstId, etc...
			}*/

			//Might be simpler? no need for parameters.
			/*string inClause = string.Join(",", userIds.Select(id=> string.Concat("'", id, "'"))); //'id1','id2'... directly!
			query = string.Format(query, inClause); //replaces {0} with the list of paramNames

			sql.Command(query, null, GrabAllUsernamesFromGivenIdsCallback);
		}

		//sql.Command(query, null, parameters, GrabAllUsernamesFromGivenIdsCallback);

		StartCoroutine(GrabAllUsernamesFromGivenIdsResult((rows) =>
		{
			Dictionary<string, int> userIdToNumPlayersInRm;
			//Do something with the rows. If sort by num, then reverse the results so it goes from max->min
			if (sortType == 1) userIdToNumPlayersInRm = rows.Reverse().ToDictionary(r => r.TrueOwnerID, r => r.NumPlayersInRoom);
			else userIdToNumPlayersInRm = rows.ToDictionary(r => r.TrueOwnerID, r => r.NumPlayersInRoom);
			rsps.UpdatePlayerRoomList(userIdToNumPlayersInRm, sortType);
		}
		));// StartCoroutine on Main Thread
	}*/
	/*private void GrabAllUsernamesFromGivenIdsCallback(bool ok, SQLResult result)
    {
		grabAllUsernamesFromGivenIdsResult = result;
		grabAllUsernamesFromGivenIdsReady = true;
	}
	IEnumerator GrabAllUsernamesFromGivenIdsResult(Action<hirebeatprojectdb_userroomsassociated[]> callback)
	{
		yield return new WaitUntil(() => grabAllUsernamesFromGivenIdsReady);

		Debug.Log("Retrieving all usernames from given ids info! Here's the result: " + grabAllUsernamesFromGivenIdsResult.resultType.ToString() + " " +
			grabAllUsernamesFromGivenIdsResult.status.ToString() + " " + grabAllUsernamesFromGivenIdsResult.message);
		if (grabAllUsernamesFromGivenIdsResult.status)
		{
			try
			{
				hirebeatprojectdb_userroomsassociated[] rows = grabAllUsernamesFromGivenIdsResult.Get<hirebeatprojectdb_userroomsassociated>();
				callback(rows);
			}
			catch (Exception ex)
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.LogError("Grabbing player room info database failed: " + ex.Message);
				callback(null);
			}
		}
		else
		{
			Debug.LogError("Retrieving player room info failed!");
			callback(null);
		}
	}*/


	//SQLResult grabAllPublicRoomsResult;
	//bool grabAllPublicRoomsResultReady;
	public void GrabAllPublicRooms(int sortType, Action<SQLResult> callback) //0 is normal, 1 is numCount, 2 is alphabet
    {
		//grabAllPublicRoomsResultReady = false;

		string query = "";
		if(sortType == 0 || sortType == 3 || sortType == 1) 
			query = "SELECT UserName, UserId FROM UserDataStorage WHERE IsRoomPublic = true"; //public-only for now
		else if(sortType == 2) 
			query = "SELECT UserName, UserId FROM UserDataStorage WHERE IsRoomPublic = true ORDER BY UserName";
		
		//sql.Command(query, null, GrabAllPublicRoomsCallback);
		Execute(query, callback);

		/*StartCoroutine(ReturnAllPublicRoomsResult((rows) =>
		{
			//Do something with the rows.
			rsps.GrabAllPublicRoomInfo(rows, sortType);
		}
		));// StartCoroutine on Main Thread
		*/
	}
	/*private void GrabAllPublicRoomsCallback(bool ok, SQLResult result)
    {
		grabAllPublicRoomsResult = result;
		grabAllPublicRoomsResultReady = true;
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
				Debug.LogError("Grabbing player info database failed: " + ex.Message);
				callback(null);
			}
		}
		else
		{
			Debug.LogError("Retrieving ALL player info failed!");
			callback(null);
		}
	}*/



	//This simply changes room's public status. Can only be called from the official owner.
	public void ChangeRoomPublicStatus(string userId, bool isPublic) //we know id is 100% unique.
    {
		string query = "UPDATE UserDataStorage SET IsRoomPublic = %isPublic% WHERE UserId = %userId%";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("isPublic", isPublic); 
		parameters.SetValue("userId", userId);

		Execute(query, ChangeRoomPublicStatusCallback, parameters);
		//sql.Command(query, null, parameters, ChangeRoomPublicStatusCallback);
	}
	private void ChangeRoomPublicStatusCallback(SQLResult result)
    {
		Debug.Log("Changed room status! Result: " + result.message.ToString());
	}

	//This grabs own room's public status and owner name, so on login/room switch etc user can get correct feedback on room status.
	//Can also use this to grab the public status of the current room you are in, using the id.
	//SQLResult getCurrentRoomInfoResult;
	//bool getCurrentRoomInfoResultReady;
	public void GetCurrentRoomInfo(string trueOwnerID, string purpose)
    {
		//getCurrentRoomInfoResultReady = false;
		string query = "SELECT UserName, IsRoomPublic FROM UserDataStorage WHERE UserId = %userId%";
		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("userId", trueOwnerID);

		switch (purpose)
		{
			case "CurrentRoomInfo":
				Execute(query, rsps.SetCurrentRoomInfoTexts, parameters);
				break;
			case "SettingsPublicCheck":
				Execute(query, rsps.UpdateSelfRoomPublicStatusFromDB, parameters);
				break;
		}
		/*sql.Command(query, null, parameters, GetGetCurrentRoomInfoCallback);

		StartCoroutine(ReturnGetCurrentRoomInfoResult((roomInfo) =>
		{
			switch (purpose)
            {
				case "CurrentRoomInfo":
					rsps.SetCurrentRoomInfoTexts(roomInfo);
					break;
				case "SettingsPublicCheck":
					rsps.UpdateSelfRoomPublicStatusFromDB(roomInfo.IsRoomPublic);
					break;
			}

		}
		));// StartCoroutine on Main Thread*/
	}
	/*private void GetGetCurrentRoomInfoCallback(bool ok, SQLResult result)
    {
		getCurrentRoomInfoResult = result;
		getCurrentRoomInfoResultReady = true;
	}
	IEnumerator ReturnGetCurrentRoomInfoResult(Action<hirebeatprojectdb_userdatastorage> callback)
	{
		yield return new WaitUntil(() => getCurrentRoomInfoResultReady);

		if (getCurrentRoomInfoResult.status)
		{
			try
			{
				hirebeatprojectdb_userdatastorage roomInfo = getCurrentRoomInfoResult.Get<hirebeatprojectdb_userdatastorage>()[0];
				callback(roomInfo);
				//grabAllPublicRoomsResult.Clear();
				//grabAllPublicRoomsResultReady = false;
			}
			catch (Exception ex)
			{
				// May throw an Illegal Cast Exception if the local database is missing
				Debug.LogError("Grabbing current room info database failed: " + ex.Message);
				callback(null);
			}
		}
		else
		{
			Debug.LogError("Retrieving current room info failed!");
			callback(null);
		}
	}*/

	//This simply updates numPlayersInRoom (based off of photon room count, simple) //can access with PhotonCurrentRoom.roomName etc not anymore
	//Can access id through the "currentRoomTrueOwnerID" in rsps! Which is stored when connect button is pressed.
	public static void UpdateNumPlayersInRoom(string roomOwnerId, int numPlayers, Action<SQL4Unity.SQLResult> callback = null) //hopefully our effort has ensured name's uniqueness.
    {
		string query = "UPDATE UserRoomsAssociated SET NumPlayersInRoom = %numPlayers% WHERE TrueOwnerID = %userId%";
		SQLParameter parameters = new SQLParameter();

		parameters.SetValue("numPlayers", numPlayers); 
		parameters.SetValue("userId", roomOwnerId);

		if(callback == null) Execute(query, UpdateNumPlayersInRoomCallback, parameters);
		else Execute(query, callback, parameters);
		//sql.Command(query, null, parameters, UpdateNumPlayersInRoomCallback);
	}
	private static void UpdateNumPlayersInRoomCallback(SQLResult result)
    {
		Debug.Log("Update Num players in current room! Result: " + result.message.ToString());
	}

	public static void UpdateCurrOwnerOfRoom(string trueOwnerId, string newCurrOwnerId)
    {
		string query = "UPDATE UserRoomsAssociated SET CurrOwnerID = %newCurrOwnerID% WHERE TrueOwnerID = %userId%";
		SQLParameter parameters = new SQLParameter();
		parameters.SetValue("newCurrOwnerID", newCurrOwnerId);
		parameters.SetValue("userId", trueOwnerId);

		Execute(query, UpdateCurrOwnerOfRoomCallback, parameters);
    }
	private static void UpdateCurrOwnerOfRoomCallback(SQLResult result)
	{
		Debug.Log("Update new curr owner of curr room! Result: " + result.message.ToString());
	}

	#endregion

	private void OnDestroy()
    {
		sql.Close();
	}

}