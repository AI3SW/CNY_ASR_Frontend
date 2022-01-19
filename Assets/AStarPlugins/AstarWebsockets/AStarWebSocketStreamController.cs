using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Net;
using System.Net.NetworkInformation;

//using NAudio;
//using NAudio.Wave;
using UnityEngine;
namespace Astar.WebSocket
{
	/// <summary>
	/// T needs to be an jsonObject DataContract, and needs the server to send it in the same data contract format
	/// Else the class will simply call the callback for onByteResults.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class AStarWebSocketStreamController<T>
	{
		//public static System.Net.Security.RemoteCertificateValidationCallback ServerCertificateValidationCallback { get; set; }
		public AStarWebSocketStreamController(string url, int reconnectionDelay = 300, int reconnectionCount = 3, string ping_ip = "www.google.com")
		{
			_running = false;
			_ws = null;
			_unexpectedDisconnect = false;

			_url = url;
			_ping_ip = ping_ip;
			_reconnectionDelay = reconnectionDelay;
			_reconnectionCount = reconnectionCount;
			//_isReconnecting = false;
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			//streamIsUnused = new Task<bool>(isStreamUsed);
			sendingStream = false;
			//_threadRecv = null;
			//jsonReturnType = typeof(T).GetTypeInfo();
		}
		/// <summary>
		/// returns false when connection is cannot be created
		/// </summary>
		/// <typeparam name="T"></typeparam> , where T is the Type to Listen For
		/// <returns></returns>
		async public Task<bool> Init()
		{
			bool successfulConnection = false;

			try
			{
				_ws = new ClientWebSocket();
				_ws.Options.KeepAliveInterval = TimeSpan.Zero;
				_ws.Options.SetRequestHeader("User-Agent", "Unity3D");
				Debug.Log(_url); 
				
				CancellationTokenSource source = new CancellationTokenSource(10000);
				//source.CancelAfter(1000); // error pops out now

				_Uri = new Uri(_url);
				Task Connection = _ws.ConnectAsync(_Uri, source.Token);
				//source.
				//Task  // throw error, certicate related.

				int reconnectDelay = _reconnectionDelay;
				int reconnectCount = _reconnectionCount;
				while (_ws.State != WebSocketState.Open && reconnectCount > 0)
                {
					await Connection;// throw error after this line
					if (_ws.State == WebSocketState.Open)
					{

						StartListening();
						successfulConnection = true;
						//Debug.Log("test1");
					}
					else 
					{
						Debug.Log("Reconnecting in " + reconnectDelay + "ms. Count Remaining " + reconnectCount);
						await Task.Delay(reconnectDelay);
						--reconnectCount;
						//Debug.Log("test2");
					}

					
				}
				if(_ws.State != WebSocketState.Open)
                {
					throw new WebSocketException("WebSocketState is Currently " + _ws.State);
				}

				///////////////////////////////////////////////////////////////////
				//recording has started.

				 // start a async task. 
				//_threadRecv = new Thread(threadRecv);
				//_threadRecv.Start(this);
			}
			catch (Exception ex)
			{
				Astar.Utils.ErrorUtils.printAllErrors(ex);
				if (ex.Message.Contains("Unable to connect to the remote server"))
                {
					Debug.LogWarning("check for wifi / connection and try again");
                }
				successfulConnection = false;
				//checkForConnectivityAndReconnection();
			}
			return successfulConnection;
		}

		async public void exit()
		{
			_running = false;
			if (_ws != null)
			{
				if (_ws.State == WebSocketState.Open)
				{
					try
					{

						Debug.Log("Disconnect state " + _ws.State);
						Task disconect = _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
						await disconect;

					}
					catch (Exception ex)
					{
						Astar.Utils.ErrorUtils.printAllErrors(ex);
					}
				}
				//Debug.Log("socket state " + _ws.State);
				_ws.Dispose();
				_ws = null;
			}

			
			/*
			if(_threadRecv != null)
            {
				_threadRecv.Abort();
				_threadRecv = null;
			}*/
		}

		public async Task stream(Utils.AstarStreamWrapper streamWrapper)
		{
			if(_ws != null && _running)
            {
				try
                {
					switch (streamWrapper.usageType)
					{
						//case Utils.AstarStreamWrapper.wsUsage.ASR_DAT: // DAT data is light weight
						//	await _ws.SendAsync(streamWrapper.outMsg, streamWrapper.messageType, true, CancellationToken.None);
						//	break;
						default:// for string and unformatted binary, we ensure that they are send in proper chunks if they exceed limit
							int messagesCount = (int)Math.Ceiling((double)streamWrapper.outMsg.Count / 4096);
							//Debug.Log(streamWrapper.outMsg.Count);
							for (int i = 0; i < messagesCount; i++)
							{
								int offset = (ChunkSize * i);
								var count = ChunkSize;
								bool lastMessage = ((i + 1) == messagesCount);

								if ((count * (i + 1)) > streamWrapper.outMsg.Count)
								{
									count = streamWrapper.outMsg.Count - offset;
								}
								
								await isStreamUsed();
								//Debug.Log("using");
								sendingStream = true;
								await _ws.SendAsync(new ArraySegment<byte>(streamWrapper.outMsg.Array, offset, count), streamWrapper.messageType, lastMessage, CancellationToken.None);
								sendingStream = false;
								//Debug.Log("freed");
							}
							break;
					}
					
				}
				catch (Exception ex)
                {
					sendingStream = false;
					Astar.Utils.ErrorUtils.printAllErrors(ex);
					//exit();
					//
				}
				//Debug.Log("sending");
			}
			return;
		}

		//T will be the Json object to be serialized into;
		private Astar.Utils.Websocket.OnStreamResultEventArgs<T> parseBufferIntoJson(byte[] buf, int length, int ind = 0)
		{
			MemoryStream stream = new MemoryStream(buf, ind, length);

			DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(T));
			Astar.Utils.Websocket.OnStreamResultEventArgs<T> args = new Astar.Utils.Websocket.OnStreamResultEventArgs<T>();
			try
			{
				T result = (T)s.ReadObject(stream);
				args.eventData = result;
				args.parseError = false;
			}
			catch (Exception ex)
			{
				args.parseError = true;
				Astar.Utils.ErrorUtils.printAllErrors(ex);
			}
			return args;
		}

