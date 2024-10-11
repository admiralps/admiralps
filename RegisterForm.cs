using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;
using System.Diagnostics;
using PortSIP;

namespace admiralps
{
    public partial class RegisterForm : Form, SIPCallbackEvents
    {
        private const int MAX_LINES = 9; // Maximum lines
        private const int LINE_BASE = 1;


        private Session[] _CallSessions = new Session[MAX_LINES];

        private bool _SIPInited = false;
        private bool _SIPLogined = false;
        private int _CurrentlyLine = LINE_BASE;


        private PortSIPLib _sdkLib;


        public RegisterForm()
        {
            InitializeComponent();
            
        }
        private int findSession(int sessionId)
        {
            int index = -1;
            for (int i = LINE_BASE; i < MAX_LINES; ++i)
            {
                if (_CallSessions[i].getSessionId() == sessionId)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }
        private byte[] GetBytes(string str)
        {
            return System.Text.Encoding.Default.GetBytes(str);
        }

        private string GetString(byte[] bytes)
        {
            return System.Text.Encoding.Default.GetString(bytes);
        }


        private string getLocalIP()
        {
            StringBuilder localIP = new StringBuilder();
            localIP.Length = 64;
            int nics = _sdkLib.getNICNums();
            for (int i = 0; i < nics; ++i)
            {
                _sdkLib.getLocalIpAddress(i, localIP, 64);
                if (localIP.ToString().IndexOf(":") == -1)
                {
                    // No ":" in the IP then it's the IPv4 address, we use it in our sample
                    break;
                }
                else
                {
                    // the ":" is occurs in the IP then this is the IPv6 address.
                    // In our sample we don't use the IPv6.
                }

            }

            return localIP.ToString();
        }
        private void loadDevices()
        {
            if (_SIPInited == false)
            {
                return;
            }

            int num = _sdkLib.getNumOfPlayoutDevices();
            for (int i = 0; i < num; ++i)
            {
                StringBuilder deviceName = new StringBuilder();
                deviceName.Length = 256;

            }


            num = _sdkLib.getNumOfRecordingDevices();
            for (int i = 0; i < num; ++i)
            {
                StringBuilder deviceName = new StringBuilder();
                deviceName.Length = 256;

            }


            num = _sdkLib.getNumOfVideoCaptureDevices();
            for (int i = 0; i < num; ++i)
            {
                StringBuilder uniqueId = new StringBuilder();
                uniqueId.Length = 256;
                StringBuilder deviceName = new StringBuilder();
                deviceName.Length = 256;


            }


            int volume = _sdkLib.getSpeakerVolume();     

            volume = _sdkLib.getMicVolume();

        }
        private void deRegisterFromServer()
        {
            if (_SIPInited == false)
            {
                return;
            }

            for (int i = LINE_BASE; i < MAX_LINES; ++i)
            {
                if (_CallSessions[i].getRecvCallState() == true)
                {
                    _sdkLib.rejectCall(_CallSessions[i].getSessionId(), 486);
                }
                else if (_CallSessions[i].getSessionState() == true)
                {
                    _sdkLib.hangUp(_CallSessions[i].getSessionId());
                }

                _CallSessions[i].reset();
            }

            if (_SIPLogined == true)
            {
                _sdkLib.unRegisterServer(100);
                _SIPLogined = false;
            }

            _sdkLib.removeUser();


            if (_SIPInited == true)
            {
                  _sdkLib.unInitialize();

                //
                // MUST called after _sdkLib.unInitliaze();
                //
                _sdkLib.releaseCallbackHandlers();
                _SIPInited = false;
            }


            _CurrentlyLine = LINE_BASE;


        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // Retrieve SIP account details from the form fields
            
            
            if (_SIPInited == true)
            {
                MessageBox.Show("You are already logged in.");
                return;
            }


            int SIPServerPort = 5060;
            


            int StunServerPort = 0;
            

            Random rd = new Random();
            int LocalSIPPort = rd.Next(1000, 5000) + 4000; // Generate the random port for SIP

            TRANSPORT_TYPE transportType = TRANSPORT_TYPE.TRANSPORT_UDP;
            /*switch (ComboBoxTransport.SelectedIndex)
            {
                case 0:
                    transportType = TRANSPORT_TYPE.TRANSPORT_UDP;
                    break;

                case 1:
                    transportType = TRANSPORT_TYPE.TRANSPORT_TLS;
                    break;

                case 2:
                    transportType = TRANSPORT_TYPE.TRANSPORT_TCP;
                    break;

                case 3:
                    transportType = TRANSPORT_TYPE.TRANSPORT_PERS;
                    break;
                default:
                    MessageBox.Show("The transport is not support.");
                    return;
            }*/
           


            //
            // Create the class instance of PortSIP VoIP SDK, you can create more than one instances and 
            // each instance register to a SIP server to support multiples accounts & providers.
            // for example:
            /*
            _sdkLib1 = new PortSIPLib(this);
            _sdkLib2 = new PortSIPLib(this);
            _sdkLib3 = new PortSIPLib(this);
            */


            _sdkLib = new PortSIPLib(this);

            //
            // Create and set the SIP callback handers, this MUST called before
            // _sdkLib.initialize();
            //
            _sdkLib.createCallbackHandlers();

            string logFilePath = "d:\\"; // The log file path, you can change it - the folder MUST exists
            string agent = "PortSIP VoIP SDK";
            string stunServer = "127.0.0.1";

            // Initialize the SDK
            int rt = _sdkLib.initialize(transportType,
                // Use 0.0.0.0 for local IP then the SDK will choose an available local IP automatically.
                // You also can specify a certain local IP to instead of "0.0.0.0", more details please read the SDK User Manual
                             "0.0.0.0",
                             LocalSIPPort,
                PORTSIP_LOG_LEVEL.PORTSIP_LOG_NONE,
                             logFilePath,
                             MAX_LINES,
                             agent,
                             0,
                             0,
                             "/",
                             "",
                             false);

            if (rt != 0)
            {
                _sdkLib.releaseCallbackHandlers();
                MessageBox.Show("Failed to initialize.");
                return;
            }
            MessageBox.Show("Initialized.");
            _SIPInited = true;

            loadDevices();

            string userName = txtUsername.Text;
            string password = txtPassword.Text;
            string sipDomain = txtDomain.Text;
            string SIPServer = txtProxy.Text;
            string displayName = txtUsername.Text+"@"+txtDomain.Text;
            string authName = txtUsername.Text+"@"+txtDomain.Text;


            int outboundServerPort = 0;
            string outboundServer = "";

            // Set the SIP user information
            rt = _sdkLib.setUser(userName,
                                       displayName,
                                       authName,
                                       password,
                                       sipDomain,
                                       SIPServer,
                                       SIPServerPort,
                                       stunServer,
                                       StunServerPort,
                                       outboundServer,
                                       outboundServerPort);
            if (rt != 0)
            {
                _sdkLib.unInitialize();
                _sdkLib.releaseCallbackHandlers();

                _SIPInited = false;

                MessageBox.Show("Failed to setUser.");
                return;
            }
            MessageBox.Show("Succeeded set user information.");

            SetSRTPType();

            string licenseKey = "PORTSIP_TEST_LICENSE";
            rt = _sdkLib.setLicenseKey(licenseKey);
            if (rt == PortSIP_Errors.ECoreTrialVersionLicenseKey)
            {
                MessageBox.Show("This sample was built base on evaluation PortSIP VoIP SDK, which allows only three minutes conversation. The conversation will be cut off automatically after three minutes, then you can't hearing anything. Feel free contact us at: sales@portsip.com to purchase the official version.");
            }
            else if (rt == PortSIP_Errors.ECoreWrongLicenseKey)
            {
                MessageBox.Show("The wrong license key was detected, please check with sales@portsip.com or support@portsip.com");
            }


            UpdateAudioCodecs();

            InitSettings();

            updatePrackSetting();

            if ( true)
            {
                rt = _sdkLib.registerServer(120, 0);
                if (rt != 0)
                {
                    _SIPInited = false;
                    _sdkLib.removeUser();
                    _sdkLib.unInitialize();
                    _sdkLib.releaseCallbackHandlers();


                    MessageBox.Show("Failed to register to server.");
                }
                MessageBox.Show("Registering...");
            }

        }
        private void updatePrackSetting()
        {
            if (!_SIPInited)
            {
                return;
            }

            if ( true)
            {
                _sdkLib.setReliableProvisional(2);
            }
            else
            {
                _sdkLib.setReliableProvisional(0);
            }
        }
        private void InitSettings()
        {
            if (_SIPInited == false)
            {
                return;
            }
        }
        private void InitDefaultAudioCodecs()
        {
            if (_SIPInited == false)
            {
                return;
            }


            _sdkLib.clearAudioCodec();

            // Default we just using PCMU, PCMA, G729
            _sdkLib.addAudioCodec(AUDIOCODEC_TYPE.AUDIOCODEC_PCMU);
            _sdkLib.addAudioCodec(AUDIOCODEC_TYPE.AUDIOCODEC_PCMA);
            _sdkLib.addAudioCodec(AUDIOCODEC_TYPE.AUDIOCODEC_G729);

            _sdkLib.addAudioCodec(AUDIOCODEC_TYPE.AUDIOCODEC_DTMF);  // for DTMF as RTP Event - RFC2833
        }


        private void UpdateAudioCodecs()
        {
            if (_SIPInited == false)
            {
                return;
            }
            _sdkLib.clearAudioCodec();
            _sdkLib.addAudioCodec(AUDIOCODEC_TYPE.AUDIOCODEC_G729);
            _sdkLib.addAudioCodec(AUDIOCODEC_TYPE.AUDIOCODEC_GSM);
            _sdkLib.addAudioCodec(AUDIOCODEC_TYPE.AUDIOCODEC_G722);
            _sdkLib.addAudioCodec(AUDIOCODEC_TYPE.AUDIOCODEC_G7221);
            _sdkLib.addAudioCodec(AUDIOCODEC_TYPE.AUDIOCODEC_DTMF);

        }


        
        private void SetSRTPType()
        {
            if (_SIPInited == false)
            {
                return;
            }

            SRTP_POLICY SRTPPolicy = SRTP_POLICY.SRTP_POLICY_NONE;

            /*switch (ComboBoxSRTP.SelectedIndex)
            {
                case 0:
                    SRTPPolicy = SRTP_POLICY.SRTP_POLICY_NONE;
                    break;

                case 1:
                    SRTPPolicy = SRTP_POLICY.SRTP_POLICY_PREFER;
                    break;

                case 2:
                    SRTPPolicy = SRTP_POLICY.SRTP_POLICY_FORCE;
                    break;
            }*/

            _sdkLib.setSrtpPolicy(SRTPPolicy, false);
        }
        public Int32 onRegisterSuccess(String statusText, Int32 statusCode, StringBuilder sipMessage)
        {
            // use the Invoke method to modify the control.

            _SIPLogined = true;
  

            return 0;
        }


        public Int32 onRegisterFailure(String statusText, Int32 statusCode, StringBuilder sipMessage)
        {


            _SIPLogined = false; 

            return 0;
        }


        public Int32 onInviteIncoming(Int32 sessionId,
                                             String callerDisplayName,
                                             String caller,
                                             String calleeDisplayName,
                                             String callee,
                                             String audioCodecNames,
                                             String videoCodecNames,
                                             Boolean existsAudio,
                                             Boolean existsVideo,
                                             StringBuilder sipMessage)
        {



            //  You should write your own code to play the wav file here for alert the incoming call(incoming tone);

            return 0;

        }

        public Int32 onInviteTrying(Int32 sessionId)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            string Text = "Line " + i.ToString();
            Text = Text + ": Call is trying...";
 
            return 0;

        }



