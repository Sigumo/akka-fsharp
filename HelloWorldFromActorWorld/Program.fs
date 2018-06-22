//This shit isn't working
//ToDo Сделать так шобы это говно работало

open Akka.Actor
open Akka.FSharp
open Akka.FSharp.Actors

type GreetMessage = GreetMessage of string

type GreetActor() = 
    inherit UntypedActor()
        override this.OnReceive(msg:obj) = 
            printfn "Received message: %A" msg

let system = System.create "system" <| Configuration.defaultConfig()

let echoActorRef = spawn system "echo" 
                       (actorOf (fun m -> printfn "received %A" m))
                        
// tell a message to actor ref
echoActorRef <! "Hello World!"

system.WhenTerminated.Wait()