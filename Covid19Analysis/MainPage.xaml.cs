using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Covid19Analysis.IO;
using Covid19Analysis.Model;
using Covid19Analysis.View;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Covid19Analysis
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Data members
        // TODO: Maybe delete?
        private CsvReader csvReader;
        private CovidLocationDataCollection covidCollection;

        /// <summary>
        ///     The application height
        /// </summary>
        public const int ApplicationHeight = 355;

        /// <summary>
        ///     The application width
        /// </summary>
        public const int ApplicationWidth = 625;

        #endregion

        #region Properties
        /// <summary>
        ///     Gets or sets the current data file to analyze.
        /// </summary>
        /// <value>The current data file to analyze.</value>
        public StorageFile CurrentFile { get; set; }
        #endregion 

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MainPage" /> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            ApplicationView.PreferredLaunchViewSize = new Size {Width = ApplicationWidth, Height = ApplicationHeight};
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(ApplicationWidth, ApplicationHeight));

            this.csvReader = new CsvReader();
            this.covidCollection = new CovidLocationDataCollection();
        }

        #endregion
        private async void displayErrors_Click(object sender, RoutedEventArgs e)
        {
            string defaultOutput = "No Known Errors";
            var errorDialog = new ContentDialog()
            {
                Title = "CSV Errors",
                Content = new ScrollViewer()
                {
                    Content = new TextBlock()
                    {
                        Text = (this.csvReader.Errors.Count > 0) ? this.csvReader.GetErrorsAsString() : defaultOutput
                    },
                },
                CloseButtonText = "ok"
            };

            await errorDialog.ShowAsync();
        }

        private async void loadFile_Click(object sender, RoutedEventArgs e)
        {
            if (this.CurrentFile != null && await promptReplaceExistingFile())
            {
                var fileToMerge = await this.chooseFile();
                if (fileToMerge != null)
                {
                    // TODO: Add fucntionality to merge new file data into system and remove the following line.
                    this.CurrentFile = fileToMerge;
                }
            }

            if (this.CurrentFile == null)
            {
                this.CurrentFile = await this.chooseFile();
            }

            this.displayInformation();
        }

        private async Task<bool> promptReplaceExistingFile()
        {
            var dialogBox = new ContentDialog()
            {
                Title = "Replace Existing File?",
                Content = "Do you want to replace the current file?",
                PrimaryButtonText = "Yes!",
                SecondaryButtonText = "No!"
            };
            var result = await dialogBox.ShowAsync();

            return (result == ContentDialogResult.Primary);
        }

        private async Task<StorageFile> chooseFile()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".csv");
            picker.FileTypeFilter.Add(".txt");

            return await picker.PickSingleFileAsync();
        }

        private async void displayInformation()
        {
            if (this.CurrentFile != null)
            {
                try
                {
                    this.summaryTextBox.Text = "Loading...";

                    // var csvReader = new CsvReader(this.CurrentFile);
                    csvReader.CsvFile = this.CurrentFile;
                    IList<CovidCase> covidCases = await csvReader.Parse();
                    this.covidCollection.AddAllCovidCases(covidCases);

                    CovidLocationData covidLocationData = this.covidCollection.GetLocationData("GA");
                    OutputBuilder report;

                    if (covidLocationData != null)
                    {
                        report = new OutputBuilder(covidLocationData);
                        this.summaryTextBox.Text = report.GetLocationSummary() + report.GetYearlySummary();
                    }
                    else
                    {
                        this.summaryTextBox.Text = "No data found for the requested location.";
                    }
                }
                catch (Exception)
                {
                    var message = "Invalid File. Please make sure you have chosen the correct file or ensure the file is in the proper format.";
                    this.summaryTextBox.Text = message;
                }
            }
            else
            {
                this.summaryTextBox.Text = "No file loaded...";
            }
        }
    }
}
