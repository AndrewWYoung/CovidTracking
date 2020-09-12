using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Graphics.Printing;

namespace Covid19Analysis.Model
{
    /// <summary>
    ///     Stores the data for a given LocationData / territory.
    /// </summary>
    public class CovidLocationData
    {
        #region Properties        
        /// <summary>
        ///     Gets or sets the LocationData.
        /// </summary>
        /// <value>
        ///     The state to be set.
        /// </value>
        public string State { get; set; }

        #endregion

        #region Constructors        
        /// <summary>
        ///     Initializes a new instance of the <see cref="CovidLocationData"/> class.
        /// </summary>
        /// <param name="state">The state / territory of the data</param>
        /// <exception cref="ArgumentNullException">state cannot be null</exception>
        public CovidLocationData(string state)
        {
            this.State = state ?? throw new ArgumentNullException(nameof(state));
            this.covidCases = new List<CovidCase>();
        }

        #endregion

        #region Methods
        /// <summary>
        ///     Adds a single covid case to the collection
        /// </summary>
        /// <param name="covidCase">The covid case to be added</param>
        /// <exception cref="ArgumentNullException">covidCase cannot be null</exception>
        public void AddCovidCase(CovidCase covidCase)
        {
            if (covidCase == null)
            {
                throw new ArgumentNullException(nameof(covidCase));
            }
            this.covidCases.Add(covidCase);
            this.covidCases = this.SortData();
        }

        /// <summary>
        ///     Gets all cases for this state / territory.
        /// </summary>
        /// <returns>A collection of covid cases for this state / territory</returns>
        public IList<CovidCase> GetAllCases()
        {
            return this.covidCases;
        }

        /// <summary>
        ///     Gets the earliest positive case for this LocationData / territory
        /// </summary>
        /// <returns>A single covid case of the earliest positive case</returns>
        public CovidCase GetEarliestPositiveCase()
        {
            if (covidCases.Count == 0)
            {
                return null;
            }

            var earliestPostiveCase = covidCases[0];

            foreach (var covidCase in this.covidCases)
            {
                if (covidCase.PositiveIncrease > 0)
                {
                    return covidCase;
                }
            }

            return earliestPostiveCase;
        }

        /// <summary>
        ///     Gets the highest number of negative increases.
        /// </summary>
        /// <returns>A CovidCase with the highest number of negative tests</returns>
        public CovidCase GetHighestNumberOfNegativeIncreases()
        {
            if (this.covidCases.Count == 0)
            {
                return null;
            }
            CovidCase highestNegativeIncrease = this.covidCases[0];
            foreach (var covidCase in this.covidCases)
            {
                if (covidCase.NegativeIncrease > highestNegativeIncrease.NegativeIncrease)
                {
                    highestNegativeIncrease = covidCase;
                }
            }

            return highestNegativeIncrease;
        }

        /// <summary>
        ///     Sorts the data by date (earliest first).
        /// </summary>
        /// <returns>the original data in order by date (earliest first)</returns>
        public IList<CovidCase> SortData()
        {
            return this.covidCases.OrderBy(x => x.Date).ToList();
        }

        /// <summary>
        ///     Gets the number of days where positive tests are above a specified amount.
        /// </summary>
        /// <param name="numberOfPositiveTests">The number of positive tests.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public int GetNumberOfDaysWherePositiveTestsAreAbove(int numberOfPositiveTests)
        {
            var earliestCovidCase = this.GetEarliestPositiveCase();
            var indexOfEarliestCase = this.covidCases.IndexOf(earliestCovidCase);
            var positiveTests = 0;
            for (var i = indexOfEarliestCase; i < this.covidCases.Count; i++)
            {
                if (this.covidCases[i].PositiveIncrease > numberOfPositiveTests)
                {
                    positiveTests++;
                }
            }

            return positiveTests;
        }


