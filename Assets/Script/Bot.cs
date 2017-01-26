using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct KeyData
{
    public bool Forward;
    public bool Break;
    public bool Left;
    public bool Right;

    private int m_TotalState;
 
    public bool DetectChange()
    {
        int newState = BoolToInt(Forward) + BoolToInt(Break) + BoolToInt(Left) + BoolToInt(Right);
        if (m_TotalState != newState)
        {
            m_TotalState = newState;
            return true;
        }
        return false;
    }

    public int BoolToInt(bool _b)
    {
        return _b ? 1 : 0;
    }

    public void ReleaseAll()
    {
        Forward = false;
        Break = false;
        Left = false;
        Right = false;
    }

    public void PrintKeyState()
    {
        Debug.Log("Forward=" + Forward + " Break=" + Break + " Left=" + Left + " Right=" + Right);
    }
}

public class Bot : MonoBehaviour
{
    public float rayLength = 10.0f;
    public float L, Ln, FL, FLn, F, Fn, FR, FRn, R, Rn = 0.0f;
    public float currentSpeed = 0.0f;
    public Network network;
    public CarController car;
    public GameObject NetworkDrawer;

    private KeyData lastKeyData;

    public void Start ()
    {
        // Initialize network
        List<int> topology = new List<int>();
        topology.Add(6);
        topology.Add(10);
        topology.Add(4); // ----------- ERROR MIGHT ARISE DUE TO CHANGING FROM 4 TO 6

        network = new Network(topology);
        car = GetComponent<CarController>();
        NetworkDrawer.GetComponent<NetworkGraphics>().CreateNetwork(network.m_layers);
    }
	
	void Update ()
    {
        // Get sensor data. Distance and normal values
        L = SensorData(-transform.right, rayLength * 0.25f); // Multiply by 0.25 to increase its fluctuation (i.e. making it more important)

        FL = SensorData((2*transform.forward - transform.right).normalized, rayLength);

        F = SensorData(transform.forward, rayLength);

        FR = SensorData((2*transform.forward + transform.right).normalized, rayLength);

        R = SensorData(transform.right, rayLength * 0.25f); // Multiply by 0.25 to increase its fluctuation (i.e. making it more important)

        currentSpeed = car.speed / car.maxSpeed;

        // Release any unused/uncleared lists that might exist
        //Resources.UnloadUnusedAssets();
    }

    List<float> CreateInputList()
    {
        List<float> inputVals = new List<float>();
        inputVals.Add(L);
        //inputVals.Add(Ln);

        inputVals.Add(FL); // ------------------------ ERROR MIGHT ARISE HERE
        //inputVals.Add(FLn);

        inputVals.Add(F);
        //inputVals.Add(Fn);

        inputVals.Add(FR);
        //inputVals.Add(FRn);

        inputVals.Add(R);
        //inputVals.Add(Rn);

        inputVals.Add(currentSpeed);

        return inputVals;
    }

    public void Control()
    {
        // Give sensor data
        network.GiveNewInput(CreateInputList());
        network.UpdateOutputValues();

        // Get values from other end
        List<float> outputValues = new List<float>();
        outputValues = network.GetOutputValues();
        car.Move(outputValues[0]); // Forward
        car.Move(-outputValues[1]); // Backward

        //if(outputValues[2] > outputValues[3])
            car.Turn(-outputValues[2]); // Left
        //else
            car.Turn(outputValues[3]); // Right
    }

    // Give the network your input data and teach it accordingly
    public void TeachNetwork(KeyData _keyData)
    {
        // Feed the network new input values
        network.FeedForward(CreateInputList());

        List<float> wantedOutputVals = new List<float>();
        wantedOutputVals.Add((float)_keyData.BoolToInt(_keyData.Forward));
        wantedOutputVals.Add((float)_keyData.BoolToInt(_keyData.Break));
        wantedOutputVals.Add((float)_keyData.BoolToInt(_keyData.Left)); // Multiplying by distance to Left because of logic
        wantedOutputVals.Add((float)_keyData.BoolToInt(_keyData.Right)); // Multiplying by distance to right because of logic

        // Change weights accordingly
        network.BackProp(wantedOutputVals);

        // Use these keys to compare with if we change key inputs the next loop
        lastKeyData = _keyData;
    }

    // 
    float SensorData(Vector3 _direction, float _rayLength)
    {
        RaycastHit hit;
        Vector3 angleDir = Vector3.zero;
        float distance = 0.0f;
        float dir = 0.0f;
        if (Physics.Raycast(transform.position, _direction, out hit, _rayLength))
        {
            angleDir = Vector3.Cross((transform.position - hit.point).normalized, hit.normal);
            dir = Vector3.Dot(angleDir, transform.up);
            distance = 1.0f - (hit.distance / _rayLength);
        }
        Mathf.Abs(dir);

        return distance * dir;
    }
    /*
    // Return special kind of normal data to determine curve turning
    float NormalVectorSensorData(Vector3 _direction, float _rayLength)
    {
        RaycastHit hit;
        Vector3 angleDir = Vector3.zero;
        if (Physics.Raycast(transform.position, _direction, out hit, _rayLength))
            angleDir = Vector3.Cross((transform.position - hit.point).normalized, hit.normal);

        float dir = Vector3.Dot(angleDir, transform.up);

        if (dir > 0.0f)
            return 1.0f;
        else if (dir < 0.0f)
            return 0.0f;
        else
            return 0.5f;        
    }
    */

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.right * rayLength * 0.25f);
        Gizmos.DrawRay(transform.position, (2*transform.forward - transform.right).normalized * rayLength );
        Gizmos.DrawRay(transform.position, transform.forward * rayLength);
        Gizmos.DrawRay(transform.position, (2*transform.forward + transform.right).normalized * rayLength);
        Gizmos.DrawRay(transform.position, transform.right * rayLength * 0.25f);
    }
}
