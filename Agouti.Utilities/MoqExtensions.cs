using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using Moq.Language;
using Moq.Language.Flow;
using NUnit.Framework;

namespace Agouti.Utilities.Extensions
{
    public static class MoqExtensions
    {

        /// <summary>
        /// Ensure that the method returns the right values in the correct order
        /// </summary>
        /// <typeparam name="T">The type of object being mocked</typeparam>
        /// <typeparam name="TResult">The type of result expected</typeparam>
        /// <param name="setup">The Moq setup object being extended</param>
        /// <param name="results">A sequence of results that will be expected in order</param>
        public static IReturnsResult<T> ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup, params TResult[] results) where T : class
        {
            return setup.Returns(new Queue<TResult>(results).Dequeue);
        }

        /*
        public static IReturnsResult<TProp> ReturnsDummy<TProp, TResult>(this ISetup<TProp, TResult> setup)
            where TProp : class
            where TResult : class
        {
            return setup.Returns(new Mock<TResult>().Object);
        }
        */

        /// <summary>
        /// Ensure that this method is invoked at the proper point in the sequence
        /// </summary>
        /// <typeparam name="T">The type of object being mocked</typeparam>
        /// <param name="setup">The Moq setup object being extended</param>
        /// <param name="seq">A sequence object representing a sequence</param>
        /// <param name="order">Any number of integer positions (starting 0)</param>
        public static ICallbackResult CalledInSequence<T>(this ISetup<T> setup, Sequence seq, params int[] order) where T : class
        {
            var queue = new Queue<int>(order);
            return setup.Callback(() => seq.ShouldBe(queue.Dequeue()));
        }

        /// <summary>
        /// Ensure that this method is invoked at the proper point in the sequence
        /// </summary>
        /// <param name="setup">The Moq setup object being extended</param>
        /// <param name="seq">A sequence object representing a sequence</param>
        /// <param name="order">Any number of integer positions (starting 0)</param>
        public static ICallbackResult CalledInSequence(this ICallback setup, Sequence seq, params int[] order)
        {
            var queue = new Queue<int>(order);
            return setup.Callback(() => seq.ShouldBe(queue.Dequeue()));
        }



        /// <summary>
        /// Handy dandy String.Format attached to a string
        /// </summary>
        /// <param name="str">A string</param>
        /// <param name="args">Format args</param>
        /// <returns>A formatted string</returns>
        public static string F(this string str, params object[] args)
        {
            return String.Format(str, args);
        }


    }

    //mockCriteria.Setup(o => o.SetFetchMode(It.Is<Expression<Func<Supplier, object>>>(c => ExpressionMatches(c, x => x.Email))));
    //mockCriteria.Setup(o => o.SetFetchMode(It.IsLambda<Supplier>( x => x.Email)));



    public static class ItIs
    {
        public static Expression<Func<T, object>> Lambda<T>(Expression<Func<T, object>> match) where T : class
        {
            return It.Is<Expression<Func<T, object>>>(c => ExpressionMatches(c, match));
        }

        public static bool ExpressionMatches<T>(Expression<Func<T, object>> one, Expression<Func<T, object>> two)
        {
            var oneB = one.Body as MemberExpression;
            var twoB = two.Body as MemberExpression;

            if ((oneB != null) && (twoB != null))
            {
                return oneB.Member == twoB.Member;
            }
            return false;
        }

    }




    /// <summary>
    /// A sequence of numbers used for making sure Moq methods are executed in the correct order
    /// </summary>
    public class Sequence
    {
        /// <summary>
        /// The position of the counter
        /// </summary>
        public int Counter { get; set; }

        public void ShouldBe(int number)
        {
            Assert.That(Counter, Is.EqualTo(number), "Called out of sequence");
            Counter++;
        }
    }
}
