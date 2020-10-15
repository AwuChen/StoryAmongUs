using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

/// <summary>
///Manage Network player if isLocalPlayer variable is false
/// or Local player if isLocalPlayer variable is true.
/// </summary>
public class PlayerManager : MonoBehaviour {

    public string id;

    public string name;

    public string avatar;

    public bool isOnline;

    public bool isLocalPlayer;

    //Animator myAnim;

    //Rigidbody myRigidbody;

    public enum state : int { idle, walk, attack, damage, dead };//cada estado representa um estado do inimigo

    public state currentState;

    public float verticalSpeed = 3f;

    public float rotateSpeed = 60f;

    //distances low to arrive close to the player
    [Range(1f, 200f)] [SerializeField] float minDistanceToPlayer = 10f;

    public bool onGrounded;

    [SerializeField] float m_GroundCheckDistance = 1f;

    public float jumpPower = 12f;

    public float jumpTime = 0.4f;

    public float jumpdelay = 0.4f;

    public bool m_jump;

    public bool isJumping;

    public float lastVelocityX = 0f;

    public Transform cameraTotarget;

    public bool isAtack;

    // START OF MONUMENTO

    public bool walking = false;

    [Space]

    public Transform currentCube;
    public Transform clickedCube;
    public Transform indicator;

    [Space]

    public List<Transform> finalPath = new List<Transform>();

    private float blend;

    public SkinnedMeshRenderer[] playerMR;

    [Space]

    public GameObject[] mojis;
    int mojiCount = 0;
    public Vector3 testPos;
    bool runOnce = false;
    Transform cubeTrans;

    [Space]

    int intSpaceCount = 0;

    void Start()
    {
        RayCastDown();
        if (isLocalPlayer)
        {
            gameObject.tag = "LocalPlayer";
        }
        else
        {
            gameObject.tag = "NetworkPlayer";
        }
        //UpdatePosition(testPos);
    }
    // Use this for initialization
    void Awake () {

		//myAnim = GetComponent<Animator>();
		//myRigidbody = GetComponent<Rigidbody> ();
		//lastVelocityX = myRigidbody.velocity.x;

	}

	public void Set3DName(string name)
	{
		GetComponentInChildren<TextMesh> ().text = name;

	}

	// Update is called once per frame
	void FixedUpdate () {
		//Turning ();

		if (isLocalPlayer) {

			//Atack ();
			Move ();
            Moji();
		}
        else
        {
            // added this so that net player will always be raycasting and parenting 
            //RayCastDown();

            
            //if (myRigidbody.velocity.x != lastVelocityX)
            //{
            //    lastVelocityX = myRigidbody.velocity.x;
            //    //UpdateAnimator("idle");
            //}
            //else
            //{
            //    UpdateIdle();
            //}
        }
        //test 
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    UpdatePosition(testPos);
        //}

        //Move();

        //Jump ();



    }

    public void RayCastDown()
    {

        Ray playerRay = new Ray(transform.GetChild(0).position, -transform.up);
        RaycastHit playerHit;

        if (Physics.Raycast(playerRay, out playerHit))
        {
            if (playerHit.transform.GetComponent<Walkable>() != null)
            {
                currentCube = playerHit.transform;

                if (playerHit.transform.GetComponent<Walkable>().isStair)
                {
                    DOVirtual.Float(GetBlend(), blend, .1f, SetBlend);
                }
                else
                {
                    DOVirtual.Float(GetBlend(), 0, .1f, SetBlend);
                }
            }
        }
    }