        public Int32 onInviteSessionProgress(Int32 sessionId,
                                             String audioCodecNames,
                                             String videoCodecNames,
                                             Boolean existsEarlyMedia,
                                             Boolean existsAudio,
                                             Boolean existsVideo,
                                             StringBuilder sipMessage)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            if (existsVideo)
            {
                // If more than one codecs using, then they are separated with "#",
                // for example: "g.729#GSM#AMR", "H264#H263", you have to parse them by yourself.
            }
            if (existsAudio)
            {
                // If more than one codecs using, then they are separated with "#",
                // for example: "g.729#GSM#AMR", "H264#H263", you have to parse them by yourself.
            }

            _CallSessions[i].setSessionState(true);

            string Text = "Line " + i.ToString();
            Text = Text + ": Call session progress.";

            _CallSessions[i].setEarlyMeida(existsEarlyMedia);

            return 0;
        }




        public Int32 onInviteRinging(Int32 sessionId,
                                            String statusText,
                                            Int32 statusCode,
                                            StringBuilder sipMessage)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            if (_CallSessions[i].hasEarlyMedia() == false)
            {
                // No early media, you must play the local WAVE  file for ringing tone
            }

            string Text = "Line " + i.ToString();
            Text = Text + ": Ringing...";



            return 0;
        }




        public Int32 onInviteAnswered(Int32 sessionId,
                                             String callerDisplayName,
                                             String caller,
                                             String calleeDisplayName,
                                             String callee,
                                             String audioCodecNames,
                                             String videoCodecNames,
                                             Boolean existsAudio,
                                             Boolean existsVideo,
                                             StringBuilder sipMessage)
        {

            if (existsVideo)
            {
                // If more than one codecs using, then they are separated with "#",
                // for example: "g.729#GSM#AMR", "H264#H263", you have to parse them by yourself.
            }
            if (existsAudio)
            {
                // If more than one codecs using, then they are separated with "#",
                // for example: "g.729#GSM#AMR", "H264#H263", you have to parse them by yourself.
            }


            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }


            _CallSessions[i].setSessionState(true);

            string Text = "Line " + i.ToString();
            Text = Text + ": Call established";


            // If this is the refer call then need set it to normal
            if (_CallSessions[i].isReferCall())
            {
                _CallSessions[i].setReferCall(false, 0);
            }

            return 0;
        }


        public Int32 onInviteFailure(Int32 sessionId,
                                            String callerDisplayName,
                                            String caller,
                                            String calleeDisplayName,
                                            String callee,
                                            String reason, Int32 code, StringBuilder sipMessage)
        {
            int index = -1;
            for (int i = LINE_BASE; i < MAX_LINES; ++i)
            {
                if (_CallSessions[i].getSessionId() == sessionId)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                return 0;
            }


            string Text = "Line " + index.ToString();
            Text += ": Call failure, ";
            Text += reason;
            Text += ", ";
            Text += code.ToString();



            if (_CallSessions[index].isReferCall())
            {
                // Take off the origin call from HOLD if the refer call is failure
                int originIndex = -1;
                for (int i = LINE_BASE; i < MAX_LINES; ++i)
                {
                    // Looking for the origin call
                    if (_CallSessions[i].getSessionId() == _CallSessions[index].getOriginCallSessionId())
                    {
                        originIndex = i;
                        break;
                    }
                }

            }

            _CallSessions[index].reset();

            return 0;
        }



        public Int32 onInviteUpdated(Int32 sessionId,
                                             String audioCodecNames,
                                             String videoCodecNames,
                                             Boolean existsAudio,
                                             Boolean existsVideo,
                                             Boolean existsScreen,
                                             StringBuilder sipMessage)
        {
            return 0;


        }



        public Int32 onInviteConnected(Int32 sessionId)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            string Text = "Line " + i.ToString();
            Text = Text + ": Call is connected";


            return 0;
        }




        public Int32 onInviteBeginingForward(String forwardTo)
        {
            string Text = "Call has been forwarded to: ";
            Text = Text + forwardTo;

            return 0;
        }


        public Int32 onInviteClosed(Int32 sessionId)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }


            _CallSessions[i].reset();

            string Text = "Line " + i.ToString();
            Text = Text + ": Call closed";


            return 0;
        }

        public Int32 onDialogStateUpdated(String BLFMonitoredUri,
                                             String BLFDialogState,
                                             String BLFDialogId,
                                             String BLFDialogDirection)
        {
            string text = "The user ";
            text += BLFMonitoredUri;
            text += " dialog state is updated: ";
            text += BLFDialogState;
            text += ", dialog id: ";
            text += BLFDialogId;
            text += ", direction: ";
            text += BLFDialogDirection;



            return 0;
        }

        public Int32 onRemoteHold(Int32 sessionId)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            string Text = "Line " + i.ToString();
            Text = Text + ": Placed on hold by remote party";


            return 0;
        }


        public Int32 onRemoteUnHold(Int32 sessionId,
                                               String audioCodecNames,
                                               String videoCodecNames,
                                               Boolean existsAudio,
                                               Boolean existsVideo)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            string Text = "Line " + i.ToString();
            Text = Text + ": Take off hold by remote party";


            return 0;
        }


        public Int32 onReceivedRefer(Int32 sessionId,
                                                    Int32 referId,
                                                    String to,
                                                    String from,
                                                    StringBuilder referSipMessage)
        {
            int index = findSession(sessionId);
            if (index == -1)
            {
                _sdkLib.rejectRefer(referId);
                return 0;
            }


            string Text = "Received REFER on line ";
            Text += index.ToString();
            Text += ", refer to ";
            Text += to;

            // Accept the REFER automatically
            int referSessionId = _sdkLib.acceptRefer(referId, referSipMessage.ToString());
           

            return 0;
        }

        public Int32 onReferAccepted(Int32 sessionId)
        {
            int index = findSession(sessionId);
            if (index == -1)
            {
                return 0;
            }

            string Text = "Line ";
            Text += index.ToString();
            Text += ", the REFER was accepted";

            return 0;
        }



        public Int32 onReferRejected(Int32 sessionId, String reason, Int32 code)
        {
            int index = -1;
            for (int i = LINE_BASE; i < MAX_LINES; ++i)
            {
                if (_CallSessions[i].getSessionId() == sessionId)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                return 0;
            }

            string Text = "Line ";
            Text += index.ToString();
            Text += ", the REFER was rejected";

            return 0;
        }



        public Int32 onTransferTrying(Int32 sessionId)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            // for example, if A and B is established call, A transfer B to C, the transfer is trying,
            // B will got this transferTring event, and use referTo to know C ( C is "referTo" in this case)

            string Text = "Line " + i.ToString();
            Text = Text + ": Transfer Trying";


            return 0;
        }

        public Int32 onTransferRinging(Int32 sessionId)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            string Text = "Line " + i.ToString();
            Text = Text + ": Transfer Ringing";

            // Use hasVideo to check does this transfer call has video.
            // if hasVideo is true, then it's have video, if hasVideo is false, means has no video.


            return 0;
        }


        public Int32 onACTVTransferSuccess(Int32 sessionId)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            // Close the call after succeeded transfer the call
            _sdkLib.hangUp(_CallSessions[i].getSessionId());
            _CallSessions[i].reset();

            string Text = "Line " + i.ToString();
            Text = Text + ": Transfer succeeded, call closed.";

            return 0;
        }

        public Int32 onACTVTransferFailure(Int32 sessionId, String reason, Int32 code)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }


            string Text = "Line " + i.ToString();
            Text = Text + ": Failed to Transfer.";


            //  statusText is error reason
            //  statusCode is error code

            return 0;
        }


        public Int32 onReceivedSignaling(Int32 sessionId, StringBuilder signaling)
        {
            // This event will be fired when the SDK received a SIP message
            // you can use signaling to access the SIP message.

            return 0;
        }


        public Int32 onSendingSignaling(Int32 sessionId, StringBuilder signaling)
        {
            // This event will be fired when the SDK sent a SIP message
            // you can use signaling to access the SIP message.

            return 0;
        }




        public Int32 onWaitingVoiceMessage(String messageAccount,
                                                  Int32 urgentNewMessageCount,
                                                  Int32 urgentOldMessageCount,
                                                  Int32 newMessageCount,
                                                  Int32 oldMessageCount)
        {

            string Text = messageAccount;
            Text += " has voice message.";


            // You can use these parameters to check the voice message count

            //  urgentNewMessageCount;
            //  urgentOldMessageCount;
            //  newMessageCount;
            //  oldMessageCount;

            return 0;
        }


        public Int32 onWaitingFaxMessage(String messageAccount,
                                                  Int32 urgentNewMessageCount,
                                                  Int32 urgentOldMessageCount,
                                                  Int32 newMessageCount,
                                                  Int32 oldMessageCount)
        {
            string Text = messageAccount;
            Text += " has FAX message.";


            // You can use these parameters to check the FAX message count

            //  urgentNewMessageCount;
            //  urgentOldMessageCount;
            //  newMessageCount;
            //  oldMessageCount;

            return 0;
        }


        public Int32 onRecvDtmfTone(Int32 sessionId, Int32 tone)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            string DTMFTone = tone.ToString();
            switch (tone)
            {
                case 10:
                    DTMFTone = "*";
                    break;

                case 11:
                    DTMFTone = "#";
                    break;

                case 12:
                    DTMFTone = "A";
                    break;

                case 13:
                    DTMFTone = "B";
                    break;

                case 14:
                    DTMFTone = "C";
                    break;

                case 15:
                    DTMFTone = "D";
                    break;

                case 16:
                    DTMFTone = "FLASH";
                    break;
            }

            string Text = "Received DTMF Tone: ";
            Text += DTMFTone;
            Text += " on line ";
            Text += i.ToString();


            return 0;
        }


        public Int32 onPresenceRecvSubscribe(Int32 subscribeId,
                                             String fromDisplayName,
                                             String from,
                                             String subject)
        {


            return 0;
        }


        public Int32 onPresenceOnline(String fromDisplayName,
                                      String from,
                                      String stateText)
        {

            return 0;
        }

        public Int32 onPresenceOffline(String fromDisplayName, String from)
        {


            return 0;
        }


        public Int32 onRecvOptions(StringBuilder optionsMessage)
        {
            //         string text = "Received an OPTIONS message: ";
            //       text += optionsMessage.ToString();
            //     MessageBox.Show(text, "Received an OPTIONS message", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            return 0;
        }

        public Int32 onRecvInfo(StringBuilder infoMessage)
        {
            string text = "Received a INFO message: ";
            text += infoMessage.ToString();

            MessageBox.Show(text, "Received a INFO message");

            return 0;
        }


        public Int32 onRecvNotifyOfSubscription(Int32 subscribeId,
            StringBuilder notifyMsg,
            byte[] contentData,
            Int32 contentLenght)
        {

            return 0;
        }

        public Int32 onSubscriptionFailure(Int32 subscribeId, Int32 statusCode)
        {
            return 0;
        }

        public Int32 onSubscriptionTerminated(Int32 subscribeId)
        {
            return 0;
        }


        public Int32 onRecvMessage(Int32 sessionId,
            String mimeType,
            String subMimeType,
            byte[] messageData,
            Int32 messageDataLength)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            string text = "Received a MESSAGE message on line ";
            text += i.ToString();

            if (mimeType == "text" && subMimeType == "plain")
            {
                string mesageText = GetString(messageData);
            }
            else if (mimeType == "application" && subMimeType == "vnd.3gpp.sms")
            {
                // The messageData is binary data
            }
            else if (mimeType == "application" && subMimeType == "vnd.3gpp2.sms")
            {
                // The messageData is binary data
            }

            MessageBox.Show(text, "Received a MESSAGE message");

            return 0;
        }


        public Int32 onRecvOutOfDialogMessage(String fromDisplayName,
            String from,
            String toDisplayName,
            String to,
            String mimeType,
            String subMimeType,
            byte[] messageData,
            Int32 messageDataLength)
        {
            string text = "Received a message(out of dialog) from ";
            text += from;

            if (mimeType == "text" && subMimeType == "plain")
            {
                string mesageText = GetString(messageData);
            }
            else if (mimeType == "application" && subMimeType == "vnd.3gpp.sms")
            {
                // The messageData is binary data
            }
            else if (mimeType == "application" && subMimeType == "vnd.3gpp2.sms")
            {
                // The messageData is binary data
            }

            MessageBox.Show(text, "Received a out of dialog MESSAGE message");

            return 0;
        }

        public Int32 onSendMessageSuccess(Int32 sessionId, Int32 messageId)
        {
            return 0;
        }


        public Int32 onSendMessageFailure(Int32 sessionId,
            Int32 messageId,String reason,
            Int32 code)
        {

            return 0;
        }



        public Int32 onSendOutOfDialogMessageSuccess(Int32 messageId,
            String fromDisplayName,
            String from,
            String toDisplayName,
            String to)
        {


            return 0;
        }

        public Int32 onSendOutOfDialogMessageFailure(Int32 messageId,
            String fromDisplayName,
            String from,
            String toDisplayName,
            String to,
            String reason,
            Int32 code)
        {
            return 0;
        }




        public Int32 onPlayFileFinished(Int32 sessionId, String fileName)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            string Text = "Play file - ";
            Text += fileName;
            Text += " end on line: ";
            Text += i.ToString();


            return 0;
        }

        public Int32 onStatistics(Int32 sessionId, String stat)
        {
            int i = findSession(sessionId);
            if (i == -1)
            {
                return 0;
            }

            string Text = "Get Statistics on line: ";
            Text += i.ToString();
            Text += stat;



            return 0;
        }

        public Int32 onRTPPacketCallback(IntPtr callbackObject,
             Int32 sessionId,
             Int32 mediaType,
             Int32 direction,
             byte[] RTPPacket,
             Int32 packetSize)
        {
            /*
                !!! IMPORTANT !!!

                Don't call any PortSIP SDK API functions in here directly. If you want to call the PortSIP API functions or 
                other code which will spend long time, you should post a message to main thread(main window) or other thread,
                let the thread to call SDK API functions or other code.

            */
            return 0;
        }


        public Int32 onAudioRawCallback(IntPtr callbackObject,
                                               Int32 sessionId,
                                               Int32 callbackType,
                                               [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] data,
                                               Int32 dataLength,
                                               Int32 samplingFreqHz)
        {

            /*
                !!! IMPORTANT !!!

                Don't call any PortSIP SDK API functions in here directly. If you want to call the PortSIP API functions or 
                other code which will spend long time, you should post a message to main thread(main window) or other thread,
                let the thread to call SDK API functions or other code.

            */

            // The data parameter is audio stream as PCM format, 16bit, Mono.
            // the dataLength parameter is audio steam data length.



            //
            // IMPORTANT: the data length is stored in dataLength parameter!!!
            //

            DIRECTION_MODE type = (DIRECTION_MODE)callbackType;

            if (type == DIRECTION_MODE.DIRECTION_SEND)
            {
                // The callback data is from local record device of each session, use the sessionId to identifying the session.
            }
            else if (type == DIRECTION_MODE.DIRECTION_RECV)
            {
                // The callback data is received from remote side of each session, use the sessionId to identifying the session.
            }




            return 0;
        }


        public Int32 onVideoRawCallback(IntPtr callbackObject,
                                               Int32 sessionId,
                                               Int32 callbackType,
                                               Int32 width,
                                               Int32 height,
                                               [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 6)] byte[] data,
                                               Int32 dataLength)
        {
            /*
                !!! IMPORTANT !!!

                Don't call any PortSIP SDK API functions in here directly. If you want to call the PortSIP API functions or 
                other code which will spend long time, you should post a message to main thread(main window) or other thread,
                let the thread to call SDK API functions or other code.

                The video data format is YUV420, YV12.
            */

            //
            // IMPORTANT: the data length is stored in dataLength parameter!!!
            //

            DIRECTION_MODE type = (DIRECTION_MODE)callbackType;

            if (type == DIRECTION_MODE.DIRECTION_SEND)
            {

            }
            else if (type == DIRECTION_MODE.DIRECTION_RECV)
            {

            }


            return 0;

        }

        public Int32 onVideoDecoderCallback(IntPtr callbackObject,
                                        Int32 sessionId,
                                       Int32 width,
                                       Int32 height,
                                       Int32 framerate,
                                       Int32 bitrate)
        {
            /*
                !!! IMPORTANT !!!

                Don't call any PortSIP SDK API functions in here directly. If you want to call the PortSIP API functions or 
                other code which will spend long time, you should post a message to main thread(main window) or other thread,
                let the thread to call SDK API functions or other code.
            */
            return 0;
        }

        public Int32 onScreenRawCallback(IntPtr callbackObject,
                                              Int32 sessionId,
                                              Int32 callbackType,
                                              Int32 width,
                                              Int32 height,
                                              [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 6)] byte[] data,
                                              Int32 dataLength)
        {

            /*
                !!! IMPORTANT !!!

                Don't call any PortSIP SDK API functions in here directly. If you want to call the PortSIP API functions or 
                other code which will spend long time, you should post a message to main thread(main window) or other thread,
                let the thread to call SDK API functions or other code.

            */

            // The data parameter is audio stream as PCM format, 16bit, Mono.
            // the dataLength parameter is audio steam data length.



            //
            // IMPORTANT: the data length is stored in dataLength parameter!!!
            //
            return 0;
        }



    }
}