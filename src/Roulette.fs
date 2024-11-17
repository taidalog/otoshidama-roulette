// otoshidama-radix Version 0.1.0
// https://github.com/taidalog/otoshidama-radix
// Copyright (c) 2023-2024 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/otoshidama-radix/blob/main/LICENSE
namespace OtoshidamaRadix

open System
open System.Diagnostics
open Browser.Dom
open Browser.Types
open Fermata
open JsNative
open RunningState

module Roulette =
    let f (value: string) (radix: string) : unit =
        value
        |> Seq.rev
        |> Seq.iteri (fun i x -> (document.getElementById $"digit%d{i + 1}").innerText <- string x)

        (document.getElementById "radix").innerText <- radix

    let turn (f: unit -> string) (element: HTMLElement) : unit = element.innerText <- f ()

    let randomBinary (lessThan: int) : string =
        let n = Random.lessThan lessThan
        Convert.ToString(n, 2) |> String.padLeft 11 '0'

    let randomRadix () : string =
        List.item (Random.between 0 3) [ "₍₂₎"; "₍₁₀₎"; "₍₁₆₎" ]

    // let stop (intervalId: int) (value: string) (radix: string) : RunningState =
    //     clearInterval intervalId
    //     f value radix
    //     (document.getElementById "button" :?> HTMLButtonElement).innerText <- "Start"
    //     RunningState.Stopping

    let start () : RunningState =
        let n = Random.lessThan 2026
        let b = Convert.ToString(n, 2) |> String.padLeft 11 '0'
        let radix = List.item (Random.between 0 3) [ "₍₂₎"; "₍₁₀₎"; "₍₁₆₎" ]
        Debug.WriteLine $"%d{n}%s{radix}"

        let values: string list = radix :: (b |> Seq.toList |> List.map string) |> List.rev

        let ids: string list =
            [ "digit1"
              "digit2"
              "digit3"
              "digit4"
              "digit5"
              "digit6"
              "digit7"
              "digit8"
              "digit9"
              "digit10"
              "digit11"
              "radix" ]

        let generators: (unit -> string) list =
            randomRadix :: (List.replicate 11 (fun _ -> Random.between 0 2 |> string))
            |> List.rev

        let intervalIds: int list =
            ids
            |> List.map (fun x -> document.getElementById x)
            |> List.map2 (fun g e -> setInterval (fun _ -> turn g e) 250) generators

        // let intervalIds' =
        //     (setInterval (fun _ -> turn (fun _ -> randomRadix ()) (document.getElementById "radix")) 250)
        //     :: intervalIds
        //     |> List.rev

        (document.getElementById "button" :?> HTMLButtonElement).innerText <- "Stop"
        RunningState.Running(intervalIds, values, ids)

    let rec toggle runningState =
        let button = document.getElementById "button" :?> HTMLButtonElement

        match runningState with
        | RunningState.Stopping -> start () |> fun x -> button.onclick <- fun _ -> toggle x
        | RunningState.Running(intervalIds, values, ids) ->
            match intervalIds with
            | h :: t ->
                clearInterval h
                (document.getElementById (List.head ids)).innerText <- List.head values

                match t with
                | [] ->
                    button.onclick <- fun _ -> toggle RunningState.Stopping
                    button.innerText <- "Start"
                | _ -> button.onclick <- fun _ -> toggle (RunningState.Running(t, List.tail values, List.tail ids))

            | [] -> ()
