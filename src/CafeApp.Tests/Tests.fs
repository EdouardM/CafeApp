module Tests

open Expecto

open CafeApp.Core

[<Tests>]
let tests =
  testList "samples" [
    testCase "reference CafeApp" <| fun _ ->
      let cafe = new CafeApp()
      Expect.equal cafe.X "F#" "We should access CafeApp.Core type."
  ]