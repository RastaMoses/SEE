using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Serialize Params
    [SerializeField] GameObject[] screens;
    [SerializeField] int startCamera = 0;
    [SerializeField] CinemachineCamera fPSCam;
    //Cached Comps
    CinemachineBrain cMBrain;

    //State
    int currentCamera;
    int nextScreen;

    private void Start()
    {
        //Connect Events
        EventManager.OnEnterFishing += EnterFishingCam;
        EventManager.OnExitFishing += ExitFishingCam;

        //Cache Comps
        cMBrain = GetComponent<CinemachineBrain>();

        //Initializing
        currentCamera = startCamera;
        ChangeCamera(currentCamera);
    }


    public void ChangeCamera(int nextCamera)
    {
        screens[currentCamera].SetActive(false);
        screens[nextCamera].SetActive(true);
        currentCamera = nextCamera;
    }

    public void EnterFishingCam()
    {
        fPSCam.Priority = 2;
    }

    public void ExitFishingCam() 
    {
        fPSCam.Priority = 0;
    }
    

}
