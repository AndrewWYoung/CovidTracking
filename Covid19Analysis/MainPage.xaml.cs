using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Covid19Analysis.Factory;
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
        #region Constants
        private const string LocationOfInterest = "GA";
        private const int LowerThresholdDefault = 0;
        private const int UpperThresholdDefault = 2500;
        #endregion

        #region Data members
        private int lowerThreshold;
        private int upperThreshold = 2500;
        private readonly CsvReader csvReader;
        private readonly CovidLocationDataCollection covidCollection;
        private CovidLocationData covidLocationData;

        /// <summary>
        ///     The application height
        /// </summary>
        public const int ApplicationHeight = 400;

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
            this.lowerThresholdTextBox.Text = LowerThresholdDefault.ToString();
            this.upperThresholdTextBox.Text = UpperThresholdDefault.ToString();
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
            this.lowerThreshold = int.Parse(this.lowerThresholdTextBox.Text);
            this.upperThreshold = int.Parse(this.upperThresholdTextBox.Text);

            var validThreshold = (this.lowerThreshold < this.upperThreshold);
            if (validThreshold)
            {
                if (this.CurrentFile == null)
                {
                    this.CurrentFile = await this.chooseFile();
                }
                else
                {
                    ContentDialogResult dialogResult = await DialogFactory.ShowDialog(DialogType.ReplaceOrMerge);
                    if (dialogResult == ContentDialogResult.Primary)
                    {
                        await this.loadFile();
                    }
                    else if (dialogResult == ContentDialogResult.Secondary)
                    {
                        this.covidCollection.ClearData();
                        await this.loadFile();
                    }
                }

                await this.extractData();
                await this.displayInformation();
            }
            else
            {
                await DialogFactory.ShowDialog(DialogType.InvalidThresholds);
            }
        }

        private async Task loadFile()
        {
            var fileToLoad = await this.chooseFile();
            if (fileToLoad != null)
            {
                this.CurrentFile = fileToLoad;
            }
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

        private async Task extractData()
        {
            if (this.CurrentFile != null)
            {
                csvReader.CsvFile = this.CurrentFile;
                IList<CovidCase> covidCases = await csvReader.Parse();
                this.covidCollection.AddAllCovidCases(covidCases);

                this.covidLocationData = this.covidCollection.GetLocationData(LocationOfInterest);
            }
        }

        private async Task displayInformation()
        {
            if (this.CurrentFile != null)
            {
                try
                {
                    this.summaryTextBox.Text = "Loading...";

                    if (this.covidLocationData != null)
                    {
                        OutputBuilder report = new OutputBuilder(this.covidLocationData)
                        {
                            LowerThreshold = this.lowerThreshold, 
                            UpperThreshold = this.upperThreshold
                        };
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

        
        private async void duplicateCasesButton_Click(object sender, RoutedEventArgs e)
        {
            IList<CovidCase> tempList = new List<CovidCase>();

            if (this.covidLocationData == null || this.covidLocationData.DuplicateCases.Count == 0)
            {
                await DialogFactory.ShowDialog(DialogType.NoDuplicateKeys);
            }

            if (this.covidLocationData != null && this.covidLocationData.DuplicateCases.Count > 0)
            {

                var skipOrReplaceDialog = new DuplicateEntryContentDialog();

                for (var i = 0; i < this.covidLocationData.DuplicateCases.Count; i++)
                {
                    skipOrReplaceDialog.Subtitle = $"There are {this.covidLocationData.DuplicateCases.Count - i} items with the same date";
                    skipOrReplaceDialog.Message = this.covidLocationData.DuplicateCases[i].ToString();
                    skipOrReplaceDialog.UpdateContent();

                    if (!skipOrReplaceDialog.IsChecked)
                    { 
                        var result = await skipOrReplaceDialog.ShowAsync();

                        if (result == ContentDialogResult.Primary)
                        {
                            this.covidLocationData.FindAndReplace(this.covidLocationData.DuplicateCases[i]);
                            tempList.Add(this.covidLocationData.DuplicateCases[i]);
                        }
                    } else if (skipOrReplaceDialog.IsChecked && skipOrReplaceDialog.LastKnownButtonPress == "Primary")
                    {
                        this.covidLocationData.FindAndReplace(this.covidLocationData.DuplicateCases[i]);
                        tempList.Add(this.covidLocationData.DuplicateCases[i]);
                    }
                }

                var itemsRemoved = this.removeDuplicateItems(tempList);
                tempList.Clear();
                if (itemsRemoved > 0)
                {
                    this.promptItemsHaveBeenReplaced(itemsRemoved);
                }

                var buttonDuplicateCases3 = this.covidLocationData.DuplicateCases.Count;
            }
        }

        private async void promptItemsHaveBeenReplaced(int itemsRemoved)
        {
            var dialogBox = new ContentDialog()
            {
                Title = "Items Replaced",
                Content = $"{itemsRemoved} items have been replaced",
                PrimaryButtonText = "Yes!",
            };
            await dialogBox.ShowAsync();
            await this.displayInformation();
        }

        private int removeDuplicateItems(IList<CovidCase> tempList)
        {

            var count = 0;
            foreach (var currentCase in tempList)
            {
                
                var item = this.covidLocationData.DuplicateCases.First(i => i.Date.Equals(currentCase.Date));
                var index = this.covidLocationData.DuplicateCases.IndexOf(item);

                if (index == -1) continue;
                this.covidLocationData.DuplicateCases.RemoveAt(index);

                count++;
            }

            return count;
        }

        private void TextBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }
    }
}
