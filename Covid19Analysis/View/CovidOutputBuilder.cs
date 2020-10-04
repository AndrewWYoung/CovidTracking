using System;
using System.Collections.Generic;
using System.Globalization;
using Covid19Analysis.Model;

namespace Covid19Analysis.View
{

    /// <summary>
    ///     Report class to showcase data at a given location.
    /// </summary>
    public class CovidOutputBuilder
    {
        #region Data members

        private CovidLocationData location;

        #endregion

        #region Properties        
        /// <summary>
        ///     Gets or sets the LocationData for the report.
        /// </summary>
        /// <value>
        ///     The LocationData.
        /// </value>
        /// <exception cref="NullReferenceException">value cannot be null</exception>
        public CovidLocationData LocationData
        {
            get => this.location;
            set => this.location = value ?? throw new NullReferenceException(nameof(value));
        }


        /// <summary>
        ///     Gets or sets the upper threshold for positive cases.
        /// </summary>
        /// <value>The upper threshold.</value>
        public int UpperThreshold { get; set; }

        /// <summary>
        ///     Gets or sets the lower threshold for positive cases.
        /// </summary>
        /// <value>The lower threshold.</value>
        public int LowerThreshold { get; set; }

        #endregion

        #region Constructors        
        /// <summary>
        ///     Initializes a new instance of the <see cref="CovidOutputBuilder"/> class.
        /// </summary>
        /// <param name="stateData">The location data.</param>
        /// <exception cref="NullReferenceException">stateData cannot be null</exception>
        public CovidOutputBuilder(CovidLocationData stateData)
        {
            this.location = stateData ?? throw new NullReferenceException(nameof(stateData));
        }

        #endregion

        #region Methods
        /// <summary>
        ///     Gets the LocationData summary.
        /// </summary>
        /// <returns>A summary of the LocationData</returns>
        public string GetLocationSummary()
        {
            var output = "";

            output += $"{this.location.State}{Environment.NewLine}";
            output += $"{this.getEarliestKnownPositiveTestStatement()} {Environment.NewLine}";
            output += $"{this.getHighestNumberOfPositiveTestsStatement(this.location.GetAllCases())} {Environment.NewLine}";
            output += $"{this.getHighestNumberOfNegativeTestsStatement()} {Environment.NewLine}";
            output += $"{this.getHighestNumberOfTestsOfAGivenDayStatement(this.location.GetAllCases())} {Environment.NewLine}";
            output += $"{this.getHighestNumberOfDeathsStatement()} {Environment.NewLine}";
            output += $"{this.getHighestNumberOfHospitalizationsStatement()} {Environment.NewLine}";
            output += $"{this.getHighestPercentageOfPositiveTestsStatement()} {Environment.NewLine}";
            output += $"{this.getAverageOfPositiveTestsSinceFirstPositiveCaseStatement()} {Environment.NewLine}";
            output += $"{this.getOverallPositivityRatesStatement()} {Environment.NewLine}";
            output += $"{this.getNumberOfDaysWherePosiviteTestsAreAboveStatement(this.LowerThreshold)} {Environment.NewLine}";
            output += $"{this.getNumberOfDaysWherePositiveTetsAreBelowStatement(this.UpperThreshold)} {Environment.NewLine}";

            output += $"{Environment.NewLine}{this.buildHistogramOfPositiveCases()} {Environment.NewLine}";

            return output;
        }

        /// <summary>
        ///     Gets the monthly summary of a given month.
        /// </summary>
        /// <param name="month">The month to generate a report for.</param>
        /// <returns>The monthly summary of a given month.</returns>
        public string GetMonthlySummary(int month)
        {
            if (month < 1 || month > 12)
            {
                return "Invalid Month" + Environment.NewLine;
            }

            var covidEvents = this.location.GetEventsFromMonth(month);
            var output = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + $" [{covidEvents.Count} days of data]" + Environment.NewLine;

            if (covidEvents.Count == 0)
            {
                return output;
            }

            var caseWithHighestPositiveTests = this.location.GetHighestNumberOfPositiveTests(covidEvents);
            var caseWithLowestPositiveTests = this.location.GetLowestNumberOfPositiveTests(covidEvents);
            var caseWithHighestTestCount = this.location.GetHighestNumberOfTestsOnAGivenDay(covidEvents);
            var caseWithLowestTestCount = this.location.GetLowestNumberOfTotalTests(covidEvents);
            var averageOfPositiveTests= Math.Round(this.location.GetAverageNumberOfPositiveTests(covidEvents), 2);
            var averageOfTotalTests= Math.Round(this.location.GetAverageNumberOfAllTests(covidEvents), 2);

            output += $"Highest # of positive tests: {caseWithHighestPositiveTests.PositiveIncrease:N0} occurred on the {this.getDayWithSuffix(caseWithHighestPositiveTests.Date.Day)} {Environment.NewLine}";
            output += $"Lowest # of positive tests: {caseWithLowestPositiveTests.PositiveIncrease:N0} occurred on the {this.getDayWithSuffix(caseWithLowestPositiveTests.Date.Day)} {Environment.NewLine}";
            output += $"Highest # of total tests: {caseWithHighestTestCount.TotalTestCount:N0} occurred on the {this.getDayWithSuffix(caseWithHighestTestCount.Date.Day)} {Environment.NewLine}";
            output += $"Lowest # of total tests: {caseWithLowestTestCount.TotalTestCount:N0} occurred on the {this.getDayWithSuffix(caseWithLowestTestCount.Date.Day)} {Environment.NewLine}";
            output += $"Average # of positive tests: {averageOfPositiveTests:N2} {Environment.NewLine}";
            output += $"Average # of total tests: {averageOfTotalTests:N2} {Environment.NewLine}";

            return output;
        }

