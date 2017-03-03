module MusicStore.DataLayer

type Genre =
| Folk
| Country
| Rock
| Metal
| Classical

type TrackMetadata = {
    Name: string
    Genre: Genre
    Artist: string
    Album: string
}

type Track = {
    Id: int
    Metadata: TrackMetadata
}

let getTrackMetadata trackId =
    {   
        Id = 1
        Metadata = {
            Name = "Track"
            Genre = Folk
            Artist = "Artist"
            Album = "Album"
        }
    }

