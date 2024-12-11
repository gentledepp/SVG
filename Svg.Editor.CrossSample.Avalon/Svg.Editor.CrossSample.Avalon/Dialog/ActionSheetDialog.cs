using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Editor.Sample.Avalon.Dialog
{
    public class ActionSheetDialog : Window
    {
        private string _selectedAction;

        public ActionSheetDialog(string title, string cancelButton, string[] actions)
        {
            // Window setup
            Width = 350;
            Height = actions.Length * 50 + 150; // Dynamic height based on actions
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Title = title;

            // Main vertical stack panel
            var mainPanel = new StackPanel
            {
                Margin = new Avalonia.Thickness(10),
                Spacing = 10
            };

            // Title
            if (!string.IsNullOrEmpty(title))
            {
                mainPanel.Children.Add(new TextBlock
                {
                    Text = title,
                    FontWeight = FontWeight.Bold,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 0, 0, 10)
                });
            }

            // Action buttons
            foreach (var action in actions)
            {
                var button = new Button
                {
                    Content = action,
                    Height = 40,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Avalonia.Thickness(0, 5)
                };

                button.Click += (s, e) =>
                {
                    _selectedAction = action;
                    Close(action);
                };

                mainPanel.Children.Add(button);
            }

            // Cancel button
            if (!string.IsNullOrEmpty(cancelButton))
            {
                var button = new Button
                {
                    Content = cancelButton,
                    Height = 40,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Background = new SolidColorBrush(Colors.LightGray)
                };

                button.Click += (s, e) => Close();

                mainPanel.Children.Add(button);
            }

            // Set content
            Content = mainPanel;
        }

        // Static method to show the dialog similar to Xamarin's approach
        public static async Task<string> ShowActionSheet(
            Window owner,
            string title,
            string cancelButton,
            string[] actions)
        {
            var dialog = new ActionSheetDialog(title, cancelButton, actions);

            // Show dialog and wait for result
            var result = await dialog.ShowDialog<string>(owner);
            return result;
        }
    }
}
