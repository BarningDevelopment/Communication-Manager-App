﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using NativeWifi;
using SimpleWifi;
using System.Net.NetworkInformation;
using System.Xml;
using MBNConnect;
using MBNConnect;
using System.IO;
using MbnApi;

namespace ConnectionManager
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
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
        //wifi elements
        private void checkBox1_Checked(object sender, RoutedEventArgs e){}
               
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
                       Wlan.WlanBssEntry[] wlanBssEntries = wlanIface.GetNetworkBssList();

                        foreach (Wlan.WlanBssEntry network in wlanBssEntries)
                        {
                            int rss = network.rssi;
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
                    wifiConnect selectedAp = (wifiConnect)wifi_listView.SelectedItem;

                    if (ap.Name.StartsWith(selectedAp.AccesPoint))
                    {                        
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
                     Wifi wifi = new Wifi();

                    IEnumerable<AccessPoint> accessPoints = wifi.GetAccessPoints();

                    // for each access point from list
                    foreach (AccessPoint ap in accessPoints)
                    {
                        if (ap.Name.StartsWith(selectedItem.AccesPoint))
                            {
                            //verify connection to desired SSID
                            wifi_password_label.Content = ("connected:" + ap.Name + System.Environment.NewLine);
                            wifi_password_label.Content += ("password needed: " + ap.IsConnected+ System.Environment.NewLine);
                            wifi_password_label.Content += ("profile" + ap.HasProfile + System.Environment.NewLine);
                            }
                    }
                }
            }
        }        

        private void wifi_password_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }





        //mobile stuk
        public void mobile_checkBox_Checked(object sender, RoutedEventArgs e)
        {
            mobile_password_label.Content = "Checking mobile networks....";

            //create mobile profile if not exist
            CreateMobileProfile();
            //netsh mbn show profile

            MBNConnect.MBNConnect connectInfo = new MBNConnect.MBNConnect();

            NetworkInterface[] networkIntrInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in networkIntrInterfaces)
            {
                //select only mobile apn
                if (networkInterface.Name.Contains("Mobile Broadband adapter Mobiel")){
                    IPv4InterfaceStatistics interfaceStats = networkInterface.GetIPv4Statistics();
                    int bytesSentSpeed = (int)(interfaceStats.BytesSent);

                    string id = (string)networkInterface.Id;
                    string name = (string)networkInterface.Name;
                    string description = (string)networkInterface.Description;
                    PhysicalAddress physical_address = networkInterface.GetPhysicalAddress();

                    //create the view
                    var gridview = new GridView();
                    this.mobile_listView.View = gridview;

                    gridview.Columns.Add(new GridViewColumn
                    {
                        Header = "Name",
                        DisplayMemberBinding = new System.Windows.Data.Binding("Name")
                    });

                    gridview.Columns.Add(new GridViewColumn
                    {
                        Header = "Id",
                        DisplayMemberBinding = new System.Windows.Data.Binding("Id")
                    });

                    gridview.Columns.Add(new GridViewColumn
                    {
                        Header = "Adress",
                        DisplayMemberBinding = new System.Windows.Data.Binding("Adress")
                    });
                    gridview.Columns.Add(new GridViewColumn
                    {
                        Header = "Netwerk",
                        DisplayMemberBinding = new System.Windows.Data.Binding("Netwerk")
                    });
                    gridview.Columns.Add(new GridViewColumn
                    {
                        Header = "MaxBandWidth",
                        DisplayMemberBinding = new System.Windows.Data.Binding("MaxBandWidth")
                    });

                    gridview.Columns.Add(new GridViewColumn
                    {
                        Header = "BytesSentSpeed",
                        DisplayMemberBinding = new System.Windows.Data.Binding("BytesSentSpeed")
                    });

                    int maxBandWidth = connectInfo.GetMaxBandwidth();

                    mobile_listView.Items.Add(new MBNConnect.MBNConnect { Name = name, Id = id, Adress = physical_address, Netwerk = networkInterface.Description, MaxBandWidth = maxBandWidth, BytesSentSpeed = bytesSentSpeed });
                }
            }
        }     
                    
          public void ForceMobileConnection()
        {
            //IMbnConnectionProfileManager::CreateConnectionProfile method
        }

        public void CreateMobileProfile()
        {
            //create mobile profile v4 windows10
            //https://msdn.microsoft.com/nl-nl/library/windows/desktop/mt243438

            XmlWriterSettings settings = new XmlWriterSettings();
           settings.OmitXmlDeclaration = true;

            string MyNewPath = System.IO.Path.Combine("C://", "temp");
            System.IO.Directory.CreateDirectory(MyNewPath);
            string path = "c:\\temp\\MBNProfile.xml";

            using (XmlWriter writer = XmlWriter.Create(path, settings))
           {             

            XmlDocument xmlDocument = new XmlDocument();
                //writer.WriteStartElement(m_strFilePath);

                writer.WriteStartElement("MBNProfileExt" , "http://www.microsoft.com/networking/WWAN/profile/v4");
                writer.WriteElementString("Name", "!!##MBIMModemProvisionedContextInternetProfile##8931087115077657062");
                writer.WriteElementString("Description", "ModemProvisionedProfile##vzwinternet created by Mwalima Peltenburg");
                writer.WriteElementString("IsDefault", "true");
                writer.WriteElementString("ProfileCreationType", "DeviceProvisiond");
                writer.WriteElementString("SubscriberID", "204080806249856");
                writer.WriteElementString("SimIccID", "8931087115077657062");
                writer.WriteElementString("HomeProviderName", "KPN");
                writer.WriteElementString("ConnectionMode", "auto");
                writer.WriteElementString("ICONFilePath", Path.GetFullPath("Images/Vodafone.bmp"));                              
                writer.WriteElementString("ProfileCreationType", "UserProvisioned");                              
                writer.WriteElementString("AutoConnectOnInternet","true");        
                writer.WriteEndElement();
               writer.Flush();
                
               mobile_password_label.Content = "Profile is created";
           }
       }
       private void mobile_connect_button_Click(object sender, RoutedEventArgs e)
       {
           //connect to mobile profile
           mobile_password_label.Content = "Connected to Mobile Broadband Network";

            MBNConnect.MBNConnect allive = new MBNConnect.MBNConnect();
            MbnInterfaceManager mbnInfMgr = new MbnInterfaceManager();
            IMbnInterfaceManager mbnInfMgrInterface = mbnInfMgr as IMbnInterfaceManager;

            IMbnInterface[] mobileInterfaces = mbnInfMgrInterface.GetInterfaces() as IMbnInterface[];
            IMbnInterface inf =(IMbnInterface)mobileInterfaces[1];
            IMbnConnection conn = inf.GetConnection();
            allive.KeepConnectionAllive(conn);
        }

       

       private void mobile_password_TextBox_TextChanged(object sender, TextChangedEventArgs e)
       {

       }

       private void listView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
       {
           if ((mobile_checkBox.IsChecked == true) && (mobile_listView.SelectedItem != null))
           {

               mobile_password_label.Content = "Mobile Broadband Network selected";
           }
       }

   }
}
