using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    public class D00925
    {
        private class Dic
        {
            public string Value { get; set; } = "";
            public Dictionary<string, Dic> Inner { get; set; }
        }

        public static void Exection()
        {
            var dic = new Dictionary<string, Dic>
            {
                { "key", new Dic { Value = "3" } },
                {
                    "foo",
                    new Dic
                    {
                        Inner = new Dictionary<string, Dic> {
                            { "a", new Dic { Value = "5" } },
                            { "bar", new Dic { Inner = new Dictionary<string, Dic> {
                                { "bar", new Dic { Value = "8" } } } } } }
                    }
                }
            };
            var dic2 = new Dictionary<string, string>();
            AddItem("", dic, dic2);
        }

        private static void AddItem(string pkey, Dictionary<string, Dic> pairs, Dictionary<string, string> dics)
        {
            foreach (var item in pairs)
            {
                var key = string.IsNullOrEmpty(pkey) ? item.Key : (pkey + "." + item.Key);
                var value = item.Value.Value;
                var subdic = item.Value.Inner;
                if (string.IsNullOrEmpty(value))
                {
                    AddItem(key, subdic, dics);
                }
                else
                {
                    dics.Add(key, value);
                }
            }
        }
    }
}
