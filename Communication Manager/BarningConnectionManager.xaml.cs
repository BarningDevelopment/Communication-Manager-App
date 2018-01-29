using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NativeWifi;
using System.Runtime.InteropServices;
using SimpleWifi;
using SimpleWifi.Win32;
using SimpleWifi.Win32.Interop;
using System;
using System.Data;

using MbnApi;
using System.Xml;
using System.Net.NetworkInformation;

namespace ConnectionManager
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private WlanInterface _interface;
        private WlanAvailableNetwork _network;
        private int PhoneSignal;
        private uint cookie;
        private bool showProgress;

        public string PhoneNetwork { get; private set; }
        public string SIMNumber { get; private set; }
        public string APNAccessString { get; private set; }
        public string APNAuthProtocol { get; private set; }
        public string APNUsername { get; private set; }
        
        public string APNPassword { get; private set; }
        public string APNCompression { get; private set; }

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



                NativeWifi.WlanClient client = new NativeWifi.WlanClient();

                try
                {
                    wifi_password_label.Content = "";

                    foreach (NativeWifi.WlanClient.WlanInterface wlanIface in client.Interfaces)
                    {
                        //string asdsadf = wlanIface.InterfaceName;
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
                            string bssType = network.dot11BssType + "";
                            string MAC = tMac;
                            string RSSID = rss.ToString();

                            // Populate list
                            
                            wifi_listView.Items.Add(new wifiConnect { AccesPoint = networkName, Signal = signal, BSSType = bssType, MAC = MAC, RSSID = RSSID });                                                
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
            Wifi wifi = new Wifi();

            // get list of access points
            IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints();

            // for each access point from list
            foreach (AccessPoint ap in accessPoints)
            {
                if (ap.IsConnected)
                {
                    wifiConnect selectedItem = (wifiConnect)wifi_listView.SelectedItem;
                    wifi_password_label.Content = "Already connected to " + selectedItem.AccesPoint;
                    return;
                }else { 

                    Console.WriteLine("ap: {0}\r\n", ap.Name);
                    //check if SSID is desired
                    wifiConnect selectedAp = (wifiConnect)wifi_listView.SelectedItem;

                    if (ap.Name.StartsWith(selectedAp.AccesPoint))
                    {
                        //verify connection to desired SSID
                   
                        wifi_password_label.Content= ("connected:"+ ap.Name + "password needed: "+ ap.IsConnected +  "profile"+  ap.HasProfile+"\r\n");
                                                
                        wifi_password_label.Content =("Trying to connect..\r\n");
                        //AuthRequest authRequest = new AuthRequest(ap);
                        //authRequest.Password = "!VrH*DY^%4mdf&582GD8";                        
                        ConnectToWifi(ap, wifi_passwordBox.Password);
                    }
                }
            }
        }

        private bool ConnectToWifi(SimpleWifi.AccessPoint ap, string password)
        {
           SimpleWifi.AuthRequest authRequest = new SimpleWifi.AuthRequest(ap);
           authRequest.Password = wifi_passwordBox.Password;
            return ap.Connect(authRequest);
        }

        private void wifi_listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((wifi_checkBox.IsChecked == true) && (wifi_listView.SelectedItem != null))
            {
                wifiConnect selectedItem = (wifiConnect)wifi_listView.SelectedItem;

                if (selectedItem != null)
                {
                    wifi_password_label.Content = ("AccesPoint " + selectedItem.AccesPoint + " is selected");
                }
            }
        }

        private void wifi_password_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }









        //mobile stuk  

        public void mobile_checkBox_Checked(object sender, RoutedEventArgs e)
        {
            //get the connection state
            try
            {
                MbnInterfaceManager mbnInfMgr = new MbnInterfaceManager();
                IMbnInterfaceManager mbnInfMgrInterface = mbnInfMgr as IMbnInterfaceManager;
                if (mbnInfMgrInterface != null)
                {
                    IMbnInterface[] mobileInterfaces = mbnInfMgrInterface.GetInterfaces() as IMbnInterface[];
                    if (mobileInterfaces != null && mobileInterfaces.Length > 0)
                    {
                        // Use the first interface, as there should only be one mobile data adapter
                        IMbnSignal signalDetails = mobileInterfaces[0] as IMbnSignal;

                        Int32.TryParse(signalDetails.GetSignalStrength().ToString(), out PhoneSignal);
                        PhoneSignal = Convert.ToInt32(((float)PhoneSignal / 16) * 100);

                        MBN_PROVIDER provider = mobileInterfaces[0].GetHomeProvider();
                        PhoneNetwork = provider.providerName.ToString();

                        if (String.IsNullOrEmpty(SIMNumber))
                        {
                            try
                            {
                                IMbnSubscriberInformation subInfo = mobileInterfaces[0].GetSubscriberInformation();

                                if (subInfo != null)
                                {
                                    SIMNumber = subInfo.SimIccID;
                                }
                                else
                                {
                                    mobile_password_label.Content = "Unable to read SIM info";
                                }
                            }
                            catch (Exception)
                            {
                                mobile_password_label.Content = "Unable to read SIM info";
                            }
                        }

                        // Check whether the connection is active
                        IMbnConnection connection = mobileInterfaces[0].GetConnection();

                        if (connection != null)
                        {
                            MBN_ACTIVATION_STATE state;
                            string profileName = String.Empty;
                            connection.GetConnectionState(out state, out profileName);

                            var Connected = (state == MBN_ACTIVATION_STATE.MBN_ACTIVATION_STATE_ACTIVATED);
                        }
                        else
                        {
                            mobile_password_label.Content=("Connection not found.");
                        }
                    }
                    else
                    {
                        mobile_password_label.Content=("No mobile interfaces found.");
                    }
                }
                else
                {
                    mobile_password_label.Content=("mbnInfMgrInterface is null.");
                }
            }
            catch (Exception ex)
            {               
                if (ex.Message.Contains("SIM is not inserted."))
                {
                    mobile_password_label.Content = "No SIM inserted.";
                }
                else
                {
                    mobile_password_label.Content=("No mobile APN available");
                }
                PhoneSignal = 0;
                PhoneNetwork = "Unknown";
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
