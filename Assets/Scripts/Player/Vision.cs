using UnityEngine;

public class Vision : MonoBehaviour
{
    //Serialize Prarams
    [SerializeField] float detectRange = 10;
    [SerializeField] float detectAngle = 45;

    //Cached Comps
    MeshRenderer meshRenderer;
    FirstPersonCamera player;

    //State
    public bool isInAngle, isInRange, isHidden;


    private void Start()
    {
        //Cache
        player = FindAnyObjectByType<FirstPersonCamera>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        isInAngle = false;
        isInRange = false;
        isHidden = true;

        if (Vector3.Distance(transform.position, player.transform.position) < detectRange)
        {
            isInRange = true;
        }
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (player.transform.position - transform.position), out hit))
        {
            if (hit.transform == player.transform)
            {
                isHidden = false;
            }
        }

        //Calc Player Vision Angle
        Vector3 side1 = transform.position - player.transform.position;
        Vector3 side2 = player.transform.forward;
        float angle = Vector3.SignedAngle(side1,side2, Vector3.up);
        if (angle < detectAngle && angle > -detectAngle)
        { 
            isInAngle = true;
        }

        if (isInAngle && isInRange && !isHidden)
        {
            meshRenderer.enabled = true;
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }

}
