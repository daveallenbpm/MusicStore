module MusicStore.DataLayer

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

let getTrack trackId =
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

