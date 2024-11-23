using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControl : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            var cam = Camera.main;
            var mousePos = Mouse.current.position;
            var ray = cam.ScreenPointToRay(mousePos.value);
            SimulationManager.Instance.ShootBall(ray);
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            SimulationManager.Instance.Reset();
        }
    }
}
