using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    Rigidbody rigid;
    Animator anim;
    [SerializeField] private GameObject player;
    [SerializeField] private Transform jumpTransform;
    public GameObject shield;
    float hAxis;
    float vAxis;
    public Transform model;

    public float movePower = 1f;
    public float jumpPower = 1f;
    bool runDown; //r키를 눌렀는지 확인하는 변수
    bool isRotate = false;

    Vector3 moveVector;
    bool jumpDown;
    public bool isGround;
    bool isJump;
    public float groundCheckDistance;
    public LayerMask groundLayer;

    public bool inputLeft = false;
    public bool inputRight = false;
    public bool inputJump = false;
    bool gunDown = false;
    bool swingDown = false;
    float rotateSpeed = 1080f;

    [SerializeField] Text coinText;

    public GameObject collectionEffect; //파티클 시스템 자체는 게임 오브젝트에 저장
    public GameObject collectionLargeEffect;
    public GameObject collectionStarEffect;

    public ParticleSystem collectionSystem; //필요할 때 한번만 추출하면 되도록 하고자 함
    public ParticleSystem collectionLargeSystem;
    public ParticleSystem collectionStarSystem;

    AudioSource audioSource;
    public AudioClip itemCollect;
    public AudioClip coinCollect;

    public GameObject playerBullet;
    public Transform bulletPosition;

    public float dashSpeed; //대시 속도
    public float dashDuration; //대시 지속시간
    public float dashCooldown; //대시 쿨다운 시간
    bool isDash; //대시 중인지 확인하는 변수
    bool dashDown; //left shift키를 눌렀는지 확인하는 변수
    bool canDash;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        collectionSystem = collectionEffect.GetComponent<ParticleSystem>();
        collectionLargeSystem = collectionLargeEffect.GetComponent<ParticleSystem>();
        collectionStarSystem = collectionStarEffect.GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.coin = 0;
        canDash = true;
        coinText.text = string.Format("{0:#,0}", GameManager.instance.coin);
    }

    // Update is called once per frame
    void Update()
    {
        if(isDash)
        {
            return;
        }
        GetInput();

        moveVector = new Vector3(hAxis, 0, vAxis).normalized;

        isGround = Physics.Raycast(jumpTransform.position, Vector3.down, groundCheckDistance, groundLayer);
        anim.SetBool("isJump", !isGround);

        Turn();
        Jump();
        Attack();
        Swing();
        PDash();
    }

    void FixedUpdate()
    {
        if (isDash)
        {
            return;
        }
        Move();
        if (rigid.linearVelocity.y < 0)
        {
            rigid.AddForce(Vector3.down * 200f, ForceMode.Acceleration);
        }
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        runDown = Input.GetButton("Run");
        jumpDown = Input.GetButtonDown("Jump");
        gunDown = Input.GetMouseButtonDown(0);
        swingDown = Input.GetKeyDown(KeyCode.V);
        dashDown = Input.GetKeyDown(KeyCode.LeftShift);
    }

    void Move()
    {
        if(isDash)
        {
            return;
        }
        Vector3 nextPos = rigid.position + moveVector * (runDown ? movePower + 3 : movePower) * Time.fixedDeltaTime;

        rigid.MovePosition(nextPos);

        anim.SetBool("isWalk", moveVector != Vector3.zero);
        anim.SetBool("isRun", runDown || isDash);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVector); //플레이어가 나아가는 방향을 향해 바라봄.
    }

    void Jump()
    {
        Debug.DrawRay(jumpTransform.position, Vector3.down * groundCheckDistance, Color.red);
        if (jumpDown && isGround)
        {
            Vector3 v = rigid.linearVelocity; //속도를 0으로 만든 후, impulse를 주어 같은 높이에서 점프하도록 구현
            v.y = 0;
            rigid.linearVelocity = v;
            
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
        }
    }

    void Attack()
    {
        if (gunDown)
        {
            GameObject bullet = Instantiate(playerBullet, bulletPosition.position, bulletPosition.rotation);
            Rigidbody bulletRigid = bullet.GetComponent<Rigidbody>();
            bulletRigid.linearVelocity = bulletPosition.forward * 100;
        }
    }

    void Swing()
    {
        if (!isRotate && swingDown)
        {
            StartCoroutine(Rotate());
        }
    }

    void PDash()
    {
        if(dashDown && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Rotate()
    {
        Debug.Log("Rotate");
        isRotate = true;
        float rotate = 0f;

        while (rotate < 360f)
        {
            float angle = rotateSpeed * Time.deltaTime;

            if (rotate + angle > 360f) //마지막 프레임에서 360도를 넘지 않도록 보정
            {
                angle = 360f - rotate;
            }
            model.Rotate(0, angle, 0);
            rotate += angle;
            yield return null;
        }
        isRotate = false;
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDash = true;
        rigid.linearVelocity = new Vector3(moveVector.x * dashSpeed, 0, moveVector.z * dashSpeed);
        yield return new WaitForSeconds(dashDuration);
        rigid.linearVelocity = Vector3.zero;
        isDash = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item") || other.CompareTag("Coin"))
        {
            collectionLargeSystem.Emit(1); //딜레이 시간을 없애기 위하여 emission 값을 0으로 설정 후, 코드로 호출
            collectionLargeSystem.Play(); //emit한 후, play로 호출하면 방출이 시작되고 지연이 발생하지 않음.
            collectionStarSystem.Play();
            Destroy(other.gameObject);


            if (other.CompareTag("Coin"))
            {
                GameManager.instance.coin += 1000;
                coinText.text = string.Format("{0:#,0}", GameManager.instance.coin);
                audioSource.clip = coinCollect;
                audioSource.Play();
            }
            else
            {
                shield.SetActive(true);
                audioSource.clip = itemCollect;
                audioSource.Stop();
                audioSource.Play();
            }
        }
    }
}

