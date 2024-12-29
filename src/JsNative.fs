// otoshidama-roulette
// https://github.com/taidalog/otoshidama-roulette
// Copyright (c) 2023-2024 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/otoshidama-roulette/blob/main/LICENSE
namespace OtoshidamaRadix

open Fable.Core

module JsNative =
    [<Emit("setInterval($0, $1)")>]
    let setInterval (callback: unit -> unit) (interval: int) : int = jsNative

    [<Emit("clearInterval($0)")>]
    let clearInterval (intervalID: int) : unit = jsNative
