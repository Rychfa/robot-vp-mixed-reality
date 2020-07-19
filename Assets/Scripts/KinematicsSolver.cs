using UnityEngine;

public class KinematicsSolver : MonoBehaviour
{
    public RobotJoint[] Joints;
    private static float DistanceThreshold = 0.005f;
    private static float LearningRate = 1.0f;
    private static float SamplingDistance = 0.3f;
    private static int maxSteps = 50000;

    public Vector3 ForwardKinematics(float[] angles)
    {
        Vector3 prevPoint = Joints[0].transform.position;
        Quaternion rotation = Quaternion.identity;
        for (int i = 1; i < Joints.Length; i++)
        {
            // Rotates around a new axis
            var angleAxis = Quaternion.AngleAxis(angles[i - 1], Joints[i - 1].Axis);
            rotation *= angleAxis;
            var jointPosition = rotation * Joints[i].StartOffset;
            Vector3 nextPoint = prevPoint + jointPosition;

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

        while (step < maxSteps && !(DistanceFromTarget(target, angles) < DistanceThreshold))
        {
            for (int i = Joints.Length - 3; i >= 0; i--)
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
    
    public static void InverseKinematics(KinematicProblem problem)
    {
        Debug.Log("------  Begin IK  ------");
        Debug.Log("Original distance: " + DistanceFromTarget(problem));
        Debug.Log("------------------------");

        var step = 0;
        // var maxSteps = 50000;

        while (step < maxSteps && !(DistanceFromTarget(problem) < DistanceThreshold))
        {
            for (int i = problem.angles.Length - 3; i >= 0; i--)
            {
                // Gradient descent
                // Update : Solution -= LearningRate * Gradient
                float gradient = PartialGradient(problem, i);
                problem.angles[i] -= LearningRate * gradient;

                // Early termination
                if (DistanceFromTarget(problem) < DistanceThreshold)
                {
                    Debug.Log("------ Early End IK  ------");
                    Debug.Log("Final distance (success true): " + DistanceFromTarget(problem).ToString("F4"));
                    return;
                }
            }

            step++;
        }
        
        var finalDistance = DistanceFromTarget(problem);

        Debug.Log("------  End IK  ------");
        Debug.Log("Final distance (success " + (finalDistance < DistanceThreshold) + "): " + finalDistance.ToString("F4"));
    }
    
    public static float PartialGradient(KinematicProblem problem, int i)
    {
        // Saves the angle,
        // it will be restored later
        float angle = problem.angles[i];

        // Gradient : [F(x+SamplingDistance) - F(x)] / h
        float f_x = DistanceFromTarget(problem);

        problem.angles[i] += SamplingDistance;
        float f_x_plus_d = DistanceFromTarget(problem);

        float gradient = (f_x_plus_d - f_x) / SamplingDistance;

        // Restores
        problem.angles[i] = angle;

        return gradient;
    }
    
    public static float DistanceFromTarget(KinematicProblem problem)
    {
        Vector3 point = ForwardKinematics(problem);
        return Vector3.Distance(point, problem.targetPosition);
    }
    
        
    public static Vector3 ForwardKinematics(KinematicProblem problem)
    {
        Vector3 prevPoint = problem.firstJointPosition;
        Quaternion rotation = Quaternion.identity;
        for (int i = 1; i < problem.angles.Length; i++)
        {
            // Rotates around a new axis
            rotation *= Quaternion.AngleAxis(problem.angles[i - 1], problem.jointAxis[i - 1]);
            Vector3 nextPoint = prevPoint + rotation * problem.jointOffsets[i];

            prevPoint = nextPoint;
        }

        return prevPoint;
    }
    
}