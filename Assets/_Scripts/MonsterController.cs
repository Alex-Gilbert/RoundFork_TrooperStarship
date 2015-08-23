using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour 
{
    public Animator clawAnimator;
    public Animator impalerAnimator;
    AnimatorStateInfo stateinfo;

    public float mouseSensitivityX = 250f;
    public float mouseSensitivityY = 250f;
    public float walkSpeed = 2;
    public float maxJumpForce;    
    public float timeToChargeJump;
    public float leapForce = 3;

    public LayerMask groundMask;

    Transform cameraT;
    float verticalLookRotation;

    Vector3 moveAmount;
    Vector3 smoothMoveVelocity;

    Vector3 jumpDir;
    Vector3 jumpDirVel;

    float jumpforce;
    float jumpVel;

    Rigidbody rb;

    bool grounded;

    bool chargingJump = false;

    bool checkingForLeap = false;

    float _doubleTapTimeD;
    float _doubleTapTimeA;

    public float timeToMakeNoise = 2;
    float noiseMakeTime;
    public float idleNoise = 3;
    public float movingNoise = 5;
    public float jumpingNoise = 10;

    public Transform weaponT;
    public float SphereRadius;

    public float health;

    public float TimeToStayDamaged = 4;
    float damagedTime;

    float healthVel;
    public float healTime = .5f;

    public Animator NorthHit;
    public Animator SouthHit;
    public Animator EastHit;
    public Animator WestHit;

    public BloodOverlay bloodOverlay;

    public bool Dead;
    float rotateVel;
    float curRotation = 0;

    bool launched = false;
    bool launchCheck = false;

    public void OnCollisionEnter(Collision collision)
    {
        if(launched)
        {
            if(collision.gameObject.tag == "Enemy")
            {
                collision.gameObject.SendMessage("Executed");
                impalerAnimator.SetTrigger("HitSoldier");
            }

            //End Launch
        }
    }



	// Use this for initialization
	void Start () 
    {
        noiseMakeTime = Time.time;

        GameBroadcaster.Instance.PlayerSwipeAttacked += HandlePlayerSwipeAttacked;
        GameBroadcaster.Instance.PlayerHit += HandlePlayerHit;

        jumpDir = new Vector3(1, 0, 0);
        cameraT = Camera.main.transform;
        rb = this.GetComponent<Rigidbody>();
	}

    public void FixedUpdate()
    {
        if(!chargingJump && !Dead)
            rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }
	
	// Update is called once per frame
	void Update () 
    {
        stateinfo = impalerAnimator.GetCurrentAnimatorStateInfo(0);

        RecoverDamage();
        bloodOverlay.targetAlpha = health;

        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivityX);
        verticalLookRotation += Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivityY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60, 60);
        cameraT.localEulerAngles = Vector3.left * verticalLookRotation;

        grounded = false;


        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1 + .1f, groundMask))
        {
            grounded = true;
            if(launched)
            {
                if (launchCheck)
                {
                    print("landed");
                    launchCheck = false;
                    launched = false;

                    impalerAnimator.SetTrigger("Landed");
                }
            }
        }
        else
        {
            clawAnimator.SetFloat("Speed", 12.2f);

            if (launched)
                launchCheck = true;
        }

        Debug.DrawLine(transform.position, transform.position - transform.up * 1.1f, chargingJump ? Color.green : Color.red);

        if (!Dead)
        {
            Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
            Vector3 targetMoveAmount = moveDir * walkSpeed;

            bool doubleTapA = DoubleTapA();
            bool doubleTapD = DoubleTapD();

            moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothMoveVelocity, .15f);

            moveAmount *= (Input.GetKey(KeyCode.LeftShift) ? .5f : 1f);

            if (!chargingJump && grounded)
            {
                if (doubleTapA)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.AddForce(transform.rotation * new Vector3(-1, .35f, 0) * 100 * leapForce);
                }
                else if (doubleTapD)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.AddForce(transform.rotation * new Vector3(1, .35f, 0) * 100 * leapForce);
                }
            }

            if (Input.GetButton("Jump") && !chargingJump && stateinfo.IsName("Movement") && grounded)
            {
                StopCoroutine(ChargeJump());
                StartCoroutine(ChargeJump());
            }

            if (Input.GetMouseButtonDown(0))
                clawAnimator.SetTrigger("Attack");
        }

        float moveAmountSqrMag = moveAmount.sqrMagnitude;

        if(grounded)
            clawAnimator.SetFloat("Speed", moveAmountSqrMag);

        impalerAnimator.SetFloat("Speed", moveAmountSqrMag);
        impalerAnimator.SetBool("ChargingJump", chargingJump);

        if (!chargingJump)
            idleNoiseCheck(moveAmountSqrMag);

        Die();
	}

    void RecoverDamage()
    {
        if(Time.time - damagedTime >= TimeToStayDamaged)
        {
            health = 0;
        }
    }

    void idleNoiseCheck(float MoveAmount)
    {
        if(Time.time - noiseMakeTime >= timeToMakeNoise)
        {
            if (MoveAmount >= 8f)
                MakeNoise(movingNoise);
            else
                MakeNoise(idleNoise);
        }
    }

    void MakeNoise(float NoiseRange)
    {
        noiseMakeTime = Time.time;

        GameBroadcaster.Instance.OnPlayerMadeNoise(this.gameObject, NoiseRange);
    }

    bool DoubleTapD()
    {
        bool doubleTapD = false;

        #region doubleTapD

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time < _doubleTapTimeD + .25f)
            {
                doubleTapD = true;
            }
            _doubleTapTimeD = Time.time;
        }

        #endregion

        if (doubleTapD)
        {
            Debug.Log("DoubleTapD");
            return true;
        }

        return false;
    }

    bool DoubleTapA()
    {
        bool doubleTapA = false;

        #region doubleTapA

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (Time.time < _doubleTapTimeA + .25f)
            {
                doubleTapA = true;
            }
            _doubleTapTimeA = Time.time;
        }

        #endregion

        if (doubleTapA)
        {
            Debug.Log("DoubleTapA");
            return true;
        }

        return false;
    }

    void HandlePlayerSwipeAttacked()
    {
        Collider[] hitColliders = Physics.OverlapSphere(weaponT.position, SphereRadius);

        Debug.DrawLine(weaponT.position, weaponT.position + Vector3.up * SphereRadius);

        for (int i = 0; i < hitColliders.Length; ++i)
        {
            if (hitColliders[i].tag == "Enemy")
            {
                hitColliders[i].SendMessage("OnHit");
            }
            else if (hitColliders[i].tag == "Interactable")
            {
                hitColliders[i].SendMessage("OnInteract");
            }
        }
    }

    void Die()
    {
        if (Dead)
        {
            var rotationVector = transform.rotation.eulerAngles;

            rotationVector.z = Mathf.SmoothDamp(rotationVector.z, 90, ref rotateVel, .5f);

            transform.rotation = Quaternion.Euler(rotationVector);

            
        }
    }

    IEnumerator ChargeJump()
    {
        GetComponent<JumpPreview>().SetDrawPath(true);
        chargingJump = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        jumpforce = 0;
        jumpDir = new Vector3(0, 0, 1);
        while(Input.GetButton("Jump") || jumpforce < maxJumpForce * .75f)
        {
            jumpforce = Mathf.SmoothDamp(jumpforce, maxJumpForce, ref jumpVel, timeToChargeJump);
            jumpDir = Vector3.SmoothDamp(jumpDir, new Vector3(0, .35f, 1), ref jumpDirVel, timeToChargeJump);

            GetComponent<JumpPreview>().force = transform.rotation * jumpDir * jumpforce * 100;

            Debug.DrawLine(transform.position, transform.position + (transform.rotation * jumpDir * jumpforce));
            //print(string.Format("JumpForce: {0}", jumpforce));
            yield return null;
        }

        rb.AddForce((transform.rotation * jumpDir * jumpforce * 100));
        MakeNoise(jumpingNoise);
        chargingJump = false;
        launched = true;
        GetComponent<JumpPreview>().SetDrawPath(false);
        yield return null;
    }

    public

    void HandlePlayerHit(GameObject enemy, float damage)
    {
        damagedTime = Time.time;

        health += damage;

        if (health >= 1)
            Dead = true;

        Vector3 toEnemy = enemy.transform.position - transform.position;
        toEnemy.y = transform.position.y;

        Quaternion refertialShift = Quaternion.FromToRotation(transform.forward, toEnemy);

        float rotationAmount = refertialShift.eulerAngles.y;

        if(rotationAmount <= 270 && rotationAmount >= 90)
        {
            SouthHit.SetTrigger("Ping");
        }
        else
        {
            NorthHit.SetTrigger("Ping");
        }

        if(rotationAmount >= 45 && rotationAmount <=135)
        {
            EastHit.SetTrigger("Ping");
        }
        else if(rotationAmount <= 315 && rotationAmount >= 225)
        {
            WestHit.SetTrigger("Ping");
        }
    }
}
