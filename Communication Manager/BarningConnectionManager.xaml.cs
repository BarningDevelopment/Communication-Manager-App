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


namespace Communication_Manager
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private WlanInterface _interface;
        private WlanAvailableNetwork _network;
       

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
                            this.wifi_listView.Items.Add(new wifiConnect { AccesPoint = networkName, Signal = signal, BSSType = bssType, MAC = MAC, RSSID = RSSID });                                                
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



        public void MobileBroadbandAPN()
        {
            try
            {
                MbnInterfaceManager mbnInfMgr = new MbnInterfaceManager();
                IMbnInterfaceManager mbnInfMgrInterface = mbnInfMgr as IMbnInterfaceManager;
                if (mbnInfMgrInterface != null)
                {
                    IMbnInterface[] mobileInterfaces = mbnInfMgrInterface.GetInterfaces() as IMbnInterface[1];

                    if (mobileInterfaces != null && mobileInterfaces.Length > 0)
                    {
                        // Just use the first interface
                        IMbnSubscriberInformation subInfo = mobileInterfaces[0].GetSubscriberInformation();

                        if (subInfo != null)
                        {
                            string SIMNumber = subInfo.SimIccID;
                            // Get the connection profile
                            MbnConnectionProfileManager mbnConnProfileMgr = new MbnConnectionProfileManager();
                            IMbnConnectionProfileManager mbnConnProfileMgrInterface = mbnConnProfileMgr as IMbnConnectionProfileManager;
                            if (mbnConnProfileMgrInterface != null)
                            {
                                bool connProfileFound = false;
                                string profileName = String.Empty;

                                try
                                {
                                    IMbnConnectionProfile[] mbnConnProfileInterfaces = mbnConnProfileMgrInterface.GetConnectionProfiles(mobileInterfaces[0]) as IMbnConnectionProfile[];

                                    foreach (IMbnConnectionProfile profile in mbnConnProfileInterfaces)
                                    {
                                        string xmlData = profile.GetProfileXmlData();

                                        if (xmlData.Contains("<SimIccID>" + SIMNumber + "</SimIccID>"))
                                        {
                                            connProfileFound = true;
                                            bool updateRequired = false;

                                            // Check if the profile is set to auto connect
                                            XmlDocument xdoc = new XmlDocument();
                                            xdoc.LoadXml(xmlData);

                                            profileName = xdoc["MBNProfile"]["Name"].InnerText;

                                            if (xdoc["MBNProfile"]["ConnectionMode"].InnerText != "auto")
                                            {
                                                xdoc["MBNProfile"]["ConnectionMode"].InnerText = "auto";
                                                updateRequired = true;
                                            }

                                            // Check the APN settings
                                            if (xdoc["MBNProfile"]["Context"] == null)
                                            {
                                                XmlElement context = (XmlElement)xdoc["MBNProfile"].AppendChild(xdoc.CreateElement("Context", xdoc["MBNProfile"].NamespaceURI));
                                                context.AppendChild(xdoc.CreateElement("AccessString", xdoc["MBNProfile"].NamespaceURI));
                                                context.AppendChild(xdoc.CreateElement("Compression", xdoc["MBNProfile"].NamespaceURI));
                                                context.AppendChild(xdoc.CreateElement("AuthProtocol", xdoc["MBNProfile"].NamespaceURI));
                                                updateRequired = true;
                                            }

                                            if (xdoc["MBNProfile"]["Context"]["AccessString"].InnerText != APNAccessString)
                                            {
                                                xdoc["MBNProfile"]["Context"]["AccessString"].InnerText = APNAccessString;
                                                updateRequired = true;
                                            }
                                            if (xdoc["MBNProfile"]["Context"]["Compression"].InnerText != APNCompression)
                                            {
                                                xdoc["MBNProfile"]["Context"]["Compression"].InnerText = APNCompression;
                                                updateRequired = true;
                                            }
                                            if (xdoc["MBNProfile"]["Context"]["AuthProtocol"].InnerText != APNAuthProtocol)
                                            {
                                                xdoc["MBNProfile"]["Context"]["AuthProtocol"].InnerText = APNAuthProtocol;
                                                updateRequired = true;
                                            }

                                            if (xdoc["MBNProfile"]["Context"]["UserLogonCred"] == null && !String.IsNullOrEmpty(APNUsername))
                                            {
                                                XmlElement userLogonCred = (XmlElement)xdoc["MBNProfile"]["Context"].InsertAfter(xdoc.CreateElement("UserLogonCred", xdoc["MBNProfile"].NamespaceURI), xdoc["MBNProfile"]["Context"]["AccessString"]);
                                                userLogonCred.AppendChild(xdoc.CreateElement("UserName", xdoc["MBNProfile"].NamespaceURI));
                                                userLogonCred.AppendChild(xdoc.CreateElement("Password", xdoc["MBNProfile"].NamespaceURI));
                                                updateRequired = true;
                                            }

                                            if (xdoc["MBNProfile"]["Context"]["UserLogonCred"] != null && xdoc["MBNProfile"]["Context"]["UserLogonCred"]["UserName"].InnerText != APNUsername)
                                            {
                                                xdoc["MBNProfile"]["Context"]["UserLogonCred"]["UserName"].InnerText = APNUsername;
                                                updateRequired = true;
                                            }

                                            if (xdoc["MBNProfile"]["Context"]["UserLogonCred"] != null && xdoc["MBNProfile"]["Context"]["UserLogonCred"]["Password"] == null && !String.IsNullOrEmpty(APNUsername))
                                            {
                                                xdoc["MBNProfile"]["Context"]["UserLogonCred"].AppendChild(xdoc.CreateElement("Password", xdoc["MBNProfile"].NamespaceURI));
                                            }

                                            if (xdoc["MBNProfile"]["Context"]["UserLogonCred"] != null && xdoc["MBNProfile"]["Context"]["UserLogonCred"]["Password"].InnerText != APNPassword)
                                            {
                                                xdoc["MBNProfile"]["Context"]["UserLogonCred"]["Password"].InnerText = APNPassword;
                                                updateRequired = true;
                                            }

                                            if (updateRequired)
                                            {
                                                // Update the connection profile
                                                profile.UpdateProfile(xdoc.OuterXml);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (!ex.Message.Contains("Element not found"))
                                    {
                                        throw ex;
                                    }
                                }

                                if (!connProfileFound)
                                {
                                    // Create the connection profile
                                    XmlDocument xdoc = new XmlDocument();
                                    xdoc.AppendChild(xdoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                                    XmlElement mbnProfile = (XmlElement)xdoc.AppendChild(xdoc.CreateElement("MBNProfile", "http://www.microsoft.com/networking/WWAN/profile/v1"));
                                    mbnProfile.AppendChild(xdoc.CreateElement("Name", xdoc["MBNProfile"].NamespaceURI)).InnerText = SIMNumber;
                                    mbnProfile.AppendChild(xdoc.CreateElement("IsDefault", xdoc["MBNProfile"].NamespaceURI)).InnerText = "true";
                                    mbnProfile.AppendChild(xdoc.CreateElement("ProfileCreationType", xdoc["MBNProfile"].NamespaceURI)).InnerText = "DeviceProvisioned";
                                    mbnProfile.AppendChild(xdoc.CreateElement("SubscriberID", xdoc["MBNProfile"].NamespaceURI)).InnerText = subInfo.SubscriberID;
                                    mbnProfile.AppendChild(xdoc.CreateElement("SimIccID", xdoc["MBNProfile"].NamespaceURI)).InnerText = SIMNumber;
                                    mbnProfile.AppendChild(xdoc.CreateElement("HomeProviderName", xdoc["MBNProfile"].NamespaceURI)).InnerText = SIMNumber;
                                    mbnProfile.AppendChild(xdoc.CreateElement("AutoConnectOnInternet", xdoc["MBNProfile"].NamespaceURI)).InnerText = "true";
                                    mbnProfile.AppendChild(xdoc.CreateElement("ConnectionMode", xdoc["MBNProfile"].NamespaceURI)).InnerText = "auto";

                                    XmlElement context = (XmlElement)xdoc["MBNProfile"].AppendChild(xdoc.CreateElement("Context", xdoc["MBNProfile"].NamespaceURI));
                                    context.AppendChild(xdoc.CreateElement("AccessString", xdoc["MBNProfile"].NamespaceURI));
                                    XmlElement userLogonCred = (XmlElement)context.AppendChild(xdoc.CreateElement("UserLogonCred", xdoc["MBNProfile"].NamespaceURI));
                                    userLogonCred.AppendChild(xdoc.CreateElement("UserName", xdoc["MBNProfile"].NamespaceURI));
                                    userLogonCred.AppendChild(xdoc.CreateElement("Password", xdoc["MBNProfile"].NamespaceURI));
                                    context.AppendChild(xdoc.CreateElement("Compression", xdoc["MBNProfile"].NamespaceURI));
                                    context.AppendChild(xdoc.CreateElement("AuthProtocol", xdoc["MBNProfile"].NamespaceURI));

                                    xdoc["MBNProfile"]["Context"]["AccessString"].InnerText = APNAccessString;
                                    xdoc["MBNProfile"]["Context"]["UserLogonCred"]["UserName"].InnerText = APNUsername;
                                    xdoc["MBNProfile"]["Context"]["UserLogonCred"]["Password"].InnerText = APNPassword;
                                    xdoc["MBNProfile"]["Context"]["Compression"].InnerText = APNCompression;
                                    xdoc["MBNProfile"]["Context"]["AuthProtocol"].InnerText = APNAuthProtocol;

                                    profileName = xdoc["MBNProfile"]["Name"].InnerText;

                                    mbnConnProfileMgrInterface.CreateConnectionProfile(xdoc.OuterXml);
                                }

                                // Register the connection events
                                MbnConnectionManager connMgr = new MbnConnectionManager();
                                IConnectionPointContainer connPointContainer = connMgr as IConnectionPointContainer;

                                Guid IID_IMbnConnectionEvents = typeof(IMbnConnectionEvents).GUID;
                                IConnectionPoint connPoint;
                                connPointContainer.FindConnectionPoint(ref IID_IMbnConnectionEvents, out connPoint);

                                ConnectionEventsSink connEventsSink = new ConnectionEventsSink();
                                connPoint.Advise(connEventsSink, out cookie); if (showProgress) { MessageBox.Show("After registering events"); }

                                // Connect
                                IMbnConnection connection = mobileInterfaces[0].GetConnection();

                                if (connection != null)
                                {
                                    MBN_ACTIVATION_STATE state;
                                    string connectionProfileName = String.Empty;
                                    connection.GetConnectionState(out state, out connectionProfileName);

                                    if (state != MBN_ACTIVATION_STATE.MBN_ACTIVATION_STATE_ACTIVATED && state != MBN_ACTIVATION_STATE.MBN_ACTIVATION_STATE_ACTIVATING)
                                    {
                                        if (String.IsNullOrEmpty(connectionProfileName))
                                        {
                                            connectionProfileName = profileName;
                                        }
                                        uint requestID;
                                        connection.Connect(MBN_CONNECTION_MODE.MBN_CONNECTION_MODE_PROFILE, connectionProfileName, out requestID);

                                    }
                                    else
                                    {
                                        // Do nothing, already connected
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Connection not found.");
                                }
                            }
                            else
                            {
                                MessageBox.Show("mbnConnProfileMgrInterface is null.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("No subscriber info found.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("No mobile interfaces found.");
                    }
                }
                else
                {
                    MessageBox.Show("mbnInfMgrInterface is null.");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("SIM is not inserted."))
                {
                    SIMNumber. = "No SIM inserted.";
                }
                MessageBox.Show("LoginForm.DataConnection ConfigureWindowsDataConnection Error " + ex.Message);
            }
        }

        public void GetConnectionStatus()
        {
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
                                    SIMNumber = "Unable to read SIM info";
                                }
                            }
                            catch (Exception)
                            {
                                SIMNumber = "Unable to read SIM info";
                            }
                        }

                        // Check whether the connection is active
                        IMbnConnection connection = mobileInterfaces[0].GetConnection();

                        if (connection != null)
                        {
                            MBN_ACTIVATION_STATE state;
                            string profileName = String.Empty;
                            connection.GetConnectionState(out state, out profileName);

                            Connected = (state == MBN_ACTIVATION_STATE.MBN_ACTIVATION_STATE_ACTIVATED);
                        }
                        else
                        {
                            MessageBox.Show("Connection not found.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("No mobile interfaces found.");
                    }
                }
                else
                {
                    MessageBox.Show("mbnInfMgrInterface is null.");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("SIM is not inserted."))
                {
                    SIMNumber = "No SIM inserted.";
                }
                else
                {
                    MessageBox.Show("LoginForm.DataConnection GetWindowsMobileDataStatus " + ex.Message);
                }
                PhoneSignal = 0;
                PhoneNetwork = "Unknown";
            }
        }
        //mobile elements


        private void mobile_checkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (mobile_checkBox.IsChecked == true)
            {
                mobile_password_label.Content = "mobile connections are beeing collected......";


                //get connected to UMTS mobile broadband          

                MbnInterfaceManager mbnInfMgr = new MbnInterfaceManager();
                IMbnInterfaceManager infMgr = (IMbnInterfaceManager)mbnInfMgr;


                MbnConnectionManager mbnConnectionMgr = new MbnConnectionManager();
                IMbnConnectionManager ImbnConnectionMgr = (IMbnConnectionManager)mbnConnectionMgr;


                if (ImbnConnectionMgr == null)
                {
                    mobile_password_label.Content = "no mobile connections available";
                    


                }else{
                    IMbnConnection[] connections = (IMbnConnection[])ImbnConnectionMgr.GetConnections();
                }
                /*
                    foreach (IDbConnection conn in connections)
                    {
                        IMbnInterface mobileInterface = infMgr.GetInterface(conn.InterfaceID) as IMbnInterface;
                        MBN_INTERFACE_CAPS caps = mobileInterface.GetInterfaceCapability();

                        MBN_PROVIDER provider = mobileInterface.GetHomeProvider();

                        List<string> InterfaceCaps = new List<string>();
                        InterfaceCaps.Add(caps.deviceID);
                        //MessageBox.Show("Device Id :" + caps.deviceID + "DataClass: " + caps.cellularClass + "Manufacturer: " + caps.manufacturer + "Model : " + caps.model + "Firmware Version: " + caps.firmwareInfo);

                    }

                }
                else
                {
                    mobile_password_label.Content = "hjhjkljlkjlkjljl";

                }
                */
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
