using System;
using System.Collections.Generic;

namespace Covid19Analysis.Model
{

    /// <summary>
    ///     Report class to showcase data on a given State
    /// </summary>
    public class Report
    {
        #region Data members

        private StateData state;

        #endregion

        #region Properties        
        /// <summary>
        ///     Gets or sets the State for the report.
        /// </summary>
        /// <value>
        ///     The State.
        /// </value>
        /// <exception cref="NullReferenceException">value cannot be null</exception>
        public StateData State
        {
            get => this.state;
            set => this.state = value ?? throw new NullReferenceException(nameof(value));
        }

        #endregion

        #region Constructors        
        /// <summary>
        ///     Initializes a new instance of the <see cref="Report"/> class.
        /// </summary>
        /// <param name="stateData">The state data.</param>
        /// <exception cref="NullReferenceException">stateData cannot be null</exception>
        public Report(StateData stateData)
        {
            this.state = stateData ?? throw new NullReferenceException(nameof(stateData));
        }

        #endregion

        #region Methods
        /// <summary>
        ///     Gets the State summary.
        /// </summary>
        /// <returns>A summary of the State</returns>
        public string GetStateSummary()
        {
            var output = "";
            output += $"{this.state.State}{Environment.NewLine}";
            output += $"{this.getEarliestKnownPositiveTest()} {Environment.NewLine}";
            output += $"{this.getHighestNumberOfPositiveTests(this.state.GetAllCases())} {Environment.NewLine}";
            output += $"{this.getHighestNumberOfNegativeTests()} {Environment.NewLine}";
            output += $"{this.getHighestNumberOfTestsOfAGivenDay(this.state.GetAllCases())} {Environment.NewLine}";
            output += $"{this.getHighestNumberOfDeaths()} {Environment.NewLine}";
            output += $"{this.getHighestNumberOfHospitalizations()} {Environment.NewLine}";
            output += $"{this.getHighestPercentageOfPositiveTests()} {Environment.NewLine}";
            output += $"{this.getAverageOfPositiveTestsSinceFirstPositiveCase()} {Environment.NewLine}";
            output += $"{this.getOverallPositivityRates()} {Environment.NewLine}";
            output += $"{this.getNumberOfDaysWherePosiviteTestsAreAbove(2500)} {Environment.NewLine}";
            output += $"{this.getNumberOfDaysWherePositiveTetsAreBelow(1000)}";

            return output;
        }

        /// <summary>
        ///     Gets the monthly summary of a given month.
        /// </summary>
        /// <param name="month">The month to generate a report for.</param>
        /// <returns>The monthly summary of a given month.</returns>
        public string GetMonthlySummary(Month month)
        {
            var output = month + Environment.NewLine;
            var covidEvents = this.state.GetEventsFromMonth(month);
            if (covidEvents.Count == 0)
            {
                return "";
            }

            var caseWithHighestPositiveTests = this.state.GetHighestNumberOfPositiveTests(covidEvents);
            var caseWithLowestPositiveTests = this.state.GetLowestNumberOfPositiveTests(covidEvents);
            var caseWithHighestTestCount = this.state.GetHighestNumberOfTestsOnAGivenDay(covidEvents);
            var caseWithLowestTestCount = this.state.GetLowestNumberOfTotalTests(covidEvents);
            var averageOfPositiveTests= Math.Round(this.state.GetAverageNumberOfPositiveTests(covidEvents), 2);
            var averageOfTotalTests= Math.Round(this.state.GetAverageNumberOfAllTests(covidEvents), 2);

            output += $"Highest # of positive tests: {caseWithHighestPositiveTests.PositiveIncrease:N0} occurred on the {this.formatDay(caseWithHighestPositiveTests.Date.Day)} {Environment.NewLine}";
            output += $"Lowest # of positive tests: {caseWithLowestPositiveTests.PositiveIncrease:N0} occurred on the {this.formatDay(caseWithLowestPositiveTests.Date.Day)} {Environment.NewLine}";
            output += $"Highest # of total tests: {caseWithHighestTestCount.GetTotalTestCount():N0} occurred on the {this.formatDay(caseWithHighestTestCount.Date.Day)} {Environment.NewLine}";
            output += $"Lowest # of total tests: {caseWithLowestTestCount.GetTotalTestCount():N0} occurred on the {this.formatDay(caseWithLowestTestCount.Date.Day)} {Environment.NewLine}";
            output += $"Average # of positive tests: {averageOfPositiveTests:N2} {Environment.NewLine}";
            output += $"Average # of total tests: {averageOfTotalTests:N2} {Environment.NewLine}";

            return output;
        }

