using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] PlayerShooting playerShoot;
    public WeaponParametars[] weapons;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartCoroutine(ChangeWeapon(0));
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(ChangeWeapon(1));
        }
       
       
    }

    IEnumerator ChangeWeapon(int weaponID)
    {
        playerShoot.weaponReady = false;
        yield return new WaitForSeconds(1);
        playerShoot.currentWeapon.gameObject.SetActive(false);
        playerShoot.currentWeapon = weapons[weaponID];
        playerShoot.currentWeapon.gameObject.SetActive(true);
        playerShoot.RefreshAmmoText();
        yield return new WaitForSeconds(1);
        playerShoot.weaponReady = true;
    }
}
