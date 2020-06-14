using UnityEngine;

public class CameraManager:MonoBehaviour {
    public float speed = 3.5f;
    private float X;
    private float Y;

    void Update() {
        var fov = Camera.main.fieldOfView;
        fov -= Input.GetAxis("Mouse ScrollWheel") * 10f;
        fov = Mathf.Clamp(fov, 10, 100);
        Camera.main.fieldOfView = fov;
        
        if(Input.GetMouseButton(0)) {
            transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * speed, -Input.GetAxis("Mouse X") * speed, 0));
            X = transform.rotation.eulerAngles.x;
            Y = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(X, Y, 0);
        }
    }
}