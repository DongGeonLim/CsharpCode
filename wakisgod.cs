using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawOutline : MonoBehaviour
{
    Material outline;

    Renderer renderers;
    List<Material> materialList = new List<Material>();

    void Start()
    {
        outline = new Material(Shader.Find("Draw/OutlineShader"));
    }

    public void VisibleOutline(GameObject go)
    {
        renderers = go.GetComponent<Renderer>();

        materialList.Clear();
        materialList.AddRange(renderers.sharedMaterials);
        materialList.Add(outline);

        renderers.materials = materialList.ToArray();
    }

    public void InvisibleOutline(GameObject go)
    {
        Renderer renderer = go.GetComponent<Renderer>();

        materialList.Clear();
        materialList.AddRange(renderer.sharedMaterials);       
        materialList.Remove(outline);

        renderer.materials = materialList.ToArray();  
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WakStatus : MonoBehaviour
{
    [Header("Walk, Run Speed")]
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WakObjectHold : MonoBehaviour
{
    [SerializeField] private GameObject system;
    private CameraCon cameraCon;
    [SerializeField] private GameObject interactExplanationUI;
    [SerializeField] private TextMeshProUGUI interactExplanationText;
    GameObject Object = null;
    GameObject beforeObject = null;
    GameObject tempObject = null; //Object = null에 대한 예외처리
    GameObject[] holdableObj;
    [SerializeField] GameObject interactionObject = null;
    Rigidbody ObjectRigid;
    Rigidbody holdableRigid;
    public Transform PlayerTransform;
    public Camera Cam;
    Vector3 CamRay;
    int layerMask = 1 << 7;
    int layerMaskForInteraction = 1 << 8;
    [SerializeField] bool isPickup = false;
    int countChild = 0;
    [SerializeField] bool ishit = false;
    public Text door_open_text;

    private float rayLength = 70f;
    private float spinPower = 70f;
    AudioSource AS;


    [SerializeField] InteractionTrigger IT;

    void Start()
    {
        if(!GameManager.tutorial&& !GameManager.production)
        {
            AS = Cam.GetComponent<AudioSource>();
            AS.Play();
        }
        cameraCon = system.GetComponent<CameraCon>();
        holdableObj = GameObject.FindGameObjectsWithTag("holdable");
    }

    void Update()
    {
        if(cameraCon.nowCam == CameraCon.CamType.wak)
        {
            CamRay = Cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));

            if (isPickup == false)
            {
                RayObjectCheck();
            }

            if (Input.GetMouseButton(0))
            {
                PickUp();
                if (Input.GetMouseButton(1))
                {
                    Spin();
                }
            }

            else if (Input.GetKeyDown(KeyCode.E))
            {
                Interaction();
            }

            if (Input.GetMouseButtonUp(0))
            {
                Drop();
            }
        }
        else
        {
            Object = null;
            interactExplanationUI.SetActive(false);
        }
    }

    void RayObjectCheck()
    {
        RaycastHit hit;
       //Debug.DrawRay(CamRay, transform.forward * rayLength, Color.red, 0.3f);

        if (Physics.Raycast(CamRay, Cam.transform.forward, out hit, rayLength, layerMask))
        {
            if (Object == null)
            {
                Object = hit.collider.gameObject;
                ObjectRigid = Object.GetComponent<Rigidbody>();
                this.GetComponent<DrawOutline>().VisibleOutline(Object);
                tempObject = Object;
                beforeObject = Object;
            }
            else
            {
                Object = hit.collider.gameObject;
                ObjectRigid = Object.GetComponent<Rigidbody>();
                tempObject = Object;
            }
            
            if (beforeObject != Object)
            {
                this.GetComponent<DrawOutline>().VisibleOutline(Object);
                this.GetComponent<DrawOutline>().InvisibleOutline(beforeObject);
                tempObject = Object;
                beforeObject = Object;
            }
            ishit = true;
        }
        else
        {
            ishit = false;
            if (Object != null && beforeObject != null)
            {
                this.GetComponent<DrawOutline>().InvisibleOutline(Object);
                this.GetComponent<DrawOutline>().InvisibleOutline(beforeObject);
                tempObject = Object;
                Object = null;
            }
        }
        if(!isPickup&&!ishit&& cameraCon.nowCam == CameraCon.CamType.wak)
        {
            if(Physics.Raycast(CamRay, Cam.transform.forward, out hit, rayLength, layerMaskForInteraction))
            {
                interactionObject = hit.collider.gameObject;
                IT = interactionObject.GetComponent<InteractionTrigger>();
                if(IT.isWorking)
                {
                    interactionObject = null;
                    interactExplanationUI.SetActive(false);
                }
                else
                {
                    interactExplanationUI.SetActive(true);
                    interactExplanationText.text = IT.explanation;
                }
            }
            else
            {
                interactionObject = null;
                interactExplanationUI.SetActive(false);
                IT = null;
            }
        }
        else if (cameraCon.nowCam != CameraCon.CamType.wak)
        {
            interactExplanationUI.SetActive(false);
        }
    }

    void PickUp()
    {
        //PlayerTransform.DetachChildren();
        if (Object != null && ishit == true)
        {
            countChild = PlayerTransform.childCount;
            
            if (countChild == 0)
            {
                Object.transform.SetParent(PlayerTransform);
                ObjectRigid.isKinematic = true;
                isPickup = true;
            }

            this.GetComponent<DrawOutline>().InvisibleOutline(Object);
        }
    }

    void Interaction()
    {
        if (!isPickup && !ishit && interactionObject != null && cameraCon.nowCam == CameraCon.CamType.wak)
        {
            if (!IT.isWorking)
            {
                IT.InteractionFunction();
            }
        }
    }

    void Drop()
    {
        if (Object != null && tempObject !=  null)
        {
            PlayerTransform.DetachChildren();
            isPickup = false;
            Object = null;
        }
        for (int i = 0; i < holdableObj.Length; i++) //큐브끼리 겹쳐진 후 무중력 상태가 유지되는 버그의 예외처리
        {
            holdableRigid = holdableObj[i].GetComponent<Rigidbody>();
            holdableRigid.isKinematic = false;
        }
        if (Object != null)
        {
           if (Object.name == ("SmartPhone")) Object.transform.eulerAngles = new Vector3(0, Object.transform.rotation.eulerAngles.y, 0);
        }
    }

    void Spin()
    {
        Object = tempObject;
        Object.transform.Rotate(0, spinPower * Time.deltaTime, 0, Space.World);

        if (Object.name == ("SmartPhone"))
        {
            Object.transform.eulerAngles = new Vector3(0, Object.transform.rotation.eulerAngles.y, 0);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WakMove : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;
    private Vector3 moveForce;

    private Transform trans;

    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float gravity;
    private float fallDelay = 0f;
  
    public float MoveSpeed
    {
        set => moveSpeed = Mathf.Max(0, value);
        get => moveSpeed;
    }

    private CharacterController characterController;

    private void Awake()
    {
        trans = GetComponent<Transform>();
        characterController = GetComponent<CharacterController>();
        //characterController.height = 0f;
    }

    void Update()
    {
        if (!characterController.isGrounded)
        {
           fallDelay += Time.deltaTime;

            if (fallDelay < 0.2f)
            {
                 moveForce.y += gravity * Time.deltaTime;
            }

            else
            {
                 moveForce.y += gravity * 1.8f * Time.deltaTime;
            }
        }

        else fallDelay = 0f;
        
        characterController.Move(moveForce * Time.deltaTime);
    }

    public void MoveTo(Vector3 direction)
    {
        direction = transform.rotation * new Vector3(direction.x, 0, direction.z);
        direction = direction.normalized;
        moveForce = new Vector3(direction.x * moveSpeed, moveForce.y, direction.z * moveSpeed);
    }

    public void Jump()
    {
       if (characterController.isGrounded)
       {
            moveForce.y = jumpForce;
       }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WakController : MonoBehaviour
{
    [Header("Input KeyCodes")]
    private KeyCode keycodeRun = KeyCode.LeftShift;
    private KeyCode keycodeJump = KeyCode.Space;
    private CharacterController characterController;
    private RotateToMoues rotateToMouse;
    private WakMove wakmove;
    private WakStatus status;
    
    static public bool overlap = true;

    //0925
    public GameObject WakHead;
    [SerializeField] private GameObject system;
    private CameraCon cameraCon;

    [SerializeField] Texture2D blackcursor;
    [SerializeField] GameObject cursorUI;

    private void Awake()
    {
        rotateToMouse = GetComponent<RotateToMoues>();
        wakmove = GetComponent<WakMove>();
        status = GetComponent<WakStatus>();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorUI.SetActive(false);
        overlap = true;

        cameraCon = system.GetComponent<CameraCon>();
    }

    static public void NoneCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        overlap = true;
    }

    void Update()
    {
        if ((cameraCon.nowCam == CameraCon.CamType.messi) && overlap)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            overlap = false;
        }
        else if ((cameraCon.nowCam == CameraCon.CamType.wak) && !overlap)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            cursorUI.SetActive(true);
            overlap = true;
        }
        else if ((cameraCon.nowCam == CameraCon.CamType.interactionCam) && overlap)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            cursorUI.SetActive(false);
            overlap = false;
        }

        if (cameraCon.nowCam == CameraCon.CamType.wak)
        {
            UpdateRotate();
            UpdateJump();
            //if (StopMove.stopMove == false) UpdateMove();
            UpdateMove();
        }
    }

    private void UpdateRotate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        rotateToMouse.UpdateRotate(mouseX, mouseY);
    }

    private void UpdateMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        if (x != 0 || z != 0)
        {
            bool isRun = false;

            if (z > 0) isRun = Input.GetKey(keycodeRun);
            wakmove.MoveSpeed = isRun == true ? status.RunSpeed : status.WalkSpeed;
        }

        wakmove.MoveTo(new Vector3(x, 0, z));
    }


    private void UpdateJump()
    {
        if (Input.GetKeyDown(keycodeJump))
        {
            wakmove.Jump();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateToMoues : MonoBehaviour
{
    [SerializeField]
    private float rotCamXAxisSpeed = 5;

    [SerializeField]
    private float rotCamYAxisSpeed = 5;

    [SerializeField]
    private float limitMinX = -80;

    [SerializeField]
    private float limitMaxX = 80;

    private float eulerAngleX;
    private float eulerAngleY;

    void Start()
    {
        eulerAngleX = 0;
        eulerAngleY = 180;
    }
    
    public void UpdateRotate(float mouseX, float mouseY)
    {
        eulerAngleY += mouseX * rotCamYAxisSpeed;
        eulerAngleX -= mouseY * rotCamXAxisSpeed;

        eulerAngleX = ClampAngle(eulerAngleX, limitMinX, limitMaxX);

        transform.rotation = Quaternion.Euler(eulerAngleX, eulerAngleY, 0);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Production : MonoBehaviour
{
    public Camera clockCam;
    public Camera fireCam;
    public Camera cardCam;
    public Camera wakCam;
    public Camera MainCam;
    [SerializeField] Camera realWakCam;

    AudioSource ASCC;
    public AudioClip ccs;
    public AudioClip dub1;
    [SerializeField] private ProductionSubtitle productionSubtitle;

    private bool gofireCam = false;
    private bool gocardCam = false;
    private bool gocwakCam = false;
    private bool audioplayed1 = false;
    private bool audioplayed2 = false;
    private float time = 0;
    private bool skip = false;


    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.production == true)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            this.ASCC = clockCam.GetComponent<AudioSource>();
            clockCam.GetComponent<AudioListener>().enabled = true;
            fireCam.GetComponent<AudioListener>().enabled = false;
            cardCam.GetComponent<AudioListener>().enabled = false;
            wakCam.GetComponent<AudioListener>().enabled = false;
            realWakCam.GetComponent<AudioListener>().enabled = false;
            clockCam.depth = 3;
            GameManager.tutorial = false;
        }
        else
        {
            clockCam.enabled = false;
            fireCam.enabled = false;
            cardCam.enabled = false;
            wakCam.enabled = false;
            clockCam.GetComponent<AudioListener>().enabled = false;
            fireCam.GetComponent<AudioListener>().enabled = false;
            cardCam.GetComponent<AudioListener>().enabled = false;
            wakCam.GetComponent<AudioListener>().enabled = false;
            realWakCam.GetComponent<AudioListener>().enabled = true;
            clockCam.depth = -9;
            clockCam.depth = -9;
            fireCam.depth = -9;
            cardCam.depth = -9;
            wakCam.depth = -9;
            MainCam.depth = -9;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.production == true)
        {
            time += Time.deltaTime;
            if (Clock_Digital.end617 == true && gofireCam == false && skip == false)
            {
                //playAudio();
                Debug.Log("1111111111");
                if (audioplayed1 == false)
                {
                    ASCC.clip = ccs;
                    ASCC.Play();
                    audioplayed1 = true;
                }

                if (time > 23f)
                {
                    fireCam.depth = 3;
                    clockCam.depth = -1;
                    if (audioplayed2 == false)
                    {
                        ASCC.Stop();
                        ASCC.clip = dub1;
                        productionSubtitle.StartProductionSubtitles();
                        ASCC.Play();
                        audioplayed2 = true;
                    }
                    gofireCam = true;
                    skip = true;
                    time = 0;
                }
            }
            else if (gofireCam == true && gocardCam == false && gocwakCam == false)
            {
                Debug.Log("22222222");
                if (time > 18f)
                {
                    cardCam.depth = 4;
                    fireCam.depth = -1;
                    gofireCam = false;
                    gocardCam = true;
                    time = 0;
                }
            }
            else if (gocardCam == true && gocwakCam == false)
            {
                Debug.Log("3333333333");
                if (time > 1.5f)
                {
                    wakCam.depth = 5;
                    cardCam.depth = -1;
                    gocardCam = false;
                    gocwakCam = true;
                    time = 0;
                }
            }
            else if (gocwakCam == true)
            {
                Debug.Log("44444444444");
                if (time > 24f)
                {
                    wakCam.depth = -1;
                    cardCam.depth = -2;
                    fireCam.depth = -3;
                    clockCam.depth = -4;
                    gocwakCam = false;

                    clockCam.enabled = false;
                    fireCam.enabled = false;
                    cardCam.enabled = false;
                    wakCam.enabled = false;
                    clockCam.GetComponent<AudioListener>().enabled = false;
                    fireCam.GetComponent<AudioListener>().enabled = false;
                    cardCam.GetComponent<AudioListener>().enabled = false;
                    wakCam.GetComponent<AudioListener>().enabled = false;
                    realWakCam.GetComponent<AudioListener>().enabled = true;
                    GameManager.production = false;
                    GameManager.tutorial = true;
                }
            }
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class messiMove : MonoBehaviour
{
    Vector3 mousePos, transPos, WorldMouse;
    static public Vector3 turnPos;
    private float DelayTime;
    private float PosGap;
    private float vecAngle;
    private float dot;
    //[SerializeField] private GameObject wakHead;
    [SerializeField] private GameObject system;
    
    public GameObject messiLeck;
    public GameObject messiTail;
    private CameraCon cameraCon;
    private Vector3 nowPos;
    private Vector3 subVec;
    private Vector3 tmpVec;
    private Vector3 angleVec;
    private Vector3 LeckTailVec;
    private Vector3 transTailVec;

    public bool messiinBath = false;
    public bool messiinRoom = false;
    private Animator animator;
    private messiCursor mc;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        mc = GetComponent<messiCursor>();
    }

    void Start()
    {
        transPos = transform.position;
        turnPos = Vector3.zero;
        cameraCon = system.GetComponent<CameraCon>();
        animator.SetBool("isWalk", false);
    }

    // Update is called once per frame
    void Update()
    {
        nowPos = this.transform.position;
        // if (PosGap > 5f)
        // {
        //     animator.SetBool("isWalk", true);
        // }
        // else
        // {
        //     animator.SetBool("isWalk", false);
        // }
        DelayTime += Time.deltaTime;
        if ((Input.GetMouseButtonDown(0)) && (cameraCon.nowCam == CameraCon.CamType.messi) && (DelayTime > 0.2f) && (messiCursor.canClick == true))
        {
            TargetPos();
            DelayTime = 0;
        }
        MoveToTarget();
        //float diff = transform.rotation.eulerAngles.y - Quaternion.LookRotation(subVec).normalized.eulerAngles.y;
        // if (Mathf.Abs(diff) <= 30)
        // {
        //     animator.SetBool("is90turnright", false);
        //     animator.SetBool("is90turnleft", false);
        // }
    }

    void TargetPos()
    {
        LeckTailVec = messiLeck.transform.position - messiTail.transform.position;
        turnPos = transform.position;
        mousePos = Input.mousePosition;
        WorldMouse = new Vector3(mousePos.x, mousePos.y, 230f);
        transPos = Camera.main.ScreenToWorldPoint(WorldMouse);
        subVec = transPos - transform.position;
        //vecAngle = Mathf.Atan2(angleVec.y, angleVec.x) * Mathf.Rad2Deg;
        //vecAngle = Vector3.SignedAngle(LeckTailVec, transTailVec, transform.forward);
        //Debug.Log(r);
        //Debug.Log(transform.rotation.eulerAngles);
    }

    void MoveToTarget()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(subVec).normalized, Time.deltaTime * 7f);
        
        //Debug.Log(transform.rotation.eulerAngles + "----" + Quaternion.LookRotation(subVec).normalized.eulerAngles);
        PosGap = Vector3.Distance(transform.position, transPos);

        if (PosGap > 5f)
        {
            animator.SetBool("isWalk", true);
            // animator.SetBool("isWalk", true);
            var dir = transPos - transform.position;
            transform.position += dir.normalized * Time.deltaTime * 50f;
            // animator.SetBool("is90turnright", false);
            // animator.SetBool("is90turnleft", false);
            
        }
        else
        {
            animator.SetBool("isWalk", false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("T_bathroom"))
        {
            if (cameraCon.nowCam == CameraCon.CamType.wak && messiinBath == false) messiinBath = true;

            else if (cameraCon.nowCam == CameraCon.CamType.wak && messiinBath == true) messiinBath = false;

            else if (cameraCon.nowCam == CameraCon.CamType.messi && messiinBath == false && (messiMove.turnPos.z < nowPos.z)) //&& (messiMove.lastPos.z < nowPos.z) && )
            {
                cameraCon.ShowBathCam();
                messiinBath = true;
            }

            else if (cameraCon.nowCam == CameraCon.CamType.messi && messiinBath == true && (messiMove.turnPos.z > nowPos.z)) // && (messiMove.lastPos.z > nowPos.z) && )
            {
                cameraCon.ShowMainCam();
                messiinBath = false;
            }
        }

        else if (other.gameObject.CompareTag("T_room"))
        {
            if (cameraCon.nowCam == CameraCon.CamType.wak && messiinRoom == false) messiinRoom = true;

            else if (cameraCon.nowCam == CameraCon.CamType.wak && messiinRoom == true) messiinRoom = false;

            else if (cameraCon.nowCam == CameraCon.CamType.messi && messiinRoom == false && (messiMove.turnPos.x < nowPos.x)) // (messiMove.lastPos.x < nowPos.x) && )
            {
                cameraCon.ShowRoomCam();
                messiinRoom = true;
            }

            else if (cameraCon.nowCam == CameraCon.CamType.messi && messiinRoom == true && (messiMove.turnPos.x > nowPos.x)) //(messiMove.lastPos.x > nowPos.x) && )
            {
                cameraCon.ShowMainCam();
                messiinRoom = false;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class messiCursor : MonoBehaviour
{
    [SerializeField] Texture2D cursorGeneral;
    [SerializeField] Texture2D cursorInteraction;
    [SerializeField] Texture2D blackcursor;

    public Camera MainCam;
    public Camera BathCam;
    public Camera RoomCam;
    private CameraCon cameraCon;
    private int h;
    private int w;
    [SerializeField] private GameObject system;
    Vector2 hotSpotG;
    Vector2 hotSpotI;    
    Vector2 hotSpotB;
    public static bool canClick;

    private void Awake()
    {
        //cursorGeneral.Resize(5, 5);
        hotSpotG.x = cursorGeneral.width / 2;
        hotSpotG.y = cursorGeneral.height / 2;

        hotSpotI.x = cursorInteraction.width / 2;
        hotSpotI.y = cursorInteraction.height / 2;

        hotSpotB.x = blackcursor.width / 2;
        hotSpotB.y = blackcursor.height / 2;
    }
    void Start()
    {
        cameraCon = system.GetComponent<CameraCon>();
        Cursor.SetCursor(cursorGeneral, hotSpotG, CursorMode.ForceSoftware);
        //Cursor.lockState = CursorLockMode.Confined;
        //Cursor.SetCursor(cursorInteraction, hotSpotI, CursorMode.ForceSoftware);
        h = Screen.height;
        w = Screen.width;
    }
    // Update is called once per frame
    void Update()
    {
        if (cameraCon.nowCam == CameraCon.CamType.messi)
        {
            clickSet();
            cursorSet();
        }
    }

    void clickSet()
    {
        Vector3 mp = Input.mousePosition;
        if (MainCam.enabled == true)
        {
            if (mp.y / h > 820f / 1080f)
            {
                if (mp.x / w > 1300f / 1920f && mp.x / w < 1355f / 1920f && mp.y / h < 1000f / 1080f && mp.y / h > 875f / 1080f) canClick = true;
                else canClick = false;
            }
            else if (mp.y / h < 370f / 1080f)
            {
                if (mp.y / h > 310f / 1080f && ((mp.x / w < 650f / 1920f && mp.x / w > 510f / 1920f) || (mp.x / w > 1180f / 1920f && mp.x / w < 1500f / 1920f))) canClick = true;
                else canClick = false;
                
            }
            else if (mp.x / w < 525f / 1920f)
            {
                canClick = false;
            }
            else if (mp.x / w > 1500f / 1920f)
            {
                if (mp.x / w > 1590f / 1920f && mp.y / h > 695f / 1080f && mp.x / w < 1770f / 1920f && mp.y / h < 810f / 1080f) canClick = true;
                else canClick = false;
            }
            else
            {
                canClick = true;
            }
        }
        
        else if (BathCam.enabled == true)
        {
            if (mp.x / w > 995f / 1920f && mp.y / h > 570f / 1080f)
            {
                canClick = false;
            }
            else if (mp.x / w < 770f / 1920f)
            {
                canClick = false;
            }
            else if (mp.y / h < 260f / 1080f)
            {
                if (mp.x / w > 910f / 1920f && mp.y / h < 175f / 1080f && mp.x / w < 1030f / 1920f && mp.y / h > 90f / 1080f)
                {
                    canClick = true;
                }
                else canClick = false;
            }
            else if (mp.y / h > 810f / 1080f)
            {
                canClick = false;
            }
            else if (mp.x / w > 1185f / 1920f)
            {
                canClick = false;
            }
            else
            {
                canClick = true;
            }
        }

        else if (RoomCam.enabled == true)
        {
            if (mp.x / w < 495f / 1920f)
            {
                if (mp.x / w < 395f / 1920f && mp.y / h < 410f / 1080f && mp.x / w > 215f / 1920f && mp.y / h > 300f / 1080f)
                {
                    canClick = true;
                }
                else canClick = false;
            }
            else if (mp.x / w > 1405f / 1920f)
            {
                canClick = false;
            }
            else if (mp.y / h < 275f / 1080f)
            {
                canClick = false;
            }
            else if (mp.y / h > 805f / 1080f)
            {
                canClick = false;
            }
            else if (mp.x / w < 555f / 1920f && mp.y / h < 310f / 1080f)
            {
                canClick = false;
            }
            else
            {
                canClick = true;
            }
        }
    }

    void cursorSet()
    {
        if (canClick == false)
        {
            Cursor.SetCursor(blackcursor, hotSpotB, CursorMode.ForceSoftware);
        }
        else
        {
            Cursor.SetCursor(cursorGeneral, hotSpotG, CursorMode.ForceSoftware);
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool tutorial = true;
    public static bool production = true;
    public static int gameTry = 1;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            gameTry += 1;
            SceneManager.LoadScene(0);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour
{
    [Range(1, 100)]
    public int fFont_Size;
    [Range(0, 1)]
    public float Red, Green, Blue;

    float deltaTime = 0.0f;

    private void Start()
    {
        fFont_Size = fFont_Size == 0 ? 50 : fFont_Size;
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 0.02f);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / fFont_Size;
        style.normal.textColor = new Color(Red, Green, Blue, 1.0f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLock : MonoBehaviour
{
    public Light light1;
    private Collider collider1;
    // Start is called before the first frame update
    void Start()
    {
        collider1 = GetComponent<BoxCollider>();
        collider1.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (light1.enabled == true)
        {
            collider1.enabled = true;
        }
        else if (light1.enabled == false) collider1.enabled = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAction : MonoBehaviour
{
    public bool open = false;
    public bool isMoving = false;
    public float doorDelay = 0f;
    public float doorOpenAngle = 90f;
    public float doorCloseAngle = 0f;
    public float smoot = 3f;



    // Start is called before the first frame update

    public void ChangeDoorState()
    {
        open = !open;
    }

    // Update is called once per frame
    void Update()
    {
        if(isMoving)
        {
            if (open)
            {
                doorDelay += Time.deltaTime;
                Quaternion targetRotation = Quaternion.Euler(0, doorOpenAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smoot * Time.deltaTime);               
            }
            else
            {
                doorDelay += Time.deltaTime;
                Quaternion targetRotation2 = Quaternion.Euler(0, doorCloseAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation2, smoot * Time.deltaTime);
            }
            if (doorDelay > 2f)
            {
                isMoving = false;
                doorDelay = 0f;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DigitalClock_724 : MonoBehaviour {
    public enum RoomNums
    {
        ROOM723,
        ROOM724
    }
    public Material blackmat;
    public GameObject hamburger = null;
    
    //[SerializeField] private Light[] Lights = new Light[8];
    //public Camera WakCam;
    //public Camera downCam;
    //public Camera wakpushCam;
    //public GameObject door;
   // public GameObject door2;
   // private bool CamMove = false;
   // private float time;

    //-------------------------------------------------
    [SerializeField] CheckTime checkTime;
    //public float speed = 1.0f; 
    public int sec;      // start Time
    public int minute;        // start Time
    //-------------------------------------------------
    Renderer objRenderer; 
    //-------------------------------------------------
    //float delay;
    //-------------------------------------------------
    [SerializeField]
    private GameObject sysObj;
    private ChangePasswordManager sys_passwordManager;
    [SerializeField]
    private RoomNums roomNum;

//----------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------
    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        objRenderer.materials[5].SetTextureOffset("_MainTex", new Vector2(0.0f, 0.9f)); // 가운데 " : " 깜빡임 키기
        objRenderer.materials[5].SetTextureOffset("_EmissionMap", new Vector2(0.0f, 0.9f));
        if(sysObj == null)
        {
            sysObj = GameObject.Find("System");
        }
        if(sys_passwordManager==null) sys_passwordManager = sysObj.GetComponent<ChangePasswordManager>();
        if (checkTime == null) checkTime = sysObj.GetComponent<CheckTime>();
    }
    //----------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------------------------------------------------------------------
    void Update()
    {
        if (roomNum == RoomNums.ROOM723 && sys_passwordManager.nowState == ChangePasswordManager.State.ChangePassword_723 && sys_passwordManager.is723Changed)
        {
            showPassword(sys_passwordManager.password723);
        }
        else if (roomNum == RoomNums.ROOM724 && sys_passwordManager.nowState == ChangePasswordManager.State.ChangePassword_724 && sys_passwordManager.is724Changed)
        {
            showPassword(sys_passwordManager.password724);
        }
        else
        {
            minute = checkTime.minute;
            sec = checkTime.sec;

            float offset = 0.0f - 0.1f * (float)(minute % 10);
            objRenderer.materials[4].SetTextureOffset("_MainTex", new Vector2(0.0f, offset));
            objRenderer.materials[4].SetTextureOffset("_EmissionMap", new Vector2(0.0f, offset));

            // Minute 10er
            offset = 0.0f - 0.1f * (float)((minute / 10) % 10);
            objRenderer.materials[3].SetTextureOffset("_MainTex", new Vector2(0.0f, offset));
            objRenderer.materials[3].SetTextureOffset("_EmissionMap", new Vector2(0.0f, offset));

            // 3시
            objRenderer.materials[1].SetTextureOffset("_MainTex", new Vector2(0.0f, -0.3f));
            objRenderer.materials[1].SetTextureOffset("_EmissionMap", new Vector2(0.0f, -0.3f));

            // 20시
            objRenderer.materials[2].SetTextureOffset("_MainTex", new Vector2(0.0f, -0.2f));
            objRenderer.materials[2].SetTextureOffset("_EmissionMap", new Vector2(0.0f, -0.2f));
        }
    }

    private void showPassword(int password)
    {
        //2
        float offset = 0.0f - 0.1f * (float)(((password / 100) % 10));
        objRenderer.materials[1].SetTextureOffset("_MainTex", new Vector2(0.0f, offset));
        objRenderer.materials[1].SetTextureOffset("_EmissionMap", new Vector2(0.0f, offset));

        //1
        offset = 0.0f - 0.1f * (float)((password / 1000) % 10);
        objRenderer.materials[2].SetTextureOffset("_MainTex", new Vector2(0.0f, offset));
        objRenderer.materials[2].SetTextureOffset("_EmissionMap", new Vector2(0.0f, offset));

        //4
        offset = 0.0f - 0.1f * (float)(password % 10);
        objRenderer.materials[4].SetTextureOffset("_MainTex", new Vector2(0.0f, offset));
        objRenderer.materials[4].SetTextureOffset("_EmissionMap", new Vector2(0.0f, offset));

        //3
        offset = 0.0f - 0.1f * (float)((password / 10) % 10);
        objRenderer.materials[3].SetTextureOffset("_MainTex", new Vector2(0.0f, offset));
        objRenderer.materials[3].SetTextureOffset("_EmissionMap", new Vector2(0.0f, offset));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock_Digital : MonoBehaviour {
    //-------------------------------------------------        
    public float speed; 
    public int minute;      // start Time
    public int hour;        // start Time
    //-------------------------------------------------
    Renderer objRenderer; 
    //-------------------------------------------------
    float delay;
    //-------------------------------------------------
    static public bool end617 = false;
    private int i = 0;

//----------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------
void Start()
{
    objRenderer = GetComponent<Renderer>();
    objRenderer.materials[5].SetTextureOffset("_MainTex", new Vector2(0.0f, 0.9f));
    objRenderer.materials[5].SetTextureOffset("_EmissionMap", new Vector2(0.0f, 0.9f));    
}
//----------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------
void Update()
{
    //--------------------------------------------------------------------------------------------------------------------------
    delay -= Time.deltaTime * speed;
    if (minute == 17 && hour == 6)
    {
        end617 = true;
        speed = 0;
    }
    else if(delay < 0.0f)
    { 
        delay = 1.0f;
        minute++;
        i++;
        if (i > 137) speed -= 0.5f;
        if(minute >= 60)
        {
            minute = 0;
            hour++;
            if(hour >= 24)
                hour = 0;
        }
    }
    //--------------------------------------------------------------------------------------------------------------------------
    // Minute 1er
    float offset = 0.0f - 0.1f * (float)(minute % 10);
    objRenderer.materials[4].SetTextureOffset("_MainTex",     new Vector2(0.0f, offset));
    objRenderer.materials[4].SetTextureOffset("_EmissionMap", new Vector2(0.0f, offset));

    // Minute 10er
    offset = 0.0f - 0.1f * (float)((minute / 10) % 10);
    objRenderer.materials[3].SetTextureOffset("_MainTex",     new Vector2(0.0f, offset));
    objRenderer.materials[3].SetTextureOffset("_EmissionMap", new Vector2(0.0f, offset));

    // Hour 1er
    offset = 0.0f - 0.1f * (float)(hour % 10);
    objRenderer.materials[1].SetTextureOffset("_MainTex",     new Vector2(0.0f, offset));
    objRenderer.materials[1].SetTextureOffset("_EmissionMap", new Vector2(0.0f, offset));

    // Hour 10er
    offset = 0.0f - 0.1f * (float)((hour / 10) % 10);
    objRenderer.materials[2].SetTextureOffset("_MainTex",     new Vector2(0.0f, offset));
    objRenderer.materials[2].SetTextureOffset("_EmissionMap", new Vector2(0.0f, offset));
}
//----------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------
//----------------------------------------------------------------------------------------------------------------------------------------------
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePasswordManager : MonoBehaviour
{
    public enum State
    {
        NotWorking,
        ChangePassword_723,
        ChangePassword_724
    }
    public State nowState = State.NotWorking;
    public int password723 = 1217;
    public int password724 = 1113; //비밀번호 때려맞추기 방지, 비밀번호 변경시 0000으로 초기화
    [SerializeField] GameObject clock723;
    [SerializeField] GameObject clock724;
    [SerializeField] Material[] matsRedNum;
    [SerializeField] Material[] matsWhiteNum;
    private int password723_old;
    private int password724_old;
    public bool is723Changed = false;
    public bool is724Changed = false;
    public bool is724Open = false;
    private bool isColorChanged = false;

    void Start()
    {
        nowState = State.NotWorking;
        password723 = 1217;
        password724 = 1113;
        password723_old = password723;
        password724_old = password724;
    }

    // Update is called once per frame
    void Update()
    {
        if(nowState == State.ChangePassword_723&& !isColorChanged)
        {
            clock723.GetComponent<MeshRenderer>().materials = matsRedNum;
            isColorChanged = true;
        }
        else if(nowState == State.ChangePassword_724&&!isColorChanged)
        {
            clock724.GetComponent<MeshRenderer>().materials = matsRedNum;
            isColorChanged = true;
        }

        if(nowState == State.NotWorking && isColorChanged)
        {
            clock723.GetComponent<MeshRenderer>().materials = matsWhiteNum;
            clock724.GetComponent<MeshRenderer>().materials = matsWhiteNum;
            isColorChanged=false;
        }

        if(nowState == State.ChangePassword_723 && (password723 != password723_old) && !is723Changed)
        {
            is723Changed = true;
        }
        else if(nowState == State.ChangePassword_724 &&(password724 != password724_old) && !is724Changed)
        {
            is724Changed = true;
        }
        else if(nowState == State.NotWorking && (is723Changed||is724Changed))
        {
            is723Changed = false;
            is724Changed = false;  
            password723_old = password723;
            password724_old = password724;
            isColorChanged = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

public class CameraCon : MonoBehaviour
{
    public enum CamType
    {
        messi,
        wak,
        interactionCam,
        interaction_SeeObject,
        end
    }
    private bool camset = true;
    public CamType nowCam = CamType.messi;
    public Camera MainCam;
    public Camera BathCam;
    public Camera RoomCam;
    public Camera WakCam;
    public Camera EndCam;

    [SerializeField]
    private GameObject interaction_info;

    [SerializeField]
    private GameObject messi;

    [SerializeField]
    private GameObject wakgood; //WakHead

    [SerializeField] GameObject cursorUI;

    // Start is called before the first frame update
    void Start()
    {
        BathCam.enabled = false;
        RoomCam.enabled = false;
        MainCam.enabled = false;
        BathCam.GetComponent<AudioListener>().enabled = false;
        RoomCam.GetComponent<AudioListener>().enabled = false;
        MainCam.GetComponent<AudioListener>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.production == false)
        {
            if (camset == true) // 한번만 실행되는 if문
            {
                nowCam = CamType.wak;
                camset = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                cursorUI.SetActive(true);
                BathCam.enabled = false;
                RoomCam.enabled = false;
                MainCam.enabled = false;
                BathCam.GetComponent<AudioListener>().enabled = false;
                RoomCam.GetComponent<AudioListener>().enabled = false;
                MainCam.GetComponent<AudioListener>().enabled = false;
                WakCam.enabled = true;
                WakCam.GetComponent<AudioListener>().enabled = true;
                interaction_info.gameObject.SetActive(true);
                wakgood.GetComponent<WakMove>().enabled = true;
            }
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CheckmessiCam();
            }
        }
    }

    public void CheckmessiCam()
    {
        if (nowCam == CamType.messi) //메시시점에서 왁굳시점으로 가는 조건문
        {
            BathCam.enabled = false;
            RoomCam.enabled = false;
            MainCam.enabled = false;
            BathCam.GetComponent<AudioListener>().enabled = false;
            RoomCam.GetComponent<AudioListener>().enabled = false;
            MainCam.GetComponent<AudioListener>().enabled = false;
            WakCam.enabled = true;
            WakCam.GetComponent<AudioListener>().enabled = true;
            nowCam = CamType.wak;
            interaction_info.gameObject.SetActive(true);
            wakgood.GetComponent<WakMove>().enabled = true; //메시캠 활성시 왁굳 이동 중지
        }
        else if (nowCam == CamType.wak) //왁굳시점에서 메시시점으로 가는 조건문
        {
            WakCam.enabled = false;
            WakController.NoneCursor();
            cursorUI.SetActive(false);
            messiMove tmp = messi.GetComponent<messiMove>();
            if ((tmp.messiinBath == false) && (tmp.messiinRoom == false)) ShowMainCam();
            else if (tmp.messiinBath == true) ShowBathCam();
            else if (tmp.messiinRoom == true) ShowRoomCam();
            nowCam = CamType.messi;
            interaction_info.gameObject.SetActive(false);
            wakgood.GetComponent<WakMove>().enabled = false; //메시캠 비활성시 왁굳 이동 재개
            WakCam.GetComponent<AudioListener>().enabled = false;
        }
    }

    public void ShowMainCam()
    {
        BathCam.enabled = false;
        RoomCam.enabled = false;
        MainCam.enabled = true;
        BathCam.GetComponent<AudioListener>().enabled = false;
        RoomCam.GetComponent<AudioListener>().enabled = false;
        MainCam.GetComponent<AudioListener>().enabled = true;
    }

    public void ShowBathCam()
    {
        RoomCam.enabled = false;
        MainCam.enabled = false;
        BathCam.enabled = true;
        BathCam.GetComponent<AudioListener>().enabled = true;
        RoomCam.GetComponent<AudioListener>().enabled = false;
        MainCam.GetComponent<AudioListener>().enabled = false;
    }
    public void ShowRoomCam()
    {
        BathCam.enabled = false;
        MainCam.enabled = false;
        RoomCam.enabled = true;
        BathCam.GetComponent<AudioListener>().enabled = false;
        RoomCam.GetComponent<AudioListener>().enabled = true;
        MainCam.GetComponent<AudioListener>().enabled = false;
    }

    public void ShowInteractionCam(Camera focusCam)
    {
        if (nowCam == CamType.wak)
        {
            nowCam = CamType.interactionCam;
            focusCam.enabled = true;
            focusCam.GetComponent<AudioListener>().enabled = true;
            WakCam.enabled = false;
            WakCam.GetComponent<AudioListener>().enabled = false;
            BathCam.enabled = false;
            MainCam.enabled = false;
            RoomCam.enabled = false;
        }
    }
    public void CloseInteractionCam(Camera focusCam)
    {
        if (nowCam == CamType.interactionCam)
        {
            nowCam = CamType.wak;
            focusCam.enabled = false;
            focusCam.GetComponent<AudioListener>().enabled = false;
            WakCam.enabled = true;
            WakCam.GetComponent<AudioListener>().enabled = true;
            BathCam.enabled = false;
            MainCam.enabled = false;
            RoomCam.enabled = false;
        }
    }
    public void ShowInteractionShowObject()
    {
        BathCam.enabled = false;
        RoomCam.enabled = false;
        MainCam.enabled = false;
        BathCam.GetComponent<AudioListener>().enabled = false;
        RoomCam.GetComponent<AudioListener>().enabled = false;
        MainCam.GetComponent<AudioListener>().enabled = false;
        WakCam.enabled = true;
        WakCam.GetComponent<AudioListener>().enabled = true;
        nowCam = CamType.interaction_SeeObject;
        interaction_info.gameObject.SetActive(true);
        wakgood.GetComponent<WakMove>().enabled = false;
    }
    public void CloseInteractionShowObject()
    {
        BathCam.enabled = false;
        RoomCam.enabled = false;
        MainCam.enabled = false;
        BathCam.GetComponent<AudioListener>().enabled = false;
        RoomCam.GetComponent<AudioListener>().enabled = false;
        MainCam.GetComponent<AudioListener>().enabled = false;
        WakCam.enabled = true;
        WakCam.GetComponent<AudioListener>().enabled = true;
        nowCam = CamType.wak;
        interaction_info.gameObject.SetActive(true);
        wakgood.GetComponent<WakMove>().enabled = true;
    }

    public void EndCameraSet()
    {
        MainCam.enabled = false;
        BathCam.enabled = false;
        RoomCam.enabled = false;
        WakCam.enabled = false;
        EndCam.enabled = true;
        nowCam = CamType.end;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddGravity : MonoBehaviour
{
    private Rigidbody rb;
    AudioSource AS;
    
    [SerializeField]
    private float gravityForce = 100f; 
    private bool sound = true;
    private bool start = false;
    private float time = 0;
    private float temptime = 0;
    // Start is called before the first frame update
    void Start()
    {
        AS = this.GetComponent<AudioSource>();
        AS.volume = 0.18f;
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // FixedUpdate is called once per frame
    private void FixedUpdate()
    {
        if (start == false)
        {
            temptime += Time.deltaTime;
            if (temptime > 1f) start = true;
        }
        rb.AddForce(Vector3.down * gravityForce);   
        if (sound == false)
        {
            time += Time.deltaTime;
            if (time > 1.5f)
            {   
                sound = true;
                time = 0;
            }
        } 
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("floor") && GameManager.production == false && sound == true && start == true)
        {
            AS.enabled = true;
            AS.Play();
            sound = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialQuest : MonoBehaviour
{
    public enum Steps
    {
        init,
        clickTap,
        clickTapSec,
        holdCude,
        rotateCude,
        clear
    }public Steps nowStep;

    [SerializeField] private GameObject sysObj;
    [SerializeField] private GameObject wakCamera;
    [SerializeField] private GameObject questUI;
    [SerializeField] private TextMeshProUGUI UIText;

    private float timer;
    AudioSource AS;

    [SerializeField] private AudioClip sounds;
    [SerializeField] private float questVolume = 0.25f;

    // Start is called before the first frame update
    void Start()
    {
        nowStep = Steps.init;
        questUI.SetActive(false);
        timer = 0;
        AS = wakCamera.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.tutorial == true)
        {
            switch (nowStep)
            {
                case Steps.init:
                    StartTutorial();
                    break;
                case Steps.clickTap:
                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        nowStep = Steps.clickTapSec;
                        UIText.text = "Tab 키를 눌러\n시점 변경하기 (2/2)";
                    }
                    break;
                case Steps.clickTapSec:
                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        nowStep = Steps.holdCude;
                        UIText.text = "로비로 이동해\n매력큐브 잡기 (좌클릭 꾹)";

                        this.GetComponent<AudioSource>().clip = sounds;
                        this.GetComponent<AudioSource>().volume = questVolume;
                        this.GetComponent<AudioSource>().Play();
                    }
                    break;
                case Steps.holdCude:
                    if (wakCamera.transform.childCount > 0)
                    {
                        nowStep = Steps.rotateCude;
                        UIText.text = "매력큐브 살펴보기 (우클릭 꾹)";
                        timer = 0;

                        this.GetComponent<AudioSource>().clip = sounds;
                        this.GetComponent<AudioSource>().volume = questVolume;
                        this.GetComponent<AudioSource>().Play();
                    }
                    break;
                case Steps.rotateCude:
                    if (wakCamera.transform.childCount < 1)
                    {
                        nowStep = Steps.holdCude;
                        UIText.text = "매력큐브 잡기 (좌클릭 꾹)";
                        timer = 0;
                    }
                    else if (timer > 1)
                    {
                        nowStep = Steps.clear;
                        UIText.text = "";
                        timer = 0;

                        this.GetComponent<AudioSource>().clip = sounds;
                        this.GetComponent<AudioSource>().volume = questVolume;
                        this.GetComponent<AudioSource>().Play();
                    }
                    else if (wakCamera.transform.childCount > 0 && Input.GetMouseButton(1))
                    {
                        timer += Time.deltaTime;
                    }
                    break;
                case Steps.clear:
                    if (timer < 3)
                    {
                        timer += Time.deltaTime;
                    }
                    else
                    {
                        questUI.SetActive(false);
                        GameManager.tutorial = false;
                        AS.Play();
                    }
                    break;
                default:
                    break;

            }
        }
        else
        {
            //this.GetComponent<TutorialQuest>().enabled = false;
        }
    }

    void StartTutorial()
    {
        nowStep = Steps.clickTap;
        questUI.SetActive(true);
        UIText.text = "Tab 키를 눌러\n시점 변경하기 (1/2)";    
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SmokeAndRestart : MonoBehaviour
{
    enum Steps
    {
        Phase1,
        Phase2,
        Phase3,
        Phase4,
        Phase5,
        Phase6,
        Phase7,
        Phase8
    }
    [SerializeField] Steps nowStep;
    [SerializeField] CheckTime checkTime;
    [SerializeField] private Light[] Lights = new Light[0];

    public GameObject[] fire;
    [SerializeField] private GameObject[] smokes;
    public Material blackmat;
    public GameObject hamburger = null;

    [SerializeField] private CameraCon cameraCon;
    private bool isParticleOn = false;

    public Camera WakCam;
    public Camera wakpushCam;
    public Camera downCam;
    public GameObject door;
    public GameObject door2;
    private bool CamMove = false;
    private float time;


    private void Start()
    {
        WakCam.enabled = false;
        wakpushCam.enabled = false;
        downCam.enabled = false;

        foreach (GameObject f in fire) f.SetActive(false);
    }
    void Update()
    {
        if(cameraCon.nowCam == CameraCon.CamType.messi && !isParticleOn)
        {
            isParticleOn = true;
            ReLoadParticle();
        }
        else if(cameraCon.nowCam != CameraCon.CamType.messi && isParticleOn)
        {
            isParticleOn = false;
            ReLoadParticle();
        }
        switch(nowStep)
        {
            case Steps.Phase1:
                if (checkTime.minute > 0 && checkTime.sec > 8)
                {
                    fire[0].SetActive(true);
                    smokes[0].SetActive(true);
                    nowStep = Steps.Phase2;
                    ReLoadParticle();
                }
                break;
            case Steps.Phase2:
                if (checkTime.minute > 1 && checkTime.sec > 1)
                {
                    Material[] mat = hamburger.GetComponent<Renderer>().materials;
                    for (int i = 0; i < 6; i++)
                    {
                        mat[i] = blackmat;
                    }
                    hamburger.GetComponent<Renderer>().materials = mat;
                    fire[0].SetActive(false);
                    fire[1].SetActive(true);
                    smokes[0].SetActive(false);
                    smokes[1].SetActive(true);
                    smokes[2].SetActive(true);
                    nowStep= Steps.Phase3;
                    ReLoadParticle();
                }
                break;
            case Steps.Phase3:
                if (checkTime.minute > 1 && checkTime.sec > 55)
                {
                    fire[2].SetActive(true);
                    nowStep = Steps.Phase4;
                    ReLoadParticle();
                }
                break;
            case Steps.Phase4:
                if (checkTime.minute > 2 && checkTime.sec > 49)
                {
                    fire[3].SetActive(true);
                    nowStep = Steps.Phase5;
                    ReLoadParticle();
                }
                break;
            case Steps.Phase5:
                if (checkTime.minute > 3 && checkTime.sec > 43)
                {
                    fire[4].SetActive(true);
                    nowStep = Steps.Phase6;
                    ReLoadParticle();
                }
                break;
            case Steps.Phase6:
                if (checkTime.minute > 4 && checkTime.sec > 25)
                {
                    fire[5].SetActive(true);
                    nowStep = Steps.Phase7;
                    ReLoadParticle();
                }
                break;
            case Steps.Phase7:
                if (checkTime.minute > 5 && checkTime.sec > 13)
                {
                    WakCam.enabled = true;
                    wakpushCam.enabled = true;
                    downCam.enabled = true;
                    door2.transform.rotation = Quaternion.Euler(0, 0, 0);
                    for (int i = 0; i < 8; i++)
                    {
                       Lights[i].enabled = false;
                    }
                    time += Time.deltaTime;
                    wakpushCam.depth = 10;
                    if (time > 0.5f && CamMove == false)
                    {
                        Quaternion targetRotation2 = Quaternion.Euler(0, 90f, 0);
                        door.transform.localRotation = Quaternion.Slerp(door.transform.localRotation, targetRotation2, 5f * Time.deltaTime);
                    }
                    if (time > 0.8f && CamMove == false)
                    {
                        wakpushCam.transform.position = Vector3.MoveTowards(wakpushCam.transform.position, downCam.transform.position, Time.deltaTime * 70);
                        wakpushCam.transform.rotation = Quaternion.RotateTowards(wakpushCam.transform.rotation, downCam.transform.rotation, Time.deltaTime * 70);
                        if (wakpushCam.transform.position == downCam.transform.position && wakpushCam.transform.rotation == downCam.transform.rotation) CamMove = true;
                    }
                    if (CamMove == true)
                    {
                        wakpushCam.transform.position = Vector3.MoveTowards(wakpushCam.transform.position, WakCam.transform.position, Time.deltaTime * 70);
                        wakpushCam.transform.rotation = Quaternion.RotateTowards(wakpushCam.transform.rotation, WakCam.transform.rotation, Time.deltaTime * 70);
                    }
                    if (time > 1.8f && CamMove == true)
                    {
                        nowStep = Steps.Phase8;
                        GameManager.gameTry += 1;
                        SceneManager.LoadScene(0);
                    }
                }
                break;
            default:
                break;
        }
        if (nowStep >= Steps.Phase2 && checkTime.minute > 1 && smokes[2].transform.position.x < 100f)
        {
            smokes[2].transform.position += new Vector3(2*Time.deltaTime, 0, 0);
        }
    }

    private void ReLoadParticle()
    {
        if (!isParticleOn)
        {
            if (nowStep > Steps.Phase1)
            {
                if (fire[0].activeSelf)
                {
                    fire[0].GetComponent<ParticleSystem>().Stop();
                    fire[0].GetComponent<ParticleSystem>().Clear();
                }
                if (smokes[0].activeSelf)
                {
                    smokes[0].GetComponent<ParticleSystem>().Stop();
                    smokes[0].GetComponent<ParticleSystem>().Clear();
                }
                if (nowStep > Steps.Phase2)
                {
                    fire[1].GetComponent<ParticleSystem>().Stop();
                    fire[1].GetComponent<ParticleSystem>().Clear();
                    smokes[1].GetComponent<ParticleSystem>().Stop();
                    smokes[1].GetComponent<ParticleSystem>().Clear();
                    ParticleSystem[] tmp = smokes[2].GetComponentsInChildren<ParticleSystem>();
                    foreach (ParticleSystem p in tmp)
                    {
                        p.GetComponent<ParticleSystem>().Stop();
                        p.GetComponent<ParticleSystem>().Clear();
                    }

                    if (nowStep > Steps.Phase3)
                    {
                        fire[2].GetComponent<ParticleSystem>().Stop();
                        fire[2].GetComponent<ParticleSystem>().Clear();

                        if (nowStep > Steps.Phase4)
                        {
                            fire[3].GetComponent<ParticleSystem>().Stop();
                            fire[3].GetComponent<ParticleSystem>().Clear();

                            if (nowStep > Steps.Phase5)
                            {
                                fire[4].GetComponent<ParticleSystem>().Stop();
                                fire[4].GetComponent<ParticleSystem>().Clear();

                                if (nowStep > Steps.Phase6){
                                    fire[5].GetComponent<ParticleSystem>().Stop();
                                    fire[5].GetComponent<ParticleSystem>().Clear();
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (nowStep > Steps.Phase1)
            {
                if (fire[0].activeSelf) fire[0].GetComponent<ParticleSystem>().Play();
                if (smokes[0].activeSelf) smokes[0].GetComponent<ParticleSystem>().Play();
                if (nowStep > Steps.Phase2)
                {
                    fire[1].GetComponent<ParticleSystem>().Play();
                    smokes[1].GetComponent<ParticleSystem>().Play();
                    ParticleSystem[] tmp = smokes[2].GetComponentsInChildren<ParticleSystem>();
                    foreach (ParticleSystem p in tmp)
                    {
                        p.GetComponent<ParticleSystem>().Play();
                    }

                    if (nowStep > Steps.Phase3)
                    {
                        fire[2].GetComponent<ParticleSystem>().Play();

                        if (nowStep > Steps.Phase4)
                        {
                            fire[3].GetComponent<ParticleSystem>().Play();

                            if (nowStep > Steps.Phase5)
                            {
                                fire[4].GetComponent<ParticleSystem>().Play();

                                if (nowStep > Steps.Phase6)
                                {
                                    fire[5].GetComponent<ParticleSystem>().Play();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckTime : MonoBehaviour
{
    float delay = 1;
    public float speed = 1.0f;

    public int minute;
    public int sec;
    // Start is called before the first frame update
    void Start()
    {
        minute = 0;
        sec = 0;
    }

    // Update is called once per frame
    void Update()
    {
        delay -= Time.deltaTime * speed;
        if (delay < 0.0f)
        {
            delay = 1.0f;
            if (!GameManager.tutorial && !GameManager.production)
            {
                sec++;
                if (sec >= 60)
                {
                    sec = 0;
                    minute++;
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubtitleTrigger : MonoBehaviour
{
    public GameObject subtitleSys;
    SubtitleManager subtitleManager;
    public Subtitle subtitle;

    public bool isShow = false;
    int index = 0;
    int endIndex = 0;
    float timer = 0;

    private void Start()
    {
        subtitleManager = subtitleSys.GetComponent<SubtitleManager>();
    }
    private void Update()
    {
        if(isShow)
        {
            timer += Time.deltaTime;
            if(timer >= subtitle.time[index])
            {
                if(index+1 == subtitle.time.Length)
                {
                    timer = 0;
                    index = 0;
                    subtitleManager.EndSubtitle();
                    isShow = false;
                }
                else
                {
                    timer = 0;
                    index++;
                    subtitleManager.ShowNextSentence();
                }             
            }
        }
    }

    public void TriggerSubtitle(int startIndex = 0, int endIdx = -1)
    {
        if(!isShow)
        {
            timer = 0;
            isShow = true;
            index = startIndex;
            if (endIdx == -1) endIndex = subtitle.time.Length;
            else endIndex = endIdx;
            subtitleManager.StartSubtitle(subtitle);
        }     
    }

    public void ShowOneSentence(string subtitleData, string name = null )
    {
        if (name == null) name = subtitle.name;
        Debug.Log(name);
        subtitleManager.ShowOneSentence(subtitleData, name, subtitle.nameColor);
    }

    public void OffSubtitle()
    {
        isShow = false;
        subtitleManager.OffSubtitle();
    }
    
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SubtitleManager : MonoBehaviour
{
    [SerializeField] private GameObject subtitleUI;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    private string nameStr;
    private Color nameColor;
    private Queue<string> sentences;

    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartSubtitle (Subtitle data)
    {
        sentences.Clear();

        subtitleUI.SetActive(true);
        nameStr = data.name;
        nameColor = data.nameColor;
        foreach(string s in data.sentence)
        {
            sentences.Enqueue(s);
        }
        ShowNextSentence();
    }

    public void ShowNextSentence()
    {
        Debug.Log(this.name+" : ShowNextSentence()");
        string sentence = sentences.Dequeue();

        if (sentence!= "<empty>")
        {
            nameText.text = nameStr;
            nameText.color = nameColor;
            subtitleText.text = sentence;
        }
    }

    public void EndSubtitle()
    {
        subtitleUI.SetActive(false);
        subtitleText.text = null;
        sentences.Clear();
    }

    public void ShowOneSentence(string subtitle, string name, Color color)
    {
        subtitleUI.SetActive(true);
        nameText.text = name;
        nameText.color = color;
        subtitleText.text = subtitle;
    }

    public void OffSubtitle()
    {
        subtitleUI.SetActive(false);
        nameText.text = null;
        subtitleText.text = null;
        sentences.Clear();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Subtitle
{
    public string name;
    public Color nameColor;

    [TextArea(3,10)]
    public string[] sentence;
    public float[] time;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionSubtitle : MonoBehaviour
{
    [SerializeField] private SubtitleTrigger subDev;
    [SerializeField] private SubtitleTrigger subDoc;
    void Start()
    {
        
    }

    public void StartProductionSubtitles()
    {
        subDev.TriggerSubtitle();
        subDoc.TriggerSubtitle();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using static DigitalClock_724;

public class InteractionTrigLightSwitch : InteractionTrigger
{
    enum RoomNums
    {
        ROOM723,
        ROOM724,
        BOTH,
        NON
    }
    enum SwitchPlaces
    {
        RESTROOM,   //100
        BEDROOM,    //1
        LIVINGROOM, //10
        HALLWAY,    //1000
        NON
    }


    public bool isOn = false;
    [SerializeField] private Light[] Lights = new Light[0];
    public AudioClip switchUp;
    public AudioClip switchDown;
    public float animationSpeed = 10.0f;
    public bool loopForwardAnimation = true;
    public float maxSwitchDistance = 1.25f;
    private Transform switchTransform;
    AudioSource audioSource;

    [SerializeField]
    private GameObject sysObj;
    private ChangePasswordManager sys_passwordManager;

    [SerializeField]
    private RoomNums roomNum;
    [SerializeField]
    private SwitchPlaces switchPlace;

    [SerializeField]
    private bool isChangePassword = false;

    private void Start()
    {
        isOn = false;
        if (isOn)
        {
            foreach (Light light in Lights) light.enabled = true;
            explanation = "불 끄기";
        }
        else
        {
            foreach (Light light in Lights) light.enabled = false;
            explanation = "불 켜기";
        }
        //audioSource = GetComponent<AudioSource>();
        if (sysObj == null)
        {
            sysObj = GameObject.Find("System");
        }
        sys_passwordManager = sysObj.GetComponent<ChangePasswordManager>();
    }

    private void Update()
    {
        if(!isChangePassword)
        {
            if(roomNum == RoomNums.BOTH && sys_passwordManager.nowState != ChangePasswordManager.State.NotWorking)
            {
                isChangePassword = true;
                explanation = "스위치 작동";
            }
            else if (roomNum == RoomNums.ROOM723 && sys_passwordManager.nowState == ChangePasswordManager.State.ChangePassword_723)
            {
                isChangePassword = true;
                explanation = "스위치 작동";
            }
            else if (roomNum == RoomNums.ROOM724 && sys_passwordManager.nowState == ChangePasswordManager.State.ChangePassword_724)
            {
                isChangePassword = true;
                explanation = "스위치 작동";
            }
        }
        else
        {
            if(sys_passwordManager.nowState == ChangePasswordManager.State.NotWorking)
            {
                isChangePassword = false;
                if (Lights[0].GetComponent<Light>().enabled == true) setSwitchStateToOn();
                else setSwitchStateToOff();
            }
        }
    }

    public override void InteractionFunction()
    {
        base.InteractionFunction();
        //audioSource.Play();
        if (!isChangePassword)
        {
            if (isOn)
            {
                foreach (Light light in Lights) light.enabled = false;
                explanation = "불 켜기";
            }
            else
            {
                foreach (Light light in Lights) light.enabled = true;
                explanation = "불 끄기";
            }
        }
        else
        {
            switch (switchPlace)
            {
                case SwitchPlaces.RESTROOM:
                    changePassword(1);
                    break;
                case SwitchPlaces.BEDROOM:
                    changePassword(2);
                    break;
                case SwitchPlaces.LIVINGROOM:
                    changePassword(3);
                    break;
                case SwitchPlaces.HALLWAY:
                    changePassword(4);
                    break;
                default:
                    break;
            }
        }
        //isOn = !isOn;
        flipSwitch();
        isWorking = false;
    }
    
    public void flipSwitch()
    {
        if(isOn)
        {
            gameObject.GetComponent<AudioSource>().clip = switchDown;
            gameObject.GetComponent<AudioSource>().Play();

            if(loopForwardAnimation)
            {
                this.GetComponent<Animation>()["ToggleSwitch"].speed = animationSpeed;
                this.GetComponent<Animation>().Play("ToggleSwitch");
            }
            else
            {
                this.GetComponent<Animation>()["ToggleSwitch"].speed = animationSpeed;
                this.GetComponent<Animation>().Play("ToggleSwitch");
            }
            isOn = false;
        }
        else
        {
            gameObject.GetComponent<AudioSource>().clip = switchUp;
            gameObject.GetComponent<AudioSource>().Play();

            if(loopForwardAnimation)
            {
                this.GetComponent<Animation>()["ToggleSwitch"].speed = animationSpeed;
                this.GetComponent<Animation>().Play("ToggleSwitch");
            }
            else
            {
                this.GetComponent<Animation>()["ToggleSwitch"].speed = -animationSpeed;
                this.GetComponent<Animation>().Play("ToggleSwitch");
            }

            isOn = true;
        }

        // Tell parent Lighting System to toggle its state
        // We don't make it the same as the switch, because two-way switching systems
        // can have individual swithces in the off position but the lights are still on
        //transform.parent.GetComponent<LightingSystemScript>().toggleLights();
    }

    // Simply sets switch
    public void setSwitchStateToOn()
    {
        isOn = true;
        explanation = "불 끄기";
        this.GetComponent<Animation>()["ToggleSwitch"].speed = -animationSpeed;
        this.GetComponent<Animation>().Play("ToggleSwitch");
        
    }

    public void setSwitchStateToOff()
    {
        isOn = false;
        explanation = "불 켜기";
        this.GetComponent<Animation>()["ToggleSwitch"].speed = animationSpeed;
        this.GetComponent<Animation>().Play("ToggleSwitch");
        
    }

    public float getMaxSwitchDistance()
    {
        return maxSwitchDistance;
    }

    private void changePassword(int numtype)
    {
        if (sys_passwordManager.nowState == ChangePasswordManager.State.ChangePassword_723)
        {
            switch (numtype)
            {
                case 1:
                    if (sys_passwordManager.password723 % 10 == 9) sys_passwordManager.password723 -= 9;
                    else sys_passwordManager.password723 += 1;
                    break;
                case 2:
                    if ((sys_passwordManager.password723 /10)% 10 == 9) sys_passwordManager.password723 -= 90;
                    else sys_passwordManager.password723 += 10;
                    break;
                case 3:
                    if ((sys_passwordManager.password723 / 100) % 10 == 9) sys_passwordManager.password723 -= 900;
                    else sys_passwordManager.password723 += 100;
                    break;
                case 4:
                    if ((sys_passwordManager.password723 / 1000) % 10 == 9) sys_passwordManager.password723 -= 9000;
                    else sys_passwordManager.password723 += 1000;
                    break;
                default:
                    return;
            }
        }
        else if (sys_passwordManager.nowState == ChangePasswordManager.State.ChangePassword_724)
        {
            switch (numtype)
            {
                case 1:
                    if (sys_passwordManager.password724 % 10 == 9) sys_passwordManager.password724 -= 9;
                    else sys_passwordManager.password724 += 1;
                    break;
                case 2:
                    if ((sys_passwordManager.password724 / 10) % 10 == 9) sys_passwordManager.password724 -= 90;
                    else sys_passwordManager.password724 += 10;
                    break;
                case 3:
                    if ((sys_passwordManager.password724 / 100) % 10 == 9) sys_passwordManager.password724 -= 900;
                    else sys_passwordManager.password724 += 100;
                    break;
                case 4:
                    if ((sys_passwordManager.password724 / 1000) % 10 == 9) sys_passwordManager.password724 -= 9000;
                    else sys_passwordManager.password724 += 1000;
                    break;
                default:
                    return;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//All interaction object will inheritance this class
//DO NOT EDIT THIS CODE
public class InteractionTrigger : MonoBehaviour
{
    public bool isWorking = false;
    public string explanation;

    public virtual void InteractionFunction()
    {
        isWorking = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTrig_gosuguPoster : InteractionTrigger
{
    AudioSource audioSource;
    SubtitleTrigger subtitleTrigger;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        subtitleTrigger = GetComponent<SubtitleTrigger>();
    }

    private void Update()
    {
        if(isWorking)
        {
            if (subtitleTrigger.isShow == false)
            {
                isWorking = false;
            }
        }
    }

    public override void InteractionFunction()
    {
        base.InteractionFunction();
        subtitleTrigger.TriggerSubtitle();
        audioSource.Play();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionTrig_Door : InteractionTrigger
{
    [SerializeField] DoorAction action;

    AudioSource audioSource;
    [SerializeField] private AudioClip[] sounds = new AudioClip[3];
    [SerializeField] private float openVolume = 0.25f;
    [SerializeField] private float lockedVolume = 0.5f;

    [SerializeField]
    GameObject sys;
    private ChangePasswordManager sys_passwordManager;

    [SerializeField] EndingProduction endingProduction;
    public bool isLockedDoor = false;


    private void Start()
    {
        if (action.open) explanation = "¹® ´Ý±â";
        else explanation = "¹® ¿­±â";

        audioSource = GetComponent<AudioSource>();

        if (sys == null)
        {
            sys = GameObject.Find("System");
        }
        sys_passwordManager = sys.GetComponent<ChangePasswordManager>();
    }
    void Update()
    {
        if(isWorking&&!action.isMoving)
        {
            isWorking = false;
        }
    }
    public override void InteractionFunction()
    {
       base.InteractionFunction();
       if(!action.isMoving)
        {
            if (isLockedDoor && !sys_passwordManager.is724Open)
            {
                audioSource.clip = sounds[2];
                audioSource.volume = lockedVolume;
                audioSource.Play();
            }
            else if(isLockedDoor && sys_passwordManager.is724Open)
            {
                if (action.open) audioSource.clip = sounds[0];
                else if (!action.open) audioSource.clip = sounds[1];
                audioSource.volume = openVolume;
                audioSource.Play();

                action.open = !action.open;
                if (action.open) explanation = "¹® ´Ý±â";
                else explanation = "¹® ¿­±â";
                action.isMoving = true;

                //ADD CLEAR FUNCTION
                endingProduction.StartEnding();
            }
            else
            {
                if (action.open) audioSource.clip = sounds[0];
                else if (!action.open) audioSource.clip = sounds[1];
                audioSource.volume = openVolume;
                audioSource.Play();

                action.open = !action.open;
                if (action.open) explanation = "¹® ´Ý±â";
                else explanation = "¹® ¿­±â";
                action.isMoving = true;
            }  
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionTrig_Call : InteractionTrigger
{
    [SerializeField] private Camera focusCam;
    [SerializeField] private GameObject sys;
    [SerializeField] private GameObject[] buttons = new GameObject[10];
    [SerializeField] private AudioClip[] sounds = new AudioClip[10];
    [SerializeField] private float buttonVolume = 0.25f;
    [SerializeField] private float voiceVolume = 0.75f;
    AudioSource audioSource;
    SubtitleTrigger subtitleTrigger;

    int layerMaskForInteractionSec = 1 << 9;
    private float rayLength = 70f;
    [SerializeField] private String numString = null;
    [SerializeField] private int numCnt = 0;

    private ChangePasswordManager sys_passwordManager;

    [SerializeField] Texture2D blackcursor;
    [SerializeField] GameObject escapeUI;
    [SerializeField] TextMeshProUGUI escapeText;

    private CameraCon cameraCon;
    private enum Steps
    {
        notWork,
        showExplaination,
        getCallNum,
        checkResultFirst,
        getCharmNum,
        checkResultSec,
        resetCharmNum,
        endWork
    }
    private enum RoomNum
    {
        ROOM_NULL,
        ROOM_723,
        ROOM_724
    }

    [SerializeField] private Steps nowStep = Steps.notWork;
    [SerializeField] private RoomNum nowRoomNum = RoomNum.ROOM_NULL;
    private bool soundPlayed = false;


    // Start is called before the first frame update
    void Start()
    {
        cameraCon = sys.GetComponent<CameraCon>();
        focusCam.enabled = false;
        focusCam.GetComponent<AudioListener>().enabled = false;
        subtitleTrigger = GetComponent<SubtitleTrigger>();
        audioSource = GetComponent<AudioSource>();
        if (sys == null)
        {
            sys = GameObject.Find("System");
        }
        sys_passwordManager = sys.GetComponent<ChangePasswordManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isWorking)
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                isWorking = false;
                cameraCon.CloseInteractionCam(focusCam);
                subtitleTrigger.OffSubtitle();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                audioSource.Stop();
                escapeUI.SetActive(false);
                if (nowStep != Steps.resetCharmNum)
                {
                    nowStep = Steps.notWork;
                    nowRoomNum = RoomNum.ROOM_NULL;
                    soundPlayed = false;
                }
            }
            else
            {
                switch (nowStep)
                {
                    case Steps.notWork:
                        escapeUI.SetActive(true);
                        escapeText.text = "ESC로 나가기";
                        cameraCon.ShowInteractionCam(focusCam);
                        nowStep = Steps.showExplaination;
                        numString = null;
                        numCnt = 0;
                        audioSource.clip = null;
                        break;
                    case Steps.showExplaination:
                        nowStep = Steps.getCallNum;
                        break;
                    case Steps.getCallNum:
                        RaycastHit hit;

                        Ray touchray = focusCam.ScreenPointToRay(Input.mousePosition);
                        Physics.Raycast(touchray, out hit, rayLength, layerMaskForInteractionSec);

                        if (Input.GetMouseButtonDown(0) && hit.collider != null)
                        {
                            numString += hit.collider.gameObject.name[0];
                            numCnt++;
                            string tmp_buttonNum = null;
                            tmp_buttonNum += hit.collider.gameObject.name[0];
                            if (hit.collider.gameObject.name[0] == '#') audioSource.clip = sounds[10];
                            else if (hit.collider.gameObject.name[0] == '*') audioSource.clip = sounds[11];
                            else audioSource.clip = sounds[Int32.Parse(tmp_buttonNum)];
                            audioSource.volume = buttonVolume;
                            audioSource.Play();
                        }
                        if(numString != null)
                        {
                            subtitleTrigger.ShowOneSentence(numString, "(번호)");
                        }
                        if (numCnt >= 11)
                        {
                            nowStep = Steps.checkResultFirst;
                        }
                        break;
                    case Steps.checkResultFirst:
                        if((numString == "72300240724") || (numString == "72400240724"))
                        {
                            if (numString == "72300240724") nowRoomNum = RoomNum.ROOM_723;
                            else if (numString == "72400240724") nowRoomNum = RoomNum.ROOM_724;

                            subtitleTrigger.TriggerSubtitle(0, 1);
                            audioSource.clip = sounds[12];
                            audioSource.volume = voiceVolume;
                            audioSource.Play();
                            nowStep = Steps.getCharmNum;
                            numString = null;
                            numCnt = 0;                       
                        }
                        else if((numString == "70014200116") || (numString == "70014200610")|| (numString == "70006100309") || (numString == "70006101008"))
                        {
                            subtitleTrigger.ShowOneSentence("상담 가능 시간이 아닙니다.");
                            audioSource.clip = sounds[19];
                            audioSource.volume = voiceVolume;
                            audioSource.Play();
                            nowStep = Steps.getCallNum;
                            numString = null;
                            numCnt = 0;
                        }
                        else if (Int32.Parse(numString.Substring(0,3))>700 && Int32.Parse(numString.Substring(0, 3)) < 741 && numString.Substring(3, numString.Length-3) =="00240724" )
                        {
                            subtitleTrigger.ShowOneSentence("알 수 없는 오류가 발생했습니다.");
                            audioSource.clip = sounds[20];
                            audioSource.volume = voiceVolume;
                            audioSource.Play();
                            nowStep = Steps.getCallNum;
                            numString = null;
                            numCnt = 0;
                        }
                        else
                        {
                            subtitleTrigger.ShowOneSentence("존재하지 않는 번호입니다.");
                            if (!soundPlayed)
                            {
                                audioSource.clip = sounds[13];
                                audioSource.volume = voiceVolume;
                                audioSource.Play();
                            }
                            numString = null;
                            numCnt = 0;
                            nowStep = Steps.getCallNum;
                        }
                        break;
                    case Steps.getCharmNum:
                        touchray = focusCam.ScreenPointToRay(Input.mousePosition);
                        Physics.Raycast(touchray, out hit, rayLength, layerMaskForInteractionSec);

                        if (Input.GetMouseButtonDown(0) && hit.collider != null)
                        {
                            if (subtitleTrigger.isShow)
                            {
                                subtitleTrigger.OffSubtitle();
                            }
                            numString += hit.collider.gameObject.name[0];
                            numCnt++;
                            string tmp_buttonNum = null;
                            tmp_buttonNum += hit.collider.gameObject.name[0];
                            if (hit.collider.gameObject.name[0] == '#') audioSource.clip = sounds[10];
                            else if (hit.collider.gameObject.name[0] == '*') audioSource.clip = sounds[11];
                            else audioSource.clip = sounds[Int32.Parse(tmp_buttonNum)];
                            audioSource.volume = buttonVolume;
                            audioSource.Play();
                        }
                        if (numString != null)
                        {
                            subtitleTrigger.ShowOneSentence(numString, "(매력코드)");
                        }
                        if (numCnt >= 7)
                        {
                            nowStep = Steps.checkResultSec;
                        }
                        break;
                    case Steps.checkResultSec:
                        if (numString == "1678915")
                        {
                            nowStep = Steps.resetCharmNum;
                            numString = null;
                            if (nowRoomNum == RoomNum.ROOM_723) audioSource.clip = sounds[14];
                            else if (nowRoomNum == RoomNum.ROOM_724) audioSource.clip = sounds[15];
                            audioSource.volume = voiceVolume;
                            audioSource.Play();
                        }
                        else
                        {
                            subtitleTrigger.ShowOneSentence("매력코드가 일치하지 않습니다.");
                            numString = null;
                            numCnt = 0;
                            audioSource.clip = sounds[16];
                            audioSource.volume = voiceVolume;
                            audioSource.Play();
                            nowStep = Steps.getCharmNum;
                        }
                        break;
                    case Steps.resetCharmNum:
                        if(cameraCon.nowCam != CameraCon.CamType.interactionCam) cameraCon.ShowInteractionCam(focusCam);
                        string tmp_roomNum = null;
                        if (nowRoomNum == RoomNum.ROOM_723)
                        {
                            sys_passwordManager.nowState = ChangePasswordManager.State.ChangePassword_723;
                            tmp_roomNum = "723";
                        }
                        else if (nowRoomNum == RoomNum.ROOM_724)
                        {
                            sys_passwordManager.nowState = ChangePasswordManager.State.ChangePassword_724;
                            tmp_roomNum = "724";
                        }
                        subtitleTrigger.ShowOneSentence(tmp_roomNum + "호 비밀번호를 재설정합니다. 스위치를 작동시켜 비밀번호를 변경한 후 #을 눌러주세요.");
                        if(escapeUI.activeSelf == false) escapeUI.SetActive(true);
                        escapeText.text = "ESC로 나가기 (대기 중)";

                        touchray = focusCam.ScreenPointToRay(Input.mousePosition);
                        Physics.Raycast(touchray, out hit, rayLength, layerMaskForInteractionSec);

                        if (Input.GetMouseButtonDown(0) && hit.collider != null)
                        {
                            numString += hit.collider.gameObject.name[0];
                            numCnt++;
                            string tmp_buttonNum = null;
                            tmp_buttonNum += hit.collider.gameObject.name[0];
                            audioSource.volume = buttonVolume;
                            if (hit.collider.gameObject.name[0] == '#')
                            {
                                nowStep = Steps.endWork;
                                if(nowRoomNum == RoomNum.ROOM_723) audioSource.clip = sounds[17];
                                else if(nowRoomNum == RoomNum.ROOM_724) audioSource.clip = sounds[18];
                                audioSource.volume = voiceVolume;
                                soundPlayed = true;
                                escapeText.text = "ESC로 나가기";
                            }
                            else if (hit.collider.gameObject.name[0] == '*') audioSource.clip = sounds[11];
                            else audioSource.clip = sounds[Int32.Parse(tmp_buttonNum)];
                            audioSource.Play();
                        }
                        break;
                    case Steps.endWork:
                        tmp_roomNum = null;
                        if (nowRoomNum == RoomNum.ROOM_723) tmp_roomNum = "723";
                        else if (nowRoomNum == RoomNum.ROOM_724) tmp_roomNum = "724";
                        subtitleTrigger.ShowOneSentence(tmp_roomNum + "호 비밀번호의 재설정이 완료되었습니다.");
                        sys_passwordManager.nowState = ChangePasswordManager.State.NotWorking;
                        break;
                    default:
                        break;
                }
            }   
        }
    }

    public override void InteractionFunction()
    {
        base.InteractionFunction();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(blackcursor, new Vector2(blackcursor.width / 2, blackcursor.height / 2), CursorMode.ForceSoftware);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractionShowImg : InteractionTrigger
{
    [SerializeField]
    bool isShow = false;

    [SerializeField]
    GameObject showImgUI;
    [SerializeField]
    Image imageUI;
    [SerializeField]
    Sprite imageSource;
    [SerializeField]
    GameObject sys;

    private AudioSource audioSource;


    private void Start()
    {
        isShow = false;
        explanation = "살펴보기";
        if (sys == null)
        {
            sys = GameObject.Find("System");
        }
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isWorking && isShow)
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                isWorking = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                sys.GetComponent<CameraCon>().CloseInteractionShowObject();
            }
        }
        if (!isWorking && isShow)
        {
            showImgUI.SetActive(false);
            isShow = false;
        }
    }

    public override void InteractionFunction()
    {
        base.InteractionFunction();
        isShow = true;
        showImgUI.SetActive(true);
        imageUI.sprite = imageSource;
        sys.GetComponent<CameraCon>().ShowInteractionShowObject();
        audioSource.Play();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionRefigeratorDoor : InteractionTrigger
{
    private bool isOpen = false;
    [SerializeField] float doorDelay = 0f;
    [SerializeField] float doorOpenAngle = -90f;
    [SerializeField] float doorCloseAngle = 0f;
    [SerializeField] float smoot = 3f;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] sounds;
    private bool isPlaySound = false;
    // Start is called before the first frame update
    void Start()
    {
        if (isOpen) explanation = "¹® ´Ý±â";
        else explanation = "¹® ¿­±â";
    }
    private void Update()
    {
        if (isWorking)
        {
            if (isOpen)
            {
                doorDelay += Time.deltaTime;
                Quaternion targetRotation = Quaternion.Euler(0, doorOpenAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smoot * Time.deltaTime);
                if(!isPlaySound)
                {
                    audioSource.clip = sounds[0];
                    audioSource.Play();
                    isPlaySound = true;
                }
            }
            else
            {
                doorDelay += Time.deltaTime;
                Quaternion targetRotation2 = Quaternion.Euler(0, doorCloseAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation2, smoot * Time.deltaTime);
                if (!isPlaySound)
                {
                    audioSource.clip = sounds[1];
                    audioSource.Play();
                    isPlaySound = true;
                }
            }
            if (doorDelay > 2f)
            {
                isWorking = false;
                doorDelay = 0f;
                isPlaySound = false;
            }
        }
    }

    public override void InteractionFunction()
    {
        base.InteractionFunction();
        isWorking = true;
        isOpen = !isOpen;
        if (isOpen) explanation = "¹® ´Ý±â";
        else explanation = "¹® ¿­±â";
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionDoorLock : InteractionTrigger
{
    [SerializeField] private Camera focusCam;
    [SerializeField] private GameObject sys;
    [SerializeField] private AudioClip[] sounds = new AudioClip[3];
    [SerializeField] private float buttonVolume = 0.25f;
    [SerializeField] private float wrongVolume = 0.5f;
    [SerializeField] private float answerVolume = 0.5f;

    AudioSource audioSource;

    int layerMaskForInteractionSec = 1 << 9;
    private float rayLength = 70f;
    [SerializeField] private String numString = null;
    [SerializeField] private int numCnt = 0;

    [SerializeField] private GameObject keyPadUI;
    [SerializeField] private GameObject escUI;
    [SerializeField] private TextMeshProUGUI escText;

    private ChangePasswordManager sys_passwordManager;

    private CameraCon cameraCon;

    private enum Steps
    {
        notWork,
        showDoorLock,
        checkPassword
    }
    private enum RoomNum
    {
        ROOM_NULL,
        ROOM_723,
        ROOM_724
    }

    [SerializeField] private Steps nowStep = Steps.notWork;
    [SerializeField] private RoomNum nowRoomNum = RoomNum.ROOM_NULL;
    private bool soundPlayed = false;

    [SerializeField] Texture2D whiteCursor;

    // Start is called before the first frame update
    void Start()
    {
        cameraCon = sys.GetComponent<CameraCon>();
        focusCam.enabled = false;
        focusCam.GetComponent<AudioListener>().enabled = false;
        audioSource = GetComponent<AudioSource>();
        if (sys == null)
        {
            sys = GameObject.Find("System");
        }
        sys_passwordManager = sys.GetComponent<ChangePasswordManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isWorking)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                isWorking = false;
                cameraCon.CloseInteractionCam(focusCam);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                audioSource.Stop();
                keyPadUI.SetActive(false);
                escUI.SetActive(false);
                nowStep = Steps.notWork;
            }
            else
            {
                switch (nowStep)
                {
                    case Steps.notWork:
                        cameraCon.ShowInteractionCam(focusCam);
                        nowStep = Steps.showDoorLock;
                        numString = null;
                        numCnt = 0;
                        audioSource.clip = null;
                        break;
                    case Steps.showDoorLock:
                        if(numCnt>=4)
                        {
                            nowStep = Steps.checkPassword;
                        }
            
                        break;
                    case Steps.checkPassword:
                        String tmp = null;
                        if(nowRoomNum == RoomNum.ROOM_723) tmp = sys_passwordManager.password723.ToString();
                        else if(nowRoomNum == RoomNum.ROOM_724) tmp = sys_passwordManager.password724.ToString();
                        while (tmp.Length < 4) tmp = "0" + tmp;
                        if (numString == tmp)
                        {
                            isWorking = false;
                            cameraCon.CloseInteractionCam(focusCam);
                            Cursor.visible = false;
                            Cursor.lockState = CursorLockMode.Locked;
                            audioSource.Stop();
                            keyPadUI.SetActive(false);
                            escUI.SetActive(false);
                            nowStep = Steps.notWork;

                            if (nowRoomNum == RoomNum.ROOM_724)
                            {
                                sys_passwordManager.is724Open = true;
                                this.gameObject.layer = 3;
                            }
                            audioSource.clip = sounds[2];
                            audioSource.volume = answerVolume;
                            audioSource.Play();
                        }
                        else
                        {
                            numString = null;
                            numCnt = 0;
                            audioSource.clip = sounds[1];
                            audioSource.volume = wrongVolume;
                            audioSource.Play();
                            nowStep = Steps.showDoorLock;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public override void InteractionFunction()
    {
        base.InteractionFunction();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(whiteCursor, new Vector2(whiteCursor.width / 2, whiteCursor.height / 2), CursorMode.ForceSoftware);
        keyPadUI.SetActive(true);
        escUI.SetActive(true);
        escText.text = "ESC로 나가기";
    }

    public void keypadButton(String n)
    {
        numString += n;
        audioSource.clip = sounds[0];
        audioSource.volume = buttonVolume;
        audioSource.Play();
        numCnt++;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class EndingProduction : MonoBehaviour
{
    [SerializeField] GameObject sys;

    [SerializeField] GameObject messi;
    [SerializeField] GameObject wakgood;
    [SerializeField] GameObject realwakgood;
    private Animator animator;
    AudioSource AS;

    [SerializeField] Camera wakCam;
    [SerializeField] AudioClip dub2;
    [SerializeField] SubtitleTrigger dev;
    [SerializeField] SubtitleTrigger doc;

    float time;
    float time2;
    float time3;
    float time4;
    float time5;
    float time6;
    float time7;
    bool go = false;
    bool go2 = false;
    bool go3 = false;

    [SerializeField] GameObject endingTextUI;
    [SerializeField] Text end1;
    [SerializeField] Text end2;
    float a = 0f;
    float a2 = 0f;
    float a3 = 0f;
    float a4 = 1f;
    float a5 = 1f;

    [SerializeField] CheckTime checkTime;

    [SerializeField] GameObject tui;
    [SerializeField] Text ttry;

    [SerializeField] CameraCon camHandler;
    [SerializeField] endingButton endingButtonfunc;

    [SerializeField] Image fade;

    [SerializeField] GameObject cursorUI;
    [SerializeField] RawImage c;

    [SerializeField] Light main;
    [SerializeField] Light forMessi;
    [SerializeField] Light bath;

    // Start is called before the first frame update
    void Start()
    {
        if (messi != null && realwakgood != null && wakCam != null)
        {
            animator = messi.GetComponent<Animator>();
            AS = wakCam.GetComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (go)
        {
            time += Time.deltaTime;
            if (time > 1.75f)
            {
                realwakgood.GetComponent<RotateToMoues>().enabled = false;
                realwakgood.GetComponent<WakMove>().enabled = false;
                realwakgood.GetComponent<WakController>().enabled = false;
                sys.GetComponent<CameraCon>().enabled = false;
                Cursor.visible = false;
                if (dub2 != null)
                {
                    AS.clip = dub2;
                    AS.Play();
                    dev.TriggerSubtitle();
                    doc.TriggerSubtitle();

                }
                go2 = true;
                go = false;
            }
        }
        if (go2)
        {
            time2 += Time.deltaTime;
            Debug.Log(end1.color);
            if (time2 > 44f)
            {
                time7 += Time.deltaTime;
                if (a5 > 0.0f && time7 >= 0.1f)
                {
                    a5 -= 0.1f;
                    end2.color = new Color(255, 255, 255, a5);
                    time7 = 0;
                }
                if (a5 < 0)
                {
                    go2 = false;
                    go3 = true;
                }
            }
            else if (time2 > 34f)
            {
                time6 += Time.deltaTime;
                if (a3 < 1.0f && time6 >= 0.1f)
                {
                    a3 += 0.1f;
                    end2.color = new Color(255, 255, 255, a3);
                    time6 = 0;
                }
            }
            else if (time2 > 32f)
            {
                time5 += Time.deltaTime;
                if (a4 > 0.0f && time5 >= 0.1f)
                {
                    a4 -= 0.1f;
                    end1.color = new Color(255, 255, 255, a4);
                    time5 = 0;
                }
            }
            else if (time2 > 22f)
            {
                time4 += Time.deltaTime;
                if (a2 < 1.0f && time4 >= 0.1f)
                {
                    a2 += 0.1f;
                    end1.color = new Color(255, 255, 255, a2);
                    time4 = 0;
                }
            }
            else if (time2 > 18.5f)
            {
                tui.SetActive(false);
                endingTextUI.SetActive(true);
                time3 += Time.deltaTime;
                if (a < 1.0f && time3 >= 0.1f)
                {
                    a += 0.05f;
                    fade.color = new Color(0, 0, 0, a);
                    time3 = 0;
                }
            }
            else if (time2 > 15.5f)
            {
                ttry.text = "Æ®¶óÀÌ È½¼ö : "+GameManager.gameTry.ToString();
                tui.SetActive(true);
            }
        }
        if (go3)
        {
            camHandler.EndCameraSet();
            if (endingTextUI.activeSelf == true) endingTextUI.SetActive(false);
            endingButtonfunc.ShowEndingButton();
            go3 = false;
        }
    }

    public void StartEnding()
    {
        if (messi != null)
        {
            c.color = new Color(0, 0, 0, 0);
            c.enabled = false;
            if (main != null && forMessi != null && bath != null && realwakgood != null && wakCam != null && checkTime != null && tui != null)
            {
                AS.Stop();
                checkTime.enabled = false;
                main.enabled = false;
                bath.enabled = false;
                //c.color = new Color (0, 0, 0, 0);

                cursorUI.SetActive(false);
                forMessi.GetComponent<Light>().enabled = true;
            }
            messi.GetComponent<messiMove>().enabled = false;
            messi.transform.position = new Vector3(340.2f, 20.18f, -1109.19f);
            animator.SetBool("isEnd", true);
            if (wakgood != null && realwakgood != null)
            {
                go = true;
                messi.transform.LookAt(wakgood.transform);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class endingButton : MonoBehaviour
{
    [SerializeField] GameObject endingButtonUI;
    [SerializeField] GameObject showImgUI;
    [SerializeField] Image img;
    [SerializeField] Image bgImg;
    [SerializeField] GameObject showLRButtonUI;
    [SerializeField] GameObject rButton;
    [SerializeField] GameObject lButton;

    [SerializeField] Sprite[] imgs;
    enum Steps
    {
        notShow,
        showEndingButton,
        showImg,
        quitGame
    }
    [SerializeField] Steps nowStep = Steps.notShow;

    int page = 1;
    [SerializeField] int maxPage = 3;
    bool isLoad = true;

    // Start is called before the first frame update
    void Start()
    {
        nowStep = Steps.notShow;
        isLoad = false;
    }

    // Update is called once per frame
    void Update()
    {
        switch (nowStep)
        {
            case Steps.notShow:
                if (!isLoad)
                {
                    endingButtonUI.SetActive(false);
                    showImgUI.SetActive(false);
                    showLRButtonUI.SetActive(false);
                    rButton.SetActive(false);
                    lButton.SetActive(false);
                    ShowEndingButton();
                    isLoad = true;
                }
                break;
            case Steps.showEndingButton:
                if(!isLoad)
                {
                    endingButtonUI.SetActive(true);
                    showImgUI.SetActive(false);
                    showLRButtonUI.SetActive(false);
                    rButton.SetActive(false);
                    lButton.SetActive(false);
                    isLoad = true;
                    Debug.Log("Load complete");
                }
                break;
            case Steps.showImg:
                if(Input.GetKeyUp(KeyCode.Escape))
                {
                    ShowEndingButton();
                    break;
                }
                if(!isLoad)
                {
                    isLoad = true;
                    if (page < maxPage) rButton.SetActive(true);
                    else rButton.SetActive(false);
                    if (page > 1) lButton.SetActive(true);
                    else lButton.SetActive(false);
                    img.sprite = imgs[page - 1]; 
                }
                break;
            default:
                break;
        }
    }

    public void ShowEndingButton()
    {
        nowStep = Steps.showEndingButton;
        isLoad = false;
        Debug.Log("ShowEndingButton() Call");
    }
    public void ButtonShowImg()
    {
        endingButtonUI.SetActive(false);
        showImgUI.SetActive(true);
        showLRButtonUI.SetActive(true);
        rButton.SetActive(false);
        lButton.SetActive(false);
        isLoad = false;
        nowStep = Steps.showImg;
        bgImg.color = Color.black;
        img.rectTransform.sizeDelta = new Vector2(1280,720);
        page = 1;
        Debug.Log("ButtonShowImg() Call");
    }
    public void ButtonEndGame()
    {
        nowStep = Steps.quitGame;
        Debug.Log("ButtonEndGame() Call");
        Application.Quit();
    }
    public void ButtonRight()
    {
        page++;
        if(page>maxPage) page = maxPage;
        isLoad = false;
    }
    public void ButtonLeft()
    {
        page--;
        if(page<1) page = 1;
        isLoad = false;
    }
}