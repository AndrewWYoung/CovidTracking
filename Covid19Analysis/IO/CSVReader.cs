using System;
using System.Collections.Generic;
using System.Globalization;
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
        #region Constants
        private const int DateColumn = 0;
        private const int LocationColumn = 1;
        private const int PositiveColumn = 2;
        private const int NegativeColumn = 3;
        private const int DeathColumn = 4;
        private const int HospitalizedColumn = 5;
        private const string DateFormat = "yyyyMMdd";
        #endregion

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
            set
            {
                this.errors.Clear();
                this.csvFile = value ?? throw new ArgumentNullException(nameof(this.csvFile));
            }
        }

        private readonly IList<string> errors;


        /// <summary>Get all lines or errors if any.</summary>
        /// <value>The errors.</value>
        public IList<string> Errors => this.errors;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="CsvReader" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">csvFile cannot be null</exception>
        public CsvReader()
        {
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
                var dateTime = DateTime.ParseExact(data[DateColumn], DateFormat, CultureInfo.InvariantCulture);
                var state = data[LocationColumn];

                var covidCase = new CovidCase(state, dateTime)
                {
                    PositiveIncrease = int.Parse(data[PositiveColumn]),
                    NegativeIncrease = int.Parse(data[NegativeColumn]),
                    DeathIncrease = int.Parse(data[DeathColumn]),
                    HospitalizedIncrease = int.Parse(data[HospitalizedColumn])
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
            var validDate = containsValidDate(data[DateColumn]);
            var validPositive = containsValidNumber(data[PositiveColumn]);
            var validNegative = containsValidNumber(data[NegativeColumn]);
            var validDeath = containsValidNumber(data[DeathColumn]);
            var validHospitalized = containsValidNumber(data[HospitalizedColumn]);

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
            string format = DateFormat;
            if (DateTime.TryParseExact(data, format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dateTime))
            {
                return true;
            }

            return false;
        }

        private bool containsValidNumber(string data)
        {
            if (int.TryParse(data, out var number))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}