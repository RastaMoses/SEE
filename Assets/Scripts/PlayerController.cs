using System;
using System.Collections;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Serialize Params
    [Header("Variations")]
    [SerializeField] bool tankControls = true;
    [SerializeField] bool stickRotation = false;
    [SerializeField] bool rotateWhileMoving = false;
    [SerializeField] bool sideMovement = false;

    [Header("Params")]
    [SerializeField] float speed = 10f;
    [SerializeField] float rotateSpeed = 5f;
    [SerializeField] float sideMovementSlow = 0.6f;
    [SerializeField] float reelMaxRotateSpeed;
    [Header("Objects")]
    [SerializeField] FirstPersonCamera fpsCamera;

    [Header("Input Actions")]
    [SerializeField] InputActionReference releaseAction;
    [SerializeField] InputActionReference rotateLeftAction;
    [SerializeField] InputActionReference rotateRightAction;

    //Cached Comps
    CharacterController controller;
    PlayerInput input;
    CinemachineBrain cinemachineBrain;
    LineThrower lineThrower;

    //State
    bool isExploration = true;
    bool isFishing = false;
    Vector2 playerMoveInput;
    CinemachineCamera currentCam;
    Vector3 camForward;
    Vector3 camRight;
    bool rotationLeftBumper;
    bool rotationRightBumper;

    //Fishin
    bool isHookThrown = false;
    float reelDeltaX, reelDeltaY, reelOldX, reelOldY;
    Vector2 reelInputDir;


    private void OnEnable()
    {
        //Connect Method TO Buttons
        releaseAction.action.performed += ReleasePressed;
        releaseAction.action.canceled += ReleaseReleased;

        rotateLeftAction.action.performed += RotateLeftPressed;
        rotateLeftAction.action.canceled += RotateLeftReleased;
        rotateRightAction.action.performed += RotateRightPressed;
        rotateRightAction.action.canceled += RotateRightReleased;

    }
    private void OnDisable()
    {
        //Disconnect Method from buttons
        releaseAction.action.canceled -= ReleaseReleased;
        releaseAction.action.performed -= ReleasePressed;

        rotateLeftAction.action.performed -= RotateLeftPressed;
        rotateLeftAction.action.canceled -= RotateLeftReleased;
        rotateRightAction.action.performed -= RotateRightPressed;
        rotateRightAction.action.canceled -= RotateRightReleased;
    }

    private void Awake()
    {
        //Comps to cache
        lineThrower = GetComponent<LineThrower>();
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        cinemachineBrain = FindFirstObjectByType<CinemachineBrain>();

        //Connect Events


        //Initial Setting
        ToggleActionMap("Fishing");
        ToggleActionMap("Exploration");

        isExploration = true;

            //Lock and hide mouse cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
            //Delayed Start after Cinemachine for camera-relative movement
        StartCoroutine(DelayedStart());
    }

    private void Update()
    {
        if (!isExploration) { return; }
        if (tankControls) { TankMove(); }
        else
        {
            if (playerMoveInput != Vector2.zero)
            {
                CameraRelativeMove();
            }
        }
    }

    private void LateUpdate()
    {
        //LateUpdate because Cinemachine positioning gets updated after update
        if (!tankControls)
        {
            //Reset Movement angle to camera, when reset movement
            if (playerMoveInput == Vector2.zero)
            {
                ResetMovementAngle();
            }
        }
    }
    #region Inputs
    private void OnMove(InputValue value)
        {
            //Gets movement value from input
            playerMoveInput = value.Get<Vector2>();
        }
    private void OnFish()
    {
        EnterFishing();
    }

    private void OnLook(InputValue value)
    {
        //Sends value to fps camera script
        fpsCamera.Look(value);
    }

    private void OnCancel()
    {
        //Return to Exploration Camera if in fishing (if not during Charging)
        if (isFishing)
        {
            ExitFishing();
        }
    }

    private void OnAim(InputValue value)
    {
        if (!isFishing) { return; }
        if (isHookThrown)
        {
            ReelIn(value.Get<Vector2>());
        }
        else
        {
            Vector2 dir = value.Get<Vector2>();
            lineThrower.SetAim(dir);
        }
    }

    private void ReleasePressed(InputAction.CallbackContext ctx)
    {
        //Release was triggered
        lineThrower.SetRelease(true);
    }

    private void ReleaseReleased(InputAction.CallbackContext ctx)
    {
        lineThrower?.SetRelease(false);
    }

    #endregion

    #region Movement
    private void TankMove()
    {
        float rotation = 0f;
        if (rotationLeftBumper && !rotationRightBumper) { rotation = -1f; }
        else if (rotationRightBumper && !rotationLeftBumper) {rotation = 1f; }

        //Rotates with stick if allowed
        if (stickRotation)
        {
            if (playerMoveInput.x > 0.5f || playerMoveInput.x < -0.5f)
            {
                if (!rotateWhileMoving) { playerMoveInput.y = 0f; }
                rotation = playerMoveInput.x;
            }        
            else
            {
                playerMoveInput.x = 0f;
            }
        }
        else { playerMoveInput.x = 0f; }

        if (controller != null)
        {
            if (sideMovement)
            {
                controller.Move(((transform.forward * playerMoveInput.y) + (transform.right * playerMoveInput.x * sideMovementSlow)) * Time.deltaTime * speed);

            }
            else
            {
                controller.Move((transform.forward * playerMoveInput.y) * Time.deltaTime * speed);

            }
            //Allows rotation while moving
            if (rotateWhileMoving)
            {
                transform.Rotate(transform.up * rotateSpeed * rotation * Time.deltaTime);
            }
            //Only rotate when not moving
            else if (playerMoveInput.y == 0)
            {
                transform.Rotate(transform.up * rotateSpeed * rotation * Time.deltaTime);
            }
        }
        else { Debug.LogError("Character Controller missing"); }
    }

    private void CameraRelativeMove()
    {
        if (controller != null)
        {
            //create relative cam dir
            Vector3 forwardRelative = playerMoveInput.y * camForward;
            Vector3 rightRelative = playerMoveInput.x * camRight;

            Vector3 moveDir = forwardRelative + rightRelative;

            controller.Move(moveDir * speed * Time.deltaTime);
            transform.LookAt(transform.position + moveDir);
        }
        else { Debug.LogError("Character Controller missing"); }
    }

    private void ResetMovementAngle()
    {
        //Get camera data and direction
        currentCam = cinemachineBrain.ActiveVirtualCamera as CinemachineCamera;
        camForward = currentCam.transform.forward;
        camRight = currentCam.transform.right;
        camForward.y = 0;
        camRight.y = 0;
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitUntil(() => cinemachineBrain.ActiveVirtualCamera != null);
        ResetMovementAngle();
    }

    private void RotateLeftPressed(InputAction.CallbackContext ctx)
    {
        rotationLeftBumper = true;
    }
    private void RotateLeftReleased(InputAction.CallbackContext ctx)
    {
        rotationLeftBumper = false;
    }

    private void RotateRightPressed(InputAction.CallbackContext ctx)
    {
        rotationRightBumper = true;
    }
    private void RotateRightReleased(InputAction.CallbackContext ctx)
    {
        rotationRightBumper = false;
    }
    #endregion

    #region Fishing
    private void ReelIn(Vector2 input)
    {
        float deltaValuex = Input.GetAxisRaw("Horizontal") - reelOldX;
        float deltaValuey = Input.GetAxisRaw("Vertical") - reelOldY;
        reelOldX = Input.GetAxisRaw("Horizontal");
        reelOldY = Input.GetAxisRaw("Vertical");
        if (Mathf.Abs(deltaValuex) > reelMaxRotateSpeed && Mathf.Abs(deltaValuey) > reelMaxRotateSpeed)
        {
            reelInputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            
        }
    }
    

    #endregion

    #region Player States
    public void EnterFishing()
    {
        isFishing = true;
        ToggleActionMap("Fishing");
        //Call event
        EventManager.EnterFishingEvent();
    }

    public void ExitFishing()
    {
        isFishing = false;
        ToggleActionMap("Exploration");
        //Call event
        EventManager.ExitFishingEvent();
    }

    private void ToggleActionMap(string newActionMapName)
    {
        input.currentActionMap.Disable();
        input.SwitchCurrentActionMap(newActionMapName);
        input.currentActionMap.Enable();
        //Call event
        EventManager.ActionMapChangeEvent(newActionMapName);
    }

    #endregion
}

