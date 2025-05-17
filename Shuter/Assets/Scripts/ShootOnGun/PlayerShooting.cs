using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public TMP_Text ammoText;
    public WeaponParametars currentWeapon;
    public bool weaponReady, isShooting;
    public Transform cam;
    public Transform hitPoint;
    private AudioSource au;
    // Start is called before the first frame update
    void Start()
    {
        RefreshAmmoText();
        au = GetComponent<AudioSource>();
        cam = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }
    public void Shoot()
    {
        if(!weaponReady)
        {
            return;
        }
        if((currentWeapon.ammoInClip > 0 || currentWeapon.isKnife) && currentWeapon.lastTimeShoot < Time.time)
        {
            au.PlayOneShot(currentWeapon.shootClip[Random.Range(0, 1)]);
            if (currentWeapon.muzzleFlash.Length > 0)
            {
                Instantiate(currentWeapon.muzzleFlash[1], currentWeapon.shootPoint.position, Quaternion.identity);
            }
            currentWeapon.ammoInClip--;
            currentWeapon.lastTimeShoot = Time.time + currentWeapon.shootCoolDown;
            RaycastHit hit;
            Ray ray = new Ray(cam.position, cam.forward + Random.insideUnitSphere * currentWeapon.spread);
            if(Physics.Raycast(ray,out hit, currentWeapon.range))
            {
                hitPoint.position = hit.point;
            }
            RefreshAmmoText();
        }
    }
    public void Reload()
    {
        if (currentWeapon.isKnife) return;
        if (currentWeapon.ammoInClip == currentWeapon.clipSize || currentWeapon.ammoTotal <= 0)
        {
            return;
        }
        int ammoTotal = currentWeapon.ammoInClip + currentWeapon.ammoTotal;
        if(ammoTotal >= currentWeapon.clipSize)
        {
            currentWeapon.ammoInClip = currentWeapon.clipSize;
            currentWeapon.ammoTotal = ammoTotal - currentWeapon.clipSize;
        }
        else
        {
            currentWeapon.ammoInClip = ammoTotal;
            currentWeapon.ammoTotal = 0;
        }
        au.PlayOneShot(currentWeapon.reloadClip);
        currentWeapon.lastTimeShoot = currentWeapon.reloadTime + Time.time;
        RefreshAmmoText();
    }

    public void RefreshAmmoText()
    {
        if(!currentWeapon.isKnife)
        {
            ammoText.text = currentWeapon.ammoInClip + "/" + currentWeapon.ammoTotal;
        }
        else
        {
            ammoText.text = " ";
        }
    }
}
