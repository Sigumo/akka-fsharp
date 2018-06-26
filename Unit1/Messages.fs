namespace Unit1

[<AutoOpen>]
module Messages = 

    type ErrorType =
        | Null
        | Validation
    type InputResult = 
        | InputSuccess of string
        | InputError of reason:string * error:ErrorType
    type Command = 
        | Start
        | Continue
        | Message of string
        | Exit