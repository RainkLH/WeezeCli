using System;
using System.Collections.Generic;
using System.Text;
using WeezeCli.Model;

namespace WeezeCli
{
    internal static class Config
    {
        internal static InternalArg HelpArg = new InternalArg("--help", "-h");
        internal static InternalArg InfoArg = new InternalArg("--info", "-i");
    }
}
