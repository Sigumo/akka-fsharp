namespace Unit1

open System
open Akka.FSharp
open Akka.FSharp.Spawn
open Akka.Actor
open Unit1.Actors

module Main = 
    let printInstructions () =
        Console.WriteLine "Write whatever you want into the console!"
        Console.Write "Some lines will appear as"
        Console.ForegroundColor <- ConsoleColor.Red
        Console.Write " red"
        Console.ResetColor ()
        Console.Write " and others will appear as"
        Console.ForegroundColor <- ConsoleColor.Green
        Console.Write " green! "
        Console.ResetColor ()
        Console.WriteLine ()
        Console.WriteLine ()
        Console.WriteLine "Type 'exit' to quit this application at any time.\n"
    
    [<EntryPoint>]
    let main argv = 
        // initialize an actor system
        // YOU NEED TO FILL IN HERE
        let myActorSystem = System.create "MyActorSystem" (Configuration.load ())
        printInstructions ()
        
        // make your first actors using the 'spawn' function
        // YOU NEED TO FILL IN HERE
        let ConsoleWriterActor = spawn myActorSystem "consoleWriterActor" (actorOf consoleWriterActor)
        let ConsoleReaderActor = spawn myActorSystem "consoleReaderActor" (actorOf2 (consoleReaderActor ConsoleWriterActor))
        // tell the consoleReader actor to begin
        // YOU NEED TO FILL IN HERE
        ConsoleReaderActor <! Start
        myActorSystem.WhenTerminated.Wait() 
        0