        /// <summary
        ///     >Gets the number of days where positive tests are below a specified amount.
        /// </summary>
        /// <param name="numberOfPositiveTests">The number of positive tests.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public int GetNumberOfDaysWherePositiveTestsAreBelow(int numberOfPositiveTests)
        {
            var earliestCovidCase = this.GetEarliestPositiveCase();
            var indexOfEarliestCase = this.covidCases.IndexOf(earliestCovidCase);
            var positiveTests = 0;
            for (var i = indexOfEarliestCase; i < this.covidCases.Count; i++)
            {
                if (this.covidCases[i].PositiveIncrease < numberOfPositiveTests)
                {
                    positiveTests++;
                }
            }

            return positiveTests;
        }

        /// <summary>
        ///     Gets the overall positivity rate.
        /// </summary>
        /// <param name="covidCases">Collection of covid cases</param>
        /// <returns>The overall positivity rate</returns>
        public double GetOverallPositivityRate(IList<CovidCase> covidCases)
        {
            if (covidCases.Count == 0)
            {
                return 0.00;
            }

            double totalPositiveTests = 0;
            double totalNegativeTests = 0;

            foreach (var covidCase in covidCases)
            {
                totalPositiveTests += covidCase.PositiveIncrease;
                totalNegativeTests += covidCase.NegativeIncrease;
            }

            return Math.Round(totalPositiveTests / totalNegativeTests, 2);
        }

        /// <summary>
        ///     Gets the average number of positive tests.
        /// </summary>
        /// <param name="covidCases">The collection of covid cases.</param>
        /// <returns></returns>
        public double GetAverageNumberOfPositiveTests(IList<CovidCase> covidCases)
        {
            double positiveTestCount = 0;

            foreach (var covidEvent in this.covidCases)
            {
                positiveTestCount += covidEvent.PositiveIncrease;
            }

            return positiveTestCount / this.covidCases.Count;
        }

        /// <summary>
        ///     Gets the average number of all tests.
        /// </summary>
        /// <param name="covidCases">The covid cases.</param>
        /// <returns>The average number of all tests.</returns>
        public double GetAverageNumberOfAllTests(IList<CovidCase> covidCases)
        {
            double positiveTestCount = 0;
            double negativeTestCount = 0;
            foreach (var covidEvent in this.covidCases)
            {
                positiveTestCount += covidEvent.PositiveIncrease;
                negativeTestCount += covidEvent.NegativeIncrease;
            }

            return (positiveTestCount + negativeTestCount) / covidCases.Count;
        }

        /// <summary>
        ///     Gets the highest number of tests on a given day.
        /// </summary>
        /// <param name="covidCases">The covid cases.</param>
        /// <returns>CovidCase with the highest test on a given day</returns>
        public CovidCase GetHighestNumberOfTestsOnAGivenDay(IList<CovidCase> covidCases)
        {
            if (covidCases.Count == 0)
            {
                return null;
            }

            var highestNumberOfTests = covidCases[0];
            foreach (var covidCase in covidCases)
            {
                var currentHighest = highestNumberOfTests.PositiveIncrease + highestNumberOfTests.NegativeIncrease;
                var newTestCount = covidCase.PositiveIncrease + covidCase.NegativeIncrease;
                if (newTestCount > currentHighest)
                {
                    highestNumberOfTests = covidCase;
                }
            }

            return highestNumberOfTests;
        }

        /// <summary>
        ///     Gets the highest deaths event.
        /// </summary>
        /// <returns>CovidCase with the highest death on a single day.</returns>
        public CovidCase GetHighestDeathsEvent()
        {
            if (this.covidCases.Count == 0)
            {
                return null;
            }

            var covidEvent = this.covidCases[0];
            foreach (var covidCase in this.covidCases)
            {
                var highestDeathCount = covidEvent.DeathIncrease;
                var currentDeathCount = covidCase.DeathIncrease;
                if (currentDeathCount > highestDeathCount)
                {
                    covidEvent = covidCase;
                }
            }

            return covidEvent;
        }

        /// <summary>
        ///     Gets the CovidCase with the highest hospitalization.
        /// </summary>
        /// <returns>CovidCase with the highest hospitalizations</returns>
        public CovidCase GetHighestHospitalization()
        {
            if (this.covidCases.Count == 0)
            {
                return null;
            }

            var covidEvent = this.covidCases[0];
            foreach (var covidCase in this.covidCases)
            {
                var highestCount = covidEvent.HospitalizedIncrease;
                var currentCount = covidCase.HospitalizedIncrease;
                if (currentCount > highestCount)
                {
                    covidEvent = covidCase;
                }
            }

            return covidEvent;
        }

