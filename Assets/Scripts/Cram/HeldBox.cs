using UnityEngine;
using Shift25.Managers;

namespace Shift25.Gameplay
{
    public class HeldBox : MonoBehaviour
    {
        private float _originalSpeed;
        private PlayerController _player;

        public void Initialize(float weightMult)
        {
            _player = FindObjectOfType<PlayerController>();
            if (_player != null)
            {
                _originalSpeed = _player.moveSpeed;
                _player.moveSpeed *= weightMult;
            }

            // [Parenting] Attach to Main Camera
            transform.SetParent(Camera.main.transform);
            
            // [Positioning] Adjusted for a large scale (100). 
            // If scale is 100, position values usually need to be very different.
            // Try (0, -0.5, 1) first.
            transform.localPosition = new Vector3(0f, -0.5f, 1.5f);
            transform.localRotation = Quaternion.identity;
            
            // [Physics] Disable all physics to prevent jittering
            if (TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
            if (TryGetComponent<Collider>(out var col)) col.enabled = false;
        }

        public void ConsumeBox()
        {
            if (_player != null) _player.moveSpeed = _originalSpeed;
            Destroy(gameObject); // [Cleanup] Remove from hierarchy
        }
    }
}