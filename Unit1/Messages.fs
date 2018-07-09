namespace Unit1

open Akka.Actor

[<AutoOpen>]
module Messages =
    type ErrorType =
        | Null
        | Validation
    
    type InputResult =
        | InputSuccess of string
        | InputError of reason : string * error : ErrorType
    
    type Command =
        | Start
        | Continue
        | Message of string
        | Exit
    
    type TailCommand =
        | StartTail of filePath : string * reporterActor : IActorRef
        | StopTail of filePath : string
    
    type FileCommand =
        | FileWrite of fileName : string
        | FileError of fileName : string * reason : string
        | InitialRead of filename : string * text : string
