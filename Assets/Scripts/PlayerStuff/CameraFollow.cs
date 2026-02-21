using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;
    private float smothTime = 0.25f;
    //Defines the distance the camera is from the x,y plane
    private Vector3 cameraOfSet = new Vector3(0,0,-10);
    private Vector3 desiredVelocity = Vector3.zero;
    private GameObject cameraObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraObject = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        //without this the camera just moves into the player and you cant see anything
        Vector3 targetLocation = player.transform.position+cameraOfSet;
        //smothly move the camera to the player
        cameraObject.transform.position = Vector3.SmoothDamp(cameraObject.transform.position,targetLocation,ref desiredVelocity,smothTime);
    }

}
