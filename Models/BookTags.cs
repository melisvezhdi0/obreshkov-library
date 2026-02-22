using System;

namespace ObreshkovLibrary.Models
{
    [Flags]
    public enum BookTags
    {
        None = 0,
        Classic = 1,
        Romance = 2,
        Drama = 4,
        Fantasy = 8,
        Horror = 16,
        Bulgarian = 32,
        Foreign = 64,
        SchoolLiterature = 128
    }
}