    void FindPath()
    {
        List<Transform> nextCubes = new List<Transform>();
        List<Transform> pastCubes = new List<Transform>();
        // here is where the null ref is found (currentcube) 
        // think we need to have a raycast down before this 
        // ray cast down is not giving our currentCube for older networked players 
        if (currentCube != null)
        {
            foreach (WalkPath path in currentCube.GetComponent<Walkable>().possiblePaths)
            {
                if (path.active)
                {
                    nextCubes.Add(path.target);
                    path.target.GetComponent<Walkable>().previousBlock = currentCube;
                }
            }
            pastCubes.Add(currentCube);
            if (clickedCube != null)
            {
                ExploreCube(nextCubes, pastCubes);
                BuildPath();
            }
            else
            {
                Debug.Log("CLICKED CUBE IS NULL");
            }
        }else
        {
            Debug.Log("CURRENT CUBE = NULL");
        }
    }

    void ExploreCube(List<Transform> nextCubes, List<Transform> visitedCubes)
    {
        Transform current = nextCubes.First();
        nextCubes.Remove(current);
        if (current == clickedCube)
        {
            return;
        }
        foreach (WalkPath path in current.GetComponent<Walkable>().possiblePaths)
        {
            if (!visitedCubes.Contains(path.target) && path.active)
            {
                nextCubes.Add(path.target);
                path.target.GetComponent<Walkable>().previousBlock = current;
            }
        }
        visitedCubes.Add(current);
        if (nextCubes.Any())
        {
            ExploreCube(nextCubes, visitedCubes);
        }
    }

    void BuildPath()
    {
        Transform cube = clickedCube;
        // added this to fix issue with player ghosting
        if (isLocalPlayer)
        {
            UpdateStatusToServer(clickedCube);
        }
        while (cube != currentCube)
        {
            finalPath.Add(cube);
            if (cube.GetComponent<Walkable>().previousBlock != null)
                cube = cube.GetComponent<Walkable>().previousBlock;
            else
                return;
        }
        finalPath.Insert(0, clickedCube);
        FollowPath();
        
    }

    void FollowPath()
    {
        Sequence s = DOTween.Sequence();
        walking = true;
        for (int i = finalPath.Count - 1; i > 0; i--)
        {
            float time = finalPath[i].GetComponent<Walkable>().isStair ? 1.5f : 1;

            s.Append(transform.DOMove(finalPath[i].GetComponent<Walkable>().GetWalkPoint(), .2f * time).SetEase(Ease.Linear));

            if (!finalPath[i].GetComponent<Walkable>().dontRotate)
                s.Join(transform.DOLookAt(finalPath[i].position, .1f, AxisConstraint.Y, Vector3.up));
        }
        if (clickedCube.GetComponent<Walkable>().isButton)
        {
            if (clickedCube.name == "Surprise")
            {
                s.AppendCallback(() => GM.instance.ActivateEvent());
            }
            if (clickedCube.name == "Key")
            {
                s.AppendCallback(() => GM.instance.ActivateKey());
            }
            if (clickedCube.name == "17button")
            {
                s.AppendCallback(() => GM.instance.RotateRightPivot());
            }
        }
        s.AppendCallback(() => Clear());
    }

