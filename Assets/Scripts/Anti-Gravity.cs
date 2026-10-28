using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityController3D : MonoBehaviour
{
    private Rigidbody rb;

    public bool isGravityReversed = true;
    public float gravityForce = 9.81f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        UpdateGravityState();
    }

    void FixedUpdate()
    {
        if (isGravityReversed)
        {
            rb.AddForce(Vector3.up * gravityForce, ForceMode.Acceleration);
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("ResetGravity"))
    //    {
    //        isGravityReversed = false;
    //        UpdateGravityState();
    //    }
    //}

    private void UpdateGravityState()
    {
        if (isGravityReversed)
        {
            rb.useGravity = false;
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 180f);
        }
        else
        {
            rb.useGravity = true;
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0f);
        }
    }
}