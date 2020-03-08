﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public int playerNum;
    public int lifeCount;
    public float speed;
    public float speedLimit;
    public float jumpPower;
    public float rotationSpeed;
    bool freeAim = true;

    bool invincible;
    public float iFrameDuration;

    public LayerMask groundLayerMask;

    Vector2 playerInput;
    bool grounded;
    float playerAngle;
    float vel;

    public Transform spawnPointContainer;

    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public CircleCollider2D circleCollider2D;
    [HideInInspector]
    public SpriteRenderer sr;

    [HideInInspector]
    public WeaponBase weaponBase;

    public GameObject[] randomWeaponPrefabs;

    public GridLayoutGroup UILifeCounter;
    public GameObject lifeImagePrefab;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        weaponBase = GetComponentInChildren<WeaponBase>();
        GiveWeapon(randomWeaponPrefabs[Random.Range(0, randomWeaponPrefabs.Length)]);
        freeAim = GameManager.instance.freeAim;

        for (int i = 0; i < lifeCount; i++)
        {
            Image newLifeImage = Instantiate(lifeImagePrefab, UILifeCounter.transform).GetComponent<Image>();
            newLifeImage.color = sr.color;
            if (playerNum == 2) newLifeImage.transform.localScale = new Vector3(-newLifeImage.transform.localScale.x, newLifeImage.transform.localScale.y, newLifeImage.transform.localScale.z);
        }
    }

    void Update()
    {
        if (invincible) sr.enabled = !sr.enabled;

        // Get player input
        playerInput = new Vector2(Input.GetAxisRaw("P" + playerNum + "_Horizontal"), Input.GetAxisRaw("P" + playerNum + "_Vertical"));

        // Jump control
        grounded = Physics2D.OverlapCircle(transform.position, circleCollider2D.radius * 1.5f, groundLayerMask);
        if (grounded && playerInput.y == 1)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
        }

        // Speed limit
        if (rb.velocity.x > speedLimit) rb.velocity = new Vector2(speedLimit, rb.velocity.y);
        else if (rb.velocity.x < -speedLimit) rb.velocity = new Vector2(-speedLimit, rb.velocity.y);

        // Rotation
        if (freeAim)
        {
            if (playerInput != Vector2.zero)
            {
                playerAngle = -(TrigUtilities.VectorToDegrees(playerInput) - 90);
            }
        }
        else
        {
            if (!grounded)
            {
                playerAngle -= rotationSpeed * Time.deltaTime;
                if (playerAngle < 0) playerAngle += 360;
            }
        }

        // Off-screen death
        if (GameManager.instance.IsTransformOffCamera(transform))
        {
            Die();
        }
    }

    void FixedUpdate()
    {
        rb.rotation = Mathf.SmoothDampAngle(rb.rotation, playerAngle, ref vel, 0.05f);
        rb.AddForce(new Vector2(playerInput.x * speed, 0), ForceMode2D.Force);
    }



    public void GiveWeapon(GameObject newWeapon)
    {
        if (weaponBase != null) Destroy(weaponBase.gameObject);
        GameObject createdWeapon = Instantiate(newWeapon, transform);
        weaponBase = createdWeapon.GetComponent<WeaponBase>();
    }

    public void Die()
    {
        if (gameObject.activeSelf && !invincible)
        {
            CameraShakeHandler.instance.AddIntensity(0.4f);

            GameObject newWeaponDebris = Instantiate(weaponBase.gameObject, weaponBase.transform.position, Quaternion.identity);
            newWeaponDebris.layer = 10;
            Destroy(newWeaponDebris.GetComponent<WeaponBase>());
            BoxCollider2D newBC = newWeaponDebris.AddComponent<BoxCollider2D>();
            Rigidbody2D newRB = newWeaponDebris.AddComponent<Rigidbody2D>();
            newRB.AddForce(new Vector2(Random.Range(-10f, 10f), Random.Range(5f, 10f)), ForceMode2D.Impulse);
            newRB.AddTorque(Random.Range(10f, 40f));
            newWeaponDebris.AddComponent<DestroyWhenOffCamera>();

            gameObject.SetActive(false);

            lifeCount--;
            if (playerNum == 1)
            {
                LeanTween.scale(UILifeCounter.transform.GetChild(UILifeCounter.transform.childCount - 1).gameObject, Vector3.zero, 1f)
                .setEase(LeanTweenType.easeOutExpo)
                .setDestroyOnComplete(true);
            }
            else
            {
                LeanTween.scale(UILifeCounter.transform.GetChild(0).gameObject, Vector3.zero, 1f)
                .setEase(LeanTweenType.easeOutExpo)
                .setDestroyOnComplete(true);
            }
            
            if (lifeCount == 0)
            {
                LeanTween.delayedCall(gameObject, 3, () =>
                {
                    MatchHandler.instance.EndGame(playerNum);
                });
            }
            else
            {
                LeanTween.delayedCall(gameObject, 2, () =>
                {
                    int randomChildIndex = Random.Range(0, spawnPointContainer.childCount);
                    transform.position = spawnPointContainer.GetChild(randomChildIndex).position;
                    GiveWeapon(randomWeaponPrefabs[Random.Range(0, randomWeaponPrefabs.Length)]);

                    invincible = true;
                    LeanTween.delayedCall(gameObject, iFrameDuration, () =>
                    {
                        invincible = false;
                        sr.enabled = true;
                    });

                    gameObject.SetActive(true);
                });
            }
        }
    }
}
