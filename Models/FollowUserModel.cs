using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3.Models
{
    public class FollowUserModel
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("Follower")]
        public int FollowerID { get; set; }
        [ForeignKey("Following")]
        public int FollowingID { get; set; }

        //Navigation properties
        public UserModel Follower { get; set; }
        public UserModel Following { get; set; }
    }
}
