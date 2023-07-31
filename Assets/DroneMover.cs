using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMover : MonoBehaviour
{
    public GameObject target;

    public WheelCollider rightWheel;
    public WheelCollider leftWheel;


    enum wheelstate:int{FORWARD=100,BACKWARD=-100,OFF=0}
    wheelstate rightState;
    wheelstate leftState;
    Vector3 oldPos;
    Vector3 startPos;
    float moveTimer;

    Rigidbody body;

    NeuralNet RLClient;
    float [] inputs;


    int xAxisOkay(){
        if(transform.rotation.eulerAngles.x > -15f && 
            transform.rotation.eulerAngles.x < 15f) return 2;
        return -3;
    }
    int zAxisOkay(){
        if(transform.rotation.eulerAngles.z > -15f && 
            transform.rotation.eulerAngles.z < 15f) return 2;
        return -3;
    }

    int directionCheckRight(){
        Vector3 targetLine3 = target.transform.position - transform.position;
        Vector2 targetLine = new Vector2(targetLine3.x, targetLine3.z);
        targetLine.Normalize();
        Vector2 sightLine = new Vector2(transform.forward.x, transform.forward.z);
        sightLine.Normalize();
        if(sightLine.x - targetLine.x < 0.3f) return 0;
        else if (sightLine.x - targetLine.x < -0.3f) return -1; //negative score if need to turn right
        return 3; //reward if in the right area
    }

    int directionCheckLeft(){
        Vector3 targetLine3 = target.transform.position - transform.position;
        Vector2 targetLine = new Vector2(targetLine3.x, targetLine3.z);
        targetLine.Normalize();
        Vector2 sightLine = new Vector2(transform.forward.x, transform.forward.z);
        sightLine.Normalize();
        if(sightLine.x - targetLine.x < 0.3f) return -1; //negative score if need to turn left
        else if (sightLine.x - targetLine.x < -0.3f) return 0; 
        return 3; //reward if in the right area
    }

    int distanceCheck(){
        float dist = (transform.position-target.transform.position).magnitude;
        float oldDist = (oldPos-target.transform.position).magnitude;
        oldPos = transform.position;
        
        if(oldDist - dist  > -0.0005 && oldDist - dist < 0.0005) {
            // Debug.Log(moveTimer);
            moveTimer += Time.deltaTime;
            return -3; // mild penalty for staying still
        }else if(oldDist - dist > 0.0005) {
            moveTimer = 0;
            return 5; //Reward on move forward
        }
        return -5; //BIG penalty for moving backwards
    }


    
    void Start()
    {
        leftState = wheelstate.OFF;
        rightState = wheelstate.OFF;
        moveTimer = 0;
        body=GetComponent<Rigidbody>();
        startPos = transform.position;

        this.RLClient = new NeuralNet();
        this.RLClient.init(5,6,12);
        this.inputs = new float[5];
        this.inputs[4] = 0;

    }

    // Update is called once per frame
    void Update()
    {
        this.inputs[0] = transform.rotation.x;
        this.inputs[1] = transform.rotation.z;
        this.inputs[2] = transform.position.x-target.transform.position.x;
        this.inputs[3] = transform.position.z-target.transform.position.z;
        

        int RLOutput = RLClient.evaluate(inputs);
        print(RLOutput);
        int reward = 0;
        if(this.inputs[4] == RLOutput && RLOutput !=6){
            reward -= 5;
        } else reward+=4;
        this.inputs[4] = RLOutput;
        switch (RLOutput)
        {
            case 0:
                leftState=wheelstate.OFF;
                break;
            case 1:
                leftState=wheelstate.FORWARD;
                break;
            case 2:
                leftState=wheelstate.BACKWARD;
                break;
            case 3:
                leftState=wheelstate.OFF;
                break;
            case 4:
                leftState=wheelstate.FORWARD;
                break;
            case 5:
                leftState=wheelstate.BACKWARD;
                break;
            case 6:
                break;
        }
        // if(Input.GetKey(KeyCode.U)){
        //     if(leftState==wheelstate.FORWARD) leftState=wheelstate.OFF;
        //     else leftState=wheelstate.FORWARD;
        // }
        // if(Input.GetKey(KeyCode.J)){
        //     if(leftState==wheelstate.BACKWARD) leftState=wheelstate.OFF;
        //     else leftState=wheelstate.BACKWARD;
        // }
        // if(Input.GetKey(KeyCode.I)){
        //     if(rightState==wheelstate.FORWARD) rightState=wheelstate.OFF;
        //     else rightState=wheelstate.FORWARD;
        // }
        // if(Input.GetKey(KeyCode.K)){
        //     if(rightState==wheelstate.BACKWARD) rightState=wheelstate.OFF;
        //     else rightState=wheelstate.BACKWARD;
        // }
        
        rightWheel.motorTorque = (int)rightState;
        leftWheel.motorTorque = (int)leftState;
        // if(leftState != 0){/
        //     leftWheel.r
        // }
        body.AddForce(transform.up*40);
        Debug.Log(rightState);



        if(transform.position.y < 0 || moveTimer > 3.0){
            Debug.Log("Resetting");
            RLClient.train((int)(27-Mathf.Abs((target.transform.position-transform.position).magnitude)), 0.3);
            transform.position = new Vector3(0,0.9f,0);
            transform.rotation = Quaternion.identity;
            moveTimer = 0;
        }else if(Mathf.Abs(transform.rotation.x) > 70f || Mathf.Abs(transform.rotation.z) > 70f){
            //Do Nothing
        } 
         else{
            reward = xAxisOkay();
            reward += zAxisOkay();
            reward += directionCheckLeft();
            reward += directionCheckRight();
            reward += distanceCheck();
            RLClient.train(reward, 0.1);
        }


    }
}
