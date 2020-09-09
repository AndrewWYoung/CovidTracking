using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Covid19Analysis.Model;

namespace Covid19Analysis.IO
{
    /// <summary>
    ///     Reads CSV file and extracts information
    /// </summary>
    public class CsvReader
    {
        #region Data members

        private readonly char defaultDelimiter = ',';
        private StorageFile csvFile;

        #endregion

        #region Properties

        /// <summary>Gets or sets the CSV file.</summary>
        /// <value>The CSV file.</value>
        /// <exception cref="ArgumentNullException">csvFile cannot be null</exception>
        public StorageFile CsvFile
        {
            get => this.csvFile;
            set => this.csvFile = value ?? throw new ArgumentNullException(nameof(this.csvFile));
        }

        #endregion

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="CsvReader" /> class.</summary>
        /// <param name="csvFile">The CSV file.</param>
        /// <exception cref="ArgumentNullException">csvFile cannot be null</exception>
        public CsvReader(StorageFile csvFile)
        {
            this.csvFile = csvFile ?? throw new ArgumentNullException(nameof(csvFile));
        }

        #endregion

        #region Methods

        /// <summary>Parses the current CSV that has been loaded in.</summary>
        /// <returns>Collection of CovidCases</returns>
        public async Task<List<CovidCase>> Parse()
        {
            var covidCollection = new List<CovidCase>();
            var text = await FileIO.ReadTextAsync(this.CsvFile);
            var buffer = await FileIO.ReadBufferAsync(this.CsvFile);
            using (var dataReader = DataReader.FromBuffer(buffer))
            {
                var content = dataReader.ReadString(buffer.Length);
                var data = content.Split(Environment.NewLine);

                var count = 0;
                foreach (var record in data)
                {
                    if (count != 0 && count < data.Length - 1)
                    {
                        var stateData = record.Split(this.defaultDelimiter);
                        covidCollection.Add(this.processCovidData(stateData));
                    }

                    count++;
                }
            }

            return covidCollection;
        }

        private CovidCase processCovidData(string[] data)
        {
            var state = data[1];
            var covidCase = new CovidCase(state, this.ExtractDateTime(data[0])) {
                PositiveIncrease = int.Parse(data[2]),
                NegativeIncrease = int.Parse(data[3]),
                DeathIncrease = int.Parse(data[4]),
                HospitalizedIncrease = int.Parse(data[5])
            };
            this.ExtractDateTime(data[0]);

            return covidCase;
        }

        private DateTime ExtractDateTime(string data)
        {
            var year = int.Parse(data.Substring(0, 4));
            var month = int.Parse(data.Substring(4, 2));
            var day = int.Parse(data.Substring(6, 2));

            return new DateTime(year, month, day);
        }

        #endregion
    }
}