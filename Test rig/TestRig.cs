using System;
using System.Threading;
using Prolog;

namespace Test_Rig
{
    class TestRig
    {
        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
#if !DEBUG
            Console.WriteLine("Test rig must be compiled with DisableUnity defined.");
            Console.WriteLine("Switch to DEBUG configuration.");
            Thread.Sleep(int.MaxValue);
#else
            Console.WriteLine("Unity Prolog console test rig");
            KnowledgeBase.Global.Consult("../../Prolog/");
            if (!KnowledgeBase.Global.IsTrue(new Structure("run_tests")))
            {
                Thread.Sleep(int.MaxValue);
            }
            Thread.Sleep(1000);
#endif
        }
    }
}
