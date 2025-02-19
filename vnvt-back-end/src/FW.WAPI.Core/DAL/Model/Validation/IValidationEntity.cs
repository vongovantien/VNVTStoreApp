
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FW.WAPI.Core.DAL.Model.Validation
{
    public interface IValidationEntity
    {
        IEnumerable<ValidationResult> Validate();
    }    
}
