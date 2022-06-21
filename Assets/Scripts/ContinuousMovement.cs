using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ContinuousMovement : MonoBehaviour {

    [Header("Continuous Movement Config:")]
    [Tooltip("Action with value Button")]
    [SerializeField] InputActionReference continuousMovementActivation;
    [Tooltip("Trackpad/Axis returning Vector2")]
    [SerializeField] InputActionReference continuousMovementTrackpad;
    [SerializeField] List<GameObject> trackpadDependencies;
    [SerializeField] bool hasContinuousWalk;
    [SerializeField] float walkSpeed = 25;
    [Tooltip("Responsable of locomotion range, both forward/back and left/right")]
    [SerializeField] [Range(0f, 1f)] float axisXLimit = 0.5f;
    [SerializeField] bool hasContinuousRotation;
    [SerializeField] float rotationSensitivity = 25;
    [Tooltip("Responsable of rotation activation")]
    [SerializeField] [Range(0f, 1f)] float axisYLimit = 0.5f;

    [Tooltip("Gravity 0 equals to no gravity affecting Player")]
    [SerializeField] float gravity = -9.81f;
    [SerializeField] LayerMask groundLayer;

    private float additionalHeight = 0.2f;
    private float fallingSpeed;
    private XRRig rig;

    private bool hasGravity = true;

    private CharacterController characterController;

    void Start()
    {
        characterController = gameObject.GetComponent<CharacterController>();
        rig = gameObject.GetComponent<XRRig>();

        if (hasContinuousWalk & hasContinuousRotation & (axisXLimit + axisYLimit > 1)) {
            Debug.LogError("Conflic of \"Interests\" between Locomotion and Rotation");
            return;
        }

        if(hasContinuousWalk) 
            continuousMovementActivation.action.performed += ContinuousWalkActivation;

        if (hasContinuousRotation) 
            continuousMovementActivation.action.performed += ContinuousRotationActivation;

        continuousMovementActivation.action.Enable();

    }

    private void FixedUpdate() {
        if (characterController)
            CapsuleFollowHeadset();

        if (gravity != 0 && hasGravity)
            ControlGravity();
    }

    public void ChangeGravityStatus(bool hasGravity) {
        this.hasGravity = hasGravity;
    }

    public void DisableContinuousWalk() {
        if (hasContinuousWalk)
            continuousMovementActivation.action.performed -= ContinuousWalkActivation;
        
        hasContinuousWalk = false;

    }

    public void EnableContinuousWalk() {
        if (!hasContinuousWalk)
            continuousMovementActivation.action.performed += ContinuousWalkActivation;

        hasContinuousWalk = true;

    }

    public void DisableContinuousRotation() {
        if (hasContinuousRotation)
            continuousMovementActivation.action.performed -= ContinuousRotationActivation;

        hasContinuousRotation = false;

    }

    public void EnableContinuousRotation() {
        if (!hasContinuousRotation)
            continuousMovementActivation.action.performed += ContinuousRotationActivation;

        hasContinuousRotation = true;

    }

    private IEnumerator EnableDependenciesTimer() {
        yield return new WaitForSeconds(0.7f);
        EnableDependencies();
    }

    private void ContinuousMovementDesactivation(InputAction.CallbackContext obj) {
        continuousMovementTrackpad.action.performed -= CheckLocomotionDirection;

        if (trackpadDependencies != null)
            StartCoroutine(EnableDependenciesTimer());

        continuousMovementActivation.action.canceled -= ContinuousMovementDesactivation;
    }

    private void ContinuousRotationActivation(InputAction.CallbackContext obj) {
        continuousMovementTrackpad.action.performed += CheckRotationDirection;
        continuousMovementTrackpad.action.Enable();
        continuousMovementActivation.action.canceled += ContinuousMovementDesactivation;

    }

    private void ContinuousWalkActivation(InputAction.CallbackContext obj) {
        continuousMovementTrackpad.action.performed += CheckLocomotionDirection;
        continuousMovementTrackpad.action.Enable();

        continuousMovementActivation.action.canceled += ContinuousMovementDesactivation;
    }

    private void CheckLocomotionDirection(InputAction.CallbackContext obj) {
        Vector2 vector = obj.ReadValue<Vector2>();
        Quaternion headYaw = Quaternion.Euler(0, rig.cameraGameObject.transform.eulerAngles.y, 0);

        if (vector.x > -axisXLimit && vector.x < axisXLimit) {
            if (trackpadDependencies != null)
                DisableDependencies();
            characterController.Move(headYaw * new Vector3(vector.x, 0, vector.y) * Time.deltaTime * walkSpeed);

        }
            

    }
    
    private void CheckRotationDirection(InputAction.CallbackContext obj) {
       Vector2 vector = obj.ReadValue<Vector2>();

        if (vector.y > -axisYLimit && vector.y < axisYLimit) {
            rig.RotateAroundCameraUsingRigUp(Mathf.RoundToInt(vector.x) * rotationSensitivity);
            continuousMovementTrackpad.action.performed -= CheckRotationDirection;

        }

    }



    private void CapsuleFollowHeadset() {
        characterController.height = rig.cameraInRigSpaceHeight + additionalHeight;
        Vector3 capsuleCenter = transform.InverseTransformPoint(rig.cameraGameObject.transform.position);
        characterController.center = new Vector3(capsuleCenter.x, characterController.height / 2 + characterController.skinWidth, capsuleCenter.z);

    }

    private bool IsGrounded() {
        Vector3 rayStart = transform.TransformPoint(characterController.center);
        float rayLength = characterController.center.y / 2;
        return Physics.SphereCast(rayStart, characterController.radius, Vector3.down, out RaycastHit hitInfo, rayLength, groundLayer);

    }

    private void ControlGravity() {
        if (IsGrounded()) {
            fallingSpeed = 0;

        } else {
            fallingSpeed += gravity * Time.fixedDeltaTime;

        }

        characterController.Move(Vector3.up * fallingSpeed * Time.deltaTime);

    }

    private void DisableDependencies() {
        foreach (GameObject gO in trackpadDependencies) {
            gO.SetActive(false);

        }

    }

    private void EnableDependencies() {
        foreach (GameObject gO in trackpadDependencies) {
            gO.SetActive(true);

        }

    }

}
