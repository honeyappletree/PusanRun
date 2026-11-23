using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int curHealth;

    Rigidbody rigid;
    BoxCollider collider;
    Material mat;
    public ParticleSystem explosion;
    public AudioSource dead;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        collider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        anim = GetComponentInChildren<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;

            StartCoroutine(OnDamage());
        }
    }

    IEnumerator OnDamage()
    {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);

        if(curHealth > 0)
        {
            mat.color = Color.white;
        }
        else
        {
            mat.color = Color.gray;
            gameObject.layer = 8;
            anim.SetTrigger("doDead");
            Destroy(gameObject, 0.8f);
            explosion.Play();
            dead.Play();
        }
    }
}
