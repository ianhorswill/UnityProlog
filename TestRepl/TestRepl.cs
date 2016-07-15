using System;
using System.Threading;
using Prolog;

namespace TestRepl
{
    class TestRepl
    {
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
#if !DEBUG
            Console.WriteLine("Test repl must be compiled with DisableUnity defined.");
            Console.WriteLine("Switch to DEBUG configuration.");
            Thread.Sleep(int.MaxValue);
#else
            Console.WriteLine("Unity Prolog console test rig");
            bool quit = false;
            while (!quit)
            {
                Console.Write("?- ");
                var input = Console.ReadLine();
                if (input == "quit")
                    quit = true;
                else
                    try
                    {
                        var goal = ISOPrologReader.Read(input);
                        Console.WriteLine(KnowledgeBase.Global.IsTrue(goal) ? goal : "no");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: "+e.GetType().FullName);
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }
            }
#endif
        }
    }
}
