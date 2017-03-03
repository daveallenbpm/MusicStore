module MusicStore.DataLayer

type Genre =
| Folk
| Country
| Rock
| Metal
| Classical

type TrackMetadata = {
    Id: int
    Name: string
    Genre: Genre
    Artist: string
    Album: string
}

let getTrackMetadata trackId =
    {   
        Id = 1
        Name = "Track"
        Genre = Folk
        Artist = "Artist"
        Album = "Album"
    }
