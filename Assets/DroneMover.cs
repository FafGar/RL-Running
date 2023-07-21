using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMover : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject target;

    public WheelCollider rightWheel;
    public WheelCollider leftWheel;


    enum wheelstate:int{FORWARD=1,BACKWARD=-1,OFF=0}
    wheelstate rightState;
    wheelstate leftState;
    Vector3 oldPos;

    Rigidbody body;


    bool xAxisOkay(){
        if(transform.rotation.eulerAngles.x > -15f && 
            transform.rotation.eulerAngles.x < 15f) return true;
        return false;
    }
    bool zAxisOkay(){
        if(transform.rotation.eulerAngles.z > -15f && 
            transform.rotation.eulerAngles.z < 15f) return true;
        return false;
    }

    int directionCheckRight(){
        Vector3 targetLine3 = target.transform.position - transform.position;
        Vector2 targetLine = new Vector2(targetLine3.x, targetLine3.z);
        targetLine.Normalize();
        Vector2 sightLine = new Vector2(transform.forward.x, transform.forward.z);
        sightLine.Normalize();
        if(sightLine.x - targetLine.x < 0.3f) return 0;
        else if (sightLine.x - targetLine.x < -0.3f) return -1; //negative score if need to turn right
        return 2; //reward if in the right area
    }

    int directionCheckLeft(){
        Vector3 targetLine3 = target.transform.position - transform.position;
        Vector2 targetLine = new Vector2(targetLine3.x, targetLine3.z);
        targetLine.Normalize();
        Vector2 sightLine = new Vector2(transform.forward.x, transform.forward.z);
        sightLine.Normalize();
        if(sightLine.x - targetLine.x < 0.3f) return -1; //negative score if need to turn left
        else if (sightLine.x - targetLine.x < -0.3f) return 0; 
        return 2; //reward if in the right area
    }

    int distanceCheck(){
        float dist = (transform.position-target.transform.position).magnitude;
        float oldDist = (oldPos-target.transform.position).magnitude;
        if(oldDist - dist > 0.05) return 1; //Reward on move forward
        if(oldDist - dist > 0 && oldDist - dist < 0.05) return -5; // mild penalty for staying still
        return -10; //BIG penalty for moving backwards
    }


    
    void Start()
    {
        leftState = wheelstate.OFF;
        rightState = wheelstate.OFF;
        body=GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.U)){
            if(leftState==wheelstate.FORWARD) leftState=wheelstate.OFF;
            else leftState=wheelstate.FORWARD;
        }
        if(Input.GetKey(KeyCode.J)){
            if(leftState==wheelstate.BACKWARD) leftState=wheelstate.OFF;
            else leftState=wheelstate.BACKWARD;
        }
        if(Input.GetKey(KeyCode.I)){
            if(rightState==wheelstate.FORWARD) rightState=wheelstate.OFF;
            else rightState=wheelstate.FORWARD;
        }
        if(Input.GetKey(KeyCode.K)){
            if(rightState==wheelstate.BACKWARD) rightState=wheelstate.OFF;
            else rightState=wheelstate.BACKWARD;
        }
        
        rightWheel.motorTorque = (int)rightState;
        leftWheel.motorTorque = (int)leftState;
        body.AddForce(transform.up);
    }
}
