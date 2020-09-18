using System;
using System.Collections.Generic;

namespace Covid19Analysis.Model
{
    /// <summary>
    ///     Class to hold the collection of States
    /// </summary>
    public class CovidLocationDataCollection
    {
        private readonly Dictionary<string, CovidLocationData> covidLocationDataCollection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CovidLocationDataCollection"/> class.
        /// </summary>
        public CovidLocationDataCollection()
        {
            this.covidLocationDataCollection = new Dictionary<string, CovidLocationData>();
        }

        /// <summary>
        ///     Gets the LocationData if it exists.
        /// </summary>
        /// <param name="locationAbbreviation">The state / territory abbreviation you want to search for.</param>
        /// <returns>The LocationData / Territory and it's related data</returns>
        public CovidLocationData GetLocationData(string locationAbbreviation)
        {
            CovidLocationData locationData = null;
            if (covidLocationDataCollection.ContainsKey(locationAbbreviation))
            {
                locationData = this.covidLocationDataCollection[locationAbbreviation];
            }

            return locationData;
        }

        /// <summary>
        ///     Adds the specified covid case to the collection.
        /// </summary>
        /// <param name="covidCase">The covid case you want to add.</param>
        /// <exception cref="ArgumentNullException">covidCase cannot be null</exception>
        public void AddCovidCase(CovidCase covidCase)
        {
            if (covidCase == null)
            {
                throw new ArgumentNullException(nameof(covidCase));
            }

            if (covidLocationDataCollection.ContainsKey(covidCase.Location))
            {
                covidLocationDataCollection[covidCase.Location].AddCovidCase(covidCase);
            }
            else
            {
                CovidLocationData newState = new CovidLocationData(covidCase.Location);
                covidLocationDataCollection.Add(newState.State, newState);
                covidLocationDataCollection[covidCase.Location].AddCovidCase(covidCase);
            }
        }

        /// <summary>
        ///     Adds all of the covid cases within the specified list.
        /// </summary>
        /// <param name="covidCases">The covid cases you would like to add.</param>
        /// <exception cref="ArgumentNullException">covidCases cannot be null</exception>
        public void AddAllCovidCases(IList<CovidCase> covidCases)
        {
            if (covidCases == null)
            {
                throw new ArgumentNullException(nameof(covidCases));
            }

            foreach (var covidCase in covidCases)
            {
                this.AddCovidCase(covidCase);
            }
        }

        /// <summary>
        ///     Clears the data.
        /// </summary>
        public void ClearData()
        {
            foreach (var item in this.covidLocationDataCollection.Values)
            {
                item.ClearData();
            }
            this.covidLocationDataCollection.Clear();
        }
    }
}