        /// <summary>
        ///     Gets the highest percentage of postive tests event.
        /// </summary>
        /// <returns>CovidCase with the highest percentage of positive tests.</returns>
        public CovidCase GetHighestPercentageOfPositiveTests()
        {
            if (this.covidCases.Count == 0)
            {
                return null;
            }

            var covidEvent = this.covidCases[0];

            foreach (var covidCase in this.covidCases)
            {
                double highestPercentage = (covidEvent.PositiveIncrease > 0) ? Convert.ToDouble(covidEvent.PositiveIncrease + covidEvent.NegativeIncrease) /
                                                                               Convert.ToDouble(covidEvent.PositiveIncrease) : 0;
                double currentPercentage = (covidCase.PositiveIncrease > 0) ? Convert.ToDouble(covidCase.PositiveIncrease + covidCase.NegativeIncrease) /
                                           Convert.ToDouble(covidCase.PositiveIncrease) : 0;

                if (currentPercentage > highestPercentage)
                {
                    covidEvent = covidCase;
                }
            }

            return covidEvent;
        }

        /// <summary>
        ///     Gets the events from a given month.
        /// </summary>
        /// <param name="month">The month.</param>
        /// <returns>CovidCases of a given month.</returns>
        public IList<CovidCase> GetEventsFromMonth(Month month)
        {
            var covidEvents = new List<CovidCase>();
            foreach (var covidCase in this.covidCases)
            {
                if (covidCase.Date.Month == (int) month)
                {
                    covidEvents.Add(covidCase);
                }
            }

            return covidEvents;
        }

        /// <summary>
        ///     Gets the lowest number of total tests.
        /// </summary>
        /// <param name="covidCases">The covid cases.</param>
        /// <returns>CovidCase with the lowest number of total tests</returns>
        public CovidCase GetLowestNumberOfTotalTests(IList<CovidCase> covidCases)
        {
            if (covidCases.Count == 0)
            {
                return null;
            }

            var lowestNumberOfTests = covidCases[0];
            foreach (var covidCase in covidCases)
            {
                var highestCount = lowestNumberOfTests.PositiveIncrease + lowestNumberOfTests.NegativeIncrease;
                var currentCount = covidCase.PositiveIncrease + covidCase.NegativeIncrease;

                if (covidCase.PositiveIncrease < lowestNumberOfTests.PositiveIncrease)
                {
                    lowestNumberOfTests = covidCase;
                }
            }

            return lowestNumberOfTests;
        }

        /// <summary>
        ///     Gets the highest number of positive tests.
        /// </summary>
        /// <param name="covidCases">The covid cases.</param>
        /// <returns>CovidCase with the highest number of positve tests.</returns>
        public CovidCase GetHighestNumberOfPositiveTests(IList<CovidCase> covidCases)
        {
            if (covidCases.Count == 0)
            {
                return null;
            }

            var highestPositiveIncrease = covidCases[0];
            foreach (var covidCase in covidCases)
            {
                if (covidCase.PositiveIncrease > highestPositiveIncrease.PositiveIncrease)
                {
                    highestPositiveIncrease = covidCase;
                }
            }

            return highestPositiveIncrease;
        }

        /// <summary>
        ///     Gets the lowest number of positive tests.
        /// </summary>
        /// <param name="covidCases">The covid cases.</param>
        /// <returns>CovidCase with the lowest number of positive.</returns>
        public CovidCase GetLowestNumberOfPositiveTests(IList<CovidCase> covidCases)
        {
            if (covidCases.Count == 0)
            {
                return null;
            }

            var lowestPositiveIncrease = covidCases[0];

            foreach (var covidCase in covidCases)
            {
                if (covidCase.PositiveIncrease < lowestPositiveIncrease.PositiveIncrease)
                {
                    lowestPositiveIncrease = covidCase;
                }
            }

            return lowestPositiveIncrease;
        }

        #endregion

        #region Member Variables

        // private readonly IList<CovidCase> covidCases;
        private IList<CovidCase> covidCases;

        #endregion
    }
}