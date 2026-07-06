using UnityEngine;
using UnityEngine.InputSystem;
using Shift25.Gameplay;
using Shift25.Managers;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private float rayDistance = 2.5f;
    [SerializeField] private LayerMask interactableLayer;
    private Camera _mainCam;
    private IInteractable _currentInteractable;

    private void Awake() => _mainCam = Camera.main;

    // [New Input System] ปุ่ม E
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started && _currentInteractable != null)
        {
            // [Audio Integration] Play generic interaction sound
            if (AudioManager.Instance != null) AudioManager.Instance.PlayInteractSFX();

            _currentInteractable.Interact();
        }
    }

    // [New Input System] ปุ่มคลิกซ้าย (Fire) - แก้ไขตามรูปที่คุณส่งมา
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // [Logic] ยิง Raycast เพื่อดูว่าคลิกโดนสินค้าที่กำลังสแกนอยู่ไหม
            Ray ray = (Cursor.lockState == CursorLockMode.Locked)
                ? _mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0))
                : _mainCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableLayer))
            {
                // [Syntax Decoding] ลองหา ScannableItem จากสิ่งที่คลิกโดน
                if (hit.collider.TryGetComponent<ScannableItem>(out var item))
                {
                    item.OnClickAction();
                }
            }
        }
    }

    private void Update() => CheckForInteractable();

    private void CheckForInteractable()
    {
        var ray = _mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableLayer))
        {
            _currentInteractable = hit.collider.GetComponent<IInteractable>();
        }
        else _currentInteractable = null;
    }
}