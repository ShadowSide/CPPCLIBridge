module LanguageEx

let flip f a b = f b a
//let apply f a = f a
let cartesian xs ys = xs |> List.collect (fun x -> ys |> List.map (fun y -> x, y))
let cartesianWith f xs ys = xs |> List.collect (fun x -> ys |> List.map (f x))