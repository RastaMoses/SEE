using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class FirstPersonCamera : MonoBehaviour
{
    //Serialize Params
    [SerializeField] float lookSensitivity = 0.05f;
    [SerializeField] float maxYRotation = 90f;
    [SerializeField] bool invertY = false;
    [SerializeField] [Range(-180,180)] float minXRotation = -90f;
    [SerializeField][Range(-180, 180)] float maxXRotation = 90f;

    [SerializeField] bool controller = true;

    //Cached Comps

    //State
    bool active = false;
    public Vector2 cameraRotation = Vector2.zero;
    Vector2 input;

    private void Start()
    {
        //Connect Events
        EventManager.OnEnterFishing += ActivateCamera;
        EventManager.OnExitFishing += DeactivateCamera;
    }

    public void Look(InputValue value)
    {
        if (!active) {return; }
        input = value.Get<Vector2>();
    }

    private void Update()
    {
        if (!active) { return; }
        if (input == null) {return; }
        cameraRotation.x += input.x * lookSensitivity;
        //Invert Y
        if (!invertY) { cameraRotation.y -= input.y * lookSensitivity; }
        else { cameraRotation.y += input.y * lookSensitivity; }

        //Clamp Max Rotation
        cameraRotation.y = Mathf.Clamp(cameraRotation.y, -maxYRotation, maxYRotation);
        cameraRotation.x = Mathf.Clamp(cameraRotation.x, minXRotation, maxXRotation);

        //Rotate Character
        transform.localRotation = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0);
    }

    private void ActivateCamera()
    {
        cameraRotation = Vector2.zero;
        transform.localRotation = Quaternion.Euler(cameraRotation.y, cameraRotation.x, 0);
        active = true;
    }

    private void DeactivateCamera()
    {
        active = false;
    }
}
