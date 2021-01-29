using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform[] walkPath;
    public float speed = 1.0f;
    private float dist;
    int i = 0;
    
    public GameObject player;
    public GameObject enemy;
    public bool isDetected = false;
    // true if the player is shooting enemy
    public bool isShooted = false;
    public float health = 100;
    public bool isDead = false;

    public GameObject end, start; // The gun start and end point
    public GameObject bulletHole;
    public GameObject muzzleFlash;
    public GameObject shotSound;
    public GameObject gun;

    public float Timer = 0f;
    public float maxShootRate = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        // set the initial position to target1
        transform.position = walkPath[0].transform.position;
        isDetected = false;
        GetComponent<Animator>().SetBool("dead",false);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPosition = player.transform.position;
        Vector3 enemyPosition = transform.position;
        
        // the distance between enemy and player
        float distance = Vector3.Distance(playerPosition, enemyPosition);
        if(isDetected == false && isDead == false)
        {
            Move();
        }
        // enemy will detect the player in the field of view or when player shoot enemy
        // set the min detected distance to 20 meters
        
        if(isDetected == false && distance < 20f && isDead == false && isPlayer() == true){
            isDetected = true;
        }
        if(isDetected == false && isDead == false && isShooted == true){
            isDetected = true;
        }
        
        // detected the player, player is alive
        if(isDetected == true && isDead == false)
        {
            transform.LookAt(player.transform);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerPosition - enemyPosition), Time.deltaTime);
            //print(distance);
            //is the distance == 10 meters, stop and shoot the player
            if(distance <= 10f)
            {
                GetComponent<Animator>().SetBool("fire",true);
                GetComponent<Animator>().SetBool("run",false);
                // shoot at most 5 bullet per second
                if(maxShootRate > Timer)
                    {
                        shootPlayer();
                        addEffects();
                        //reset the timer
                        Timer = 0f;
                    }
                    // record
                    Timer = Timer + Time.deltaTime;
            }
            else
                {
                    // run to player
                    GetComponent<Animator>().SetBool("run",true);
                    GetComponent<Animator>().SetBool("fire",false);
                }
        }
    }

    
    // Enemy: enemy character walks in a path
    void Move(){
        float step = speed * Time.deltaTime;
        this.transform.LookAt(walkPath[i].transform);

        dist = Vector3.Distance(this.transform.position, walkPath[i].transform.position);
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerPosition - enemyPosition), Time.deltaTime);

        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(walkPath[i].transform.position - transform.position), step);
        if(dist < 0.1f && i < walkPath.Length)
        {
            i++;
            //print(i);
        }
        else if(i == 4)
        {
            i = 0;
            //print(i);
        }
    }

    // determine whether player enter the room
    bool isPlayer(){
        bool inrange = false;
        RaycastHit rayHit;
        // idk why need to add (0,1,0), but it works...
        Ray ray = new Ray(transform.position + new Vector3(0,1,0), player.transform.position-transform.position);
        if(Physics.Raycast(ray, out rayHit, 100f)){
            if(rayHit.transform.name == "player"){
                return true;
            }
        }
        
        return inrange;
    }

    void shootPlayer()
    {
        RaycastHit rayHit;
        // randomized the shooting vector, change up & right
        Vector3 randomized_end = end.transform.position + (end.transform.right * Random.Range(-0.2f, 0.2f)) + (end.transform.up * Random.Range(-0.2f, 0.2f));
        if(Physics.Raycast(randomized_end, (randomized_end - start.transform.position).normalized, out rayHit, 100.0f))
        {
            if(rayHit.transform.tag == "Player")
            {
                //call whatever
                rayHit.transform.GetComponent<Gun>().Being_shot(20);

            }else{
                //bullet hole
                //collider: give the object that we hit
                GameObject BulletHoleObject = Instantiate(bulletHole, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
            }
            
        }
    }

    void addEffects() // Adding muzzle flash, shoot sound and bullet hole on the wall
    {
        // muzzle flash
        GameObject flash = Instantiate(muzzleFlash, end.transform.position, end.transform.rotation);
        flash.GetComponent<ParticleSystem>().Play();
        Destroy(flash, 2.0f);

        // shot sound
        Destroy((GameObject) Instantiate(shotSound, transform.position, transform.rotation), 2.0f);
    
    }
    //void reloading()
   //{
      //  animator.SetTrigger("reloading");
    //}

    public void Being_shot(float damage) // getting hit from player, similar with player.Being_shot()
    {
        health = health - damage;
        print(health);
        
        if(health > 0)
        {
            isShooted = true;
        }
        // if health <= 0, run the death animation, gun drop on the floor
        else
        {
            isDead = true;
            GetComponent<Animator>().SetBool("fire", false);
            GetComponent<Animator>().SetBool("dead", true);
            GetComponent<CharacterController>().enabled = false;
             // make gun as an independent object from the enemy
            gun.transform.parent = null;
            //gun.GetComponent<CharacterController>().enabled = false;
            // add a rigidbody and collider to the gun
            gun.AddComponent<BoxCollider>();
            // set the box size to 0.1, 0.4, 1
            gun.GetComponent<BoxCollider>().size = new Vector3(0.1f, 0.4f, 1f);
            gun.AddComponent<Rigidbody>();
            gun.GetComponent<Rigidbody>().useGravity = true;
        }
    }
}
