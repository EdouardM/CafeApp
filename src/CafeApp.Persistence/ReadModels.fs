module ReadModel
open System
open CafeApp.Domain

(*
To understand the query side better, let's have a look at the target users of the CafeApp and determines what information that they need.
---------------
Waiter 
---------------
What is the status of each table? 
What are the drinks and the food items that they need to serve for a given table?
---------------
Chef
--------------
What are all food items that they need to prepare?

--------------
Cashier
--------------
How much amount to be paid for a table? 
*)

type TableStatus =
| Open of Guid
| InService of Guid
| Closed

type Table = {
  Number : int
  Waiter : string
  Status : TableStatus
}

type ChefToDo = {
  Tab : Tab
  Foods : Food list
}

type WaiterToDo = {
  Tab : Tab
  Foods : Food list
  Drinks : Drink list
}

type CashierToDo = {
  Tab : Tab
  Payment : Payment
}        