using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Gun : MonoBehaviour {

    public GameObject end, start; // The gun start and end point
    public GameObject gun;
    public Animator animator;
    
    public GameObject spine;
    public GameObject handMag;
    public GameObject gunMag;

    float gunShotTime = 0.1f;
    float gunReloadTime = 1.0f;
    Quaternion previousRotation;
    public float health = 100;
    public bool isDead;
 

    public Text magBullets;
    public Text remainingBullets;
    public Text playerHealth;

    int magBulletsVal = 30;
    int remainingBulletsVal = 90;
    int magSize = 30;
    public GameObject headMesh;
    public static bool leftHanded { get; private set; }


    public GameObject bulletHole;
    public GameObject muzzleFlash;
    public GameObject shotSound;
    public GameObject player;
    
    // Use this for initialization
    void Start() {
        headMesh.GetComponent<SkinnedMeshRenderer>().enabled = false; // Hiding player character head to avoid bugs :)
    }

    // Update is called once per frame
    void Update() {
        // Cool down times
        if (gunShotTime >= 0.0f)
        {
            gunShotTime -= Time.deltaTime;
        }
        if (gunReloadTime >= 0.0f)
        {
            gunReloadTime -= Time.deltaTime;
        }

        // ready for fire
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && gunShotTime <= 0 && gunReloadTime <= 0.0f && magBulletsVal > 0 && !isDead)
        { 
            shotDetection(); // Should be completed

            addEffects(); // Should be completed

            animator.SetBool("fire", true);
            gunShotTime = 0.5f;
            
            // Instantiating the muzzle prefab and shot sound
            
            magBulletsVal = magBulletsVal - 1;
            if (magBulletsVal <= 0 && remainingBulletsVal > 0)
            {
                animator.SetBool("reloadAfterFire", true);
                gunReloadTime = 2.5f;
                Invoke("reloaded", 2.5f);
            }
        }
        else
        {
            animator.SetBool("fire", false);
        }
        // press R to reload
        if ((Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.R)) && gunReloadTime <= 0.0f && gunShotTime <= 0.1f && remainingBulletsVal > 0 && magBulletsVal < magSize && !isDead )
        {
            animator.SetBool("reload", true);// run the reload animation --> call ReloadEvent(1) & call ReloadEvent(2)
            gunReloadTime = 2.5f;
            //call reloaded, in 2.5 seconds
            Invoke("reloaded", 2.0f);
        }
        else
        {
            animator.SetBool("reload", false);
        }
        updateText();
       
    }

  

    public void Being_shot(float damage) // getting hit from enemy
    {
        health = health - damage;
        // if health < 0, run the death animation and move the camera to a fixed position
        if(health <= 0)
        {
            isDead = true;
            GetComponent<Animator>().SetBool("dead", true);
            GetComponent<CharacterMovement>().isDead = true;
            GetComponent<CharacterController>().enabled = false;
            // show head
            headMesh.GetComponent<SkinnedMeshRenderer>().enabled = true;   

            // restart the game after 10s
            Invoke("restartGame", 10f);     
        }
    }

    // https://docs.unity3d.com/ScriptReference/Collider.OnTriggerEnter.html
    void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.tag == "door")
        {
            Debug.Log("Restart Game in 10 seconds");
            Invoke("restartGame", 10f);
        }
    }
    void restartGame()
    {
        Debug.Log("Restart Game in 10 seconds");
        SceneManager.LoadScene("SampleScene");
    }
    // call by animation itself
    // access handMag and gunMag
    public void ReloadEvent(int eventNumber) // appearing and disappearing the handMag and gunMag
    {
        if(eventNumber == 1)
        {
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
            handMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
        }
        if(eventNumber == 2)
        {
            gunMag.GetComponent<SkinnedMeshRenderer>().enabled = true;
            handMag.GetComponent<SkinnedMeshRenderer>().enabled = false;
        }
        
    }

    void reloaded()// what happened after gun is done reloading
    {
        int newMagBulletsVal = Mathf.Min(remainingBulletsVal + magBulletsVal, magSize);
        int addedBullets = newMagBulletsVal - magBulletsVal;
        magBulletsVal = newMagBulletsVal;
        remainingBulletsVal = Mathf.Max(0, remainingBulletsVal - addedBullets);
        animator.SetBool("reloadAfterFire", false);
    }

    void updateText()
    {
        magBullets.text = magBulletsVal.ToString() ;
        remainingBullets.text = remainingBulletsVal.ToString();
        //add health information
        playerHealth.text = health.ToString();
    }

    void shotDetection() // Detecting the object which player shot 
    {
        // bit shift the index of the layer to get the bit mask
        int layerMask = 1 << 8;
        layerMask = ~layerMask;

        RaycastHit rayHit;
        if(Physics.Raycast(end.transform.position, (end.transform.position - start.transform.position).normalized, out rayHit, 100.0f))
        {
            if(rayHit.transform.tag == "enemy")
            {
                //call whatever
                rayHit.transform.GetComponent<Enemy>().Being_shot(20);

            }else
            {
                //bullet hole
                //collider: give the object that we hit
                GameObject BulletHoleObject = Instantiate(bulletHole, rayHit.point + rayHit.collider.transform.up * 0.01f, rayHit.collider.transform.rotation);
            }
            
        }
        // muzzle flash
        //GameObject flash = Instantiate(muzzleFlash, end.transform.position, end.transform.rotation);
        //flash.GetComponent<ParticleSystem>().Play();
        //Destroy(flash, 1.0f);

        // shot sound
        //Destroy((GameObject) Instantiate(shotSound, transform.position, transform.rotation), 2.0f);
    }

    void addEffects() // Adding muzzle flash, shoot sound and bullet hole on the wall
    {
        // muzzle flash
        GameObject flash = Instantiate(muzzleFlash, end.transform.position, end.transform.rotation);
        flash.GetComponent<ParticleSystem>().Play();
        Destroy(flash, 1.0f);

        // shot sound
        Destroy((GameObject) Instantiate(shotSound, transform.position, transform.rotation), 2.0f);
    
    }

}
