using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleCalculator
{
    //----------------------------------------------------------------
    //  PROGRAM TITLE:  SimpleCalculator
    //  AUTHOR:         Colin Haroutunian
    //  LAST MODIFIED:  January 31st, 2021
    //  PROGRAM USE:    For calculating the amount of water
    //                  a person may consume on a daily basis.
    //                  Includes calculations for volume based
    //                  on a cylinder, which represents the
    //                  user's water bottle.
    //----------------------------------------------------------------

    /// <summary>
    /// Interaction logic for CalculatorWindow.xaml
    /// </summary>
    public partial class CalculatorWindow : Window
    {
        public CalculatorWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// BUTTON CLICK EVENT FUNCTIONS
        /// (Clear, Calculate, ExitApp, Help)
        /// These functions' nested functions are listed beneath other summaries
        /// </summary>
        private void Clear(object sender, RoutedEventArgs e)
        {
            com_Opts.SelectedIndex = -1;
            txt_Diameter.Text = "";
            txt_Height.Text = "";
            txt_Volume.Text = "";
            rad_btn_input_cm.IsChecked = false;
            rad_btn_input_in.IsChecked = false;
            rad_btn_output_ml.IsChecked = false;
            rad_btn_output_oz.IsChecked = false;
            txt_Bottles.Text = "";
            lbl_Consumed.Content = "Total consumed:";
            tb_Error_List.Text = "";
        }

        private void Calculate(object sender, RoutedEventArgs e)
        {
            string msg = EnsureUserEntries();
            if (msg == "")
            {
                double conversion = DetermineConversion();
                double volume = RetrieveVolume();
                double correctVol = volume / conversion;
                int iVol = Convert.ToInt32(correctVol);
                int.TryParse(txt_Bottles.Text, out int iQuantity);
                string unitsOut = GetOutputUnits();
                lbl_Consumed.Content = "Total consumed: " + (iVol * iQuantity).ToString() + " " + unitsOut;
            }
            else
            {
                tb_Error_List.Text = msg;
            }
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenHelp(object sender, RoutedEventArgs e)
        {
            HelpWindow window = new HelpWindow();
            window.Show();
        }

        /// <summary>
        /// COMBO BOX EVENT LISTENER
        /// Changing the combo box item freezes buttons and greys out labels
        /// This ensures the user does not mistakenly input more information than needed
        /// </summary>
        private void ComboBoxItemChange(object sender, SelectionChangedEventArgs e)
        {
            if (com_Opts.SelectedItem == cbi_Opt_Known)
            {
                rad_btn_input_cm.Content = "Milliliters (ml)";
                rad_btn_input_in.Content = "Fluid ounces (oz)";
                lbl_Volume.Style = Resources["sty_lbl_enabled"] as Style;
                txt_Volume.Style = Resources["sty_txt_enabled"] as Style;
                lbl_Diameter.Style = Resources["sty_lbl_disabled"] as Style;
                txt_Diameter.Style = Resources["sty_txt_disabled"] as Style;
                lbl_Height.Style = Resources["sty_lbl_disabled"] as Style;
                txt_Height.Style = Resources["sty_txt_disabled"] as Style;
            }
            if (com_Opts.SelectedItem == cbi_Opt_Unknown)
            {
                rad_btn_input_cm.Content = "Centimeters (cm)";
                rad_btn_input_in.Content = "Inches (in)";
                lbl_Diameter.Style = Resources["sty_lbl_enabled"] as Style;
                txt_Diameter.Style = Resources["sty_txt_enabled"] as Style;
                lbl_Height.Style = Resources["sty_lbl_enabled"] as Style;
                txt_Height.Style = Resources["sty_txt_enabled"] as Style;
                lbl_Volume.Style = Resources["sty_lbl_disabled"] as Style;
                txt_Volume.Style = Resources["sty_txt_disabled"] as Style;
            }
            if (com_Opts.SelectedIndex == -1)
            {
                rad_btn_input_cm.Content = "Centimeters (cm)";
                rad_btn_input_in.Content = "Inches (in)";
                lbl_Diameter.Style = Resources["sty_lbl_enabled"] as Style;
                txt_Diameter.Style = Resources["sty_txt_enabled"] as Style;
                lbl_Height.Style = Resources["sty_lbl_enabled"] as Style;
                txt_Height.Style = Resources["sty_txt_enabled"] as Style;
                lbl_Volume.Style = Resources["sty_lbl_enabled"] as Style;
                txt_Volume.Style = Resources["sty_txt_enabled"] as Style;
            }
        }

        /// <summary>
        /// CALCULATE BUTTON EVENT LISTENER: retrieving raw volume data
        /// Depending on the combo box item chosen, the user's records will be calculated for volume
        /// If the volume is known and provided, DirectVolume() is called
        /// If the user must enter in height and diameter (volume unknown), then LengthCalculation() is called
        /// </summary>
        private double RetrieveVolume()
        {
            double volume = 0;
            if (com_Opts.SelectedItem == cbi_Opt_Known)
            {
                volume = DirectVolume(volume);
            }
            if (com_Opts.SelectedItem == cbi_Opt_Unknown)
            {
                volume = LengthCalculation(volume);
            }
            return volume;
        }

        private double DirectVolume(double volume)
        {
            double.TryParse(txt_Volume.Text, out volume);
            return volume;
        }

        private double LengthCalculation(double volume)
        {
            double.TryParse(txt_Diameter.Text, out double bottleDiameter);
            double.TryParse(txt_Height.Text, out double bottleHeight);
            double radius = bottleDiameter * 0.5;
            double sqRadius = Math.Pow(radius, 2);
            volume = Math.PI * sqRadius * bottleHeight;
            return volume;
        }

        /// <summary>
        /// CALCULATE BUTTON EVENT LISTENER: getting units and conversion rate
        /// These functions determine what the user chose and input and output unit types
        /// If they are the same units, the conversion factor is equal to 1
        /// If they are different, a conversion factor will need to be retrieved
        /// This conversion factor will be sent back to Calculate()
        /// </summary>
        private double DetermineConversion()
        {
            double conversion = 1;
            string unitsIn = GetInputUnits();
            string unitsOut = GetOutputUnits();
            bool typesMatch = CompareUnitTypes(unitsIn, unitsOut);
            if (typesMatch == false)
            {
                conversion = GetUnitConversion(unitsIn, unitsOut);
            }
            return conversion;
        }

        private string GetInputUnits()
        {
            string type = "";
            bool isLength = CheckInputType();
            if (rad_btn_input_cm.IsChecked == true)
            {
                if (isLength == true)
                {
                    type = "cm";
                }
                if (isLength == false)
                {
                    type = "ml";
                }
            }
            if (rad_btn_input_in.IsChecked == true)
            {
                if (isLength == true)
                {
                    type = "in";
                }
                if (isLength == false)
                {
                    type = "oz";
                }
            }
            return type;
        }

        private bool CheckInputType()
        {
            bool isLength = false;
            string type = rad_btn_input_cm.Content.ToString();
            if (type == "Centimeters (cm)")
            {
                isLength = true;
            }
            return isLength;
        }

        private string GetOutputUnits()
        {
            string type = "";
            if (rad_btn_output_ml.IsChecked == true)
            {
                type = "ml";
            }
            if (rad_btn_output_oz.IsChecked == true)
            {
                type = "oz";
            }
            return type;
        }

        private bool CompareUnitTypes(string input, string output)
        {
            bool typesMatch = false;
            if (input == output)
            {
                typesMatch = true;
            }
            if (input == "cm" && output == "ml")
            {
                typesMatch = true;
            }
            return typesMatch;
        }

        private double GetUnitConversion(string input, string output)
        {
            // Conversion factors retrieved from Google
            double conversion = 1;
            if (input == "in" && output == "oz")
            {
                conversion = 1.805;
            }
            if (input == "in" && output == "ml")
            {
                conversion = 1/16.387;
            }
            if ((input == "ml" || input == "cm") && output == "oz")
            {
                conversion = 29.574;
            }
            if (input == "oz" && output == "ml")
            {
                conversion = 1/29.574;
            }
            return conversion;
        }

        /// <summary>
        /// CALCULATE BUTTON EVENT LISTENER: validating user entries
        /// Before calculating the volume and finding the conversion factors, the user entries are validated
        /// If they are incorrect, a message is added to the error log and a calculation is not performed
        /// </summary>
        private string EnsureUserEntries()
        {
            string msg = "";

            if (com_Opts.SelectedIndex == -1)
            {
                msg += "No volume specified in the options drop-down menu.\n\n";
            }
            else
            {
                if (com_Opts.SelectedItem == cbi_Opt_Known)
                {
                    msg += TestNumericValue(txt_Volume.Text, "volume");
                }

                if (com_Opts.SelectedItem == cbi_Opt_Unknown)
                {
                    msg += TestNumericValue(txt_Diameter.Text, "diameter");
                    msg += TestNumericValue(txt_Height.Text, "height");
                }

                if (rad_btn_input_cm.IsChecked == false && rad_btn_input_in.IsChecked == false)
                {
                    msg += ErrorMsgRadioButtons("Input");
                }

                if (rad_btn_output_ml.IsChecked == false && rad_btn_output_oz.IsChecked == false)
                {
                    msg += ErrorMsgRadioButtons("Output");
                }

                msg += TestNumericValue(txt_Bottles.Text, "bottles");
            }

            return msg;
        }

        private string TestNumericValue(string value, string itemName)
        {
            string msg = "";
            if (value == "")
            {
                msg = $"No value specified as the {itemName}.\n\n";
            } 
            else
            {
                bool parsedVal = double.TryParse(value, out double ignore);
                if (parsedVal == false)
                {
                    msg = $"Incorrect syntax for value specified in {itemName}. Use only non-zero, positive numbers and decimals.\n\n";
                }
                if (ignore == 0)
                {
                    msg = $"The {itemName} value cannot be equal to 0. Use only non-zero, positive numbers and decimals.\n\n";
                }
            }
            return msg;
        }

        private string ErrorMsgRadioButtons(string unitIO)
        {
            string msg = "";
            msg = $"{unitIO} units not chosen in the {unitIO.ToLower()} button section.\n\n";
            return msg;
        }
    }
}
