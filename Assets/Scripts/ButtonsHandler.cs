using UnityEngine;

public class ButtonsHandler : MonoBehaviour
{
    public void ClearRobot()
    {
        if (PlacementController.spawnedRobot != null)
        {
            Destroy(PlacementController.spawnedRobot);
        }
    }

    public void SetMode1()
    {
        PlacementController.mode1 = true;
    }

    public void SetMode2()
    {
        PlacementController.mode1 = false;
    }

    public void setPositiveRotation()
    {
        PlacementController.sign = 1.0f;
    }
    public void setNegativeRotation()
    {
        PlacementController.sign = -1.0f;
    }
}