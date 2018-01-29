using System;
using MbnApi;

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

                            var Connected = (state == MBN_ACTIVATION_STATE.MBN_ACTIVATION_STATE_ACTIVATED);
                        }
                        else
                        {
                            Console.WriteLine("Connection not found.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No mobile interfaces found.");
                    }
                }
                else
                {
                    Console.WriteLine("mbnInfMgrInterface is null.");
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
                    Console.WriteLine("LoginForm.DataConnection GetWindowsMobileDataStatus " + ex.Message);
                }
                PhoneSignal = 0;
                PhoneNetwork = "Unknown";
            }
        }

        class ConnectionEventsSink : IMbnConnectionEvents
        {
            public ConnectionEventsSink() { }
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
    }

