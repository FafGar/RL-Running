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
    double [] inputs;


    int xAxisOkay(){
        if(transform.rotation.eulerAngles.x > -15f && 
            transform.rotation.eulerAngles.x < 15f) return 2;
        return -3;
    }
    int zAxisOkay(){
        if(transform.rotation.eulerAngles.z > -15f && 
            transform.rotation.eulerAngles.z < 15f) return 0;
        return -9;
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
        
        if(oldDist - dist  > -0.0002 && oldDist - dist < 0.0002) {
            Debug.Log("stopped");
            // Debug.Log(moveTimer);
            moveTimer += Time.deltaTime;
            return -8; // mild penalty for staying still
        }else if(oldDist - dist > 0.0002) {
            moveTimer = 0;
            return 5; //Reward on move forward
        }
        return -12; //BIG penalty for moving backwards
    }


    
    void Start()
    {
        leftState = wheelstate.OFF;
        rightState = wheelstate.OFF;
        moveTimer = 0;
        body=GetComponent<Rigidbody>();
        startPos = transform.position;

        this.RLClient = new NeuralNet();
        this.RLClient.init(6,7,12, 0.9d , 2d);
        this.inputs = new double[6];
        this.inputs[0] = 0;

    }

    // Update is called once per frame
    void Update()
    {
        this.inputs[1] = (double)transform.rotation.x;
        this.inputs[2] = transform.rotation.z;
        this.inputs[3] = transform.position.x-target.transform.position.x;
        this.inputs[4] = transform.position.z-target.transform.position.z;
        

        int RLOutput = RLClient.evaluate(inputs);
        int reward = 0;
        // if(this.inputs[0] == RLOutput && RLOutput != 6){
        //     reward -= 5;
        // } else reward+=4;
        this.inputs[0] = RLOutput;
        Debug.Log(RLOutput);
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
        // switch (RLOutput)
        // {
        //     case 0:
        //         leftState=wheelstate.FORWARD;
        //         break;
        //     case 1:
        //         leftState=wheelstate.BACKWARD;
        //         break;
        //     case 2:
        //         leftState=wheelstate.FORWARD;
        //         break;
        //     case 3:
        //         leftState=wheelstate.BACKWARD;
        //         break;
        //     case 4:
        //         break;
        // }
        
        rightWheel.motorTorque = (int)rightState;
        leftWheel.motorTorque = (int)leftState;
        body.AddForce(transform.up*40);
        Debug.Log(rightState);



        if(transform.position.y < 0 || moveTimer > 3.0){
            Debug.Log("Resetting");
            transform.position = new Vector3(0,0.9f,0);
            transform.rotation = Quaternion.identity;
            oldPos = new Vector3(0,0,1);
            moveTimer = 0;
        } 
        else{
            this.inputs[1] = (double)transform.rotation.x;
            this.inputs[2] = transform.rotation.z;
            this.inputs[3] = transform.position.x-target.transform.position.x;
            this.inputs[4] = transform.position.z-target.transform.position.z;
            reward = xAxisOkay();
            reward += zAxisOkay();
            reward += directionCheckLeft();
            reward += directionCheckRight();
            reward += distanceCheck();
            Debug.Log("Reward " + reward);
            RLClient.train(reward, this.inputs);
        }


    }
}
