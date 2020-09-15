using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Covid19Analysis.View
{
    public sealed partial class DuplicateEntryContentDialog : ContentDialog
    {
        public bool IsChecked { get; set; }
        public string Subtitle { get; set; }
        public string Message { get; set; }
        public string LastKnownButtonPress { get; set; }
        public DuplicateEntryContentDialog()
        {
            this.InitializeComponent();
            // this.IsChecked = false;
        }

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

        private void repeatActions_Click(object sender, RoutedEventArgs e) => this.IsChecked = (this.repeatActionForAll.IsChecked == true) ? true : false;
    }
}
