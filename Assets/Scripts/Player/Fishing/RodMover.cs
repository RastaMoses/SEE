using UnityEngine;
using UnityEngine.InputSystem;

public class RodMover : MonoBehaviour
{
    //Serialize Params
    [SerializeField] private float moveSpeed = 0.05f;
    [SerializeField] private float minMovement = 0.1f;
    [SerializeField] private float resetSpeed = 2f;
    [Header("Alternative Control")]
    [SerializeField] bool instantControl = false;


    //Cached Comps
    PlayerAnimationController anim; 

    //State
    private Transform originalTransform;
    private Vector2 inputs;
    private Vector2 currentVelocity;
    private bool resetting;

    private void Start()
    {
        anim = GetComponent<PlayerAnimationController>();
    }


    private void FixedUpdate()
    {
        if (instantControl)
        {
            UpdateAnimation(inputs);
            return;
        }
        //If input basically zero
        if (inputs.x > -0.05f && inputs.x < 0.05f && inputs.y > -0.05f && inputs.y < 0.05f)
        {
            currentVelocity = Vector2.zero;
            return;
        }
        
        if (resetting)
        {
            if (currentVelocity.x > 0) { currentVelocity.x = Mathf.Clamp(currentVelocity.x - (resetSpeed * Time.fixedDeltaTime), 0, 1); }
            if (currentVelocity.x < 0) { currentVelocity.x = Mathf.Clamp(currentVelocity.x + (resetSpeed * Time.fixedDeltaTime), -1, 0); }
            if (currentVelocity.y > 0) { currentVelocity.y = Mathf.Clamp(currentVelocity.y - (resetSpeed * Time.fixedDeltaTime), 0, 1); }
            if (currentVelocity.y < 0) { currentVelocity.y = Mathf.Clamp(currentVelocity.y + (resetSpeed * Time.fixedDeltaTime), -1, 0); }
        }
        else
        {
            currentVelocity = (Time.fixedDeltaTime * moveSpeed * inputs) + currentVelocity;
            currentVelocity.x = Mathf.Clamp(currentVelocity.x, -1, 1);
            currentVelocity.y = Mathf.Clamp(currentVelocity.y, -1, 1);
        }

        UpdateAnimation(currentVelocity);
    }

    public void Input(Vector2 input)
    {
        inputs = input;
        if (inputs.magnitude < minMovement)
        {
            Reset();
        }
        else
        {
            resetting = false;
        }
    }

    public void Reset()
    {
        resetting = true;
    }


    private void UpdateAnimation(Vector2 movement)
    {
        anim.SetRodMove(movement);
    }
}
