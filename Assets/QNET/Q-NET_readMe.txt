이 txt 파일은 라이브러리 내장 함수에 대한 설명입니다.

유니티에서 바로 리소스 변경은 메인스레드에서만 가능하기때문에 패킷전송을 사용하도록한다.

* Server클래스 ************************************************************************************
*

클래스 생성자를 할 때 첫번째 인자는 포트번호, ( 이 때 사용된 포트번호의 +1 까지 사용합니다 ) 두번째 인자는 이 서버에 최대 접속자 제한입니다.
클래스를 스태틱으로 생성해야 합니다.
* Server클래스 생성자
* Server(int port, int maxConnetSize)
*	port : 포트포워딩된 포트번호, maxConnetSize : 접속가능한 최대인원수	
*	
*
**** 변수  ************************************************************************************
* Server.listener : 서버소켓의 정보
* Server.port : 서버의 포트번호
* Server.clientList : 접속한 클라이언트 리스트
* 	clientList.id : 클라이언트 아이디
* 	clientList.worker : 클라이언트 소켓
* 	clientList.buffer : 클라이언트 버퍼
* 	clientList.bufferQueue : 클라이언트 버퍼에 저장될 데이터를 보관하는 큐
* Server.packetQueue : 서버의 패킷큐
*
**** 이벤트 헨들러 ************************************************************************** 


이벤트 핸들러는 서버에서 사용하고 싶은 것만 사용하면 된다.
메인안에 작성함으로 서버에 이벤트를 등록 하는것이다.

* Server.OnConnetHandler : 클라이언트와 접속을 알리는 이벤트 
서버와 접속했을때 들어오는 번호는 서버에서 자동으로 할당해준다.


* Server.OnCloseHandler : 클라이언트 종료를 알리는 이벤트


* Server.OnErrorHandler : 서버에 예외 처리된 오류를 알리는 이벤트


* Server.OnReceiveHandler : 데이터 수신완료를 알리는 이벤트
이 핸들러 안에서 e.receiveData 로 값을 받을수 있다.


* Server.OnSendHandler : 데이터 송신완료를 알리는 이벤트
*
**** 이벤트 들은 전부 람다식 함수를 사용한다.
**** 이벤트 생성법
* Server.OnReceiveHandler += (e) =>
* {
* 	// 변수 'e' 에 해당이벤트로 날라온 데이터들이 들어있다.
* 	//something...
* };
*
***********************************************************************************************
*
**** 함수 *************************************************************************************
* Server.Start()
* 	return : void
*	서버를 시작하게하는 함수
*
* Server.Stop()
* 	return void
* 	서버를 종료합니다.
*
* Server.SetPacketQueueState()
*	return : void
*	송신된 데이터를 따로 저장하는 큐의 사용여부를 설정합니다.
*        	굳이 데이터를 저장할 필요가 없다면 'OnReceiveHandler'의 이벤트로 데이터를 받을것을 권장합니다.
*	기본설정 : false
*	'server.OnReceiveHandler' 으로 들어온 데이터를 받을수 있지만 'server.OnReceiveHandler'은 백그라운드 쓰레드에서 동작됩니다.  
*
* Server.GetClientCount()
*	return : int (클라이언트 갯수)
*	접속된 클라이언트의 갯수를 반환해줍니다.
*

size는 방 전체 갯수를 의미
서버 스타트 하기전에 해야 한다.

