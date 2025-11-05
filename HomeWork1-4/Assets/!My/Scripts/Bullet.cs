using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private GameObject _hitEffect;
    private float _timeLiveBullet = 5f;
    private int _damage;

    public void Init(Vector3 transformPosition, Vector3 velocity, int damage = 1)
    {
        _damage = damage;
        transform.position = transformPosition;
        transform.rotation = Quaternion.LookRotation(velocity);
        gameObject.SetActive(true);
        _rb.AddForce(velocity, ForceMode.VelocityChange);
        Destroy(gameObject, _timeLiveBullet);
    }
    

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
        
        _hitEffect.transform.parent = null;
        _hitEffect.transform.position = collision.contacts[0].point;
        _hitEffect.SetActive(true);

        if(collision.collider.TryGetComponent<LimbPart> (out LimbPart limbPart))
        {
            limbPart.TakeDamager(_damage);
        }
    }
}
