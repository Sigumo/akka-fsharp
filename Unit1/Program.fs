namespace Unit1

open Akka.Actor
open Akka.FSharp
open Akka.FSharp.Spawn
open System
open Unit1.Actors

module Main =
    open System.IO
    open System.Threading
    
    [<EntryPoint>]
    let main argv =
        // initialize an actor system
        let myActorSystem = System.create "MyActorSystem" (Configuration.load())
        
        let strategy() =
            Strategy.OneForOne((fun ex -> 
                               match ex with
                               | :? ArithmeticException -> Directive.Resume
                               | :? NotSupportedException -> Directive.Stop
                               | _ -> Directive.Restart), 10, TimeSpan.FromSeconds(30.))
        
        // make actors using the 'spawn' function
        let ConsoleWriter = spawn myActorSystem "consoleWriter" (actorOf consoleWriterActor)
        let TailCoordinator =
            spawnOpt myActorSystem "tailCoordinator" (actorOf2 tailCoordinatorActor) 
                [ SpawnOption.SupervisorStrategy(strategy()) ]
        let FileValidator = spawn myActorSystem "fileValidator" (actorOf2 (fileValidator ConsoleWriter))
        let ConsoleReader = spawn myActorSystem "consoleReader" (actorOf2 (consoleReaderActor))
        // tell the consoleReader actor to begin
        ConsoleReader <! Start
        myActorSystem.WhenTerminated.Wait()
        0
