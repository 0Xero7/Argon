using System;
using System.Collections.Generic;
using System.Text;

namespace ArgonASTSerializer
{
    public static class JSONBeautifier
    {
        public static string GetBeautifiedJSON(string json, string tabs = "    ")
        {
            string response = "";
            string indent = "";

            return json.Replace("}", "\n}").Replace("{", "\n{\n");
            for (int i = 0; i < json.Length; i++)
            {
                string block = "";

                if (json[i] == '{')


                while (json[i] != '}')
                    block += json[i++];

                response += indent + json;
            }
        }
    }
}
