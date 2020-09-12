using System;
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

        /// <summary>
        ///     The application height
        /// </summary>
        public const int ApplicationHeight = 355;

        /// <summary>
        ///     The application width
        /// </summary>
        public const int ApplicationWidth = 625;

        #endregion

        /// <summary>
        ///     Gets or sets the current data file to analyze.
        /// </summary>
        /// <value>The current data file to analyze.</value>
        public StorageFile CurrentFile { get; set; }

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
        }

        #endregion

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

                    var csvReader = new CsvReader(this.CurrentFile);
                    var stateDataCollection = new CovidLocationDataCollection();
                    stateDataCollection.AddAllCovidCases(await csvReader.Parse());

                    var report = new OutputBuilder(stateDataCollection.GetLocationData("GA"));
                    this.summaryTextBox.Text = report.GetLocationSummary() + report.GetYearlySummary();
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
