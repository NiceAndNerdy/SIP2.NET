SIP2.NET
========

9/29/2017 - Update - Added console debug mode option.  Fixed empty string error in checkout function.

3/29/2017 - Update - Added checksum capabilities, resoved Sirsi SSH connection issues, and cleaned up some of the implementations.  This version has been tested on Sirsi, III, Koha, and TLC SIP2 servers. 

5/23/2014 - Update - Testing against Koha has begun.  So far, it looks like the connection testers and patron parser works right out of the box.  Other methods also appear to work.  

Implementation in C# of the SIP2 client version 2.0 for libraries.  Allows users to programmatically checkout, renew, and checkin materials regardless of database architecture.  Requires a running and configured SIP2 server. 

~~Important to note for this version! - This (unsigned, ps) DLL was only tested against The Library Corporation's (TLC's) and Koha's implementation of SIP2!  It is unknown whether this will work with other ILS products.  It should.  But we all know where assumptions get us.  I would also strongly encourage anyone using another ILS to take a crack at trying this code against their SIP server.  Please report back your sucesses and your brickwalls.

Also important to note - the hold and renewal behavior has some known bugs.  At the time of this writing, they include:


1) If a patron has the same barcode on hold more than once, TLC's SIP server implementation throws a precision exception.


2) Renewals are mad funky.  RenewAll seems to work and will read the ILS's renewal rules (i.e. how long a renewed item is good for).  Individual renewal (i.e. one item at a time) don't seem to have that intelligence.  Due dates have to be      submitted along with the initial SIP2 renewal command.  That is not supported [yet] by this implementation.  It's also up to the developer to find someway to establish those renewal rules themselves.  SIP doesn't seem to be capable of pulling them from the ILS.
   
   





