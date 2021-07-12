using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public class NNManager
    {
        private int _numInputParameters;
        private int _numHiddenLayers;
        private int[] _hiddenNeurons;
        private int _numOutpurParameters;
        private Network _network;

        public NNManager SetupNetwork()
        {
            _numInputParameters = 2;
            int[] hidden = new int[2];
            hidden[0] = 3;
            hidden[1] = 1;
            _numHiddenLayers = 1;
            _hiddenNeurons = hidden;
            _numOutpurParameters = 1;
            _network = new Network(_numInputParameters, _numHiddenLayers, _numOutpurParameters);
            return this;
        }

        public static Network ImportNetwork()
        {
            var dn = GetHelperNetwork();
            if (dn == null) return null;

            var network = new Network();
            var allNeuronse = new List<Neuron>();

            network.LeatningRate = dn.LeatningRate;
            network.Momentum = dn.Momentum;

            foreach (var n in dn.InputLayer)
            {
                var neuron = new Neuron()
                {
                    Id = n.Id,
                    Bias = n.Bias,
                    BiasDelta = n.BiasDelta,
                    Gradient = n.Gradient,
                    Value = n.Value,
                };
                network.InputLayer?.Add(neuron);
                allNeuronse.Add(neuron);
            }

            foreach (var layer in dn.HiddenLayers)
            {
                var neurons = new List<Neuron>();
                foreach (var n in layer)
                {
                    var neuron = new Neuron()
                    {
                        Id = n.Id,
                        Bias = n.Bias,
                        BiasDelta = n.BiasDelta,
                        Gradient = n.Gradient,
                        Value = n.Value,
                    };
                    neurons.Add(neuron);
                    allNeuronse.Add(neuron);
                }
                network.HiddenLayers?.Add(neurons);
            }

            foreach (var n in dn.OutputLayer)
            {
                var neuron = new Neuron()
                {
                    Id = n.Id,
                    Bias = n.Bias,
                    BiasDelta = n.BiasDelta,
                    Gradient = n.Gradient,
                    Value = n.Value,
                };
                network.OutputLayer?.Add(neuron);
                allNeuronse.Add(neuron);
            }

            foreach (Synapse syn in dn.Synapses)
            {
                var synapse = new Synapse() { Id = syn.Id };
                var inputNeuron = allNeuronse.First(x => x.Id == syn.InpurNeuron.Id);
                var outputNeuron = allNeuronse.First(x => x.Id == syn.OutputNeuron.Id);
                synapse.InpurNeuron = inputNeuron;
                synapse.OutputNeuron = outputNeuron;
                synapse.Weight = syn.Weight;
                synapse.WeightDelta = syn.WeightDelta;

                inputNeuron?.OutputSynapses.Add(synapse);
                outputNeuron?.InputSynapses.Add(synapse);
            }

            return network;
        }

        private static Network GetHelperNetwork()
        {
            try
            {

                using (var file = File.OpenText("Network1.txt"))
                {
                    return JsonSerializer.Deserialize<Network>(file.ReadToEnd());

                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
