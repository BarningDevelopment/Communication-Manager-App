using System;
using MbnApi;
using System.Net.NetworkInformation;


namespace Communication_Manager
{
    class MBNConnect
    {

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
        public int MaxBandWidth { get; internal set; }
        public int BytesSentSpeed { get; internal set; }
        public string Name { get; internal set; }
        public object Speed { get; internal set; }
        public PhysicalAddress Adress { get; internal set; }
        public string Netwerk { get; internal set; }
        public string Id { get; internal set; }

        public void GetConnectionStatus()
        {
            try
            {
                MbnInterfaceManager mbnInfMgr = new MbnInterfaceManager();
                IMbnInterfaceManager mbnInfMgrInterface = mbnInfMgr as IMbnInterfaceManager;
                if (mbnInfMgrInterface == null)
                {
                    string connectionMessage = "no connection found!";
                    
                }

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
                                    SIMNumber = (subInfo.SimIccID);
                                }
                                else
                                {
                                    Console.WriteLine("Unable to read SIM info");
                                }
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Unable to read SIM info");
                            }
                        }
                        else
                        {

                        }
                    }
                }
                Console.WriteLine("no good connection");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
           

        public int GetMaxBandwidth()
        {
            int maxBandwidth = 0;
            NetworkInterface[] networkIntrInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var networkInterface in networkIntrInterfaces)
            {
                IPv4InterfaceStatistics interfaceStats = networkInterface.GetIPv4Statistics();
                int bytesSentSpeed = (int)(interfaceStats.BytesSent);
                int bytesReceivedSpeed = (int)(interfaceStats.BytesReceived);

                if (bytesSentSpeed + bytesReceivedSpeed > maxBandwidth)
                {
                    maxBandwidth = bytesSentSpeed + bytesReceivedSpeed;
                }
                return maxBandwidth;
            }
            return 0;
        }

       
        public void OnConnectComplete(IMbnConnection newConnection, uint requestID, int status)
        {
            Console.WriteLine("OnConnectComplete");
        }
        public void OnConnectStateChange(IMbnConnection newConnection)
        {
            MBN_ACTIVATION_STATE activationState;
            string profileName;
            newConnection.GetConnectionState(out activationState, out profileName);
            Console.WriteLine("OnConnectStateChange - " + profileName + " - " + activationState);
        }

        public void OnDisconnectComplete(IMbnConnection newConnection, uint requestID, int status)
        {
            Console.WriteLine("OnDisconnectComplete");
        }

        public void OnVoiceCallStateChange(IMbnConnection newConnection)
        {
            Console.WriteLine("OnVoiceCallStateChange");
        }
    }
}
        

        
            
        





    
