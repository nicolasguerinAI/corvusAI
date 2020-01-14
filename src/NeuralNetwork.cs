using System;
using System.Collections.Generic;

public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private readonly int[] _layers;
    private float[][] _neurons;
    private float[][][] _weights;
    private float _fitness;

    public NeuralNetwork(IList<int> layers)
    {
        _layers = new int[layers.Count];
        for (var i = 0; i < layers.Count; i++)
            _layers[i] = layers[i];

        InitNeurons();
        InitWeights();
    }

    public NeuralNetwork(NeuralNetwork copyNetwork)
    {
        _layers = new int[copyNetwork._layers.Length];
        for (var i = 0; i < copyNetwork._layers.Length; i++)
            _layers[i] = copyNetwork._layers[i];

        InitNeurons();
        InitWeights();
        CopyWeights(copyNetwork._weights);
        SetFitness(copyNetwork._fitness);
    }

    public void CopyWeights(IList<float[][]> copyWeights)
    {
        for (var i = 0; i < _weights.Length; i++)
        {
            for (var j = 0; j < _weights[i].Length; j++)
            {
                for (var k = 0; k < _weights[i][j].Length; k++)
                    _weights[i][j][k] = copyWeights[i][j][k];
            }
        }
    }

    private void InitNeurons()
    {
        var neuronsList = new List<float[]>();

        for (var i = 0; i < _layers.Length; i++)
        {
            var numberNodes = _layers[i];

            if (i != _layers.Length - 1)
            {
                numberNodes++;
                neuronsList.Add(new float[numberNodes]);
            }
            else
                neuronsList.Add(new float[numberNodes]);
        }

        _neurons = neuronsList.ToArray();
    }

    private void InitWeights()
    {
        var weightsList = new List<float[][]>();
        var random = new Random(DateTime.Now.Millisecond);

        for (var i = 1; i < _layers.Length; i++)
        {
            var layerWeightsList = new List<float[]>();
            var neuronsInPreviousLayer = _neurons[i - 1].Length;

            for (var j = 0; j < _neurons[i].Length; j++)
            {
                var neuronWeights = new float[neuronsInPreviousLayer];

                var k = 0;

                for (; k < neuronsInPreviousLayer - 1; k++)
                    neuronWeights[k] = (float) ((random.NextDouble() * .5 - 1.0));

                neuronWeights[neuronsInPreviousLayer - 1] = 1f;

                layerWeightsList.Add(neuronWeights);
            }

            weightsList.Add(layerWeightsList.ToArray());
        }

        _weights = weightsList.ToArray();
    }

        public float[] FeedForward(float[] inputs)
        {
            for (var i = 0; i < inputs.Length; i++)
                _neurons[0][i] = inputs[i];

            _neurons[0][inputs.Length] = 1f; // Bias

            for (var i = 1; i < _layers.Length; i++)
            {
                for (var j = 0; j < _neurons[i].Length; j++)
                {
                    var value = 0f;

                    for (var k = 0; k < _neurons[i - 1].Length; k++)
                        value += _weights[i - 1][j][k] * _neurons[i - 1][k];


                    _neurons[i][j] = (float) Math.Tanh(value); //TanH activation
                    //_neurons[i][j] = 1f / (1f + Mathf.Pow((float)Math.E, -4.9f * value)); // Sigmoid activation between 0 and 1
                }
            }

            return _neurons[_neurons.Length - 1];
        }

    public void Mutate()
    {
        var random = new Random(DateTime.Now.Millisecond);

        for (var i = 0; i < _weights.Length; i++)
        {
            for (var j = 0; j < _weights[i].Length; j++)
            {
                for (var k = 0; k < _weights[i][j].Length; k++)
                {
                    var weight = _weights[i][j][k];
                    var randomNumber = (float)(random.NextDouble() * 100.0);

                    if (randomNumber <= 5f)
                        weight *= -1f;
                    else if (randomNumber <= 10f)
                        weight = (float) ((random.NextDouble() * .5 - 1.0));
                    else if (randomNumber <= 15f)
                    {
                        var randomChoice = (float) random.Next(0, 2);

                        if (randomChoice == 0)
                        {
                            var factor = (float) random.NextDouble() + 1f;
                            weight *= factor;
                            }
                        else
                        {
                            var factor = (float) random.NextDouble();
                            weight *= factor;
                        }
                    }

                    if (weight > 1f)
                        weight = 1f;
                    if (weight < -1f)
                        weight = -1f;

                    if (k == _weights[i][j].Length - 1)
                        weight = 1f;

                    _weights[i][j][k] = weight;
                }
            }
        }
    }

    public void AddFitness(float fitness)
    {
        _fitness += fitness;
    }

    public void SetFitness(float fitness)
    {
        _fitness = fitness;
    }

    public float GetFitness()
    {
        return _fitness;
    }

    public void DumpWeights()
    {
        for (var i = 0; i < _weights.Length; i++)
        {
            for (var j = 0; j < _weights[i].Length; j++)
            {
                for (var k = 0; k < _weights[i][j].Length; k++)
                    Console.WriteLine("Layer: " + (i + 1) + " Connection: " + k + "->" + j + ": " + _weights[i][j][k]);
            }
        }
    }

    public float[][][] GetWeights()
    {
        return _weights;
    }

    public int CompareTo(NeuralNetwork otherNetwork)
    {
        if (otherNetwork == null) return 1;

        if (_fitness > otherNetwork._fitness) return 1;
        if (_fitness < otherNetwork._fitness) return -1;

        return 0;
    }
}