using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NativeWifi;
using System.Runtime.InteropServices;
using SimpleWifi;

namespace Communication_Manager
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
       
        public Window1()
        {
            InitializeComponent();
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            
        }






        //wifi elements
        private void wifi_checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (wifi_checkBox.IsChecked == true)
            {             

                //punt info in list grid
                var gridView = new GridView();
                this.wifi_listView.View = gridView;
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Accesspoints",
                    DisplayMemberBinding = new System.Windows.Data.Binding("AccesPoint")
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "Signal Strength",
                    DisplayMemberBinding = new System.Windows.Data.Binding("Signal")
                });

                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "BSSType",
                    DisplayMemberBinding = new System.Windows.Data.Binding("BSSType")
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "MAC",
                    DisplayMemberBinding = new System.Windows.Data.Binding("MAC")
                });
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = "RSSID",
                    DisplayMemberBinding = new System.Windows.Data.Binding("RSSID")
                });

                WlanClient client = new WlanClient();

                try
                {
                    wifi_password_label.Content = "";
                    foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                    {

                        Wlan.WlanBssEntry[] wlanBssEntries = wlanIface.GetNetworkBssList();

                        foreach (Wlan.WlanBssEntry network in wlanBssEntries)
                        {                            

                            int rss = network.rssi;
                            //     MessageBox.Show(rss.ToString());
                            byte[] macAddr = network.dot11Bssid;

                            string tMac = "";

                            for (int i = 0; i < macAddr.Length; i++)
                            {

                                tMac += macAddr[i].ToString("x2").PadLeft(2, '0').ToUpper();
                            }
                            Wlan.Dot11Ssid ssid = network.dot11Ssid;

                            string networkName = System.Text.Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
                            uint signal = network.linkQuality;
                            string bssType = network.dot11BssType + "la";
                            string MAC = tMac;
                            string RSSID = rss.ToString();


                            // Populate list
                            this.wifi_listView.Items.Add(new wifiConnect { AccesPoint = networkName, Signal = signal, BSSType = bssType, MAC = MAC, RSSID = RSSID });
                                                
                        }
                        wifiConnect selectedItem = (wifiConnect)wifi_listView.SelectedItem;
                       
                        if (selectedItem !=null)
                        {
                            
                            wifi_password_label.Content = "AccesPoint is selected" + selectedItem.AccesPoint;
                        }else
                        {
                            wifi_password_label.Content = "Please select an AccesPoint" ;
                        }
                    }

                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                            
            }else if (wifi_checkBox.IsChecked == false)
            {
                wifi_password_label.Content = "Please select connection type";
            }                      
            
        }


        
        private void wifi_connect_button_Click(object sender, RoutedEventArgs e)
        {
            wifi_password_label.Content = "Connet to wifi AP...";
           

            if (wifi_listView.SelectedItems.Count > 0 )
            {
                wifiConnect selectedItem = (wifiConnect)wifi_listView.SelectedItem;
                MessageBox.Show(selectedItem.AccesPoint);

                ListView selectAp = new ListView();
                selectAp.Items.Add(selectedItem.AccesPoint);
                SimpleWifi.AccessPoint ap = (AccessPoint)selectAp.Tag;

                if (ConnectToWifi(ap, wifi_passwordBox.Password))
                {
                    wifi_password_label.Content = "You are now connected to the internet";

                }
                else
                {
                    wifi_password_label.Content = "Something went wrong try again";
                }
            }
            else
            {
                wifi_password_label.Content = "Please select a network";
            }
            
            

        }

        private bool ConnectToWifi(SimpleWifi.AccessPoint ap, string password)
        {
            SimpleWifi.AuthRequest authRequest = new SimpleWifi.AuthRequest(ap);
           authRequest.Password = wifi_passwordBox.Password;

            return ap.Connect(authRequest);

        }
        private void wifi_listView_SelectionChanged(object sender, System.EventArgs e)
        {


        }

        private void wifi_password_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }





        //mobile elements
        private void mobile_checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (mobile_checkBox.IsChecked == true)
            {
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

        private void wifi_listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
