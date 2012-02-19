using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
 
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
using Sfs2X.Requests;
using Sfs2X.Exceptions;

public class Manager : MonoBehaviour 
{
	private SmartFox smartFox; 
	private TestLobby lobby;
	private Room currentRoom;
    private bool running = false; //Can I delete it mommy? huh? huh? Can I delete it? pleaseeeeeeeeeeee?
	private GameObject[] PhysObjects;

    private PlayerInputController localController;

	public String myId;
	
	public GameObject daTank;
	
	private string clientName;
	public string ClientName 
    {
		get { return clientName;}
	}
	
	private static Manager instance;
	public static Manager Instance 
    {
		get { return instance;}
	}
	
	private static bool isPhysAuth;
	public bool IsPhysAuth{
		get { return isPhysAuth; }	
	}

    public float TimeBetweenUpdates = 0.2f;
    private float LastUpdateTime = 0;
    private uint ObjectSent = 0;

	void Awake() 
    {
		instance = this;
	}
	
	void Start () 
    {
		running = true; //*Resisting the urge to delete*
		if (SmartFoxConnection.IsInitialized)
        {
			smartFox = SmartFoxConnection.Connection;
		}
		else
        {
            smartFox = new SmartFox(Application.isEditor);
		}
		
		currentRoom = smartFox.LastJoinedRoom;
		clientName = smartFox.MySelf.Name;

		if(currentRoom.UserCount == 1)
        {
			isPhysAuth = true;	
		}
		else
        {
			isPhysAuth = false;	
		}

		updatePhysList();

        localController = GetLocalController();
		
		myId = smartFox.MySelf.Id.ToString();
		
		smartFox.AddEventListener(SFSEvent.USER_ENTER_ROOM, OnUserEnterRoom);
		smartFox.AddEventListener(SFSEvent.USER_EXIT_ROOM, OnUserLeaveRoom);
		smartFox.AddEventListener(SFSEvent.USER_COUNT_CHANGE, OnUserCountChange);
		smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		smartFox.AddEventListener(SFSEvent.OBJECT_MESSAGE, OnObjectMessageReceived);
		smartFox.AddEventListener(SFSEvent.USER_VARIABLES_UPDATE, OnUserVariablesUpdate); 
		smartFox.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariablesUpdate);
        //smartFox.AddEventListener(SFSEvent.UDP_INIT, OnUDPInit);
		//smartFox.AddEventListener(SFSEvent.EXTENSION_RESPONSE, onExtensionResponse);
        //smartFox.InitUDP("129.21.29.6", 9933); //FIX THIS: Should be dynamic ========================================
	}

    void OnUDPInit(BaseEvent evt) 
    {
        if ((bool)evt.Params["success"]) 
        {
            // Execute an extension call via UDP
            smartFox.Send(new ExtensionRequest("udpTest", new SFSObject(), null, true));
        } 
        else 
        {
            Console.WriteLine("UDP init failed!");
        }
    }
	
	void FixedUpdate () 
    {
		//if (!running) return; <<< It is impossible for this to return false... Additionally, bad flow control Ahoy!
		smartFox.ProcessEvents();
		updatePhysList();

        sendInputs();

        if (isPhysAuth)
        {
            if (LastUpdateTime >= TimeBetweenUpdates)
            {
                if (ObjectSent < PhysObjects.Length)
                    ObjectSent = 0;
                sendTelemetry(PhysObjects[ObjectSent]);
                ObjectSent++;

                LastUpdateTime = 0;
            }
            LastUpdateTime += Time.deltaTime;
        }
	}
	
	public void OnUserEnterRoom (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];

        GameObject tank = (GameObject)Instantiate(daTank, new Vector3(2151.378f, 36.38875f, 2893.621f), Quaternion.identity);
        InputController ic = tank.GetComponent<InputController>();
		ic.id = user.Id.ToString();  //ID Schema: UserId + Type + InstanceNumber
		
		Debug.Log ("user entered room " + user.Name + " with id of " + user.Id);
	}
	
	public void OnUserLeaveRoom (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];
	}
	
	public void OnUserCountChange (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];
		
		Debug.Log ("User count change based on " + user.Name + " with user Id of " + user.Id);
	}
	
	public void OnConnectionLost (BaseEvent evt)
    {
		smartFox.RemoveAllEventListeners();
	}

    public void OnObjectMessageReceived(BaseEvent evt) //You do not recieve these messages from yourself
    {
		User sender = (User)evt.Params["sender"];
		ISFSObject obj = (SFSObject)evt.Params["message"];
        NetInputController remoteController;

        if (obj.GetUtfString("PID") == myId)
        {
            //localController;
            if (obj.ContainsKey("PhysMaster"))
            {
                if (obj.GetBool("PhysMaster"))
                {
                    //remoteController = GetRemoteController(obj.GetUtfString("PID"));

                    //localController.Extrapolate();

                    localController.Hull.transform.position = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz"));

                    localController.Hull.transform.rotation = Quaternion.Euler(new Vector3(obj.GetFloat("rx"), obj.GetFloat("ry"), obj.GetFloat("rz")));

                    localController.Hull.rigidbody.velocity = new Vector3(obj.GetFloat("vx"), obj.GetFloat("vy"), obj.GetFloat("vz"));

                    //remoteController.Hull.rigidbody.angularVelocity = new Vector3(obj.GetFloat("ax"), obj.GetFloat("ay"), obj.GetFloat("az"));

                    //localController.TimeSinceLastUpdate = Time.time;
                }
            }

        }
        else if (GetRemoteController(obj.GetUtfString("PID")) != null)
        {
            if (obj.ContainsKey("PhysMaster"))
            {
                if (obj.GetBool("PhysMaster"))
                {
                    remoteController = GetRemoteController(obj.GetUtfString("PID"));

                    remoteController.Extrapolate();

                    remoteController.Hull.transform.position = remoteController.LastPosition = new Vector3(obj.GetFloat("px"), obj.GetFloat("py"), obj.GetFloat("pz")) + remoteController.PositionExtrapolation;

                    remoteController.Hull.transform.rotation = Quaternion.Euler(remoteController.LastRotation = new Vector3(obj.GetFloat("rx"), obj.GetFloat("ry"), obj.GetFloat("rz")) + remoteController.RotationExtrapolation);

                    remoteController.Hull.rigidbody.velocity = new Vector3(obj.GetFloat("vx"), obj.GetFloat("vy"), obj.GetFloat("vz"));
                    
                    //remoteController.Hull.rigidbody.angularVelocity = new Vector3(obj.GetFloat("ax"), obj.GetFloat("ay"), obj.GetFloat("az"));

                    remoteController.TimeSinceLastUpdate = Time.time;
                }
            }

            if (obj.ContainsKey("inputs"))
            {
                remoteController = GetRemoteController(obj.GetUtfString("PID"));
                if (obj.ContainsKey("iT"))
                    remoteController.Throttle = obj.GetFloat("iT");
                if (obj.ContainsKey("iP"))
                    remoteController.Pitch = obj.GetFloat("iP");
                if (obj.ContainsKey("iR"))
                    remoteController.Roll = obj.GetFloat("iR");
                if (obj.ContainsKey("iY"))
                    remoteController.Yaw = obj.GetFloat("iY");
                if (obj.ContainsKey("iS"))
                    remoteController.Strafe = obj.GetFloat("iS");
                if (obj.ContainsKey("iJ"))
                    remoteController.Jump = obj.GetFloat("iJ");
            }
        }
	}
	
	public void OnUserVariablesUpdate (BaseEvent evt)
    {
		User user = (User)evt.Params["user"];
	}
	
	public void OnRoomVariablesUpdate (BaseEvent evt)
    {
		Room room = (Room)evt.Params["room"];
	}
	
	public void onExtensionResponse(BaseEvent evt)
    {
		
	}
	
	private void sendTelemetry(GameObject gO)
    {
        SFSObject myData = new SFSObject();
        
        myData.PutUtfString("PID", myId);
        myData.PutBool("PhysMaster", isPhysAuth);

        myData.PutFloat("px", gO.transform.position.x);
        myData.PutFloat("py", gO.transform.position.y);
        myData.PutFloat("pz", gO.transform.position.z);

        myData.PutFloat("rx", gO.transform.rotation.eulerAngles.x);
        myData.PutFloat("ry", gO.transform.rotation.eulerAngles.y);
        myData.PutFloat("rz", gO.transform.rotation.eulerAngles.z);

        myData.PutFloat("vx", gO.rigidbody.velocity.x);
        myData.PutFloat("vy", gO.rigidbody.velocity.y);
        myData.PutFloat("vz", gO.rigidbody.velocity.z);

        smartFox.Send(new ObjectMessageRequest(myData));
	}

    private void sendInputs()
    {
        if ((localController.PrevThrottle != localController.Throttle) || (localController.Pitch != localController.Pitch) || 
            (localController.PrevRoll != localController.Roll) || (localController.PrevYaw != localController.Yaw) || 
            (localController.PrevStrafe != localController.Strafe) || (localController.PrevJump != localController.Jump))
        {
            SFSObject myData = new SFSObject();

            myData.PutUtfString("PID", myId);

            myData.PutBool("inputs", true);

            if (localController.PrevThrottle != localController.Throttle)
            {
                myData.PutFloat("iT", localController.Throttle);
                localController.PrevThrottle = localController.Throttle;
            }
            if (localController.Pitch != localController.Pitch)
            {
                myData.PutFloat("iP", localController.Pitch);
                localController.PrevPitch = localController.Pitch;
            }
            if (localController.PrevRoll != localController.Roll)
            {
                myData.PutFloat("iR", localController.Roll);
                localController.PrevRoll = localController.Roll;
            }
            if (localController.PrevYaw != localController.Yaw)
            {
                myData.PutFloat("iY", localController.Yaw);
                localController.PrevYaw = localController.Yaw;
            }
            if (localController.PrevStrafe != localController.Strafe)
            {
                myData.PutFloat("iS", localController.Strafe);
                localController.PrevStrafe = localController.Strafe;
            }
            if (localController.PrevJump != localController.Jump)
            {
                myData.PutFloat("iJ", localController.Jump);
                localController.PrevJump = localController.Jump;
            }

            smartFox.Send(new ObjectMessageRequest(myData));
        }
    }
	
	private void updatePhysList() // This is a very expensive operation, it should only be called when a relevant object is created/destroyed
    {
        List<GameObject> PhysObjs = new List<GameObject>();
		//PhysObjects = GameObject.FindGameObjectsWithTag("PhysObj");
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("PhysObj"))
        {
            if (!g.GetComponent<NetTag>())
                g.AddComponent<NetTag>();
            PhysObjs.Add(g);
        }
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
            PhysObjs.Add(p);
        PhysObjects = PhysObjs.ToArray();
	}

    private PlayerInputController GetLocalController() //Should probably check to see if we are a spectator first...
    {
        Debug.Log(PhysObjects.ToString());
        foreach (GameObject g in PhysObjects)
        {
            if (g.GetComponent<PlayerInputController>())
            {
                return g.GetComponent<PlayerInputController>();
            }
        }
        return null;
    }

    private NetInputController GetRemoteController(string id)
    {
        foreach (GameObject g in PhysObjects)
        {
            if (g.GetComponent<NetInputController>() && g.GetComponent<NetInputController>().id == id)
            {
                return g.GetComponent<NetInputController>();
            }
        }
        return null;
    }
}