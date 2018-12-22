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


    public class Network : IBrain
    {
        public Network()
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
            var learningRate = 0.1;
            for (var i = 0; i < outputNeuron.Dendrites.Count; i++)
            {
                var inputSig = Math.Max(inputData[i], 0);
                var delta = Convert.ToDouble(inputSig - outputNeuron.AxonValue);
                outputNeuron.Dendrites[i].Weight += Convert.ToDouble(inputSig * delta * learningRate);
            }
        }
    }
}