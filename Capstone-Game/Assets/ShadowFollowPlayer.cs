using UnityEngine;

public class ShadowFollowPlayer : MonoBehaviour
{
    private Vector3 initialLocalPosition;
    public Transform targetPosition;

    private void Start()
    {
        initialLocalPosition = gameObject.transform.localPosition;
    }

    private void LateUpdate()
    {
        gameObject.transform.position = targetPosition.position + initialLocalPosition;
    }
}