* Server.CreateSpace(int size) 
* Server.CreateSpace(int size, bool autoDelete)
* 	return : void
* 	size : 방갯수, 
*	autoDelete : 'true' 일시 클라이언트가 접속종료시 자동으로 스페이스에서 클라이언트를 지움
*                              'false' 일시 클라이언트가 접속종료를 해도 스페이스에서 지우지않음. 'Server.OutUserSpace' 를 호출하여 수동으로 직접 지워야함	
* 	스페이스를 생성합니다. 
* 	스페이스에는 클라이언트의 아이디값이 저장됩니다.
* 	방기능이나 채널같은 기능이 필요할시 사용.	
* 
* Server.GetSpaceCount()
* 	return : int (스페이스 갯수)
* 	생성된 스페이스의 갯수를 반환해줍니다.
* 
* Server.GetSpaceArray()
* 	return : List<int>[] (스페이스 전체 리스트배열)
* 	스페이스 전체를 반환해줍니다. 
* 	
* 	
* Server.SetUserSpace(int _UserId, int _Space)
* 	return : void
* 	_UserId : 클라이언트의 아이디, _Space : 변경할 스페이스 번호	
* 	입력된 클라이언트의 스페이스를 변경합니다.	
* 
* Server.GetUserSpace(int _UserId)
* 	return : int (클라이언트의 스페이스 번호)
* 	_UserId : 클라이언트의 아이디	
* 	입력된 클라이언트의 스페이스 번호를 반환합니다.
* 	
* Server.OutUserSpace(int _UserId)
* 	return : bool (성공여부)
* 	_UserId : 클라이언트의 아이디	
* 	입력된 클라이언트의 스페이스를 초기화 하고 초기화가 성공시 true 실패시 false를 반환합니다.
* 	초기화된 클라이언트의 스페이스번호는 '-1' 이됩니다.
* 	
* Server.GetSpaceUserArray(int _Space)
* 	return : int[]
* 	_Space : 스페이스의 번호
* 	입력된 스페이스안에 있는 클라이언트의 아이디배열을 반환합니다.
* 
*
****  'Send' 관련 함수에는 절대 인자로 음수를 적지 말아주세요!  ****
*
* Server.Send(int To, byte[] buffer)
* Server.Send(int From, int To, byte[] buffer)
* Server.Send(int[] To, byte[] buffer)
* Server.Send(int From, int[] To, byte[] buffer)
* 	return : void
* 	From :  데이터를 보내는 아이디
* 	To : 데이터를 받는 아이디
* 	buffer : 보내는 데이터
* 	'To'에 입력된 아이디의 클라이언트에게 바이트형 데이터를 보냅니다.
* 	'To'에 int형 배열을 넣으면 배열안에 있는 모든 아이디에게 데이터를 보냅니다.
* 	데이터를 받는 클라이언트는 'From'을 통해 누가 데이터를 보냇는지 알 수 있습니다.
* 	'From'의 값이 없으면 서버 아이디인 '-1' 로 데이터를 보냅니다.
* 	인자에 음수를 넣지 말아주세요.	
*
* Server.SendToAll(byte[] buffer)
* Server.SendToAll(int From, byte[] buffer)
* 	return : void
* 	From : 데이터를 보내는 아이디
* 	buffer : 보내는 데이터
* 	서버에 접속해있는 모든 클라이언트에게 바이트형 데이터를 보냅니다.
* 	데이터를 받는 클라이언트는 'From'을 통해 누가 데이터를 보냇는지 알 수 있습니다.
* 	'From'의 값이 없으면 서버 아이디인 '-1' 로 데이터를 보냅니다.
* 	인자에 음수를 넣지 말아주세요.
*
* Server.SendToSpace(int space, byte[] buffer)
* Server.SendToSpace(int space, int From, byte[] buffer) 
* 	return : void
* 	space : 데이터를 보낼 스페이스 번호
* 	From : 데이터를 보내는 아이디
* 	buffer : 보내는 데이터
* 	입력된 스페이스안에 있는 모든 클라이언트에게 바이트형 데이터를 보냅니다.
* 	데이터를 받는 클라이언트는 'From'을 통해 누가 데이터를 보냇는지 알 수 있습니다.
* 	'From'의 값이 없으면 서버 아이디인 '-1' 로 데이터를 보냅니다.
* 	인자에 음수를 넣지 말아주세요.
* 
* Server.Send_unSafe(int To, byte[] buffer)
* Server.Send_unSafe(int From, int To, byte[] buffer)
* Server.Send_unSafe(int[] To, byte[] buffer)
* Server.Send_unSafe(int From, int[] To, byte[] buffer)
* Server.SendToAll_unSafe(byte[] buffer)
* Server.SendToAll_unSafe(int From, byte[] buffer)
* Server.SendToSpace_unSafe(int space, byte[] buffer)
* Server.SendToSpace_unSafe(int space, int From, byte[] buffer) 	
*	 'unSafe' 모드는 Udp 통신으로서 안전하지 않지만 빠른데이터 전송이 가능합니다.
*	다중 캐릭터 이동등을 구현할때 유용하게 사용될 것입니다.
*
* Server.FindErrorCode(Exception e)
*	return : int(에러코드번호)
* 	입력된 'Excption'의 에러코드를 반환해줍니다.
*
* Server.CompareErrorCode(Exception e, int ErrorCode)
*	return : bool(일치여부)
*	입력된 'Excption' 의 에러코드가 입력된 에러코드 번호와 일치하면 'true' 아니면 'false'를 반환합니다.
*
*
****************************************************************************************************	
	
