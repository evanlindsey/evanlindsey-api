using System.Collections.Generic;

namespace EvanLindseyApi.Models
{
    public partial class User
    {
        public User()
        {
            Message = new HashSet<Message>();
        }

        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<Message> Message { get; set; }
    }
}
