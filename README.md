# CNY Unity Version

Open Project with unity 2018.4.29f1

# Important Scripts to Take Note
AICUBE_ASR is the script that handle the ASR and the assigned asr server.
ASR is initiates a handshake with the server using websocket and then commeneces the communication during the duration of the game and ends it after.
As the ASR we used is using a patented technology, feel free to adjust the ASR to any online sources of ASR by making the changes here.


# Important GameObjects to Take Note
Most important sccripts are found under the system gameobject
Where you will find the 
ASR_web GO
Where the AI controller for asr_web is found

Gamemanager GO
Most of the Main logic are inside the Gamemanager script
Where you have the
CNY dictionary, for loading of the dictionary online or hardcoded style
Game Manager for most of the game logic 
UI Controller is mainly for objects that we interact with like buttons etc;

Canvas GO
The page controller is found in te Canvas game object. This script simplifies and is the interface for switching pages in the app.

BG GO
is interface to adjust and control the number of cow as well as the post processing objects.