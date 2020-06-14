using UnityEngine;

public class KinematicsSolver : MonoBehaviour
{
    public RobotJoint[] Joints;
    private float DistanceThreshold = 0.05f;
    private float LearningRate = 1.5f;
    private float SamplingDistance = 0.1f;

    public Vector3 ForwardKinematics(float[] angles)
    {
        Vector3 prevPoint = Joints[0].transform.position;
        Quaternion rotation = Quaternion.identity;
        for (int i = 1; i < Joints.Length; i++)
        {
            // Rotates around a new axis
            rotation *= Quaternion.AngleAxis(angles[i - 1], Joints[i - 1].Axis);
            Vector3 nextPoint = prevPoint + rotation * Joints[i].StartOffset;

            prevPoint = nextPoint;
        }

        return prevPoint;
    }

    public float DistanceFromTarget(Vector3 target, float[] angles)
    {
        Vector3 point = ForwardKinematics(angles);
        return Vector3.Distance(point, target);
    }

    public float PartialGradient(Vector3 target, float[] angles, int i)
    {
        // Saves the angle,
        // it will be restored later
        float angle = angles[i];

        // Gradient : [F(x+SamplingDistance) - F(x)] / h
        float f_x = DistanceFromTarget(target, angles);

        angles[i] += SamplingDistance;
        float f_x_plus_d = DistanceFromTarget(target, angles);

        float gradient = (f_x_plus_d - f_x) / SamplingDistance;

        // Restores
        angles[i] = angle;

        return gradient;
    }


    public void InverseKinematics(Vector3 target, float[] angles)
    {
        Debug.Log("------  Begin IK  ------");
        Debug.Log("Original distance: " + DistanceFromTarget(target, angles));
        Debug.Log("------------------------");

        var step = 0;
        var maxSteps = 50000;

        while (step < maxSteps && !(DistanceFromTarget(target, angles) < DistanceThreshold))
        {
            for (int i = Joints.Length - 1; i >= 0; i--)
            {
                // Gradient descent
                // Update : Solution -= LearningRate * Gradient
                float gradient = PartialGradient(target, angles, i);
                angles[i] -= LearningRate * gradient;

                // Early termination
                if (DistanceFromTarget(target, angles) < DistanceThreshold)
                {
                    Debug.Log("------ Early End IK  ------");
                    Debug.Log("Final distance (success true): " + DistanceFromTarget(target, angles).ToString("F4"));
                    return;
                }
            }

            step++;
        }
        
        var finalDistance = DistanceFromTarget(target, angles);

        Debug.Log("------  End IK  ------");
        Debug.Log("Final distance (success " + (finalDistance < DistanceThreshold) + "): " + finalDistance.ToString("F4"));

        // }
    }
}