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
    public class Item
    {
        public string DueDate { get; set; }
        public string Title { get; set; }
        public string ItemBarcode { get; set; }
        public string PatronId { get; set; }
        public string InstitutionId { get; set; }
        public string Message { get; set; }
        public bool SuccessfulTransaction { get; set; }
        public bool SuccessfulRenewal { get; set; }
        public bool MagneticMedia { get; set; }
        public bool Desensitize { get; set; }

        
        public void ItemParse(string ItemResponse)
        {
            string[] item_data = ItemResponse.Split(new Char[] { '|' });
            
            if (item_data[0].Substring(2, 1) == "1") this.SuccessfulTransaction = true;
            else this.SuccessfulTransaction = false;

            if (item_data[0].Substring(3, 1) == "Y") this.SuccessfulRenewal = true;
            else this.SuccessfulRenewal = false;

            if (item_data[0].Substring(4, 1) == "Y") this.MagneticMedia = true;
            else this.MagneticMedia = false;

            if (item_data[0].Substring(5, 1) == "Y") this.Desensitize = true;
            else this.Desensitize = false;

            foreach (string element in item_data)
            {
                // Due Date               
                if (element.Substring(0, 2).ToUpper() == "AH") this.DueDate = element.Substring(2);
                               
                // Item title
                if (element.Substring(0, 2).ToUpper() == "AJ") this.Title = element.Substring(2);
                
                // Item barcode
                if (element.Substring(0, 2).ToUpper() == "AB") this.ItemBarcode = element.Substring(2);
                
                // Patron id
                if (element.Substring(0, 2).ToUpper() == "AA") this.PatronId = element.Substring(2);
                
                // Institution id
                if (element.Substring(0, 2).ToUpper() == "AO") this.InstitutionId = element.Substring(2);
                
                // Screen message
                if (element.Substring(0, 2).ToUpper() == "AF") this.Message = element.Substring(2);
            }
        }
    }
}
