namespace WeezeCli.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WeezeCliApp weezeCliHelper = new WeezeCliApp("");
            weezeCliHelper.Register(new Test());
            weezeCliHelper.Register(new Test2());
            if (!weezeCliHelper.ParseAndInvoke(args, out string message))
                Console.WriteLine(message);
            Console.ReadLine();

        }

        [CommandGroup("测试", asMainApp: true)]
        public class Test
        {
            [Command("Add方法")]
            public void Add(string arg1, string arg2)
            {
                Console.WriteLine($"Test.Add {arg1}, {arg2}");
            }
        }

        [CommandGroup("Test2")]
        public class Test2
        {
            [Command("Add方法")]
            public void Add(string arg1, string arg2)
            {
                Console.WriteLine("Test2.Add {arg1}, {arg2}");
            }
        }
    }
}
