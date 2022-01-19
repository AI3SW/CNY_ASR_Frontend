using System.Threading.Tasks; // Task, is an object that handles threads, in essence its the same as a Coroutine
using System;

public interface ASR_UploadandReceive 
{

    /// <summary>
    /// Task object is that holds nothing / void, so you will just need to do "return;"
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    Task recordAudio(float duration);

    //file send success or fail
    /// <summary>
    /// Task<bool> is a thread that hold a bool, so you will need to return the bool
    /// </summary>
    /// <returns></returns>
    Task<bool> ConnectandSend();

    //
    /// <summary>
    /// the event that end users will register to.
    /// @TODO, you will need to invoke it when the string is returned to you. 
    /// IE: On_ReceiveASR_Results?.invoke(data);
    /// </summary>
    event Action<string> On_ReceiveASR_Results;

    /// <summary>
    /// for debuging purposes in realtime, to know what is the name of the model.
    /// </summary>
    /// <returns></returns>
    string GetName();

    /// <summary>
    /// future purposes when we merge live stream mode and upload mode of ASR.
    /// </summary>
    /// <returns></returns>
    SP.MODE GetMode();
}
