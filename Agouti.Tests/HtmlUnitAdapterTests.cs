using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Agouti;
using Agouti.Utilities.Extensions;

namespace Agouti.Tests
{
    [TestFixture]
    public class HtmlUnitAdapterTests
    {

        [TestCase(new[] { false, true }, 1)]
        [TestCase(new[] { false, true }, 10)]
        [TestCase(new[] { false, false, true }, 3)]
        public void EventualSuccessShouldPassIfThePredicateSucceedsAtLeastOnce(bool[] success, int timesToRetry)
        {
            const int MILLISECOND_DELAY = 1500;
            // ARRANGE

            var seq = new Sequence();

            var mockTime = new Mock<ITime>();
            var mockPredicate = new Mock<IPredicate<IBrowser, int>>();

            // Create the object under test
            var objectUnderTest = new HtmlUnitAdapter(mockTime.Object);
            var callSequence = 0;
            var retryNumber = 0;

            var timeDelaySequence = new List<int>();
            foreach (var result in success)
            {
                var number = retryNumber++;
                mockPredicate.Setup(o => o
                    .Invoke(objectUnderTest, number))
                    .Returns(result)
                    .CalledInSequence(seq, callSequence++)
                    .Verifiable(MoqExtensions.F("Call number {0} should be to the predicate", callSequence));

                // If this is a fail, we want to delay
                if (!result) timeDelaySequence.Add(callSequence++);
            }

            mockTime.Setup(o => o
                   .Sleep(MILLISECOND_DELAY))
                   .CalledInSequence(seq, timeDelaySequence.ToArray())
                   .Verifiable(MoqExtensions.F("Call number {0} should be for a delay", timeDelaySequence.CommaSeparated()));
            // ACT
            objectUnderTest.ShouldEventually(DelegateWrapper.For(mockPredicate), timesToRetry, MILLISECOND_DELAY);


            // ASSERT
            //mockTime.Verify( o => o.Sleep(MILLISECOND_DELAY), Times.Exactly(timesToRetry),"Should be asked to delay between each ");
            mockPredicate.Verify();
            mockTime.Verify();
        }


        [TestCase(1)]
        [TestCase(3)]
        public void EventualSuccessShouldFailIfThePredicateFailsOnEveryRetry(int timesToRetry)
        {
            const int MILLISECOND_DELAY = 1500;
            // ARRANGE

            var seq = new Sequence();

            var mockTime = new Mock<ITime>();
            var mockPredicate = new Mock<IPredicate<IBrowser, int>>();

            // Create the object under test
            var objectUnderTest = new HtmlUnitAdapter(mockTime.Object);

            var timeDelaySequence = new List<int>();
            (timesToRetry + 1).Times(ctr =>
            {
                mockPredicate.Setup(o => o
                    .Invoke(objectUnderTest, ctr))
                    .Returns(false)
                    .CalledInSequence(seq, (ctr * 2))
                    .Verifiable(MoqExtensions.F("Call number {0} should be to the predicate", ctr * 2));

                timeDelaySequence.Add((ctr * 2) + 1);
            });



            mockTime.Setup(o => o
                   .Sleep(MILLISECOND_DELAY))
                   .CalledInSequence(seq, timeDelaySequence.ToArray())
                   .Verifiable(MoqExtensions.F("Call number {0} should be for a delay", timeDelaySequence.CommaSeparated()));

            // ACT
            Assert.Throws<AssertionException>(
                () =>
                objectUnderTest.ShouldEventually(DelegateWrapper.For(mockPredicate), timesToRetry, MILLISECOND_DELAY),
                "Expected an Assertion Exception");


            // ASSERT
            //mockTime.Verify( o => o.Sleep(MILLISECOND_DELAY), Times.Exactly(timesToRetry),"Should be asked to delay between each ");
            mockPredicate.Verify();
            mockTime.Verify();
        }

        public interface IPredicate<T>
        {
            bool Invoke(T argument);
        }

        public interface IPredicate<T1, T2>
        {
            bool Invoke(T1 arg1, T2 arg2);
        }

        public static class DelegateWrapper
        {
            public static Predicate<T> For<T>(Mock<IPredicate<T>> mockArgument)
            {
                return o => mockArgument.Object.Invoke(o);
            }

            public static Func<T1, T2, bool> For<T1, T2>(Mock<IPredicate<T1, T2>> mockArgument)
            {
                return (o1, o2) => mockArgument.Object.Invoke(o1, o2);
            }

        }

    }
}