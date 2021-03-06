using UnityEngine;
using System.Collections;

public class PlayerInputController : InputController
{
    private float prevThrottle;
    public float PrevThrottle
    {
        get
        {
            return prevThrottle;
        }
        set
        {
            prevThrottle = value;
        }
    }
    private float prevPitch;
    public float PrevPitch
    {
        get
        {
            return prevPitch;
        }
        set
        {
            prevPitch = value;
        }
    }
    private float prevRoll;
    public float PrevRoll
    {
        get
        {
            return prevRoll;
        }
        set
        {
            prevRoll = value;
        }
    }
    private float prevYaw;
    public float PrevYaw
    {
        get
        {
            return prevYaw;
        }
        set
        {
            prevYaw = value;
        }
    }
    private float prevStrafe;
    public float PrevStrafe
    {
        get
        {
            return prevStrafe;
        }
        set
        {
            prevStrafe = value;
        }
    }
    private float prevJump;
    public float PrevJump
    {
        get
        {
            return prevJump;
        }
        set
        {
            prevJump = value;
        }
    }

    public Ray ray;
    public RaycastHit hit;
    public Vector3 TargetPosition = Vector3.zero;
    public Transform TargetTransfrom;
	
    public bool driving = false;
	private bool enterJustPressed;

    private GameObject mapCamera;
    private GameObject mainCamera;
	
    void Awake()
    {
        PlayerControlled = true;
    }

	// Use this for initialization
	void Start () 
    {
        turret = GetComponentInChildren<TurretScript>();
        hull = GetComponentInChildren<Hovercraft>();
        mapCamera = GameObject.FindWithTag("MapCamera");
        mainCamera = GameObject.FindWithTag("MainCamera");
        //Manager.Instance.gameOver = false;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        if (!Manager.Instance.gameOver)
        {
            if (!hull.Dead && driving/* && Camera.current == mainCamera.camera*/ /*mainCamera*/)//mainCamera.camera.enabled)
            {
                //Debug.Log(Camera.current);
                if (Camera.current != null)
                {
                    ray = Camera.current.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, 200000.0f)) //, 1 << 9
                    {
                        TargetPosition = hit.point;
                        turret.TargetPosition = TargetPosition; //FinalTargetPosition
                        //hit.normal;
                        //Debug.Log("Hit something at: " + hit.point);
                    }
                }
            }
            else
            {
                //Debug.Log("Hull dead?: " + hull.Dead + " Driving?: " + driving + " Camera??: " + Camera.current);
            }
        }
        /*prevJump = Jump;
        prevPitch = Pitch;
        prevRoll = Roll;
        prevStrafe = Strafe;
        prevThrottle = Throttle;
        prevYaw = Yaw;*/
	}

    void Update()
    {
        if (!Manager.Instance.gameOver)
        {
            enterJustPressed = false;
            if (mainCamera.GetComponent<MainCameraScript>().typing == false)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    mainCamera.GetComponent<MainCameraScript>().typing = true;
                    enterJustPressed = true;
                }

                Throttle = Input.GetAxis("Vertical");
                Yaw = Input.GetAxis("Horizontal");
                Jump = Input.GetAxis("Jump");
                PrimaryFire = Input.GetButton("Fire1");
                SecondaryFire = Input.GetButton("Fire2");
                Strafe = Input.GetAxis("Strafe");

                //Code for map and respawn
                //mapCamera = GameObject.FindWithTag("MapCamera");

                if (Input.GetButton("Map"))
                {
                    driving = false;
                    mapCamera.camera.enabled = true;
                    mainCamera.camera.enabled = false;
                    Screen.showCursor = false;

                    /*RaycastHit hit;
                    if(Input.GetMouseButtonDown(0))
                    {	
                        if(Physics.Raycast(mapCamera.camera.ScreenPointToRay(Input.mousePosition), out hit))
                        {
                            Vector3 pos = new Vector3(hit.point.x, 2000, hit.point.z);
                            gameObject.GetComponent<Hovercraft>().respawn(pos);
                            Manager.Instance.sendSpawnData(pos);
                            Manager.Instance.Spawned = true;
                        }
                    }*/
                }
                else
                {
                    driving = true;
                    mapCamera.camera.enabled = false;
                    mainCamera.camera.enabled = true;
                    Screen.showCursor = true;
                }
            }

            if (Input.GetKeyDown(KeyCode.Return) && !enterJustPressed)
            {
                mainCamera.GetComponent<MainCameraScript>().sendMsg();
            }

            if (Input.GetKey(KeyCode.Tab))
            {
                mainCamera.GetComponent<MainCameraScript>().displayScores = true;
            }
            else
            {
                mainCamera.GetComponent<MainCameraScript>().displayScores = false;
            }
        }
        else
        {
            mainCamera.GetComponent<MainCameraScript>().typing = true;
            mainCamera.GetComponent<MainCameraScript>().displayScores = true;
        }
    }
}
