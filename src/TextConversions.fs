namespace FSharp.Data

module internal UnicodeHelper =

    // used http://en.wikipedia.org/wiki/UTF-16#Code_points_U.2B010000_to_U.2B10FFFF as a guide below
    let getUnicodeSurrogatePair num =
        // only code points U+010000 to U+10FFFF supported
        // for coversion to UTF16 surrogate pair
        let codePoint = num - 0x010000u
        let HIGH_TEN_BIT_MASK = 0xFFC00u // 1111|1111|1100|0000|0000
        let LOW_TEN_BIT_MASK = 0x003FFu // 0000|0000|0011|1111|1111

        let leadSurrogate =
            (codePoint &&& HIGH_TEN_BIT_MASK >>> 10) + 0xD800u

        let trailSurrogate =
            (codePoint &&& LOW_TEN_BIT_MASK) + 0xDC00u

        char leadSurrogate, char trailSurrogate
