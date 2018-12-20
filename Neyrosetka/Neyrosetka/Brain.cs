using System;
using System.Collections.Generic;

namespace Neyrosetka
{
    public interface IBrain
    {
        List<ILayer> Layers { get; set; }
        void Think();
        void Train(int[] inputData, INeuron outputNeuron);
    }


    public class Brain : IBrain
    {
        public Brain()
        {
            Layers = new List<ILayer>();
        }
        public List<ILayer> Layers { get; set; }

        public void Think()
        {
            foreach (var t in Layers) t.Think();
        }

        public void Train(int[] inputData, INeuron outputNeuron)
        {
            if (Layers.Count == 0) return;
            var inputLayer = Layers[0];
            for (var i = 0; i < inputData.Length; i++) inputLayer.Neurons[i].AxonValue = inputData[i];
            Think();
            var learningRate = 0.01;
            for (var i = 0; i < outputNeuron.Dendrites.Count; i++)
            {
                var dendrite = outputNeuron.Dendrites[i];
                var delta = Convert.ToDouble(Math.Max(inputData[i], 0) - outputNeuron.AxonValue);
                dendrite.Weight += Convert.ToDouble(Math.Max(inputData[i], 0) * delta * learningRate);
            }
        }
    }
}