using UnityEngine;

public class KinematicProblem
{
    public Vector3 firstJointPosition { get; private set; } // The position of the base of the robot
    public Vector3[] jointOffsets{ get; private set; } // The position of the joint relative to the parent
    public Vector3[] jointAxis{ get; private set; }
    
    public Vector3 targetPosition{ get; private set; }
    public float[] angles{ get; set; }

    public KinematicProblem(Vector3 firstJointPosition, Vector3[] jointOffsets, Vector3[] jointAxis, Vector3 targetPosition, float[] angles)
    {
        this.firstJointPosition = firstJointPosition;
        this.jointOffsets = jointOffsets;
        this.jointAxis = jointAxis;
        this.targetPosition = targetPosition;
        this.angles = angles;
    }

    public static KinematicProblem empty()
    {
        var first = Vector3.zero;
        var offset = new Vector3[2] { Vector3.zero, Vector3.zero};
        var axis = new Vector3[2] { Vector3.forward, Vector3.up};
        var targetPosition = new Vector3(0.5f, 0.5f, 0.5f);
        var angles = new float[2] { 0f, 0f};
        return new KinematicProblem(first, offset, axis, targetPosition, angles);
    }
    
}