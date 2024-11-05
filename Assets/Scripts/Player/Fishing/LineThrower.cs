using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class LineThrower : MonoBehaviour
{
    //Serialize Params
    [Header("References")]
    [SerializeField] GameObject hookPrefab;
    [SerializeField] Transform releasePosition;
    [SerializeField] CinemachineCamera fpsCam;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] GameObject rod;

    [Header("Throw Power")]
    [SerializeField] float minThrowPower = 10;
    [SerializeField] float maxThrowPower = 200;
    [SerializeField] float throwUpwardPower = 10;

    [Header("Charge and Throw")]
    [SerializeField] float chargeSpeed = 10;
    [SerializeField] float leftStickErrorRoom = 0.05f;
    [SerializeField] float stickFlickMaxTime = 1.5f; //Should be as long as animation blend time to throw blendtree
    [SerializeField] float minThrowY = 0.75f;
    [SerializeField] float maxThrowX = 0.6f;

    [Header("Line Renderer")]
    [SerializeField] bool lineRendererToggle = true;
    [SerializeField] [Range(10, 100)] int linePoints = 25;
    [SerializeField] [Range(0.01f, 0.25f)] float timeBetweenPoints = 0.1f;

    [Header("Visuals")]
    [SerializeField] Material standardMat;
    [SerializeField] Material releaseHeldMat;

    //Cached Comps
    PlayerController playerController;

    //State
    public float currentCharge;
    bool isCharging = false;
    public bool releaseHeld = false;
    bool canThrow = false;
    Vector2 aimDir;
    Coroutine throwTime;
    bool hookOut;

    GameObject currentHook;

    //animation stuff
    float aimDirX;

    #region Built-In Functions

    private void Awake()
    {
        //Cache Comps
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (isCharging && releaseHeld)
        {
            //Charges
            currentCharge += chargeSpeed * Time.deltaTime;
            currentCharge = Mathf.Clamp(currentCharge, minThrowPower, maxThrowPower);
            DrawProjection();
        }
        else
        {
            lineRenderer.enabled = false;
        }
        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        if (hookOut)
        {
            ReelIn(playerController.stickDelta);
        }
    }

    private void UpdateVisuals()
    {
        if (releaseHeld)
        {
            rod.GetComponent<MeshRenderer>().material = releaseHeldMat;
        }
        else
        {
            rod.GetComponent<MeshRenderer>().material = standardMat;
        }
        //Charge anim
        if (isCharging) 
        {
            GetComponent<PlayerAnimationController>().RodCharge(currentCharge/maxThrowPower);
        }

        //Throw Anim
        if (!isCharging)
        {
            GetComponent<PlayerAnimationController>().SetThrowAim(aimDirX, currentCharge/maxThrowPower);
        }
        

    }
    #endregion
    public void SetRelease(bool release)
    {
        releaseHeld = release;
    }

    private void StartCharge()
    {
        //As long as button is held will charge the throw
        isCharging = true;
        currentCharge = 0;
    }

    public void SetAim(Vector2 dir)
    {
        aimDir = dir;
        //If stick down will start charge
        if (aimDir.y <= -1 + leftStickErrorRoom && !isCharging && !canThrow && releaseHeld)
        {
            StartCharge();
        }
        //If stick leave charge position
        else if (aimDir.y > -1 + leftStickErrorRoom && isCharging && !canThrow && releaseHeld)
        {
            //Activate Throw Animation
            aimDirX = aimDir.x;
            GetComponent<PlayerAnimationController>().StartThrow();

            //Stops charging
            isCharging = false;

            //Start Timer for finishing throw
            canThrow = true;
            throwTime = StartCoroutine(ThrowTimer());
        }
        //If final position and can throw
        else if(aimDir.y > minThrowY && aimDir.x > -maxThrowX && aimDir.x < maxThrowX && canThrow && !releaseHeld)
        {
            aimDirX = aimDir.x;
            //Release Throw Animation


            canThrow = false;
            //Throw depending on angle
            Throw(aimDir);
        }

    }

    private IEnumerator ThrowTimer()
    {
        yield return new WaitForSeconds(stickFlickMaxTime);
        canThrow = false;
    }

    private void Throw(Vector2 dir)
    {
        //Catapults the object based on stick trajectory/movement and charge duration
        //Insantiate
        currentHook = Instantiate(hookPrefab, releasePosition.position, transform.rotation);
        currentHook.GetComponent<HookBehavior>().SetRodPos(releasePosition.position);

        //Calc Relative direction of throw
        Vector3 globalDir = new Vector3(dir.x, 0, dir.y);
        var relativeDir = transform.rotation * globalDir;
        Debug.Log(globalDir + " / " + relativeDir);

        //Add Force

        Rigidbody rbProjectile = currentHook.GetComponent<Rigidbody>();
        Vector3 forceToAdd = relativeDir * currentCharge + transform.up * throwUpwardPower;
        rbProjectile.AddForce(forceToAdd, ForceMode.Impulse);

        //Tell Player Controller hook is out
        hookOut = true;
        playerController.HookOut();

    }

    private void DrawProjection()
    {
        if (!lineRendererToggle) {  return; }
        lineRenderer.enabled = true;
        lineRenderer.positionCount = Mathf.CeilToInt(linePoints / timeBetweenPoints) + 1;
        Vector3 startPosition = releasePosition.position;
        Vector3 startVelocity = (currentCharge * transform.forward + transform.up * throwUpwardPower) / hookPrefab.GetComponent<Rigidbody>().mass;
        int i = 0;
        lineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < linePoints; time += timeBetweenPoints)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time); ;
        
            lineRenderer.SetPosition(i, point);
        }
    }

    public void ReelIn(float stickDelta)
    {
        if (stickDelta < 0) { return; }
        currentHook.GetComponent<HookBehavior>().Reel(stickDelta);
    }


}
