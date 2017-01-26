using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Layer = System.Collections.Generic.List<Node>;

public class Network
{
    public List<Layer> m_layers = new List<Layer>(); // Node net as [layers][nodes]
    public float predictionChance;
    float averageError = 0.0f;
    float precentAverageError = 0.0f;

    public Network(List<int> _topology)
    {
        // Iterate over all [layers][nodes] and add nodes
        for (int i = 0; i < _topology.Count; i++)
        {
            m_layers.Add(new Layer());

            // The amount of connections to give each node (output nodes get 0 connections)
            int numOutputs = i == _topology.Count - 1 ? 0 : _topology[i + 1];

            // Iterate and add Neurons, including bias Neuron
            for (int j = 0; j <= _topology[i]; j++)
            {
                m_layers[m_layers.Count - 1].Add(new Node(numOutputs, j));
                Debug.Log("Node added LAYER: " + i + "   INDEX: " + j + "   NUMOUTPUTS: " + numOutputs);
            }

            m_layers[m_layers.Count - 1][m_layers[m_layers.Count - 1].Count - 1].SetOutputValue(1.0f);
        }
    }

    public List<float> GetOutputValues()
    {
        List<float> outputValues = new List<float>();

        for (int i = 0; i < m_layers[m_layers.Count - 1].Count; i++)
            outputValues.Add(m_layers[m_layers.Count - 1][i].GetOutputValue());

        return outputValues;
    }

    public void FeedForward(List<float> _inputVals)
    {
        // Set input values for the first layer neurons
        for (int i = 0; i < _inputVals.Count; i++)
            m_layers[0][i].SetOutputValue(_inputVals[i]);

        // Set all bias node values to 1
        for (int i = 0; i < m_layers.Count - 1; i++)
            m_layers[i][m_layers[i].Count - 1].SetOutputValue(1);

        // Step through the layers and feed values forward
        for (int i = 1; i < m_layers.Count; i++)
        {
            // Iterate over all hidden nodes except for the bias node
            for (int j = 0; j < m_layers[i].Count - 1; j++)
                m_layers[i][j].FeedForward(m_layers[i - 1]);
        }
    }

    public void GiveNewInput(List<float> _inputVals)
    {
        // Iterate over input nodes and update values
        for (int i = 0; i < _inputVals.Count; i++)
            m_layers[0][i].SetOutputValue(_inputVals[i]);
    }

    public void UpdateOutputValues()
    {
        // Iterate over all layers except for the first
        for(int i = 1; i < m_layers.Count; i++)
        {
            // Iterate over all nodes (excluding bias, because no node is connected to bias)
            for(int j = 0; j < m_layers[i].Count - 1; j++)
            {
                float tmpSum = 0.0f;
                // Iterate over all connecting nodes
                for(int k = 0; k < m_layers[i-1].Count; k++)
                {
                    // Add all last layer's node's [outputValue * weight] pointing with index to this node
                    tmpSum += m_layers[i - 1][k].GetOutputValue() * m_layers[i - 1][k].GetOutputWeights()[j].weight;
                }
                float sum = Math.Tanh(tmpSum);
                m_layers[i][j].SetOutputValue(sum);
            }
        }
    }

    public void CalculatePredictionChance(List<float> _targetVals)
    {
        // Calculate overall error
        Layer outputLayer = m_layers[m_layers.Count - 1];
        float m_error = 0.0f;

        for (int n = 0; n < outputLayer.Count - 1; ++n)
        {
            float delta = _targetVals[n] - outputLayer[n].GetOutputValue();
            m_error += delta * delta;
        }
        m_error /= outputLayer.Count - 1; // get average error squared
        m_error = Mathf.Sqrt(m_error); // RMS

        // Implement a recent average measurement

        precentAverageError = (precentAverageError * 100.0f + m_error) / (100.0f + 1.0f);

        float x = precentAverageError * 5.0f;
        predictionChance = 100.0f - 100.0f * (x * (1.0f / (1.0f + x)));
    }

    public void BackProp(List<float> _targetVals)
    {
        CalculatePredictionChance(_targetVals);

        // Calculate output layer gradient
        for (int i = 0; i < m_layers[m_layers.Count - 1].Count - 1; ++i)
        {
            m_layers[m_layers.Count - 1][i].CalcOutputGradients(_targetVals[i]);
        }

        // Iterate from last hidden layer to first hidden layer
        // And calculate the gradient of all hidden nodes
        for(int i = m_layers.Count - 2; i > 0; i--)
        {
            Layer hiddenLayer = m_layers[i];
            Layer nextLayer = m_layers[i + 1];
            for (int j = 0; j < m_layers.Count; j++)
                m_layers[i][j].CalcHiddenGradients(m_layers[i + 1]);
        }

        // Update all weights from output node back to start
        for (int i = m_layers.Count - 1; i > 0; i--)
        {
            // Iterate and update all network weights
            for (int j = 0; j < m_layers[i].Count - 1; j++)
            {
                m_layers[i][j].UpdateInputWeights(m_layers[i - 1]);
            }
        }
    }

    public void PrintWeightResults()
    {
        // Iterate over all layers, nodes, weights and print these weights
        for (int i = 0; i < m_layers.Count - 1; i++)
        {
            for (int j = 0; j < m_layers[i].Count; j++)
            {
                // ------------------------------ MIGHT SPARK ERROR HERE BECAUSE OF REFERENCE FOR [weights]
                List<Connection> weights = m_layers[i][j].GetOutputWeights();
                for (int k = 0; k < weights.Count; k++)
                {
                    Debug.Log("Weight of Node [" + i + "][" + j + "]" + " = " + weights[k].weight);
                }
            }
        }
    }
}

// Add wanted input distance from walls
// Try sizing up the track and increasing speed so you need to break or release gas at least
// Try adding together F and Fn into 1 value vor easier input values

// Was working on getting a bot driving continuously



// Turning right makes the values go negative?!?!?!? why?! because of transfer functions?

// ------------------- Are we actually forwarding to output nodes?!!?!?!

// ------------- TO DO
// Set to only input the forward line distance and normal
// Edit eta & alpha to lower values
// Maybe we should invert sensor distance so it goes higher the closer you get
// -------------

// BEST IDEA! Check the forward sensor normal, if it is pointing right or left of car
// this will make it easier to use the forward sensor to determine if we should turn or not

