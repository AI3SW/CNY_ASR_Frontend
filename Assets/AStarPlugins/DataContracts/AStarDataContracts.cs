using System;
using System.Runtime.Serialization;

namespace Astar.WebSocket.DataContracts.Receive
{
	namespace HLTASRLib
	{

		[Serializable]
		[DataContract]
		public class ASRResult
		{
			[DataMember]
			public string cmd;
			[DataMember]
			public UInt64 uttID;
			[DataMember]
			public string result;
		}
	}

	[Serializable]
	[DataContract]
	public class SentenceInfo
	{

		[DataMember]
		public string session_id;
		[DataMember]
		public string right_text;
		[DataMember]
		public string sequence_id;
	}


}
namespace Astar.WebSocket
{
	[Serializable]
	[DataContract]
	public class ASRConfigFile
	{

		[DataMember]
		public string ip;
		[DataMember]
		public string port;
	}
}
