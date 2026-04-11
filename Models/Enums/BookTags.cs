using System;

namespace ObreshkovLibrary.Models.Enums
{
    [Flags]
    public enum BookTags : long
    {
        None = 0,

        BulgarianLiterature = 1L << 0,
        WorldLiterature = 1L << 1,
        BalkanLiterature = 1L << 2,
        AncientLiterature = 1L << 3,
        ClassicLiterature = 1L << 4,
        ContemporaryLiterature = 1L << 5,

        Love = 1L << 6,
        Rebellion = 1L << 7,
        Suffering = 1L << 8,
        Romantic = 1L << 9,

        Horror = 1L << 10,
        Historical = 1L << 11,
        ScienceFiction = 1L << 12,
        Fantasy = 1L << 13,
        Psychological = 1L << 14,
        Social = 1L << 15,

        RequiredReading = 1L << 16,
        RecommendedReading = 1L << 17,
        ForMatura = 1L << 18,

        BulgarianAuthor = 1L << 19,
        ForeignAuthor = 1L << 20,

        EducationalContent = 1L << 21,
        ClassicalWork = 1L << 22,

        Poetry = 1L << 23,
        Prose = 1L << 24,
        Dramaturgy = 1L << 25,

        Mathematics = 1L << 26,
        BulgarianLanguage = 1L << 27,
        History = 1L << 28,
        Geography = 1L << 29,
        Biology = 1L << 30,
        Chemistry = 1L << 31,
        Physics = 1L << 32,
        Philosophy = 1L << 33,
        IT = 1L << 34
    }
}