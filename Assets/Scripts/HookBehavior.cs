using UnityEngine;

public class HookBehavior : MonoBehaviour
{
    //Serialize Params
    [SerializeField] public float hookWeight = 1f;
    [SerializeField] float waterSlowMultiplier = 0.6f;
    [SerializeField] float waterGravity = -0.3f;
    [SerializeField] float reelSpeed = 0.1f;

    //Cached Comp
    Rigidbody rb;

    //State
    bool inWater = false;
    Vector3 rodPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Cache
        rb = GetComponent<Rigidbody>();

        //Init
        rb.mass = hookWeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Water"))
        {
            Debug.Log("Touch Water");

            //Touched Water
            inWater = true;

            //Slow down speed
            rb.linearVelocity = rb.linearVelocity * waterSlowMultiplier;


            //Set gravity to water gravity
            rb.useGravity = false;


        } 
    }


    private void FixedUpdate()
    {
        //Slower Water Gravity
        if (inWater)
        {
            rb.AddForce(waterGravity * Vector3.up, ForceMode.Acceleration);

            //Gradually slow down
            rb.linearVelocity *= waterSlowMultiplier;
        }

        
    }

    public void Reel(float stickDelta)
    {
        Debug.Log("Reeling");
        rb.AddForce((rodPos - transform.position) * stickDelta * reelSpeed, ForceMode.Force);
    }

    public void SetRodPos(Vector3 pos)
    {
        rodPos = pos;
    }
}
