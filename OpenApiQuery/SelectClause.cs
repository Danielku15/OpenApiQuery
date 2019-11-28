using System.Collections.Generic;
using System.Reflection;

namespace OpenApiQuery
{
    public class SelectClause
    {
        public PropertyInfo SelectedMember { get; set; }
        public bool IsStarSelect { get; set; }
        public IDictionary<PropertyInfo, SelectClause> SelectClauses { get; set; }
    }
}