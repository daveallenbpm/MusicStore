module MusicStore.ApiModel

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

let retrieve getTrack trackId =
    getTrack trackId

let getTrackMetadata trackId =
    {
        Name = "Track"
        Genre = Folk
        Artist = "Artist"
        Album = "Album"
    }
