using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NativeWifi;



namespace Communication_Manager
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private static Wifi wifi;

        public Window1()
        {
            InitializeComponent();
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {

        }






        //wifi elements
        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
            }
               
        }


        private void wifi_listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

           }
        private void wifi_connect_button_Click(object sender, RoutedEventArgs e)
        {

            
            
        }

       

        private void wifi_password_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        //mobile elements
        private void checkBox1_Copy_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox.IsChecked == true)
            {
                checkBox1_Copy.IsChecked = false;

                mobile_password_label.Content = "mobile connections are beeing collected......";

            }
        }

        private void mobile_connect_button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mobile_password_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

    }
}
