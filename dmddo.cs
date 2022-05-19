using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
    private int SceneNum;
    static int BeforSceneNum;
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(this);
        BeforSceneNum = SceneManager.GetActiveScene().buildIndex;
    }

    // Update is called once per framea
    void Update()
    {
        SceneNum = SceneManager.GetActiveScene().buildIndex;
        if (BeforSceneNum != SceneNum)
            GameObject.Destroy(this);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tag_Fruits : MonoBehaviour
{

    private GameObject Fruit;
    // Start is called before the first frame update
    public void Delete()
    {
        Fruit = GameObject.FindGameObjectWithTag("Fruits");

    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class cameraMove : MonoBehaviour
{

    public Transform main_camera;
    public Transform move_camera;

    static public bool isStart = false;
    private bool Move_end = false;

    void Update()
    {   
        if (isStart && !Move_end)
            loopAll();

        if (Move_end) {
            SceneManager.LoadScene(1);
        }

    }
    

    public void loopAll()
    {
        isStart = true;
        main_camera = GameObject.FindGameObjectWithTag("MainCamera").transform;

        move_camera.transform.position = Vector3.MoveTowards(move_camera.transform.position, main_camera.transform.position, Time.deltaTime * 200);
        move_camera.transform.rotation = Quaternion.RotateTowards(move_camera.transform.rotation, main_camera.transform.rotation, Time.deltaTime * 20);

        if (move_camera.transform.position == main_camera.transform.position && move_camera.transform.rotation == main_camera.transform.rotation)
            Move_end = true;
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class cameraReStart : MonoBehaviour
{

    public Transform main_camera;
    public Transform move_camera;

    public bool Turn_ONOFF = false;
    private bool Move_end = false;

    void Update()
    {
        if (Turn_ONOFF && !Move_end)
            loopAlls();

    }


    public void loopAlls()
    {
        main_camera = GameObject.FindGameObjectWithTag("MainCamera").transform;

        move_camera.transform.position = Vector3.MoveTowards(move_camera.transform.position, main_camera.transform.position, Time.deltaTime * 200);
        move_camera.transform.rotation = Quaternion.RotateTowards(move_camera.transform.rotation, main_camera.transform.rotation, Time.deltaTime * 20);

        if (move_camera.transform.position == main_camera.transform.position && move_camera.transform.rotation == main_camera.transform.rotation)
            Move_end = true;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{

    private int StageNum;

    private void Start()
    {
        StageNum = SceneNumber_Script.SceneCount;
    }


    public void RetryNumScene()
    {
        PlayerHpBar.Pdie = false;
        SceneManager.LoadScene(StageNum);
        waterfly.amIDestroyed = true;
    }

    public void NextStage(){
        PlayerHpBar.Pdie = false;
        StageNum += 1;
        SceneManager.LoadScene(StageNum);
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNumber_Script : MonoBehaviour
{
    static public int SceneCount;
    // Start is called before the first frame update
    void Start()
    {
        SceneCount = SceneManager.GetActiveScene().buildIndex;
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public Text ScoreText;

    // Start is called before the first frame update
    void Update()
    {
        print_score();
    }

    private void print_score()
    {
        ScoreText.text = "" + GameManager.Score;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimedelayText : MonoBehaviour
{
    Text flashingText;
    // Use this for initialization
    void Start () 
    {
        flashingText = GetComponent<Text> ();
        StartCoroutine (BlinkText());
    }
    
    public IEnumerator BlinkText()
    {
        while (true)
        {
            flashingText.text = "";
            yield return new WaitForSeconds (0.7f);
            flashingText.text = "Touch To Start";
            yield return new WaitForSeconds (0.7f);
        }
            
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update

    static public int speedB = 30;
    private Rigidbody bulletRigidbody;
    PlayerHpBar healthManager;


    void Start()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
        bulletRigidbody.velocity = transform.forward * speedB;
        healthManager = FindObjectOfType<PlayerHpBar>();
        Destroy(gameObject, 20.0f);
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "AimToPlayer")
        {
            Debug.Log("총에 맞았습니다");
            Player agent = other.GetComponent<Player>();

            healthManager.Dmg();

            if (agent != null)
            {
                agent.Die();
            }
        }

        else if (other.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Destroy(gameObject);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject moveAim1;
    public GameObject backAim1;

    private Transform target;
    private Transform chasetarget;
    private float spawnRate;
    private float timeAfterSpawn;
    private bool isMove = true;
    private bool isShoot = false;
    private bool isLine = false;
    private bool isFirstMove = true;

    [SerializeField]
    private Animator animator;

    private float bugSpeed;
    private float PosGap;
    private float adjustT;
    Vector3 playerPos;
    Vector3 bugPos;

    Rigidbody rigid;

    private bool Die_bool;

    private void Awake()
    {
        //animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        timeAfterSpawn = 0;
        spawnRate = 1.1f;
        target = GameObject.FindGameObjectWithTag("AimToPlayer").transform;
        chasetarget = GameObject.FindGameObjectWithTag("chasetarget").transform;
        Die_bool = GameObject.Find("EnemyHpBar_Canvas").GetComponent<EnemyHpBar>().Edie;
    }

    // Update is called once per frame
    void Update()
    {
        bugPos = gameObject.transform.position;
        playerPos = target.position;
        PosGap = Vector3.Distance(playerPos, bugPos);
        //Debug.Log(PosGap);

        if (Die_bool == true)
        {
            animator.SetBool("isDead", true);
        }

        else if ((isMove == false) && (isLine != true))
        {
            timeAfterSpawn += Time.deltaTime;
            if (PosGap > 60f)
            {
                animator.SetBool("isMove", false);
                isShoot = true;
                transform.LookAt(target.transform.position);
                //isMove = true;
                Bullet.speedB = 55;

                if (timeAfterSpawn > spawnRate)
                {
                    timeAfterSpawn = 0f;

                    if (isShoot == true)
                    {
                        animator.SetBool("isShoot", true);
                    }

                    GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);

                    bullet.transform.LookAt(target);



                    if (isShoot == false)
                    {
                        animator.SetBool("isShoot", false);
                    }
                }
            }
            else if (PosGap <= 60f)
            {
                animator.SetBool("isShoot", false);
                isShoot = false;
                animator.SetBool("isMove", true);
                transform.LookAt(chasetarget.transform.position);
                bugSpeed = 0.07f;
                Bullet.speedB = 25;
                transform.Translate(0f, 0f, bugSpeed);

                if (timeAfterSpawn > spawnRate)
                {
                    timeAfterSpawn = 0f;

                    if (isShoot == true)
                    {
                        animator.SetBool("isShoot", true);
                    }

                    GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);

                    bullet.transform.LookAt(chasetarget);



                    if (isShoot == false)
                    {
                        animator.SetBool("isShoot", false);
                    }
                }
            }

        }

        else if (isMove == true)
        {
            animator.SetBool("isMove", true);
            transform.LookAt(moveAim1.transform.position);
            bugSpeed = 0.15f;
            transform.position = Vector3.MoveTowards(gameObject.transform.position, moveAim1.transform.position, bugSpeed);
        }

        else if (isLine == true)
        {
            animator.SetBool("isShoot", false);
            animator.SetBool("isMove", true);
            transform.LookAt(backAim1.transform.position);
            bugSpeed = 0.15f;
            transform.position = Vector3.MoveTowards(gameObject.transform.position, backAim1.transform.position, bugSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "MoveAim")
        {
            isMove = false;
            isLine = false;
            animator.SetBool("isMove", isMove);
            Invoke("Update", 1.0f);
            transform.LookAt(target.transform.position);
        }

        else if (other.gameObject.tag == "Line")
        {
            isLine = true;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Banana")
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
        }
    }



    public void Dead()  //왜 오류?
    {
        Debug.Log("Dead 함수 호출");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    public GameObject enemyCanvas;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Water")
        {
            Debug.Log("Hit!");
            enemyCanvas.GetComponent<EnemyHpBar> ().Dmg();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpBar : MonoBehaviour
{
    public GameObject Enemy;
    public Slider shSlider;
    public Slider hpSlider;
    public Slider BackHpSlider;
    public bool backHpHit = false;

    public Transform enemy;
    public float maxHP = 30f;
    public float currentHp = 30f;

    public float maxSH = 30f;
    public float currentSH = 30f;

    public bool Edie = false;

    [SerializeField]
    public Transform Main_cam;

    [SerializeField]
    public GameObject Color_Ball;



    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(Main_cam.transform);
        this.transform.Rotate(0,180,0);
        
        this.transform.position = enemy.position + new Vector3(0, 4f, 0);
        if (currentSH > 0 )
        {
            shSlider.value = Mathf.Lerp(shSlider.value, currentSH / maxSH, Time.deltaTime * 5f);
        }
        else
            hpSlider.value = Mathf.Lerp(hpSlider.value, currentHp / maxHP, Time.deltaTime * 5f);

        if (backHpHit)
        {
            BackHpSlider.value = Mathf.Lerp(BackHpSlider.value, hpSlider.value, Time.deltaTime * 10f);
        }
    }    

    public void Dmg()
    {
        if (currentSH > 0)
        {
            if (waterfly.isFrozen)
                currentSH -= 10f;
            else {
                Color_Ball.GetComponent<MeshRenderer>().enabled = true;
                Invoke("Mesh_ball", 0.2f);
            }
            if (currentSH == 0)
            {
                shSlider.gameObject.SetActive(false);
            }
        }
        else
        {
            currentHp -= 10f;
            Invoke("BackHpFun", 0.5f);

            if (currentHp <= 0){
                Edie = true;
                hpSlider.gameObject.SetActive(false);
                Enemy.gameObject.SetActive(false);
            }
        }
    }

    void Mesh_ball() {
        Color_Ball.GetComponent<MeshRenderer>().enabled = false;
    }

    void BackHpFun()
    {
        backHpHit = true;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour//, IPointerDownHandler
{
    public GameObject gameoverText;
    //public Text[] timeText;
    public Text TimeText;
    public Text ScoreText;
    // public Text recordText;

    public Canvas Main_canvas;
    public Canvas canvas;

    private string boo;
    private string cho;

    static public bool isDieAll;
    static public int Score = 0;


    private float surviveTime;
    static public bool isGameover;
    //private bool restart = false;

    private float timeS;
    private int healthy_enemy;

    public GameObject[] E_list;

    //private GameManager Script;

    [SerializeField]
    public GameObject MC;


    //public void Awake()
    //{
    //    this.gameObject.SetActive(false);
    //    GameManager gameManager = FindObjectOfType<GameManager>();
        
    //}

    //public void OnPointerDown(PointerEventData eventData)
    //{
    //    // if (-500 < (int)eventData.position.x < 500)
    //    Debug.Log("�� �۵��Ѵ�");
    //    if (isGameover)
    //    {
    //        restart = true;
    //    }
    //}

    // Start is called before the first frame update
    void Start()
    {
        surviveTime = 0;
        isGameover = false;
        E_list = GameObject.FindGameObjectsWithTag("EnemySet");
    }

    // Update is called once per frame
    void Update()
    {
        if (E_list.Length >= 1)
        {
            healthy_enemy = E_list.Length;
            for (int i = 0; i < E_list.Length; i++)
            {
                if (E_list[i].activeSelf == false)
                    healthy_enemy -= 1;
            }

            Debug.Log(healthy_enemy);
            if (healthy_enemy == 0)
            {
                isDieAll = true;
                isGameover = true;
            }
        }
        else 
        {
            isGameover = true;
            isDieAll = false;
        }


        if (!isGameover)
        {
            surviveTime += Time.deltaTime;
            timeS = (int)surviveTime;

            boo = (((int)surviveTime / 60 % 60)).ToString();
            cho = ((int)surviveTime % 60).ToString();
            if (((int)surviveTime % 60 < 10) && (((int)surviveTime / 60 % 60) < 10))
                TimeText.text = "0" + boo + " : " + "0" + cho;
            else if (((int)surviveTime % 60 >= 10) && (((int)surviveTime / 60 % 60) < 10))
                TimeText.text = "0" + boo + " : " + cho;
            else if (((int)surviveTime % 60 < 10) && (((int)surviveTime / 60 % 60) >= 10))
                TimeText.text = boo + " : " + "0" + cho;
            else
                TimeText.text = boo + " : " + cho;

            ScoreText.text = "Score: " + Score;
        }
        else
        {
            if (isDieAll == true)
            {
                Score = (int)(50000 * (waterfly.hitNum / Pshoot.shootNum));
                Debug.Log(Score);
                ScoreText.text = "Score: " + Score;

                Main_canvas.gameObject.SetActive(false);

                Invoke("allDieOK", 0.5f);
                Invoke("TurnCanvas", 2.05f);
            }
        }
    }

    public void EndGame()
    {
        isGameover = true;
        gameoverText.SetActive(true);

       

        //float bestTime = PlayerPrefs.GetFloat("BestTime"); // ���̺� ����

        //if (surviveTime > bestTime)
        //{
        //    bestTime = surviveTime;
        //    PlayerPrefs.SetFloat("BestTime", bestTime);
        //}

        //recordText.text = "Best Time: " + (int)bestTime;
    }


    private void allDieOK() {
        MC.GetComponent<cameraReStart>().Turn_ONOFF = true;
    }

    private void TurnCanvas() {
        canvas.gameObject.SetActive(true);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    //[SerializeField]
    //private Image imageBackground;
    //private Image imageController;

    [SerializeField]
    private RectTransform imageBackground;

    [SerializeField]
    private RectTransform imageController;



    private Vector2 touchPosition;

    private void Awake()
    {
        //imageBackground = GetComponent<Image>();
        //imageController = transform.GetChild(0).GetComponent<Image>();

        imageBackground = GetComponent<RectTransform>();
        imageController = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {

    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 touchPosition = Vector2.zero;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(imageBackground, eventData.position, eventData.pressEventCamera, out touchPosition))
        {
            touchPosition.x = (touchPosition.x / imageBackground.sizeDelta.x);
            touchPosition.y = (touchPosition.y / imageBackground.sizeDelta.y);

            touchPosition = new Vector2(touchPosition.x * 2 + 1, touchPosition.y * 2 + 1);

            touchPosition = (touchPosition.magnitude > 1) ? touchPosition.normalized : touchPosition;

            imageController.anchoredPosition = new Vector2(
                touchPosition.x * imageBackground.sizeDelta.x / 2,
                touchPosition.y * imageBackground.sizeDelta.y / 2);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        imageController.anchoredPosition = Vector2.zero;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    static public float lineRoty;


    // Start is called before the first frame update
    void Start()
    { 
        lineRoty = GetComponent<Transform>().rotation.y;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour
{
    [SerializeField]
    private Transform characterBody;
    [SerializeField]
    private Transform cameraArm;

    [SerializeField]
    private Transform player_body;

    Animator animator;

    private float LineRoty;
    bool jump;
    Rigidbody rdby;

    //[SerializeField]
    //private float jumpower;

    private bool isBorder = false;

    private void Awake()
    {
        rdby = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = characterBody.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //LookAround();
        //Move();
        Line.lineRoty = 0f;
        if (PlayerHpBar.Pdie && !GameManager.isDieAll)
            SceneManager.LoadScene("GameOver");
        StopToWall();
            
    }

    public void Move(Vector2 inputDirection)
    {
        // 이동 방향 구하기 1
        //Debug.DrawRay(cameraArm.position, cameraArm.forward, Color.red);

        // 이동 방향 구하기 2
        //Debug.DrawRay(cameraArm.position, new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized, Color.red);

        // 이동 방향키 입력 값 가져오기
        Vector2 moveInput = inputDirection;
        // 이동 방향키 입력 판정 : 이동 방향 벡터가 0보다 크면 입력이 발생하고 있는 중
        bool isMove = moveInput.magnitude != 0;
        // 입력이 발생하는 중이라면 이동 애니메이션 재생
        animator.SetBool("isMove", isMove);
        if (isMove)
        {
            // 카메라가 바라보는 방향
            Vector3 lookForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
            // 카메라의 오른쪽 방향
            Vector3 lookRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
            // 이동 방향
            Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;

            // 이동할 때 카메라가 보는 방향 바라보기
            //characterBody.forward = lookForward;
            // 이동할 때 이동 방향 바라보기
            characterBody.forward = moveDir;
            // 이동
            if (!isBorder)
                transform.position += moveDir * Time.deltaTime * 5f;
        }
    }


    void StopToWall()
    {
        Debug.DrawRay(player_body.transform.position, player_body.transform.forward * 1, Color.green);
        isBorder = Physics.Raycast(player_body.transform.position, player_body.transform.forward, 1, LayerMask.GetMask("Wall"));
    }

    //void JumpInSpace()
    //{
    //    if (jump)
    //    {
    //        rdby.AddForce(Vector3.up * jumpower, ForceMode.Impulse);
    //    }
    //}

    public void Die()
    {
        this.gameObject.SetActive(false);

        GameManager gameManager = FindObjectOfType<GameManager>();
        gameManager.EndGame();
    }

   
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpBar : MonoBehaviour
{
    public Slider P_hpSlider;
    public Slider P_BackHpSlider;
    public bool P_backHpHit = false;

    public Image bloodScreen;

    public float P_maxHP = 50f;
    public float P_currentHp = 50f;

    static public bool Pdie = false;



    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
        P_hpSlider.value = Mathf.Lerp(P_hpSlider.value, P_currentHp / P_maxHP, Time.deltaTime * 5f);

        if (P_backHpHit)
        {
            P_BackHpSlider.value = Mathf.Lerp(P_BackHpSlider.value, P_hpSlider.value, Time.deltaTime * 10f);
        }
    }    

    public void Dmg()
    {
        StartCoroutine(ShowBloodScreen());

        P_currentHp -= 10f;
        Invoke("BackHpFun", 0.5f);

        if (P_currentHp <= 0){
            Pdie = true;
        }
    }

    void BackHpFun()
    {
        P_backHpHit = true;
    }

    IEnumerator ShowBloodScreen() {
        bloodScreen.color = new Color(1, 0, 0, Random.Range(0.2f, 0.3f));
        yield return new WaitForSeconds(0.2f);
        bloodScreen.color = Color.clear;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pshoot : MonoBehaviour
{
    public GameObject waterPrefab;
    private Transform target;
    
    private Vector3 firstPos;
    private Vector3 nowPos;
    private float PosGap;

    static public int shootNum = 0;

    [SerializeField]
    public GameObject PS;


    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Aim").transform;
    }

    private void Update()
    {
        nowPos = waterfly.waterflyPos; //���� ��ġ�� waterfly���� ��� �� update ���� �ҷ��ͼ� nowPos�� �Ҵ���.

        if (waterfly.amIDestroyed == true) //�߻��� ���� �μ�������: ó�� ��ġ�� ���� ��ġ�� 0���� �ʱ�ȭ (�⺻���� true)
        {
            firstPos = Vector3.zero;
            nowPos = Vector3.zero;
        }
        PosGap = Vector3.Distance(firstPos, nowPos); //��ġ ���̸� float ���·� ���ϴ� �Լ�

        //if (Time.timeScale < 0.6f)
        //    Time.timeScale += 0.02f;
        //if ((Time.timeScale < 1f) && (waterfly.isFrozen == true))
        //    Time.timeScale += 0.1f;

        if (VirtualJoystick.isShoot == true) //���̽�ƽ�� ������:
        {
            shootNum++;
            
            GameObject water = Instantiate(waterPrefab, transform.position, transform.rotation); //�� ����

            water.transform.LookAt(target); //target ��ġ(�ü� ����)�� �߻�

            waterfly.amIDestroyed = false; //��� �߻������Ƿ� ���� �μ����� ����
            waterfly.isFrozen = false; //��� �߻������Ƿ� ���� ���� ����

            var babyWater = water.GetComponent<waterfly>(); //��� �߻��� �� �Ʊ� ��ü�� �޾ƿͼ�

            if (firstPos == Vector3.zero)
            {
                firstPos = babyWater.transform.position; //�Ʊ� ���� ��ġ�� ì���
            }
            VirtualJoystick.isShoot = false; //���� �ߺ� �߻縦 ���� ���� �ٷ� false �Ҵ�
        }

        if ((firstPos != Vector3.zero) && (nowPos != Vector3.zero) && (PosGap > 25f) && (waterfly.isFrozen == false)) //���� ��ġ ���̰� 20 ���� ũ��:
        {
            //if (Time.timeScale > 0.1f)
                //Time.timeScale -= 0.02f; //�ð� �������� ȿ���� ����
            //else if (Time.timeScale < 0.1f)
            //{
            waterfly.isFrozen = true; //���� ���
            //}
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField]
    private RectTransform lever;
    private RectTransform rectTransform;

    [SerializeField]
    private float leverRange;

    [SerializeField]
    private RectTransform imageBackground;

    [SerializeField]
    private Player controller;

    private float speedP = 2.0f;

    private Vector2 inputDirection;

    static public bool isShoot = false;

    private bool isInput;

    Vector2 adjustedPos;





    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ControlJoystickLever(eventData);

        isInput = true;
    }


    public void OnDrag(PointerEventData eventData)
    {
        ControlJoystickLever(eventData);
    } 

    public void OnPointerUp(PointerEventData eventData)
    {
        lever.anchoredPosition = Vector2.zero;

        isInput = false;

        controller.Move(Vector2.zero);

        if (waterfly.amIDestroyed == true)
            isShoot = true;
    }

    private void ControlJoystickLever(PointerEventData eventData)
    {
        adjustedPos = rectTransform.position;
        //var inputPos = eventData.position - rectTransform.anchoredPosition;
        var inputPos = eventData.position - adjustedPos;

        var inputVector = inputPos.magnitude < leverRange ? inputPos : inputPos.normalized * leverRange;

        lever.anchoredPosition = inputVector * 2.0f;

        inputDirection = (inputVector / leverRange) * speedP;
    }

    private void InputControlVector()
    {
        controller.Move(inputDirection);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isInput)
        {
            InputControlVector();
        }
    }

    
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waterfly : MonoBehaviour
{
    // Start is called before the first frame update

    public int speedW = 70;
    private Rigidbody bulletRigidbody;
    
    static public bool amIDestroyed = true;
    static public bool isFrozen = false;

    [SerializeField]
    public GameObject effect;

    [SerializeField]
    public GameObject Hiteffect;

    public GameObject[] dmddoWater;
    static public Vector3 waterflyPos;

    public MeshRenderer iceRenderer;
    public Texture iceTex;
    public Texture waterTex;

    static public int hitNum = 0;


    void Start()
    {

        bulletRigidbody = GetComponent<Rigidbody>();
        bulletRigidbody.velocity = transform.forward * speedW;
        iceRenderer.material.SetTexture("_MainTex", waterTex);
        effect.SetActive(false);
        Hiteffect.SetActive(false);


    }

    // Update is called once per frame
    void Update()
    {
        
        dmddoWater = GameObject.FindGameObjectsWithTag("Water");

        if ((dmddoWater.Length == 2) && (amIDestroyed == false))
        {
            effect.transform.position = dmddoWater[1].transform.position;
            waterflyPos = dmddoWater[1].transform.position;
        }
        if (isFrozen == true)
        {
            iceRenderer.material.SetTexture("_MainTex", iceTex);
            effect.SetActive(true);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            hitNum++;
            Hiteffect.SetActive(true);
            Enemy agent = other.GetComponent<Enemy>();
            if (agent != null)
            {
                agent.Dead();
            }
        }

        else if (other.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
            amIDestroyed = true;
            isFrozen = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            Destroy(gameObject);
            amIDestroyed = true;
            isFrozen = false;
        }
    }

}


















