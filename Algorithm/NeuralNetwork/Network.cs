using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetwork
{
    public class Network
    {
        private int numInputParameters;
        private int numHiddenLayers;
        private int numOutpurParameters;

        public Network(int numInputParameters, int numHiddenLayers, int numOutpurParameters)
        {
            this.numInputParameters = numInputParameters;
            this.numHiddenLayers = numHiddenLayers;
            this.numOutpurParameters = numOutpurParameters;
        }

        public Network()
        {
        }

        public double LeatningRate { get; set; }
        public double Momentum { get; set; }
        public List<Neuron> InputLayer { get; set; }
        public List<List<Neuron>> HiddenLayers { get; set; }
        public List<Neuron> OutputLayer { get; set; }
        public List<Neuron> MirrorLayer { get; set; }
        public List<Neuron> CanonicalLayer { get; set; }
        public IEnumerable<Synapse> Synapses { get; set; }

        private void ForwardPropagate(params double[] inputs)
        {
            var i = 0;
            InputLayer?.ForEach(x => x.Value = inputs[i++]);
            HiddenLayers?.ForEach(a => a.ForEach(x => x.CanonicalValue()));
            OutputLayer?.ForEach(x => x.CanonicalValue());
        }

        private void BackPropagate(params double[] targets)
        {
            var i = 0;
            OutputLayer?.ForEach(x => x.CalculateGradient(targets[i]));
            HiddenLayers?.Reverse();
            HiddenLayers?.ForEach(x => x.ForEach(a => a.CalculateGradient()));
            HiddenLayers?.ForEach(x => x.ForEach(a => a.UpdateWeight(LeatningRate, Momentum)));
            HiddenLayers?.Reverse();
            OutputLayer?.ForEach(x => x.UpdateWeight(LeatningRate, Momentum));
        }
    }
}
