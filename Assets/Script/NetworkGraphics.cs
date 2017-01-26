using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Layer = System.Collections.Generic.List<Node>;

public class NetworkGraphics : MonoBehaviour
{
    public GameObject inputObject;
    public GameObject hiddenObject;
    public GameObject outputObject;
    public GameObject lineObject;

    public GameObject percentageText;

    public float offsetX = 2.0f;
    public float offsetY = 1.0f;

    public GameObject car;
    Bot bot;

    List<Layer> m_layers = new List<Layer>();
    //List<GameObject> nodeObjects = new List<GameObject>();
    List<List<GameObject>> nodeObjectLayers = new List<List<GameObject>>();
    List<float> inputVals;

    void Start()
    {
        bot = car.GetComponent<Bot>();

        inputVals = new List<float>();
        inputVals.Add(bot.L);
        inputVals.Add(bot.FL);
        inputVals.Add(bot.F);
        inputVals.Add(bot.FR);
        inputVals.Add(bot.R);
    }

    void Update ()
    {
        UpdateInputText();
        UpdateOutputText();
    }

    public void UpdateOutputText()
    {
        percentageText.GetComponent<TextMesh>().text = bot.network.predictionChance.ToString("F0") + "/100%";
        percentageText.GetComponent<TextMesh>().color = new Color((100.0f - bot.network.predictionChance) / 100.0f, bot.network.predictionChance / 100.0f, 0);

        for (int i = 0; i < nodeObjectLayers[nodeObjectLayers.Count - 1].Count; i++)
        {
            string outputValue = (m_layers[m_layers.Count - 1][i].GetOutputValue()).ToString("F2");
            string errorValue = (m_layers[m_layers.Count - 1][i].GetErrorValue()).ToString("F2");
            nodeObjectLayers[nodeObjectLayers.Count - 1][i].transform.GetChild(0).GetComponent<TextMesh>().text = "val " + outputValue + " err " + errorValue;
        }
    }

    public void UpdateInputText()
    {
        for (int i = 0; i < nodeObjectLayers[0].Count; i++)
        {
            nodeObjectLayers[0][i].transform.GetChild(0).GetComponent<TextMesh>().text = (m_layers[0][i].GetOutputValue()).ToString("F2");
        }
    }

    public void CreateConnections()
    {
        // Layers
        for (int i = nodeObjectLayers.Count - 1; i > 0; i--)
        {
            // Nodes
            for (int j = 0; j < nodeObjectLayers[i].Count; j++)
            {
                Vector3 nodeFrom = new Vector3(nodeObjectLayers[i][j].transform.position.x, nodeObjectLayers[i][j].transform.position.y, nodeObjectLayers[i][j].transform.position.z);

                // Nodes in layer to the left
                for (int k = 0; k < nodeObjectLayers[i - 1].Count; k++)
                {
                    Vector3 nodeTo = new Vector3(nodeObjectLayers[i - 1][k].transform.position.x, nodeObjectLayers[i - 1][k].transform.position.y, nodeObjectLayers[i - 1][k].transform.position.z);

                    GameObject o = Instantiate(lineObject, nodeFrom, Quaternion.identity) as GameObject;

                    o.transform.LookAt(nodeTo);
                    o.transform.localScale = new Vector3(o.transform.localScale.x, o.transform.localScale.y, Vector3.Distance(nodeFrom, nodeTo));
                    o.transform.parent = transform;
                }
            }
        }
    }

    public void CreateNetwork(List<Layer> _layers)
    {
        foreach (Transform c in transform)
            Destroy(c.gameObject);

        m_layers.Clear();
        m_layers = _layers;
        nodeObjectLayers.Clear();

        for (int i = 0; i < _layers.Count; i++)
        {
            List<GameObject> nodeObjects = new List<GameObject>();

            for (int j = 0; j < _layers[i].Count; j++)
            {
                Vector3 nodePosition = new Vector3(i * offsetX, 0,  -j * offsetY);

                //GameObject o = Instantiate(nodeObject, transform.position + nodePosition, Quaternion.identity) as GameObject;

                GameObject o;

                if (i == 0)
                    o = Instantiate(inputObject, transform.position + nodePosition, Quaternion.identity) as GameObject;
                else if (i == _layers.Count - 1)
                    o = Instantiate(outputObject, transform.position + nodePosition, Quaternion.identity) as GameObject;
                else
                    o = Instantiate(hiddenObject, transform.position + nodePosition, Quaternion.identity) as GameObject;

                o.transform.parent = transform;
                nodeObjects.Add(o);
            }
            nodeObjectLayers.Add(nodeObjects);
        }

        CreateConnections();
    }
}
