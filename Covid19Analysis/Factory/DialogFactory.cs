using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Covid19Analysis.Factory
{

    /// <summary>
    ///     Factory class that prompts a user and returns the ContentDialogResult
    /// </summary>
    public class DialogFactory
    {

        /// <summary>Shows the dialog based on the given parameter.</summary>
        /// <param name="dialogType">Type of the dialog.</param>
        /// <returns>
        ///   ContentDialogResult
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">dialogType - null</exception>
        public static async Task<ContentDialogResult> ShowDialog(DialogType dialogType)
        {
            switch (dialogType)
            {
                case DialogType.ReplaceOrMerge:
                    var mergeOrReplaceDialog = new ContentDialog()
                    {
                        Title = "Replace or Merge Existing File?",
                        Content = "Do you want to replace or merge to the current file?",
                        PrimaryButtonText = "Merge!",
                        SecondaryButtonText = "Replace!",
                        CloseButtonText = "Cancel"
                    };
                    var mergeOrReplaceResult = await mergeOrReplaceDialog.ShowAsync();
                    return mergeOrReplaceResult;
                case DialogType.InvalidThresholds:
                    var invalidThresholdDialog = new ContentDialog()
                    {
                        Title = "Invalid Thresholds",
                        Content = "Lower threshold CAN'T be higher than the upper threshold.",
                        PrimaryButtonText = "Okay, fine...",
                    };
                    var invalidThresholdResult = await invalidThresholdDialog.ShowAsync();
                    return invalidThresholdResult;
                case DialogType.NoDuplicateKeys:
                    var noDuplicateKeysDialog = new ContentDialog()
                    {
                        Title = "No Duplicate Keys Found",
                        Content = "No duplicate keys have been found",
                        PrimaryButtonText = "Okay!",
                    };
                    var noDuplicateKeysResult = await noDuplicateKeysDialog.ShowAsync();
                    return noDuplicateKeysResult;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dialogType), dialogType, null);
            }

        }
    }
}
