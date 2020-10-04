using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Covid19Analysis.View
{

    /// <summary>
    ///   Class for creating a duplicate entry content dialog
    /// </summary>
    public sealed partial class DuplicateEntryContentDialog
    {

        /// <summary>Gets or sets a value indicating whether this instance is checked.</summary>
        /// <value>
        /// <c>true</c> if this instance is checked; otherwise, <c>false</c>.</value>
        public bool IsChecked { get; set; }

        /// <summary>Gets or sets the subtitle.</summary>
        /// <value>The subtitle.</value>
        public string Subtitle { get; set; }

        /// <summary>Gets or sets the message.</summary>
        /// <value>The message.</value>
        public string Message { get; set; }

        /// <summary>Gets or sets the last known button press.</summary>
        /// <value>The last known button press.</value>
        public string LastKnownButtonPress { get; set; }


        /// <summary>Initializes a new instance of the <a onclick="return false;" href="DuplicateEntryContentDialog" originaltag="see">DuplicateEntryContentDialog</a> class.</summary>
        public DuplicateEntryContentDialog()
        {
            this.InitializeComponent();
            // this.IsChecked = false;
        }


        /// <summary>Updates the content.</summary>
        public void UpdateContent()
        {
            this.subtitleTextBox.Text = Subtitle ?? "";
            this.contentTextBox.Text = Message ?? "";
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.LastKnownButtonPress = "Primary";
            if (this.repeatActionForAll.IsChecked == true)
            {

            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.LastKnownButtonPress = "Secondary";
        }

        private void repeatActions_Click(object sender, RoutedEventArgs e) => this.IsChecked = (this.repeatActionForAll.IsChecked == true);
    }
}
