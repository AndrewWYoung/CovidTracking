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

        private const int DATE_COLUMN = 0;
        private const int LOCATION_COLUMN = 1;
        private const int POSITIVE_COLUMN = 2;
        private const int NEGATIVE_COLUMN = 3;
        private const int DEATH_COLUMN = 4;
        private const int HOSPITALIZED_COLUMN = 5;
        private const string DATE_FORMAT = "yyyyMMdd";

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
            set
            {
                this.errors.Clear();
                this.csvFile = value ?? throw new ArgumentNullException(nameof(this.csvFile));
            }
        }

        private readonly IList<string> errors;


        /// <summary>Get all lines or errors if any.</summary>
        /// <value>The errors.</value>
        public IList<string> Errors
        {
            get { return this.errors; }
        }


        #endregion

        #region Constructors

        public CsvReader()
        {
            this.errors = new List<string>();
        }
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

        /// <summary>
        ///     Parses the current CSV that has been loaded in.
        /// </summary>
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
                    }

                    count++;
                }
            }

            return covidCollection;
        }

        /// <summary>Gets the errors as string.</summary>
        /// <returns>
        ///   A string of all errors.
        /// </returns>
        public string GetErrorsAsString()
        {
            string errors = "";
            foreach (var currentError in this.errors)
            {
                errors += currentError + Environment.NewLine;
            }

            return errors;
        }

        private CovidCase processCovidData(int row, string[] data)
        {
            if (!isValid(data))
            {
                this.createErrorMessage(row, data);
                return null;
            }

            return this.createCovidCase(data);
        }

        private void createErrorMessage(int row, string[] data)
        {
            string line = "";
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
        }

        private CovidCase createCovidCase(string[] data)
        {
            try
            {
                var dateTime = DateTime.ParseExact(data[DATE_COLUMN], DATE_FORMAT, CultureInfo.InvariantCulture);
                var state = data[LOCATION_COLUMN];

                var covidCase = new CovidCase(state, dateTime)
                {
                    PositiveIncrease = int.Parse(data[POSITIVE_COLUMN]),
                    NegativeIncrease = int.Parse(data[NEGATIVE_COLUMN]),
                    DeathIncrease = int.Parse(data[DEATH_COLUMN]),
                    HospitalizedIncrease = int.Parse(data[HOSPITALIZED_COLUMN])
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
            var validDate = containsValidDate(data[DATE_COLUMN]);
            var validPositive = containsValidNumber(data[POSITIVE_COLUMN]);
            var validNegative = containsValidNumber(data[NEGATIVE_COLUMN]);
            var validDeath = containsValidNumber(data[DEATH_COLUMN]);
            var validHospitalized = containsValidNumber(data[HOSPITALIZED_COLUMN]);

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
            string format = DATE_FORMAT;
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