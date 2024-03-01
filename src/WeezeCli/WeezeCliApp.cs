using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WeezeCli.Model;

namespace WeezeCli
{
    public class WeezeCliApp
    {
        private string appInfo;
        private Dictionary<string, CommandGroup> cmdGroups;

        public WeezeCliApp(string appInfo)
        {
            this.appInfo = appInfo ?? "";
            this.cmdGroups = new Dictionary<string, CommandGroup>();
        }

        public void Register<T>(T instance) where T : class
        {
            Type type = typeof(T);
            CommandGroupAttribute groupAttribute = type.GetCustomAttribute<CommandGroupAttribute>();
            if (groupAttribute == null)
            {
                throw new ArgumentException($"Can`t find CommandGroupAttribute config on {nameof(instance)}. ");
            }

            string key = groupAttribute.Name.ToLower();
            if (cmdGroups.ContainsKey(key))
            {
                throw new ArgumentException($"Command group {key} is already existed. ");
            }
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
                    message = this.appInfo;
                    return false;
                }

                string callerName = args[0];
                CommandExecuter executer = GetMethod(callerName);
                if (executer == null)
                {
                    message = $"No executable command found matching '{callerName}'. ";
                    return false;
                }
                CommandArg commandArg = new CommandArg(callerName);
                ParseArgs(args, ref commandArg);
                if (commandArg.Error)
                {
                    if (args.Length > 0 && Config.HelpArg.Equals(args[1]))
                    {
                        message = GetCmdHelperMessage(commandArg.Name, executer.MethodInfo);
                    }
                    else
                    {
                        message = "Failed to parse the input parameters. ";
                    }
                    return false;
                }

                if (!executer.TryExecute(commandArg, out message))
                {
                    message = message + "\r\n" + GetCmdHelperMessage(commandArg.Name, executer.MethodInfo);
                }
            }
            catch (Exception err)
            {
                message = err.Message;
                return false;
            }

            return true;
        }

        private CommandExecuter GetMethod(string callerName)
        {
            int spliterCount = callerName.Count(x => x == '.');
            if (spliterCount > 1)
            {
                return null;
            }
            string groupKey = Config.DefaultGroupName.ToLower();
            string cmdKey = callerName.ToLower();
            if (spliterCount == 1)
            {
                var fullName = callerName.Split('.');
                groupKey = fullName[0].ToLower();
                cmdKey = fullName[1].ToLower();
            }
            if (string.IsNullOrEmpty(groupKey) || string.IsNullOrEmpty(cmdKey))
            {
                return null;
            }
            if (!cmdGroups.ContainsKey(groupKey))
            {
                return null;
            }
            var group = cmdGroups[groupKey];

            var instance = group.Instance;
            var methods = instance.GetType().GetMethods();
            var method = methods.FirstOrDefault(x => x.GetCustomAttribute<CommandAttribute>() != null && x.Name.ToLower() == cmdKey);
            if (method is null)
                return null;

            return new CommandExecuter(group, method);
        }

        private void ParseArgs(string[] args, ref CommandArg commandArg)
        {
            if (args.Length > 2)
            {
                string paramKey = args[1];
                List<string> paramValues = new List<string>();
                for (int i = 2; i < args.Length; i++)
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
                messages.Add($"{group.Name}: {group.Description}");

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

        private string GetCmdHelperMessage(string name, MethodInfo methodInfo)
        {
            List<string> message = new List<string>();
            message.Add("Description:");
            message.Add("  " + methodInfo.GetCustomAttribute<CommandAttribute>().Description);
            message.Add("");

            message.Add("Usage:");
            string usage = "  " + name;
            var parameters = methodInfo.GetParameters();
            foreach (var item in parameters)
            {
                usage += " " + $"--{item.Name.ToLower()}" + " " + $"[{item.Name.ToUpper()}]";
            }
            message.Add(usage);
            message.Add("");

            return string.Join("\r\n", message);
        }

    }
}
