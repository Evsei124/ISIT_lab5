using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Neyrosetka
{
    public interface IBZ
    {
        IBrain Brain { get; set; }
        void Train(int[] array, string bukva); //тренировка мозга
        bool LoadBrain(string path); //загрузка данных из файла в формате XML
        bool SaveBrain(string path); //сохранение данных из файла в формате XML

        void ReadChar(int[] inputVector, out string answer,
            out Dictionary<string, string> neuronValues); //метод возвращает список нейронов и ответ сети

        void CreateNewBrain(int inputSignalCount);
        void TrainingFromFile();
    }

    public class BZ : IBZ
    {
        public IBrain Brain { get; set; }

        public void Train(int[] array, string bukva)
        {
            var brain = Brain;
            var character = bukva.ToUpper(); //определение буквы, которой будем обучать
            if (character.Length == 0) return; //проверка наличия буквы
            var inputLayer = brain.Layers[0]; //входной слой
            var outputLayer = brain.Layers[brain.Layers.Count - 1]; //выходной слой
            var outputNeuron = outputLayer.GetNeuron(character); //проверяем, есть ли в мозге нейрон с такой буквой

            if (outputNeuron == null) //если нет, создаем новый
            {
                outputNeuron = new Neuron(character);
                inputLayer.ConnectNeuron(outputNeuron);
                outputLayer.Neurons.Add(outputNeuron);
            }

            //обучаем сеть с текущим рисунком, примененным к выходному нейрону
            brain.Train(array, outputNeuron);
        }


        public bool LoadBrain(string path)
        {
            var dataBase = XDocument.Load(path);
            var Xbrain = dataBase.Root;
            IBrain brain = new Network();
            var first = true;
            foreach (var Xlayer in Xbrain.Elements())
            {
                ILayer layer;
                layer = first ? methodForFirstLayer(Xlayer) : methodForNextLayer(Xlayer, brain.Layers[0]);
                first = false;
                brain.Layers.Add(layer);
            }

            if (brain.Layers.Count < 2)
            {
                MessageBox.Show(
                    "Неподдерживаемый файл, обновите файл и повторите попытку. Или просто натренируйте сеть заново.");
                return false;
            }

            Brain = brain;
            return true;
        }


        public bool SaveBrain(string path)
        {
            var brain = Brain;
            if (brain == null) return false;
            var Xbrain = new XElement("Brain"); //создаем мозг
            Xbrain.Add(new XAttribute("FirstLayerNeyronCount", brain.Layers[0].Neurons.Count));
            var XLayers = new List<XElement>(); //создаем список слоев
            foreach (var layer in brain.Layers)
            {
                XLayers.Add(new XElement("Layer")); //создаем входной слой
                var Neurons = new List<XElement>(); //создаем список нейронов
                for (var i = 0; i < layer.Neurons.Count; i++)
                {
                    Neurons.Add(new XElement("Neuron")); //добавляем новый нейрон в слой
                    Neurons[i].Add(new XAttribute("AxonValue", layer.Neurons[i].AxonValue));

                    Neurons[i].Add(layer.Neurons[i].Name == null
                        ? new XAttribute("Name", "null")
                        : new XAttribute("Name", layer.Neurons[i].Name));

                    var Dendrites = new List<XElement>(); //создаем список дендритов
                    for (var j = 0; j < layer.Neurons[i].Dendrites.Count; j++)
                    {
                        Dendrites.Add(new XElement("Dendrite")); //добавляем дендрит в нейрон
                        Dendrites[j].Add(new XAttribute("Weight", layer.Neurons[i].Dendrites[j].Weight));
                    }

                    Neurons[i].Add(Dendrites);
                }

                XLayers[XLayers.Count - 1].Add(Neurons);
            }

            Xbrain.Add(XLayers);
            Xbrain.Save(path);
            return true;
        }

        public void ReadChar(int[] inputVector, out string answer, out Dictionary<string, string> neuronValues)//распознать символ
        {
            var brain = Brain;
            var inputLayer = brain.Layers[0]; //входной слой
            var outputLayer = brain.Layers[brain.Layers.Count - 1]; //выходной слой

            //fill the input layer
            for (var i = 0; i < inputVector.Length; i++) inputLayer.Neurons[i].AxonValue = inputVector[i];

            //make the network 'think'
            brain.Think();
            double sum = 0;
            var neuronValuesT = new Dictionary<string, string>();
            for (var i = 0; i < outputLayer.Neurons.Count; i++)
            {
                var neuron = outputLayer.Neurons[i];
                neuronValuesT.Add(neuron.Name.ToUpper(), neuron.AxonValue.ToString());
                sum += neuron.AxonValue;
            }

            neuronValues = new Dictionary<string, string>();

            foreach (var item in neuronValuesT)
            {
                neuronValues.Add(item.Key,(double.Parse(item.Value)/sum).ToString());
            }

            var bestGuess = outputLayer.BestGuess();
            answer = bestGuess != null ? bestGuess.Name.ToUpper() : "ваша писанина нечитабельна";
        }


        public void CreateNewBrain(int inputSignalCount)
        {
            IBrain brain = new Network(); //создаем мозг
            brain.Layers = new List<ILayer> {new Layer()}; //создаем список слоев //создаем входной слой
            brain.Layers[0].Neurons = new List<INeuron>(); //создаем список нейронов
            for (var i = 0; i < inputSignalCount; i++)
                brain.Layers[0].Neurons.Add(new Neuron()); //добавляем новый нейрон в слой
            brain.Layers.Add(new Layer()); //создаем выходной слой
            brain.Layers[1].Neurons = new List<INeuron>(); //создаем список нейронов
            Brain = brain;
        }

        public void TrainingFromFile()
        {
            for (var epoch = 0; epoch < 10; ++epoch)
            for (var i = 0; i < TrainingSet.Vectors.Count; ++i)
                Train(TrainingSet.Vectors[i], TrainingSet.Chars[i]);
        }

        private ILayer methodForFirstLayer(XElement Xlayer)
        {
            ILayer layer = new Layer();
            foreach (var unused in Xlayer.Elements()) layer.Neurons.Add(new Neuron());
            return layer;
        }

        private ILayer methodForNextLayer(XElement Xlayer, ILayer prevLayer)
        {
            ILayer layer = new Layer();
            foreach (var Xneuron in Xlayer.Elements())
            {
                INeuron neuron = new Neuron(Xneuron.Attribute("Name").Value);
                neuron.AxonValue = double.Parse(Xneuron.Attribute("AxonValue").Value);
                layer.Neurons.Add(neuron);
            }

            foreach (var neuron in layer.Neurons) prevLayer.ConnectNeuron(neuron);

            var i = 0;
            foreach (var Xneuron in Xlayer.Elements())
            {
                var neuron = layer.Neurons[i];
                var j = 0;
                foreach (var Xdendrite in Xneuron.Elements())
                {
                    neuron.Dendrites[j].Weight = double.Parse(Xdendrite.Attribute("Weight").Value);
                    j++;
                }

                i++;
            }

            return layer;
        }
    }
}