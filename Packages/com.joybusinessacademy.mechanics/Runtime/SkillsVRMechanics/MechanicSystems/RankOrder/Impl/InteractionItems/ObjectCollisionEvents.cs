using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectCollisionEvents : MonoBehaviour
{
    [System.Serializable]
    public class UnityEventCollider : UnityEvent<Collider> { }

    public UnityEventCollider OnColliderEnter = new UnityEventCollider();
    public UnityEventCollider OnColliderStay = new UnityEventCollider();
    public UnityEventCollider OnColliderExit = new UnityEventCollider();

    [System.Serializable]
    public class UnityEventCollider2D : UnityEvent<Collider2D> { }
    public UnityEventCollider2D OnCollider2DEnter = new UnityEventCollider2D();
    public UnityEventCollider2D OnCollider2DStay = new UnityEventCollider2D();
    public UnityEventCollider2D OnCollider2DExit = new UnityEventCollider2D();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnCollider2DEnter?.Invoke(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        OnCollider2DStay?.Invoke(collision.collider);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        OnCollider2DExit?.Invoke(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnCollider2DEnter?.Invoke(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        OnCollider2DStay?.Invoke(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        OnCollider2DExit?.Invoke(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        OnColliderEnter?.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        OnColliderStay?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        OnColliderExit?.Invoke(other);
    }


    private void OnCollisionEnter(Collision other)
    {
        OnColliderEnter?.Invoke(other.collider);
    }

    private void OnCollisionStay(Collision other)
    {
        OnColliderStay?.Invoke(other.collider);
    }

    private void OnCollisionExit(Collision other)
    {
        OnColliderExit?.Invoke(other.collider);
    }
}