		private Astar.Utils.Websocket.OnStreamResultEventArgs<string> parseBufferIntoString(byte[] buf)
		{

			Astar.Utils.Websocket.OnStreamResultEventArgs<string> args = new Astar.Utils.Websocket.OnStreamResultEventArgs<string>();
			try
			{
				args.eventData = Encoding.UTF8.GetString(buf);
				args.parseError = false;
			}
			catch (Exception ex)
			{
				args.parseError = true;
				Astar.Utils.ErrorUtils.printAllErrors(ex);
			}
			return args;
		}
		
		public bool isConnected()
        {
			return _running;
        }
		public void startAutoPing()
		{
			_pingLoopTask = true;
			checkForConnectivityAndReconnection(doPingReplyAndAutoReconnect);
			//_pingLoopTask.Dispose();
		}
		public void killAutoPing()
		{
			//_pingLoopTask = checkForConnectivityAndReconnection(doPingReplyAndAutoReconnect);
			_pingLoopTask = false;
		}
		async private Task ForceReconnect()
        {
			//ignores the error and assume bad connection and tries to init again via init;

			exit(); //calling again to makesure its a new socket;
			//allow time 500ms to allow threads to deactivate before re-initiating
			//await Task.Delay(500);

			Task<bool> reconnecting = Init(); // init does a 3 pass connection, then ends if fails
			Debug.Log("Connecting to server");
			await reconnecting; // no need to await 
			Debug.Log("Connected");
			if (reconnecting.Result)
			{
				if(_unexpectedDisconnect)
                {
					OnReconnect?.Invoke();

					_unexpectedDisconnect = false;
					Debug.Log("Connection Re-established");
				} else {
					OnConnect?.Invoke();
					Debug.Log("Connection established");
				}
				
			}
			else
			{
				Debug.Log("Cannot Reconnect, Refer to Warning");

			}

			//}
		}

