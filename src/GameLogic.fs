module Minesweeper.GameLogic

open Types
open System

let private random =
    let rand = Random()
    rand.Next

let getCoordinates size boxIndex  = boxIndex/size, boxIndex % size
let getIndex size (row, col) = row * size + col
let getSize minefield = minefield |> Array.length |> float |> Math.Sqrt |> int

let private getNeighboursMineIndexes size boxIndex =
    let (row, col) = getCoordinates size boxIndex
    seq {
        for r = -1 to 1 do
        for c = -1 to 1 do
            yield r + row, c + col }
    |> Seq.filter(function
        | -1, _
        | _, -1 -> false
        | r, c when (r = row && c = col) || r >= size || c >= size -> false
        | _ -> true) 
    |> Seq.map (getIndex size)

let generateMineField minesCount size =
    let minefield =
        Seq.unfold
            (function
                | 0, 0 -> None
                | 0, empties -> Some (Empty, (0, empties - 1))
                | mines, 0 -> Some (Mine, (mines - 1, 0))
                | mines, empties ->
                    if mines > random (mines + empties)
                        then Some (Mine, (mines - 1, empties))
                        else Some (Empty, (mines, empties - 1))
            )
            (minesCount, size * size - minesCount)
        |> Seq.toArray
    minefield
    |> Array.mapi (fun i box ->
        match box with
        | Mine 
        | MineProximity _ -> box
        | Empty ->
            let dangerLevel =
                getNeighboursMineIndexes size i
                |> Seq.sumBy(fun ni -> match minefield.[ni] with Mine -> 1 | _ -> 0)
            if 0 = dangerLevel
                then box
                else MineProximity dangerLevel
    )

let revealEmptyFieldsAndNeighbours model currentBoxIndex =
    let getNearbySafeBoxes = getNeighboursMineIndexes (getSize model.map) >> Seq.toList
    let rec loop updatedRevealed = function
        | [] -> updatedRevealed
        | boxIndex :: boxesToProcess ->
            match updatedRevealed |> Map.tryFind boxIndex with
            | Some Open -> loop updatedRevealed boxesToProcess
            | _ ->
                match model.map.[boxIndex] with
                | Mine -> loop updatedRevealed boxesToProcess
                | MineProximity _ -> loop (updatedRevealed |> Map.add boxIndex Open) boxesToProcess
                | Empty -> loop (updatedRevealed |> Map.add boxIndex Open) (getNearbySafeBoxes boxIndex @ boxesToProcess )
    loop model.revealed [currentBoxIndex]