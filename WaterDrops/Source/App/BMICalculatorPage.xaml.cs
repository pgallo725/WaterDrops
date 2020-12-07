using Microsoft.Toolkit.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace WaterDrops
{
    public sealed partial class BMICalculatorPage : Page
    {
        private UserData user;

        public BMICalculatorPage()
        {
            this.InitializeComponent();

            // Try to cache the page, if the cache size allows it
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.Loaded += (sender, e) =>
            {
                // Page initialization
                GenderComboBox.SelectedIndex = (int)user.Person.Gender;
                AgeTextBox.Text = user.Person.Age.ToString();

                WeightTextBox.Text = user.Person.Weight.ToString("0.#");
                HeightTextBox.Text = user.Person.Height.ToString("0.##");

                UpdateBodyMassIndex(user.Person, EventArgs.Empty);

                // Connect event hanlder to PersonChanged event
                user.Person.PersonChanged += UpdateBodyMassIndex;
            };

            this.Unloaded += (sender, e) =>
            {
                // Disconnect event handlers
                user.Person.PersonChanged -= UpdateBodyMassIndex;
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Catch the parameter that has been forwarded from the MainPage
            this.user = (UserData)e.Parameter;
        }

        private void UpdateBodyMassIndex(Person person, EventArgs args)
        {
            // Update BMI TextBlock
            BMITextBlock.Text = person.BodyMassIndex.ToString("0.##") + " kg/m2";

            SolidColorBrush brush = new SolidColorBrush();
            string text;

            // Update "Your status" TextBlock
            switch (user.Person.HealthStatus)
            {
                case Person.HealthStatusType.Underweight:
                    brush.Color = Colors.BlueViolet;
                    text = "Underweight";
                    break;

                case Person.HealthStatusType.Healthy:
                    brush.Color = Colors.Green;
                    text = "Healthy";
                    break;

                case Person.HealthStatusType.Overweight:
                    brush.Color = Colors.Orange;
                    text = "Overweight";
                    break;

                case Person.HealthStatusType.Obese:
                    brush.Color = Colors.OrangeRed;
                    text = "Obese";
                    break;

                case Person.HealthStatusType.ExtremelyObese:
                    brush.Color = Colors.Red;
                    text = "Extremely obese";
                    break;

                default:
                    brush.Color = Colors.DimGray;
                    text = "---";
                    break;
            }

            HealthStatusTextBlock.Foreground = brush;
            HealthStatusTextBlock.Text = text;
        }


        private void GenderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            // Handle gender selection
            ComboBox comboBox = sender as ComboBox;
            user.Person.Gender = (Person.GenderType)comboBox.SelectedIndex;
        }


        private void TextBox_CheckEnter(object sender, KeyRoutedEventArgs e)
        {
            if (!this.IsLoaded)
                return;

            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Accept)
            {
                this.Focus(FocusState.Pointer);
            }
        }


        private void AgeTextBox_ValidateInput(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!this.IsLoaded)
                return;

            if (!args.NewText.IsNumeric())
                args.Cancel = true;
        }

        private void AgeTextBox_Apply(object sender, RoutedEventArgs args)
        {
            if (!this.IsLoaded)
                return;

            // Handle a new age value inserted by the user
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length == 0)
                textBox.Text = "0";

            // Parse the content of the TextBox
            int ageValue = int.Parse(textBox.Text, CultureInfo.InvariantCulture);

            // Update Person data structure (age)
            user.Person.Age = ageValue;
        }


        private Regex weightRegex = new Regex("^\\d*([\\.,]\\d?)?$");
        private void WeightTextBox_ValidateInput(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!this.IsLoaded)
                return;

            if (!weightRegex.IsMatch(args.NewText))
            {
                args.Cancel = true;
            }
        }

        private void WeightTextBox_Apply(object sender, RoutedEventArgs args)
        {
            if (!this.IsLoaded)
                return;

            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length == 0)
                textBox.Text = "0";

            // Parse the content of the TextBox
            string weightValueStr = textBox.Text.Replace(',', '.');
            float weightValue = float.Parse(weightValueStr, CultureInfo.InvariantCulture);

            // Update Person data structure (weight)
            user.Person.Weight = weightValue;
        }


        private Regex heightRegex = new Regex("^\\d*([\\.,]\\d{0,2})?$");
        private void HeightTextBox_ValidateInput(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!this.IsLoaded)
                return;

            if (!heightRegex.IsMatch(args.NewText))
            {
                args.Cancel = true;
            }
        }

        private void HeightTextBox_Apply(object sender, RoutedEventArgs args)
        {
            if (!this.IsLoaded)
                return;

            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length == 0)
                textBox.Text = "0";

            // Parse the content of the TextBox
            string heightValueStr = textBox.Text.Replace(',', '.');
            float heightValue = float.Parse(heightValueStr, CultureInfo.InvariantCulture);

            // Update Person data structure (height)
            user.Person.Height = heightValue;
        }
    }
}
