// otoshidama-roulette
// https://github.com/taidalog/otoshidama-roulette
// Copyright (c) 2023-2024 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/otoshidama-roulette/blob/main/LICENSE
namespace OtoshidamaRadix

open System
open System.Diagnostics
open Browser.Dom
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Fermata
open Fermata.RadixConversion
open JsNative
open RunningState

module Roulette =
    let randomRadix () : string =
        List.item (Random.lessThan 3) [ "(2)"; "(10)"; "(16)" ]

    let randomByte () : string = Random.lessThan 2 |> string

    let display (value: string) (radix: string) : unit =
        value
        |> Seq.rev
        |> Seq.iteri (fun i x -> (document.getElementById $"digit%d{i + 1}").innerText <- string x)

        (document.getElementById "radix").innerText <- radix

    let turn (f: unit -> string) (element: HTMLElement) : unit = element.innerText <- f ()

    type Radix =
        | Bin
        | Dec
        | Hex

    let radixToString (radix: Radix) : string =
        match radix with
        | Radix.Bin -> "(2)"
        | Radix.Dec -> "(10)"
        | Radix.Hex -> "(16)"

    let randomRadix' () : Radix =
        List.item (Random.lessThan 3) [ Radix.Bin; Radix.Dec; Radix.Hex ]

    let shouldGuaranteeDec (radix: Radix) : bool =
        radix = Radix.Dec && Random.nextDouble () < 0.03

    let shouldGuaranteeHex (radix: Radix) : bool =
        radix = Radix.Hex && Random.nextDouble () < 0.03

    let binaryExpression (radix: Radix) : string =
        match radix with
        | Radix.Bin -> DateTime.Now.Year + 1
        | Radix.Dec -> 1024
        | Radix.Hex -> 256
        |> Random.lessThan
        |> fun x -> Convert.ToString(x, 2)
        |> String.padLeft 11 '0'

    let rec removeLeadingZeros (input: string) : string =
        match String.tryHead input with
        | None -> input
        | Some v ->
            if v <> "0" then
                input
            else
                removeLeadingZeros (String.tail input)

    let start () : RunningState =
        let radix: Radix = randomRadix' ()
        let b: string = binaryExpression radix

        let dec: Dec =
            match radix with
            | Radix.Bin -> b |> Bin.validate |> Bin.toDec
            | Radix.Dec -> b |> Dec.validate
            | Radix.Hex -> b |> removeLeadingZeros |> Hex.validate |> Hex.toDec

        match dec with
        | Dec.Invalid _ -> RunningState.Invalid
        | Dec.Valid v ->
            let values: string list =
                radixToString radix :: (b |> Seq.toList |> List.map string) |> List.rev

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
                randomRadix :: (List.replicate 11 randomByte) |> List.rev

            let intervalIds: int list =
                ids
                |> List.map (fun x -> document.getElementById x)
                |> List.map2 (fun g e -> setInterval (fun _ -> turn g e) 100) generators

            let message = document.getElementById "message"
            let button = document.getElementById "button" :?> HTMLButtonElement

            if shouldGuaranteeDec radix then
                message.innerText <- "10進法確定!"
                button.className <- "bamboo-leaves"
            else if shouldGuaranteeHex radix then
                message.innerText <- "16進法確定!"
                button.className <- "pine-leaves"
            else
                message.innerText <- "お年玉ルーレット！"
                button.className <- ""

            (document.getElementById "button" :?> HTMLButtonElement).innerText <- "止"
            RunningState.Running(intervalIds, values, ids, v, b, radixToString radix)

    let rec toggle runningState : unit =
        let button = document.getElementById "button" :?> HTMLButtonElement
        let result = document.getElementById "result"

        match runningState with
        | RunningState.Invalid -> ()
        | RunningState.Stopping ->
            result.innerText <- ""
            let outputArea = document.getElementById "outputArea"

            outputArea.children
            |> JS.Constructors.Array?from
            |> Array.iter (fun (x: HTMLElement) -> x.classList.remove "fixed")

            let audio = document.getElementById "koto" :?> HTMLAudioElement
            audio.currentTime <- 0
            audio.play ()

            start () |> fun x -> button.onclick <- fun _ -> toggle x
        | RunningState.Running(intervalIds, values, ids, amount, bin, radix) ->

            match intervalIds with
            | [] -> ()
            | h :: t ->
                clearInterval h
                let element = document.getElementById (List.head ids)
                element.innerText <- List.head values
                element.classList.add "fixed"

                match t with
                | [] ->
                    button.onclick <- fun _ -> toggle RunningState.Stopping
                    button.className <- ""
                    button.innerText <- "始"

                    let message = document.getElementById "message"
                    message.innerText <- "お年玉ルーレット！"

                    result.innerHTML <-
                        if radix = "(10)" then
                            $"%d{amount}<sub>(10)</sub>"
                        else
                            $"%s{bin}<sub>%s{radix}</sub> = %d{amount}<sub>(10)</sub>"

                    let audio = document.getElementById "shakuhachi" :?> HTMLAudioElement
                    audio.currentTime <- 0
                    audio.play ()
                | _ ->
                    button.onclick <-
                        if List.length values > 5 then
                            let audio = document.getElementById "kodutsumi" :?> HTMLAudioElement
                            audio.currentTime <- 0
                            audio.play ()
                        else
                            let audio = document.getElementById "hyoushigi" :?> HTMLAudioElement
                            audio.currentTime <- 0
                            audio.volume <- 0.25
                            audio.play ()

                        fun _ -> toggle (RunningState.Running(t, List.tail values, List.tail ids, amount, bin, radix))