    void Clear()
    {
        foreach (Transform t in finalPath)
        {
            t.GetComponent<Walkable>().previousBlock = null;
        }
        finalPath.Clear();
        walking = false;
        runOnce = false;
        //GetComponent<BoxCollider>().enabled = true;
        //added this for network player 
        RayCastDown();
        if (currentCube.GetComponent<Walkable>().movingGround)
        {
            transform.parent = currentCube.parent;
        }
        else
        {
            transform.parent = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Ray ray = new Ray(transform.GetChild(0).position, -transform.up);
        Gizmos.DrawRay(ray);
    }

    float GetBlend()
    {
        return GetComponentInChildren<Animator>().GetFloat("Blend");
    }
    void SetBlend(float x)
    {
        GetComponentInChildren<Animator>().SetFloat("Blend", x);
    }

    // end of MONUMENTO

 //   void Atack()
	//{
	//	if (isLocalPlayer)
	//	{
	//		//user press A keyboard button or not
	//		isAtack = Input.GetKey (KeyCode.A);


	//		if (isAtack)
	//		{
	//			currentState = state.attack;
	//			UpdateAnimator ("IsAtack");
	//			string msg = id;
	//			NetworkManager.instance.EmitAttack(msg);//call method NetworkSocketIO.EmitPosition for transmit new  player position to all clients in game

	//			foreach(KeyValuePair<string, PlayerManager> enemy in NetworkManager.instance.networkPlayers)
	//			{

	//				if ( enemy.Key != id)
	//				{
	//					//calcula o vetor distancia de mim até o player
	//					Vector3 meToEnemy = transform.position - enemy.Value.transform.position;
	//					Debug.Log ("meToEnemy.sqrMagnitude: "+meToEnemy.sqrMagnitude);
	//					//if i am close to player
	//					if (meToEnemy.sqrMagnitude < minDistanceToPlayer)
	//					{


	//						NetworkManager.instance.EmitPhisicstDamage (id, enemy.Key);
	//					}
	//				}
	//			}
	//		}

	//	}
	//}

	void Move( )
	{
        //// read inputs
        ////float  h = CrossPlatformInputManager.GetAxis ("Horizontal");
        ////float  v = CrossPlatformInputManager.GetAxis ("Vertical");

        //bool move = false;
        //if (Input.GetKey("up"))//up button or joystick
        //{
        //  move = true;
        //  transform.Translate (new Vector3 (0, 0, 1 * verticalSpeed * Time.deltaTime));
        ////  UpdateAnimator("run");
        //}
        //if (Input.GetKey("down"))//down button or joystick
        //{
        //	move = true;
        //	transform.Translate (new Vector3 (0, 0, -1 * verticalSpeed * Time.deltaTime));
        //	//UpdateAnimator("run");
        //}


        //if (Input.GetKey ("right")) {//right button or joystick
        //	move = true;
        //	this.transform.Rotate (Vector3.up, rotateSpeed * Time.deltaTime);
        //}
        //if (Input.GetKey ("left")) {//left button or joystick
        //	move = true;
        //	this.transform.Rotate (Vector3.up, -rotateSpeed * Time.deltaTime);
        //}


        //if (move || isJumping)
        //{
        //    //currentState = state.walk;
        //    //UpdateAnimator("IsWalk");
        //    Debug.Log("Right before Update Status to Server");
        //    UpdateStatusToServer();
        //    Debug.Log("Right after Update Status to Server");
        //}
        //else
        //{
        //    //currentState = state.idle;
        //    //UpdateAnimator("IsIdle");
        //}

        //GET CURRENT CUBE (UNDER PLAYER)

        RayCastDown();

        if (currentCube.GetComponent<Walkable>().movingGround)
        {
            transform.parent = currentCube.parent;
        }
        else
        {
            transform.parent = null;
        }

        // CLICK ON CUBE

        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition); RaycastHit mouseHit;

            if (Physics.Raycast(mouseRay, out mouseHit))
            {
                if (mouseHit.transform.GetComponent<Walkable>() != null)
                {
                    //GetComponent<BoxCollider>().enabled = false;
                    clickedCube = mouseHit.transform;
                    DOTween.Kill(gameObject.transform);
                    finalPath.Clear();
                    FindPath();
                    blend = transform.position.y - clickedCube.position.y > 0 ? -1 : 1;
                    Transform indicatorC;
                    indicatorC = GameObject.Instantiate(indicator);
                    indicatorC.position = mouseHit.transform.GetComponent<Walkable>().GetWalkPoint();
                    Sequence s = DOTween.Sequence();
                    s.AppendCallback(() => indicatorC.GetComponentInChildren<ParticleSystem>().Play());
                    s.Append(indicatorC.GetComponent<Renderer>().material.DOColor(Color.white, .1f));
                    s.Append(indicatorC.GetComponent<Renderer>().material.DOColor(Color.black, .3f).SetDelay(.2f));
                    s.Append(indicatorC.GetComponent<Renderer>().material.DOColor(Color.clear, .3f));
                    Destroy(((indicatorC as Transform).gameObject), 1);
                    //if (clickedCube != null)
                    //{
                    //    cubeTrans = clickedCube;
                    //    UpdateStatusToServer(cubeTrans);
                    //}
                }
                //else if (mouseHit.transform.GetComponent<Walkable>(). != null)
            }
        }
        else
        {
            //  currentState = state.idle;
            //	UpdateAnimator ("IsIdle");
        }
    }

    void Moji()
    {
        RayCastDown();

        // CLICK ON Player

        if (Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition); RaycastHit mouseHit;

            if (Physics.Raycast(mouseRay, out mouseHit))
            {
                if (mouseHit.transform.GetComponent<PlayerManager>() != null)
                {
                    if (mojiCount < mojis.Length - 1)
                    {
                        mojis[mojiCount].SetActive(false);
                        mojiCount++;
                    }else
                    {
                        mojis[mojiCount].SetActive(false);
                        mojiCount = 0;
                    }
                    mojis[mojiCount].SetActive(true);
                    UpdateStatusToServer(transform);
                }
            }
        }
        
    }

    public void Interact(string obj)
    {

        print("interacted");
        if (obj == "CPK")
            intSpaceCount = 1;
        else if (obj == "vinyl")
            intSpaceCount = 2;
        else if (obj == "puzzle")
            intSpaceCount = 3;
        //else if (obj == "art")
        //    intSpaceCount = 4;
        //else if (obj == "1")
        //    intSpaceCount = 5;
        //else if (obj == "-1")
        //    intSpaceCount = 6;
        UpdateStatusToServer(transform);

    }


    void UpdateStatusToServer (Transform cube)
	{
        Debug.Log("Right at the start of Update Status to Server");

        if(NetworkManager.instance == null)
        {
            Debug.Log("NetworkManager is null");
        }
        //hash table <key, value>
        Dictionary<string, string> data = new Dictionary<string, string>();

		data["local_player_id"] = id;

        data["position"] = cube.position.x + "," + cube.position.y + "," + cube.position.z;

		data["rotation"] = transform.rotation.x+","+transform.rotation.y+","+transform.rotation.z+","+transform.rotation.w;

        data["moji"] = mojiCount.ToString();

        data["interact"] = intSpaceCount.ToString();

        NetworkManager.instance.EmitMoveAndRotate(data);//call method NetworkSocketIO.EmitPosition for transmit new  player position to all clients in game
        print("updatedPos");
        Debug.Log("Right at the end of Update Status to Server");

    }


	public void UpdateIdle()
	{

		currentState = state.idle;
		//UpdateAnimator ("IsIdle");

	}

    public void UpdatePosition(Vector3 position)
    {

        if (!isLocalPlayer)
        {
            //if (currentCube.GetComponent<Walkable>().movingGround)
            //{
            //    transform.parent = currentCube.parent;
            //}
            //else
            //{
            //    transform.parent = null;
            //}

            currentState = state.walk;
            //UpdateAnimator ("IsWalk");

            // works fine with multiple users 


            //somehow the player who joined earlier, their movement will not be updated to the player who joined later 
            //player who joined late can see the network player stuck at their initial spawn point, these network players are having a nullreference whenever they receive input to move
            //the update doesnt come through below 

            // this is because of the history error 
            // if all player wait until everyone is here then begin to move then there is no issue 
            //RayCastDown();
            

            Vector3 downwardPlayer = transform.TransformDirection(Vector3.down);
            Vector3 targetPositionPlayer = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
            RaycastHit playerHit;

            if (Physics.Raycast(targetPositionPlayer, downwardPlayer, out playerHit))
            {
                if (playerHit.transform.GetComponent<Walkable>() != null)
                {
                    currentCube = playerHit.transform;
                    Debug.Log("currentCube is playerhit.transform");
                    //if (playerHit.transform.GetComponent<Walkable>().isStair)
                    //{
                    //    DOVirtual.Float(GetBlend(), blend, .1f, SetBlend);
                    //}
                    //else
                    //{
                    //    DOVirtual.Float(GetBlend(), 0, .1f, SetBlend);
                    //}
                }
            }


            Vector3 downward = transform.TransformDirection(Vector3.down);
            RaycastHit targetBlock;
            Vector3 targetPosition = new Vector3(position.x, position.y + 3f, position.z);
            Physics.Raycast(targetPosition, downward, out targetBlock);
            Debug.Log("found cube at " + position);
            clickedCube = targetBlock.transform;
            //DOTween.Kill(gameObject.transform);
            //finalPath.Clear();
            if (currentCube != null)
            {
                
                if (currentCube != clickedCube && !runOnce)
                {
                    runOnce = true;
                    //added
                    DOTween.Kill(gameObject.transform);
                    finalPath.Clear();
                    //added 
                    FindPath();
                    blend = transform.position.y - position.y > 0 ? -1 : 1;
                }
                else
                {
                    // added this awu 
                    runOnce = false;
                    print(currentCube + " == " + clickedCube);
                }
            }
            else
            {
                Debug.Log("CURRENT CUBE is null");
                //transform.position = new Vector3(position.x, position.y, position.z);
            }
        }

    }

	public void UpdateRotation(Quaternion _rotation)
	{
        if (!isLocalPlayer)
        {
            //transform.rotation = _rotation;

        }

    }

    public void UpdateMoji(int mojiCount)
    {
        if (!isLocalPlayer)
        {
            for(int i = 0; i < mojis.Length; i++)
            {
                mojis[i].SetActive(false);
            }
            mojis[mojiCount].SetActive(true);
        }

    }




    public void UpdateAnimator(string _animation)
	{


		//switch (_animation) {


		//case "IsWalk":
		//	if (!myAnim.GetCurrentAnimatorStateInfo (0).IsName ("Walk"))
		//	{
		//		myAnim.SetTrigger ("IsWalk");

		//	}
		//	break;

		//case "IsIdle":

		//	if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		//	{
		//		myAnim.SetTrigger ("IsIdle");

		//	}
		//	break;

		//case "IsDamage":
		//	if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Damage") )
		//	{
		//		myAnim.SetTrigger ("IsDamage");
		//	}
		//	break;

		//case "IsAtack":
		//	if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Atack"))
		//	{
		//		myAnim.SetTrigger ("IsAtack");
		//	}
		//	break;


		//case "IsDead":
		//	if (!myAnim.GetCurrentAnimatorStateInfo(0).IsName("Dead"))
		//	{
		//		myAnim.SetTrigger ("IsDead");
		//	}
		//	break;

		//}//END_SWITCH


	}


	public void UpdateJump()
	{
		//m_jump = true;
	}

	public void Jump()
	{
		//RaycastHit hitInfo;

		//onGrounded = Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance);

		//jumpTime -= Time.deltaTime;

		//if (isLocalPlayer)
		//{
		//	m_jump = Input.GetKey("space");
		//}

		//// se ja deu o tempo de pulo e o player esta colidindo com o chão e ele  estava pulando
		//if (jumpTime <= 0 && isJumping && onGrounded)
		//{

		//	m_jump = false;
		//	isJumping = false;//marca que o player não esta pulando
		//}


		////verifica se o usuario apertou espaco e ele ja não esta pulando ou se o player esta na lona e ja não esta pulando
		//if (m_jump && !isJumping)
		//{

		//	//efeito do pulo
		//	myRigidbody.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
		//	//calcula o tempo de pulo
		//	jumpTime = jumpdelay;
		//	//marca que o player esta pulando
		//	isJumping = true;
		//	//UpdateAnimator("jump");

		//}

	}

    public void HidePlayer(bool hide)
    {
        for (int i = 0; i < playerMR.Length; i++)
        {
            playerMR[i].enabled = !hide;
        }
    }
}
