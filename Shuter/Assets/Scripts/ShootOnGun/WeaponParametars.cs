using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponParametars : MonoBehaviour
{
    [Header("Parametars")]
    public GameObject[] muzzleFlash;
    public AudioClip[] shootClip;
    public AudioClip reloadClip;
    public float damage, shootCoolDown, reloadTime;
    public int ammoInClip, clipSize, ammoTotal;
    public bool isKnife;
    public float range = 500f;
    public float lastTimeShoot;
    public float spread = 0.1f;
    public Transform shootPoint;
  
}
