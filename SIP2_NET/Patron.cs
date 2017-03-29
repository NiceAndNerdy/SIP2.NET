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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SIP2
{
    public class Patron
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public decimal Fines { get; set; }
        public decimal FineLimit { get; set; }
        public string Message { get; set; }
        public int HoldItemLimit { get; set; }
        public string Pin { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool Authorized { get; set; }

        public void PatronParse(string PatronInformationResponse)
        {
            this.Authorized = true;
            string[] patron_data = PatronInformationResponse.Split(new Char[] { '|' });
            
            //  Check of 14 character fixed field for potential block information.
            if ((patron_data[0].ToUpper().Contains("Y"))) this.Authorized = false;
            
            //  Loop through the rest of the fields.
            foreach (var element in patron_data)
            {

                if (element.Length >= 2)
                {
                    //  Name
                    if (element.Substring(0, 2).ToUpper() == "AE") this.Name = element.Substring(2);

                    //  Pin number    
                    if (element.Substring(0, 2).ToUpper() == "CQ") this.Pin = element.Substring(2);

                    //  Fines
                    if (element.Substring(0, 2).ToUpper() == "BV") this.Fines = Convert.ToDecimal(element.Substring(2));

                    //  Max fines
                    if (element.Substring(0, 2).ToUpper() == "CC") this.FineLimit = Convert.ToDecimal(element.Substring(2));

                    //  Address
                    if (element.Substring(0, 2).ToUpper() == "BD") this.Address = element.Substring(2);

                    //  Email
                    if (element.Substring(0, 2).ToUpper() == "BE") this.Email = element.Substring(2);

                    //  Phone
                    if (element.Substring(0, 2).ToUpper() == "BF") this.Phone = element.Substring(2);

                    //  System Message
                    if (element.Substring(0, 2).ToUpper() == "AF") this.Message = element.Substring(2);

                    //  Patron type
                    if (element.Substring(0, 2).ToUpper() == "PT") this.Type = element.Substring(2);

                    //  Hold Item Limit
                    if (element.Substring(0, 2).ToUpper() == "BZ") this.HoldItemLimit = Convert.ToInt32(element.Substring(2));

                    //  Check for valid patron status.
                    if (element.Substring(0, 2).ToUpper() == "BL")
                    {
                        if (element.Substring(2, 1).ToUpper().Trim().Equals("N")) this.Authorized = false;
                    }
                }
            }
        }
    }
}
