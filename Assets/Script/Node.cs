using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Layer = System.Collections.Generic.List<Node>;

public class Connection
{
    public float weight;
    public float deltaWeight;
};

public class Node
{
    int m_index;
    float m_outputValue;
    float m_gradient;
    float m_error;
    List<Connection> m_outputWeights = new List<Connection>();

    public float GetOutputValue() { return m_outputValue; }
    public float GetErrorValue() { return m_error; }
    public void SetOutputValue(float _value) { m_outputValue = _value; }
    public List<Connection> GetOutputWeights() { return m_outputWeights; }
    
    public Node(int _numOutputs, int _index)
    {
        m_index = _index;
        for(int c = 0; c < _numOutputs; c++)
        {
            m_outputWeights.Add(new Connection());
            m_outputWeights[m_outputWeights.Count - 1].weight = Random.Range(-1.0f, 1.0f); // --------------------------------- ERROR MIGHT ARISE
            m_outputWeights[m_outputWeights.Count-1].deltaWeight = m_outputWeights[m_outputWeights.Count - 1].weight;
        }
    }

    public void FeedForward(Layer _prevLayer)
    {
        float sum = 0.0f;
        // Iterate and set hidden layer output to input layer output * weight (include bias node)
        for (int i = 0; i < _prevLayer.Count; i++)
            sum += _prevLayer[i].GetOutputValue() * _prevLayer[i].m_outputWeights[m_index].weight;

        m_outputValue = TransferFunction(sum);
    }

    float TransferFunction(float _val){ return Math.Tanh(_val); }
    float TransferFunctionDerivative(float _val) { return 1.0f - (_val * _val); }
    //float TransferFunction(float _val) { return Mathf.RoundToInt(_val); } // ----------------------------------- ERROR MIGHT ARISE HERE
    //float TransferFunctionDerivative(float _val) { return 1.0f - (_val * _val); }

    public void CalcHiddenGradients(Layer _nextLayer)
    {
        float sum = 0.0f;
        // Iterate over all nodes in layer, weight * gradient
        for (int i = 0; i < _nextLayer.Count - 1; i++)
            sum += m_outputWeights[i].weight * _nextLayer[i].m_gradient;

        m_gradient = sum * TransferFunctionDerivative(m_outputValue);
    }

    public void CalcOutputGradients(float _targetVal)
    {
        float delta = _targetVal - m_outputValue;
        m_error = delta;
        m_gradient = delta * TransferFunctionDerivative(m_outputValue); // --------------------------- ERROR MIGHT ARISE HERE
        //m_gradient = delta * TransferFunction(m_outputValue);
    }

    public void UpdateInputWeights(Layer _prevLayer)
    {
        float eta = 0.25f;
        float alpha = 0.5f;

        for (int i = 0; i < _prevLayer.Count; i++)
        {
            Node node = _prevLayer[i];

            // Calculate newDeltaWeight as result of eta * alpha * node.outputVal * gradient
            float newDeltaWeight = eta * alpha * node.GetOutputValue() * m_gradient;

            node.m_outputWeights[m_index].deltaWeight = newDeltaWeight;
            node.m_outputWeights[m_index].weight += newDeltaWeight;
        }
    }
}

static class Math
{
    static float e = 2.71828182f;
    public static float Tanh(float _x)
    {
        return (Mathf.Pow(e, _x) - Mathf.Pow(e, -_x)) / (Mathf.Pow(e, _x) + Mathf.Pow(e, -_x));
    }
}