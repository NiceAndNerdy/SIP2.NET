/**************************************************************
 * 
 *  (c) 2014 Mark Lesniak - Nice and Nerdy LLC
 *  
 * 
 *  Implementation of the Standard Interchange Protocol version 
 *  2.0.  Used to standardize queries across multiple database
 *  architectures.  
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 *  
 * 
**************************************************************/


using System;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;


namespace SIP2
{
    public class SipConnection : IDisposable
    {
        /*********************************************
         * Instance Variables
         *********************************************/
        
        private TcpClient sipSocket = new TcpClient();
        private NetworkStream sipStream = null;
        private SipServerParameters sip = new SipServerParameters();
        private bool connected = false;
        private bool authorized = false;


        /*********************************************
         * Constructors
         *********************************************/

        //  Main Constructor
        public SipConnection() { }



        /// <summary>
        ///     SIP Constructor taking the connection information as string parameters.
        /// </summary>
        /// <param name="ip">SIP server IP</param>
        /// <param name="port">SIP server port</param>
        /// <param name="username">SIP server username</param>
        /// <param name="password">SIP server password</param>
        /// <param name="extra_number">SIP server extra number (optional in some implementations)</param>
        public SipConnection(string ip, string port, string username, string password, string extra_number = "")
        {
            this.sip.ip = ip;
            this.sip.port = port;
            this.sip.username = username;
            this.sip.password = password;
            this.sip.extra_number = extra_number;
        }

        

        /// <summary>
        ///     SIP constructor taking an instance of the SipServerParameter class as it's parameter
        /// </summary>
        /// <param name="sipParameters">Instance of the SipServerParameter class</param>
        /// <remarks>
        ///     Use this constructor if you will be regularly accessing and/or changing your SIP server parameters in your application.
        /// </remarks>
        public SipConnection(SipServerParameters sipParameters)
        {
            this.sip.ip = sipParameters.ip;
            this.sip.port = sipParameters.port;
            this.sip.username = sipParameters.username;
            this.sip.password = sipParameters.password;
            this.sip.extra_number = sipParameters.extra_number;
        }
       

 
        /*********************************************
         * Public Methods
         *********************************************/
        
        /// <summary>
        ///     Starts SIP connection assuming that SIP parameters were defined in the contructor.
        /// </summary>
        public void Open()
        {
            
            //  Set up socket.
            sipSocket.SendTimeout = 500;
            try
            {
                sipSocket.Connect(sip.ip, Convert.ToInt32(sip.port));
                sipStream = sipSocket.GetStream();
            }
            catch (Exception ex)
            {
                throw new NotConnectedException(ex.Message);
            }

            //  Set up SIP start command.
            string sipCommand = string.Format("9300CN{0}|CO{1}|CP{2}", sip.username, sip.password, sip.extra_number);
            
            //  Communicate with server.
            if (SipFactory(sipCommand).Contains("941"))
            {
                if (HandShake() != "0") connected = true;
                else
                {
                    connected = false;
                    throw new HandshakeFailedException("Unable to establish SIP rules.  Handshake with server failed!");
                }
            }
            else
            {
                connected = false;
                throw new ConnectionFailedException("Unable to connect to the server.  Are your SIP parameters correct?  Is the server available?");
            }
        }



        /// <summary>
        ///     Starts connection with SIP server.  Parameters are required.  Use this if you do not establish server parameteres in the constructor.
        /// </summary>
        /// <param name="ip">SIP server IP</param>
        /// <param name="port">SIP server port</param>
        /// <param name="username">SIP server username</param>
        /// <param name="password">SIP server password</param>
        /// <param name="extra_number">SIP server extra number (optional in some implementations)</param>
        public void Open(string ip, string port, string username, string password, string extra_number)
        {
            //  Set up instance variables.   
            this.sip.ip = ip;
            this.sip.port = port;
            this.sip.username = username;
            this.sip.password = password;
            this.sip.extra_number = extra_number;
            Open();
        }



        /// <summary>
        ///     Test whether an instance of a constructed SIP class is able to communicate with the server.
        /// </summary>
        /// <returns>Returns true if connection is viable.</returns>
        public bool TestConnection()
        {
            Open();
            return connected;
        }
        


