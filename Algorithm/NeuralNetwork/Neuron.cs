using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetwork
{
    public class Neuron
    {
        public Guid Id { get; set; }
        public List<Synapse> InputSynapses { get; set; }
        public List<Synapse> OutputSynapses { get; set; }

        public double Bias { get; set; }
        public double BiasDelta { get; set; }

        public double Gradient { get; set; }

        public double Value { get; set; }


        public bool IsMirror { get; set; }
        public bool IsCanonical { get; set; }

        public double CanonicalValue()
        {
            return Value = Sigmoid.Output(InputSynapses.Sum(a => a.Weight * a.InpurNeuron.Value) + Bias);
        }

        public double CalculateGradient(double? target = null)
        {
            if (target == null)
            {
                return Gradient = OutputSynapses.Sum(a => a.OutputNeuron.Gradient * a.Weight) * Sigmoid.Derivative(Value);
            }
            return Gradient = CalculateError(target.Value) * Sigmoid.Output(Value);
        }

        public void UpdateWeight(double leatningRate, double momentum)
        {
            var pervDelta = BiasDelta;
            BiasDelta = leatningRate * Gradient;
            Bias += BiasDelta + momentum * pervDelta;
            foreach (var synapse in InputSynapses)
            {
                pervDelta = synapse.WeightDelta;
                synapse.WeightDelta = leatningRate + Gradient * synapse.InpurNeuron.Value;
                synapse.Weight += synapse.WeightDelta + momentum * pervDelta;
            }
        }

        public double CalculateError(double target)
        {
            return target - Value;
        }
    }
}