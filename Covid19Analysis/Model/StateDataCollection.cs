using System;
using System.Collections.Generic;

namespace Covid19Analysis.Model
{
    /// <summary>
    ///     Class to hold the collection of States
    /// </summary>
    public class StateDataCollection
    {
        private readonly Dictionary<string, StateData> stateCollection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="StateDataCollection"/> class.
        /// </summary>
        public StateDataCollection()
        {
            this.stateCollection = new Dictionary<string, StateData>();
        }

        /// <summary>
        ///     Gets the State if it exists.
        /// </summary>
        /// <param name="stateAbbreviation">The state / territory abbreviation you want to search for.</param>
        /// <returns>The State / Territory and it's related data</returns>
        public StateData GetStateData(string stateAbbreviation)
        {
            StateData stateData = null;
            if (stateCollection.ContainsKey(stateAbbreviation))
            {
                stateData = this.stateCollection[stateAbbreviation];
            }

            return stateData;
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

            if (stateCollection.ContainsKey(covidCase.State))
            {
                stateCollection[covidCase.State].AddCovidCase(covidCase);
            }
            else
            {
                StateData newState = new StateData(covidCase.State);
                stateCollection.Add(newState.State, newState);
                stateCollection[covidCase.State].AddCovidCase(covidCase);
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
    }
}
