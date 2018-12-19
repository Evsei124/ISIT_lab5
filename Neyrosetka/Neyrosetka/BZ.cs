using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Neyrosetka
{
    public interface IBZ
    {
        XDocument Baza { get; set; }
        IBrain Brain { get; set; }
        void Train(int[] array, IBrain brain, string bukva); //тренировка мозга
        IBrain LoadBrain(XmlDocument dataBase); //загрузка данных из файла в формате XML
        void SaveBrain(IBrain dataBase); //загрузка данных из файла в формате XML
        XDocument LoadXDocument(string path);
        Dictionary<string, string> ReadChar(int[] array, IBrain brain, out string answer);
    }

    public class BZ : IBZ
    {
        public IBrain Brain { get; set; }

        public void Train(int[] array, IBrain brain, string bukva)
        {
            var character = bukva.ToUpper();//определение буквы, которой будем обучать
            if (character.Length == 0) return;//проверка наличия буквы
            var inputLayer = brain.Layers[0];//входной слой
            var outputLayer = brain.Layers[brain.Layers.Count - 1];//выходной слой
            var outputNeuron = outputLayer.GetNeuron(character);//проверяем, есть ли в мозге нейрон с такой буквой

            if (outputNeuron == null)//если нет, создаем новый
            {
                outputNeuron = new Neuron(character);
                inputLayer.ConnectNeuron(outputNeuron);
                outputLayer.Neurons.Add(outputNeuron);
            }

            //обучаем сеть с текущим рисунком, примененным к выходному нейрону
            brain.Train(array, outputNeuron);
        }

        public IBrain LoadBrain(XmlDocument dataBase)
        {
            return null;
        }

        public void SaveBrain(IBrain brain)
        {
            if (brain != null)
            {
                var Xbrain = new XElement("Brain");//создаем мозг
                var XLayers = new List<XElement>();//создаем список слоев
                XLayers.Add(new XElement("Layer"));//создаем первый слой
                var Neurons = new List<XElement>();//создаем список нейронов
                for (int i = 0; i < brain.Layers[0].Neurons.Count; i++)
                {
                    Neurons.Add(new XElement("Neuron"));//добавляем новый нейрон в слой
                    var Dendrites = new List<XElement>();//создаем список дендритов
                    for (int j = 0; j < brain.Layers[0].Neurons[i].Dendrites.Count; j++)
                    {
                       Dendrites.Add(new XElement("Dendrite"));//добавляем дендрит в нейрон
                        Dendrites[j].Add(new XAttribute("Weight",brain.Layers[0].Neurons[i].Dendrites[j].Weight));
                    }
                    Neurons[i].Add(Dendrites);
                }
                XLayers[0].Add(Neurons);
                Xbrain.Add(XLayers);
                Xbrain.Save("test.xml");
            }
        }

        public Dictionary<string,string> ReadChar(int[] array, IBrain brain, out string answer)
        {
            var inputLayer = brain.Layers[0];//входной слой
            var outputLayer = brain.Layers[brain.Layers.Count - 1];//выходной слой

            //fill the input layer
            for (var i = 0; i < array.Length; i++)
            {
                inputLayer.Neurons[i].AxonValue = array[i];
            }

            //make the network 'think'
            brain.Think();

            Dictionary<string, string> values = new Dictionary<string, string>();
            for (var i = 0; i < outputLayer.Neurons.Count; i++)
            {
                var neuron = outputLayer.Neurons[i];
                   values.Add(neuron.Name.ToUpper(), neuron.AxonValue.ToString());
            }
            var bestGuess = outputLayer.BestGuess();
            if (bestGuess != null)
            {
                answer = bestGuess.Name.ToUpper();

            }
            else
            {
                answer = "ваша писанина нечитабельна";
            }

            return values;
        }


        public XDocument LoadXDocument(string path)
        {
            return XDocument.Load(path);
        }

        public XDocument Baza { get; set; }
    }
}