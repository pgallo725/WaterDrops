using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Toolkit.Extensions;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.Resources;

namespace WaterDrops
{
    public sealed partial class PersonPage : Page
    {
        public PersonPage()
        {
            this.InitializeComponent();

            this.Loaded += (sender, e) =>
            {
                // Page initialization
                GenderComboBox.SelectedIndex = (int)App.User.Person.Gender;
                AgeTextBox.Text = App.User.Person.Age.ToString();

                WeightTextBox.Text = App.User.Person.Weight.ToString("0.#");
                HeightTextBox.Text = App.User.Person.Height.ToString("0.##");

                UpdateBodyMassIndex(App.User.Person, EventArgs.Empty);

                // Connect UI event handlers
                GenderComboBox.SelectionChanged += GenderComboBox_SelectionChanged;
                AgeTextBox.BeforeTextChanging += AgeTextBox_ValidateInput;
                AgeTextBox.KeyDown += TextBox_CheckEnter;
                AgeTextBox.LostFocus += AgeTextBox_Apply;
                WeightTextBox.BeforeTextChanging += WeightTextBox_ValidateInput;
                WeightTextBox.KeyDown += TextBox_CheckEnter;
                WeightTextBox.LostFocus += WeightTextBox_Apply;
                HeightTextBox.BeforeTextChanging += HeightTextBox_ValidateInput;
                HeightTextBox.KeyDown += TextBox_CheckEnter;
                HeightTextBox.LostFocus += HeightTextBox_Apply;

                // Connect event hanlder to PersonChanged event
                App.User.Person.PersonChanged += UpdateBodyMassIndex;
            };

            this.Unloaded += (sender, e) =>
            {
                // Disconnect all event handlers
                App.User.Person.PersonChanged -= UpdateBodyMassIndex;

                GenderComboBox.SelectionChanged -= GenderComboBox_SelectionChanged;
                AgeTextBox.BeforeTextChanging -= AgeTextBox_ValidateInput;
                AgeTextBox.KeyDown -= TextBox_CheckEnter;
                AgeTextBox.LostFocus -= AgeTextBox_Apply;
                WeightTextBox.BeforeTextChanging -= WeightTextBox_ValidateInput;
                WeightTextBox.KeyDown -= TextBox_CheckEnter;
                WeightTextBox.LostFocus -= WeightTextBox_Apply;
                HeightTextBox.BeforeTextChanging -= HeightTextBox_ValidateInput;
                HeightTextBox.KeyDown -= TextBox_CheckEnter;
                HeightTextBox.LostFocus -= HeightTextBox_Apply;
            };
        }


        private void UpdateBodyMassIndex(Person person, EventArgs args)
        {
            // Update BMI TextBlock
            BMITextBlock.Text = person.BodyMassIndex.ToString("0.##") + " kg/m2";

            ResourceLoader resources = ResourceLoader.GetForCurrentView();

            SolidColorBrush brush = new SolidColorBrush();
            string text;

            // Update "Your status" TextBlock
            switch (App.User.Person.HealthStatus)
            {
                case Person.HealthStatusType.Underweight:
                    brush.Color = Colors.BlueViolet;
                    text = resources.GetString("UnderweightString");
                    break;

                case Person.HealthStatusType.Healthy:
                    brush.Color = Colors.Green;
                    text = resources.GetString("HealthyString");
                    break;

                case Person.HealthStatusType.Overweight:
                    brush.Color = Colors.Orange;
                    text = resources.GetString("OverweightString");
                    break;

                case Person.HealthStatusType.Obese:
                    brush.Color = Colors.OrangeRed;
                    text = resources.GetString("ObeseString");
                    break;

                case Person.HealthStatusType.ExtremelyObese:
                    brush.Color = Colors.Red;
                    text = resources.GetString("ExtremelyObeseString");
                    break;

                default:
                    brush = this.Resources["TextBlockSemiLightForeground"] as SolidColorBrush;
                    text = "---";
                    break;
            }

            HealthStatusTextBlock.Foreground = brush;
            HealthStatusTextBlock.Text = text;
        }


        private void GenderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle gender selection
            ComboBox comboBox = sender as ComboBox;
            App.User.Person.Gender = (Person.GenderType)comboBox.SelectedIndex;
        }


        private void TextBox_CheckEnter(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Accept)
            {
                this.Focus(FocusState.Pointer);
            }
        }


        private void AgeTextBox_ValidateInput(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = !(args.NewText.IsNumeric() || args.NewText.Length == 0);
        }

        private void AgeTextBox_Apply(object sender, RoutedEventArgs args)
        {
            // Handle a new age value inserted by the user
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length == 0)
                textBox.Text = "0";

            // Parse the content of the TextBox
            int ageValue = int.Parse(textBox.Text, CultureInfo.InvariantCulture);

            // Update Person data structure (age)
            App.User.Person.Age = ageValue;
        }


        private readonly Regex weightRegex = new Regex("^\\d*([\\.,]\\d?)?$");
        private void WeightTextBox_ValidateInput(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!weightRegex.IsMatch(args.NewText))
            {
                args.Cancel = true;
            }
        }

        private void WeightTextBox_Apply(object sender, RoutedEventArgs args)
        {
            TextBox textBox = sender as TextBox;

            if (textBox.Text.Length == 0)
                textBox.Text = "0";

            string weightValueStr = textBox.Text.Replace(',', '.');

            // Add starting 0 if the value begins with a decimal separator
            if (weightValueStr.StartsWith('.'))
                textBox.Text = weightValueStr.Insert(0, "0");

            // Remove trailing decimal separators
            if (weightValueStr.EndsWith('.'))
                textBox.Text = weightValueStr.Truncate(weightValueStr.Length - 1);

            // Parse the content of the TextBox
            float weightValue = float.Parse(weightValueStr, CultureInfo.InvariantCulture);

            // Update Person data structure (weight)
            App.User.Person.Weight = weightValue;
        }


        private readonly Regex heightRegex = new Regex("^\\d*([\\.,]\\d{0,2})?$");
        private void HeightTextBox_ValidateInput(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!heightRegex.IsMatch(args.NewText))
            {
                args.Cancel = true;
            }
        }

        private void HeightTextBox_Apply(object sender, RoutedEventArgs args)
        {
            TextBox textBox = sender as TextBox;

            // Avoid empty strings
            if (textBox.Text.Length == 0)
                textBox.Text = "0";

            string heightValueStr = textBox.Text.Replace(',', '.');

            // Add starting 0 if the value begins with a decimal separator
            if (heightValueStr.StartsWith('.'))
                textBox.Text = heightValueStr.Insert(0, "0");

            // Remove trailing decimal separators
            if (heightValueStr.EndsWith('.'))
                textBox.Text = heightValueStr.Truncate(heightValueStr.Length - 1);

            // Parse the content of the TextBox
            float heightValue = float.Parse(heightValueStr, CultureInfo.InvariantCulture);

            // Update Person data structure (height)
            App.User.Person.Height = heightValue;
        }
    }
}
