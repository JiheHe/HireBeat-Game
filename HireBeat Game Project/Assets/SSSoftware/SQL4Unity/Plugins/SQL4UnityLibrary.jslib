// *
// * This File and its contents are Copyright SteveSmith.Software 2021. All rights reserverd
// * 
// * No part may be copied, modified or stored in a public repository
// * 
// * This code may only be distributed in compiled form as part of a Compiled Unity Project
// * 
// * Library Functions for WebSocket support when targeting WebGL.
// * These functions are part of the SQL4Unity Client-Server database system.
// * Refer to the SQL4Unity documentation at https://stevesmith.software for more information
// */

var SQL4UnityLibrary = {

	// Library Data
	$S4U_data: {
		client : null,			// Websocket
		secure : false,         	// If True use WSS protocol
		url : '127.0.0.1',		// URL of Server
		port : 19191,			// Port of Server
		go : null,			// Name of Gameobject to receive messages
		onmessage : null,		// Method to call to receive new mesages
		onopen : null,			// Method to call after Open
		onclose : null,			// Method to call after Close
		onerror : null,			// Method to call if Error
		message : null,			// Message Received (byte[])
		hasmessage : false,		// True when new message received
		haserror : false,		// True when an error occured
		ispolling : false,		// If True messages will be picked up manually
		isconnected : false,		// Set to True when connected to Server
		sendmsg : false,		// If True send message on arrival.
		trace : false,			// If True Log actions to console
		CHUNK_SZ : 0x8000		// Constant for splitting up large byte[] in Base64 conversion
	},

	// Initialise Callback Method Data
	S4U_init : function(go,onmessage,onopen,onclose, onerror) {
		//_S4U_log('init');
		 S4U_data.go = Pointer_stringify(go);
		 if (onmessage != undefined) { S4U_data.onmessage = Pointer_stringify(onmessage); }
		 if (onopen != undefined) { S4U_data.onopen = Pointer_stringify(onopen); }
		 if (onclose != undefined) { S4U_data.onclose = Pointer_stringify(onclose); }
		 if (onerror != undefined) { S4U_data.onerror = Pointer_stringify(onerror); }
	},

	// Open the WebSocket
	S4U_open : function(url, port, secure, ispolling, sendmsg) {
		//_S4U_log('open');
		 S4U_data.url = Pointer_stringify(url);
		 if (port != undefined) { S4U_data.port = port; }
		 if (secure != undefined) { S4U_data.secure = secure; }
		 if (ispolling != undefined) { S4U_data.ispolling = ispolling; }
		 if (sendmsg != undefined) { S4U_data.sendmsg = sendmsg; }

        var ws = 'ws';
        if (S4U_data.secure==true) { ws += 's'; }
        var uri = ws + '://' + S4U_data.url + ':' + S4U_data.port + '/';

        var client = new WebSocket(uri, ['sql4unity']);
        client.onopen = function () { _S4U_callonconnected() };
        client.onclose = function () { _S4U_callondisconnected() };
        client.onmessage = function (event) { _S4U_callonmessage(event) };
        client.onerror = function (event) { _S4U_callonerror(event) };
		S4U_data.client = client;
	},

	// Close the WebSocket
	S4U_close : function() {
		//_S4U_log('close');
		if (S4U_data.client != null) {
			S4U_data.client.close();
		}
	},

	// Websocket Connected
	S4U_callonconnected : function() {
		//_S4U_log('callonconnected');
		S4U_data.isconnected = true;
		if (S4U_data.onopen != null) {
			_S4U_sendmessage(S4U_data.onopen);
		}
	},

	// Websocket Disconnected
	S4U_callondisconnected : function() {
		//_S4U_log('callondisconnected');
		S4U_data.isconnected = false;
		if (S4U_data.onclose != null) {
			_S4U_sendmessage(S4U_data.onclose);
		}
	},

	// Websocket Received new Message
	S4U_callonmessage : function(msg) {
		//_S4U_log('callonmessage '+Object.prototype.toString.call(msg.data));
		var reader = new FileReader();
		reader.addEventListener('loadend', function() {
			var ba = new Uint8Array(reader.result);
			if (!S4U_data.ispolling) {
				if (S4U_data.sendmsg) { 
					_S4U_sendmessage(S4U_data.onmessage, _S4U_bytetobase64(ba));
					return;
				}
				_S4U_sendmessage(S4U_data.onmessage);
			}
			S4U_data.hasmessage = true;
			S4U_data.message = ba;
		});
		reader.readAsArrayBuffer(msg.data);

	},

	// Websocket Error
	S4U_callonerror : function(msg) {
		//_S4U_log('SQL4Unity callonerror '+ msg.data, true);
		S4U_data.haserror = true;
		if (S4U_data.onerror != null) {
			_S4U_sendmessage(S4U_data.onerror, msg.data);
		}
	},

	// Set Callback GameObject
	S4U_gameobject : function(str) {
		 S4U_data.go = Pointer_stringify(str);
		//_S4U_log('gameobject '+ S4U_data.go);
	},

	// Set Callback method for message received
	S4U_onmessage : function(str) {
		 S4U_data.onmessage = Pointer_stringify(str);
		//_S4U_log('onmessage '+ S4U_data.onmessage);
	},

	// Set Callback method for Open
	S4U_onopen : function(str) {
		 S4U_data.onopen = Pointer_stringify(str);
		//_S4U_log('onopen '+ S4U_data.onopen);
	},

	// Set Callback method for Close
	S4U_onclose : function(str) {
		 S4U_data.onclose = Pointer_stringify(str);
		//_S4U_log('onclose '+ S4U_data.onclose);
	},

	// Set Callback method for Error
	S4U_onerror : function(str) {
		 S4U_data.onerror = Pointer_stringify(str);
		//_S4U_log('onerror '+ S4U_data.onerror);
	},

	// Check if message waiting to be picked up
	S4U_hasmessage : function() {
		//_S4U_log('hasmessage '+ S4U_data.hasmessage);
		return S4U_data.hasmessage;
	},

	// Check if error occured
	S4U_haserror : function() {
		//_S4U_log('haserror '+ S4U_data.haserror);
		return S4U_data.haserror;
	},

	// Check if WebSocket connected
	S4U_isconnected : function() {
		//_S4U_log('isconnected '+ S4U_data.isconnected);
		return S4U_data.isconnected;
	},

	// Set Trace for Debugging
	S4U_trace : function(trace) {
		S4U_data.trace = trace;
		_S4U_log('Trace set: '+trace,true);
	},

	// Set Polling
	S4U_polling : function(polling) {
		S4U_data.ispolling = polling;
		//_S4U_log('Polling set: '+polling);
	},

	// Get message length
	S4U_getlength : function() {
		//_S4U_log('getlength '+S4U_data.message.length+' bytes');
		if (S4U_data.hasmessage) {
			return S4U_data.message.length;
		}
		return -1;
	},		

	// Pick up message.
	S4U_getmessage : function() {
		//_S4U_log('getmessage');
		if (S4U_data.hasmessage) {
			S4U_data.hasmessage = false;
			//var buffer = _malloc(S4U_data.message.length);
			//HEAPU8.set(S4U_data.message, buffer);

			var base64 = _S4U_bytetobase64(S4U_data.message);
			var buffer = _S4U_str2byte(base64);
			return buffer;
		}
		return null;
	},

	// Send Websocket message
	S4U_send : function(bytes, length) {
		//_S4U_log('send');
		S4U_data.client.send (HEAPU8.buffer.slice(bytes, bytes+length));
	},

	// Convert String to Byte Array
	S4U_str2byte : function(str) {
		var bufferSize = lengthBytesUTF8(str) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(str, buffer, bufferSize);
		return buffer;
	},

	// Convert Byte Array to Base64 String
	S4U_bytetobase64 : function(bytes) {
		var sa = [];
		for (var i=0; i < bytes.length; i+=S4U_data.CHUNK_SZ) {
			sa.push(String.fromCharCode.apply(null, bytes.subarray(i, i+S4U_data.CHUNK_SZ)));
		}
		return btoa(sa.join(""));
	},

	// Send a Message to a Unity GameObject/Method
	S4U_sendmessage : function(method, str) {
		//_S4U_log('sendmessage to '+S4U_data.go+'.'+method+' '+str);
		if (S4U_data.go != null && method != null) {
			if (str == undefined) {
				SendMessage(S4U_data.go, method);
			}
			else {
				SendMessage(S4U_data.go, method, str);
			}
		}
	},

	S4U_log : function(msg, override) {
		if (override == undefined) { override = false; }
		if (S4U_data.trace || override) {
			console.log(msg);
		}
	}
}
autoAddDeps(SQL4UnityLibrary, '$S4U_data');
autoAddDeps(SQL4UnityLibrary, 'S4U_callonconnected');
autoAddDeps(SQL4UnityLibrary, 'S4U_callondisconnected');
autoAddDeps(SQL4UnityLibrary, 'S4U_callonmessage');
autoAddDeps(SQL4UnityLibrary, 'S4U_callonerror');
autoAddDeps(SQL4UnityLibrary, 'S4U_str2byte');
autoAddDeps(SQL4UnityLibrary, 'S4U_bytetobase64');
autoAddDeps(SQL4UnityLibrary, 'S4U_sendmessage');
autoAddDeps(SQL4UnityLibrary, 'S4U_log');
mergeInto(LibraryManager.library, SQL4UnityLibrary);