        /// <summary>
        ///     Test whether an instance of an unconstructed SIP class is able to communicate with the server.  Use this for on-the-fly tests of SIP parameters.
        /// </summary>
        /// <param name="ip">SIP server IP</param>
        /// <param name="port">SIP server port</param>
        /// <param name="username">SIP server username</param>
        /// <param name="password">SIP server password</param>
        /// <param name="extra_number">SIP server extra number (optional in some implementations)</param>
        /// <returns>Returns true if connection is viable.</returns>
        public bool TestConnection(string ip, string port, string username, string password, string extra_number)
        {
            this.sip.ip = ip; 
            this.sip.port = port;
            this.sip.username = username;
            this.sip.password = password;
            this.sip.extra_number = extra_number;
            Open();
            return connected;
        }



        /// <summary>
        ///     Test whether an instance of an unconstructed SIP class is able to communicate with the server.  Use this for on-the-fly tests of SIP parameters.
        /// </summary>
        /// <param name="sipParameters">Instance of the SipServerParameter class</param>
        /// <returns>Returns true if connection is viable.</returns>
        public bool TestConnection(SipServerParameters sipParameters)
        {
            this.sip.ip = sipParameters.ip;
            this.sip.port = sipParameters.port;
            this.sip.username = sipParameters.username;
            this.sip.password = sipParameters.password;
            this.sip.extra_number = sipParameters.extra_number;
            Open();
            return connected;
        }



        /// <summary>
        ///     Authorizes patron barcode against the SIP server.  This is should be used for especially for public facing implementations of SIP2.
        /// </summary>
        /// <param name="barcode">Barcode of the patron.</param>
        /// <param name="patron">Instance of the patron class.  This will be popluated with a human-readable break down of the Patron Response Message from the SIP server.</param>
        /// <param name="failureResponse">Reason, if any, that a card failed to authorize.</param>
        /// <returns>Return true if authorization is successful.  Returns false if bardcode failed to authorize.</returns>
        public bool AuthorizeBarcode(string barcode, ref Patron patron, ref string failureResponse)
        {
            patron = new Patron();
            string response = String.Empty;
            if (connected)
            {
                string date = GetDateString();
                string sipCommand = string.Format("63001{0}          AO1|AA{1}|AC{2}|AD|BP|BQ|", date, barcode, sip.password);
                response = SipFactory(sipCommand);
                patron.PatronParse(response);
                if ((patron.Authorized) & (patron.Fines < patron.FineLimit))
                {
                    failureResponse = "";
                    authorized = true;
                    return authorized;
                }
                else
                {
                    failureResponse = "There seems to be a problem with your card.  Please see a librarian!";
                    authorized = false;
                    return authorized;
                }
            }
            else
            {
                throw new NotConnectedException("Server is currently unavailable.  Please see a librarian!");
            }
        }



        /// <summary>
        ///     Authorizes patron barcode against the SIP server.
        /// </summary>
        /// <param name="barcode">Barcode of the patron.</param>
        /// <returns>Return true if authorization is successful.  Returns false if bardcode failed to authorize.</returns>
        public bool AuthorizeBarcode(string barcode)
        {
            Patron patron = new Patron();
            string PatronInformationResponse = String.Empty;
            if (connected)
            {
                string date = GetDateString();
                string sipCommand = string.Format("63001{0}          AO1|AA{1}|AC{2}|AD|BP|BQ|", date, barcode, sip.password);
                PatronInformationResponse = SipFactory(sipCommand);
                patron.PatronParse(PatronInformationResponse);
                if (patron.Authorized) 
                {
                    //  All good.
                    authorized = true;
                    return authorized;
                }
                else
                {
                    //  Blocked or excessive fines.  Also could be an invalid barcode.
                    authorized = false;
                    return authorized;
                }
            }
            else
            {
                throw new NotConnectedException("SIP server connection is not established.  Failed to authorize barcode.");
            }
            
        }