        /// <summary>
        ///     Gets the yearly summary of all months with Covid data.
        /// </summary>
        /// <returns>A yearly summary of the covid data for the current State.</returns>
        public string GetYearlySummary()
        {
            var output = "";
            for (var month = 1; month <= 12; month++)
            {
                output += this.GetMonthlySummary((Month) month) + Environment.NewLine;
            }

            return output;
        }

        private string getEarliestKnownPositiveTest()
        {
            var earliestCovidCase = this.state.GetEarliestPositiveCase();
            var date = earliestCovidCase.Date;
            var numberOfPositiveTests = earliestCovidCase.PositiveIncrease;
            return
                $"Earliest known positive case occurred on [{date:MMMM dd yyyy}] with {numberOfPositiveTests:N0} positive tests.";
        }

        private string getHighestNumberOfPositiveTests(IList<CovidCase> covidCases)
        {
            var highestPostiveTests = this.state.GetHighestNumberOfPositiveTests(covidCases);
            var date = highestPostiveTests.Date;
            var numberOfPositiveTests = highestPostiveTests.PositiveIncrease;
            return
                $"Highest number of positive tests occurred on [{date:MMMM dd yyyy}] with {numberOfPositiveTests:N0} positive tests.";
        }

        private string getHighestNumberOfNegativeTests()
        {
            var highestNegativeTests = this.state.GetHighestNumberOfNegativeIncreases();
            var date = highestNegativeTests.Date;
            var numberOfPositiveTests = highestNegativeTests.NegativeIncrease;
            return
                $"Highest number of negative tests occurred on [{date:MMMM dd yyyy}] with {numberOfPositiveTests:N0} negative tests.";
        }

        private string getHighestNumberOfTestsOfAGivenDay(IList<CovidCase> covidCases)
        {
            var highestNumberOfTests = this.state.GetHighestNumberOfTestsOnAGivenDay(covidCases);
            var date = highestNumberOfTests.Date;
            var totalTests = highestNumberOfTests.PositiveIncrease + highestNumberOfTests.NegativeIncrease;
            return $"Highest number of total tests occurred on [{date:MMMM dd yyyy}] with {totalTests:N0} tests.";
        }


        private string getHighestNumberOfDeaths()
        {
            var highestNumberOfDeaths = this.state.GetHighestDeathsEvent();
            var date = highestNumberOfDeaths.Date;
            var deaths = highestNumberOfDeaths.DeathIncrease;
            return $"Highest number of deaths occurred on [{date:MMMM dd yyyy}] with {deaths:N0} deaths.";
        }

        private string getHighestNumberOfHospitalizations()
        {
            var highestNumberOfHospitalizations = this.state.GetHighestHospitalization();
            var date = highestNumberOfHospitalizations.Date;
            var hospitalizations = highestNumberOfHospitalizations.HospitalizedIncrease;
            return
                $"Highest number of hospitalizations occurred on [{date:MMMM dd yyyy}] with {hospitalizations:N0} hospitalizations.";
        }

        private string getHighestPercentageOfPositiveTests()
        {
            var highestPositivePercentage = this.state.GetHighestPercentageOfPositiveTests();
            var date = highestPositivePercentage.Date;
            return $"Highest percentage of positive tests occurred on [{date:MMMM dd yyyy}]";
        }

        private string getAverageOfPositiveTestsSinceFirstPositiveCase()
        {
            var averageOfPositiveTests = this.state.GetAverageNumberOfPositiveTests(this.state.GetAllCases());
            return $"Average number of positive tests: {averageOfPositiveTests:N2}";
        }

        private string getOverallPositivityRates()
        {
            var overallPositivityRate = this.state.GetOverallPositivityRate(this.state.GetAllCases());
            return $"Overall positivity rate of all tests: {overallPositivityRate:N2}%";
        }

        private string getNumberOfDaysWherePosiviteTestsAreAbove(int numberOfPostiveTests)
        {
            return $"Number of days with Positive Tests > {numberOfPostiveTests:N0}: {this.state.GetNumberOfDaysWherePositiveTestsAreAbove(numberOfPostiveTests)}";
        }

        private string getNumberOfDaysWherePositiveTetsAreBelow(int numberOfPositiveTests)
        {
            return $"Number of days with positive tests < {numberOfPositiveTests:N0}: {this.state.GetNumberOfDaysWherePositiveTestsAreBelow(numberOfPositiveTests)}";
        }

        private string formatDay(int day)
        {
            if (day < 1 || day > 31)
            {
                return "";
            }

            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return $"{day}st";
                case 2:
                case 22:
                    return $"{day}nd";
                case 3:
                case 23:
                    return $"{day}rd";
                default:
                    return $"{day}th";
            }
        }

        #endregion
    }
}