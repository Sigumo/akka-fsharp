namespace Unit1

open System
open System.IO
open Akka.Actor
open Akka.FSharp

[<AutoOpen>]
module FileUtility = 
    type FileObserver (tailActor: IActorRef, absolutePath: string) = 
        let fileDir = Path.GetDirectoryName absolutePath
        let fileNameOnly = Path.GetFileName absolutePath
        let mutable watcher = null : FileSystemWatcher
        
        member this.Start () = 
            watcher <- new FileSystemWatcher(fileDir, fileNameOnly)
            watcher.NotifyFilter <- NotifyFilters.FileName ||| NotifyFilters.LastWrite
            watcher.Changed.Add (fun e -> if e.ChangeType = WatcherChangeTypes.Changed then tailActor <! FileWrite(e.Name) else ())
            watcher.Error.Add (fun e -> tailActor <! FileError (fileNameOnly, (e.GetException ()).Message ))
            watcher.EnableRaisingEvents <- true
        
        interface IDisposable with 
            member this.Dispose () = watcher.Dispose () 