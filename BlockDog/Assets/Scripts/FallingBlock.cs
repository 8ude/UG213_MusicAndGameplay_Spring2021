using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingBlock : MonoBehaviour {
    public bool inGrid;
    public bool pickedUp;
    public GridCell myCell;
    public Rigidbody2D rb;
    public SpriteRenderer spr;
    public int colNum;
    public ParticleSystem partSys;
    public BoxCollider2D coll;
    public float justThrownTimer;
    public ParticleSystem[] dustParts;
    public Vector2 prevVel;
    public ColorWiggler wigl;
    Color baseColor;
    public float dontPushAgainTimer;


	void Start () {
        rb = GetComponent<Rigidbody2D>();
        //colNum = 
        spr.color = Global.me.blockColors[colNum];
        rb.useFullKinematicContacts = true;
        baseColor = spr.color;
    }

	
	void Update () {
        if (inGrid) {
            coll.size = new Vector2(1, 1);
        } else {
            coll.size = new Vector2(.85f, .85f);
        }
        if (pickedUp || justThrownTimer > 0) {
            gameObject.layer = LayerMask.NameToLayer("Grabbed");
            coll.gameObject.layer = LayerMask.NameToLayer("Grabbed");
        } else {
            gameObject.layer = LayerMask.NameToLayer("Block");

            coll.gameObject.layer = LayerMask.NameToLayer("Block");
        }

        justThrownTimer -= Time.deltaTime;
        dontPushAgainTimer -= Time.deltaTime;
	}

    private void FixedUpdate() {
        if (inGrid) {
            rb.MovePosition(Vector2.Lerp(transform.position, myCell.worldPos, .25f));
        }
        prevVel = rb.velocity;
    }



    void OnCollisionStay2D(Collision2D coll) {
        if (!inGrid &&
            rb.velocity.magnitude < .01f &&
            !pickedUp &&
            (coll.gameObject.layer == LayerMask.NameToLayer("Block") || coll.gameObject.layer == LayerMask.NameToLayer("Wall"))) {
            Vector2 gridPos = Grid.me.ToGrid(transform.position);
            GridCell cell = Grid.me.GetCell(gridPos);
            if (!cell.ocupied && Grid.me.GetCell(gridPos.x, gridPos.y - 1).ocupied) {
                inGrid = true;
                rb.isKinematic = true;
                cell.GetBlock(this);
            } else if (dontPushAgainTimer <= 0){
                GridCell nextCell = Grid.me.NextNearest((Vector2)transform.position);
                if (!nextCell.ocupied && 
                    Grid.me.GetCell(nextCell.gridPos.x, nextCell.gridPos.y - 1).ocupied &&
                    ((Vector2)transform.position - nextCell.worldPos).magnitude < .7f) {
                    inGrid = true;
                    rb.isKinematic = true;
                    cell.GetBlock(this);
                } else {
                    rb.AddForce((cell.worldPos - (Vector2)transform.position).normalized * 1.75f, ForceMode2D.Impulse);
                    rb.AddForce(Vector2.up * 1.75f, ForceMode2D.Impulse);
                    dontPushAgainTimer = .05f;
                }
            }
        }
    }
    public void BlowUp() {
        partSys.transform.parent = null;
        partSys.Play();
        colNum = -1;
        Destroy(gameObject);
    }
    private void OnCollisionEnter2D(Collision2D coll) {
        if (coll.gameObject.tag != "Player" && prevVel.magnitude > 15f) {
            for (int i = 0; i < dustParts.Length; i++) {
                var mainModule = dustParts[i].main;

                mainModule.startColor = baseColor;//spr.color;
                dustParts[i].Play();
                CameraControl.me.Shake(.1f);
                //CameraControl.me.Flash(.1f);
                StartCoroutine(FlashYourself());

            }

            //NewSound
            AudioDirector.Instance.PlayBlockLandSound(transform.position.y, transform.position.x);

        }
        if (coll.gameObject.tag == "Player" && coll.gameObject.transform.position.y < transform.position.y - .5f && prevVel.y < -14f) {
            if (Mathf.Abs(coll.transform.position.x - transform.position.x) > .5f) {
                coll.gameObject.SendMessageUpwards("GetPushed", transform.position.x);
            } else {
                Global.me.player.SendMessage("BlowUp", null);
                CameraControl.me.Shake(1f);
            }

            //rb.AddForce(new Vector2(transform.position.x > coll.gameObject.transform.position.x ? 4f : -4f, 8f), ForceMode2D.Impulse);
            //CameraControl.me.Shake(.5f);
        }
    }

    public IEnumerator FlashYourself() {
        wigl.baseColor = Color.white;
        spr.color = Color.white;
        yield return new WaitForSeconds(.05f);
        wigl.baseColor = Color.black;
        spr.color = Color.black;
        yield return new WaitForSeconds(.05f);
        spr.color = baseColor;
        wigl.baseColor = baseColor;

    }

    public void Selected() {
        wigl.baseColor = baseColor * 1.8f;
    }

    public void UnSelected() {
        wigl.baseColor = baseColor;
    }
}
