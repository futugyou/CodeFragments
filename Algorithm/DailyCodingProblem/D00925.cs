using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyCodingProblem
{
    /// <summary>
    /// Write a function to flatten a nested dictionary. Namespace the keys with a period.
    /// For example, given the following dictionary:
    ///{
    ///   "key": 3,
    ///    "foo": {
    ///        "a": 5,
    ///       "bar": {
    ///           "baz": 8
    ///       }
    ///    }
    ///}
    ///it should become:
    ///{
    ///   "key": 3,
    ///   "foo.a": 5,
    ///    "foo.bar.baz": 8
    ///}
    ///You can assume keys do not contain dots in them, i.e. no clobbering will occur.
    /// </summary>
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
