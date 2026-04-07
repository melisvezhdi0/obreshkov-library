using System;

namespace ObreshkovLibrary.Models
{
    [Flags]
    public enum BookTags : long
    {
        None = 0,

        BulgarianLiterature = 1 << 0,
        WorldLiterature = 1 << 1,
        BalkanLiterature = 1 << 2,
        AncientLiterature = 1 << 3,
        ClassicLiterature = 1 << 4,
        ContemporaryLiterature = 1 << 5,

        Love = 1 << 6,
        Rebellion = 1 << 7,
        Suffering = 1 << 8,
        Romantic = 1 << 9,

        Horror = 1 << 10,
        Historical = 1 << 11,
        ScienceFiction = 1 << 12,
        Fantasy = 1 << 13,
        Psychological = 1 << 14,
        Social = 1 << 15,

        RequiredReading = 1 << 16,
        RecommendedReading = 1 << 17,
        ForMatura = 1 << 18,

        BulgarianAuthor = 1 << 19,
        ForeignAuthor = 1 << 20,

        EducationalContent = 1 << 21,
        ClassicalWork = 1 << 22,

        Poetry = 1 << 23,
        Prose = 1 << 24,
        Dramaturgy = 1 << 25,

        Mathematics = 1 << 24,
        BulgarianLanguage = 1 << 25,
        History = 1 << 26,
        Geography = 1 << 27,
        Biology = 1 << 28,
        Chemistry = 1 << 29,
        Physics = 1 << 30,
        Philosophy = 1 << 31,
        IT = 1 << 32
    }
}