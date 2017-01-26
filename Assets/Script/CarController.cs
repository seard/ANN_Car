using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour {

    public float speed;
    public float maxSpeed = 6.0f;
    public float speedMultiplier = 3.0f;
    public float drag = 0.5f;
    public float rotation;
    float smoothRotation;
    KeyData keys;
    Bot bot;
    public bool controlState = true;

    // Use this for initialization
    public void Start ()
    {
        bot = GetComponent<Bot>();
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle who controls the car 
        if (Input.GetKeyDown(KeyCode.F))
            controlState = !controlState;

        // If we want player to control
        if (controlState)
        {
            keys.ReleaseAll();

            if (keys.Forward = Input.GetKey(KeyCode.W))
                Move(1.0f);
            if (keys.Break = Input.GetKey(KeyCode.S))
                Move(-1.0f);
            if (keys.Left = Input.GetKey(KeyCode.A))
                Turn(-1.0f);
            if (keys.Right = Input.GetKey(KeyCode.D))
                Turn(1.0f);

            //Keys.PrintKeyState();
            bot.TeachNetwork(keys);

            /*
            if (keys.DetectChange())
            {
                // Send the new key data to the network teacher
                //bot.TeachNetwork(keys);

                //bot.network.PrintOutputResults();
                //Debug.Log("A key was switched");
            }
            */
        }
        // If we want bot to control
        else
        {
            bot.Control();
        }
    }

    void FixedUpdate()
    {
        speed = Mathf.Clamp(speed, -maxSpeed, maxSpeed);

        speed -= speed * drag * Time.fixedDeltaTime;
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
        transform.position = Vector3.Scale(transform.position, new Vector3(1, 0, 1));

        smoothRotation = Mathf.Lerp(smoothRotation, rotation, Time.fixedDeltaTime * 4.0f);
        transform.eulerAngles = new Vector3(0, smoothRotation, 0);
	}

    public void Turn(float _angle)
    {
        rotation += _angle * Time.deltaTime * 90.0f;
    }

    public void Move(float _amount)
    {
        speed += _amount * speedMultiplier * Time.deltaTime;
    }

    void OnCollisionStay(Collision _collision)
    {
        transform.position += _collision.contacts[0].normal * 2.0f * Time.deltaTime;
    }
}
