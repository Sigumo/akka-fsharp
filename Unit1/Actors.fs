namespace Unit1

open Akka.Actor
open Akka.FSharp
open System
open System.IO
open Unit1.FileUtility
open Unit1.Messages

module Actors =

    let consoleReaderActor (mailbox : Actor<_>) message =
        let doPrintInstructions() = Console.WriteLine "Please provide the URI of a log file on disk.\n"
        
        let (|Message|Exit|) (str : string) =
            match str.ToLower() with
            | "exit" -> Exit
            | _ -> Message(str)
        
        let validationActor = select "/user/fileValidator" mailbox.Context.System
        
        let getAndValidateInput() =
            let line = Console.ReadLine()
            match line with
            | Exit -> 
                Console.ForegroundColor <- ConsoleColor.Red
                Console.WriteLine "Terminating..."
                mailbox.Context.System.Terminate() |> ignore
            | _ -> validationActor <! line
        
        match box message with
        | :? Command as command -> 
            match command with
            | Start -> doPrintInstructions()
            | _ -> ()
        | _ -> ()
        getAndValidateInput()
    
    let consoleWriterActor message =
        let (|Even|Odd|) n =
            if n % 2 = 0 then Even
            else Odd
        
        let printInColor color message =
            Console.ForegroundColor <- color
            Console.WriteLine(message.ToString())
            Console.ResetColor()
        
        match box message with
        | :? InputResult as inputResult -> 
            match inputResult with
            | InputSuccess reason -> printInColor ConsoleColor.Green reason
            | InputError(reason, _) -> printInColor ConsoleColor.Red reason
        | _ -> printInColor ConsoleColor.Yellow (message.ToString())
    
    let validationActor (consoleWriter : IActorRef) (mailbox : Actor<_>) message =
        let (|EmptyMessage|MessageLengthIsEven|MessageLengthIsOdd|) (msg : string) =
            match msg.Length, msg.Length % 2 with
            | 0, _ -> EmptyMessage
            | _, 0 -> MessageLengthIsEven
            | _, _ -> MessageLengthIsOdd
        match message with
        | EmptyMessage -> consoleWriter <! InputError("No input received", ErrorType.Null)
        | MessageLengthIsEven -> consoleWriter <! InputSuccess("Thank you, the message was valid")
        | _ -> consoleWriter <! InputError("The message is invalid (odd number of characters", ErrorType.Validation)
        mailbox.Sender() <! Continue
    
    let fileValidator (consoleWriter : IActorRef) (mailbox : Actor<_>) message =
        let (|IsFileUri|_|) path =
            if File.Exists path then Some path
            else None
        
        let tailCoordinator = select "/user/tailCoordinator" mailbox.Context.System
        
        let (|EmptyMessage|Message|) (msg : string) =
            match msg.Length with
            | 0 -> EmptyMessage
            | _ -> Message(msg)
            
        match message with
        | EmptyMessage -> 
            consoleWriter <! InputError("Input was blank. Please try again.\n", ErrorType.Null)
            mailbox.Sender() <! Continue
        | IsFileUri _ -> 
            consoleWriter <! InputSuccess(sprintf "Starting processing for %s" message)
            tailCoordinator <! StartTail(message, consoleWriter)
        | _ -> 
            consoleWriter <! InputError(sprintf "%s is not an existing URI on disk." message, ErrorType.Validation)
            mailbox.Sender() <! Continue
    
    let tailActor (filePath : string) (reporter : IActorRef) (mailbox : Actor<_>) =
        let observer = new FileObserver(mailbox.Self, Path.GetFullPath filePath)
        do observer.Start()
        let fileStream = new FileStream(Path.GetFullPath(filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        let fileStreamReader = new StreamReader(fileStream, Text.Encoding.UTF8)
        let text = fileStreamReader.ReadToEnd()
        do mailbox.Self <! InitialRead(filePath, text)
        
        mailbox.Defer <| fun () -> 
            (observer :> IDisposable).Dispose()
            fileStreamReader.Close()
            (fileStreamReader :> IDisposable).Dispose()
            (fileStream :> IDisposable).Dispose()
            
        let rec loop() =
            actor { 
                let! message = mailbox.Receive()
                match (box message) :?> FileCommand with
                | FileWrite(_) -> 
                    let text = fileStreamReader.ReadToEnd()
                    if not <| String.IsNullOrEmpty text then reporter <! text
                    else ()
                | FileError(_, reason) -> reporter <! sprintf "Tail error: %s" reason
                | InitialRead(_, text) -> reporter <! text
                return! loop()
            }
        loop()
    
    let tailCoordinatorActor (mailbox : Actor<_>) message =
        match message with
        | StartTail(filePath, reporter) -> spawn mailbox.Context "TailActor" (tailActor filePath reporter) |> ignore
        | _ -> ()
