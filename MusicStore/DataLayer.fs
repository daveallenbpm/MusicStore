module MusicStore.DataLayer

open ROP

type Genre =
| Folk
| Country
| Rock
| Metal
| Classical

[<CLIMutable>]
type TrackMetadata = {
    Name: string
    Genre: Genre
    Artist: string
    Album: string
}

[<CLIMutable>]
type Track = {
    Id: int
    Metadata: TrackMetadata
}

type Error =
| DatabaseError
| NotFoundError

let example =
        {   
            Id = 1
            Metadata = 
            {
                Name = "Track"
                Genre = Folk
                Artist = "Artist"
                Album = "Album"
            }
        }

let getTrack trackId =
    match trackId with
    | 1 -> Success example
    | 2 -> Failure NotFoundError
    | 3 -> Failure DatabaseError
    | _ -> Success example

let saveTrack (track: TrackMetadata) =
    ignore
