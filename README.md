SIP2.NET
========

Implementation in C# of the SIP2 client version 2.0 for libraries.  Allows users to programmatically checkout, renew, and checkin materials regardless of database architecture.  Requires a running and configured SIP2 server. 


Important to note for version 0.1!!!! - This was only tested against The Library Corporation's (TLC's) implementation of SIP2!!!  It is unknown whether this will work with other ILS products!  It should.  But we all know where assumptions get us.

Also important to note - the hold and renewal behavior has some known bugs.  At the time of this writing, they include:

1) If a patron has the same barcode on hold more than once, TLC's SIP server implementation throws a precision error.  
2) Renewals are mad funky.  RenewAll seems to work and will read the ILS's renewal rules (i.e. how long a renewed item is
   good for.  Individual renewal (i.e. one item at a time) don't seem to have that intelligence and have to be submitted     along with the initial SIP renewal command.  That is not supported [yet] by this implementation.  It's also up to the     developer to find someway to establish those renewal rules themselves.  SIP doesn't seem to be able to do it. 
