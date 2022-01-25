using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace AICUBE.Utils
{
    namespace Websocket
    {
        public class OnStreamResultEventArgs<T> : EventArgs
        {
            public T eventData { get; set; }
            public bool parseError = false;
        }
    }


    #region HelperFunction
    public class StringUtils
    {
        public static string UTF8toUnicode(string utf8String)
        {

            // read the string as UTF-8 bytes.
            byte[] encodedBytes = Encoding.UTF8.GetBytes(utf8String);

            // convert them into unicode bytes.
            byte[] unicodeBytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, encodedBytes);

            // builds the converted string.
            return Encoding.Unicode.GetString(unicodeBytes);

        }
        public static string UnicodeToASCIIescaped(string unicodeString)
        {

            // read the string as UTF-8 bytes.
            byte[] encodedBytes = Encoding.Unicode.GetBytes(unicodeString);

            // convert them into unicode bytes.
            byte[] asciiBytes = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, encodedBytes);

            // builds the converted string.
            return Encoding.Unicode.GetString(asciiBytes);

        }
        public static string EncodeNonAsciiCharacters(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string DecodeEncodedNonAsciiCharacters(string value)
        {
            return Regex.Replace(
                value,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }
    }

    public class IOUtils
    {

        public static string[] loadStringLinesfromFile(string filepath)
        {
            if (File.Exists(Application.persistentDataPath + filepath))
            {
                //Debug.Log(Application.persistentDataPath + filepath);
                return File.ReadAllLines(Application.persistentDataPath + filepath);
            }
            else
            {
                return null;
            }
        }

        public static bool saveDataToFile(string directoryPath, string filepath, string data)
        {
            bool succesfullySaved = false;


            try
            {
                if (!File.Exists(Application.persistentDataPath + filepath))
                {
                    Directory.CreateDirectory(Application.persistentDataPath + directoryPath);
                    using ( File.Create(Application.persistentDataPath + filepath) );
                }
                File.WriteAllText(Application.persistentDataPath + filepath, data);
                succesfullySaved = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"ERROR - {ex.StackTrace}\n\n ErrorMessage - {ex.Message}");
                Exception innerE = ex;
                while ((innerE = innerE.InnerException) != null)
                {
                    Debug.LogWarning($"ERROR - {ex.InnerException.StackTrace}\n\n ErrorMessage - {ex.InnerException.Message}");
                }
                succesfullySaved = false;
            }
            return succesfullySaved;
        }
    }
    public class ErrorUtils
    {

        public static void printAllErrors(System.Exception ex)
        {
            Debug.LogWarning($"ERROR - {ex.StackTrace}\n");
            Debug.LogWarning($"ErrorMessage - {ex.Message}\n\n");
            Exception innerE = ex;
            while ((innerE = innerE.InnerException) != null)
            {
                Debug.LogWarning($"ERROR - {innerE.StackTrace}\n");
                Debug.LogWarning($"ErrorMessage - {innerE.Message}\n\n");
            }
        }
            
    }
    #endregion
}