* Client 클래스 ***********************************************************************************
* 
* Client클래스 생성자
* Client(string address, int port)
* 	address : 서버 주소
* 	port : 서버 포트번호
* 	우리 라이브러리의 서버는 두개의 포트를 사용한다.
* 	입력된 'port' (TCP)와 'port + 1' (WCF)의 포트를 사용한다.
*
* 안도로이드용 클라이언트
* AndroidClient(string address, int port)
*
**** 변수 ******************************************************************************************
* Client.connection : 클라이언트의 소켓정보
* Client.id : 클라이언트의 아이디
* Client.packetQueue : 클라이언트에 들어오는 패킷을 저장하는 큐
* 
**** 이벤트 헨들러 ************************************************************************** 
* Client.OnConnetHandler : 서버와 접속을 알리는 이벤트 
* Client.OnCloseHandler : 클라이언트 종료를 알리는 이벤트
* Client.OnErrorHandler : 클라이언트에 예외 처리된 오류를 알리는 이벤트
* Client.OnReceiveHandler : 데이터 수신완료를 알리는 이벤트
*
**** 이벤트 들은 전부 람다식 함수를 사용한다.
**** 이벤트 생성법 서버랑 똑같다.
* Client.OnReceiveHandler += (e) =>
* {
* 	// 변수 'e' 에 해당이벤트로 날라온 데이터들이 들어있다.
* 	//something...
* };
*
***********************************************************************************************
*
**** 함수 *************************************************************************************
* Client.Start()
*	return : void
* 	클라이언트 시작함수
*
* Client.Close()
*	return : void
*	클라이언트 종료함수
*
* Client.isConnect()
* 	return : bool (연결성공여부)
*	클라이언트 연결성공여부를 반환
*
* Client.SetPacketQueueState()
*	송신된 데이터를 따로 저장하는 큐의 사용여부를 설정합니다.
*	Unity의 경우 unity내부 리소스등은 메인쓰레드가 아닌곳에서는 사용을 할수 없으므로 'PacketQueue'를 통한 통신을 권장합니다.
*        	굳이 데이터를 저장할 필요가 없다면 'OnReceiveHandler'의 이벤트로 데이터를 받을것을 권장합니다.
*	'client.OnReceiveHandler' 으로 들어온 데이터를 받을수 있지만 'client.OnReceiveHandler'은 백그라운드 쓰레드에서 동작됩니다.  	
*	기본설정 : true
*
* Client.GetId()
*	return : int (클라이언트 아이디)
*	클라이언트의 아이디를 반환합니다.
*
* Client.SetSpace(int space)
*	return : bool (성공여부)
*	space : 스페이스 번호
*	클라이언트의 스페이스를 설정합니다.
*
* Client.GetSpace()
*	return : int (스페이스 번호)
*	클라이언트의 스페이스 번호를 반환합니다.
*
* Client.OutSpace()
* 	return : bool (성공여부)
* 	클라이언트의 스페이스를 초기화 합니다.
* 
* Client.GetSpaceUserArray()
*	return : int[] (아이디 배열)
*	클라이언트의 스페이스에 들어있는 모든 클라이언트의 아이디 배열을 반환합니다.
*
*
*****  'Send' 관련 함수에는 절대 인자로 음수를 적지 말아주세요!  ****
*
* Client.Send(int id, byte[] buffer)
* Client.Send(int[] id, byte[] buffer)
* 	return : void
*	id : 송신할 아이디 및 아이디배열
* 	buffer : 송신 데이터
* 	서버에 접속해 있는 다른 클라이언트에게 데이터를 보냅니다.
*
* Client.SendToAll(byte[] buffer)
* 	return : void
*	buffer : 송신 데이터
*	서버에 접속해 있는 모든 클라이언트에게 데이터를 보냅니다.
*
* Client.SendToSpace(byte[] buffer)
* 	return : void
*	buffer : 송신 데이터
*	같은 스페이스에 있는 모든 클라이언트에게 데이터를 보냅니다.
* 
* Client.SendToServer(byte[] buffer)
*	return : void
* 	buffer : 송신 데이터
*	서버에게 데이터를 보냅니다.
*
* Client.Send_unSafe(int id, byte[] buffer)
* Client.Send_unSafe(int[] id, byte[] buffer)
* Client.SendToAll_unSafe(byte[] buffer)
* Client.SendToSpace_unSafe(byte[] buffer)
* Client.SendToServer_unSafe(byte[] buffer)
*	 'unSafe' 모드는 Udp 통신으로서 안전하지 않지만 빠른데이터 전송이 가능합니다.
*	다중 캐릭터 이동등을 구현할때 유용하게 사용될 것입니다.
*
* Client.FindErrorCode(Exception e)
*	return : int(에러코드번호)
* 	입력된 'Excption'의 에러코드를 반환해줍니다.
*
* Client.CompareErrorCode(Exception e, int ErrorCode)
*	return : bool(일치여부)
*	입력된 'Excption' 의 에러코드가 입력된 에러코드 번호와 일치하면 'true' 아니면 'false'를 반환합니다.
*
*
*
******************************************************************************************************

