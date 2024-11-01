using UnityEngine;

public class HookBehavior : MonoBehaviour
{
    //Serialize Params
    [SerializeField] public float hookWeight = 1f;
    [SerializeField] float waterSlowMultiplier = 1f;
    [SerializeField] float waterGravity = -0.3f;

    //Cached Comp
    Rigidbody rb;

    //State
    bool inWater = false;


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

        }
    }
}
