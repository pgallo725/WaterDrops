using System;
using System.Linq;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace WaterDrops
{
    public sealed partial class MainPage : Page
    {
        // List of ValueTuple holding the Navigation Tag and the relative Navigation Page
        private readonly Dictionary<string, Type> pages = new Dictionary<string, Type>
        {
            { "water", typeof(WaterPage) },
            { "health", typeof(BMICalculatorPage) },
            { "settings", typeof(SettingsPage) }
        };


        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += (sender, e) =>
            {
                // Select the first page to be loaded in the content frame
                NavigationBar.SelectedItem = NavigationBar.MenuItems[1];
            };
        }


        // Handle navigation across multiple pages using the top navigation bar
        private void NavigationBar_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                // Navigate to the settings page
                ContentFrame.Navigate(typeof(SettingsPage), null, args.RecommendedNavigationTransitionInfo);
            }
            else
            {
                string tag = args.SelectedItemContainer.Tag.ToString();
                Type newPageType = pages.GetValueOrDefault(tag);

                // Get the page type before navigation to prevent duplicate entries in the backstack
                Type prevPageType = ContentFrame.CurrentSourcePageType;

                // Only navigate if the selected page isn't currently loaded
                if (!(newPageType is null) && !Equals(prevPageType, newPageType))
                {
                    ContentFrame.Navigate(newPageType, null, args.RecommendedNavigationTransitionInfo);
                }
            }
        }

        private void NavigationBar_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }


        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavigationBar.IsBackEnabled = ContentFrame.CanGoBack &&
                ContentFrame.SourcePageType != typeof(WaterPage);

            if (ContentFrame.SourcePageType == typeof(SettingsPage))
            {
                // SettingsItem is not part of NavigationBar.MenuItems, and doesn't have a tag
                NavigationBar.SelectedItem = (NavigationViewItem)NavigationBar.SettingsItem;
            }
            else if (ContentFrame.SourcePageType != null)
            {
                string tag = pages.First(i => i.Value == e.SourcePageType).Key;

                NavigationBar.SelectedItem = NavigationBar.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(n => n.Tag.Equals(tag));
            }
        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

    }
}
