using UnityEngine;

public class RodMover : MonoBehaviour
{
    //Serialize Params
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private Transform left, right, up, down;


    //Cached Comps

    //State
    private Transform originalTransform;
}
