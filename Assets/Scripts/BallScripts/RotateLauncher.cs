﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLauncher : MonoBehaviour
{
    public GameObject dummyBall;
    public float ballSpeed = 10;
    private GameObject instanceBall;
    public float shootDelay = 0.5f;
    private bool canShoot = true;
    public AudioSource shotSound;

    private Vector3 lookPos;

    private void Start()
    {
        CreateBall();
    }

    private void Update()
    {
        RotatePlayerAlongMousePosition();
        SetBallPostion();
    }

    private void FixedUpdate()
    {
        ShootBall();
    }

    private void RotatePlayerAlongMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Camera.main.transform.position.y))
            lookPos = hit.point;

        Vector3 lookDir = lookPos - transform.position;
        lookDir.y = 0;

        transform.LookAt(transform.position + lookDir, Vector3.up);
    }

    private void SetBallPostion()
    {
        if (instanceBall != null)
        {
            instanceBall.transform.forward = transform.forward;
            instanceBall.transform.position = transform.position + transform.forward * transform.localScale.z;
        }
    }

    private void ShootBall()
    {
        if (Input.GetKey(KeyCode.Mouse1) && canShoot)
        {
            instanceBall.GetComponent<Rigidbody>().AddForce(instanceBall.transform.forward * ballSpeed);
            CreateBall();
            shotSound.Play();
            StartCoroutine(ShootDelay());
        }
    }

    private IEnumerator ShootDelay()
    {
        canShoot = false;
        yield return new WaitForSeconds(shootDelay);
        canShoot = true;
    }

    private void CreateBall()
    {

        instanceBall = Instantiate(dummyBall, transform.position, Quaternion.identity);
        instanceBall.SetActive(true);

        instanceBall.tag = "NewBall";
        instanceBall.gameObject.layer = LayerMask.NameToLayer("Default");

        SetBallColor(instanceBall);
    }

    private void SetRandomColor(GameObject go)
    {
        Color color = new Color(Random.Range(0F, 1F), Random.Range(0, 1F), Random.Range(0, 1F));
        go.GetComponent<Renderer>().material.SetColor("_Color", color);
    }

    private void SetBallColor(GameObject go)
	{
		BallColor randColor = MoveBalls.GetRandomBallColor();

		switch (randColor)
		{
			case BallColor.red:
				go.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
				break;

			case BallColor.green:
				go.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
				break;

			case BallColor.blue:
				go.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
				break;
		}
	}
}
