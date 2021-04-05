using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour {
    Rigidbody2D rb;
    public float jumpForce;
    public float floorDrag;
    public float airDrag;
    public float floorForce;
    public float airForce;
    bool jumpFlag;
    bool onFloor;
    int floorObjs;
    public KeyCode jumpButton;
    public KeyCode left;
    public KeyCode right;
    public KeyCode up;
    public KeyCode down;
    public KeyCode grabButton;
    public KeyCode throwButoon;
    public float jumpAggro;
    public float moveDeadzone;
    public float stopForce;
    public Rigidbody2D ball;
    public float hitDist;
    public Vector2 strikeForce;
    public AudioClip[] hitSnds;
    public AudioSource audioSource;
    public float hitBuffer;
    public float timeSinceLastStrikePress;
    public float strikeCoolDown;
    public float timeSinceLastStrike;
    public GameObject arrow;
    float height;
    float baseHeight;
    float baseWidth;
    float wiggleSpd;
    int facing;
    float gameOverInputTimer;
    FallingBlock grabbedBlock;
    public GameObject spr;
    public GameObject grabDot;
    public BoxCollider2D floorTrig;
    public GameObject arm;
    Vector2 prevVel;
    public ParticleSystem deadPart;
    bool blockJumped;
    public ParticleSystem jumpDust;
    public ParticleSystem[] landDust;
    Collider2D selectedBlock;
    float justPushedTimer;
    int pushing;
    int pushTimer;
    float selectedBufferTime;
    float justPressedGrabTimer;

    bool playedGameOverSound = false;

    void Start () {
        baseHeight = spr.transform.localScale.y;
        baseWidth = spr.transform.localScale.x;
        //ball = Global.me.ball;
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        playedGameOverSound = false;
	}
	
	void Update () {
        //Time.timeScale = ((rb.velocity.magnitude / 10f) * .8f) + .2f;
        //Time.fixedDeltaTime = (1f / 60f) * Time.timeScale;
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
        if (Grid.me.gameOver) {

            //NewSound
            if (!playedGameOverSound)
            {
                AudioDirector.Instance.FadeOutAudio(AudioDirector.Instance.dangerSource, 0.15f);
                playedGameOverSound = true;
            }

            gameOverInputTimer += Time.deltaTime;
            if (gameOverInputTimer > 1f) {
                if (Input.GetKeyDown(jumpButton)) {

                    //NewSound
                    AudioDirector.Instance.gameplaySnapshot.TransitionTo(1.0f);

                    SceneManager.LoadScene(0);
                }
            }
            return;
        }
        /*if (Input.GetMouseButtonDown(0))
        {
            Vector3 centerPt = transform.TransformPoint(new Vector3(0, .5f, 0));
            Vector2 vct = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - centerPt);
            Instantiate(arrow,
                    (Vector2)centerPt + vct.normalized * .25f,
                    Quaternion.Euler(0, 0, Geo.ToAng(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)));
        }*/
        wiggleSpd += (1f - height) * 2.8f * Time.deltaTime;
        wiggleSpd -= wiggleSpd * 5.8f * Time.deltaTime;
        height += wiggleSpd;
        spr.transform.localScale = new Vector3(baseWidth * (2f - height), baseHeight * height, 1);

		if (Input.GetKeyDown(jumpButton) && onFloor) {
            jumpFlag = true;
        }
        PlaceDot();
        if (grabbedBlock == null) {
            SelectDeal();
        }
        GrabDeal();

        arm.transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(arm.transform.eulerAngles.z-.1f, Geo.ToAng((grabDot.transform.position - arm.transform.position)) - 90f, .4f));
        justPushedTimer -= Time.deltaTime;
        selectedBufferTime -= Time.deltaTime;
        justPressedGrabTimer -= Time.deltaTime;
    }




    void PlaceDot() {
        if (Input.GetKey(down)) {
            grabDot.transform.position = transform.TransformPoint(new Vector2(0, -.75f));

        } else if (Input.GetKey(up)) {
            grabDot.transform.position = transform.TransformPoint(new Vector2(0, 1.5f));

        } else {
            grabDot.transform.position = transform.TransformPoint(new Vector2(.5f * facing, .5f));
        }
    }
    void SelectDeal() {
        Collider2D bloop = Physics2D.OverlapPoint(grabDot.transform.position, LayerMask.GetMask("Block"));
        if (bloop == null) {
            bloop = Physics2D.OverlapArea((Vector2)grabDot.transform.position + new Vector2(.1f, .1f),
                                           (Vector2)grabDot.transform.position - new Vector2(.1f, .1f),
                                          LayerMask.GetMask("Block"));
        }
        if (bloop != null) {
            if (bloop != selectedBlock) {
                bloop.SendMessageUpwards("Selected", null, SendMessageOptions.DontRequireReceiver);
                if (selectedBlock != null) {
                    selectedBlock.SendMessageUpwards("UnSelected", null, SendMessageOptions.DontRequireReceiver);
                }
                selectedBlock = bloop;
            }
            selectedBufferTime = .0833334f;

        } else if (selectedBlock != null) {
            if (selectedBufferTime <= 0f) {
                selectedBlock.SendMessageUpwards("UnSelected", null, SendMessageOptions.DontRequireReceiver);
                selectedBlock = null;
            }
        }


    }
    void GrabDeal() {
        if (Input.GetKeyDown(grabButton) && grabbedBlock == null && selectedBlock == null) {
            justPressedGrabTimer = .0833334f;
        }
        if (Input.GetKeyDown(grabButton) || justPressedGrabTimer > 0) {
            if (grabbedBlock == null) {
                if (selectedBlock != null) {
                    justPressedGrabTimer = 0;
                    PickUp(selectedBlock.transform.parent.GetComponent<FallingBlock>());
                }
            } else {
                //throw block
                //NewSound
                AudioDirector.Instance.PlaySound(AudioDirector.Instance.throwSound, true, transform.position.x, AudioDirector.Instance.throwVolume, 0.2f);

                grabbedBlock.pickedUp = false;
                grabbedBlock.rb.isKinematic = false;
                
                grabbedBlock.rb.velocity = new Vector2(rb.velocity.x * .5f, rb.velocity.y * .2f);
                if (!Input.GetKey(down)) {
                    grabbedBlock.justThrownTimer = .1f;
                    if (Input.GetKey(up)) {
                        if (!(Input.GetKey(right) || Input.GetKey(left))) {
                            grabbedBlock.rb.AddForce(new Vector2(0f, 12f), ForceMode2D.Impulse);
                        } else {
                            grabbedBlock.rb.AddForce(new Vector2(4f * facing, 12f), ForceMode2D.Impulse);
                        }

                    } else {
                        grabbedBlock.rb.AddForce(new Vector2(5f * facing, 10f), ForceMode2D.Impulse);
                    }
                } else {
                    grabbedBlock.transform.position = (Vector2)transform.position + new Vector2(0f, 0.5f);
                    transform.position = ((Vector2)transform.position + new Vector2(0f, 1.5f));
                    rb.velocity = new Vector2(rb.velocity.x, 0);
                    grabbedBlock.rb.velocity = Vector2.zero;
                    
                    if (!onFloor) {
                        grabbedBlock.rb.AddForce(new Vector2(0f, -8f), ForceMode2D.Impulse);
                        blockJumped = true;
                        rb.AddForce(new Vector2(0f, 12f), ForceMode2D.Impulse);
                    }
                }
                grabbedBlock = null;

            }
        }
    }
    void PickUp(FallingBlock block) {
        if (block.inGrid) {
            block.inGrid = false;
            block.myCell.LoseBlock();
            block.myCell = null;
        }
        block.pickedUp = true;
        block.rb.isKinematic = true;
        grabbedBlock = block;
        block.UnSelected();
        block.rb.velocity = Vector2.zero;

        //NewSound
        AudioDirector.Instance.PlaySound(AudioDirector.Instance.pickupSound, true, transform.position.x, AudioDirector.Instance.pickupVolume, 0.2f);
        
    }
    void GetPushed(float x) {
        justPushedTimer = .2f;
        rb.AddForce(Vector2.right * Mathf.Sign(transform.position.x - x) * 13f, ForceMode2D.Impulse);
    }
    //Destroy Self
    void BlowUp() {
        deadPart.transform.parent = null;
        deadPart.Play();

        //NewSound
        AudioDirector.Instance.PlaySound(AudioDirector.Instance.gameOverSound, false, 0f, AudioDirector.Instance.gameOverVolume, 0f, true);
        //SetSnapshot
        AudioDirector.Instance.gameOverSnapshot.TransitionTo(1f);

        Grid.me.gameOver = true;
        transform.position = new Vector3(100f, 100f, 0);
    }

    void FixedUpdate() {
        wiggleSpd += (prevVel.y - rb.velocity.y) * .005f;
        bool prevOnFloor = onFloor;
        onFloor = Physics2D.OverlapArea(floorTrig.bounds.center + floorTrig.bounds.extents,
                                        floorTrig.bounds.center - floorTrig.bounds.extents,
                                        LayerMask.GetMask("Block", "Wall"));
        if (!prevOnFloor && onFloor) {
            //Landing

            //NewSound
            AudioDirector.Instance.PlaySound(AudioDirector.Instance.landSound, true, transform.position.x, AudioDirector.Instance.landVolume, 0.1f);
       
            //wiggleSpd -= .1f;
            blockJumped = false;
            for (int i = 0; i < landDust.Length; i++) {
                landDust[i].Play();
            }
        }
        if (jumpFlag && onFloor) {
            //jumping

            //NewSound
            AudioDirector.Instance.PlaySound(AudioDirector.Instance.jumpSound, true, transform.position.x, AudioDirector.Instance.jumpVolume, 0.1f);

            jumpDust.Play();
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            wiggleSpd += .1f;
        }
        if (grabbedBlock != null) {
            grabbedBlock.transform.position = transform.TransformPoint(new Vector2(0, 1.5f));
        }
        //this is variable jump height
        if (Input.GetKeyUp(jumpButton) && rb.velocity.y >= 0 && !blockJumped) {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y - jumpAggro, 0));
        }

        //turn key input into a direction (either 1, 0 or -1)
        int goDir = 0;
        if (Input.GetKey(left)) { goDir--; }
        if (Input.GetKey(right)) { goDir++; }
        if (goDir != 0) { facing = goDir;}
        //Quadratic drag
        if (justPushedTimer <= 0) {
            if (onFloor) {
                rb.AddForce(Vector2.right * floorForce * goDir);
                rb.AddForce(Vector2.left * floorDrag * Mathf.Sign(rb.velocity.x) * Mathf.Pow(rb.velocity.x, 2));
            } else {
                rb.AddForce(Vector2.right * airForce * goDir);
                rb.AddForce(Vector2.left * airDrag * Mathf.Sign(rb.velocity.x) * Mathf.Pow(rb.velocity.x, 2));
            }
        }

        if (goDir == 0) {
            if (Mathf.Abs(rb.velocity.x) < moveDeadzone)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else
            {
                rb.AddForce(Vector2.left * Mathf.Sign(rb.velocity.x) * stopForce);
            }
        }
        jumpFlag = false;
        prevVel = rb.velocity;
        if (pushing > 0) {
            pushing--;
        } else {
            pushTimer = 0;
        }
    }

    private void OnCollisionStay2D(Collision2D coll) {
        if (coll.gameObject.tag == "Block" &&
            (Input.GetKey(left) || Input.GetKey(right))) {
            pushing = 2;
            pushTimer++;
            if (pushTimer > 5) {
                FallingBlock block = coll.gameObject.GetComponent<FallingBlock>();
                
                if (block.inGrid && 
                    !Grid.me.grid[(int)block.myCell.gridPos.x, (int)block.myCell.gridPos.y + 1].ocupied &&
                    !Grid.me.grid[(int)block.myCell.gridPos.x + facing, (int)block.myCell.gridPos.y].ocupied) {
                    block.inGrid = false;
                    block.myCell.LoseBlock();
                    block.myCell = null;
                    block.rb.isKinematic = false;
                    block.rb.AddForce(Vector2.right * facing * 2f, ForceMode2D.Impulse);
                }
                
            }
        }
    }

    /*
    void OnTriggerEnter2D(Collider2D c) {
        if (!onFloor) {
            wiggleSpd = -.1f;
        }
        onFloor = true;
        floorObjs++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        floorObjs--;
        if (floorObjs <= 0) {
            onFloor = false;
        }
    }*/

    /*
    public void PlayHitSound()
    {
        Sound.me.PlaySound(hitSnds[Random.Range(0, hitSnds.Length)], 1f);
        //audioSource.Play();
    }
    */
}
