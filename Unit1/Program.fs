namespace Unit1

open System
open Akka.FSharp
open Akka.FSharp.Spawn
open Akka.Actor
open Unit1.Actors

module Main = 
    open System.Threading

    [<EntryPoint>]
    let main argv = 
        // initialize an actor system
        let myActorSystem = System.create "MyActorSystem" (Configuration.load ())
        
        // make actors using the 'spawn' function
        let ConsoleWriterActor = spawn myActorSystem "consoleWriterActor" (actorOf consoleWriterActor)
        let ConsoleReaderActor = spawn myActorSystem "consoleReaderActor" (actorOf2 (consoleReaderActor ConsoleWriterActor))
        // tell the consoleReader actor to begin
        ConsoleReaderActor <! Start
        
        myActorSystem.WhenTerminated.Wait ()
        0