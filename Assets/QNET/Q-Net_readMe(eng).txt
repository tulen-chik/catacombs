This txt file is a description of the library built-in functions.

It is recommended to use packet transmission since resource change is possible only in the main thread.

* Server Class ************************************************************************************
*

When doing class constructor, the first argument is the port number (up to +1 of the port number used at this time). The second argument is the maximum number of users to connect to this server.
Classes must be created as static.

* Server Class constructor
* Server(int port, int maxConnetSize)
*	port : Port forwarded port number, maxConnetSize: Maximum number of users that can be connected	
*	
*
**** Variable  ************************************************************************************
* Server.listener : Server Socket Information
* Server.port : Server port number
* Server.clientList : List of connected clients
* 	clientList.id : Client ID
* 	clientList.worker : Client socket
* 	clientList.buffer : Client buffer
* 	clientList.bufferQueue : Queue holding data to be stored in the client buffer
* Server.packetQueue : Server's packet queue
*
**** Event handlers ************************************************************************** 

Event handlers only need to be used on the server.
By writing in the main, you are registering an event on the server.

* Server.OnConnetHandler : Event notifying clients
When connecting to the server, the incoming number is automatically assigned by the server.


* Server.OnCloseHandler : Event notifying of client shutdown


* Server.OnErrorHandler : Event notifying the server of an exception error


* Server.OnReceiveHandler : Event notifying completion of data reception.
In this handler, you can get the value as e.receiveData.


* Server.OnSendHandler : Event notifying completion of data transmission
*
**** All events use lambda functions.
**** How to create an event
* Server.OnReceiveHandler += (e) =>
* {
* 	// The variable 'e' contains the data that came as the event.
* 	// something...
* };
*
***********************************************************************************************
*
**** Function *************************************************************************************
* Server.Start()
* 	return : void
*	Function to start the server
*
* Server.Stop()
* 	return void
* 	Shut down the server.
*
* Server.SetPacketQueueState()
*	return : void
*	Set whether to use the queue for saving the transmitted data separately.
* 	If there is no need to save data, it is recommended to receive data as an event of 'OnReceiveHandler'.
*	Basic setting : false
*	You can receive the data that came into 'server.OnReceiveHandler', but 'server.OnReceiveHandler' runs in the background thread.
*
* Server.GetClientCount()
*	return : int (number of clients)
*	Returns the number of connected clients.
*

size means the total number of rooms
This should be done before starting the server.