* PacketQueue 클래스 *****************************************************************************
*
* PacketQueue 클래스 생성자
* PacketQueue()
*
*
**** 변수 ********************************************************************************************
* PacketQueue.Packet : 패킷 저장 정보
* 	Packet.id : 송신자 아이디
*	Packet.buff : 'BufferManager' 클래스
* 	Packet.size : 데이터 크기
*
* PacketQueue.Count : 패킷갯수
*
****************************************************************************************************
*
**** 함수 ******************************************************************************************
*
* PacketQueue.Clear()
* 	return : void
*	패킷큐에 담겨있는 모든 데이터를 초기화 합니다.
*
* PacketQueue.Enqueue(int id, BufferManager data, int size)
*	return : void
*	id : 송신자 아이디
*	data : 'BufferManager' 클래스
*	size : 데이터 크기
*	패킷큐의 맨뒤에 데이터를 추가합니다.
*
* PacketQueue.Dequeue()
*	return : Packet
*	패킷큐에 들어있는 맨앞의 데이터를 꺼냅니다.
*
* PacketQueue.GetPacketCount()
*	return : int (패킷갯수)
* 	패킷 갯수를 반환합니다.
*
*
*****************************************************************************************************

* BufferManager 클래스 ***************************************************************************
* 버퍼를 읽고 쓰는 기능들을 지원하는 클래스
*
* BufferManager 클래스 생성자
* BufferManager()
* BufferManager(byte[] recv)
* 	recv : 바이트 데이터
*
**** 변수 *****************************************************************************************
* BufferManager.buffer : 바이트 배열인 버퍼
*
**** 함수 ********************************************************************************************
*
* BufferManager.ClearBuffer()
* 	return : void
* 	버퍼를 초기화 합니다.
*
* BufferManager.GetBuffer()
* 	return : byte[] (버퍼)
*	읽고 남은 버퍼전체를 반환합니다.
*
* BufferManager.Read(out bool data)
* BufferManager.Read(out int data)
* BufferManager.Read(out float data)
* BufferManager.Read(out double data)
* BufferManager.Read(out string data, int strLength)
*	return : void
* 	out으로 입력된 데이터안에 데이터타입만큼 버퍼를 읽어 반환해줍니다.
* 	'Read' 함수들은 순차적으로 버퍼를 읽기 때문에 읽고난 이전데이터는 읽을수 없습니다.
* 	그렇다고 뒤에 남은 버퍼가 지워지는건 아닙니다!
*	'string'의 경우 어디까지 읽을지 길이를 설정해 주어야합니다.
*
* BufferManager.ReadToBoolean()
* BufferManager.ReadToInt()
* BufferManager.ReadToFloat()
* BufferManager.ReadToDouble()
* BufferManager.ReadToString(int strLength)
*	return : 'dataType'
* 	함수 이름으로 정이된 데이터타입만큼 버퍼를 읽어 해당 데이터타입으로 변환하면 반환해 줍니다.
*	'Read' 함수들은 순차적으로 버퍼를 읽기 때문에 읽고난 이전데이터는 읽을수 없습니다.
* 	그렇다고 뒤에 남은 버퍼가 지워지는건 아닙니다!
*	'string'의 경우 어디까지 읽을지 길이를 설정해 주어야합니다.
* 
* BufferManager.Write(bool data)
* BufferManager.Write(int data)
* BufferManager.Write(float data)
* BufferManager.Write(double data)
* BufferManager.Write(string data)
*	return : void (입력 버퍼);
*	이렵된 데이터를 바이트화 시켜 버퍼에 붙여줍니다.
* 
****************************************************************************************************************

* DataBase 클래스 *****************************************************************************************
* 우리 라이브러리는 mysql innoDB 연동을 지원 합니다.
*
* DataBase 클래스 생성자
* DataBase(string host, string dataBaseName, string id, string pass)
*	host : 접속할 데이터베이스 주소
* 	dataBaseName : 접속할 데이터베이스 이름
*	id : 데이터베이스 아이디
*	pass : 데이터베이스 비밀번호
*
*
**** 함수 ****************************************************************************************************
* doQuery(string query)
*	return : DataTable(쿼리 결과문)
*	query : 쿼리문
*	mysql 쿼리문을 실행해 줍니다.
*
************************************************************************************************************