        /// <summary>
        ///     Gets the yearly summary of all months with Covid data.
        /// </summary>
        /// <returns>A yearly summary of the covid data for the current LocationData.</returns>
        public string GetYearlySummary()
        {
            var output = "";
            for (var month = 1; month <= 12; month++)
            {
                output += this.GetMonthlySummary(month) + Environment.NewLine;
            }

            return output;
        }

        private string getEarliestKnownPositiveTestStatement()
        {
            var earliestCovidCase = this.location.GetEarliestPositiveCase();
            var date = earliestCovidCase.Date;
            var numberOfPositiveTests = earliestCovidCase.PositiveIncrease;
            return
                $"Earliest known positive case occurred on [{date:MMMM dd yyyy}] with {numberOfPositiveTests:N0} positive tests.";
        }

        private string getHighestNumberOfPositiveTestsStatement(IList<CovidCase> covidCases)
        {
            var highestPostiveTests = this.location.GetHighestNumberOfPositiveTests(covidCases);
            var date = highestPostiveTests.Date;
            var numberOfPositiveTests = highestPostiveTests.PositiveIncrease;
            return
                $"Highest number of positive tests occurred on [{date:MMMM dd yyyy}] with {numberOfPositiveTests:N0} positive tests.";
        }

        private string getHighestNumberOfNegativeTestsStatement()
        {
            var highestNegativeTests = this.location.GetHighestNumberOfNegativeIncreases();
            var date = highestNegativeTests.Date;
            var numberOfPositiveTests = highestNegativeTests.NegativeIncrease;
            return
                $"Highest number of negative tests occurred on [{date:MMMM dd yyyy}] with {numberOfPositiveTests:N0} negative tests.";
        }

        private string getHighestNumberOfTestsOfAGivenDayStatement(IList<CovidCase> covidCases)
        {
            var highestNumberOfTests = this.location.GetHighestNumberOfTestsOnAGivenDay(covidCases);
            var date = highestNumberOfTests.Date;
            var totalTests = highestNumberOfTests.PositiveIncrease + highestNumberOfTests.NegativeIncrease;
            return $"Highest number of total tests occurred on [{date:MMMM dd yyyy}] with {totalTests:N0} tests.";
        }


        private string getHighestNumberOfDeathsStatement()
        {
            var highestNumberOfDeaths = this.location.GetHighestDeathsEvent();
            var date = highestNumberOfDeaths.Date;
            var deaths = highestNumberOfDeaths.DeathIncrease;
            return $"Highest number of deaths occurred on [{date:MMMM dd yyyy}] with {deaths:N0} deaths.";
        }

        private string getHighestNumberOfHospitalizationsStatement()
        {
            var highestNumberOfHospitalizations = this.location.GetHighestHospitalization();
            var date = highestNumberOfHospitalizations.Date;
            var hospitalizations = highestNumberOfHospitalizations.HospitalizedIncrease;
            return
                $"Highest number of hospitalizations occurred on [{date:MMMM dd yyyy}] with {hospitalizations:N0} hospitalizations.";
        }

        private string getHighestPercentageOfPositiveTestsStatement()
        {
            var highestPositivePercentage = this.location.GetHighestPercentageOfPositiveTests();
            var percent = (highestPositivePercentage.PositiveIncrease / (highestPositivePercentage.NegativeIncrease + highestPositivePercentage.PositiveIncrease)) * 100;
            var date = highestPositivePercentage.Date;
            return $"Highest percentage of positive tests occurred on [{date:MMMM dd yyyy}] at {percent:N2}%";
        }

        private string getAverageOfPositiveTestsSinceFirstPositiveCaseStatement()
        {
            var averageOfPositiveTests = this.location.GetAverageNumberOfPositiveTests(this.location.GetAllCases());
            return $"Average number of positive tests: {averageOfPositiveTests:N2}";
        }

        private string getOverallPositivityRatesStatement()
        {
            var overallPositivityRate = this.location.GetOverallPositivityRate(this.location.GetAllCases());
            return $"Overall positivity rate of all tests: {overallPositivityRate:N2}%";
        }

        private string getNumberOfDaysWherePosiviteTestsAreAboveStatement(int numberOfPostiveTests)
        {
            return $"Number of days with Positive Tests > {numberOfPostiveTests:N0}: {this.location.GetNumberOfDaysWherePositiveTestsAreAbove(numberOfPostiveTests)}";
        }

        private string getNumberOfDaysWherePositiveTetsAreBelowStatement(int numberOfPositiveTests)
        {
            return $"Number of days with positive tests < {numberOfPositiveTests:N0}: {this.location.GetNumberOfDaysWherePositiveTestsAreBelow(numberOfPositiveTests)}";
        }

        private string buildHistogramOfPositiveCases()
        {
            var startingPoint = 0;
            var segments = 500;
            var output = $"HISTOGRAM of Postive Tests{Environment.NewLine}";
            var highestNumberOfCases = this.LocationData.GetHighestNumberOfPositiveTests(this.location.GetAllCases());
            int highestPositveTests = highestNumberOfCases.PositiveIncrease;


            for (int i = startingPoint; i < highestPositveTests; i += segments)
            {
                int numberOfCases = this.location.NumberOfPositiveCasesBetween(i, i + segments);
                output += $"{i} - {i + segments}: {numberOfCases} {Environment.NewLine}";
            }

            return output;
        }

        private string getDayWithSuffix(int day)
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