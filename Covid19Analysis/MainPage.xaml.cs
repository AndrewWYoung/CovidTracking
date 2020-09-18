using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private const string LOCATION_OF_INTEREST = "GA";
        private CsvReader csvReader;
        private CovidLocationDataCollection covidCollection;

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
            if (this.CurrentFile == null)
            {
                this.CurrentFile = await this.chooseFile();
            }
            else
            {
                ContentDialogResult dialogResult = await promptReplaceOrMergeFile();
                if (dialogResult == ContentDialogResult.Secondary)
                {
                    this.covidCollection.ClearData();
                    await this.loadFile();
                } else if (dialogResult != ContentDialogResult.None)
                {
                    await this.loadFile();
                }
            }

            this.displayInformation();
        }

        private async Task<ContentDialogResult> promptReplaceOrMergeFile()
        {
            var dialogBox = new ContentDialog()
            {
                Title = "Replace or Merge Existing File?",
                Content = "Do you want to replace or merge to the current file?",
                PrimaryButtonText = "Merge!",
                SecondaryButtonText = "Replace!",
                CloseButtonText = "Cancel"
            };
            var result = await dialogBox.ShowAsync();

            return result;
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

        private async void displayInformation()
        {
            if (this.CurrentFile != null)
            {
                try
                {
                    this.summaryTextBox.Text = "Loading...";

                    csvReader.CsvFile = this.CurrentFile;
                    IList<CovidCase> covidCases = await csvReader.Parse();
                    this.covidCollection.AddAllCovidCases(covidCases);

                    CovidLocationData covidLocationData = this.covidCollection.GetLocationData(LOCATION_OF_INTEREST);
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

        
        private async void duplicateCasesButton_Click(object sender, RoutedEventArgs e)
        {
            string defaultOutput = "No Duplicate Keys Found";
            var output = "";
            IList<CovidCase> duplicateCases = new List<CovidCase>();

            var location = this.covidCollection.GetLocationData(LOCATION_OF_INTEREST);

            IList<CovidCase> tempList = new List<CovidCase>();

            // var result = await skipOrReplaceDialog.ShowAsync();
            if (location != null)
            {
                var skipOrReplaceDialog = new DuplicateEntryContentDialog();
                duplicateCases = location.DuplicateCases;
                // var totalDuplicates = duplicateCases;
                for (var i = 0; i < duplicateCases.Count; i++)
                {
                    
                    skipOrReplaceDialog.Subtitle = $"There are {duplicateCases.Count - i} items with the same date";
                    skipOrReplaceDialog.Message = duplicateCases[i].ToString();
                    skipOrReplaceDialog.UpdateContent();

                    if (!skipOrReplaceDialog.IsChecked)
                    { 
                        var result = await skipOrReplaceDialog.ShowAsync();

                        if (result == ContentDialogResult.Primary)
                        {
                            location.FindAndReplace(duplicateCases[i]);
                            tempList.Add(duplicateCases[i]);
                        }
                    } else if (skipOrReplaceDialog.IsChecked && skipOrReplaceDialog.LastKnownButtonPress == "Primary")
                    {
                        location.FindAndReplace(duplicateCases[i]);
                        tempList.Add(duplicateCases[i]);
                    }
                }
                var itemsRemoved = this.removeDuplicateItems(tempList, duplicateCases);
                if (itemsRemoved > 0)
                {
                    this.promptItemsHaveBeenReplaced(itemsRemoved);
                }
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
            this.displayInformation();
        }

        private int removeDuplicateItems(IList<CovidCase> tempList, IList<CovidCase> duplicateList)
        {
            var count = 0;
            Debug.WriteLine($"tempList: {tempList.Count} __ duplicates: {duplicateList.Count}");
            foreach (var currentCase in tempList)
            {
                var item = duplicateList.First(i => i.Date.Equals(currentCase.Date));
                var index = duplicateList.IndexOf(item);

                if (index != -1)
                {
                    duplicateList.RemoveAt(index);
                    count++;
                }
            }
            return count;
        }

        /*
        private async void duplicateCasesButton_Click(object sender, RoutedEventArgs e)
        {
            this.displayReplaceOrSkipDialog();
        }
        */
    }
}