		async private void doPingReplyAndAutoReconnect(object sender, PingCompletedEventArgs e) {
			//Debug.Log("test");
			if (e.Cancelled) // when ping is canccelled
			{
				Console.WriteLine("Ping canceled.");
			}
			// If an error occurred, display the exception to the user.
			else if (e.Error != null)
			{
				Console.WriteLine("Ping has error / ping failed:");
				Console.WriteLine(e.Error.ToString());
				if (e.Reply.Status == IPStatus.TimedOut)
				{
					
					Debug.Log("Cannot Connect as ping timeout");
					((AutoResetEvent)e.UserState).Set();
				}
				else
				{
					await ForceReconnect();

					((AutoResetEvent)e.UserState).Set();
				}
			}
			else
			{
				((AutoResetEvent)e.UserState).Set();
				if (_running)
					Debug.Log("Still connected to server");
				else
                {
					Debug.Log("Can ping to google aka, theres connection");
					//thus i reconnect
					await ForceReconnect();
				}
					
			}

			
		}



		async private void checkForConnectivityAndReconnection(PingCompletedEventHandler callback)
        {
			System.Net.NetworkInformation.Ping pingSender = new System.Net.NetworkInformation.Ping();
			AutoResetEvent waiter = new AutoResetEvent(false);
			int autoPingTimer = 1000 * 5;
			int pingTimeout = 1000 * 1;

			//VERY IMPORTANT LINE, calls the connection function everytime pings is sucessful.
			pingSender.PingCompleted += callback;
			
			while(_pingLoopTask) //keep pining server when i am still connected to check for disconnection
            {
                try
                {
					pingSender.SendAsync(_ping_ip, pingTimeout, waiter);
					//Debug.Log("send");
				}
				catch (System.Exception ex)
                {
					Astar.Utils.ErrorUtils.printAllErrors(ex);
					//case 1, no network connection
					if (_running)
                    {
						//if websocket has been established.
						_unexpectedDisconnect = true;
						OnUnexpectedDisconnection?.Invoke();
					} else
                    {
						OnNoNetworkConnection?.Invoke();
					}


					//Debug.Log("autoreconnect");
					//await ForceReconnect();
				}
				//Debug.Log("Pinging "+ _ip);
				await Task.Delay(autoPingTimer);
			}
			//Debug.Log("pingLast");
		}
		/*
		public Action<T> CreateListenOfType<T>()
        {
			Action<Action<T>> callback = (Action<T> cb) => { };
			StartListening<T>(callback);
			return callback;
		}
		*/
		async private void StartListening()
		{
			WebSocketReceiveResult receivedResult;
			byte[] buf = new byte[2048]; // 2kb.
										 //ArraySegment<byte> segementBuffer = new ArraySegment<byte>(buf);
			Queue<byte> collectedMsg = new Queue<byte>();
			Queue<byte> collectedByte = new Queue<byte>();
			//CancellationTokenSource source = new CancellationTokenSource(10000);

			_running = true;
			try
			{

				while (_running)
				{
					do
					{
						receivedResult = await _ws.ReceiveAsync(new ArraySegment<byte>(buf), CancellationToken.None);
						for (int i = 0; i < receivedResult.Count; ++i)
						{
							if(receivedResult.MessageType == WebSocketMessageType.Binary)
                            {
								collectedByte.Enqueue(buf[i]);
							} else
                            {
								collectedMsg.Enqueue(buf[i]);
							}
							
						}

					} while (!receivedResult.EndOfMessage);

					//send complete

					if (receivedResult.MessageType == WebSocketMessageType.Binary)
					{
						Astar.Utils.Websocket.OnStreamResultEventArgs<T> eventArgsJSON = parseBufferIntoJson(collectedByte.ToArray(), collectedByte.ToArray().Length);
						
						if (!eventArgsJSON.parseError)
						{
							OnStreamResult?.Invoke(this, eventArgsJSON);
							Debug.Log("json");
						}
						else
                        {
							Astar.Utils.Websocket.OnStreamResultEventArgs<byte[]> eventArgsByte = new Astar.Utils.Websocket.OnStreamResultEventArgs<byte[]>();
							eventArgsByte.eventData = collectedByte.ToArray();
							OnByteResult?.Invoke(this, eventArgsByte);
							Debug.LogWarning("Class Usage is incorrect or Json parsing has failed.");
						}
					}	
					else if (receivedResult.MessageType == WebSocketMessageType.Text)
					{

						//Debug.Log("receiving");
						Astar.Utils.Websocket.OnStreamResultEventArgs<string> eventArgsString = parseBufferIntoString(collectedMsg.ToArray());
						//Debug.Log(eventArgsString.eventData);
						if (eventArgsString.eventData != null)
                        {
							
							if (eventArgsString.eventData[0] == '0')
							{
								OnStringResult?.Invoke(this, eventArgsString);
							}
							else
							{
								string temp = eventArgsString.eventData;
								//Debug.Log(temp);

								Astar.Utils.Websocket.OnStreamResultEventArgs<T> eventArgsJSON = parseBufferIntoJson(collectedMsg.ToArray(),collectedMsg.ToArray().Length);
								if (!eventArgsJSON.parseError)
								{
									OnStreamResult?.Invoke(this, eventArgsJSON);
								}
								else
								{
									OnStringResult?.Invoke(this, eventArgsString);
								}
							}
						}		
						//Debug.Log("string");
					} else
                    {
						//Disconnect
                    }
					//Debug.Log("clearing queue");
					//Debug.Log(collectedMsg.Count);
					collectedMsg.Clear();
					collectedByte.Clear();
					await Task.Delay(50);
					//Debug.Log(collectedMsg.Count);
				}
			}
			catch (Exception ex)
			{

				Astar.Utils.ErrorUtils.printAllErrors(ex);
				//exit();
				//throw new System.Exception("case is handled, throwing to end thread loop.");
			}
		}
		
