namespace Unit1

open Akka.Actor
open Akka.FSharp
open System
open System.IO

[<AutoOpen>]
module FileUtility =
    type FileObserver(tailActor : IActorRef, absolutePath : string) =
        let fileDir = Path.GetDirectoryName absolutePath
        let fileNameOnly = Path.GetFileName absolutePath
        let mutable watcher : FileSystemWatcher = null
        
        member this.Start() =
            watcher <- new FileSystemWatcher(fileDir, fileNameOnly)
            watcher.NotifyFilter <- NotifyFilters.FileName ||| NotifyFilters.LastWrite
            watcher.Changed.Add(fun e -> 
                if e.ChangeType = WatcherChangeTypes.Changed then tailActor <! FileWrite(e.Name)
                else ())
            watcher.Error.Add(fun e -> tailActor <! FileError(fileNameOnly, (e.GetException()).Message))
            watcher.EnableRaisingEvents <- true
        
        interface IDisposable with
            member this.Dispose() = watcher.Dispose()
