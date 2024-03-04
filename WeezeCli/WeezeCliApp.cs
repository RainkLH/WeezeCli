using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeezeCli.Model;

namespace WeezeCli
{
    public class WeezeCliApp
    {
        private string appName;
        private string description;
        private Dictionary<string, CommandGroup> cmdGroups;

        public WeezeCliApp(string appName, string description = "")
        {
            if (string.IsNullOrEmpty(appName))
                throw new ArgumentNullException("The application must have a name. ");
            this.appName = appName;
            this.description = description ?? "";
            this.cmdGroups = new Dictionary<string, CommandGroup>();
        }

        public void Register<T>(T instance) where T : class
        {
            Type type = typeof(T);
            //step1: check attribute
            CommandGroupAttribute groupAttribute = type.GetCustomAttribute<CommandGroupAttribute>();
            if (groupAttribute == null)
            {
                throw new ArgumentException($"Can`t find CommandGroupAttribute config on {nameof(instance)}. ");
            }
            //step2: check is existed
            string key = groupAttribute.AsMainApp ? "" : type.Name.ToLower();
            if (cmdGroups.ContainsKey(key))
            {
                if (key == "")
                {
                    throw new ArgumentException($"Only one main application can be registered. ");
                }
                else
                {
                    throw new ArgumentException($"Application: [{key}] is already existed. ");
                }
            }
            //step3 chek commands
            var methods = instance.GetType().GetMethods();
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<CommandAttribute>() != null)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Any(x => x.ParameterType != typeof(string)))
                    {
                        throw new ArgumentOutOfRangeException("Only variables of type string are supported as command parameters. ");
                    }
                }                
            }
            cmdGroups[key] = new CommandGroup(key, groupAttribute.Description, instance);
            //step4: check main entrance
            if (!cmdGroups.ContainsKey(""))
            {
                throw new ArgumentException($"Please regist main application first. ");
            }
        }

        public bool ParseAndInvoke(string[] args, out string message)
        {
            message = string.Empty;
            try
            {
                if (args.Length == 0 || Config.HelpArg.Equals(args[0]))
                {
                    message = GetAppHelperMessage();
                    return false;
                }

                if (Config.InfoArg.Equals(args[0]))
                {
                    message = $"Name: {this.appName}\r\nDescription:{this.description}";
                    return false;
                }

                if (args.Length >= 1 && cmdGroups.ContainsKey(args[0]))
                {
                    if (args.Length == 1 || (args.Length > 1 && Config.HelpArg.Equals(args[1])))
                    {
                        message = GetCmdHelperMessage(cmdGroups[args[0]], null);
                        return false;
                    }
                }

                CommandGroup commandGroup = GetCmdGroup(args, out int splitIndex);
                string callerName = string.Join(" ", args.Take(splitIndex));
                if (commandGroup == null)
                {
                    message = $"No executable command found matching '{callerName}'. ";
                    return false;
                }

                string cmdName = args[splitIndex - 1];
                CommandExecuter executer = GetMethod(commandGroup, cmdName);
                if (executer == null)
                {
                    message = $"No executable command found matching '{cmdName}'. ";
                    return false;
                }
                CommandArg commandArg = new CommandArg(callerName);
                ParseArgs(args, splitIndex, ref commandArg);
                if (commandArg.Error)
                {
                    if (args.Length == splitIndex 
                        || (args.Length > splitIndex && Config.HelpArg.Equals(args[splitIndex + 1])))
                    {
                        message = GetCmdHelperMessage(commandGroup, executer.MethodInfo);
                    }
                    else
                    {
                        message = "Failed to parse the input parameters. ";
                    }
                    return false;
                }

                if (!executer.TryExecute(commandArg, out message))
                {
                    message = message + "\r\n" + GetCmdHelperMessage(commandGroup, executer.MethodInfo);
                }
            }
            catch (Exception err)
            {
                message = err.Message;
                return false;
            }

            return true;
        }

        private CommandGroup GetCmdGroup(string[] args, out int splitIndex)
        {
            string group = "";
            string cmdName = args[0].ToLower();
            splitIndex = 1;
            if (args.Length > 1 && !args[1].StartsWith("-") && !double.TryParse(args[1], out _))
            {
                group = args[0].ToLower();
                cmdName = args[1].ToLower();
                splitIndex = 2;
            }
            if (string.IsNullOrEmpty(cmdName))
            {
                return null;
            }            
            if (!cmdGroups.ContainsKey(group))
            {
                return null;
            }            
            return cmdGroups[group];
        }

        private CommandExecuter GetMethod(CommandGroup commandGroup, string cmdName)
        {
            var instance = commandGroup.Instance;
            var methods = instance.GetType().GetMethods();
            var method = methods.FirstOrDefault(x => x.GetCustomAttribute<CommandAttribute>() != null && x.Name.ToLower() == cmdName);
            if (method is null)
                return null;

            return new CommandExecuter(commandGroup, method);
        }

        private void ParseArgs(string[] args, int splitIndex, ref CommandArg commandArg)
        {
            if (args.Length > splitIndex)
            {
                string paramKey = args[splitIndex];
                List<string> paramValues = new List<string>();
                for (int i = splitIndex + 1; i < args.Length; i++)
                {
                    var arg = args[i];
                    if (arg.StartsWith("-") && !double.TryParse(arg, out _))
                    {
                        if (commandArg.Args.ContainsKey(paramKey))
                            commandArg.Error = true;
                        else
                            commandArg.Args[paramKey] = string.Join("", paramValues);
                        paramValues.Clear();
                        paramKey = arg;
                    }
                    else
                    {
                        paramValues.Add(arg);
                    }
                }
                if (!string.IsNullOrEmpty(paramKey))
                {
                    if (commandArg.Args.ContainsKey(paramKey))
                        commandArg.Error = true;
                    else
                        commandArg.Args[paramKey] = string.Join(" ", paramValues);
                }
            }
            else
            {
                commandArg.Error = true;
            }
        }

        public string GetAppHelperMessage()
        {
            List<string> messages = new List<string>();
            foreach (var groupKey in cmdGroups.Keys)
            {
                CommandGroup group = cmdGroups[groupKey];
                if(string.IsNullOrEmpty(group.Name))
                    messages.Add($"{this.appName} Commands: ");
                else
                    messages.Add($"{group.Name} Commands: ");

                var instance = group.Instance;
                var methods = instance.GetType().GetMethods();
                foreach (var method in methods)
                {
                    if (method.GetCustomAttribute<CommandAttribute>() is CommandAttribute command)
                    {
                        messages.Add($"  {method.Name}: {command.Description}");
                    }
                }
            }

            return string.Join("\r\n", messages);
        }

        private string GetCmdHelperMessage(CommandGroup group, MethodInfo methodInfo)
        {
            List<string> messages = new List<string>();
            if (methodInfo == null)
            {
                if (string.IsNullOrEmpty(group.Name))
                    messages.Add($"{this.appName}({group.Description})");
                else
                    messages.Add($"{group.Name}({group.Description})");
                messages.Add($"Commands：");
                var instance = group.Instance;
                var methods = instance.GetType().GetMethods();
                foreach (var method in methods)
                {
                    if (method.GetCustomAttribute<CommandAttribute>() is CommandAttribute command)
                    {
                        messages.Add($"  {method.Name}: {command.Description}");
                    }
                }
            }
            else
            {
                messages.Add("Description:");
                messages.Add("  " + methodInfo.GetCustomAttribute<CommandAttribute>().Description);
                messages.Add("");

                messages.Add("Usage:");
                string usage = "  " + group.Name.ToLower() + " " + methodInfo.Name.ToLower();
                var parameters = methodInfo.GetParameters();
                foreach (var item in parameters)
                {
                    usage += " " + $"--{item.Name.ToLower()}" + " " + $"[{item.Name.ToUpper()}]";
                }
                messages.Add(usage);
                messages.Add("");
            }

            return string.Join("\r\n", messages);
        }

    }
}