* Server.CreateSpace(int size) 
* Server.CreateSpace(int size, bool autoDelete)
* 	return : void
* 	size : Number of rooms
*	autoDelete : 'true' Temporary client automatically deletes client from space when connection is closed
*			'false' Temporary client does not delete from the space even if the connection is terminated. You have to manually delete it by calling 'Server.OutUserSpace'
* 	Create space.
* 	The ID value of the client is stored in the space.
* 	Used when a function such as room function or channel is required.
* 
* Server.GetSpaceCount()
* 	return : int (number of spaces)
* 	Returns the number of spaces created.
* 
* Server.GetSpaceArray()
* 	return : List <int> [] (sort entire list of spaces)
* 	Returns the entire space.
* 	
* 	
* Server.SetUserSpace(int _UserId, int _Space)
* 	return : void
* 	_UserId : Client ID, _Space: Space number to change
* 	Change the space of the entered client.
* 
* Server.GetUserSpace(int _UserId)
* 	return : int (Client's space number)
* 	_UserId : Client ID	
* 	Returns the space number of the entered client.
* 	
* Server.OutUserSpace(int _UserId)
* 	return : bool (Success status)
* 	_UserId : Client's ID	
* 	Initializes the space of the entered client and returns false if the initialization is successful.
* 	Initialized clients have a space number of '-1'.
* 	
* Server.GetSpaceUserArray(int _Space)
* 	return : int[]
* 	_Space : number of space
* 	Returns the array of clients' IDs in the space entered.
* 
*
****  Never write negative factors for 'Send'-related functions!  ****
*
* Server.Send(int To, byte[] buffer)
* Server.Send(int From, int To, byte[] buffer)
* Server.Send(int[] To, byte[] buffer)
* Server.Send(int From, int[] To, byte[] buffer)
* 	return : void
* 	From : ID to send data
* 	To : ID to receive data
* 	buffer: outgoing data
* 	Send byte data to clients with ID entered in 'To'.
* 	Insert an int-type array into 'To' to send data to all the IDs in the array.
* 	Clients receiving data can see who sent the data from 'From'.
* 	If 'From' is not present, data is sent to the server ID '-1'.
* 	Please do not put negative numbers in the factor.
*
* Server.SendToAll(byte[] buffer)
* Server.SendToAll(int From, byte[] buffer)
* 	return : void
* 	From : ID to send data
* 	buffer: outgoing data
* 	Send byte data to all clients connected to the server.
* 	Clients receiving data can see who sent the data from 'From'.
* 	If 'From' is not present, data is sent to the server ID '-1'.
* 	Please do not put negative numbers in the factor.
*
* Server.SendToSpace(int space, byte[] buffer)
* Server.SendToSpace(int space, int From, byte[] buffer) 
* 	return : void
* 	space : space number to send data to
* 	From : ID to send data
* 	buffer: outgoing data
* 	Send byte data to all clients in the space entered.
* 	Clients receiving data can see who sent the data from 'From'.
* 	If 'From' is not present, data is sent to the server ID '-1'.
* 	Please do not put negative numbers in the factor.
* 
* Server.Send_unSafe(int To, byte[] buffer)
* Server.Send_unSafe(int From, int To, byte[] buffer)
* Server.Send_unSafe(int[] To, byte[] buffer)
* Server.Send_unSafe(int From, int[] To, byte[] buffer)
* Server.SendToAll_unSafe(byte[] buffer)
* Server.SendToAll_unSafe(int From, byte[] buffer)
* Server.SendToSpace_unSafe(int space, byte[] buffer)
* Server.SendToSpace_unSafe(int space, int From, byte[] buffer) 	
*	 'unSafe' mode is an Udp communication that is not safe but allows fast data transfer.
* 	It will be useful when implementing multi-character moving lamps.
*
* Server.FindErrorCode(Exception e)
*	return : int(Error code number)
* 	Returns the error code of the entered 'Excription'.
*
* Server.CompareErrorCode(Exception e, int ErrorCode)
*	return : bool(Compare status)
*	Returns 'true' or 'false' if the entered error code of 'Excription' matches the entered error code number.
*
*
****************************************************************************************************	
	
* Client Class ***********************************************************************************
* 
* Client Class constructor
* Client(string address, int port)
* 	address : server address
* 	port : server port number
* 	The servers in our library use two ports.
* 	Use the entered ports of 'port' (TCP) and 'port + 1' (WCF).
*
* Client for Andoroid
* AndroidClient(string address, int port)
*
**** Variable ******************************************************************************************
* Client.connection : Socket information for client
* Client.id : Client ID
* Client.packetQueueue : Queue that stores incoming packets to the client
* 
**** Event Hendler ************************************************************************** 
* Client.OnConnetHandler : Events that indicate a connection with the server
* Client.OnCloseHandler : Events that signal a client shutdown
* Client.OnErrorHandler : Event informing client of an exception error
* Client.OnReceiveHandler : Event to notify completion of data reception
*
**** All events use a lambda function.
**** It's the same as event generation server.
* Client.OnReceiveHandler += (e) =>
* {
* 	// The variable 'e' contains the data that was sent to the event.
* 	// something...
* };
*
***********************************************************************************************
*
**** Function *************************************************************************************
* Client.Start()
*	return : void
* 	Client Start Function
*
* Client.Close()
*	return : void
*	Client Shutdown Function
*
* Client.isConnect()
* 	return : bool (Connection success status)
*	Return successful client connection
*
* Client.SetPacketQueueState()
*	Sets whether the queue stores the transmitted data separately.
* 	For Unity, communication through PacketQueueue is recommended because unity internal resources cannot be used outside the main thread.
* 	If you do not need to save the data, it is recommended that you receive the data as an 'OnReciveHandler' event.
* 	You can receive data from 'client.OnReciveHandler', but 'client.OnReciveHandler' operates in the background thread.
* 	Basic Settings: True
*
* Client.GetId()
*	return : int (client ID)
* 	Returns the ID of the client.
*
* Client.SetSpace(int space)
*	return : pool (success status)
* 	Space : Space number
* 	Set up the space for the client.
*
* Client.GetSpace()
*	return : int (space number)
* 	Returns the space number of the client.
*
* Client.OutSpace()
* 	return : pool (success status)
* 	Initialize the space on the client.
* 
* Client.GetSpaceUserArray()
*	return : int[] (addition of ID)
* 	Returns an array of IDs for all clients in the client's space.
*
*
*****  Never write negative factors for 'Send'-related functions!  ****
*
* Client.Send(int id, byte[] buffer)
* Client.Send(int[] id, byte[] buffer)
* 	return : void
*	id : ID and ID array to transmit
* 	buffer : sending data
* 	Sends data to other clients who are connected to the server.
*
* Client.SendToAll(byte[] buffer)
* 	return : void
* 	buffer : sending data
* 	Sends data to all clients who are connected to the server.
*
* Client.SendToSpace(byte[] buffer)
* 	return : void
* 	buffer : sending data
* 	Sends data to all clients in the same space.
* 
* Client.SendToServer(byte[] buffer)
*	return : void
* 	buffer : sending data
* 	Sends data to the server.
*
* Client.Send_unSafe(int id, byte[] buffer)
* Client.Send_unSafe(int[] id, byte[] buffer)
* Client.SendToAll_unSafe(byte[] buffer)
* Client.SendToSpace_unSafe(byte[] buffer)
* Client.SendToServer_unSafe(byte[] buffer)
* 	'unSafe' mode is an Udp communication, which is not safe, but allows fast data transmission.
* 	It will be useful when implementing multi-character moving lamps.
*
* Client.FindErrorCode(Exception e)
*	return : int(error code number)
* 	Returns the error code of the entered 'Excription'.
*
* Client.CompareErrorCode(Exception e, int ErrorCode)
*	return : pool (match status)
* 	Returns 'true' or 'false' if the entered error code of 'Exception' matches the entered error code number.
*
*
*
******************************************************************************************************

* PacketQueue Class *****************************************************************************
*
* PacketQueue Class constructor
* PacketQueue()
*
*
**** Variable ********************************************************************************************
* PacketQueue.Packet : Packet Storage Information
* 	Packet.id : sender ID
* 	Packet.buff : 'BufferManager' class
* 	Packet.size : Data size
*
* PacketQueue.Count : Number of packets
*
****************************************************************************************************
*
**** Function ******************************************************************************************
*
* PacketQueue.Clear()
* 	return : void
*	Initialize all data contained in the packetcue.
*
* PacketQueue.Enqueue(int id, BufferManager data, int size)
*	return : void
*	id : sender ID
* 	data : 'BufferManager' class
* 	size : data size
* 	Add data to the back of the packetcue.
*
* PacketQueue.Dequeue()
*	return : Packet
*	Ejects the data at the very front of the packetcue.
*
* PacketQueue.GetPacketCount()
*	return : int (number of packets)
* 	Returns the number of packets
*
*
*****************************************************************************************************

* BufferManager Class ***************************************************************************
* A class that supports functions that read and write buffers.
*
* BufferManager Class constructor
* BufferManager()
* BufferManager(byte[] recv)
* 	recv : byte data
*
**** Variable *****************************************************************************************
* BufferManager.buffer : byte array buffer
*
**** Function ********************************************************************************************
*
* BufferManager.ClearBuffer()
* 	return : void
* 	Initialize the buffer.
*
* BufferManager.GetBuffer()
* 	return : byte[] (buffer)
* 	Returns all remaining buffers read
*
* BufferManager.Read(out bool data)
* BufferManager.Read(out int data)
* BufferManager.Read(out float data)
* BufferManager.Read(out double data)
* BufferManager.Read(out string data, int strLength)
*	return : void
* 	Read the buffer as much as the data type in the data entered out and return it.
* 	'Read' functions read buffers sequentially, so the previous data cannot be read after reading.
* 	This does not erase the remaining buffers!
* 	In case of 'string', you must set the length to which you want to read it.
*
* BufferManager.ReadToBoolean()
* BufferManager.ReadToInt()
* BufferManager.ReadToFloat()
* BufferManager.ReadToDouble()
* BufferManager.ReadToString(int strLength)
*	return : 'dataType'
* 	The buffer is read as much as the data type that is organized by the function name and converted to that data type to return it.
* 	'Read' functions read buffers sequentially, so the previous data cannot be read after reading.
* 	This does not erase the remaining buffers!
* 	In case of 'string', you must set the length to which you want to read it.
* 
* BufferManager.Write(bool data)
* BufferManager.Write(int data)
* BufferManager.Write(float data)
* BufferManager.Write(double data)
* BufferManager.Write(string data)
*	return : void (input buffer);
* 	Bytes and pastes it to the buffer.
* 
****************************************************************************************************************

* DataBase Class *****************************************************************************************
* This library supports mysql innoDB link.
*
* DataBase Class constructor
* DataBase(string host, string dataBaseName, string id, string pass)
*	host : database address to connect to
* 	dataBaseName : Database name to connect to
* 	id : Database ID
* 	pass : Database password
*
*
**** Function ****************************************************************************************************
* doQuery(string query)
*	return : DataTable (query result statement)
* 	Query : Query statement
* 	Executes mysql query statement.
*
************************************************************************************************************
