using System;
using System.Collections.Generic;

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
    }
}