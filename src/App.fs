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
open RunningState
open Roulette

module App =
    let keyboardshortcut (e: KeyboardEvent) : unit =
        let helpWindow = document.getElementById "helpWindow"

        let isHelpWindowActive =
            helpWindow.classList |> JS.Constructors.Array?from |> Array.contains "active"

        let informationPolicyWindow = document.getElementById "informationPolicyWindow"

        let isInformationPolicyWindowActive =
            informationPolicyWindow.classList
            |> JS.Constructors.Array?from
            |> Array.contains "active"

        match e.key with
        | "Escape" ->
            if isHelpWindowActive then
                helpWindow.classList.remove "active"

            if isInformationPolicyWindowActive then
                informationPolicyWindow.classList.remove "active"
        | "?" ->
            if not isHelpWindowActive then
                helpWindow.classList.add "active"
        | _ -> ()

    let shrinkLines (width: float) (svg: Element) : unit =
        svg.setAttribute ("viewBox", $"0 0 %f{width} 17")
        svg.setAttribute ("width", $"%f{width}px")
        svg.setAttribute ("height", "17px")

        svg.querySelector "g"
        |> _.querySelectorAll("polyline")
        |> JS.Constructors.Array?from
        |> Array.iteri (fun i (x: Element) -> x.setAttribute ("points", $"""0,%d{i * 6 + 3} %f{width},%d{i * 6 + 3}"""))

    window.addEventListener (
        "DOMContentLoaded",
        (fun _ ->
            Roulette.display "00000000000" "(2)"

            let button = document.getElementById "button" :?> HTMLButtonElement
            button.innerText <- "始"

            let linesLeft = document.getElementById "linesLeft" :?> HTMLDivElement
            let svgLeft = document.createElementNS ("http://www.w3.org/2000/svg", "svg")
            let gLeft = document.createElementNS ("http://www.w3.org/2000/svg", "g")

            List.init 3 (fun _ -> document.createElementNS ("http://www.w3.org/2000/svg", "polyline"))
            |> List.iter (fun x -> gLeft.appendChild x |> ignore)

            svgLeft.appendChild gLeft |> ignore
            linesLeft.appendChild svgLeft |> ignore

            let width = (window.innerWidth - (max (window.innerWidth * 0.0625) 60.)) / 2. // window.screen.width - max(6.25vw, 60px) / 2
            Debug.WriteLine $"svg width = %f{width}"

            shrinkLines width svgLeft

            let linesRight = document.getElementById "linesRight" :?> HTMLDivElement
            let svgRight = document.createElementNS ("http://www.w3.org/2000/svg", "svg")
            let gRight = document.createElementNS ("http://www.w3.org/2000/svg", "g")

            List.init 3 (fun _ -> document.createElementNS ("http://www.w3.org/2000/svg", "polyline"))
            |> List.iter (fun x -> gRight.appendChild x |> ignore)

            svgRight.appendChild gRight |> ignore
            linesRight.appendChild svgRight |> ignore

            shrinkLines width svgRight

            let message = document.getElementById "message"
            message.innerText <- "お年玉ルーレット！"

            // help window
            [ "helpButton"; "helpClose" ]
            |> List.iter (fun x ->
                (document.getElementById x :?> HTMLButtonElement).onclick <-
                    fun _ -> (document.getElementById "helpWindow").classList.toggle "active" |> ignore)

            // information policy window
            (document.getElementById "informationPolicyLink").onclick <-
                fun event ->
                    event.preventDefault ()
                    (document.getElementById "informationPolicyWindow").classList.add "active"

            (document.getElementById "informationPolicyClose").onclick <-
                fun _ -> (document.getElementById "informationPolicyWindow").classList.remove "active"

            // keyboard shortcut
            document.onkeydown <- fun (e: KeyboardEvent) -> keyboardshortcut e

            button.onclick <- fun _ -> Roulette.toggle RunningState.Stopping)
    )

    window.addEventListener (
        "resize",
        (fun _ ->
            let width = (window.innerWidth - (max (window.innerWidth * 0.0625) 60.)) / 2. // window.screen.width - max(6.25vw, 60px) / 2
            Debug.WriteLine $"svg width = %f{width}"
            (document.getElementById "linesLeft").querySelector "svg" |> shrinkLines width
            (document.getElementById "linesRight").querySelector "svg" |> shrinkLines width)
    )
