#nullable enable
using System.Collections.Generic;
#pragma warning disable 649 // Suppresses: ___ is never assigned to

// ReSharper disable All

namespace MusicBeePlugin
{
    class SearchResult
    {
        public SearchResultSong[] result;
        public int code;
    }
    
    class SearchResultSong
    {
        public string name;
        public string mid;
    }

    class LyricResult
    {
        public string lyric;
        public string? trans;
        public int code;
    }
    
}
