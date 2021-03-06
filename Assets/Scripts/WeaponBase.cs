﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public float soundBlastPower;
    public float degreeOffset;
    public float cooldownTimerLength;
    [HideInInspector]
    public float cooldownTimer;
    public float screenShake;
    public bool additiveKnockback;

    public GameObject blastPrefab;

    public Transform blastSpawnPoint;
    protected Player player;
    public int weaponIndex; // BAD LOL

    protected virtual void Start()
    {
        player = GetComponentInParent<Player>();
    }

    protected virtual void Update()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer < 0) cooldownTimer = 0;

        if (cooldownTimer == 0 && Input.GetButton("P" + player.playerNum + "_Fire"))
        {
            CameraShakeHandler.instance.AddIntensity(screenShake);
            cooldownTimer = cooldownTimerLength;
            
            if (additiveKnockback)
                player.rb.AddForce(-player.transform.right * soundBlastPower, ForceMode2D.Impulse);
            else
                player.rb.velocity = -player.transform.right * soundBlastPower;

            GameObject newBlast = Instantiate(blastPrefab, new Vector2(blastSpawnPoint.position.x, blastSpawnPoint.position.y), Quaternion.identity);
            newBlast.transform.rotation = player.transform.rotation;
            newBlast.transform.localEulerAngles = new Vector3(newBlast.transform.localEulerAngles.x, newBlast.transform.localEulerAngles.y, newBlast.transform.localEulerAngles.z + Random.Range(-degreeOffset, degreeOffset));
            newBlast.GetComponent<Blast>().owner = player;
        }
    }
}