        /// <summary>
        ///     Method to checkout items via SIP2 protocol.  Returns list of Items.  Returns null if patron is not authorized.  
        /// </summary>
        /// <param name="patronBarcode"></param>
        /// <param name="itemBarcodes"></param>
        /// <returns>Returns list of Items.  Returns null if patron is not authorized.</returns>
        public List<Item> CheckOut(string patronBarcode, IEnumerable<string> itemBarcodes)
        {
            if (!connected) throw new NotConnectedException("Cannot checkout books.  SIP connection is not established!");
            if (authorized)
            {
                List<Item> itemListOut = new List<Item>();
                string date = GetDateString();
                foreach (string item in itemBarcodes)
                {
                    Item CheckedOutItem = new Item();
                    string sipCommand = string.Format("11YN{0}                  AO1|AA{1}|AB{2}|AC{3}", date, patronBarcode, item, sip.password);
                    string sipResponse = (SipFactory(sipCommand));
                    CheckedOutItem.ItemParse(sipResponse);
                    itemListOut.Add(CheckedOutItem);
                }
                return itemListOut;
            }
            else
            {
                List<Item> itemListOut = null;
                return itemListOut;
            }
        }


        /// <summary>
        ///     Method for checking in materials.  Returns a list of parsed SIP item responses.  Returns null if transaction fails or no barcodes entered.
        /// </summary>
        /// <param name="itemBarcodes">Barcode numbers of the items to be checked in.</param>
        /// <returns>Returns a list of parsed SIP item responses.  Returns null if transaction fails.</returns>
        public List<Item> CheckIn(IEnumerable<string> itemBarcodes)
        {
            if (!connected) throw new NotConnectedException("Cannot checkin books.  SIP connection is not established!");
            List<Item> itemListOut = null;
            string date = GetDateString();
            foreach (string item in itemBarcodes)
            {
                Item CheckedInItem = new Item();
                string sipCommand = string.Format("09Y{0}{1}AP0|AO1|AB{2}|AC{3}", date, date, item, sip.password);
                string sipResponse = (SipFactory(sipCommand));
                CheckedInItem.ItemParse(sipResponse);
                itemListOut.Add(CheckedInItem);
            }
            return itemListOut;
        }



        /// <summary>
        ///     Method to appy or remove item holds.  Holds removal does not work if the patron has the same barcode on hold more than once.  
        /// </summary>
        /// <param name="patronBarcode">Patron barcode wishing to place the hold.</param>
        /// <param name="itemBarcode">Barcode of the item they wish to place on hold.</param>
        /// <param name="action">Integer value 1 for placing hold.  Integer value -1 for removing hold.</param>
        /// <returns>Return instance of the Item class.  Returns null if barcode is not authorized first.</returns>
        public Item ItemHold(string patronBarcode, string itemBarcode, int action)
        {
            if (!connected) throw new NotConnectedException("Cannot checkin books.  SIP connection is not established!");
            if (authorized)
            {
            string holdAction = String.Empty;
            switch (action)
            {
                case  1:  
                    holdAction = "+";
                    break;
                case -1: 
                    holdAction = "-";
                    break;
                default: 
                    throw new InvalidParameterException("Unrecognized hold parameter!  Use -1 to delete hold, 1 to add hold!");
            }
            Item itemResponse = new Item();
            string date = GetDateString();
            string sipCommand = string.Format("15{0}{1}|AO1|AA{2}|AB{3}|AC{4}", holdAction, date, patronBarcode, itemBarcode, sip.password);
            string sipResponse = (SipFactory(sipCommand));
            itemResponse.ItemParse(sipResponse);
            return itemResponse;
            }
            else return null;
        }


        
        /// <summary>
        ///     Method to renew a list of books.  DOES NOT WORK EFFECTIVELY!  Due dates cannot be calculated as SIP does not have access to these rules directly.  No idea why this is implemented this way.  Help is appreciated!  Use RenewAll for now.
        /// </summary>
        /// <param name="patronBarcode">Patron barcode</param>
        /// <param name="itemBarcodes">IEnumberable of string - the item(s)' barcode(s) to renew.</param>
        /// <returns>Returns list of the Items class.  Returns null if patron is not authorized.</returns>
        public List<Item> Renew(string patronBarcode, IEnumerable<string> itemBarcodes)
        {
            if (!connected) throw new NotConnectedException("Cannot checkout books.  SIP connection is not established!");
            if (authorized)
            {
                List<Item> itemListOut = new List<Item>();
                string date = GetDateString();
                foreach (string item in itemBarcodes)
                {
                    Item RenewedItem = new Item();
                    string sipCommand = string.Format("29YY{0}{1}AO1|AA{2}|AB{3}|AC{4}", date, date, patronBarcode, item, sip.password);
                    string sipResponse = (SipFactory(sipCommand));
                    RenewedItem.ItemParse(sipResponse);
                    itemListOut.Add(RenewedItem);
                }
                return itemListOut;
            }
            else
            {
                return null;
            }
        }
      

