#region Copyright
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnityExtensionMethods.cs" company="Ian Horswill">
// Copyright (C) 2015 Ian Horswill
//  
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
//  
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion

using System;

using UnityEngine;

namespace Prolog
{
    public static class UnityExtensionMethods
    {
        /// <summary>
    /// Returns the parent GameObject of this GameObject
    /// </summary>
    /// <param name="o">The GameObject to get the parent of</param>
    /// <returns>The parent GameObject</returns>
    public static GameObject GetParent(this GameObject o)
    {
        return o.transform.parent.gameObject;
    }

        /// <summary>
        /// Gets the KnowledgeBase attached to this component's GameObject.
        /// </summary>
        /// <returns>The KnowledgeBase attached to component's GameObjec</returns>
        public static KnowledgeBase KnowledgeBase(this Component component)
        {
            return component.GetComponent<KB>().KnowledgeBase;
        }

        /// <summary>
        /// Gets the KnowledgeBase attached to this GameObject.
        /// </summary>
        /// <returns>The KnowledgeBase attached to component's GameObjec</returns>
        public static KnowledgeBase KnowledgeBase(this GameObject gameObject)
        {
            return gameObject.GetComponent<KB>().KnowledgeBase;
        }

        /// <summary>
        /// Adds ASSERTION to GAMEOBJECT's knowledgebase.
        /// </summary>
        /// <param name="gameObject">Object whose knowledge base it should be added to</param>
        /// <param name="assertion">Assertion to add</param>
        public static void Assert(this GameObject gameObject, Structure assertion)
        {
            gameObject.KnowledgeBase().AssertZ(assertion);
        }

        /// <summary>
        /// Adds assertion to GAMEOBJECT's knowledgebase.
        /// </summary>
        /// <param name="gameObject">Object whose knowledge base it should be added to</param>
        /// <param name="functor">Functor (i.e. predicate) of the assertion</param>
        /// <param name="args">Arguments to the functor</param>
        public static void Assert(this GameObject gameObject, Symbol functor, params object[] args)
        {
            gameObject.KnowledgeBase().AssertZ(new Structure(functor, args));
        }

        /// <summary>
        /// Adds assertion to GAMEOBJECT's knowledgebase.
        /// </summary>
        /// <param name="gameObject">Object whose knowledge base it should be added to</param>
        /// <param name="functor">Functor (i.e. predicate) of the assertion</param>
        /// <param name="args">Arguments to the functor</param>
        public static void Assert(this GameObject gameObject, string functor, params object[] args)
        {
            gameObject.KnowledgeBase().AssertZ(new Structure(functor, args));
        }

        /// <summary>
        /// Adds ASSERTION to COMPONENT's knowledgebase.
        /// </summary>
        /// <param name="component">Object whose knowledge base it should be added to</param>
        /// <param name="assertion">Assertion to add</param>
        public static void Assert(this Component component, Structure assertion)
        {
            component.KnowledgeBase().AssertZ(assertion);
        }

        /// <summary>
        /// Adds assertion to component's knowledgebase.
        /// </summary>
        /// <param name="component">Object whose knowledge base it should be added to</param>
        /// <param name="functor">Functor (i.e. predicate) of the assertion</param>
        /// <param name="args">Arguments to the functor</param>
        public static void Assert(this Component component, Symbol functor, params object[] args)
        {
            component.KnowledgeBase().AssertZ(new Structure(functor, args));
        }

        /// <summary>
        /// Adds assertion to component's knowledgebase.
        /// </summary>
        /// <param name="component">Object whose knowledge base it should be added to</param>
        /// <param name="functor">Functor (i.e. predicate) of the assertion</param>
        /// <param name="args">Arguments to the functor</param>
        public static void Assert(this Component component, string functor, params object[] args)
        {
            component.KnowledgeBase().AssertZ(new Structure(functor, args));
        }

        /// <summary>
        /// True if goal is provable within this GameObject's knowledge base.
        /// </summary>
        /// <param name="gameObject">GameObject whose KB should be queried.</param>
        /// <param name="goal">Term to try to prove.</param>
        /// <returns>True if the goal is provable.</returns>
        public static bool IsTrue(this GameObject gameObject, object goal)
        {
            return gameObject.KnowledgeBase().IsTrue(goal, gameObject);
        }

        /// <summary>
        /// True if functor(args) is provable within this GameObject's knowledge base.
        /// </summary>
        /// <param name="gameObject">GameObject whose KB should be queried.</param>
        /// <param name="functor">Functor of the goal</param>
        /// <param name="args">Arguments of the goal</param>
        /// <returns></returns>
        public static bool IsTrue(this GameObject gameObject, string functor, params object[] args)
        {
            return gameObject.KnowledgeBase().IsTrue(new Structure(functor, args), gameObject);
        }

        /// <summary>
        /// True if functor(args) is provable within this Component's GameObject's knowledge base.
        /// </summary>
        /// <param name="component">GameObject whose KB should be queried.</param>
        /// <param name="functor">Functor of the goal</param>
        /// <param name="args">Arguments of the goal</param>
        /// <returns></returns>
        public static bool IsTrue(this Component component, string functor, params object[] args)
        {
            return component.KnowledgeBase().IsTrue(new Structure(functor, args), component);
        }