		/*
		private static void threadRecv(System.Object obj)
		{
			((AStarWebSocketStreamController<T>)obj).doThreadRecv();
		}
		*/

		
		//Event or action
		public event EventHandler<Astar.Utils.Websocket.OnStreamResultEventArgs<T>> OnStreamResult;
		public event EventHandler<Astar.Utils.Websocket.OnStreamResultEventArgs<byte[]>> OnByteResult;
		public event EventHandler<Astar.Utils.Websocket.OnStreamResultEventArgs<string>> OnStringResult;
		public event Action OnConnect;
		public event Action OnReconnect;
		public event Action OnUnexpectedDisconnection;
		public event Action OnNoNetworkConnection;


		ClientWebSocket _ws;
		bool _running;
		//Thread _threadRecv;
		private string _url;
		private Uri _Uri;
		private string _ping_ip;
		/// <summary>
		/// 4 
		/// </summary>
		public static readonly int ChunkSize = 4096;

		private int _reconnectionDelay;
		private int _reconnectionCount;

		private bool _pingLoopTask;
		private bool _unexpectedDisconnect;

		bool sendingStream = false;
		//Task<bool> streamIsUnused;

		async Task<bool> isStreamUsed()
        {
			//Debug.Log("waiting : status of Sending stream is : " + sendingStream.ToString());
			while(sendingStream == true)
            {
				await Task.Delay(10); // need delay 
				continue;
            }
			return sendingStream == false;

		}
		
	}
	

}

