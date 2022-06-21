using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InformationDisplay : MonoBehaviour
{
    [Header("Information Display Config:")]
    [SerializeField] TextMeshProUGUI infomation;
    [SerializeField] Transform canvasRotationPoint;
    [SerializeField] Transform playerCamera;
    [SerializeField] float distance = 5f;

    private Transform rotationPivot;

    // Start is called before the first frame update
    void Start()
    {
        rotationPivot = GetComponent<Transform>();

        playerCamera = FindObjectOfType<Camera>().gameObject.transform;
    }

    public void DisplayInformation(string objectInformation) {
        infomation.text = objectInformation;

        canvasRotationPoint.position = playerCamera.transform.position + playerCamera.transform.forward * distance;
        canvasRotationPoint.position = new Vector3(canvasRotationPoint.position.x, 1f, canvasRotationPoint.position.z);
        canvasRotationPoint.rotation = new Quaternion(0f, playerCamera.transform.rotation.y, 0f, playerCamera.transform.rotation.w);
        canvasRotationPoint.gameObject.SetActive(true);
    }
    
}
