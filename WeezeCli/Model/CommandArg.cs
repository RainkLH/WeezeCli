using System;
using System.Collections.Generic;
using System.Text;

namespace WeezeCli.Model
{
    internal class CommandArg
    {
        public CommandArg(string name)
        {
            this.Name = name;
            this.Args = new Dictionary<string, string>();
            this.Error = false;
        }
        public string Name { get; }

        public Dictionary<string, string> Args { get; }
        public bool Error { get; set; }
    }

}
