using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public GameObject shield;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ShieldDuration());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(3f);
        shield.SetActive(false);
    }
}
