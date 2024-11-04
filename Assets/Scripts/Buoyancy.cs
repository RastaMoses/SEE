using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Buoyancy : MonoBehaviour
{
    [SerializeField] private List<Floaters> floaters = new List<Floaters>();
    [SerializeField] public float waterLine = 0f;
    [SerializeField] private float underWaterDrag = 3f;
    [SerializeField] private float underWaterAngularDrag = 1f;
    [SerializeField] private float defaultDrag = 0f;
    [SerializeField] private float defaultAngularDrag = 0.05f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        bool isUnderWater = false;

        for (int i = 0; i < floaters.Count; i++)
        {
            if (floaters[i].FloaterUpdate(rb, waterLine))
            {
                isUnderWater = true;
            }
        }

        SetState(isUnderWater);
    }

    private void SetState(bool isUnderWater)
    {
        if (isUnderWater)
        {
            rb.linearDamping = underWaterDrag;
            rb.angularDamping = underWaterAngularDrag;

        }
        else
        {
            rb.linearDamping = defaultDrag;
            rb.angularDamping = defaultAngularDrag;
        }
    }
}


[System.Serializable]
public class Floaters
{
    //Serialize Params
    [SerializeField] private float floatingPower = 20f;
    [SerializeField] private Transform floater;


    //State
    private bool underWater;

    public bool FloaterUpdate(Rigidbody rb, float waterLine)
    {
        float difference = floater.position.y - waterLine;
        if (difference < 0)
        {
            rb.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(difference), floater.position, ForceMode.Force);
            if (!underWater)
            {
                underWater = true;
            }
        }
        else if (underWater)
        {
            underWater = false;
        }
        return underWater;
    }
}
