using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Membership.Common
{
    public class ServiceResult<T> where T : class
    {
        public ServiceResult()
        {
            _errors = new List<string>();
        }

        private List<string> _errors;
        public IEnumerable<string> Errors => _errors;
        public bool IsValid => _errors.Any() == false;

        public T ResultObj { get; set; }

        public void AddError(string message)
        {
            if (string.IsNullOrEmpty(message) == false)
            {
                _errors.Add(message);
            }
        }

        public void AddErrors(IEnumerable<string> errors)
        {
            if (errors != null && errors.Any())
            {
                _errors.AddRange(errors);
            }
        }


    }
}
