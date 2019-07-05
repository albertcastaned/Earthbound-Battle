using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Movement : MonoBehaviour
{
    private Vector3 moveDir;
    private Vector3 move;
    private Vector3 lastDir;
    private float moveSpeed;
    public Camera cam;
    public Animator animator;
    private SpriteRenderer spriteRender;
    private RaycastHit raycast;
    private Vector3 raycastAngle;
    private Rigidbody rb;
    private bool running;
    public bool canMove;
    public TextBoxManager txtMnger;
    
    private string currentAnim;
    public List<Vector3> wayPoints;

    public SimpleBlit transitioner;

    private bool halt;
    [SerializeField] private int MaximumWaypoints;

    public float enemySpawnRange;

    public GameObject enemySpawn;
    // Start is called before the first frame update
    void Start()
    {
        CreateEnemies();
        AudioManager.instance.StopPlaying("YouWin");
        AudioManager.instance.Play("Onett");

        wayPoints = new List<Vector3>();
        if (GameController.GetNessPosition != Vector3.zero)
        {
            transform.position = GameController.GetNessPosition;
        }
        txtMnger = FindObjectOfType<TextBoxManager>();
        moveSpeed = 0.05f;
        animator = GetComponent<Animator>();
        spriteRender = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();
        running = false;


    }

    void CreateEnemies()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 position = new Vector3(transform.position.x + Random.Range( -2f + -enemySpawnRange, 2f + enemySpawnRange),
                0.2f,
                transform.position.z + Random.Range(-2f + -enemySpawnRange, 2f + enemySpawnRange));

            Instantiate(enemySpawn, position, Quaternion.identity);
        }
    }
    public Vector3 GetWayPoint(int index)
    {
        return wayPoints[index];
    }

    public int GetWaypointSize()
    {
        return wayPoints.Count;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateCam();
        
        if (halt || !canMove)
        {
            animator.enabled = false;
            return;
        }

        //Controles
        moveDir = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        move = new Vector3(0.0f, 0.0f, 0.0f);


        animator.enabled = true;
        if (running)
            moveSpeed = 0.15f;
        else
            moveSpeed = 0.05f;
 
        if (Input.GetKey("up"))
        {
            
            if (Input.GetKey("right"))
            {
                move = new Vector3(transform.position.x + moveSpeed, transform.position.y, transform.position.z + moveSpeed);
                animator.Play(Animator.StringToHash("diagUW"));
                currentAnim = "diagUW";
                raycastAngle = new Vector3(1f, 0.0f, 1f);
            }
            else if (Input.GetKey("left"))
            {
                move = new Vector3(transform.position.x - moveSpeed, transform.position.y, transform.position.z + moveSpeed);
                animator.Play(Animator.StringToHash("diagUL"));
                currentAnim = "diagUL";

                raycastAngle = new Vector3(-0.5f, 0.0f, 0.5f);

            }
            else
            {
                move = new Vector3(transform.position.x, transform.position.y, transform.position.z + moveSpeed);
                animator.Play(Animator.StringToHash("upW"));
                currentAnim = "upW";

                raycastAngle = new Vector3(0.0f, 0.0f, 1.0f);
            }


        }
        else if (Input.GetKey("down"))
        {

            if (Input.GetKey("right"))
            {
                move = new Vector3(transform.position.x + moveSpeed, transform.position.y, transform.position.z - moveSpeed);
                animator.Play(Animator.StringToHash("diagDW"));
                currentAnim = "diagDW";

                raycastAngle = new Vector3(0.5f, 0.0f, -0.5f);
            }
            else if (Input.GetKey("left"))
            {
                move = new Vector3(transform.position.x - moveSpeed, transform.position.y, transform.position.z - moveSpeed);
                animator.Play(Animator.StringToHash("diagDL"));
                currentAnim = "diagDL";

                raycastAngle = new Vector3(-0.5f, 0.0f, -0.5f);

            }
            else
            {
                move = new Vector3(transform.position.x, transform.position.y, transform.position.z - moveSpeed);
                animator.Play(Animator.StringToHash("downW"));
                currentAnim = "downW";


                raycastAngle = new Vector3(0.0f, 0.0f, -1.0f);
            }

        }
        else if (Input.GetKey("right") && (!Input.GetKey("left")))
        {
            move = new Vector3(transform.position.x + moveSpeed, transform.position.y, transform.position.z);
            animator.Play(Animator.StringToHash("rightW"));
            currentAnim = "rightW";

            raycastAngle = new Vector3(1.0f, 0.0f, 0.0f);
        }
        else if (Input.GetKey("left") && (!Input.GetKey("right")))
        {
            move = new Vector3(transform.position.x - moveSpeed, transform.position.y, transform.position.z);
            animator.Play(Animator.StringToHash("leftW"));
            currentAnim = "leftW";

            raycastAngle = new Vector3(-1.0f, 0.0f, 0.0f);

        }

        

        if (Math.Abs(move.x) > 0.01f)
            lastDir.x = moveDir.x;

        if (Math.Abs(move.x) < 0.05f && Math.Abs(move.z) < 0.05f)
        {
            animator.Play(Animator.StringToHash(currentAnim));
            animator.enabled = false;
            

        }



        if (move != Vector3.zero)
        {
            rb.position = move;
        }

        if (Physics.Raycast(transform.position, raycastAngle, out raycast, 0.2f))
        {
            if (raycast.collider.gameObject.name != "Ness")
                rb.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

            if (raycast.collider.gameObject.CompareTag("NPC") && Input.GetKeyDown(KeyCode.Space) && canMove)
            {
                NPCBehaviour aux = raycast.collider.gameObject.GetComponent<NPCBehaviour>();
                txtMnger.ReloadScript(aux.getDialogue());
                txtMnger.EnableTextBox();

            }
            if (raycast.collider.gameObject.CompareTag("Enemy"))
            {
                halt = true;
                AudioManager.instance.StopPlaying("Onett");
                AudioManager.instance.Play("EnterBattle");

                transitioner.StartCoroutine(nameof(SimpleBlit.StartTransition));
            

            }

        }
        Debug.DrawRay(transform.position, raycastAngle * 0.2f, Color.green);
        AddWayPoint();


    }

    void UpdateCam()
    {
        cam.transform.position = new Vector3(transform.position.x, transform.position.y + 1.85f, transform.position.z - 3f);
    }

    private void AddWayPoint()
    {


        if (Math.Abs(move.x) > 0.1f || Math.Abs(move.z) > 0.1f)
        {
            if (wayPoints.Count > MaximumWaypoints)
            {
                wayPoints.RemoveAt(0);
            }
            wayPoints.Add(transform.position);


        }
    }



}