using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WeezeCli.Model
{
    internal class CommandExecuter
    {
        public CommandExecuter(CommandGroup commandGroup, MethodInfo methodInfo)
        {
            this.CommandGroup = commandGroup;
            this.MethodInfo = methodInfo;
        }
        public CommandGroup CommandGroup { get; }
        public object Instance => this.CommandGroup.Instance;
        public MethodInfo MethodInfo { get; }

        private bool CanExecute(CommandArg commandArg, out List<string> args)
        {
            bool canExecute = false;
            args = new List<string>();
            var parameters = this.MethodInfo.GetParameters();
            foreach (var parameter in parameters)
            {
                var cmdArgKey = commandArg.Args.Keys.FirstOrDefault(x => IsCmdArgMatchParameter(x, parameter.Name));
                string value = parameter.DefaultValue?.ToString();
                if (cmdArgKey != null)
                {
                    value = commandArg.Args[cmdArgKey];
                }
                if (string.IsNullOrEmpty(value) && parameter.DefaultValue == null)
                {
                    return canExecute;
                }
                args.Add(value);
            }
            canExecute = args.Count == parameters.Count();
            return canExecute;
        }

        private bool IsCmdArgMatchParameter(string arg, string parameter)
        {
            string cmdArg = arg.Replace("-", "").ToLower();
            return parameter.ToLower().StartsWith(cmdArg);
        }

        public bool TryExecute(CommandArg commandArg, out string message)
        {
            message = string.Empty;
            if (CanExecute(commandArg, out List<string> args))
            {
                try
                {
                    this.MethodInfo?.Invoke(this.Instance, args.ToArray());
                    return true;
                }
                catch (Exception e)
                {
                    message = e.Message;
                    return false;
                }

            }

            message = "Params matching failed";
            return false;
        }
    }
}
