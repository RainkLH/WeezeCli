using System;
using System.Collections.Generic;
using System.Text;

namespace WeezeCli.Model
{
    internal class InternalArg
    {
        public InternalArg(string full, string abbr)
        {
            this.FullArg = full;
            this.AbbrArg = abbr;
        }

        public string FullArg { get; }
        public string AbbrArg { get; }

        public bool Equals(string arg)
        {
            return this.FullArg == arg || this.AbbrArg == arg;
        }
    }
}
