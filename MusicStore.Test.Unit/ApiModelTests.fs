namespace MusicStore.Test.Unit.ApiModel

open MusicStore.ApiModel
open MusicStore.DataLayer
open Swensen.Unquote
open Xunit
open FsCheck.Xunit

module Tests =
    [<Fact>]
    let ``Retrieve should return a track``() =
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

        let expected = 
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

        (retrieve getTrack 1) =! expected

    [<Property>]
    let ``Retrieve should return the correct track``
        (trackMetadata: TrackMetadata)
        (trackNumber: int) =
        let getTrack (x:int) = trackMetadata
        (retrieve getTrack trackNumber) =! trackMetadata
