using System;
using System.Collections.Generic;
using System.Linq;

namespace Neyrosetka
{
    public interface INeuron
    {
        string Name { get; set; }
        List<IDendrite> Dendrites { get; set; }
        double AxonValue { get; set; }

        void Think();
    }

    public interface IDendrite
    {
        double Weight { get; set; }
        void AddToNeuron(INeuron neuron);
        INeuron GetSourceNeuron();
    }

    public class Neuron : INeuron
    {
        public string Name { get; set; }

        public Neuron()
        {
            this.Dendrites = new List<IDendrite>();
        }

        public Neuron(string Name)
        {
            this.Name = Name;
            Dendrites = new List<IDendrite>();
            AxonValue = 0.5;
        }

        public double AxonValue { get; set; }

        public List<IDendrite> Dendrites { get; set; }
        
        public void Think()
        {
            double sum = 0;
            if (Dendrites.Count <= 0) return;
            sum += Dendrites.Sum(t => t.GetSourceNeuron().AxonValue * t.Weight);
            AxonValue = 1 / (1 + Math.Exp(-sum));
        }
    }

    public class Dendrite : IDendrite
    {
        private INeuron Neuron1 { get; set; }

        private Dendrite(INeuron neuron)
        {
            Neuron1 = neuron;
            Weight = 0;
        }

        public Dendrite()
        {
            Neuron1 = null;
               Weight = 0;
        }

        public void AddToNeuron(INeuron neuron)
        {
            Neuron1 = neuron;
        }

        public double Weight { get; set; }

        public INeuron GetSourceNeuron()
        {
            return Neuron1;
        }
    }
}