        /// <summary>
        /// True if goal is provable within this Component's GameObject's knowledge base.
        /// </summary>
        /// <param name="component">GameObject whose KB should be queried.</param>
        /// <param name="goal">Term to try to prove.</param>
        /// <returns>True if the goal is provable.</returns>
        public static bool IsTrue(this Component component, object goal)
        {
            return component.KnowledgeBase().IsTrue(goal, component);
        }

        /// <summary>
        /// True if goal is provable within this GameObject's knowledge base.
        /// </summary>
        /// <param name="gameObject">GameObject whose KB should be queried.</param>
        /// <param name="goal">Term to try to prove.</param>
        /// <returns>True if the goal is provable.</returns>
        public static bool IsTrueParsed(this GameObject gameObject, string goal)
        {
            return gameObject.KnowledgeBase().IsTrue(ISOPrologReader.Read(goal), gameObject);
        }

        /// <summary>
        /// True if goal is provable within this Component's GameObject's knowledge base.
        /// </summary>
        /// <param name="component">GameObject whose KB should be queried.</param>
        /// <param name="goal">Term to try to prove.</param>
        /// <returns>True if the goal is provable.</returns>
        public static bool IsTrueParsed(this Component component, string goal)
        {
            return component.KnowledgeBase().IsTrue(ISOPrologReader.Read(goal), component);
        }

        /// <summary>
        /// Finds the value of result in the first solution when proving goal within this GameObject's knowledge base.
        /// </summary>
        /// <param name="gameObject">GameObject whose KB should be queried.</param>
        /// <param name="result">Value of the variable to solve for</param>
        /// <param name="goal">Constraint on the value of the variable</param>
        /// <returns>Value found for the variable.</returns>
        public static object SolveFor(this GameObject gameObject, LogicVariable result, object goal)
        {
            return gameObject.KnowledgeBase().SolveFor(result, goal, gameObject, gameObject);
        }

        /// <summary>
        /// Finds the value of result in the first solution when proving goal within this Component's GameObject's knowledge base.
        /// </summary>
        /// <param name="gameObject">GameObject whose KB should be queried.</param>
        /// <param name="result">Value of the variable to solve for</param>
        /// <param name="functor">Functor of the goal.</param>
        /// <param name="args">Arguments for the goal.</param>
        /// <returns>Value found for the variable.</returns>
        public static object SolveFor(this GameObject gameObject, LogicVariable result, string functor, params object[] args)
        {
            return gameObject.SolveFor(result, new Structure(functor, args));
        }

        /// <summary>
        /// Given argument Variable:Constraint, finds the value of Variable in the first solution to Constraint when proved against this GameObject's knowledge base.
        /// </summary>
        /// <param name="gameObject">GameObject whose KB should be queried.</param>
        /// <param name="variableAndConstraint">String of the form "Variable:Constraint"</param>
        /// <returns>Value found for the variable.</returns>
        public static object SolveForParsed(this GameObject gameObject, string variableAndConstraint)
        {
            var colonExpression = ISOPrologReader.Read(variableAndConstraint) as Structure;
            if (colonExpression == null || !colonExpression.IsFunctor(Symbol.Colon, 2))
                throw new ArgumentException("Arguent to SolveFor(string) must be of the form Var:Goal.");
            return gameObject.SolveFor((LogicVariable)colonExpression.Argument(0), colonExpression.Argument(1));

        }

        /// <summary>
        /// Finds the value of result in the first solution when proving goal within this Component's GameObject's knowledge base.
        /// </summary>
        /// <param name="component">Component whose KB should be queried.</param>
        /// <param name="result">Value of the variable to solve for</param>
        /// <param name="goal">Constraint on the value of the variable</param>
        /// <returns>Value found for the variable.</returns>
        public static object SolveFor(this Component component, LogicVariable result, object goal)
        {
            return component.KnowledgeBase().SolveFor(result, goal, component, component.gameObject);
        }

        /// <summary>
        /// Finds the value of result in the first solution when proving goal within this Component's GameObject's knowledge base.
        /// </summary>
        /// <param name="component">Component whose KB should be queried.</param>
        /// <param name="result">Value of the variable to solve for</param>
        /// <param name="functor">Functor of the goal.</param>
        /// <param name="args">Arguments for the goal.</param>
        /// <returns>Value found for the variable.</returns>
        public static object SolveFor(this Component component, LogicVariable result, string functor, params object[] args)
        {
            return component.SolveFor(result, new Structure(functor, args));
        }

        /// <summary>
        /// Given argument Variable:Constraint, finds the value of Variable in the first solution to Constraint when proved against this Component's GameObject's knowledge base.
        /// </summary>
        /// <param name="component">Component whose KB should be queried.</param>
        /// <param name="variableAndConstraint">String of the form "Variable:Constraint"</param>
        /// <returns>Value found for the variable.</returns>
        public static object SolveForParsed(this Component component, string variableAndConstraint)
        {
            var colonExpression = ISOPrologReader.Read(variableAndConstraint) as Structure;
            if (colonExpression == null || !colonExpression.IsFunctor(Symbol.Colon, 2))
                throw new ArgumentException("Arguent to SolveFor(string) must be of the form Var:Goal.");
            return component.SolveFor((LogicVariable)colonExpression.Argument(0), colonExpression.Argument(1));

        }
    }
}
