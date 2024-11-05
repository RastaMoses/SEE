using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class FishingLineController : MonoBehaviour
{
    //Objects that will interact with the rope
    public Transform whatTheRopeIsConnectedTo;
    public Transform whatIsHangingFromTheRope;

    //Line renderer used to display the rope
    private LineRenderer lineRenderer;

    //A list with all rope sections
    public List<Vector3> allRopeSections = new List<Vector3>();

    //Rope data
    private float ropeLength = 1f;
    private float minRopeLength = 1f;
    private float maxRopeLength = 20f;
    //Mass of what the rope is carrying
    private float loadMass = 100f;
    //How fast we can add more/less rope
    float winchSpeed = 2f;


    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    private float ropeSegmentLength = 0.25f;
    private int segmentCount = 20;
    private float lineWidth = 0.1f;
    [SerializeField] private int startSegmentCount = 10;

    //The joint we use to approximate the rope
    SpringJoint springJoint;

    void Start()
    {
        springJoint = whatTheRopeIsConnectedTo.GetComponent<SpringJoint>();

        //Init the line renderer we use to display the rope
        lineRenderer = GetComponent<LineRenderer>();

        Vector3 ropeStartPoint = Vector3.zero;
        segmentCount = startSegmentCount;
        for (int i = 0; i < segmentCount; i++)
        {
            ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y += ropeSegmentLength;
        }
        //Init the spring we use to approximate the rope from point a to b
        UpdateSpring();

        //Add the weight to what the rope is carrying
        loadMass = whatIsHangingFromTheRope.GetComponent<Rigidbody>().mass;
    }

    void Update()
    {
        //Display the rope with a line renderer
        DisplayRope();
    }

    private void FixedUpdate()
    {
        //Add more or less Rope
        UpdateWinch();

        Simulation();
    }

    private void InitRope()
    {
        float dist = ropeLength;

        int tempSegmentCount = (int)(dist * 2f) + startSegmentCount;

        if (tempSegmentCount > ropeSegments.Count)
        {
            Vector3 ropeStartPoint = ropeSegments[ropeSegments.Count - 1].posNow;
            segmentCount = tempSegmentCount;
            ropeStartPoint.y += ropeSegmentLength;
            ropeSegments.Add(new RopeSegment(ropeStartPoint));
        }
        else if (tempSegmentCount < ropeSegments.Count)
        {
            segmentCount = tempSegmentCount;
            ropeSegments.RemoveAt(ropeSegments.Count - 1);
        }
    }

    private void Simulation()
    {
        Vector3 forceGravity = new Vector3(0f, -1f, 0f);

        for (int i = 1; i < ropeSegments.Count; i++)
        {
            RopeSegment currentSegment = ropeSegments[i];
            Vector3 velocity = currentSegment.posNow - currentSegment.posOld;
            currentSegment.posOld = currentSegment.posNow;

            RaycastHit hit;
            if(Physics.Raycast(currentSegment.posNow, -Vector3.up, out hit, 0.1f))
            {
                if(hit.collider != null)
                {
                    velocity = Vector3.zero;
                    forceGravity.y = 0f;
                }
            }

            currentSegment.posNow += velocity;
            currentSegment.posNow += forceGravity * Time.fixedDeltaTime;
            ropeSegments[i] = currentSegment;
        }

        for (int i = 0;i < 20; i++)
        {
            ApplyConstraint();
        }
    }

    private void ApplyConstraint()
    {
        RopeSegment firstSegment = ropeSegments[0];
        firstSegment.posNow = whatTheRopeIsConnectedTo.position;
        ropeSegments[0] = firstSegment;

        RopeSegment endSegment = ropeSegments[ropeSegments.Count - 1];
        endSegment.posNow = whatIsHangingFromTheRope.position;
        ropeSegments[ropeSegments.Count - 1] = endSegment;

        for (int i = 0; i < ropeSegments.Count - 1; i++) 
        {
            RopeSegment firstSeg = ropeSegments[i];
            RopeSegment secondSeg = ropeSegments[i + 1];

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - ropeSegmentLength);
            Vector3 changeDir = Vector3.zero;

            if (dist > ropeSegmentLength)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            }
            else if (dist < ropeSegmentLength)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector3 changeAmount = changeDir * error;

            if(i != 0)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                ropeSegments[i+1] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                ropeSegments[i + 1] = secondSeg;
            }
        }
    }

    //Display the rope with a line renderer
    private void DisplayRope()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePosition = new Vector3[segmentCount];
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            ropePosition[i] = ropeSegments[i].posNow;
        }

        lineRenderer.positionCount = ropePosition.Length;
        lineRenderer.SetPositions(ropePosition);
    }


    //Update the spring constant and the length of the spring
    private void UpdateSpring()
    {
        //Someone said you could set this to infinity to avoid bounce, but it doesnt work
        //kRope = float.inf

        //
        //The mass of the rope
        //
        //Density of the wire (stainless steel) kg/m3
        float density = 7750f;
        //The radius of the wire
        float radius = 0.02f;

        float volume = Mathf.PI * radius * radius * ropeLength;

        float ropeMass = volume * density;

        //Add what the rope is carrying
        ropeMass += loadMass;


        //
        //The spring constant (has to recalculate if the rope length is changing)
        //
        //The force from the rope F = rope_mass * g, which is how much the top rope segment will carry
        float ropeForce = ropeMass * 9.81f;

        //Use the spring equation to calculate F = k * x should balance this force, 
        //where x is how much the top rope segment should stretch, such as 0.01m

        //Is about 146000
        //float kRope = ropeForce / 0.01f;
        float kRope = 1000f;

        //print(ropeMass);

        //Add the value to the spring
        springJoint.spring = kRope * 1.0f;
        springJoint.damper = kRope * 0.05f;

        //Update length of the rope
        springJoint.maxDistance = ropeLength;
    }

    //Add more/less rope
    private void UpdateWinch()
    {
        bool hasChangedRope = false;

        //More rope
        if (Input.GetKey(KeyCode.O) && ropeLength < maxRopeLength)
        {
            ropeLength += winchSpeed * Time.deltaTime;

            InitRope();

            whatIsHangingFromTheRope.gameObject.GetComponent<Rigidbody>().WakeUp();

            hasChangedRope = true;
        }
        else if (Input.GetKey(KeyCode.I) && ropeLength > minRopeLength)
        {
            ropeLength -= winchSpeed * Time.deltaTime;

            InitRope();

            whatIsHangingFromTheRope.gameObject.GetComponent<Rigidbody>().WakeUp();

            hasChangedRope = true;
        }


        if (hasChangedRope)
        {
            ropeLength = Mathf.Clamp(ropeLength, minRopeLength, maxRopeLength);

            //Need to recalculate the k-value because it depends on the length of the rope
            UpdateSpring();
        }
    }

    public struct RopeSegment
    {
        public Vector3 posNow;
        public Vector3 posOld;

        public RopeSegment(Vector3 pos)
        {
            posNow = pos;
            posOld = pos;
        }
    }

}

