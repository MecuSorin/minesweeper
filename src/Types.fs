module Minesweeper.Types

type BoxIndex = int

type Msg =
    | Reveal of BoxIndex
    | ToggleFlagMine of BoxIndex
    | ResetGame
    
type Model = 
    {
        gameState: GameState
        map: BoxState []
        revealed: Map<BoxIndex, RevealedState>
    }
and GameState =
    | Running
    | Won
    | Lost of detonatedMine: BoxIndex
and RevealedState =
    | Open
    | FlaggedMine
and BoxState =
    | Mine
    | Empty
    | MineProximity of neighbourMines: int