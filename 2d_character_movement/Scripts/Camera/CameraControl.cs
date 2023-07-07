/*Created by Pawe³ Mularczyk*/

using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private bool debug = false;
    [Space]
    [Header("TRACKING SETTINGS")]
    [SerializeField] private Transform _followTarget;
    [SerializeField] private Vector2 _moveDeadzone = new Vector2(7 , 4);
    [SerializeField] private float _cameraSpeed = 10f;

    private Vector2 _cameraLookPoint;

    private bool _startFollow = false;

    private float _refPosX;
    private float _refPosY;
    private float calculateCamSpeed;

    private void Start()
    {
        _cameraLookPoint = new Vector2(transform.position.x, transform.position.y);
    }

    private void FixedUpdate()
    {
        CameraMove();
    }

    private void CameraMove()
    {
        _cameraLookPoint = new Vector2(transform.position.x, transform.position.y);

        float distanceX = Mathf.Abs(_cameraLookPoint.x - _followTarget.position.x);
        float distanceY = Mathf.Abs(_cameraLookPoint.y - _followTarget.position.y);

        

        if(_followTarget.gameObject.GetComponent<Rigidbody2D>().velocity.y != -30)
        {
            calculateCamSpeed = _cameraSpeed;
        }
        else
        {
            calculateCamSpeed = Mathf.Lerp(calculateCamSpeed, 3.8f, 2f * Time.fixedDeltaTime);
        }

        if (distanceX > _moveDeadzone.x / 2 || distanceY > _moveDeadzone.y / 2)
            _startFollow = true;

        if (distanceX < 0.2f && distanceY < 0.2f)
            _startFollow = false;

        if (_startFollow)
        {
            float newX = Mathf.SmoothDamp(_cameraLookPoint.x, _followTarget.position.x, ref _refPosX, calculateCamSpeed * Time.fixedDeltaTime);
            float newY = Mathf.SmoothDamp(_cameraLookPoint.y, _followTarget.position.y, ref _refPosY, calculateCamSpeed * Time.fixedDeltaTime);

            transform.position = new Vector3(newX, newY, -20);
        } 
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (debug)
        {
            //Target move deadzone
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_followTarget.position, _moveDeadzone);

            //Camera center

            if (!_startFollow)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_cameraLookPoint, 0.2f);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_cameraLookPoint, 0.2f);
            }
        }
    }
#endif
}
