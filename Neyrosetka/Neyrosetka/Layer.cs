using System;
using System.Collections.Generic;

namespace Neyrosetka
{
    public interface ILayer
    {
        List<INeuron> Neurons { get; set; }
        void Think();
        void ConnectNeuron(INeuron neuron);
        INeuron BestGuess();
        INeuron GetNeuron(string name);
    }


    public class Layer : ILayer
    {
        public Layer()
        {
            Neurons = new List<INeuron>();
        }
        public List<INeuron> Neurons { get; set; }
        
        //заставляет все нейроны выходного слоя вычислять выходное значение
        public void Think()
        {
            foreach (var t in Neurons)
                t.Think();
        }


        //соединяет нейрон из другого слоя со всеми нейронами текущего слоя
        public void ConnectNeuron(INeuron neuron)
        {
            foreach (var t in Neurons)
            {
                IDendrite dendrite = new Dendrite();
                dendrite.AddToNeuron(t);
                neuron.Dendrites.Add(dendrite);
            }
        }


        //поиск нейрона по его имени
        public INeuron GetNeuron(string name)
        {
            if(Neurons!=null)
            foreach (var t in Neurons)
                if (string.Equals(t.Name, name, StringComparison.CurrentCultureIgnoreCase))
                    return t;

            return null;
        }


        //возвращает нейрон с наибольшим весом
        public INeuron BestGuess()
        {
            double max = 0.5;
            var bestGuessIndex = -1;

            for (var i = 0; i < Neurons.Count; i++)
                if (Neurons[i].AxonValue > max)
                {
                    max = Neurons[i].AxonValue;
                    bestGuessIndex = i;
                }

            return bestGuessIndex == -1 ? null : Neurons[bestGuessIndex];
        }
    }
}