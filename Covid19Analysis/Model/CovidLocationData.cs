using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Covid19Analysis.Model
{
    /// <summary>
    ///     Stores the data for a given LocationData / territory.
    /// </summary>
    public class CovidLocationData
    {
        #region Member Variables

        // private readonly IList<CovidCase> locationsCovidCases;
        private IList<CovidCase> locationsCovidCases;
        private readonly List<CovidCase> duplicateCases;

        #endregion

        #region Properties        
        /// <summary>
        ///     Gets or sets the LocationData.
        /// </summary>
        /// <value>
        ///     The state to be set.
        /// </value>
        public string State { get; set; }


        /// <summary>Gets the duplicate cases.</summary>
        /// <value>The duplicate cases.</value>
        public IList<CovidCase> DuplicateCases => this.duplicateCases;

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
            this.locationsCovidCases = new List<CovidCase>();
            this.duplicateCases = new List<CovidCase>();
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

            if (this.locationsCovidCases.Any(covid => covid.Date.Equals(covidCase.Date)))
            {
                this.duplicateCases.Add(covidCase);
            }
            else
            {
                this.locationsCovidCases.Add(covidCase);
                // this.locationsCovidCases = this.SortData();
            }
        }

        /// <summary>
        ///     Gets all cases for this state / territory.
        /// </summary>
        /// <returns>A collection of covid cases for this state / territory</returns>
        public IList<CovidCase> GetAllCases()
        {
            return this.locationsCovidCases;
        }

        /// <summary>
        ///     Gets the earliest positive case for this LocationData / territory
        /// </summary>
        /// <returns>A single covid case of the earliest positive case</returns>
        public CovidCase GetEarliestPositiveCase()
        {
            if (this.locationsCovidCases.Count == 0)
            {
                return null;
            }

            return this.locationsCovidCases.OrderBy(covidCase => covidCase.Date).First(covidCase => covidCase.PositiveIncrease > 0);
        }

        /// <summary>
        ///     Gets the highest number of negative increases.
        /// </summary>
        /// <returns>A CovidCase with the highest number of negative tests</returns>
        public CovidCase GetHighestNumberOfNegativeIncreases()
        {
            if (this.locationsCovidCases.Count == 0)
            {
                return null;
            }

            return this.locationsCovidCases.OrderByDescending(covidCase => covidCase.NegativeIncrease).First();
        }

        /// <summary>
        ///     Sorts the data by date (earliest first).
        /// </summary>
        /// <returns>the original data in order by date (earliest first)</returns>
        public IList<CovidCase> SortData()
        {
            return this.locationsCovidCases.OrderBy(x => x.Date).ToList();
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
            var indexOfEarliestCase = this.locationsCovidCases.IndexOf(earliestCovidCase);
            var positiveTests = 0;
            for (var i = indexOfEarliestCase; i < this.locationsCovidCases.Count; i++)
            {
                if (this.locationsCovidCases[i].PositiveIncrease > numberOfPositiveTests)
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
            var indexOfEarliestCase = this.locationsCovidCases.IndexOf(earliestCovidCase);
            var positiveTests = 0;
            for (var i = indexOfEarliestCase; i < this.locationsCovidCases.Count; i++)
            {
                if (this.locationsCovidCases[i].PositiveIncrease < numberOfPositiveTests)
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

            double totalCount = totalPositiveTests + totalNegativeTests;

            return (totalCount != 0) ? (totalPositiveTests / (totalNegativeTests + totalPositiveTests)) * 100 : 0;
        }

        /// <summary>
        ///     Gets the average number of positive tests.
        /// </summary>
        /// <param name="covidCases">The collection of covid cases.</param>
        /// <returns></returns>
        public double GetAverageNumberOfPositiveTests(IList<CovidCase> covidCases)
        {
            double positiveTestCount = 0;

            foreach (var covidEvent in this.locationsCovidCases)
            {
                positiveTestCount += covidEvent.PositiveIncrease;
            }

            return positiveTestCount / this.locationsCovidCases.Count;
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
            foreach (var covidEvent in this.locationsCovidCases)
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
            if (this.locationsCovidCases.Count == 0)
            {
                return null;
            }

            return this.locationsCovidCases.OrderByDescending(covidCase => covidCase.DeathIncrease).First();
        }

        /// <summary>
        ///     Gets the CovidCase with the highest hospitalization.
        /// </summary>
        /// <returns>CovidCase with the highest hospitalizations</returns>
        public CovidCase GetHighestHospitalization()
        {
            if (this.locationsCovidCases.Count == 0)
            {
                return null;
            }

            return this.locationsCovidCases.OrderByDescending(covidCase => covidCase.HospitalizedIncrease).First();
        }

        /// <summary>
        ///     Gets the highest percentage of postive tests event.
        /// </summary>
        /// <returns>CovidCase with the highest percentage of positive tests.</returns>
        public CovidCase GetHighestPercentageOfPositiveTests()
        {
            if (this.locationsCovidCases.Count == 0)
            {
                return null;
            }

            var covidEvent = this.locationsCovidCases[0];

            foreach (var covidCase in this.locationsCovidCases)
            {
                double highestPercentage = (covidEvent.PositiveIncrease > 0) ? Convert.ToDouble(covidEvent.PositiveIncrease) /
                                                                               Convert.ToDouble(covidEvent.PositiveIncrease + covidEvent.NegativeIncrease) : 0;
                double currentPercentage = (covidCase.PositiveIncrease > 0) ? Convert.ToDouble(covidCase.PositiveIncrease) /
                                           Convert.ToDouble(covidCase.PositiveIncrease + covidEvent.NegativeIncrease) : 0;

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
        public IList<CovidCase> GetEventsFromMonth(int month)
        {
            if (month < 1 || month > 12)
            {
                throw new Exception("Invalid Month");
            }
            var covidEvents = new List<CovidCase>();
            foreach (var covidCase in this.locationsCovidCases)
            {
                if (covidCase.Date.Month == month)
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

            return this.locationsCovidCases.OrderByDescending(covidCase => covidCase.PositiveIncrease).First();
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

            return this.locationsCovidCases.OrderBy(covidCase => covidCase.PositiveIncrease).First();
        }

        /// <summary>
        ///     Finds and Replaces a covid case with the exact date.
        /// </summary>
        public void FindAndReplace(CovidCase covidCase)
        {
            var item = this.locationsCovidCases.First(i => i.Date.Equals(covidCase.Date));
            var index = this.locationsCovidCases.IndexOf(item);
            if (index != -1)
            {
                this.locationsCovidCases.RemoveAt(index);
                this.locationsCovidCases.Add(covidCase);
            }
        }

        /// <summary>
        ///     Clears all data in the collection.
        /// </summary>
        public void ClearData()
        {
            this.locationsCovidCases.Clear();
        }


        /// <summary>The number of positive cases between the given parameters.</summary>
        /// <param name="minTestCount">The minimum test count.</param>
        /// <param name="maxTestCount">The maximum test count.</param>
        /// <returns>
        ///   The number of positive cases between the given parameters.
        /// </returns>
        public int NumberOfPositiveCasesBetween(int minTestCount, int maxTestCount)
        {
            return this.locationsCovidCases
                       .Where(covidCase => covidCase.PositiveIncrease >= minTestCount)
                       .Where(covidCase => covidCase.PositiveIncrease <= maxTestCount).Count(covidCase => covidCase.Date >= this.GetEarliestPositiveCase().Date);
        }

        /// <summary>
        /// Removes a duplicate entry
        /// </summary>
        public void RemoveDuplicateEntry(CovidCase covidCase)
        {
            var item = this.duplicateCases.First(i => i.Date.Equals(covidCase.Date));
            var index = this.duplicateCases.IndexOf(item);

            if (index != -1)
                this.duplicateCases.RemoveAt(index);
        }

        #endregion
    }
}