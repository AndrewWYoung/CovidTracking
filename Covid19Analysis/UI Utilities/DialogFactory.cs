using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Covid19Analysis.Enums;

namespace Covid19Analysis.UI_Utilities
{
    class DialogFactory
    {
        public static async Task<ContentDialogResult> ShowDialog(DialogType dialogType)
        {
            switch (dialogType)
            {
                case DialogType.MergeOrReplace:
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(dialogType), dialogType, null);
            }
        }
    }
}
