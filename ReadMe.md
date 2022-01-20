## Obermind Purchase Orders API

This web api created with `ASP.NET` is to allow a user to create and manage their purchase orders with functionalities such as:
* Create a purchase order
* View all purchase orders created by a user
* View details about a purchase order
* Update a purchase order
* Sign In
* Sign Up

### Business Requirements
These are the business requirement for this web app:
* Purchase Order (PO) has time of creation, some auto-generated name and status (`DRAFT`, `SUBMITTED`)
* PO owns a list of Line Items (LI)
* PO should have at least 1 LI
* Max number of LIs per PO should be limited by 10
* Changing the list of LIs is allowed only when PO in DRAFT status
* LI has name (identifies the item) and amount (price without currency for simplicity)
* Total amount of PO (sum of its LIs) cannot exceed 10000
* Max number of submitted POs for user should be limited in time, no more than 10 per day.
* Create a purchase order in `DRAFT`
* Edit selected Purchase Order's LI only when in `Draft`
* 