        /// <summary>
        ///     Renews all items a patron currently has checked out.  Doesn't seem to care if the renewal limit has been reached.  
        /// </summary>
        /// <param name="patronBarcode">Patron barcode</param>
        /// <returns>Returns true if command was successful.  Returns false if command fails or renewal was unsuccessful.</returns>
        public bool RenewAll(string patronBarcode)
        {
            string date = GetDateString();
            string sipCommand = string.Format("65{0}AO1|AA{1}|AC{2}", date, patronBarcode, sip.password);
            string sipResponse = SipFactory(sipCommand);
            if (sipResponse.Substring(2,1) == "1") return true;
            else return false;
        }


        /// <summary>
        ///     Ends patron session.
        /// </summary>
        /// <param name="patronBarcode">Patron barcode</param>
        /// <returns>Returns true if session was sucessfully ended - false if it wasn't.</returns>
        public bool EndPatronSession(string patronBarcode) 
        {
            string date = GetDateString();
            string sipCommand = string.Format("35{0}AO1|AA{1}|AC{2}", date, patronBarcode, sip.password);
            string sipResponse = SipFactory(sipCommand);
            if (sipResponse.Substring(2, 1) == "Y") return true;
            else return false;
        }
        
        
        /// <summary>
        ///     Closes SIP connection.
        /// </summary>
        public void Close()
        {
            if ((sipStream != null) & (sipSocket != null))
            {
                sipStream.Close();
                sipSocket.Close();
                sipStream = null;
                sipSocket = null;
            }
            else throw new NotConnectedException("Cannot close connection.  Connection was not established!");
        }



        /// <summary>
        ///     Disposes SIP connection.
        /// </summary>
        public void Dispose()
        {
            Close();
        }



        /*********************************************
         * Private Methods
         *********************************************/
        
        private string HandShake()
        {
            try
            {
                string rulesCommand = "9900302.0E|";
                return SipFactory(rulesCommand);
            }
            catch (Exception)
            {
                return "0";  //  Highly unlikely that this will fail.  Hence try...catch logic vs. if...then logic.
            }
        }



        private string GetDateString() 
        {
            string year = DateTime.Now.Year.ToString("00");
            string month = DateTime.Now.Month.ToString("00");
            string day = DateTime.Now.Day.ToString("00");
            string ZZZZ = "    ";
            string hour = DateTime.Now.Hour.ToString("00");
            string minute = DateTime.Now.Minute.ToString("00");
            string second = DateTime.Now.Second.ToString("00");
            return year + month + day + ZZZZ + hour + minute + second;
        }

        
    
        private string SipFactory(string sipCommand)
        {
            try
            {
                //  Set up and submit outstream data.
                byte[] outStream = Encoding.ASCII.GetBytes(sipCommand + "\r");
                sipStream.Write(outStream, 0, outStream.Length);
                sipStream.Flush();

                //  Read SIP socket response.
                byte[] inStream = new byte[100025];  //  I have no idea why you need this many bytes.
                sipStream.Read(inStream, 0, (int)sipSocket.ReceiveBufferSize);
                string sipResponse = Encoding.ASCII.GetString(inStream).Trim();
                return sipResponse;
            }
            catch (Exception ex)
            {
                return ex.ToString();  //  For debugging purposes or if the socket fails for some reason.  
            }
        
        }
    }
}
