using System;

namespace Covid19Analysis.Model
{
    /// <summary>
    ///     Contains information of a reported covid case.
    /// </summary>
    public class CovidCase
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the name of the LocationData.
        /// </summary>
        /// <value>The name of the LocationData.</value>
        public string Location { get; set; }


        /// <summary>
        ///     Gets or sets the date of the covid case.
        /// </summary>
        /// <value>The date of the covid case.</value>
        public DateTime Date { get; set; }

        /// <summary>
        ///     Gets or sets the positive increase.
        /// </summary>
        /// <value>The positive increase you want to set.</value>
        public int PositiveIncrease { get; set; }


        /// <summary>
        ///     Gets the total test count of all positive and negative tests.
        /// </summary>
        /// <value>The total test count of all positive and negative tests.</value>
        public int TotalTestCount => this.PositiveIncrease + this.NegativeIncrease;


        /// <summary>
        ///     Gets or sets the negative increase.
        /// </summary>
        /// <value>The negative increase to be set.</value>
        public int NegativeIncrease { get; set; }

        /// <summary>
        ///     Gets or sets the death increase.
        /// </summary>
        /// <value>The death increase to be set.</value>
        public int DeathIncrease { get; set; }


        /// <summary>
        ///     Gets or sets the hospitalized increase.
        /// </summary>
        /// <value>The hospitalized increase to be set.</value>
        public int HospitalizedIncrease { get; set; }

        #endregion

        #region Constructors


        /// <summary>
        ///     Initializes a new instance of the <see cref="CovidCase" /> class.
        /// </summary>
        /// <param name="state">The name of the LocationData.</param>
        /// <param name="date">The date of the covid case.</param>
        /// <exception cref="ArgumentNullException">state cannot be null</exception>
        public CovidCase(string state, DateTime date)
        {
            this.Location = state ?? throw new ArgumentNullException(nameof(state));
            this.Date = date;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Get a string representation of the Covid Case.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var output = "";
            output +=
                $"{this.Date.ToShortDateString()} {this.Location} : [+Increase] {this.PositiveIncrease} [-increase] {this.NegativeIncrease} ";
            output +=
                $"[death increase] {this.DeathIncrease} [hospitalized increase] {this.HospitalizedIncrease}{Environment.NewLine}";
            return output;
        }
        #endregion
    }
}