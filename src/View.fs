module Minesweeper.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Types
open GameLogic

let title content = h1 [ ClassName "title"] [ str content]
let subtitle content  = h2 [ ClassName "subtitle"] [ str content]
let showIcon label = span [ ClassName "icon"] [ i [ ClassName <| sprintf "fa fa-%s" label] [] ]

let showGrid titleText presentBox model dispatch =
    let cellsTemplate = sprintf "repeat(%i, 1fr)" <| getSize model.map
    div 
        [ ]
        [
            title titleText

            model.map
            |> Array.mapi presentBox
            |> Array.toList
            |> div 
                [ 
                    ClassName "minefield"
                    Style [
                        GridTemplateColumns cellsTemplate
                        GridTemplateRows cellsTemplate ]
                ]

            button [ OnClick (fun _ -> dispatch ResetGame)] [str "Reset the game"]
        ]

let showBox isDetonatedMine _ box  =
    match box with
    | Mine -> 
        let className = if isDetonatedMine then "minebox detonated" else "minebox"
        span [ ClassName className] [ showIcon "bomb"]
    | MineProximity nearbyMines -> span [ ClassName (sprintf "minebox color-number-%i" nearbyMines)] [str <| string nearbyMines]
    | Empty -> span [ ClassName "minebox"] []

let private showRawHotBox dispatch boxIndex content =
    button 
            [   ClassName "minebox hot-minebox"
                OnClick (fun _ -> dispatch (Reveal boxIndex))
                OnContextMenu (fun e ->
                    e.preventDefault()
                    dispatch (ToggleFlagMine boxIndex)) ]
            content 

let showHotBox dispatch mbRevealedState boxIndex box =
    match mbRevealedState with
    | Some Open -> showBox false boxIndex box
    | Some FlaggedMine -> 
        [showIcon "question"] |> showRawHotBox dispatch boxIndex
    | None -> showRawHotBox dispatch boxIndex []

let view model dispatch =
    let partialApp =
        match model.gameState with
        | Won -> 
            showGrid 
                "Congratulations you won!!!" 
                (showBox false)
        | Lost detonatedMine -> 
            showGrid 
                "Oups you triggered a mine!" 
                (fun boxIndex box  -> showBox (boxIndex = detonatedMine) boxIndex box)
        | Running -> 
            showGrid 
                "Go on with care"
                (fun boxIndex box -> showHotBox dispatch (model.revealed |> Map.tryFind boxIndex) boxIndex box)
    partialApp model dispatch