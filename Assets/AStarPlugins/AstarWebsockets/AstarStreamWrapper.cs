using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.Net.WebSockets;
using System.Reflection;

namespace Astar.WebSocket.Utils
{
	public class AstarStreamWrapper
	{
        #region Variables
        public enum wsUsage {
			UNFORMATTED_BINARY = 0,
			TEXT,
			ASR_DAT
		}

		public WebSocketMessageType messageType
		{
			get;
			private set;
		}

		public ArraySegment<byte> outMsg
        {
			get;
			private set;
        }
		public wsUsage usageType
		{
			get;
			private set;
		}
		#endregion
		#region Constructor   
		private AstarStreamWrapper() { }
		public AstarStreamWrapper(byte[] streamToCopy, wsUsage type)
		{
			usageType = type;
			outMsg = ConfigureBinaryData(streamToCopy);
		}

		public AstarStreamWrapper(string stringToCopy)
		{
			usageType = wsUsage.TEXT;
			messageType = WebSocketMessageType.Text;
			byte[] newByteArray = Encoding.UTF8.GetBytes(stringToCopy);
			outMsg = new ArraySegment<byte>(newByteArray);
		}

        #endregion
        public ArraySegment<byte> ConfigureBinaryData(byte[] streamToCopy)
        {
			ArraySegment<byte> msg = new ArraySegment<byte>() ;
			switch (usageType)
            {

				case wsUsage.UNFORMATTED_BINARY:
					{
						messageType = WebSocketMessageType.Binary;
						byte[] newByteArray = new byte[streamToCopy.Length];
						Buffer.BlockCopy(newByteArray, 0, streamToCopy, 0, streamToCopy.Length);
						msg = new ArraySegment<byte>(streamToCopy);

					}
					break;
				case wsUsage.TEXT:
					{
						throw new InvalidOperationException("Should be using the string constructor");
					}
					//break;
				case wsUsage.ASR_DAT:
					{	//direct buffer from NAudio/Recording stream
				 
						messageType = WebSocketMessageType.Binary;
						// Downsamples 32bit to 16bit from unity Microphone
						byte[] downsampleStream = downsampleAudio(streamToCopy);
						// converts buffer from Naudio 16bit format to short format
						int totalSample = downsampleStream.Length / 2;
						short[] buf = new short[totalSample];
						Buffer.BlockCopy(downsampleStream, 0, buf, 0, downsampleStream.Length);
						//convert short format to ASR_DAT readable format
						msg = new ArraySegment<byte>(shortArrayToByteArrayInNetworkOrder_ASR(buf));
					}
					break;
			}
			return msg;
		}

        #region helper_function
		/// <summary>
		/// Taken from Dat's Documentation
		/// </summary>
		/// <param name="audio"></param>
		/// <returns></returns>
        public static byte[] shortArrayToByteArrayInNetworkOrder_ASR(short[] audio)
		{
			byte[] byteArray = new byte[1 + audio.Length * 2];
			byteArray[0] = 2;
			for (int i = 0; i < audio.Length; i++)
			{
				short s = System.Net.IPAddress.HostToNetworkOrder(audio[i]);
				byte[] b = BitConverter.GetBytes(s);
				byteArray[1 + i * 2] = b[0];
				byteArray[1 + i * 2 + 1] = b[1];
			}
			return byteArray;
		}
		private static byte[] stringToByteArray(string data)
		{
			var encoded = Encoding.UTF8.GetBytes(data);
			return encoded;
		}

		public byte[] downsampleAudio(byte[] byteArray)
		{
			byte[] newArray16Bit = new byte[byteArray.Length / 2];
			short two;
			float value;
			for (int i = 0, j = 0; i < byteArray.Length; i += 4, j += 2)
			{
				value = (BitConverter.ToSingle(byteArray, i));
				two = (short)(value * short.MaxValue);

				newArray16Bit[j] = (byte)(two & 0xFF);
				newArray16Bit[j + 1] = (byte)((two >> 8) & 0xFF);
			}


			return newArray16Bit;
		}

		#endregion
	}

}