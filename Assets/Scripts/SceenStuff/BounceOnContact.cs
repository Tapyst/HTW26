using UnityEditor.Callbacks;
using UnityEngine;

public class BounceOnContact : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float maxKickStrength;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        rb.gravityScale=1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        //apply a random force to the debris to make it look like the player is kicking something
        if(collision.gameObject.tag =="Player")
        {
        rb.AddForce(new Vector2(maxKickStrength*Random.Range(0.5f,1f)*(Random.value < 0.5f ? 1 : -1),maxKickStrength*Random.Range(-1f,1f)*(Random.value < 0.5f ? 1 : -1)),ForceMode2D.Impulse);
        rb.AddTorque(maxKickStrength*Random.Range(0,1f));
        }
    }
}
