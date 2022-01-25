using System;
using System.Collections.Generic;
using System.Text;

namespace SnowyBot.Structs
{
  public enum CharacterDataType
  {
    Delete = -1,
    Prefix = 0,
    Name = 1,
    Gender = 2,
    Sex = 4,
    Species = 8,
    Age = 16,
    Height = 32,
    Weight = 64,
    Orientation = 128,
    Description = 256,
    AvatarURL = 512,
    ReferenceURL = 1024
  }
}
