using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct state{
    List<Quaternion> jointQuat;
    List<Vector3> jointPos;
    float distanceToTarget;

}

public class JointMover : MonoBehaviour
{
    public ConfigurableJoint head;
    public ConfigurableJoint spine;
    public ConfigurableJoint pelvis;
    public ConfigurableJoint rElbow;
    public ConfigurableJoint lElbow;
    public ConfigurableJoint rShoulder;
    public ConfigurableJoint lShoulder;
    public ConfigurableJoint rWrist;
    public ConfigurableJoint lWrist;
    public ConfigurableJoint rHip;
    public ConfigurableJoint lHip;
    public ConfigurableJoint rKnee;
    public ConfigurableJoint lKnee;
    public ConfigurableJoint rAnkle;
    public ConfigurableJoint lAnkle;
    
    List<ConfigurableJoint> joints;

    

    // Start is called before the first frame update
    void Start()
    {
        joints = new List<ConfigurableJoint>{ head,spine,pelvis,rElbow,lElbow,rShoulder,lShoulder,rWrist,lWrist,rHip,lHip,rKnee,lKnee,rAnkle,lAnkle};

        // foreach(ConfigurableJoint joint in joints){
        //     joint.transform.rotation = Quaternion.identity;
        // }
        
    }

    // Update is called once per frame
    void Update()
    {
            rShoulder.targetRotation = Quaternion.Euler(0,10f,10f);
        // }
    }
}
