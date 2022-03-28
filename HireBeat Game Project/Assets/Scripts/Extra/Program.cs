/*
 * Copyright SteveSmith.Software 2021. All rights reserverd
 * 
 * This code may be modified and/or copied
 * All derived works must contain and display the following copyright statement
 *		SQL4Unity and SQL4Unity Client/Server copyright SteveSmith.Software. All Rights Reserved
 * 
 * This is code for the standard SQL4Unity Client / Server TCP Administraton program.
 * It requires the SQL4Unity dll's supplied with the SQL4Unity Client/Server Service
 * 
 * The code can be used as a template for producing your own Admin process.
 * 
 * Code supplied under the MIT License
 */

using System;
using SQL4Unity.Server;
//using SQL4Unity.Service;

namespace SQL4Unity.Admin
{
	/*class Program
	{
		static string version = "V.1.0.0";
		static bool isOpen = false;

		static Utility utility = new Utility(); // Configuration Settings utilities
		static SQL4UnityClient client; // SQL4Unity TCP Client

		static void Main(string[] args)
		{

			Console.WriteLine("SQL for Unity Client/Server Administration. Version " + version + ". Copyright SteveSmith.Software 2021. All Rights Reserved");
			Console.WriteLine("");
			Console.WriteLine("Type ? or Help for commands and syntax");
			Console.WriteLine("");

			string UUID = utility.getConfigString("UUID"); // UUID must match the UUID of the Service process
			if (string.IsNullOrEmpty(UUID))
			{
				Console.WriteLine("Missing UUID in .config");
			}

			string IPAddress = utility.getConfigString("IPAddress"); // IPAddress of the Service Process listener

			int portNr = utility.getConfigInt("AdminTCPPort"); // Port number of the Amin TCP Service Process

			client = SQL4UnityClient.Create(Server.Protocol.TCP, UUID, IPAddress, portNr); // Create the Client Connection
			isOpen = client.OpenAdmin(); // Open the Client Connection

			if (!isOpen) Console.WriteLine("Error opening connection to " + IPAddress + ":" + portNr + " " + client.GetError());
			else Console.WriteLine("Connected to Server");
			Console.WriteLine("");

			// Process User commands
			bool quit = false;
			while (!quit)
			{
				Console.Write("? ");
				string command = Console.ReadLine();
				if (command.Length > 0)
				{
					string[] sa = command.Split(' ');
					string cmd = sa[0].Trim().ToLower();
					switch (cmd)
					{
						case "quit":
							// Exit Program
							if (isOpen)
							{
								client.CloseAdmin();
							}
							quit = true;
							break;

						case "close":
							// Close the connection
							if (isOpen)
							{
								client.CloseAdmin();
								Console.WriteLine("Connection closed");
								isOpen = false;
							}
							break;

						case "servers":
							// Show the server processes status
							GetData(cmd, ShowServers);
							break;

						case "clients":
							// Show the Client connections
							GetData(cmd, ShowClients);
							break;

						case "stop":
							// Stop the service or a client connection
							bool done = false;
							if (sa.Length == 2)
							{
								cmd = sa[1].Trim().ToLower();
								switch (cmd)
								{
									case "service":
										GetData("stop:service", null);
										break;
									case "all":
										GetData("stop:all", null);
										break;
									default:
										int id;
										bool ok = int.TryParse(cmd, out id);
										if (ok)
										{
											GetData("stop:" + cmd, null);
											done = true;
										}
										break;
								}
							}
							if (!done) Console.WriteLine("Invalid command format, try help");
							break;

						case "pause":
						case "resume":
							// Pause or Resume a server process
							if (sa.Length == 3)
							{
								string cmd1 = sa[1].Trim().ToLower();
								if (cmd1 == "admin" || cmd1 == "client")
								{
									string cmd2 = sa[2].Trim().ToLower();
									if (cmd2 == "tcp" || cmd2 == "ws")
									{
										GetData(cmd + ":" + cmd1 + ":" + cmd2, null);
										break;
									}
								}
							}
							Console.WriteLine("Invalid command format, try help");
							break;

						case "help":
						case "?":
							// Show the Help Text
							ShowHelp();
							break;

						case "about":
							// Show the About text
							ShowAbout();
							break;

						default:
							Console.WriteLine("Unknown command");
							break;
					}
				}
			}
		}

		// Send command to the server 
		static void GetData(string command, Action<object> callback)
		{
			client.SendToServer(command, callback);
			if (callback == null) Console.WriteLine("Command executed");
		}

		// Callback for 'Servers' command
		static void ShowServers(object result)
		{
			if (result == null) Console.WriteLine("No Information Avaliable");
			else
			{
				PortData[] results = (PortData[])result; // PortData contains the info on a Server

				Console.WriteLine(PortData.Headers());

				foreach (PortData portData in results)
				{
					Console.WriteLine(portData.ToString());
				}
			}
			Console.Write("? ");
		}

		// Callback for 'Clients' command
		static void ShowClients(object result)
		{
			if (result == null) Console.WriteLine("No Information Avaliable");
			else
			{
				Connection[] results = (Connection[])result; // Connection contains the info on a Client

				Console.WriteLine(Connection.Headers());

				foreach (Connection connection in results)
				{
					Console.WriteLine(connection.ToString());
				}
			}
			Console.Write("? ");
		}

		// Show the help text
		static void ShowHelp()
		{
			Console.WriteLine("");
			Console.WriteLine("SQL for Unity Client/Server Administration - Commands");
			Console.WriteLine("");
			Console.WriteLine("About                           - Show Contact, Copyright and Version information");
			Console.WriteLine("Close                           - Close the server connection");
			Console.WriteLine("Help                            - Show this help text");
			Console.WriteLine("Servers                         - Show the status of the Server processes");
			Console.WriteLine("Clients                         - Show the Client connection details");
			Console.WriteLine("Stop Service                    - Stop the SQL4Unity service");
			Console.WriteLine("Stop All                        - Stop all Client connections");
			Console.WriteLine("Stop ID                         - Stop the Client with ID (use Clients command to find the ID to stop)");
			Console.WriteLine("Pause Admin PROTOCOL            - Pause the Admin Server for the protocol");
			Console.WriteLine("Pause Client PROTOCOL           - Pause the Client Server for the protocol");
			Console.WriteLine("Resume Admin PROTOCOL           - Resume the Admin Server for the protocol");
			Console.WriteLine("Resume Client PROTOCOL          - Resume the Client Server for the protocol");
			Console.WriteLine("   where PROTOCOL above is one of TCP or WS");
			Console.WriteLine("Quit                            - Exit program");

			Console.WriteLine("");
			Console.WriteLine("Refer to the SQL4Unity Documentation at https://stevesmith.software for usage and syntax.");
			Console.WriteLine("");
		}

		// Show the About text
		static void ShowAbout()
		{
			Console.WriteLine("");
			Console.WriteLine("SQL for Unity Client/Server Administration - About");
			Console.WriteLine("");
			Console.WriteLine("SQL4Unity Copyright SteveSmith.Software 2018. All Rights Reserved");
			Console.WriteLine("");
			Console.WriteLine("Documentation - https://stevesmith.software");
			Console.WriteLine("Email         - sql4unity@stevesmith.software");
			Console.WriteLine("");
			Console.WriteLine("Version Information :-");
			Console.WriteLine("   SQL4UnityAdmin      - " + version);
			if (isOpen)
			{
				Console.WriteLine("   SQL4UnityClient     - " + client.version);
			}
			Console.WriteLine("");
		}

	}*/
}