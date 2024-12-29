// otoshidama-roulette
// https://github.com/taidalog/otoshidama-roulette
// Copyright (c) 2023-2024 taidalog
// This software is licensed under the MIT License.
// https://github.com/taidalog/otoshidama-roulette/blob/main/LICENSE
namespace OtoshidamaRadix

module RunningState =
    type RunningState =
        | Stopping
        | Running of int list * string list * string list * int * string * string
        | Invalid
