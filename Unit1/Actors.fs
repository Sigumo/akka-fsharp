namespace Unit1

open System
open Akka.Actor
open Akka.FSharp
open Unit1.Messages

module Actors =

    let (|EmptyMessage|MessageLengthIsEven|MessageLengthIsOdd|) (msg: string) = 
        match msg.Length, msg.Length % 2  with 
        | 0, _ -> EmptyMessage 
        | _, 0 -> MessageLengthIsEven
        | _, _ -> MessageLengthIsOdd
        
    let (|Message|Exit|) (str:string) =
        match str.ToLower() with
        | "exit" -> Exit
        | _ -> Message(str)

    let doPrintInstructions () =
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
        
    let consoleReaderActor (consoleWriter: IActorRef) (mailbox: Actor<_>) message = 
    
        let getAndValidateInput () = 
                let line = Console.ReadLine ()
                match line with 
                | Exit -> mailbox.Context.System.Terminate () |> ignore
                | Message(input) -> 
                    match input with 
                    | EmptyMessage -> 
                        mailbox.Self <! InputError ("No input received", ErrorType.Null)
                    | MessageLengthIsEven -> 
                        consoleWriter <! InputSuccess("Thank you, the message was valid")
                        mailbox.Self <! Continue
                    | _ -> 
                        mailbox.Self <! InputError("The message is invalid (odd number of characters", ErrorType.Validation)
        
        match box message with 
        | :? Command as command ->
            match command with 
            | Start -> doPrintInstructions ()
            | _ -> ()
        | :? InputResult as inputResult ->
            match inputResult with 
            | InputError(_,_) as error -> consoleWriter <! error
            | _ -> ()
        | _ -> ()
        
        getAndValidateInput ()

    let consoleWriterActor message = 
    
        let (|Even|Odd|) n = if n % 2 = 0 then Even else Odd
    
        let printInColor color message =
            Console.ForegroundColor <- color
            Console.WriteLine (message.ToString ())
            Console.ResetColor ()
            
        match box message with 
        | :? InputResult as inputResult -> 
            match inputResult with 
            | InputSuccess reason -> printInColor ConsoleColor.Green reason
            | InputError (reason, _) -> printInColor ConsoleColor.Red reason
        | _ -> printInColor ConsoleColor.Yellow  (message.ToString ())