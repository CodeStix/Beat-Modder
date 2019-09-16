using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public enum BeatModsSort
    {
        None,
        Category, //category_lower
        Name, //name_lower
        Status, //status_lower
        Author, //author.username_lower
        UpdatedDate //updatedDate
    }
}
