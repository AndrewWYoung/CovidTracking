using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
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

        private IList<string> errors;

        public IList<string> Errors
        {
            get { return this.errors; }
        }


        #endregion

        #region Constructors

        /// <summary>Initializes a new instance of the <see cref="CsvReader" /> class.</summary>
        /// <param name="csvFile">The CSV file.</param>
        /// <exception cref="ArgumentNullException">csvFile cannot be null</exception>
        public CsvReader(StorageFile csvFile)
        {
            this.csvFile = csvFile ?? throw new ArgumentNullException(nameof(csvFile));
            this.errors = new List<string>();
        }

        #endregion

        #region Methods

        /// <summary>Parses the current CSV that has been loaded in.</summary>
        /// <returns>Collection of CovidCases</returns>
        public async Task<List<CovidCase>> Parse()
        {
            var covidCollection = new List<CovidCase>();
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
                        var covidData = this.processCovidData(count, stateData);
                        if (covidData != null)
                        {
                            covidCollection.Add(covidData);
                        }
                        // covidCollection.Add(this.processCovidData(stateData));

                    }

                    count++;
                }
            }

            return covidCollection;
        }

        private CovidCase processCovidData(int row, string[] data)
        {
            if (!isValid(data))
            {
                string line = $"Data:";
                foreach (var item in data)
                {
                    if (String.IsNullOrEmpty(item))
                    {
                        line += " __ ";
                    }
                    else
                    {
                        line += item + " ";
                    }
                }
                this.errors.Add($"ERROR: Invalid Row [{row}] - {line}");
                return null;
            }
            try
            {
                var dateTime = DateTime.ParseExact(data[0], "yyyyMMdd", CultureInfo.InvariantCulture);
                var state = data[1];

                var covidCase = new CovidCase(state, dateTime)
                {
                    PositiveIncrease = int.Parse(data[2]),
                    NegativeIncrease = int.Parse(data[3]),
                    DeathIncrease = int.Parse(data[4]),
                    HospitalizedIncrease = int.Parse(data[5])
                };

                return covidCase;
            }
            catch (Exception e)
            {
                throw new Exception($"Error parsing data: {e}");
            }

        }

        private bool isValid(string[] data)
        {
            var validFields = containsValidFields(data);
            var validDate = containsValidDate(data[0]);
            var validPositive = containsValidNumber(data[2]);
            var validNegative = containsValidNumber(data[3]);
            var validDeath = containsValidNumber(data[4]);
            var validHospitalized = containsValidNumber(data[5]);

            var result = validFields && validDate && validPositive && validNegative && validDeath && validHospitalized;

            return result;
        }

        private bool containsValidFields(string[] data)
        {
            var validFields = true;
            foreach (var item in data)
            {
                if (String.IsNullOrEmpty(item))
                {
                    validFields = false;
                }
            }

            return validFields;
        }

        private bool containsValidDate(string data)
        {
            string format = "yyyyMMdd";
            DateTime dateTime;
            if (DateTime.TryParseExact(data, format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out dateTime))
            {
                return true;
            }

            return false;
        }

        private bool containsValidNumber(string data)
        {
            int number;
            if (int.TryParse(data, out number))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}