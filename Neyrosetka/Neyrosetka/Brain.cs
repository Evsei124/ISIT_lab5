using System;
using System.Collections.Generic;

namespace Neyrosetka
{
    public interface IBrain
    {
        List<ILayer> Layers { get; set; }
        IBrain CreateBrainFromBrain(IBrain brain);
        void Think();
        void Train(int[] inputData, INeuron outputNeuron);
    }


    public class Brain : IBrain
    {
        public Brain()
        {
        }

        private Brain(IBrain brainData)
        {
            Layers = new List<ILayer>();

            //if brainData is provided, rebuild the brain based on a previous state
            if (brainData != null)
                for (var i = 0; i < brainData.Layers.Count; i++)
                {
                    var layerData = brainData.Layers[i];
                    ILayer layer = new Layer();
                    Layers.Add(layer);
                    for (var j = 0; j < layerData.Neurons.Count; j++)
                    {
                        var neuronData = layerData.Neurons[j];

                        var neuron = new Neuron {AxonValue = neuronData.AxonValue, Name = neuronData.Name};
                        layer.Neurons.Add(neuron);
                        if (i > 0) Layers[i - 1].ConnectNeuron(neuron);
                        //set weights for each dendrite
                        for (var k = 0; k < neuronData.Dendrites.Count; k++)
                            neuron.Dendrites[k].Weight = neuronData.Dendrites[k].Weight;
                    }
                }
        }

        public List<ILayer> Layers { get; set; }

        public IBrain CreateBrainFromBrain(IBrain brain)
        {
            return new Brain(brain);
        }

        public void Think()
        {
            foreach (var t in Layers) t.Think();
        }

        public void Train(int[] inputData, INeuron outputNeuron)
        {
            if (Layers.Count == 0) return;

            //fill the first layer with input data to feed the network
            var inputLayer = Layers[0];
            for (var i = 0; i < inputData.Length; i++) inputLayer.Neurons[i].AxonValue = inputData[i];

            //generate output for the given inputs
            Think();

            //adjust weights using the delta 
            //the generated output is compared to the training input: the drawing in this case.
            //the subtraction is the error which will be corrected by adjusting the weight. 
            double delta = 0;
            var learningRate = 0.01;
            for (var i = 0; i < outputNeuron.Dendrites.Count; i++)
            {
                var dendrite = outputNeuron.Dendrites[i];
                delta = Convert.ToDouble(Math.Max(inputData[i], 0) - outputNeuron.AxonValue);
                dendrite.Weight += Convert.ToDouble(Math.Max(inputData[i], 0) * delta * learningRate);
            }
        }
    }
}