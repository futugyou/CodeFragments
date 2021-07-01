using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CodeFragments
{
    public class JsonNodeDemo
    {
        public static void Test()
        {

            // Parse a JSON object
            JsonNode jNode = JsonNode.Parse("{\"MyProperty\":42}");
            int value = (int)jNode["MyProperty"];
            Debug.Assert(value == 42);
            // or
            value = jNode["MyProperty"].GetValue<int>();
            Debug.Assert(value == 42);

            // Parse a JSON array
            jNode = JsonNode.Parse("[10,11,12]");
            value = (int)jNode[1];
            Debug.Assert(value == 11);
            // or
            value = jNode[1].GetValue<int>();
            Debug.Assert(value == 11);

            // Create a new JsonObject using object initializers and array params
            var jObject = new JsonObject
            {
                ["MyChildObject"] = new JsonObject
                {
                    ["MyProperty"] = "Hello",
                    ["MyArray"] = new JsonArray(10, 11, 12)
                }
            };

            // Obtain the JSON from the new JsonObject
            string json = jObject.ToJsonString();
            Console.WriteLine(json); // {"MyChildObject":{"MyProperty":"Hello","MyArray":[10,11,12]}}

            // Indexers for property names and array elements are supported and can be chained
            Debug.Assert(jObject["MyChildObject"]["MyArray"][1].GetValue<int>() == 11);
        }
    }
}
