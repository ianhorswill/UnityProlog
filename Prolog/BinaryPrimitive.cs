using System;
using System.Collections.Generic;

namespace Prolog
{
    /// <summary>
    /// General driver for user-defined binary primitives
    /// (i.e. prolog predicates written in C# by users) on atomic
    /// arguments (i.e. not structures that unification will be run on).
    /// 
    /// Supports arbitrary relations, including left/right-unique relations
    /// 
    /// CAVEATS:
    /// - Assumes arguments are atomic as far as Prolog is concerned; it
    ///   it doesn't bother doing full unification of outputs.
    /// - It does not fully support reflexive relations.  In particular,
    ///   the predicate will throw an exception if you call it with the
    ///   same unbound variaable for both arguments. 
    /// </summary>
    /// <typeparam name="T1">Type for the first argument</typeparam>
    /// <typeparam name="T2">Type for the second argument</typeparam>
    public class BinaryPrimitive<T1, T2>
    {
        /// <summary>
        /// Adds a new user-defined binary primitive predicate given
        /// C# delegates to implement its different modes.  Assumes
        /// arguments are always atomic, and that the relation won't
        /// be called with the same unbound variable for both arguments.
        /// </summary>
        /// <param name="name">Name to give to the predicate</param>
        /// <param name="documentation">Description of the operation of the predicate</param>
        /// <param name="arg1Name">Name of the first argument (for documentation and error messages)</param>
        /// <param name="arg2Name">Name of hte second argument (for documentation and error messages)</param>
        /// <param name="filter">Implementation of predicate for case where both arguments are instantiated.</param>
        /// <param name="arg1Enumerator">For non-left-unique predicates: implementation for case where only the second argument is instantiated, so the values of the first argument must be enumerated.</param>
        /// <param name="arg1Function">For left-unique predicates: implementation for the case where only the second argument is instantiated, so need to compute the value of the first (if any).</param>
        /// <param name="arg2Enumerator">For non-right-unique predicates: implementation for case where only the first argument is instantiated, so the values of the second argument must be enumerated.</param>
        /// <param name="arg2Function">For right-unique predicates: implementation for the case where only the first argument is instantiated, so need to compute the value of the second (if any)</param>
        /// <param name="doubleEnumerator">Implementation of the case where neither argument is instantiated.</param>
        public static void Declare(
            string name,
            string documentation,
            string arg1Name,
            string arg2Name,
            Filter filter,
            Arg1Enumerator arg1Enumerator = null,
            Arg1Function arg1Function = null,
            Arg2Enumerator arg2Enumerator = null,
            Arg2Function arg2Function = null,
            DoubleEnumerator doubleEnumerator = null)
        {
            PrologPrimitives.DefinePrimitive(
                Symbol.Intern(name),
                new BinaryPrimitive<T1, T2>(
                    name,
                    arg1Name,
                    arg2Name,
                    filter,
                    arg1Enumerator,
                    arg1Function,
                    arg2Enumerator,
                    arg2Function,
                    doubleEnumerator).BinaryPrimitiveImplementation,
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
            Arg1Function arg1FunctionImplementation,
            Arg2Enumerator arg2EnumeratorImplementation,
            Arg2Function arg2FunctionImplementation,
            DoubleEnumerator doubleEnumeratorImplementation)
        {
            this.Name = name;
            this.arg1Name = arg1Name;
            this.arg2Name = arg2Name;
            this.expectedArguments = new object[] { arg1Name, arg2Name };
            filter = filterImplementation;
            arg1Enumerator = arg1EnumeratorImplementation;
            arg1Function = arg1FunctionImplementation;
            arg2Enumerator = arg2EnumeratorImplementation;
            arg2Function = arg2FunctionImplementation;
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
            var arg1AsLogicVariable = arg1 as LogicVariable;
            if (arg1AsLogicVariable != null)
            {
                var arg2AsLogicVariable = arg2 as LogicVariable;
                if (arg2AsLogicVariable != null)
                {
                    // Enumerating both arguments
                    if (doubleEnumerator == null)
                        throw new InstantiationException(
                            arg1AsLogicVariable,
                            "At least one argument to "+Name+"/2 must be instantiated.");
                    if (arg1AsLogicVariable == arg2AsLogicVariable)
                        throw new InvalidOperationException(Name + "(X,X) is not supported.");
                    return EnumerateBothDriver(arg1AsLogicVariable, arg2AsLogicVariable, context);
                }
                if (!(arg2 is T2))
                    throw new ArgumentTypeException(this.Name, this.arg2Name, arg1, typeof(T1));

                if (arg1Function != null)
                {
                    // It's left-unique
                    T1 arg1Value;
                    if (arg1Function((T2)arg2, out arg1Value))
                        return Term.UnifyAndReturnCutState(arg1, arg1Value);
                    return PrologPrimitives.FailImplementation;
                }
                // It's not left-unique
                if (arg1Enumerator == null)
                    throw new InstantiationException(arg1AsLogicVariable, Name + "/2: first argument must be instantiated.");
                return this.EnumerateArg1Driver(arg1AsLogicVariable, (T2)arg2, context);
            }

            var variable1 = arg2 as LogicVariable;
            if (variable1 != null)
            {
                if (!(arg1 is T1))
                    throw new ArgumentTypeException(this.Name, this.arg1Name, arg1, typeof(T1));

                if (arg2Function != null)
                {
                    // It's right-unique
                    T2 arg2Value;
                    if (arg2Function((T1)arg1, out arg2Value))
                        return Term.UnifyAndReturnCutState(arg2, arg2Value);
                    return PrologPrimitives.FailImplementation;
                }
                // It's not right-unqiue
                if (arg2Enumerator == null)
                    throw new InstantiationException(variable1, Name + "/2: second argument must be instantiated.");
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
            int tracePointer = context.MarkTrail();
            foreach (var newValue in arg2Enumerator(arg1))
            {
                arg2.UnifyWithAtomicConstant(newValue, context);

                yield return CutState.Continue;

                context.RestoreVariables(tracePointer);
            }
        }

        private IEnumerable<CutState> EnumerateArg1Driver(LogicVariable arg1, T2 arg2, PrologContext context)
        {
            int tracePointer = context.MarkTrail();
            foreach (var newValue in arg1Enumerator(arg2))
            {
                arg1.UnifyWithAtomicConstant(newValue, context);

                yield return CutState.Continue;

                context.RestoreVariables(tracePointer);
            }
        }

        private IEnumerable<CutState> EnumerateBothDriver(LogicVariable arg1, LogicVariable arg2, PrologContext context)
        {
            int tracePointer = context.MarkTrail();
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

        private readonly Arg1Function arg1Function;

        private readonly Arg2Enumerator arg2Enumerator;

        private readonly Arg2Function arg2Function;

        private readonly DoubleEnumerator doubleEnumerator;

        private readonly object[] expectedArguments;

        #endregion

        #region Type definitions

        public delegate bool Filter(T1 arg1, T2 arg2);

        public delegate IEnumerable<T1> Arg1Enumerator(T2 arg2);

        public delegate bool Arg1Function(T2 arg2, out T1 arg1);

        public delegate IEnumerable<T2> Arg2Enumerator(T1 arg1);

        public delegate bool Arg2Function(T1 arg1, out T2 arg2);

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
    //    public static void DeclareTestPrimitives()
    //    {
    //        BinaryPrimitive<int, int>.Declare(
    //            "even_sum",
    //            "True if arguments sum to and even number",
    //            "int1",
    //            "int2",
    //            // ReSharper disable once RedundantArgumentName
    //            filter: (a, b) => ((a + b) % 2 == 0),
    //            // ReSharper disable once RedundantArgumentName
    //            arg1Enumerator: EnumerateOne,
    //            arg2Enumerator: EnumerateOne,
    //            doubleEnumerator: EnumerateBoth);

    //        BinaryPrimitive<int, int>.Declare(
    //            "double",
    //            "True if Y is 2*X",
    //            "X",
    //            "Y",
    //            filter: (x, y) => y == 2*x,
    //            arg2Function: (int x, out int y) => { y = 2 * x; return true; },
    //            arg1Function: (int y, out int x) =>
    //            {
    //                if ((y & 1) == 0)
    //                {
    //                    x = y / 2;
    //                    return true;
    //                }
    //                x = 0;
    //                return false;
    //            });
    //    }

    //    static IEnumerable<int> EnumerateOne(int other)
    //    {
    //        int val = (other % 2 == 0) ? 0 : 1;
    //        while (true)
    //        {
    //            yield return val;
    //            val += 2;
    //        }
    //        // ReSharper disable once FunctionNeverReturns
    //    }

    //    static IEnumerable<BinaryPrimitive<int, int>.ValuePair> EnumerateBoth()
    //    {
    //        int val = 0;
    //        while (true)
    //        {
    //            yield return new BinaryPrimitive<int, int>.ValuePair(val, -val);
    //            val += 1;
    //        }
    //        // ReSharper disable once FunctionNeverReturns
    //    }
    //}
}
