namespace CafeApp 

    //Domain explained in the tutorial foud here: 
    //http://cqrs.nu/tutorial/cs/01-design

    (*
The Domain 
For this tutorial, we'll work in the cafe domain. Our focus will be on the concept of a tab, which tracks the visit of an individual or group to the cafe. When people arrive to the cafe and take a table, a tab is opened. They may then order drinks and food. Drinks are served immediately by the table staff, however food must be cooked by a chef. Once the chef has prepared the food, it can then be served.
During their time at the restaurant, visitors may order extra food or drinks. If they realize they ordered the wrong thing, they may amend the order - but not after the food and drink has been served to and accepted by them.
Finally, the visitors close the tab by paying what is owed, possibly with a tip for the serving staff. Upon closing a tab, it must be paid for in full. A tab with unserved items cannot be closed unless the items are either marked as served or cancelled first.
    *)

    module Domain =
        open System

(*
Aggregates

Each aggregate has its own stream of events. Taken together, they can be used to compute its current state. 
Aggregates are completely isolated from each other.
Decisions about whether to accept a command are made solely on the basis of the command itself and the information contained in the aggregate's past events.

Concretely, an aggregate is either:
    A single object, which doesn't reference any others.
    An isolated graph of objects, with one object designated as the root of the aggregate. The outside world should only know about the root.
*)
        // Aggregate
        type Tab = {
            Id : Guid
            TableNumber : int
        }

        type Item = {
            MenuNumber  : int
            Price       : decimal
            Name        : string
        }

        type Food = Food    of Item
        type Drink = Drink  of Item

        type Payment = {
            Tab     : Tab
            Amount  : decimal
        }

        type Order = {
            Foods   : Food list
            Drinks  : Drink list    
            Tab     : Tab
        }
        
(*
    Let's revisit our domain logic before we get started with implementing this transition. 
    If an order contains Drink, Waiter will serve it immediately. 
    But, if it includes food, Chef should prepare the food first and then the Waiter serve the food
*)
        type InProgressOrder = {
            PlacedOrder     : Order
            ServedDrinks    : Drink list
            ServedFoods     : Food list
            PreparedFoods   : Food list
        }

