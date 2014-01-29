module Main
[<EntryPoint>]
let main args = 
    let game = new HelloCube.Game()
    //let game = new Test.Game()
    game.Run(120.0,120.0)
    0
