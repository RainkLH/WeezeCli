using WeezeCli;

namespace WeezeCliTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WeezeCliApp weezeCliHelper = new WeezeCliApp("");
            weezeCliHelper.Register(new Test());
            if(!weezeCliHelper.ParseAndInvoke(args, out string message))
                Console.WriteLine(message);
            Console.ReadLine();
        }
    }

    [CommandGroup("测试")]
    public class Test
    {
        [Command("Main方法")]
        public void Main(string args)
        {
            Console.WriteLine("Hello, World!");
        }

        [Command("Add方法")]
        public void Add(string arg1, string arg2)
        {
            Console.WriteLine("Hello, World!");
        }

        [Command("SayHi方法")]
        public void SayHi(string name)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
