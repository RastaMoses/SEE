using Unity.Cinemachine;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    //Serialize Params
    [SerializeField] int newScreenInt;

    private void OnTriggerEnter(Collider other)
    {
        FindFirstObjectByType<CameraController>().ChangeCamera(newScreenInt);
    }
}
