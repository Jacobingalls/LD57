using info.jacobingalls.jamkit;
using UnityEngine;

[RequireComponent(typeof(PubSubSender))]
public class CameraController : MonoBehaviour
{

    public float MinHeight = 0f;
    public float MaxHeight = 10f;

    public float keyHoldTime = 0f;

    public float startPositionY = 5f;

    public bool isEndGame = false;
    public Vector3 endGamePosition = new Vector3(0f, 0f, 0f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 position = transform.position;
        position.y = startPositionY;
        transform.position = position;
    }

    private bool _postedScrollNotification = false;
    private float _totalScrolledAbs = 0.0f;

    // Update is called once per frame
    void Update()
    {

        if (isEndGame)
        {
            transform.position = expDecay(transform.position, endGamePosition, 2f, Time.deltaTime);
            return;
        }


        float moveSpeed = Mathf.Min(20f * Mathf.Pow(2f, 10f * keyHoldTime), 100f);
        Vector3 position = transform.position;

        float movement = 0.0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            movement = moveSpeed * Time.deltaTime;
            position.y += movement;
            keyHoldTime += Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            movement = moveSpeed * Time.deltaTime;
            position.y -= movement;
            keyHoldTime += Time.deltaTime;
        }
        else if (Input.mouseScrollDelta.y != 0)
        {
            movement = Input.mouseScrollDelta.y * 100 * Time.deltaTime;
            position.y += movement;
            // Don't use keyhold time as users can control speed.
        } 
        else 
        {
            keyHoldTime = 0;
        }

        _totalScrolledAbs += Mathf.Abs(movement);

        if (!_postedScrollNotification)
        {
            if (_totalScrolledAbs > 5.0f)
            {
                _postedScrollNotification = true;
                GetComponent<PubSubSender>().Publish("mouse.scrolled.sufficiently");
            }
        }

        position.y = Mathf.Clamp(position.y, MinHeight, MaxHeight);
        transform.position = position;
    }

    public void TriggerEndGameCinimatic(PubSubListenerEvent e) {
        Vector3 position = transform.position;
        position.y = e.sender.transform.position.y;
        transform.position = position;
        isEndGame = true;
        endGamePosition = position + new Vector3(0f, -10f, 0f);
    }

    // https://acegikmo.substack.com/p/lerp-smoothing-is-broken
    float expDecay(float a, float b, float decay, float deltaTime)
    {
        return b + (a - b) * Mathf.Exp(-decay * deltaTime);
    }

    Vector3 expDecay(Vector3 a, Vector3 b, float decay, float deltaTime)
    {
        return new Vector3(
            expDecay(a.x, b.x, decay, deltaTime),
            expDecay(a.y, b.y, decay, deltaTime),
            expDecay(a.z, b.z, decay, deltaTime)
        );
    }
}
