using System;
using System.Collections.Generic;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

namespace Prolog
{
    /// <summary>
    /// General driver for user-defined binary primitives
    /// (i.e. prolog predicates written in C# by users)
    /// </summary>
    /// <typeparam name="T1">Type for the first argument</typeparam>
    /// <typeparam name="T2">Type for the second argument</typeparam>
    public class BinaryPrimitive<T1, T2>
    {
        public static void Declare(
            string name,
            string documentation,
            string arg1Name,
            string arg2Name,
            Filter filterImplementation,
            Arg1Enumerator arg1EnumeratorImplementation,
            Arg2Enumerator arg2EnumeratorImplementation,
            DoubleEnumerator doubleEnumeratorImplementation)
        {
            PrologPrimitives.DefinePrimitive(
                Symbol.Intern(name),
                new BinaryPrimitive<T1, T2>(
                    name,
                    arg1Name,
                    arg2Name,
                    filterImplementation,
                    arg1EnumeratorImplementation,
                    arg2EnumeratorImplementation,
                    doubleEnumeratorImplementation).BinaryPrimitiveImplementation,
                null,
                documentation,
                arg1Name,
                arg2Name);

        }

        #region Constructor

        private BinaryPrimitive(
            string name,
            string arg1Name,
            string arg2Name,
            Filter filterImplementation,
            Arg1Enumerator arg1EnumeratorImplementation,
            Arg2Enumerator arg2EnumeratorImplementation,
            DoubleEnumerator doubleEnumeratorImplementation)
        {
            this.Name = name;
            this.arg1Name = arg1Name;
            this.arg2Name = arg2Name;
            this.expectedArguments = new object[] { arg1Name, arg2Name };
            filter = filterImplementation;
            arg1Enumerator = arg1EnumeratorImplementation;
            arg2Enumerator = arg2EnumeratorImplementation;
            doubleEnumerator = doubleEnumeratorImplementation;
        }

        #endregion

        #region Driver loops

        private IEnumerable<CutState> BinaryPrimitiveImplementation(object[] args, PrologContext context)
        {
            if (args.Length != 2)
                throw new ArgumentCountException(Name, args, expectedArguments);
            var arg1 = Term.Deref(args[0]);
            var arg2 = Term.Deref(args[1]);
            var variable = arg1 as LogicVariable;
            if (variable != null)
            {
                var logicVariable = arg2 as LogicVariable;
                if (logicVariable != null)
                {
                    // Enumerating both arguments
                    if (doubleEnumerator == null)
                        throw new InstantiationException(
                            variable,
                            Name + ": at least one argument must be instantiated.");
                    if (variable == logicVariable)
                        throw new InvalidOperationException(Name + "(X,X) is not supported.");
                    return EnumerateBothDriver(variable, logicVariable, context);
                }
                if (!(arg2 is T2))
                    throw new ArgumentTypeException(this.Name, this.arg2Name, arg1, typeof(T1));
                if (arg1Enumerator == null)
                    throw new InstantiationException(variable, Name + ": first argument must be instantiated.");
                return this.EnumerateArg1Driver(variable, (T2)arg2, context);
            }
            var variable1 = arg2 as LogicVariable;
            if (variable1 != null)
            {
                if (!(arg1 is T1))
                    throw new ArgumentTypeException(this.Name, this.arg1Name, arg1, typeof(T1));
                if (arg2Enumerator == null)
                    throw new InstantiationException(variable1, Name + ": second argument must be instantiated.");
                return this.EnumerateArg2Driver((T1)arg1, variable1, context);
            }
            // Filtering
            if (!(arg1 is T1))
                throw new ArgumentTypeException(this.Name, this.arg1Name, arg1, typeof(T1));
            if (!(arg2 is T2))
                throw new ArgumentTypeException(this.Name, this.arg1Name, arg1, typeof(T1));
            if (filter == null)
                throw new InstantiatedVariableException(null, Name + ": at least one argument must be uninstantiated.");
            return CutStateSequencer.FromBoolean(this.filter((T1)arg1, (T2)arg2));
        }

        private IEnumerable<CutState> EnumerateArg2Driver(T1 arg1, LogicVariable arg2, PrologContext context)
        {
            int tracePointer = context.MarkTrace();
            foreach (var newValue in arg2Enumerator(arg1))
            {
                arg2.UnifyWithAtomicConstant(newValue, context);

                yield return CutState.Continue;

                context.RestoreVariables(tracePointer);
            }
        }

        private IEnumerable<CutState> EnumerateArg1Driver(LogicVariable arg1, T2 arg2, PrologContext context)
        {
            int tracePointer = context.MarkTrace();
            foreach (var newValue in arg1Enumerator(arg2))
            {
                arg1.UnifyWithAtomicConstant(newValue, context);

                yield return CutState.Continue;

                context.RestoreVariables(tracePointer);
            }
        }

        private IEnumerable<CutState> EnumerateBothDriver(LogicVariable arg1, LogicVariable arg2, PrologContext context)
        {
            int tracePointer = context.MarkTrace();
            foreach (var pair in doubleEnumerator())
            {
                arg1.UnifyWithAtomicConstant(pair.Arg1, context);
                arg2.UnifyWithAtomicConstant(pair.Arg2, context);

                yield return CutState.Continue;

                context.RestoreVariables(tracePointer);
            }
        }

        #endregion

        #region Fields

        public readonly string Name;

        private readonly string arg1Name;

        private readonly string arg2Name;

        private readonly Filter filter;

        private readonly Arg1Enumerator arg1Enumerator;

        private readonly Arg2Enumerator arg2Enumerator;

        private readonly DoubleEnumerator doubleEnumerator;

        private readonly object[] expectedArguments;

        #endregion

        #region Type definitions

        public delegate bool Filter(T1 arg1, T2 arg2);

        public delegate IEnumerable<T1> Arg1Enumerator(T2 arg2);

        public delegate IEnumerable<T2> Arg2Enumerator(T1 arg1);

        public struct ValuePair
        {
            public ValuePair(T1 arg1, T2 arg2)
            {
                Arg1 = arg1;
                Arg2 = arg2;
            }

            public T1 Arg1;

            public T2 Arg2;
        }

        public delegate IEnumerable<ValuePair> DoubleEnumerator();

        #endregion
    }

    //public static class BinaryPrimitiveExample
    //{
    //    public static void DeclareTestPrimitive()
    //    {
    //        BinaryPrimitive<int, int>.Declare(
    //            "even_sum",
    //            "True if arguments sum to and even number",
    //            "int1",
    //            "int2",
    //            (a, b) => ((a + b) % 2 == 0),
    //            EnumerateOne,
    //            EnumerateOne,
    //            EnumerateBoth);
    //    }

    //    static IEnumerable<int> EnumerateOne(int other)
    //    {
    //        int val = (other % 2 == 0) ? 0 : 1;
    //        while (true)
    //        {
    //            yield return val;
    //            val += 2;
    //        }
    //    }

    //    static IEnumerable<BinaryPrimitive<int, int>.ValuePair> EnumerateBoth()
    //    {
    //        int val = 0;
    //        while (true)
    //        {
    //            yield return new BinaryPrimitive<int, int>.ValuePair(val, -val);
    //            val += 1;
    //        }
    //    } 
    //}
}
