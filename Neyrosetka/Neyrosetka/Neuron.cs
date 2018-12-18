using System;
using System.Collections.Generic;

namespace Neyrosetka
{
    internal interface INeuron
    {
        List<IDendrite> Dendrites { get; set; }
        double AxonValue { get; set; }

        void Think();
    }


    internal interface IDendrite
    {
        double Weight { get; set; }
        IDendrite AddToNeuron(INeuron neuron);
        INeuron GetSourceNeuron();
    }

    internal class Neuron : INeuron
    {
        private string _name;


        public Neuron(string Name)
        {
            _name = Name;
            Dendrites = new List<IDendrite>();
            AxonValue = 0.5;
        }

        public double AxonValue { get; set; }

        public List<IDendrite> Dendrites { get; set; }

        public void AddDendrites()
        {
            Dendrites.Add(new Dendrite().AddToNeuron(this));
        }

        public void Think()
        {
                double sum = 0;
                if (this.Dendrites.Count > 0)
                {
                    for (var i = 0; i < this.Dendrites.Count; i++)
                    {
                        sum += this.Dendrites[i].GetSourceNeuron().AxonValue * this.Dendrites[i].Weight;
                    }
                    this.AxonValue = 1 / (1 + Math.Exp(-sum));
                }
        }
    }

    internal class Dendrite : IDendrite
    {
        private INeuron _neuron;
        private IDendrite meDendrite;

        private Dendrite(INeuron neuron)
        {
            meDendrite = this;
            Weight = 0;
        }

        public Dendrite()
        {
            meDendrite = this;
            Weight = 0;
        }

        public IDendrite AddToNeuron(INeuron neuron) //возвращает дендрит который будет добавлен в нейрон
        {
            return new Dendrite(neuron);
        }

        public double Weight { get; set; }

        public INeuron GetSourceNeuron()
        {
            return _neuron;
        }
    }


}